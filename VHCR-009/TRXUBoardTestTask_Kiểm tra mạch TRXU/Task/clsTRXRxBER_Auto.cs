using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using CFTWinAppCore.Task;
using CFTWinAppCore.Sequences;
using CFTWinAppCore.DataLog;
using LogLibrary;
using GeneralTool;
using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.DeviceManager.DUTIOService;
using System.Windows.Forms;
using TRXUBoardTestTask.TypeConverters;
using CFTWinAppCore.DeviceManager.DUT;
using Option;

namespace TRXUBoardTestTask.Task
{
    [clsTaskMetaInfo("TRXU board Rx BER_De", "clsTRXRxBER_Auto", true)]
    public class clsTRXRxBER_Auto : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_duDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        Queue<PairValue> queueBerAndRFIN = null;
        List<int> listIncreaseRF = new List<int>();
        class PairValue
        {
            public double Ber { set; get; }
            public double LevelRF { set; get; }
            public byte bin { set; get; }

        }

        public enum MeasureValueType
        {
            CURRENT,
            MAX,
            MIN,
            AVERAGE
        }
        public clsTRXRxBER_Auto()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "TRXU board Rx BER";
            RxFrequency = 3640;
            CableLoss = 1.5;
            RFPower = -45;
            ARBFileName = "VHCR-10GHz_BW_QAM64_120RB.vv";
            sampleClock = 30.72;
            MaxBERLimit = 1e-5;
            MaxBERSearchTimeout = 30000;
            VHCR_Bandwidth = VHCR_Bandwidth.FIX_20M;
            VHCR_Mod = VHCR_Mod.FIX_QAM64;
            TxFrequency = 3600;
            SleepBeforeReadBER = 5000;
            measureValueType= MeasureValueType.CURRENT;
            TimeoutGetListBER = 30000;
            MeasurementCount = 5;
            SleepTimeBetweenGetBER = 100;
            MaxDeltaBER = 0.00001;
            RxSensitivityLimit = -55;
            Step = 1;
            Result_unit = "dBm";
            ParameterSearchBer = $"1,2,3,5,6,7,9,10,11,13,15,17,18,19,21,23,25,27,29,31";
            MaxNumBerRead = 10;
            NumBerReadEvaluate = 5;
        }
        [Browsable(false)]
        public bool SuperAdmin
        {
            get;
            set;
        }
        public TaskResult GetTaskResult()
        {
            return taskResult;
        }

        [Category("#General Option")]
        [Browsable(false)]
        public string Name
        {
            get { return "clsTRXRxBER_Auto"; }
        }
        [Browsable(false)]
        [Category("#General Option")]
        [DisplayName("ID Specification")]
        [SaveAtt()]
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
        [Browsable(false)]
        public int TypeMes
        {
            get
            {
                return (int)m_type_testvalue;
            }
        }
        public string GetValue()
        {
            return $"{m_duDisplayValue}";
        }

        public string GetMaxValue()
        {
            return $"{RxSensitivityLimit.ToString("F03")}";
        }

        public string GetMinValue()
        {
            return $"NA";
        }

        public string GetUnit()
        {
            return $"{Result_unit}";
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
            return $"{m_duDisplayValue} {Result_unit}";
        }
        public string GetDisplayMaxValue()
        {
            return $"{RxSensitivityLimit} {Result_unit}";
        }
        public string GetDisplayMinValue()
        {
            return "NA";
        }
        public string GetAddTaskDescription()
        {
            return $"{RxFrequency}Mhz";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            m_TraceService.WriteLine(" Test Task: " + Description + " Frequency="+RxFrequency.ToString()+"MHz \n");
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.TRXU_BOARD.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_TRXU = DevManager.GetDevRunTimeInfo(SysDevType.TRXU_BOARD.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_sigGen = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
            IAccessDeviceService SigGenAccess = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as IAccessDeviceService;
            RETRY_LABEL:
            int temp = 0;
            queueBerAndRFIN = new Queue<PairValue>(NumBerReadEvaluate);
            string[] arrParameterSearchBER = ParameterSearchBer.Split(',');
            for (int i = 0; i < arrParameterSearchBER.Length; i++)
            {
                int.TryParse(arrParameterSearchBER[i], out temp);
                listIncreaseRF.Add(temp);
            }
            ISignalGenDevice signalGenDevice = (DevManager.GetDevService(SysDevType.SIG_GEN.ToString())) as ISignalGenDevice;
            if (signalGenDevice == null)
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
            try
            {
                ITRXUBoard DutService = (DevManager.GetDevService(SysDevType.TRXU_BOARD.ToString())) as ITRXUBoard;
                if (DutService == null)
                {
                    clsLogManager.LogError("Can not get device service: ITRXUBoard");
                    if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL;
                    }
                }

                double RFPower_offset_cableloss = RFPower + CableLoss;
                bool bIsDevEnable = DevManager.IsDevEnable(SysDevType.SIG_GEN.ToString());
                m_duDisplayValue = "NA";
                //Set RF cable loss
                //
                IAccessDeviceService accessDeviceService = signalGenDevice as IAccessDeviceService;
                //if (accessDeviceService != null) accessDeviceService.InitDevice();
                if(!accessDeviceService.IsDeviceConnected())
                {
                    clsLogManager.LogError("Signal gen device is not connected!");
                    if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL;
                    }
                }
                //Thread.Sleep(2000);
                if(!signalGenDevice.SetRFFrequency(RxFrequency, FreqUnit.MHz)) throw new System.IO.IOException();
                if(!signalGenDevice.SetOutputPower(RFPower_offset_cableloss, PowerUnit.dBm)) throw new System.IO.IOException();
                if(!signalGenDevice.EnablePlayARBFile(ARBFileName)) throw new System.IO.IOException();
                if(!signalGenDevice.SetARBClockFrequency(sampleClock, FreqUnit.MHz)) throw new System.IO.IOException();
                if(!signalGenDevice.EnableOutput(true)) throw new System.IO.IOException();
                //
                double dutTxFreqConvert = 13800 - TxFrequency;
                double dutRxFreqConvert = 13800 - RxFrequency;
                DutService.SetTxChanelConfig(VHCR_Wareform.FIX, VHCR_Mod, VHCR_Bandwidth, (ulong)(dutTxFreqConvert * 1e3), (ulong)(dutRxFreqConvert * 1e3), 0, 0);
                //
                double duCurrRFPower = RFPower_offset_cableloss;
                clsLogManager.LogReport("{0, 25}{1, 25}", "RF Level", "BER");
                List<double> lstDuTxPower = new List<double>();
                if(RFPower<=RxSensitivityLimit)
                {
                    return searchBerIncreaseRFPower(signalGenDevice, DutService, duCurrRFPower);
                }
                else
                {
                    clsLogManager.LogError("RF Level = {0} dBm > Expect Level (Max Pass)={1} dBm", RFPower, RxSensitivityLimit);
                    if (clsMsgHelper.ShowYesNo("Question", "Cài đặt lại RF Level<= Max Pass, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {

                        goto RETRY_LABEL;
                    }
                }
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Excution: {0}", ex.ToString());
                m_duDisplayValue = "NA";
                if (clsMsgHelper.ShowYesNo("Question", "Exception, bạn có thực hiện lại không?") == DialogResult.No)
                {
                    return TaskResult.FAIL;
                }
                else
                {
                    if (DUT != null)
                    {
                        DUT.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_TRXU != null)
                            DUT.Connect2Device(deviceinfo_TRXU);
                    }
                    if (SigGenAccess != null)
                    {
                        SigGenAccess.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_sigGen != null)
                            SigGenAccess.Connect2Device(deviceinfo_sigGen);
                    }
                    goto RETRY_LABEL;
                }
            }
            finally
            {
                signalGenDevice.EnableOutput(false);
            }
        }
        double GetBERFromDUT(ITRXUBoard DutService, int Timeout=30000)
        {
            List<double> LstBerValue=new List<double>();
            int BeginTime = Environment.TickCount;
            while(true)
            {
                LstBerValue.Clear();
                for (int i = 0; i <MeasurementCount;i++)
                {
                    if(i>0) Thread.Sleep(SleepTimeBetweenGetBER);
                    double duBERValue = DutService.GetBER(1000);
                    clsLogManager.LogReport("BER: {0}", duBERValue.ToString());
                    if(duBERValue<-1||duBERValue.ToString()==double.NaN.ToString())
                    {
                        goto CHECK_TIMEOUT;
                    }
                    LstBerValue.Add(duBERValue);
                    
                }
                if(LstBerValue.Count == MeasurementCount)
                {
                    double DeltaBer=LstBerValue.Max()-LstBerValue.Min();
                    if(DeltaBer>= MaxDeltaBER)
                    {
                        clsLogManager.LogError("Delta BER is bigger than setting. Current Delta {0}, setting Delta {1}", DeltaBer, MaxDeltaBER);
                        goto CHECK_TIMEOUT;
                    }
                    if(measureValueType== MeasureValueType.MIN)
                    {
                        return LstBerValue.Min();
                    }
                    else if (measureValueType == MeasureValueType.MAX)
                    {
                        return LstBerValue.Max();
                    }
                    else if (measureValueType == MeasureValueType.AVERAGE)
                    {
                        return LstBerValue.Average();
                    }
                    else
                    {
                        return LstBerValue.ElementAt(LstBerValue.Count() - 1);
                    }
                }
                CHECK_TIMEOUT:
                if(Environment.TickCount-BeginTime>=Timeout)
                {
                    clsLogManager.LogError("Timeout to get BER");
                    Thread.Sleep(500);
                    return double.NaN;
                }
            }

        }
        private TaskResult searchBerIncreaseRFPower (ISignalGenDevice signalGenDevice, ITRXUBoard DutService,double RFin)
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
                    if (SleepBeforeReadBER > 0)
                    {
                        clsLogManager.LogReport("SleepBeforeReadBER: {0}ms", SleepBeforeReadBER);
                        Thread.Sleep(SleepBeforeReadBER);
                    }
                    duBERValue = GetBERFromDUT(DutService, TimeoutGetListBER);
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
                    if (duBERValue <= MaxBERLimit)
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
                            duCurrRFPower = duCurrRFPower + Step;
                            clsLogManager.LogWarning("Tăng cao tần with step={0} dB => RFPowerIn={1} dBm", Step, duCurrRFPower - CableLoss);
                            if ((duCurrRFPower - CableLoss) > RxSensitivityLimit)
                            {
                                if (clsMsgHelper.ShowYesNo("Question", $"Mức cao tần tiếp theo = {duCurrRFPower - CableLoss} dBm vượt quá giá trị cho phép = {RxSensitivityLimit} dBm, bạn có thực hiện lại") == DialogResult.No)
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
                            m_duDisplayValue = arrBerAndRFIN[4].LevelRF.ToString();//duCurrRFPower - CableLoss;
                            clsLogManager.LogReport("RFPowerIn={0} dBm, Meas_RF_IN={1} dBm,Cableloss={2} dB, RxSensitivityLimit={3} dBm", duCurrRFPower - CableLoss, duCurrRFPower, CableLoss, RxSensitivityLimit);
                            clsLogManager.LogReport("Result PASS: Ber Result={0} <= {1}", arrBerAndRFIN[4].Ber, MaxBERLimit);
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
            clsTRXRxBER_Auto task = new clsTRXRxBER_Auto();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        //
        [Category("Test Parameter")]
        [DisplayName("Cable loss(dB)")]
        [SaveAtt()]
        public double CableLoss { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Step RF Power(dB)")]
        [SaveAtt()]
        public double Step { set; get; }
        //
        [Category("DUT")]
        [DisplayName("Rx frequency(Mhz)")]
        [SaveAtt()]
        public double RxFrequency { set; get; }
        //
        [Category("Signal Generator")]
        [DisplayName("Signal gen RF power(dBm)")]
        [SaveAtt()]
        public double RFPower { set; get; }
        //
        [Category("Signal Generator")]
        [DisplayName("ARB file name")]
        [SaveAtt()]
        public string ARBFileName { set; get; }
        [Category("Signal Generator")]
        [DisplayName("Sample Clock (MHz)")]
        [SaveAtt()]
        public double sampleClock { set; get; }
        //
        [Category("Test Parameter")]
        [DisplayName("Max BER limit")]
        [SaveAtt()]
        public double MaxBERLimit { set; get; }
        //
        [Category("Test Parameter")]
        [DisplayName("Max BER search timeout(ms)")]
        [SaveAtt()]
        public int MaxBERSearchTimeout { set; get; }
        [Category("DUT")]
        [DisplayName("VHCR Bandwidth")]
        [SaveAtt()]
        public VHCR_Bandwidth VHCR_Bandwidth { set; get; }
        [Category("DUT")]
        [DisplayName("VHCR Mod")]
        [SaveAtt()]
        public VHCR_Mod VHCR_Mod { set; get; }
        [Category("DUT")]
        [DisplayName("Tx frequenccy(Mhz)")]
        [SaveAtt()]
        public double TxFrequency { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Sleep Before Read BER(ms)")]
        [SaveAtt()]
        public int SleepBeforeReadBER { set; get; }
        [Category("Measurement Parameter")]
        [DisplayName("Measurement Count")]
        [Description("Size of list BER value. After get list value of BER, tool will calculate delta BER and determine to use list value or not")]
        [SaveAtt()]
        public int MeasurementCount { set; get; }
        [Category("Measurement Parameter")]
        [DisplayName("Max Delta BER")]
        [Description("Maximum of Delta between BER value. If Delta of BER is bigger than this param, clear list of ber value end do get list BER again")]
        [SaveAtt()]
        public double MaxDeltaBER { set; get; }
        [Category("Measurement Parameter")]
        [DisplayName("MeasureValueType")]
        [Description("Type of BER value from list of measured BER: Min, Max, Average, Current")]
        [SaveAtt()]
        public MeasureValueType measureValueType { set; get; }
        [Category("Measurement Parameter")]
        [DisplayName("Time Out To Get BER(ms)")]
        [Description("TimeOut to get list BER with delta")]
        [SaveAtt()]
        public int TimeoutGetListBER { set; get; }
        [Category("Measurement Parameter")]
        [DisplayName("Sleep Time Between Get BER(ms)")]
        [SaveAtt()]
        public int SleepTimeBetweenGetBER { set; get; }
        [Category("Measurement Parameter")]
        [DisplayName("Rx Sensitivity Max Limit")]
        [SaveAtt()]
        public double RxSensitivityLimit { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
        [Category("Test Parameter")]
        [DisplayName("Parameter Search Ber")]
        [SaveAtt()]
        public string ParameterSearchBer { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Maximum number of ber read")]
        [SaveAtt()]
        public byte MaxNumBerRead { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Number of ber read for evaluate")]
        [SaveAtt()]
        public byte NumBerReadEvaluate { set; get; }
    }
}
