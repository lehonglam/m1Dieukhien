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
using VHCRTestTask;
using VHCRTestTask.Tasks.VHCR;

namespace VHCRTestTask.Tasks.VHCR
{
    [clsTaskMetaInfo("VHCR - Rx Mức tín hiệu đầu vào tối đa", "clsVHCR_Rx_MaxInputLevel", true)]
    class clsVHCR_Rx_MaxInputLevel : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        public string m_duDisplayValue = "";
        private Type_TestValue m_type_testvalue = Type_TestValue.Pass_Fail_Value;
        clsFreqTypeConv FreqTypeConv = new clsFreqTypeConv();

        public clsVHCR_Rx_MaxInputLevel()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - Rx Mức tín hiệu đầu vào tối đa 15dBm trong 5 phút";
            MaxPass = 1; //uV
            //MinPass = 0;
            DelayAfterSetConfig = 1000;
            ExpectedBER = 1e-5;
            RfLvStep = 1;
            TimeOutGetBER = 1000; //ms
            WaitTimeGetBER = 500;
            TimeTest = 300; // seconds
            CableLoss = 4.83;
            Result_unit = PowerUnit.dBm;
            WaveForm = "";
            RFLevelMax = 15; //dBm      
            RFOutport = OutputPort.RFOut;
            delayDUTPara = 10000;
            ARBFileName = $"/var/user/vhcr/22042022/VHCR_BW20M_QAM64_120RB.wv";
            DutBW = VHCR_Bandwidth.FIX_20M;
            DutFreqRx = 10.02; //GHz
            DutMode = VHCR_Mod.FIX_QAM64;
            DutWF = VHCR_Wareform.FIX;
            arrDutFreqRx = $"10.02,10.04,10.06,10.14,10.16,10.18";
            arrDutFreqTx = $"10.14,10.16,10.18,10.02,10.04,10.06";
            SenMaxPass = -70;
            RFLevel = -73;
            CableLoss_low = 4.83;
            CableLoss_high = 5.5;
            sampleClock = 30.72;
            ParameterSearchBer = $"1,2,3,5,6,7,9,10,11,13,15,17,18,19,21,23,25,27,29,31";
            MaxNumBerRead = 10;
            NumBerReadEvaluate = 5;
        }
        [Category("#General Option")]
        [Browsable(false)]
        public string Name
        {
            get { return "clsVHCR_Rx_MaxInputLevel"; }
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
            return $"{m_duDisplayValue} {Result_unit}";
        }
        public string GetDisplayMaxValue()
        {
            return "NA";
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
            return $"{m_duDisplayValue}";
        }

        public string GetMaxValue()
        {
            return "NA";
        }

        public string GetMinValue()
        {
            return $"NA";
        }

        public string GetUnit()
        {
            return "";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            ISignalGenDevice SigGenService = null;
            ISignalGenDeviceE8267D sigGenE8267D = null;
            ISignalGenDeviceSMW200A sigGenSMW200A = null;
            IDutVHCR VHCRService = null;
            bool bOK = false;
            string strMsg = null;
            m_duDisplayValue = "NaN";
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
                    bOK = false;
                    double RxFreq = 10.02;
                    double TxFreq = 10.02;
                    string[] listRX = arrDutFreqRx.Split(',');
                    string[] listTX = arrDutFreqTx.Split(',');
                   
                    if (listRX.Length!=listTX.Length)
                    {
                        if (clsMsgHelper.ShowYesNo("Question", "Chưa cài đặt đúng dải tần số kiểm tra độ nhạy, bạn có thực hiện lại không?") == DialogResult.No)
                        {
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            goto RETRY_LABEL;
                        }
                    }
                    
                    List<clsVHCR_Rx_Sensitivity> lstSensitivity = new List<clsVHCR_Rx_Sensitivity>();
                    for (int i = 0; i < listRX.Length; i++)
                    {
                        try
                        {
                            clsVHCR_Rx_Sensitivity Task = new clsVHCR_Rx_Sensitivity();
                            Task.Result_unit = Result_unit;
                            Task.RFLevel = RFLevel;
                            Task.RfLvStep = RfLvStep;
                            Task.RFOutport = RFOutport;
                            Task.TimeOutGetBER = TimeOutGetBER;
                            Task.WaitTimeGetBER = WaitTimeGetBER;
                            Task.WaveForm = WaveForm;
                            Task.ARBFileName = ARBFileName;
                            
                            Task.delayDUTPara = delayDUTPara;
                            Task.DutBW = DutBW;
                            Double.TryParse(listRX[i], out RxFreq);
                            Double.TryParse(listTX[i], out TxFreq);
                            if ((RxFreq==0)||(TxFreq==0))
                            {
                                if (clsMsgHelper.ShowYesNo("Question", "Chưa cài đặt đúng định đạng tần số kiểm tra độ nhạy, bạn có thực hiện lại không?") == DialogResult.No)
                                {
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    goto RETRY_LABEL;
                                }
                            }
                            Task.DutFreqRx = RxFreq;                          
                            Task.DutFreqTx = TxFreq;
                            if (RxFreq < 10.12)
                            {
                                Task.CableLoss = CableLoss_low;
                            }
                            else
                            {
                                Task.CableLoss = CableLoss_high;
                            }
                            Task.DutMode = DutMode;
                            Task.DutWF = DutWF;
                            Task.ExpectedBER =ExpectedBER ;
                            Task.MaxPass = SenMaxPass;
                            Task.MaxBERSearchTimeout = MaxBERSearchTimeout;
                            Task.DelayAfterSetConfig = DelayAfterSetConfig;
                            Task.sampleClock = sampleClock;
                            Task.MaxNumBerRead = MaxNumBerRead;
                            Task.NumBerReadEvaluate = NumBerReadEvaluate;
                            Task.ParameterSearchBer = ParameterSearchBer;                    
                            Task.SetModuleTask(m_IModuleTask);
                            lstSensitivity.Add(Task);
                        }
                        catch (Exception ex)
                        {
                            clsLogManager.LogError("Error: " + ex);
                            if (clsMsgHelper.ShowYesNo("Question", "Cài đạt tham số đo độ nhạy fail, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                goto RETRY_LABEL;
                            }
                        }
                    }
                    clsLogManager.LogReport("List Frequency for Measuring Sensitivity:") ;
                    clsLogManager.LogReport("{0,25}{1,25}{2,25}","RX","TX","Unit");
                    foreach (clsVHCR_Rx_Sensitivity task in lstSensitivity)
                        {
                            clsLogManager.LogReport("{0,25}{1,25}{2,25}", task.DutFreqRx.ToString(),task.DutFreqTx.ToString(),"GHz");
                        }
                    //Thiết lập SigGen
                    if (DevManager.IsDevEnable(SysDevType.SIG_GEN.ToString()) && DevManager.IsDevEnable(SysDevType.DUT.ToString()))
                    {
                        IDeviceInfor deviceinfo = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
                        if (deviceinfo == null)
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
                        if(!SigGenService.SetModulationType(ModulatioType.OFF)) throw new System.IO.IOException();
                        if(!SigGenService.SetRFFrequency(DutFreqRx, FreqUnit.GHz)) throw new System.IO.IOException();
                        if (!SigGenService.SetOutputPower(RFLevelMax+CableLoss, Result_unit)) throw new System.IO.IOException(); 
                        if(!SigGenService.Select_OutputPort(RFOutport)) throw new System.IO.IOException();

                        // Đặt tham số máy: 
                        if (VHCRService.SetParamChanel(DutWF, DutMode, DutBW, (ulong)(DutFreqTx * 1e6), (ulong)(DutFreqRx * 1e6), 0, 0))
                        {
                            Thread.Sleep(delayDUTPara);
                            bOK = true;
                        }
                        else
                        {
                            strMsg = $"SetParamChanel: {DutWF}, {DutMode}, {DutBW}, {DutFreqRx} GHz => FAIL";
                            bOK = false;
                        }

                        if(bOK)
                        {
                            bOK = false;
                            
                            // Thực hiện đo
                            if(!SigGenService.EnableOutput(true)) throw new System.IO.IOException();
                            clsLogManager.LogReport("Enable Output RF SigGen");
                            clsLogManager.LogReport($"Waiting RF SigGen in {TimeTest}s");
                            int TimeRun = 0;
                            while (TimeRun <= TimeTest)
                            {
                                clsLogManager.LogReport($"Time Test: {TimeRun}/{TimeTest} s");
                                Thread.Sleep(10000);
                                TimeRun += 10;
                            }

                            if(!SigGenService.EnableOutput(false)) throw new System.IO.IOException();
                            clsLogManager.LogReport("Off Output RF SigGen");
                            bool isShowDialogLowChannel = false;
                            bool isShowDialogHighChannel = false;
                            foreach (var Task in lstSensitivity)
                            {
                                if (Task.DutFreqRx >= 10.12)
                                {
                                    if (isShowDialogHighChannel == false)
                                    {
                                        MessageBox.Show("Đấu giắc kênh cao với máy phát");
                                        isShowDialogHighChannel = true;
                                        isShowDialogLowChannel = false;
                                    }
                                }
                                else
                                {
                                    if (isShowDialogLowChannel == false)
                                    {
                                        MessageBox.Show("Đấu giắc kênh thấp với máy phát");
                                        isShowDialogLowChannel = true;
                                        isShowDialogHighChannel = false;
                                    }
                                }
                                if (Task.Excution(DevManager) == TaskResult.PASS)
                                {
                                    clsLogManager.LogWarning($"Kết quả Bài đo độ nhạy tại tần số: {Task.DutMode}, {Task.DutMode}, {Task.DutBW}MHz, {Task.DutFreqRx}MHz => PASS");
                                    bOK = true;
                                }
                                else
                                {
                                    strMsg = $"Kết quả Bài đo độ nhạy tại tần số: {Task.DutMode}, {Task.DutMode}, {Task.DutBW}MHz, {Task.DutFreqRx}MHz => FAIL";
                                    bOK = false;
                                    break;
                                }
                            }
                    }                       
                    }
                    else
                    {
                        strMsg = "Devices are unvailable!";
                        bOK = false;
                    }

                }
                catch (System.Exception ex)
                {
                    m_duDisplayValue = "FAIL";
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
                    if(SigGenAccess!=null)
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
                    if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                    {
                        break;
                    }
                    if (DUT != null)
                    {
                        DUT.DisconnectDevice();
                    }
                    if (deviceinfo_VHCR != null)
                    {
                        VHCRService.Connect2Device(deviceinfo_VHCR);
                    }               
                }
            }
            if (bOK)
            {
                clsLogManager.LogWarning(Description + ": PASS");
                m_duDisplayValue = "15";
                return TaskResult.PASS;
            }
            else
            {
                clsLogManager.LogWarning(Description + ": FAIL");
                m_duDisplayValue = "NaN";
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
            clsVHCR_Rx_MaxInputLevel task = new clsVHCR_Rx_MaxInputLevel();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
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

        [Category("Test Parameter")]
        [DisplayName("Time Test (s)")]
        [SaveAtt()]
        public int TimeTest { set; get; }

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
        [DisplayName("RF level Max")]
        [SaveAtt()]
        public double RFLevelMax { set; get; }

        [Category("Signal generator")]
        [DisplayName("RF Output")]
        [SaveAtt()]
        public OutputPort RFOutport { set; get; }
        [Category("Test Parameter")]
        [DisplayName("RF level unit")]
        [SaveAtt()]
        public PowerUnit Result_unit { set; get; }

        [Category("Signal generator_E8267D")]
        [DisplayName("Wave Form")]
        [SaveAtt()]
        public string WaveForm { set; get; }
        [Category("Signal generator")]
        [DisplayName("Sample Clock (MHz)")]
        [SaveAtt()]
        public double sampleClock { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Cable Loss(dB)")]
        [SaveAtt()]
        public double CableLoss { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT Parameter(ms)")]
        [SaveAtt()]
        public int delayDUTPara { set; get; }
        [Category("Signal Generator_SMW200A")]
        [DisplayName("ARB file name")]
        [SaveAtt()]
        public string ARBFileName { set; get; }
        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("List DUT Frequency Tx (GHz)")]
        [SaveAtt()]
        public string arrDutFreqTx { set; get; }

        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("List DUT Frequency Rx (GHz)")]
        [SaveAtt()]
        public string arrDutFreqRx { set; get; }
        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("RF level Initial")]
        [SaveAtt()]
        public double RFLevel { set; get; }
        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("Sensitivity Max Pass")]
        [SaveAtt()]
        public double SenMaxPass { set; get; }
        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("Max BER search timeout(ms)")]
        [SaveAtt()]
        public int MaxBERSearchTimeout { set; get; }
        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("Cable Loss_Low Channel(dB)")]
        [SaveAtt()]
        public double CableLoss_low { set; get; }
        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("Cable Loss_High Channel(dB)")]
        [SaveAtt()]
        public double CableLoss_high { set; get; }
        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("Parameter Search Ber")]
        [SaveAtt()]
        public string ParameterSearchBer { set; get; }
        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("Maximum number of ber reads")]
        [SaveAtt()]
        public byte MaxNumBerRead { set; get; }
        [Category("Parameter for Measuring Sensitivity")]
        [DisplayName("Number of ber reads for evaluate")]
        [SaveAtt()]
        public byte NumBerReadEvaluate { set; get; }
        #endregion
    }
}
