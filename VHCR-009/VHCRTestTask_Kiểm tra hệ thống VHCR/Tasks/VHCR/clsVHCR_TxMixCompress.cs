using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CFTWinAppCore.Task;
using CFTWinAppCore.Sequences;
using CFTWinAppCore.DataLog;
using System.ComponentModel;
using LogLibrary;
using System.Threading;
using GeneralTool;
using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.Generic;
using CFTWinAppCore.Helper;
using System.Windows.Forms;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.DeviceManager.DUTIOService;
using VHCRTestTask.TypeConvert;
using CFTSeqManager.DUT;
using Option;

namespace VHCRTestTask.Tasks.Compress
{
    [clsTaskMetaInfo("VHCR - TX Nén Tạp", "clsVHCR_TxMixCompress", true)]
    public class clsVHCR_TxMixCompress : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private double m_duDisplayValue = double.MinValue;
        clsFreqTypeConv FreqTypeConv = new clsFreqTypeConv();
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        // private bool fast_result = false;

        public clsVHCR_TxMixCompress()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - TX Nén tạp";

            CountNextPeak = 10;
            FreqOffset = 30; //Hz
            MinPass = 40;
            DelayAfterSetConfig = 1000;
            Result_unit = "dB";

            DutFreqRx = 10.16;
            DutFreqTx = 10.02;
            Dutpower = VHCR_Power.LOW;
            DutMode = VHCR_Mod.FIX_QAM64;
            DutWF = VHCR_Wareform.FIX;
            DutBW = VHCR_Bandwidth.FIX_5M;

            StartFreq = 10; //GHz
            StopFreq = 10.2; //GHz
            RefLevel = 40; // dBm
            RefLevelOffset = 30; //dB
            Atten = 10;
            Rbw = 200; //kHz            
            VBW = 100;
            Swpoint = 10000;
            delayDUTPower = 6000;
            delayDUTSetCW = 10000;
            delayDUTCHPara = 6000;

        }
        [Category("#General Option")]
        [Browsable(true)]
        public string Name
        {
            get { return "clsVHCR_TxMixCompress"; }
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
            //if (m_duDisplayValue < 1)
            //    return string.Format("{0}", m_duDisplayValue.ToString("0.##"));
            //else
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
            return $"{DutMode}, {DutBW} MHz, {DutFreqTx} GHz, Power: {Dutpower}";
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
            ISpecDeviceN9020B SpecService = null;
            IDutVHCR VHCRService = null;
            double Curr_Level = double.MinValue;
            string strMsg = null;
            bool bOK = false;

            double P1 = 0; // Công suất đỉnh
            double P2 = 0; // Công suất tạp
            double P2_left = 0; // Công suất tạp bên trái
            double P2_right = 0; // Công suất tạp bên phải
            double Freq = 0;
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_VHCR = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_SPEC_DEV = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
            IAccessDeviceService SpecDevAccess = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as IAccessDeviceService;
            while (!bOK)
            {
                try
                {
                    RETRY_LABEL:
                    m_TraceService.WriteLine(" Test Task: " + Description + "\n");
                    bOK = false;
                    P1 = 0;
                    P2 = 0;
                    P2_left = 0; 
                    P2_right = 0; 
                    Freq = 0;

                    if (DevManager.IsDevEnable(SysDevType.SPEC_DEV.ToString()) && DevManager.IsDevEnable(SysDevType.DUT.ToString()))
                    {
                        SpecService = DevManager.GetDevService(SysDevType.SPEC_DEV.ToString()) as ISpecDeviceN9020B;
                        VHCRService = DevManager.GetDevService(SysDevType.DUT.ToString()) as IDutVHCR;
                        if(SpecService==null)
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
                        if(VHCRService==null)
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
                        // Thiết lập máy phân tích phổ 
                        if(!SpecService.ModePreset()) throw new System.IO.IOException();
                        if(!SpecService.SetupForMeasurement("SAN")) throw new System.IO.IOException();
                        if(!SpecService.SetStartFreq(StartFreq, FreqUnit.GHz)) throw new System.IO.IOException();
                        if(!SpecService.SetStopFreq(StopFreq, FreqUnit.GHz)) throw new System.IO.IOException();
                        if(!SpecService.SetSpecAttenuation(Atten)) throw new System.IO.IOException();
                        if(!SpecService.SetRefLevelOffset(RefLevelOffset)) throw new System.IO.IOException();
                        if(!SpecService.SetReferentLevel(RefLevel)) throw new System.IO.IOException(); // Đặt Ref level                      
                        if(!SpecService.SetSweepPoint(Swpoint)) throw new System.IO.IOException();
                        if(!SpecService.SetResolutionBandwidth(Rbw, FreqUnit.kHz)) throw new System.IO.IOException(); // Đăt độ phân giải băng thông
                        if(!SpecService.SetVBW(VBW, FreqUnit.kHz)) throw new System.IO.IOException();
                        if(!SpecService.Set_TraceMode(TraceMode.WRITe)) throw new System.IO.IOException();

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
                        // Bắt đầu đo
                        if(bOK)
                        {
                            bOK = false;
                            // Đọc công suất đỉnh
                            if(!SpecService.CalcMarkerMax(1)) throw new System.IO.IOException();
                            Thread.Sleep(300);
                            P1 = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Level);
                            clsLogManager.LogReport("PeakLevel at {0} GHz = {1} dBm", DutFreqTx, P1);

                            // Đọc công suất tạp                  
                            if(!SpecService.SetSpecMeasurementMode(MeasurementMode.Single)) throw new System.IO.IOException();
                            Thread.Sleep(DelayAfterSetConfig);
                            if(!CalNextMarker(SpecService, 1, SpecService.GetValueAtIndexMaker(1, MarkerValueType.Freq), SearchDirection.None)) throw new System.IO.IOException();
                            Thread.Sleep(500);
                            P2_left = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Level);
                            P2_right = double.MinValue;

                            // OFF VHCR CW
                            //if (VHCRService != null)
                            //{
                            //    VHCRService.switchEnalbeBER();
                            //    clsLogManager.LogError("VHCR switch BER (OFF TX)");
                            //}

                            P2 = Math.Max(P2_left, P2_right);
                            clsLogManager.LogReport("P2 = {0} dBm", P2);

                            m_duDisplayValue = Math.Round(P1 - P2, 2);
                            clsLogManager.LogReport("Result = {0} dB", m_duDisplayValue);
                            if (m_duDisplayValue > MinPass)
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
                        bOK = false;
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
                    // OFF VHCR CW
                    //if (VHCRService != null)
                    //{
                    //    VHCRService.switchEnalbeBER();
                    //    clsLogManager.LogError("VHCR switch BER (OFF TX)");
                    //}
                }

                if (!bOK)
                {
                    clsLogManager.LogError(strMsg);
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

        private bool CalNextMarker(ISpecDevice dev, int index, double RefFreq, SearchDirection type)
        {
            try
            {
                bool bOK = false;
                double CurrFreq = double.MinValue;
                for (int i = 0; i < CountNextPeak; i++)
                {
                    dev.CalcNextMarkerMax(index, type); Thread.Sleep(200);
                    CurrFreq = dev.GetValueAtIndexMaker(1, MarkerValueType.Freq); // Hz
                    if ((CurrFreq < RefFreq + FreqOffset) && (CurrFreq > RefFreq - FreqOffset))
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                    else
                    {
                        bOK = true;
                        break;
                    }
                }
                return bOK;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("CalNextMarker() catched an exception: " + ec.Message);
                return false;
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
            clsVHCR_TxMixCompress task = new clsVHCR_TxMixCompress();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }

        private double GetPower_FromSpecDev(ISpecDevice dev, double Freq, int Mescnt)
        {
            try
            {
                bool bOK = false;
                double[] duResult = new double[Mescnt];
                for (int i = 0; i < Mescnt; i++)
                {
                    bOK = dev.CalcMarkerAtFre(Freq, FreqUnit.MHz, 1);
                    duResult[i] = dev.GetValueAtIndexMaker(1, MarkerValueType.Level);
                    Thread.Sleep(200);
                }
                return duResult.Average();
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        #region Test Parameter
        [Category("Test Parameter")]
        [DisplayName("Min Pass")]
        [SaveAtt()]
        public double MinPass { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after setting (ms)")]
        [SaveAtt()]
        public int DelayAfterSetConfig { set; get; }

        [Category("Test Parameter")]
        [DisplayName("Result unit")]
        [SaveAtt()]
        public string Result_unit { set; get; }

        [Category("Test Parameter")]
        [DisplayName("Số lần next peak tối đa")]
        [SaveAtt()]
        [Browsable(false)]
        public int CountNextPeak { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Frequency error max (Hz)")]
        [SaveAtt()]
        public double FreqOffset { set; get; }
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
        #endregion
        #region Spectrum analyzer
        [Category("Spectrum analyzer")]
        [DisplayName("Start Frequence (GHz)")]
        [SaveAtt()]
        public double StartFreq { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Stop Frequency (GHz)")]
        [SaveAtt()]
        public double StopFreq { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("RbW (kHz)")]
        [SaveAtt()]
        public double Rbw { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Sweep Point")]
        [SaveAtt()]
        public int Swpoint { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("VBW (kHz)")]
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
        [DisplayName("Attenuator(dB)")]
        [SaveAtt()]
        public double Atten { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT power")]
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
