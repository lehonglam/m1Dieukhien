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
using CFTSeqManager.DUT;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.DeviceManager.DUTIOService;
using Option;
using VHCRTestTask;

namespace WBHFTestTask.Tasks.VHCR
{
    [clsTaskMetaInfo("VHCR - Tx - EVM", "clsVHCR_Tx_EVM", true)]
    class clsVHCR_Tx_EVM : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private double m_duDisplayValue = double.MinValue;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        clsFreqTypeConv FreqTypeConv = new clsFreqTypeConv();

        public clsVHCR_Tx_EVM()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - TX - EVM";
            MaxPass = 8;
            MinPass = 0;
            MesCnt = 3;
            DelayAfterSetConfig = 1000;
            SPAN = 40;
            Range = 0;
            DutFreqRx = 2.025;
            DutFreqTx = 2.025;
            Dutpower = VHCR_Power.LOW;
            DutMode = VHCR_Mod.FIX_QAM64;
            DutWF = VHCR_Wareform.FIX;
            DutBW = VHCR_Bandwidth.FIX_5M;
            Result_unit = "%";
            delayDUTPower = 6000;
            delayDUTSetCW = 10000;
            CenterFreq = 4.55; //GHz
            MeasType = MeasurementType.DDEMod;
            DDFormat = DigitalDemodFormat.Qpsk;
            ResultLenght = 157;
            SymbolRate = 30.72;
            PointPerSymbol = 5;
            typeFilter = VSATypeFilter.RootRaisedCosine;
            typeFilterRef = VSATypeFilterRef.RaisedCosine;
            typeFilterABT = 0.3;
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
            get { return "clsVHCR_Tx_EVM"; }
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
            ISpecDeviceN9020B_Vsa SpecService = null;
            bool bOK = false;
            double EVMValue = double.MinValue;
            string strMsg = null;
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_VHCR = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_SPEC_DEV = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
            IAccessDeviceService SpecDevAccess = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as IAccessDeviceService;
            while (!bOK)
            {
                m_TraceService.WriteLine("Begin Test Task: " + m_strDescription + string.Format("[{0}]", GetAddTaskDescription()));
                RETRY_LABEL:
                bOK = false;
                EVMValue = double.MinValue;
                try
                {
                    
                    if (DevManager.IsDevEnable(SysDevType.SPEC_DEV_Vsa.ToString()) && DevManager.IsDevEnable(SysDevType.DUT.ToString()))
                    {
                        VHCRService = DevManager.GetDevService(SysDevType.DUT.ToString()) as IDutVHCR;
                        IDeviceInfor deviceinfo= DevManager.GetDevRunTimeInfo(SysDevType.SPEC_DEV_Vsa.ToString()) as IDeviceInfor;
                        if(deviceinfo==null)
                        {
                            clsLogManager.LogError("Device SPEC_DEV_Vsa is disable");
                            if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                goto RETRY_LABEL;
                            }
                        }
                        IAccessDeviceService dev = DevManager.GetDevService(SysDevType.SPEC_DEV_Vsa.ToString()) as IAccessDeviceService;
                        if(dev==null)
                        {
                            clsLogManager.LogError("Device SPEC_DEV_Vsa is disable");
                            if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                goto RETRY_LABEL;
                            }
                        }
                        dev.Connect2Device(deviceinfo);
                        SpecService = DevManager.GetDevService(SysDevType.SPEC_DEV_Vsa.ToString()) as ISpecDeviceN9020B_Vsa;
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
                        clsLogManager.LogWarning("Set up VSA analyzer ...");
                        if(!SpecService.PresetVsa()) throw new System.IO.IOException();
                        if(!SpecService.SetVsaAutoCal(false))throw new System.IO.IOException();
                        if(!SpecService.SetVsaInputDataHW()) throw new System.IO.IOException();
                        if(!SpecService.SetupForMeasurement()) throw new System.IO.IOException();
                        if(!SpecService.SetVsaMeasurementType(MeasType.ToString())) throw new System.IO.IOException();
                        if(!SpecService.SetVSALayout(2, 2)) throw new System.IO.IOException();
                        if(!SpecService.SetCenterFreq(CenterFreq, FreqUnit.GHz)) throw new System.IO.IOException(); // Đặt tần số trung tâm
                        if(!SpecService.SetVsaSPAN(SPAN, FreqUnit.MHz)) throw new System.IO.IOException();
                        if(!SpecService.SetVsaRng(Range)) throw new System.IO.IOException();
                        if(!SpecService.SetVsaDdemSymbolRate(SymbolRate * 1e6)) throw new System.IO.IOException();
                        if(!SpecService.SetVsaDdemFormat(DDFormat.ToString())) throw new System.IO.IOException();
                        if(!SpecService.SetVsaDdemResultLenght(ResultLenght)) throw new System.IO.IOException();
                        if(!SpecService.SetVsaDdemPointPerSymbol(PointPerSymbol)) throw new System.IO.IOException();
                        if(!SpecService.SetVsaDdemFilter(typeFilter)) throw new System.IO.IOException();
                        if(!SpecService.SetVsaDdemFilterRef(typeFilterRef)) throw new System.IO.IOException();
                        if(!SpecService.SetVsaDdemFilterABT(typeFilterABT)) throw new System.IO.IOException();
                        clsLogManager.LogWarning("Set up VSA analyzer done !");

                        Thread.Sleep(DelayAfterSetConfig);

                        // Đặt tham số máy: 
                        if (VHCRService.SetParamChanel(DutWF, DutMode, DutBW, (ulong)(DutFreqTx * 1000000), (ulong)(DutFreqRx * 1000000), 0, 0))
                        {
                            Thread.Sleep(delayDUTCHPara);
                            if (VHCRService.SetPower(Dutpower))
                            {
                                Thread.Sleep(delayDUTPower);
                                //bOK = true;
                                //2 set start CW --> mode EVM measure
                                if (VHCRService.StartCW())
                                {
                                    Thread.Sleep(DelayAfterSetConfig);
                                    //bOK = true;
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
                        if(!SpecService.SetVsaAutoRng()) throw new System.IO.IOException();
                        Thread.Sleep(DelayAfterSetConfig);
                        // Bắt đầu đo
                        if(bOK)
                        {
                            bOK = false;
                            double[] P1 = new double[MesCnt];
                            // Đọc giá trị EVM
                            for (int nTry = 0; nTry < MesCnt; nTry++)
                            {
                                P1[nTry] = SpecService.GetVsaDdemResultEVM();
                                Thread.Sleep(100);
                            }

                            EVMValue = P1.Average();

                            // Chuyển thu VHCR
                            //VHCRService.switchEnalbeBER();
                            //clsLogManager.LogError("VHCR switch BER (OFF TX)");

                            clsLogManager.LogWarning($"EVM Value: {EVMValue} %");

                            m_duDisplayValue = Math.Round(EVMValue, 3);

                            if ((EVMValue>=MinPass)&(EVMValue<=MaxPass))
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
                        clsLogManager.LogError("Devices are disable!");
                        break;
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

                if(!bOK)
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

            if(bOK)
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
            clsVHCR_Tx_EVM task = new clsVHCR_Tx_EVM();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        #region Test Parameter
        [Category("Test Parameter")]
        [DisplayName("Max Pass")]
        [SaveAtt()]
        public double MaxPass { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Min Pass")]
        [SaveAtt()]
        public double MinPass { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after setting (ms)")]
        [SaveAtt()]
        public int DelayAfterSetConfig { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Measurement counter")]
        [SaveAtt()]
        public int MesCnt { set; get; } // Số lần đo
        //Powerunit
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

        #endregion
        #region Spectrum analyzer

        [Category("Spectrum analyzer")]
        [DisplayName("Center Frequency (GHz)")]
        [SaveAtt()]
        public double CenterFreq { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Symbol Rate (MHz)")]
        [SaveAtt()]
        public double SymbolRate { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Measurement Type")]
        [SaveAtt()]
        public MeasurementType MeasType { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Digital Demod Format")]
        [SaveAtt()]
        public DigitalDemodFormat DDFormat { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Result Lenght")]
        [SaveAtt()]
        public double ResultLenght { set; get; }
        [Category("Spectrum analyzer")]
        [DisplayName("SPAN(MHz)")]
        [SaveAtt()]
        public double SPAN { set; get; }
        [Category("Spectrum analyzer")]
        [DisplayName("Range (dBm)")]
        [SaveAtt()]
        public double Range { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("Point Per Symbol")]
        [SaveAtt()]
        public double PointPerSymbol { set; get; }
        [Category("Spectrum analyzer")]
        [DisplayName("Type Filter")]
        [SaveAtt()]
        public VSATypeFilter typeFilter { set; get; }
        [Category("Spectrum analyzer")]
        [DisplayName("Type Filter Reference")]
        [SaveAtt()]
        public VSATypeFilterRef typeFilterRef { set; get; }
        [Category("Spectrum analyzer")]
        [DisplayName("Type Filter Alpha/BT")]
        [SaveAtt()]
        public double typeFilterABT { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT power(ms)")]
        [SaveAtt()]
        public int delayDUTPower { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT EVM mode(ms)")]
        [SaveAtt()]
        public int delayDUTSetCW { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT Channel Parameter(ms)")]
        [SaveAtt()]
        public int delayDUTCHPara { set; get; }
        #endregion
    }
}
