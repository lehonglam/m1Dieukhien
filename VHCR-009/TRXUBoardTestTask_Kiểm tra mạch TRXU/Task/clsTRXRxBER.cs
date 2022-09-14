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
    [clsTaskMetaInfo("TRXU board Rx BER", "clsTRXRxBER", true)]
    public class clsTRXRxBER : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_duDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        public enum MeasureValueType
        {
            CURRENT,
            MAX,
            MIN,
            AVERAGE
        }
        public clsTRXRxBER()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "TRXU board Rx BER";
            RxFrequency = 3640;
            CableLoss = 1.5;
            RFPower = -45;
            ARBFileName = "VHCR-10GHz_BW_QAM64_120RB.vv";
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
            get { return "clsTRXRxBER"; }
        }

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
            RETRY_LABEL:
            ISignalGenDevice signalGenDevice = (DevManager.GetDevService(SysDevType.SIG_GEN.ToString())) as ISignalGenDevice;
            if(signalGenDevice==null)
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
                bool bOK=false;
                double RFPower_offset_cableloss = RFPower + CableLoss;
                bool bIsDevEnable = DevManager.IsDevEnable(SysDevType.SIG_GEN.ToString());
                m_duDisplayValue = "NA";
                bOK = false;
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
                bOK = signalGenDevice.SetRFFrequency(RxFrequency, FreqUnit.MHz);
                bOK = signalGenDevice.SetOutputPower(RFPower_offset_cableloss, PowerUnit.dBm);
                bOK = signalGenDevice.EnablePlayARBFile(ARBFileName);
                //bOK = signalGenDevice.SetARBClockFrequency(30.72, FreqUnit.MHz);
                bOK = signalGenDevice.EnableOutput(true);
                //
                //bOK = DutService.StartRxBER();
                double dutTxFreqConvert = 13800 - TxFrequency;
                double dutRxFreqConvert = 13800 - RxFrequency;
                bOK = DutService.SetTxChanelConfig(VHCR_Wareform.FIX, VHCR_Mod, VHCR_Bandwidth, (ulong)(dutTxFreqConvert * 1e3), (ulong)(dutRxFreqConvert * 1e3), 0, 0);
                //
                double duCurrRFPower = RFPower_offset_cableloss;
                double duCurrRFPower_old = RFPower_offset_cableloss;
                int nBegTime = Environment.TickCount;
                int nCurrTime = 0;
                double duBERValue = 0;
                double duBERValue_old = 0;
                clsLogManager.LogReport("{0, 25}{1, 25}", "RF Level", "BER");
                List<double> lstDuTxPower = new List<double>();
                while (true)
                {
                    signalGenDevice.SetOutputPower(duCurrRFPower, PowerUnit.dBm);
                    if (SleepBeforeReadBER > 0)
                    {
                        clsLogManager.LogReport("SleepBeforeReadBER: {0}ms", SleepBeforeReadBER);
                        Thread.Sleep(SleepBeforeReadBER);
                    }
                    duBERValue_old = duBERValue;
                    duBERValue = GetBERFromDUT(DutService, TimeoutGetListBER);
                    if(duBERValue.ToString()=="NaN")
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
                    clsLogManager.LogWarning("{0, 25}{1, 25}",
                                 (duCurrRFPower-CableLoss).ToString("F04"), duBERValue.ToString("F04"));
                    //m_duDisplayValue = $"{duCurrRFPower}/{duBERValue.ToString("F04")}";
                    if (duBERValue >= MaxBERLimit)
                        break;
                    m_duDisplayValue = $"{(duCurrRFPower-CableLoss).ToString("F03")}";
                    nCurrTime = Environment.TickCount - nBegTime;
                    if (nCurrTime >= MaxBERSearchTimeout)
                        throw new ArgumentException("Timeout for searching BER...");
                    duCurrRFPower_old = duCurrRFPower;
                    duCurrRFPower = duCurrRFPower - Step;
                }
                if((duCurrRFPower_old - CableLoss)<=RxSensitivityLimit)
                {
                    clsLogManager.LogReport("RFPowerIn={0} dBm, Meas_RF_IN={1} dBm,Cableloss={2} dB, RxSensitivityLimit={3} dBm", duCurrRFPower_old - CableLoss, duCurrRFPower_old, CableLoss, RxSensitivityLimit);
                        if(duBERValue_old<= MaxBERLimit)
                        {
                            clsLogManager.LogReport("Result PASS: Ber Result={0} <= {1}", duBERValue_old, MaxBERLimit);
                            bOK = true;
                        }
                        else
                        {
                            clsLogManager.LogError("Result FAIL: Ber Result={0} > {1}",duBERValue_old,MaxBERLimit);
                            bOK = false;
                        }
                    
                }
                else
                {
                    clsLogManager.LogError("Result FAIL: RFPowerIn={0} dBm, Meas_RF_IN={1} dBm,Cableloss={2} dB, RxSensitivityLimit={3} dBm", duCurrRFPower_old-CableLoss,duCurrRFPower_old,CableLoss, RxSensitivityLimit);
                    bOK = false;
                }
                //m_duDisplayValue = $"{duCurrRFPower}/{duBERValue.ToString("F04")}";
                //
                if (bOK)
                    return TaskResult.PASS;
                else
                {
                    if (clsMsgHelper.ShowYesNo("Question", "Kết quả không đạt, bạn có thực hiện lại") == DialogResult.No)
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
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Excution: {0}", ex.ToString());
                m_duDisplayValue = "NA";
                return TaskResult.FAIL;
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
                    else if (measureValueType == MeasureValueType.MAX)
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
            clsTRXRxBER task = new clsTRXRxBER();
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
        [Category("Signal Generator")]
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
        [DisplayName("Time Out To Get BER")]
        [Description("TimeOut to get list BER with delta")]
        [SaveAtt()]
        public int TimeoutGetListBER { set; get; }
        [Category("Measurement Parameter")]
        [DisplayName("Sleep Time Between Get BER")]
        [SaveAtt()]
        public int SleepTimeBetweenGetBER { set; get; }
        [Category("Measurement Parameter")]
        [DisplayName("Rx Sensitivity Min Limit")]
        [SaveAtt()]
        public double RxSensitivityLimit { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
