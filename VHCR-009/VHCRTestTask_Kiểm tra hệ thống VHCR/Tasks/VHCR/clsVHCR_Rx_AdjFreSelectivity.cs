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
using CFTWinAppCore.DeviceManager.PowerDevice;
using System.Windows.Forms;
using VHCRTestTask.TypeConvert;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.DeviceManager.DUTIOService;
using CFTWinAppCore.Helper;
using CFTSeqManager.DUT;
using Option;

namespace VHCRTestTask.Tasks.VHCR
{
    [clsTaskMetaInfo("VHCR - Rx Độ chọn lọc kênh lân cận", "clsVHCR_Rx_AdjFreSelectivity", true)]
    public class clsVHCR_Rx_AdjFreSelectivity : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private double m_duDisplayValue = double.MinValue;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;

        public clsVHCR_Rx_AdjFreSelectivity()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - Rx Độ chọn lọc kênh lân cận";
            MinPass = 20;
            DelayAfterSetConfig = 1000;
            ExpectedBER = 1e-5;
            RfLvStep = 1;
            WaitTimeGetBER = 500; //ms
            TimeOutGetBER = 1000; //ms
            TimeOut = 600000; //ms
            Result_unit = "dB";
            CableLoss_1 = 0;
            CableLoss_2 = 0;
            delayDUTPara = 10000;
            DutBW = VHCR_Bandwidth.FIX_20M;
            DutFreqRx = 10.02;
            DutFreqTx = 10.14;
            DutMode = VHCR_Mod.FIX_QAM64;
            DutWF = VHCR_Wareform.FIX;
            ARBFileName = $"/var/user/vhcr/22042022/VHCR_BW20M_QAM64_120RB.wv";
            Unit = PowerUnit.dBm;
            WaveForm = "";
            RFLevel_1st = -100;            
            RFOutport_1st = OutputPort.RFOut;
            RFFreq_1st = 10.02; //GHz
            sampleClock = 30.72;
            RFLevel_2nd = -100;
            RFOutport_2nd = OutputPort.RFOut;
            RFFreq_2nd = 4.45; //GHz
        }
        [Category("#General Option")]
        [Browsable(false)]
        public string Name
        {
            get { return "clsVHCR_Rx_AdjFreSelectivity"; }
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
                return $"{m_duDisplayValue.ToString("F03")} {Result_unit}";
        }
        public string GetDisplayMaxValue()
        {
            return "NA";
        }
        public string GetDisplayMinValue()
        {
            return $"{MinPass.ToString("F03")} {Result_unit}";
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
            return "NA";
        }

        public string GetMinValue()
        {
            return $"{MinPass.ToString("F03")}";
        }

        public string GetUnit()
        {
            return $"{Result_unit}";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            ISignalGenDevice SigGenService = null;
            ISignalGenDeviceE8267D sigGenE8267D = null;
            ISignalGenDeviceSMW200A sigGenSMW200A = null;
            ISignalGenDeviceE8267D SigGenService_2nd = null;
            IDutVHCR VHCRService = null;
            string strMsg = string.Empty;
            double SensitiveValue = double.MaxValue; // Giá trị độ nhạy máy thu
            double AdjFreSelectivity = double.MinValue; // Giá trị chọn lọc kênh lân cận
            double BERvalue = 0;
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_VHCR = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_sigGen = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
            IAccessDeviceService SigGenAccess = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_sigGen2 = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN_2nd.ToString()) as IDeviceInfor;
            IAccessDeviceService SigGenAccess2 = DevManager.GetDevService(SysDevType.SIG_GEN_2nd.ToString()) as IAccessDeviceService;
            m_TraceService.WriteLine("Begin Test Task: " + m_strDescription + string.Format("[{0}]", GetAddTaskDescription()));
            RETRY_LABEL:
            try
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
                SigGenService_2nd = DevManager.GetDevService(SysDevType.SIG_GEN_2nd.ToString()) as ISignalGenDeviceE8267D;
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
                if (SigGenService_2nd == null)
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
               //Ghi lại giá trị nhạy thu tại tần số kiểm tra
               #region Lấy giá trị nhạy thu
               SensitiveValue = clsTaskHelper.GetRxSensitivity(m_IModuleTask, DutFreqRx, DutMode);
                if (SensitiveValue == double.MaxValue)
                {
                    strMsg = $"Không lấy được kết quả của bài đo độ nhạy {DutMode} - {DutFreqRx} MHz, bạn có muộn thử lại?";
                    if (clsMsgHelper.ShowYesNo("Question", strMsg) == DialogResult.No)
                    {
                        m_duDisplayValue = double.NaN;
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL;
                    }
                }
                else
                {
                    clsLogManager.LogWarning("Get DisplayValue from task RxSensitivity {0} - {1} GHz: {2}dBm", DutMode, DutFreqRx, SensitiveValue);
                }
                #endregion
                // Setup SigGen_1st
                if (!SigGenService.SignalGenSetup()) throw new System.IO.IOException();
                if (!SigGenService.EnableOutput(false)) throw new System.IO.IOException();
                if (deviceinfo.DeviceName == "E8267D Signal Gen")
                {
                    if (!sigGenE8267D.LoadWaveForm(WaveForm)) throw new System.IO.IOException();
                    if (!sigGenE8267D.SetArbEnable(true)) throw new System.IO.IOException();
                    if (!sigGenE8267D.SetArbSampleClockRate(sampleClock * 1e6)) throw new System.IO.IOException();
                    if (!sigGenE8267D.SetALCState(false)) throw new System.IO.IOException();
                }
                else if (deviceinfo.DeviceName == "SMW200A Signal Gen")
                {
                    if (!sigGenSMW200A.EnablePlayARBFile(ARBFileName)) throw new System.IO.IOException();
                    if (!sigGenSMW200A.SetARBClockFrequency(sampleClock, FreqUnit.MHz)) throw new System.IO.IOException();
                }
                //SigGenService.SetModulationType(ModulatioType.CW); // Điều chế CW
                if(!SigGenService.SetRFFrequency(RFFreq_1st, FreqUnit.GHz)) throw new System.IO.IOException();
                if(!SigGenService.SetOutputPower(RFLevel_1st, Unit)) throw new System.IO.IOException();
                if(!SigGenService.Select_OutputPort(RFOutport_1st)) throw new System.IO.IOException();

                //Setup SigGen_2nd
                if(!SigGenService_2nd.SignalGenSetup()) throw new System.IO.IOException();
                if(!SigGenService_2nd.EnableOutput(false)) throw new System.IO.IOException();
                if(!SigGenService_2nd.SetModulationType(ModulatioType.OFF)) throw new System.IO.IOException();
                if(!SigGenService_2nd.SetRFFrequency(RFFreq_2nd, FreqUnit.GHz)) throw new System.IO.IOException();
                if(!SigGenService_2nd.SetOutputPower(RFLevel_2nd, Unit)) throw new System.IO.IOException();
                if(!SigGenService_2nd.Select_OutputPort(RFOutport_2nd)) throw new System.IO.IOException();

                // Đặt tham số máy: 
                if (VHCRService.SetParamChanel(DutWF, DutMode, DutBW, (ulong)(DutFreqTx * 1e6), (ulong)(DutFreqRx * 1e6), 0, 0))
                    {
                        Thread.Sleep(delayDUTPara);
                    }
                    else
                    {
                        clsLogManager.LogError("Đặt tham số cho DUT lỗi");
                        if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                        {
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            goto RETRY_LABEL;
                        }
                    }
                    // Băt đầu bài đo
                    // Thay đổi mức cao tần SigGen_1st tại giá trị độ nhạy
                    if(!SigGenService.SetOutputPower(SensitiveValue + CableLoss_1, PowerUnit.dBm)) throw new System.IO.IOException();
                    if(!SigGenService.EnableOutput(true)) throw new System.IO.IOException();
                RETRY_LABEL1:
                    clsLogManager.LogReport("Measure Ber at Sensitivity value get from previous task");
                List<ITask> lstTask = m_IModuleTask.Tasks;
                List<clsVHCR_Rx_Sensitivity> lstSensitivity = new List<clsVHCR_Rx_Sensitivity>();
                clsVHCR_Rx_Sensitivity sensiTask = null;

                for (int i = 0; i < lstTask.Count; i++)
                {
                    sensiTask = lstTask[i] as clsVHCR_Rx_Sensitivity;
                    if (sensiTask == null) continue;
                    if (sensiTask.DutFreqRx == DutFreqRx) break;
                }
                if(sensiTask.DutFreqRx!=DutFreqRx)
                {
                    clsLogManager.LogError("Not Task measure sensitivity with RX={0} GHz", DutFreqRx);
                    if (clsMsgHelper.ShowYesNo("Question", $"Không có bài đo độ nhạy cho tần số {DutFreqRx} GHz, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL;
                    }
                }
                sensiTask.RFLevel = SensitiveValue;
                if (sensiTask.searchBerIncreaseRFPower_3(SigGenService,VHCRService,SensitiveValue+CableLoss_1)== TaskResult.FAIL)
                {
                    clsLogManager.LogError("Đo độ nhạy tại tần số {0} GHz is Fail",DutFreqRx);
                    if (clsMsgHelper.ShowYesNo("Question", $"Đo độ nhạy tại tần số {DutFreqRx} GHz lỗi, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL1;
                    }
                }
                    if(!SigGenService_2nd.EnableOutput(true)) throw new System.IO.IOException();//bật máy phát 2
                double RFlvValue = RFLevel_2nd + CableLoss_2;
                    int StartTime = Environment.TickCount;
                    while (true)
                    {
                        if(!SigGenService_2nd.SetOutputPower(RFlvValue, Unit)) throw new System.IO.IOException();
                    clsLogManager.LogReport("Measure Sensitivity at {0} GHz with LevelRF SigGen 2 = {1} dBm", DutFreqRx,RFlvValue);
                        if (sensiTask.searchBerIncreaseRFPower_2(SigGenService,VHCRService,SensitiveValue+CableLoss_1) == TaskResult.FAIL)
                        {
                            AdjFreSelectivity = (RFlvValue - CableLoss_2 - SensitiveValue); // dBm
                            m_duDisplayValue = Math.Round(AdjFreSelectivity, 3);
                            clsLogManager.LogReport("AdjFreSelectivity = {0} when SigGen_1nd Power={1} dBm and SigGen_2nd={2} dBm ", m_duDisplayValue, SensitiveValue, RFlvValue - CableLoss_2);
                            if (AdjFreSelectivity >= MinPass)
                            {
                                clsLogManager.LogWarning($"Result PASS: Độ chọn lọc kênh lân cận={m_duDisplayValue.ToString("F03")} {Result_unit}");
                                return TaskResult.PASS;
                            }
                            else
                            {
                                clsLogManager.LogError($"Result FAIL: Độ chọn lọc kênh lân cận={m_duDisplayValue.ToString("F03")} {Result_unit} < MinPass={MinPass} {Result_unit}");
                                if (clsMsgHelper.ShowYesNo("Question", "Kết quả đo không đạt, bạn có thực hiện lại không?") == DialogResult.No)
                                {
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    goto RETRY_LABEL;
                                }
                            }
                        }
                    if (Environment.TickCount - StartTime > TimeOut)
                        {
                            clsLogManager.LogReport("TimeOut get BER");
                            m_duDisplayValue = double.NaN;
                            if (clsMsgHelper.ShowYesNo("Question", "Quá thời gian mà chưa đo được, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                m_duDisplayValue = double.NaN;
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                goto RETRY_LABEL;
                            }
                        }
                        RFlvValue = RFlvValue + RfLvStep;
                    }
            }
                catch (System.Exception ex)
                {
                    strMsg = "Exception: " + ex.Message;
                    clsLogManager.LogError("Excution: {0}", ex.ToString());
                    if (clsMsgHelper.ShowYesNo("Question", "Lỗi quá trình đo, bạn có thực hiện lại") == DialogResult.No)
                    {
                        //  m_duDisplayValue = "FAIL";
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        MessageBox.Show("Đảm bảo DUT ở chế độ thu");
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
                    if (SigGenAccess2 != null)
                    {
                        SigGenAccess2.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_sigGen2 != null)
                            SigGenAccess.Connect2Device(deviceinfo_sigGen2);
                    }
                    goto RETRY_LABEL;
                    }
                }
                finally
                {
                    //off siggen
                    if (SigGenService != null)
                    {
                        SigGenService.EnableOutput(false);
                    }
                    if (SigGenService_2nd != null)
                    {
                        SigGenService_2nd.EnableOutput(false);
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
            clsVHCR_Rx_AdjFreSelectivity task = new clsVHCR_Rx_AdjFreSelectivity();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }

        [Category("Test Parameter")]
        [DisplayName("Min Pass (dBm)")]
        [SaveAtt()]
        public double MinPass { set; get; }

        [Category("Test Parameter")]
        [DisplayName("RF level step")]
        [SaveAtt()]
        public double RfLvStep { set; get; }

        [Category("Test Parameter")]
        [DisplayName("Delay after setting (ms)")]
        [SaveAtt()]
        public int DelayAfterSetConfig { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //

        [Category("Test Parameter")]
        [DisplayName("Expected BER")]
        [SaveAtt()]
        public double ExpectedBER { set; get; }
        [Category("Test Parameter")]
        [DisplayName("WaitTime Get BER (ms)")]
        [SaveAtt()]
        public int WaitTimeGetBER { set; get; }

        [Category("Test Parameter")]
        [DisplayName("TimeOut Get BER (ms)")]
        [SaveAtt()]
        public int TimeOutGetBER { set; get; }
        [Category("Test Parameter")]
        [DisplayName("TimeOut Test Task (ms)")]
        [SaveAtt()]
        public int TimeOut { set; get; }

        #region Signal generator
        [Category("Signal generator")]
        [DisplayName("RF level 1st")]
        [SaveAtt()]
        public double RFLevel_1st { set; get; }

        [Category("Signal generator")]
        [DisplayName("RF Output SigGen 1st")]
        [SaveAtt()]
        public OutputPort RFOutport_1st { set; get; }
        [Category("Signal generator")]
        [DisplayName("RF level unit")]
        [SaveAtt()]
        public PowerUnit Unit { set; get; }

        [Category("Signal generator_E8267D")]
        [DisplayName("Wave Form")]
        [SaveAtt()]
        public string WaveForm { set; get; }
        [Category("Signal generator")]
        [DisplayName("Sample Clock (MHz)")]
        [SaveAtt()]
        public double sampleClock { set; get; }
        [Category("Signal Generator_SMW200A")]
        [DisplayName("ARB file name")]
        [SaveAtt()]
        public string ARBFileName { set; get; }

        [Category("Signal generator")]
        [DisplayName("FR Frequency 1st (GHz)")]
        [SaveAtt()]
        public double RFFreq_1st { set; get; }

        [Category("Signal generator")]
        [DisplayName("RF level 2nd")]
        [SaveAtt()]
        public double RFLevel_2nd { set; get; }
        [Category("Signal generator")]
        [DisplayName("RF Output SigGen 2nd")]
        [SaveAtt()]
        public OutputPort RFOutport_2nd { set; get; }

        [Category("Signal generator")]
        [DisplayName("FR Frequency 2nd (GHz)")]
        [SaveAtt()]
        public double RFFreq_2nd { set; get; }
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
        [Category("Test Parameter")]
        [DisplayName("Cable Loss_EXAttenuator Sig Gen 1 (dB)")]
        [SaveAtt()]
        public double CableLoss_1 { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Cable Loss_EXAttenuator Sig Gen 2 (dB)")]
        [SaveAtt()]
        public double CableLoss_2 { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT Parameter(ms)")]
        [SaveAtt()]
        public int delayDUTPara { set; get; }
        #endregion
    }
}
