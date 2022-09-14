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
using System.Windows.Forms;
using VHCRTestTask.TypeConvert;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.DeviceManager.DUTIOService;
using Option;

//using System.Windows.Forms;

namespace VHCRTestTask.Tasks.VHCR
{
    [clsTaskMetaInfo("VHCR - TX Sai số tần số", "clsVHCR_Tx_ErrorFreq", true)]
    public class clsVHCR_Tx_ErrorFreq : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private double m_duDisplayValue = double.MinValue;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        clsFreqTypeConv FreqTypeConv = new clsFreqTypeConv();

        public clsVHCR_Tx_ErrorFreq()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - TX Sai số tần số lớn nhất";
            MaxPass = 1;
            MinPass = -1;
            Result_unit = "ppm";
            MesCnt = 3;

            DelayAfterSetConfig = 200;
            delayDUTPower = 6000;
            delayDUTSetCW = 10000;
            DutFreqRx = 2.025;
            DutFreqTx = 2.025;
            Dutpower = VHCR_Power.LOW;
            DutMode = VHCR_Mod.FIX_QAM64;
            DutWF = VHCR_Wareform.FIX;
            DutBW = VHCR_Bandwidth.FIX_5M;

            CenterFreq = 4.55; //GHz
            RefLevel = 40; // dBm
            RefLevelOffset = 30; //dB
            Rbw = 200; //kHz
            Span = 100; //kHz
            VBW = 100;
            Swpoint = 10000;
            delayDUTCHPara = 6000;
        }
        [Category("#General Option")]
        [Browsable(false)]
        public string Name
        {
            get { return "clsVHCR_Tx_ErrorFreq"; }
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
            return $"{MaxPass.ToString("F03")} {Result_unit}";
        }
        public string GetDisplayMinValue()
        {
            return $"{MinPass.ToString("F03")} {Result_unit}";
        }
        public string GetAddTaskDescription()
        {
            return $"{DutMode}, {DutBW} MHz, {DutFreqTx} GHz, Power: {Dutpower}";
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
            return $"{MinPass.ToString("F03")}";
        }

        public string GetUnit()
        {
            return $"{Result_unit}";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            IDutVHCR VHCRService = null;
            ISpecDeviceN9020B SpecService = null;
            IFreqCounter FreqDev = null;
            string strMsg = null;

            bool bOK = false;
            double[] ErrFreq = new double[MesCnt];
            double PeakFreq = double.MinValue;
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_VHCR = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_SPEC_DEV = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
            IAccessDeviceService SpecDevAccess = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as IAccessDeviceService;
            while (!bOK)
            {
                try
                {
                    
                    m_TraceService.WriteLine("Begin Test Task: " + m_strDescription + string.Format("[{0}]", GetAddTaskDescription()));
                    RETRY_LABEL:
                    bOK = false;
                    if (DevManager.IsDevEnable(SysDevType.SPEC_DEV.ToString()) || DevManager.IsDevEnable(SysDevType.ALL_MESS.ToString()) || DevManager.IsDevEnable(SysDevType.FREQ_CNT.ToString()))
                    {

                        SpecService = DevManager.GetDevService(SysDevType.SPEC_DEV.ToString()) as ISpecDeviceN9020B;
                        if (SpecService == null)
                        {
                            clsLogManager.LogError("Can not get device service: ISpecDeviceN9020B");
                            if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                goto RETRY_LABEL;
                            }
                        }
                        // Thiết lập máy phân tích phổ
                        clsLogManager.LogWarning("Set up Spectrum analyzer ...");

                        if(!SpecService.ModePreset()) throw new System.IO.IOException();
                        if(!SpecService.SetCenterFreq(CenterFreq, FreqUnit.GHz)) throw new System.IO.IOException(); // Đặt tần số trung tâm
                        if(!SpecService.SetRefLevelOffset(RefLevelOffset)) throw new System.IO.IOException();
                        if(!SpecService.SetReferentLevel(RefLevel)) throw new System.IO.IOException(); // Đặt Ref level
                        //SpecService.SetRefLevelOffset(RefLevelOffset);
                        if(!SpecService.SetSweepPoint(Swpoint)) throw new System.IO.IOException();
                        if(!SpecService.SetResolutionBandwidth(Rbw, FreqUnit.Hz)) throw new System.IO.IOException(); // Đăt độ phân giải băng thông
                        if(!SpecService.SetVBW(VBW, FreqUnit.Hz)) throw new System.IO.IOException();
                        if(!SpecService.SetSpan(Span, FreqUnit.kHz)) throw new System.IO.IOException(); // Dặt độ rộng phổ
                        if(!SpecService.Set_TraceMode(TraceMode.WRITe)) throw new System.IO.IOException();
                        clsLogManager.LogWarning("Set up Spectrum analyzer done !");


                        if (DevManager.IsDevEnable(SysDevType.ALL_MESS.ToString()) || DevManager.IsDevEnable(SysDevType.FREQ_CNT.ToString()))
                        {
                            FreqDev = DevManager.GetDevService(SysDevType.FREQ_CNT.ToString()) as IFreqCounter;
                            if (FreqDev == null)
                            {
                                clsLogManager.LogError("Can not get device service: IFreqCounter");
                                if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                                {
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    goto RETRY_LABEL;
                                }
                            }
                            FreqDev.SetDemodulatioType(DemodulationType.USB, DutFreqTx, FreqUnit.MHz); // điều chế FM, đo tại tần số DUTFreq
                        }

                        VHCRService = DevManager.GetDevService(SysDevType.DUT.ToString()) as IDutVHCR;
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
                        Thread.Sleep(DelayAfterSetConfig);

                        // Đặt tham số máy: 
                        if (VHCRService.SetParamChanel(DutWF, DutMode, DutBW, (ulong)(DutFreqTx * 1000000), (ulong)(DutFreqRx * 1000000), 0, 0))
                        {
                            Thread.Sleep(delayDUTCHPara);
                            if (VHCRService.SetPower(Dutpower))
                            {
                                Thread.Sleep(delayDUTPower);
                                if (VHCRService.StartCW())
                                {
                                    Thread.Sleep(delayDUTSetCW);
                                    bOK = true;
                                }
                                else
                                {
                                    strMsg = $"StartCW: => FAIL";
                                    bOK = false;
                                }
                            }
                            else
                            {
                                strMsg = $"SetPower: {Dutpower} => FAIL";
                                bOK = false;
                            }
                        }
                        else
                        {
                            strMsg = $"SetParamChanel: {DutWF}, {DutMode}, {DutBW}, {DutFreqTx}GHz, {DutFreqRx}GHz => FAIL";
                            bOK = false;
                        }

                        if(bOK)
                        {
                            // Tính sai số tần số
                            if (DevManager.IsDevEnable(SysDevType.SPEC_DEV.ToString()))
                            {
                                for (int i = 0; i < MesCnt; i++)
                                {
                                    SpecService.CalcMarkerMax(1);
                                    Thread.Sleep(200);
                                    PeakFreq = SpecService.GetValueAtMaker(MarkerValueType.Freq);
                                    clsLogManager.LogWarning("Max peak Freq = {0} Hz",PeakFreq);
                                    ErrFreq[i] =PeakFreq - DutFreqTx * 1e9;
                                    clsLogManager.LogWarning("ErrFreq = {0} Hz", ErrFreq[i]);
                                    Thread.Sleep(100);
                                }
                            }
                            
                            m_duDisplayValue = Math.Round((ErrFreq.Average() * 1e6)/(DutFreqTx*1e9), 3);
                            clsLogManager.LogWarning("PPM = {0}", m_duDisplayValue);
                            clsLogManager.LogReport($"Frequency error = {m_duDisplayValue} {Result_unit}");
                            if (MinPass <= m_duDisplayValue && MaxPass >= m_duDisplayValue)
                            {
                                bOK = true;
                            }
                            else
                            {
                                strMsg = $"Kết quả {m_duDisplayValue:F3} không đạt";
                                bOK = false;
                            }
                        }    
                       
                    }
                    else
                    {
                        strMsg = "Devices are unvailable!";
                    }

                }
                catch (System.Exception ex)
                {
                    clsLogManager.LogError("Excution: {0}", ex.ToString());
                    strMsg = "Exception: " + ex.Message;
                    bOK = false;
                    if (DUT != null)
                    {
                        DUT.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_VHCR != null)
                        {
                            VHCRService.Connect2Device(deviceinfo_VHCR);
                        }
                    }
                    if (SpecDevAccess != null)
                    {
                        SpecDevAccess.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_SPEC_DEV != null)
                        {
                            SpecDevAccess.Connect2Device(deviceinfo_SPEC_DEV);
                        }
                    }

                }
                finally
                {
                    // OFF PTT
                    //if (VHCRService != null)
                    //{
                        //VHCRService.switchEnalbeBER();
                        //clsLogManager.LogError("VHCR switch BER (OFF TX)");
                    //}
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
                        Thread.Sleep(200);
                        if (deviceinfo_VHCR != null)
                        {
                            VHCRService.Connect2Device(deviceinfo_VHCR);
                        }
                    }
                    if (SpecDevAccess != null)
                    {
                        SpecDevAccess.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_SPEC_DEV != null)
                        {
                            SpecDevAccess.Connect2Device(deviceinfo_SPEC_DEV);
                        }
                    }
                }
            }

            //Return Result
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
            clsVHCR_Tx_ErrorFreq task = new clsVHCR_Tx_ErrorFreq();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }

        [Category("Test Parameter")]
        [DisplayName("Max Pass")]
        [SaveAtt()]
        public double MaxPass { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Min Pass")]
        [SaveAtt()]
        public double MinPass { set; get; }

        [Category("Test Parameter")]
        [DisplayName("Result unit")]
        [SaveAtt()]
        public string Result_unit { set; get; }

        [Category("Test Parameter")]
        [DisplayName("Delay after setting (ms)")]
        [SaveAtt()]
        public int DelayAfterSetConfig { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Measurement counter")]
        [SaveAtt()]
        public int MesCnt { set; get; } // Số lần đo

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
        [DisplayName("DUT Power (W)")]
        [SaveAtt()]
        public VHCR_Power Dutpower { set; get; }

        [Category("DUT")]
        [DisplayName("DUT Bandwidth (W)")]
        [SaveAtt()]
        public VHCR_Bandwidth DutBW { set; get; }

        [Category("DUT")]
        [DisplayName("DUT Wareform")]
        [SaveAtt()]
        public VHCR_Wareform DutWF { set; get; }

        [Category("DUT")]
        [DisplayName("Set DUT mode")]
        [SaveAtt()]
        public bool bSetDUT { set; get; }
        #endregion

        #region Spectrum analyzer
        [Category("Spectrum analyzer")]
        [DisplayName("Span (kHz)")]
        [SaveAtt()]
        public double Span { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("RBW (Hz)")]
        [SaveAtt()]
        public double Rbw { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Sweep Point")]
        [SaveAtt()]
        public int Swpoint { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("VBW (Hz)")]
        [SaveAtt()]
        public double VBW { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Ref level (dbm)")]
        [SaveAtt()]
        public double RefLevel { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Ref level Offset(dB)")]
        [SaveAtt()]
        public double RefLevelOffset { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Center Frequency (GHz)")]
        [SaveAtt()]
        public double CenterFreq { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT power(ms)")]
        [SaveAtt()]
        public int delayDUTPower { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT CW mode(ms)")]
        [SaveAtt()]
        public int delayDUTSetCW { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT Channel Parameter(ms)")]
        [SaveAtt()]
        public int delayDUTCHPara { set; get; }
        #endregion
    }
}
