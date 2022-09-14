﻿using System;
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
using CFTSeqManager.DUT;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.DeviceManager.DUTIOService;
using Option;

namespace VHCRTestTask.Tasks.VHCR
{
    [clsTaskMetaInfo("VHCR - TX - Ocupiced bandwidth", "clsVHCR_Tx_OcupicedBandwidth", true)]
    class clsVHCR_Tx_OcupicedBandwidth : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private double m_duDisplayValue = double.MinValue;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        clsFreqTypeConv FreqTypeConv = new clsFreqTypeConv();

        public clsVHCR_Tx_OcupicedBandwidth()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - TX - Ocupiced bandwidth";
            MaxPass = 6;
            MinPass = 4;
            Result_unit = "MHz";
            DelayAfterSetConfig = 1000;
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
            VBW = 300;
            SWE = 12;
            Swpoint = 10000;
            PowerBW = 99;
            delayDUTPower = 6000;
            delayDUTCHPara = 6000;
        }
        public enum Powerunit
        {
            dBm,
            W
        }
        [Category("#General Option")]
        [Browsable(false)]
        public string Name
        {
            get { return "clsVHCR_Tx_OcupicedBandwidth"; }
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
            return $"{m_duDisplayValue.ToString(".000")} {Result_unit}";
        }
        public string GetDisplayMaxValue()
        {
            return $"{MaxPass.ToString(".000")} {Result_unit}";
        }
        public string GetDisplayMinValue()
        {
            return $"{MinPass.ToString(".000")} {Result_unit}";
        }
        public string GetAddTaskDescription()
        {
            return $"{DutMode}, {DutBW} MHz, {DutFreqTx} GHz, Power: {Dutpower}";
        }
        public string GetValue()
        {
            return $"{m_duDisplayValue.ToString(".000")}";
        }

        public string GetMaxValue()
        {
            return $"{MaxPass.ToString(".000")}";
        }

        public string GetMinValue()
        {
            return $"{MinPass.ToString(".000")}";
        }

        public string GetUnit()
        {
            return $"{Result_unit}";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            IDutVHCR VHCRService = null;
            ISpecDeviceN9020B SpecService = null;
            string strMsg = null;
            bool bOK = false;
            double OBWValue = double.MinValue;
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_VHCR = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_SPEC_DEV = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
            IAccessDeviceService SpecDevAccess = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as IAccessDeviceService;
            while (!bOK)
            {
                try
                {
                    RETRY_LABEL:
                    m_TraceService.WriteLine("Begin Test Task: " + m_strDescription + string.Format("[{0}]", GetAddTaskDescription()));
                    bOK = false;
                    if (DevManager.IsDevEnable(SysDevType.SPEC_DEV.ToString()) && DevManager.IsDevEnable(SysDevType.DUT.ToString()))
                    {
                        SpecService = DevManager.GetDevService(SysDevType.SPEC_DEV.ToString()) as ISpecDeviceN9020B;
                        VHCRService = DevManager.GetDevService(SysDevType.DUT.ToString()) as IDutVHCR;
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
                        // Thiết lập máy phân tích phổ 
                        clsLogManager.LogWarning("Set up Spectrum analyzer ...");
                        if(!SpecService.ModePreset()) throw new System.IO.IOException();
                        if(!SpecService.SetupForMeasurement("OBWidth")) throw new System.IO.IOException();
                        if(!SpecService.SetCenterFreq(CenterFreq, FreqUnit.GHz)) throw new System.IO.IOException(); // Đặt tần số trung tâm
                        if(!SpecService.SetSpan(Span, FreqUnit.kHz)) throw new System.IO.IOException(); // Dặt độ rộng phổ
                        if(!SpecService.SetVBW(VBW, FreqUnit.kHz)) throw new System.IO.IOException();
                        if(!SpecService.SetResolutionBandwidth(Rbw, FreqUnit.kHz)) throw new System.IO.IOException(); // Đăt độ phân giải băng thông
                        if(!SpecService.SetSWE(SWE, TimeUnit.ms)) throw new System.IO.IOException();
                        if(!SpecService.SetSweepPoint(Swpoint)) throw new System.IO.IOException();
                        if(!SpecService.SetOBWPower(PowerBW)) throw new System.IO.IOException();
                        if(!SpecService.Set_TraceMode(TraceMode.WRITe)) throw new System.IO.IOException();
                        clsLogManager.LogWarning("Set up Spectrum analyzer done !");
                        Thread.Sleep(2000);

                        // Đặt tham số máy: 
                        if (VHCRService.SetParamChanel(DutWF, DutMode, DutBW, (ulong)(DutFreqTx * 1000000), (ulong)(DutFreqRx * 1000000), 0, 0))
                        {
                            Thread.Sleep(delayDUTCHPara);
                            if (VHCRService.SetPower(Dutpower))
                            {
                                Thread.Sleep(delayDUTPower);
                                bOK = true;
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
                        clsLogManager.LogReport("Wait for measurement....");
                        Thread.Sleep(DelayAfterSetConfig);
                        // Bắt đầu đo
                        if (bOK)
                        {
                            bOK = false;
                            //Đọc giá trị băng thông
                            OBWValue = SpecService.GetOBWFreq();

                            if (OBWValue != double.MinValue)
                            {
                                m_duDisplayValue = Math.Round(OBWValue / 1e6, 3);

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
                            else
                            {
                                strMsg = "Can't get OBW Value";
                                bOK = false;
                            }
                        }                            
                    }
                    else
                    {
                        strMsg = "Devices are disable!";
                        bOK = false;
                    }
                }
                catch (System.Exception ex)
                {
                    strMsg = "Exception: " + ex.Message;
                    bOK = false;
                    clsLogManager.LogError("Excution: {0}", ex.ToString());
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
            clsVHCR_Tx_OcupicedBandwidth task = new clsVHCR_Tx_OcupicedBandwidth();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        #region Test Parameter
        [Category("Test Parameter")]
        [DisplayName("Max Pass (MHz)")]
        [SaveAtt()]
        public double MaxPass { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Min Pass (MHz)")]
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
        [DisplayName("Sweep time (ms)")]
        [SaveAtt()]
        public double SWE { set; get; }
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

        [Category("Spectrum analyzer")]
        [DisplayName("Power Bandwidth (%)")]
        [SaveAtt()]
        public double PowerBW { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT power(ms)")]
        [SaveAtt()]
        public int delayDUTPower { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT Channel Parameter(ms)")]
        [SaveAtt()]
        public int delayDUTCHPara { set; get; }
        #endregion


    }
}
