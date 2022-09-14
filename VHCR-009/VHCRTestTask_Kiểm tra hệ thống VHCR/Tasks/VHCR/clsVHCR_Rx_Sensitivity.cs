using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using CFTWinAppCore.Task;
using CFTWinAppCore.Sequences;
using CFTWinAppCore.DataLog;
using CFTWinAppCore.DeviceManager;
using LogLibrary;
using GeneralTool;
using CFTWinAppCore.Helper;
using System.Windows.Forms;
using VHCRTestTask.TypeConvert;
using CFTWinAppCore.DeviceManager.DUTIOService;
using CFTSeqManager.DUT;
using Option;
using CFTWinAppCore.DeviceManager.DUT;

namespace VHCRTestTask.Tasks.VHCR
{
    [clsTaskMetaInfo("VHCR - Rx Độ nhạy máy thu", "clsVHCR_Rx_Sensitivity", true)]
    public class clsVHCR_Rx_Sensitivity : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        public double m_duDisplayValue = double.MinValue;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        clsFreqTypeConv FreqTypeConv = new clsFreqTypeConv();
        Queue<PairValue> queueBerAndRFIN = null;
        List<int> listIncreaseRF = new List<int>();

        class PairValue
        {
            public double Ber { set; get; }
            public double LevelRF { set; get; }
            public byte bin { set; get; }

        }
        public clsVHCR_Rx_Sensitivity()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - Độ nhạy máy thu";
            MaxPass = 1; //uV
            //MinPass = 0;
            DelayAfterSetConfig = 1000;
            ExpectedBER = 1e-5;
            RfLvStep = 1;
            CableLoss = 0; //dB
            TimeOutGetBER = 1000; //ms
            WaitTimeGetBER = 500;
            ARBFileName= $"/var/user/vhcr/22042022/VHCR_BW20M_QAM64_120RB.wv";
            Result_unit = PowerUnit.dBm;
            WaveForm = "";
            RFLevel = 1;    // uV            
            RFOutport = OutputPort.RFOut;
            sampleClock = 30.72;
            DutBW = VHCR_Bandwidth.FIX_20M;
            DutFreqRx = 10.02;
            DutFreqTx = 10.14;
            DutMode = VHCR_Mod.FIX_QAM64;
            DutWF = VHCR_Wareform.FIX;
            delayDUTPara = 10000;
            ParameterSearchBer = $"1,2,3,5,6,7,9,10,11,13,15,17,18,19,21,23,25,27,29,31";
            MaxNumBerRead = 10;
            NumBerReadEvaluate = 5;
            

        }
        [Category("#General Option")]
        [Browsable(false)]
        public string Name
        {
            get { return "clsVHCR_Rx_Sensitivity"; }
        }
        [Category("#General Option")]
        [DisplayName("ID Specification")]
        [SaveAtt()]
        [Browsable(false)]
        public string IDSpec
        {
            get
            {
                return m_IDSpec;
            }

            set
            {
                m_IDSpec = value;
            }
        }
        [Category("#General Option")]
        [Browsable(false)]
        public int TypeMes
        {
            get
            {
                return (int)m_type_testvalue;
            }
        }
        [Category("#General Option")]
        [DisplayName("Description")]
        [SaveAtt()]
        public string Description
        {
            get
            {
                return m_strDescription;
            }
            set
            {
                m_strDescription = value;
            }
        }
        [Category("#General Option")]
        [DisplayName("Stop When Fail")]
        [SaveAtt()]
        public bool StopWhenFail
        {
            get
            {
                return m_bStopWhenFail;
            }
            set
            {
                m_bStopWhenFail = value;
            }
        }
        [Category("#General Option")]
        [DisplayName("Allow Excution")]
        [SaveAtt()]
        public bool AllowExcution
        {
            set;
            get;
        }

        public string GetDisplayValue()
        {
            return $"{m_duDisplayValue.ToString("F03")} {Result_unit.ToString()}";
        }
        public string GetDisplayMaxValue()
        {
            return $"{MaxPass.ToString("F03")} {Result_unit.ToString()}";
        }
        public string GetDisplayMinValue()
        {
            return "NA";
        }
        public string GetAddTaskDescription()
        {
            return $"{DutMode}, {DutBW} MHz, {DutFreqRx}GHz";
        }
        public string GetValue()
        {
            return $"{m_duDisplayValue.ToString("F03")}";
        }

        public string GetMaxValue()
        {
            return $"{MaxPass.ToString("F03")}";
        }

        public string GetMinValue()
        {
            return $"NA";
        }

        public string GetUnit()
        {
            return $"{Result_unit.ToString()}";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            ISignalGenDevice SigGenService = null;
            ISignalGenDeviceE8267D sigGenE8267D = null;
            ISignalGenDeviceSMW200A sigGenSMW200A = null;
            IDutVHCR VHCRService = null;
            bool bOK = false;
            string strMsg = null;
            double BERvalue = 0;
            double RFlvValue = 0;
            int StartTime = 0;
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_VHCR = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_sigGen = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
            IAccessDeviceService SigGenAccess = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as IAccessDeviceService;
            while (!bOK)
            {
                m_TraceService.WriteLine("Begin Test Task: " + m_strDescription + string.Format("[{0}]", GetAddTaskDescription()));
                try
                {
                    RETRY_LABEL:
                    int temp=0;
                    bOK = false;
                    queueBerAndRFIN = new Queue<PairValue>(NumBerReadEvaluate);
                    string[] arrParameterSearchBER = ParameterSearchBer.Split(',');
                    for(int i=0;i<arrParameterSearchBER.Length;i++)
                    {                      
                        int.TryParse(arrParameterSearchBER[i],out temp);
                        listIncreaseRF.Add(temp);
                    }
                    //Thiết lập SigGen
                    if (DevManager.IsDevEnable(SysDevType.SIG_GEN.ToString()) && DevManager.IsDevEnable(SysDevType.DUT.ToString()))
                    {
                        IDeviceInfor deviceinfo = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
                        if(deviceinfo==null)
                        {
                            clsLogManager.LogError("Device SIG_GEN is disable");
                            if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                goto RETRY_LABEL;
                            }
                        }
                        if (deviceinfo.DeviceName == "E8267D Signal Gen")
                        {
                            sigGenE8267D = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as ISignalGenDeviceE8267D;
                            if (sigGenE8267D == null)
                            {
                                clsLogManager.LogError("Can not get device service: ISignalGenDeviceE8267D");
                                if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                                {
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    goto RETRY_LABEL;
                                }
                            }
                        }
                        else if (deviceinfo.DeviceName == "SMW200A Signal Gen")
                        {
                            sigGenSMW200A = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as ISignalGenDeviceSMW200A;
                            if (sigGenSMW200A == null)
                            {
                                clsLogManager.LogError("Can not get device service: ISignalGenDeviceSMW200A");
                                if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                                {
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    goto RETRY_LABEL;
                                }
                            }
                        }
                        SigGenService = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as ISignalGenDevice;
                        VHCRService = DevManager.GetDevService(SysDevType.DUT.ToString()) as IDutVHCR;
                        if (SigGenService == null)
                        {
                            clsLogManager.LogError("Can not get device service: ISignalGenDevice");
                            if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                goto RETRY_LABEL;
                            }
                        }
                       
                       
                        if (VHCRService == null)
                        {
                            clsLogManager.LogError("Can not get device service: IDutVHCR");
                            if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                goto RETRY_LABEL;
                            }
                        }
                        if (!SigGenService.SignalGenSetup()) throw new System.IO.IOException();
                        if(!SigGenService.EnableOutput(false)) throw new System.IO.IOException();
                        if (deviceinfo.DeviceName == "E8267D Signal Gen")
                        {
                            if(!sigGenE8267D.LoadWaveForm(WaveForm)) throw new System.IO.IOException();
                            if(!sigGenE8267D.SetArbEnable(true)) throw new System.IO.IOException();
                            if(!sigGenE8267D.SetArbSampleClockRate(sampleClock*1e6)) throw new System.IO.IOException();
                            if(!sigGenE8267D.SetALCState(false)) throw new System.IO.IOException();
                        }
                        else if(deviceinfo.DeviceName == "SMW200A Signal Gen")
                        {
                            if(!sigGenSMW200A.EnablePlayARBFile(ARBFileName)) throw new System.IO.IOException();
                            if(!sigGenSMW200A.SetARBClockFrequency(sampleClock, FreqUnit.MHz)) throw new System.IO.IOException();
                        }
                        //SigGenService.SetModulationType(ModulatioType.CW); // Điều chế CW
                        if(!SigGenService.SetRFFrequency(DutFreqRx, FreqUnit.GHz)) throw new System.IO.IOException();
                        if(!SigGenService.SetOutputPower(RFLevel, Result_unit)) throw new System.IO.IOException();
                        if(!SigGenService.Select_OutputPort(RFOutport)) throw new System.IO.IOException();
                        if(!SigGenService.EnableOutput(true)) throw new System.IO.IOException();
                        // Đặt tham số máy: 
                        if (VHCRService.SetParamChanel(DutWF, DutMode, DutBW, (ulong)(DutFreqTx * 1e6), (ulong)(DutFreqRx * 1e6), 0, 0))
                        {
                            Thread.Sleep(delayDUTPara);
                            bOK = true;
                        }
                        else
                        {
                            strMsg = $"SetParamChanel: {DutWF}, {DutMode}, {DutBW}, {DutFreqRx}GHz => FAIL";
                            bOK = false;
                        }

                        if (bOK)
                        {
                            bOK = false;
                            // Tăng RF_Out SigGen_2nd => BER > 1e-5
                            BERvalue = 0;
                            RFlvValue = RFLevel + CableLoss;
                            clsLogManager.LogReport("{0, 25}{1, 25}", "RF Level", "BER");
                            if (RFLevel <= MaxPass)
                            {
                                return searchBerIncreaseRFPower(SigGenService, VHCRService, RFlvValue);
                            }
                            else
                            {
                                clsLogManager.LogError("RF Level = {0} dBm > Expect Level (Max Pass)={1} dBm",RFLevel,MaxPass);
                                if (clsMsgHelper.ShowYesNo("Question", "Cài đặt lại RF Level<= Max Pass, bạn có thực hiện lại không?") == DialogResult.No)
                                {
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    goto RETRY_LABEL;
                                }
                            }
                            //StartTime = Environment.TickCount;
                            //while (BERvalue < ExpectedBER)
                            //{
                            //    if (Environment.TickCount - StartTime > TimeOut)
                            //    {
                            //        clsLogManager.LogReport("TimeOut get BER");
                            //        break;
                            //    }
                            //    SigGenService.EnableOutput(false);
                            //    RFlvValue -= RfLvStep;
                            //    SigGenService.SetOutputPower(RFlvValue, Result_unit);
                            //    SigGenService.EnableOutput(true);
                            //    Thread.Sleep(WaitTimeGetBER);
                            //    BERvalue = VHCRService.getBER(TimeOutGetBER);
                            //    clsLogManager.LogReport($"RF Value = {RFlvValue}    => BER Value = {BERvalue}");
                            //    Thread.Sleep(1000);
                            //}
                            // SigGenService.EnableOutput(false);

                            // m_duDisplayValue = Math.Round(RFlvValue, 4);
                            // clsLogManager.LogReport("Sensitive result = {0} {1}", m_duDisplayValue, Result_unit);

                            //    if (RFlvValue <= MaxPass && BERvalue > ExpectedBER)
                            //    {
                            //        bOK = true;
                            //    }
                            //    else
                            //    {
                            //        strMsg = $"Kết quả {m_duDisplayValue.ToString("F03")} {Result_unit.ToString()} không đạt";
                            //    }    
                            //}                           
                        }
                        else
                        {
                            strMsg = "Devices are disable!";
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    strMsg = "Exception: " + ex.Message;
                    clsLogManager.LogError("Excution: {0}", ex.ToString());
                    bOK = false;
                    if (DUT != null)
                    {
                        DUT.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_VHCR != null)
                            DUT.Connect2Device(deviceinfo_VHCR);
                    }
                    if (SigGenAccess != null)
                    {
                        SigGenAccess.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_sigGen != null)
                            SigGenAccess.Connect2Device(deviceinfo_sigGen);
                    }
                }
                finally
                {
                    //off siggen
                    if (SigGenService != null)
                    {
                        SigGenService.EnableOutput(false);
                    }
                }

                if (!bOK)
                {
                    if (clsMsgHelper.ShowYesNo("Notice", strMsg + " - Bạn muốn đo lại bài này không?") == DialogResult.No)
                    {                       
                        break;
                    }
                    if (DUT != null)
                    {
                        DUT.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_VHCR != null)
                            DUT.Connect2Device(deviceinfo_VHCR);
                    }
                    if (SigGenAccess != null)
                    {
                        SigGenAccess.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_sigGen != null)
                            SigGenAccess.Connect2Device(deviceinfo_sigGen);
                    }
                }
            }

            if (bOK)
            {
                clsLogManager.LogWarning(Description + ": PASS");
                return TaskResult.PASS;
            }
            else
            {
                clsLogManager.LogWarning(Description + ": FAIL");
                return TaskResult.FAIL;
            }
        }

        public void SetModuleTask(ISequenceManager Seq)
        {
            m_IModuleTask = Seq;
            m_TraceService = (ITraceData)m_IModuleTask.GetService(typeof(ITraceData));
        }

        public void ParaToXmlAtt(System.Xml.XmlNode paraNode)
        {
            clsSaveTaskParaHelper.SaveObj2XmlNodeAtt(paraNode, this);
        }

        public void ParaFromXmlAtt(System.Xml.XmlNode NodeData)
        {
            clsSaveTaskParaHelper.ParseObjParameter2XmlNode(NodeData, this);
        }

        public string GetParaInfor()
        {
            throw new NotImplementedException();
        }

        public ITask Clone()
        {
            clsVHCR_Rx_Sensitivity task = new clsVHCR_Rx_Sensitivity();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        private TaskResult searchBerIncreaseRFPower(ISignalGenDevice signalGenDevice, IDutVHCR DutService, double RFin)
        {
            RETRY_LABEL:
            try
            {
                int nBegTime = Environment.TickCount;
                int nCurrTime = 0;
                double duCurrRFPower = RFin;
                double duBERValue = 0;
                byte CurrentNumBERRead = 0;
                int NumCheck = 0;
                queueBerAndRFIN.Clear();
                while (true)
                {
                    NumCheck = 0;
                    if (!signalGenDevice.SetOutputPower(duCurrRFPower, PowerUnit.dBm)) throw new System.IO.IOException();
                    //Thread.Sleep(1000);
                    //DutService.GetBER(1000);
                    if (WaitTimeGetBER > 0)
                    {
                        clsLogManager.LogReport("SleepBeforeReadBER: {0}ms", WaitTimeGetBER);
                        Thread.Sleep(WaitTimeGetBER);
                    }
                    duBERValue = DutService.getBER(TimeOutGetBER);
                    if (duBERValue.ToString() == "NaN")
                    {
                        if (clsMsgHelper.ShowYesNo("Question", "DUT không thu, bạn có thực hiện lại") == DialogResult.No)
                        {
                            //  m_duDisplayValue = "FAIL";
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                            goto RETRY_LABEL;
                        }
                    }
                    CurrentNumBERRead++;
                    clsLogManager.LogWarning("{0, 25}{1, 25}", (duCurrRFPower - CableLoss).ToString("F04"), duBERValue.ToString("F07"));
                    PairValue pairValue = new PairValue();
                    if (duBERValue <= ExpectedBER)
                    {
                        pairValue.bin = 0;
                    }
                    else
                    {
                        pairValue.bin = 1;
                    }
                    pairValue.Ber = duBERValue;
                    pairValue.LevelRF = duCurrRFPower - CableLoss;
                    queueBerAndRFIN.Enqueue(pairValue);
                    if (CurrentNumBERRead >= NumBerReadEvaluate)
                    {
                        PairValue[] arrBerAndRFIN = queueBerAndRFIN.ToArray();
                        clsLogManager.LogWarning("{0, 25}{1, 25}{2,25}", "Ber", "LevelRF(dBm)", "Bin");
                        for (int i = 0; i < NumBerReadEvaluate; i++)
                        {
                            clsLogManager.LogWarning("{0, 25}{1, 25}{2,25}", arrBerAndRFIN[i].Ber.ToString("F07"), arrBerAndRFIN[i].LevelRF.ToString("F04"), arrBerAndRFIN[i].bin.ToString());
                        }
                        for (int i = 0; i < NumBerReadEvaluate; i++)
                        {
                            NumCheck |= arrBerAndRFIN[i].bin << (NumBerReadEvaluate - i - 1);
                        }
                        clsLogManager.LogWarning("NumCheck={0}", NumCheck);
                        if (listIncreaseRF.Contains(NumCheck))
                        {
                            duCurrRFPower = duCurrRFPower + RfLvStep;
                            clsLogManager.LogWarning("Tăng cao tần with step={0} dB => RFPowerIn={1} dBm", RfLvStep, duCurrRFPower - CableLoss);
                            if ((duCurrRFPower - CableLoss) > MaxPass)
                            {
                                if (clsMsgHelper.ShowYesNo("Question", $"Mức cao tần tiếp theo = {duCurrRFPower - CableLoss} dBm vượt quá giá trị cho phép = {MaxPass} dBm, bạn có thực hiện lại") == DialogResult.No)
                                {
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                                    goto RETRY_LABEL;
                                }
                            }
                            queueBerAndRFIN.Clear();
                            CurrentNumBERRead = 0;
                        }
                        else if (NumCheck == 0)
                        {
                            m_duDisplayValue = arrBerAndRFIN[4].LevelRF;//duCurrRFPower - CableLoss;
                            clsLogManager.LogReport("RFPowerIn={0} dBm, Meas_RF_IN={1} dBm,Cableloss={2} dB, RxSensitivityLimit={3} dBm", duCurrRFPower - CableLoss, duCurrRFPower, CableLoss, MaxPass);
                            clsLogManager.LogReport("Result PASS: Ber Result={0} <= {1}", arrBerAndRFIN[4].Ber, ExpectedBER);
                            return TaskResult.PASS;
                        }
                        else
                        {
                            queueBerAndRFIN.Dequeue();
                        }
                        if (CurrentNumBERRead >= MaxNumBerRead)
                        {
                            if (clsMsgHelper.ShowYesNo("Question", "Quá số lần đọc Ber mà chưa đạt, bạn có thực hiện lại") == DialogResult.No)
                            {
                                //  m_duDisplayValue = "FAIL";
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                                goto RETRY_LABEL;
                            }
                        }

                    }

                    nCurrTime = Environment.TickCount - nBegTime;
                    if (nCurrTime >= MaxBERSearchTimeout)
                    {
                        if (clsMsgHelper.ShowYesNo("Question", "Quá thời gian mà không đo được, bạn có thực hiện lại") == DialogResult.No)
                        {
                            //  m_duDisplayValue = "FAIL";
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                            goto RETRY_LABEL;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                clsLogManager.LogError(ex.ToString());
                if (clsMsgHelper.ShowYesNo("Question", "Lỗi quá trình đo, bạn có thực hiện lại") == DialogResult.No)
                {
                    //  m_duDisplayValue = "FAIL";
                    return TaskResult.FAIL;
                }
                else
                {
                    MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                    goto RETRY_LABEL;
                }
            }
        }
        public TaskResult searchBerIncreaseRFPower_2(ISignalGenDevice signalGenDevice, IDutVHCR DutService, double RFin)
        {
            RETRY_LABEL:
            try
            {

                int nBegTime = Environment.TickCount;
                int nCurrTime = 0;
                double duCurrRFPower = RFin;
                double duBERValue = 0;
                byte CurrentNumBERRead = 0;
                int NumCheck = 0;
                queueBerAndRFIN.Clear();
                while (true)
                {
                    NumCheck = 0;
                    if(!signalGenDevice.SetOutputPower(duCurrRFPower, PowerUnit.dBm)) throw new System.IO.IOException();
                    //Thread.Sleep(1000);
                    //DutService.GetBER(1000);
                    if (WaitTimeGetBER > 0)
                    {
                        clsLogManager.LogReport("SleepBeforeReadBER: {0}ms", WaitTimeGetBER);
                        Thread.Sleep(WaitTimeGetBER);
                    }
                    duBERValue = DutService.getBER(TimeOutGetBER);
                    if (duBERValue.ToString() == "NaN")
                    {
                        if (clsMsgHelper.ShowYesNo("Question", "DUT không thu, bạn có thực hiện lại") == DialogResult.No)
                        {
                            //  m_duDisplayValue = "FAIL";
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                            goto RETRY_LABEL;
                        }
                    }
                    CurrentNumBERRead++;
                    clsLogManager.LogWarning("{0, 25}{1, 25}", (duCurrRFPower - CableLoss).ToString("F04"), duBERValue.ToString("F07"));
                    PairValue pairValue = new PairValue();
                    if (duBERValue <= ExpectedBER)
                    {
                        pairValue.bin = 0;
                    }
                    else
                    {
                        pairValue.bin = 1;
                    }
                    pairValue.Ber = duBERValue;
                    pairValue.LevelRF = duCurrRFPower - CableLoss;
                    queueBerAndRFIN.Enqueue(pairValue);
                    if (CurrentNumBERRead >= NumBerReadEvaluate)
                    {
                        PairValue[] arrBerAndRFIN = queueBerAndRFIN.ToArray();
                        clsLogManager.LogWarning("{0, 25}{1, 25}{2,25}", "Ber", "LevelRF(dBm)", "Bin");
                        for (int i = 0; i < NumBerReadEvaluate; i++)
                        {
                            clsLogManager.LogWarning("{0, 25}{1, 25}{2,25}", arrBerAndRFIN[i].Ber.ToString("F07"), arrBerAndRFIN[i].LevelRF.ToString("F04"), arrBerAndRFIN[i].bin.ToString());
                        }
                        for (int i = 0; i < NumBerReadEvaluate; i++)
                        {
                            NumCheck |= arrBerAndRFIN[i].bin << (NumBerReadEvaluate - i - 1);
                        }
                        if (listIncreaseRF.Contains(NumCheck))
                        {
                            return TaskResult.FAIL;
                        }
                        else if (NumCheck == 0)
                        {
                            m_duDisplayValue = arrBerAndRFIN[4].LevelRF;//duCurrRFPower - CableLoss;
                            clsLogManager.LogReport("RFPowerIn={0} dBm, Meas_RF_IN={1} dBm,Cableloss={2} dB, RxSensitivityLimit={3} dBm", duCurrRFPower - CableLoss, duCurrRFPower, CableLoss, MaxPass);
                            clsLogManager.LogReport("Result PASS: Ber Result={0} <= {1}", arrBerAndRFIN[4].Ber, ExpectedBER);
                            return TaskResult.PASS;
                        }
                        else
                        {
                            queueBerAndRFIN.Dequeue();
                        }
                        if (CurrentNumBERRead >= MaxNumBerRead)
                        {
                            return TaskResult.FAIL;
                        }

                    }

                    nCurrTime = Environment.TickCount - nBegTime;
                    if (nCurrTime >= MaxBERSearchTimeout)
                    {
                        if (clsMsgHelper.ShowYesNo("Question", "Quá thời gian mà không đo được, bạn có thực hiện lại") == DialogResult.No)
                        {
                            //  m_duDisplayValue = "FAIL";
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                            goto RETRY_LABEL;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                clsLogManager.LogError(ex.ToString());
                if (clsMsgHelper.ShowYesNo("Question", "Lỗi quá trình đo, bạn có thực hiện lại") == DialogResult.No)
                {
                    //  m_duDisplayValue = "FAIL";
                    return TaskResult.FAIL;
                }
                else
                {
                    MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                    goto RETRY_LABEL;
                }
            }
        }
        public TaskResult searchBerIncreaseRFPower_3(ISignalGenDevice signalGenDevice, IDutVHCR DutService, double RFin)
        {
            RETRY_LABEL:
            try
            {
                int nBegTime = Environment.TickCount;
                int nCurrTime = 0;
                double duCurrRFPower = RFin;
                double duBERValue = 0;
                byte CurrentNumBERRead = 0;
                int NumCheck = 0;
                queueBerAndRFIN.Clear();
                while (true)
                {
                    NumCheck = 0;
                    if(!signalGenDevice.SetOutputPower(duCurrRFPower, PowerUnit.dBm)) throw new System.IO.IOException();
                    //Thread.Sleep(1000);
                    //DutService.GetBER(1000);
                    if (WaitTimeGetBER > 0)
                    {
                        clsLogManager.LogReport("SleepBeforeReadBER: {0}ms", WaitTimeGetBER);
                        Thread.Sleep(WaitTimeGetBER);
                    }
                    duBERValue = DutService.getBER(TimeOutGetBER);
                    if (duBERValue.ToString() == "NaN")
                    {
                        if (clsMsgHelper.ShowYesNo("Question", "DUT không thu, bạn có thực hiện lại") == DialogResult.No)
                        {
                            //  m_duDisplayValue = "FAIL";
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                            goto RETRY_LABEL;
                        }
                    }
                    CurrentNumBERRead++;
                    clsLogManager.LogWarning("{0, 25}{1, 25}", (duCurrRFPower - CableLoss).ToString("F04"), duBERValue.ToString("F07"));
                    PairValue pairValue = new PairValue();
                    if (duBERValue <= ExpectedBER)
                    {
                        pairValue.bin = 0;
                    }
                    else
                    {
                        pairValue.bin = 1;
                    }
                    pairValue.Ber = duBERValue;
                    pairValue.LevelRF = duCurrRFPower - CableLoss;
                    queueBerAndRFIN.Enqueue(pairValue);
                    if (CurrentNumBERRead >= NumBerReadEvaluate)
                    {
                        PairValue[] arrBerAndRFIN = queueBerAndRFIN.ToArray();
                        clsLogManager.LogWarning("{0, 25}{1, 25}{2,25}", "Ber", "LevelRF(dBm)", "Bin");
                        for (int i = 0; i < NumBerReadEvaluate; i++)
                        {
                            clsLogManager.LogWarning("{0, 25}{1, 25}{2,25}", arrBerAndRFIN[i].Ber.ToString("F07"), arrBerAndRFIN[i].LevelRF.ToString("F04"), arrBerAndRFIN[i].bin.ToString());
                        }
                        for (int i = 0; i < NumBerReadEvaluate; i++)
                        {
                            NumCheck |= arrBerAndRFIN[i].bin << (NumBerReadEvaluate - i - 1);
                        }
                        clsLogManager.LogWarning("NumCheck={0}", NumCheck);
                        if (listIncreaseRF.Contains(NumCheck))
                        {
                            if (clsMsgHelper.ShowYesNo("Question", $"Đo độ nhạy tại tần số {DutFreqRx} GHz lỗi, bạn có thực hiện lại") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                                goto RETRY_LABEL;
                            }
                        }
                        else if (NumCheck == 0)
                        {
                            m_duDisplayValue = arrBerAndRFIN[4].LevelRF;//duCurrRFPower - CableLoss;
                            clsLogManager.LogReport("RFPowerIn={0} dBm, Meas_RF_IN={1} dBm,Cableloss={2} dB, RxSensitivityLimit={3} dBm", duCurrRFPower - CableLoss, duCurrRFPower, CableLoss, MaxPass);
                            clsLogManager.LogReport("Result PASS: Ber Result={0} <= {1}", arrBerAndRFIN[4].Ber, ExpectedBER);
                            return TaskResult.PASS;
                        }
                        else
                        {
                            queueBerAndRFIN.Dequeue();
                        }
                        if (CurrentNumBERRead >= MaxNumBerRead)
                        {
                            if (clsMsgHelper.ShowYesNo("Question", "Quá số lần đọc Ber mà chưa đạt, bạn có thực hiện lại") == DialogResult.No)
                            {
                                //  m_duDisplayValue = "FAIL";
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                                goto RETRY_LABEL;
                            }
                        }

                    }

                    nCurrTime = Environment.TickCount - nBegTime;
                    if (nCurrTime >= MaxBERSearchTimeout)
                    {
                        if (clsMsgHelper.ShowYesNo("Question", "Quá thời gian mà không đo được, bạn có thực hiện lại") == DialogResult.No)
                        {
                            //  m_duDisplayValue = "FAIL";
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                            goto RETRY_LABEL;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                clsLogManager.LogError(ex.ToString());
                if (clsMsgHelper.ShowYesNo("Question", "Lỗi quá trình đo, bạn có thực hiện lại") == DialogResult.No)
                {
                    //  m_duDisplayValue = "FAIL";
                    return TaskResult.FAIL;
                }
                else
                {
                    MessageBox.Show("Đảm bảo DUT ở chế độ thu");
                    goto RETRY_LABEL;
                }
            }
        }
        #region Test Parameter
        [Category("Test Parameter")]
        [DisplayName("Max Pass")]
        [SaveAtt()]
        public double MaxPass { set; get; }

        [Category("Test Parameter")]
        [DisplayName("Delay after setting (ms)")]
        [SaveAtt()]
        public int DelayAfterSetConfig { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Expected BER")]
        [SaveAtt()]
        public double ExpectedBER { set; get; }

        [Category("Test Parameter")]
        [DisplayName("RF level step")]
        [SaveAtt()]
        public double RfLvStep { set; get; }

        [Category("Test Parameter")]
        [DisplayName("TimeOut Get BER (ms)")]
        [SaveAtt()]
        public int TimeOutGetBER { set; get; }

        [Category("Test Parameter")]
        [DisplayName("WaitTime Get BER (ms)")]
        [SaveAtt()]
        public int WaitTimeGetBER { set; get; }

        #endregion

        #region DUT
        [Category("DUT")]
        [DisplayName("DUT Frequency Tx (GHz)")]
        [SaveAtt()]
        public double DutFreqTx { set; get; }

        [Category("DUT")]
        [DisplayName("DUT Frequency Rx (GHz)")]
        [SaveAtt()]
        public double DutFreqRx { set; get; }

        [Category("DUT")]
        [DisplayName("DUT mode")]
        [SaveAtt()]
        public VHCR_Mod DutMode { set; get; }

        [Category("DUT")]
        [DisplayName("DUT Bandwidth (W)")]
        [SaveAtt()]
        public VHCR_Bandwidth DutBW { set; get; }

        [Category("DUT")]
        [DisplayName("DUT Wareform")]
        [SaveAtt()]
        public VHCR_Wareform DutWF { set; get; }
        #endregion

        #region Signal generator
        [Category("Signal generator")]
        [DisplayName("RF level")]
        [SaveAtt()]
        public double RFLevel { set; get; }

        [Category("Signal generator")]
        [DisplayName("RF Output")]
        [SaveAtt()]
        public OutputPort RFOutport { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Reslut")]
        [SaveAtt()]
        public PowerUnit Result_unit { set; get; }

        [Category("Signal generator_E8267D")]
        [DisplayName("WaveForm")]
        [SaveAtt()]
        public string WaveForm { set; get; }
        [Category("Signal generator")]
        [DisplayName("Sample Clock (MHz)")]
        [SaveAtt()]
        public double sampleClock { set; get; }

        [Category("Test Parameter")]
        [DisplayName("Cable Loss_EXAttenuator Sig Gen(dB)")]
        [SaveAtt()]
        public double CableLoss { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Max BER search timeout(ms)")]
        [SaveAtt()]
        public int MaxBERSearchTimeout { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT Parameter(ms)")]
        [SaveAtt()]
        public int delayDUTPara { set; get; }
        [Category("Signal Generator_SMW200A")]
        [DisplayName("ARB file name")]
        [SaveAtt()]
        public string ARBFileName { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Parameter Search Ber")]
        [SaveAtt()]
        public string ParameterSearchBer { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Maximum number of ber reads")]
        [SaveAtt()]
        public byte MaxNumBerRead { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Number of ber reads for evaluate")]
        [SaveAtt()]
        public byte NumBerReadEvaluate { set; get; }
        #endregion
    }
}
