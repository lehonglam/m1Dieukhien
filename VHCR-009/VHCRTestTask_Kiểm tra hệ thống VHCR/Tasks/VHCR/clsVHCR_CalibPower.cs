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
using System.Drawing.Design;
using kNN;
using System.IO;
//using System.Windows.Forms;

namespace VHCRTestTask.Tasks.VHCR
{
    [clsTaskMetaInfo("VHCR - calib Công suất", "clsVHCR_CalibPower", true)]
    public class clsVHCR_CalibPower : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private double m_duDisplayValue = double.MinValue;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        clsFreqTypeConv FreqTypeConv = new clsFreqTypeConv();
        Queue<double> queuePower = null;
        StreamWriter writer = null;
        FileStream fs = null;
        StreamWriter writeSamplePointcalib = null;
        FileStream fsSamplePointCalib = null;
        [Browsable(false)]
        public double RFHighPowerValue { get; set; }
        public clsVHCR_CalibPower()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - Calib Công suất";
            MesCnt = 3;
            EXAttenuator = 30; //dB
            DelayAfterSetConfig = 1000;
            Atten = 10;
            DutFreqRx = 2.025;
            DutFreqTx = 2.025;
            Dutpower = VHCR_Power.LOW;
            DutMode = VHCR_Mod.FIX_QAM64;
            DutWF = VHCR_Wareform.FIX;
            DutBW = VHCR_Bandwidth.FIX_5M;
            delayDUTPower = 6000;
            delayDUTSetCW = 10000;
            Result_unit = Powerunit.dBm;
            CenterFreq = 4.55; //GHz
            RefLevel = 40; // dBm
            RefLevelOffset = 30; //dB
            Rbw = 200; //kHz
            VBW = 200; //kHz
            Span = 100; //kHz
            delayDUTCHPara = 6000;
            delayDUTTURNONOFFAPC = 6000;
            delayDUTATT = 1000;
            delayDUTHIGHPWR = 3000;
            TimeOut = 600000;
            NumStep = 5;
            InitAtten = 35;
            AttenStep = 0.5;
            AttenStep2 = 0.1;
            AttenStep3 = 0.01;
            DeltaStep2 = 5;
            DeltaStep3 = 3;
            MinAtten = 5;
            MaxAtten = 35;
            RFPOWLOWMax = 30;
            RFPOWLOWMin = 28;
            RFPOWHIGHMax = 36;
            RFPOWHIGHMin = 34;
            RFHighPowerValue = double.MinValue;
            typecalib = TypeCalib.GetDatatrain;
            kfactor = 1;
            trainDataFile = "";
            LowRFPowerTarget = 29.2;
            HighRFPowerTarget = 35.2;
        }
        public enum Powerunit
        {
            dBm,
            W
        }
        public enum TypeCalib
        {
            KNN,
            Manual,
            GetDatatrain
        }
        [Category("#General Option")]
        [Browsable(false)]
        [SaveAtt()]
        public string Name
        {
            get { return "clsVHCR_CalibPower"; }
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

            return $"{RFPOWLOWMax.ToString("F03")} {Result_unit}";

        }
        public string GetDisplayMinValue()
        {
            return $"{RFPOWLOWMin.ToString("F03")} {Result_unit}";
        }
        public string GetAddTaskDescription()
        {
            return $"{DutFreqTx} GHz";
        }
        public string GetValue()
        {
            return $"{m_duDisplayValue.ToString("F03")}";
        }

        public string GetMaxValue()
        {
            return $"{RFPOWLOWMax.ToString("F03")}";
        }

        public string GetMinValue()
        {
            return $"{RFPOWLOWMin.ToString("F03")}";
        }

        public string GetUnit()
        {
            return $"{Result_unit.ToString()}";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {            
            IDutVHCR VHCRService = null;
            ISpecDeviceN9020B SpecService = null;
            bool bOK = false;
            string strMsg = null;
            string fileName_samplepointcalib = Application.StartupPath + "\\SamplePointCalib.txt";
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_VHCR = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_SPEC_DEV = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor;
            IAccessDeviceService SpecDevAccess = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as IAccessDeviceService;
            try
            {
            RETRY_LABEL:
                m_TraceService.WriteLine("Begin Test Task: " + m_strDescription + string.Format("[{0}]", GetAddTaskDescription()));
                bOK = false;
                queuePower = new Queue<double>(NumStep);
                if (DevManager.IsDevEnable(SysDevType.SPEC_DEV.ToString()) && DevManager.IsDevEnable(SysDevType.DUT.ToString()))
                {
                    VHCRService = DevManager.GetDevService(SysDevType.DUT.ToString()) as IDutVHCR;
                    SpecService = DevManager.GetDevService(SysDevType.SPEC_DEV.ToString()) as ISpecDeviceN9020B;
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
                    if (!SpecService.ModePreset()) throw new System.IO.IOException();
                    if (!SpecService.SetupForMeasurement("SAN")) throw new System.IO.IOException();
                    //, đặt băng thông kênh 20MHz                    
                    if (!SpecService.SetCenterFreq(CenterFreq, FreqUnit.GHz)) throw new System.IO.IOException(); // Đặt tần số trung tâm
                    if (!SpecService.SetSpecAttenuation(Atten)) throw new System.IO.IOException();
                    if (!SpecService.SetRefLevelOffset(RefLevelOffset)) throw new System.IO.IOException();
                    if (!SpecService.SetReferentLevel(RefLevel)) throw new System.IO.IOException(); // Đặt Ref level
                                                                                                    //SpecService.SetRefLevelOffset(RefLevelOffset);
                    if (!SpecService.SetResolutionBandwidth(Rbw, FreqUnit.kHz)) throw new System.IO.IOException(); // Đăt độ phân giải băng thông
                    if (!SpecService.SetVBW(VBW, FreqUnit.kHz)) throw new System.IO.IOException();
                    if (!SpecService.SetSpan(Span, FreqUnit.kHz)) throw new System.IO.IOException(); // Đặt độ rộng phổ
                    if (!SpecService.Set_TraceMode(TraceMode.WRITe)) throw new System.IO.IOException();
                    clsLogManager.LogWarning("Set up Spectrum analyzer done !");

                    Thread.Sleep(DelayAfterSetConfig);
                    if (InitAtten > MaxAtten)
                    {
                        strMsg = $"Initial Attenuation Value > Max Attenuation Value=> FAIL";
                        clsLogManager.LogError(strMsg);
                        if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", Cài đặt lại tham số và chạy lại?") == DialogResult.No)
                        {
                            m_duDisplayValue = double.MinValue;
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                            goto RETRY_LABEL;
                        }
                    }
                    else if (InitAtten < MinAtten)
                    {
                        strMsg = $"Initial Attenuation Value < Min Attenuation Value=> FAIL";
                        clsLogManager.LogError(strMsg);
                        if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", Cài đặt lại tham số và chạy lại?") == DialogResult.No)
                        {
                            m_duDisplayValue = double.MinValue;
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                            goto RETRY_LABEL;
                        }
                    }
                    if (DeltaStep3 >= DeltaStep2)
                    {
                        strMsg = $"Delta Step 3>=Delta Step 2=> FAIL";
                        clsLogManager.LogError(strMsg);
                        if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", Cài đặt lại tham số và chạy lại?") == DialogResult.No)
                        {
                            m_duDisplayValue = double.MinValue;
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                            goto RETRY_LABEL;
                        }
                    }
                    // Đặt tham số máy: 
                    if (VHCRService.AutomaticPowerControl(false))
                    {
                        Thread.Sleep(delayDUTTURNONOFFAPC);
                        strMsg = $"Turn off automatic power control: OK";
                        clsLogManager.LogReport(strMsg);
                        //set attenuation
                        if (VHCRService.SetAttenuation(Math.Round(InitAtten, 2)))
                        {
                            Thread.Sleep(delayDUTATT);
                            strMsg = $"Set Attenuation Value={Math.Round(InitAtten, 2)} dB";
                            clsLogManager.LogReport(strMsg);
                        }
                        else
                        {
                            strMsg = $"Set Attenuaton Value: FAIL";
                            clsLogManager.LogError(strMsg);
                            if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                            {
                                m_duDisplayValue = double.MinValue;
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                goto RETRY_LABEL;
                            }
                        }

                        if (VHCRService.SetParamChanel(DutWF, DutMode, DutBW, (ulong)(DutFreqTx * 1000000), (ulong)(DutFreqRx * 1000000), 0, 0))
                        {
                            Thread.Sleep(delayDUTCHPara);
                            strMsg = $"SetParamChanel: {DutWF}, {DutMode}, {DutBW}, {DutFreqTx}GHz, {DutFreqRx}GHz => OK";
                            clsLogManager.LogReport(strMsg);
                            if (VHCRService.StartCW())
                            {
                                Thread.Sleep(delayDUTSetCW);
                                strMsg = $"Set CW: OK";
                                clsLogManager.LogReport(strMsg);
                            }
                            else
                            {
                                strMsg = $"StartCW: => FAIL";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    goto RETRY_LABEL;
                                }
                            }
                        }
                        else
                        {
                            strMsg = $"SetParamChanel: {DutWF}, {DutMode}, {DutBW}, {DutFreqTx}GHz, {DutFreqRx}GHz => FAIL";
                            clsLogManager.LogError(strMsg);
                            if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                            {
                                m_duDisplayValue = double.MinValue;
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                goto RETRY_LABEL;
                            }
                        }
                    }
                    else
                    {
                        strMsg = $"Turn OFF Automatic Power Control: FAIL";
                        clsLogManager.LogError(strMsg);
                        if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                        {
                            m_duDisplayValue = double.MinValue;
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                            goto RETRY_LABEL;
                        }
                    }
                    if (VHCRService.SetAttenuation(InitAtten))
                    {
                        Thread.Sleep(delayDUTATT);
                        strMsg = $"Set Initial Attenuation Value={InitAtten} dB";
                        clsLogManager.LogReport(strMsg);
                    }
                    else
                    {
                        strMsg = $"Set Initial Attenuaton Value: FAIL";
                        clsLogManager.LogError(strMsg);
                        if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                        {
                            m_duDisplayValue = double.MinValue;
                            return TaskResult.FAIL;
                        }
                        else
                        {
                            reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                            goto RETRY_LABEL;
                        }
                    }
                    double testAtten = MaxAtten;
                    // Bắt đầu calib
                    double CurrentAtten = InitAtten;
                    double RFPOWValue = double.MinValue;
                    double RFPOWValue_old = double.MinValue;
                    double[] P1 = new double[MesCnt];
                    double Freq = 0; // Tần số tại đỉnh phổ
                    bool isCalibLowPower = false;
                    bool iscalibHighPower = false;
                    int StartTime = Environment.TickCount;
                    int timesEnqueue = 0;
                    queuePower.Clear();
                    if (typecalib == TypeCalib.GetDatatrain)
                    {
                        //string fileName =Application.StartupPath+"\\SamplePointCalib.txt";
                        string fileName = trainDataFile;
                        if (!File.Exists(fileName))
                        {
                            fs = File.Open(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                            writer = new StreamWriter(fs);
                        }
                        else
                        {
                            fs = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                            writer = new StreamWriter(fs);
                        }
                            
                        do
                        {
                            if (VHCRService.SetAttenuation(Math.Round(testAtten, 2)))
                            {
                                Thread.Sleep(delayDUTATT);
                                strMsg = $"Set Attenuation Value={Math.Round(testAtten, 2)} dB";
                                clsLogManager.LogReport(strMsg);
                            }
                            else
                            {
                                strMsg = $"Set Attenuaton Value: FAIL";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }
                                    if (fs != null)
                                    {
                                        fs.Dispose();
                                    }
                                    if (writeSamplePointcalib != null)
                                    {
                                        writeSamplePointcalib.Dispose();
                                    }
                                    if (fsSamplePointCalib != null)
                                    {
                                        fsSamplePointCalib.Dispose();
                                    }
                                    goto RETRY_LABEL;
                                }
                            }
                            // Đọc công suất đỉnh phổ
                            for (int nTry = 0; nTry < MesCnt; nTry++)
                            {

                                if (!SpecService.CalcMarkerMax(1)) throw new System.IO.IOException();
                                Thread.Sleep(300);
                                P1[nTry] = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Level);
                                Freq = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Freq);
                                clsLogManager.LogWarning("Max peak level = {0}dBm at {1} MHz", P1[nTry], Freq / 1e6);
                                Thread.Sleep(100);
                            }
                            RFPOWValue = P1.Average() - RefLevelOffset + EXAttenuator;
                            if(RFPOWValue<0)
                            {
                                strMsg = $"Powr is negative, DUT has a problem";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    if(writer!=null)
                                    {
                                        writer.Close();
                                        writer.Dispose();
                                    }
                                    if(fs!=null)
                                    { 
                                        fs.Close();
                                        fs.Dispose();
                                    
                                    }
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    goto RETRY_LABEL;
                                }
                            }
                            strMsg = $"Freq:{DutFreqTx} Ghz:Atten:{Math.Round(testAtten, 2)} dB:RFPOWER:{RFPOWValue} dBm";
                            writer.WriteLine($"{DateTime.Today.Day.ToString("00")+DateTime.Today.Month.ToString("00")+DateTime.Today.Year.ToString()} {DutFreqTx} {Math.Round(RFPOWValue, 3)} {Math.Round(testAtten, 2)}");
                            writer.Flush();
                            clsLogManager.LogReport(strMsg);
                            testAtten = Math.Round(testAtten, 2) - AttenStep3;
                            if (testAtten <= (MinAtten - AttenStep3))
                            {
                                strMsg = $"Finish Test";
                                clsLogManager.LogReport(strMsg);
                                writer.Close();
                                writer.Dispose();
                                fs.Close();
                                fs.Dispose();
                                return TaskResult.PASS;
                            }
                        }
                        while (testAtten >= MinAtten);
                        //end test 
                    }
                    else if (typecalib == TypeCalib.KNN)
                    {
                        
                        List<TestPoint> lTestPoint = new List<TestPoint>();
                        List<TestPoint> uTestPoint = new List<TestPoint>();
                        double valueRFPowerCheck = double.NaN;
                        TaskResult ResultCalib = TaskResult.FAIL;
                        LoadDataTrain(trainDataFile, lTestPoint);
                        SetDataTest(uTestPoint, "LowPower", DutFreqTx, LowRFPowerTarget);
                        SetDataTest(uTestPoint, "HighPower", DutFreqTx, HighRFPowerTarget);
                        Algorithm alg = new Algorithm(kfactor, lTestPoint, uTestPoint);
                        alg.runkNN();
                        List<TestPoint> ans = new List<TestPoint>();
                        ans = alg.getTestPointList();
                        if (!File.Exists(fileName_samplepointcalib))
                        {
                            fsSamplePointCalib = File.Open(fileName_samplepointcalib    , FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                            writeSamplePointcalib = new StreamWriter(fsSamplePointCalib);
                        }
                        else
                        {
                            fsSamplePointCalib = File.Open(fileName_samplepointcalib, FileMode.Append, FileAccess.Write, FileShare.Read);
                            writeSamplePointcalib = new StreamWriter(fsSamplePointCalib);
                        }
                        foreach (TestPoint t in ans)
                        {
                            if (t.getindex() == "LowPower")
                            {
                                CurrentAtten = t.getResponse();
                                clsLogManager.LogReport($"Predict Atten={t.getResponse()} dB at LowPower= {LowRFPowerTarget} dBm");
                                if((CurrentAtten<MinAtten)|(CurrentAtten>MaxAtten))
                                {
                                    strMsg = $"Predict Atten at LowPower: FAIL";
                                    clsLogManager.LogError(strMsg);
                                    if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                    {
                                        m_duDisplayValue = double.MinValue;
                                        return TaskResult.FAIL;
                                    }
                                    else
                                    {
                                        reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                        if (writer != null)
                                        {
                                            writer.Dispose();
                                        }
                                        if (fs != null)
                                        {
                                            fs.Dispose();
                                        }
                                        if (writeSamplePointcalib != null)
                                        {
                                            writeSamplePointcalib.Dispose();
                                        }
                                        if (fsSamplePointCalib != null)
                                        {
                                            fsSamplePointCalib.Dispose();
                                        }
                                        goto RETRY_LABEL;
                                    }
                                }    
                            }
                        }
                        if (VHCRService.SetAttenuation(Math.Round(CurrentAtten, 2)))
                        {
                            Thread.Sleep(delayDUTATT);
                            strMsg = $"Set Attenuation Value={Math.Round(CurrentAtten, 2)} dB";
                            clsLogManager.LogReport(strMsg);
                        }
                        else
                        {
                            strMsg = $"Set Attenuaton Value: FAIL";
                            clsLogManager.LogError(strMsg);
                            if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                            {
                                m_duDisplayValue = double.MinValue;
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                if (writer != null)
                                {
                                    writer.Dispose();
                                }
                                if (fs != null)
                                {
                                    fs.Dispose();
                                }
                                if (writeSamplePointcalib != null)
                                {
                                    writeSamplePointcalib.Dispose();
                                }
                                if (fsSamplePointCalib != null)
                                {
                                    fsSamplePointCalib.Dispose();
                                }
                                goto RETRY_LABEL;
                            }
                        }
                        // Đọc công suất đỉnh phổ
                        for (int nTry = 0; nTry < MesCnt; nTry++)
                        {

                            if (!SpecService.CalcMarkerMax(1)) throw new System.IO.IOException();
                            Thread.Sleep(300);
                            P1[nTry] = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Level);
                            Freq = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Freq);
                            clsLogManager.LogWarning("Max peak level = {0}dBm at {1} MHz", P1[nTry], Freq / 1e6);
                            Thread.Sleep(100);
                        }
                        valueRFPowerCheck = P1.Average() - RefLevelOffset + EXAttenuator;
                        if (valueRFPowerCheck < 0)
                        {
                            strMsg = $"Powr is negative, DUT has a problem";
                            clsLogManager.LogError(strMsg);
                            if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                            {
                                m_duDisplayValue = double.MinValue;
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                if (writer != null)
                                {
                                    writer.Dispose();
                                }
                                if (fs != null)
                                {
                                    fs.Dispose();
                                }
                                if (writeSamplePointcalib != null)
                                {
                                    writeSamplePointcalib.Dispose();
                                }
                                if (fsSamplePointCalib != null)
                                {
                                    fsSamplePointCalib.Dispose();
                                }
                                goto RETRY_LABEL;
                            }
                        }
                        writeSamplePointcalib.WriteLine($"{DateTime.Today.Day.ToString("00") + DateTime.Today.Month.ToString("00") + DateTime.Today.Year.ToString()} {DutFreqTx} {Math.Round(valueRFPowerCheck, 3)} {Math.Round(CurrentAtten, 2)}");
                        writeSamplePointcalib.Flush();
                        if ((valueRFPowerCheck >= RFPOWLOWMin) & (valueRFPowerCheck <= RFPOWLOWMax))
                        {
                            RFPOWValue = valueRFPowerCheck;
                            m_duDisplayValue = valueRFPowerCheck;
                        }
                        else if(valueRFPowerCheck>RFPOWLOWMax)
                        {
                            ResultCalib = AdjPower_Decrease(CurrentAtten, VHCRService, SpecService, RFPOWLOWMin, RFPOWLOWMax, out valueRFPowerCheck);
                            if(ResultCalib==TaskResult.FAIL)
                            {
                                strMsg = $"Calib is Fail";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }
                                    if (fs != null)
                                    {
                                        fs.Dispose();
                                    }
                                    if (writeSamplePointcalib != null)
                                    {
                                        writeSamplePointcalib.Dispose();
                                    }
                                    if (fsSamplePointCalib != null)
                                    {
                                        fsSamplePointCalib.Dispose();
                                    }
                                    goto RETRY_LABEL;
                                }
                            }
                            else
                            {
                                RFPOWValue = valueRFPowerCheck;
                                m_duDisplayValue = valueRFPowerCheck;
                            }
                        }
                        else if(valueRFPowerCheck<RFPOWLOWMin)
                        {
                            ResultCalib = AdjPower_Increase(CurrentAtten, VHCRService, SpecService, RFPOWLOWMin, RFPOWLOWMax, out valueRFPowerCheck);
                            if (ResultCalib == TaskResult.FAIL)
                            {
                                strMsg = $"Calib is Fail";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }
                                    if (fs != null)
                                    {
                                        fs.Dispose();
                                    }
                                    if (writeSamplePointcalib != null)
                                    {
                                        writeSamplePointcalib.Dispose();
                                    }
                                    if (fsSamplePointCalib != null)
                                    {
                                        fsSamplePointCalib.Dispose();
                                    }
                                    goto RETRY_LABEL;
                                }
                            }
                            else
                            {
                                RFPOWValue = valueRFPowerCheck;
                                m_duDisplayValue = valueRFPowerCheck;
                            }
                        }
                        if (VHCRService.SetPowerRF(RFPOWValue))
                        {
                            Thread.Sleep(delayDUTATT);
                            strMsg = $"Save Low Power Value:OK";
                            clsLogManager.LogReport(strMsg);
                            isCalibLowPower = true;
                        }
                        else
                        {
                            strMsg = $"Save Low Power Value:Fail";
                            clsLogManager.LogError(strMsg);
                            if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                            {
                                m_duDisplayValue = double.MinValue;
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                if (writer != null)
                                {
                                    writer.Dispose();
                                }
                                if (fs != null)
                                {
                                    fs.Dispose();
                                }
                                if (writeSamplePointcalib != null)
                                {
                                    writeSamplePointcalib.Dispose();
                                }
                                if (fsSamplePointCalib != null)
                                {
                                    fsSamplePointCalib.Dispose();
                                }
                                goto RETRY_LABEL;
                            }

                        }
                        foreach (TestPoint t in ans)
                        {
                            if (t.getindex() == "HighPower")
                            {
                                CurrentAtten = t.getResponse();
                                clsLogManager.LogReport($"Predict Atten={t.getResponse()} dB at HighPower= {HighRFPowerTarget} dBm");
                                if ((CurrentAtten < MinAtten) | (CurrentAtten > MaxAtten))
                                {
                                    strMsg = $"Predict Atten at HighPower: FAIL";
                                    clsLogManager.LogError(strMsg);
                                    if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                    {
                                        m_duDisplayValue = double.MinValue;
                                        return TaskResult.FAIL;
                                    }
                                    else
                                    {
                                        reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                        if (writer != null)
                                        {
                                            writer.Dispose();
                                        }
                                        if (fs != null)
                                        {
                                            fs.Dispose();
                                        }
                                        if (writeSamplePointcalib != null)
                                        {
                                            writeSamplePointcalib.Dispose();
                                        }
                                        if (fsSamplePointCalib != null)
                                        {
                                            fsSamplePointCalib.Dispose();
                                        }
                                        goto RETRY_LABEL;
                                    }
                                }
                            }
                        }
                        if (VHCRService.SetAttenuation(Math.Round(CurrentAtten, 2)))
                        {
                            Thread.Sleep(delayDUTATT);
                            strMsg = $"Set Attenuation Value={Math.Round(CurrentAtten, 2)} dB";
                            clsLogManager.LogReport(strMsg);
                        }
                        else
                        {
                            strMsg = $"Set Attenuaton Value: FAIL";
                            clsLogManager.LogError(strMsg);
                            if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                            {
                                m_duDisplayValue = double.MinValue;
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                if (writer != null)
                                {
                                    writer.Dispose();
                                }
                                if (fs != null)
                                {
                                    fs.Dispose();
                                }
                                if (writeSamplePointcalib != null)
                                {
                                    writeSamplePointcalib.Dispose();
                                }
                                if (fsSamplePointCalib != null)
                                {
                                    fsSamplePointCalib.Dispose();
                                }
                                goto RETRY_LABEL;
                            }
                        }
                        // Đọc công suất đỉnh phổ
                        for (int nTry = 0; nTry < MesCnt; nTry++)
                        {

                            if (!SpecService.CalcMarkerMax(1)) throw new System.IO.IOException();
                            Thread.Sleep(300);
                            P1[nTry] = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Level);
                            Freq = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Freq);
                            clsLogManager.LogWarning("Max peak level = {0}dBm at {1} MHz", P1[nTry], Freq / 1e6);
                            Thread.Sleep(100);
                        }
                        valueRFPowerCheck = P1.Average() - RefLevelOffset + EXAttenuator;
                        if (valueRFPowerCheck < 0)
                        {
                            strMsg = $"Powr is negative, DUT has a problem";
                            clsLogManager.LogError(strMsg);
                            if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                            {
                                m_duDisplayValue = double.MinValue;
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                if (writer != null)
                                {
                                    writer.Dispose();
                                }
                                if (fs != null)
                                {
                                    fs.Dispose();
                                }
                                if (writeSamplePointcalib != null)
                                {
                                    writeSamplePointcalib.Dispose();
                                }
                                if (fsSamplePointCalib != null)
                                {
                                    fsSamplePointCalib.Dispose();
                                }
                                goto RETRY_LABEL;
                            }
                        }
                        writeSamplePointcalib.WriteLine($"{DateTime.Today.Day.ToString("00") + DateTime.Today.Month.ToString("00") + DateTime.Today.Year.ToString()} {DutFreqTx} {Math.Round(valueRFPowerCheck, 3)} {Math.Round(CurrentAtten, 2)}");
                        writeSamplePointcalib.Flush();
                        if ((valueRFPowerCheck >= RFPOWHIGHMin) & (valueRFPowerCheck <= RFPOWHIGHMax))
                        {
                            RFPOWValue = valueRFPowerCheck;
                            RFHighPowerValue = valueRFPowerCheck;
                        }
                        else if (valueRFPowerCheck > RFPOWHIGHMax)
                        {
                            ResultCalib = AdjPower_Decrease(CurrentAtten, VHCRService, SpecService, RFPOWHIGHMin, RFPOWHIGHMax, out valueRFPowerCheck);
                            if (ResultCalib == TaskResult.FAIL)
                            {
                                strMsg = $"Calib is Fail";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }
                                    if (fs != null)
                                    {
                                        fs.Dispose();
                                    }
                                    if (writeSamplePointcalib != null)
                                    {
                                        writeSamplePointcalib.Dispose();
                                    }
                                    if (fsSamplePointCalib != null)
                                    {
                                        fsSamplePointCalib.Dispose();
                                    }
                                    goto RETRY_LABEL;
                                }
                            }
                            else
                            {
                                RFPOWValue = valueRFPowerCheck;
                                RFHighPowerValue = valueRFPowerCheck;
                            }
                        }
                        else if (valueRFPowerCheck < RFPOWHIGHMin)
                        {
                            ResultCalib = AdjPower_Increase(CurrentAtten, VHCRService, SpecService, RFPOWHIGHMin, RFPOWHIGHMax, out valueRFPowerCheck);
                            if (ResultCalib == TaskResult.FAIL)
                            {
                                strMsg = $"Calib is Fail";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }
                                    if (fs != null)
                                    {
                                        fs.Dispose();
                                    }
                                    if (writeSamplePointcalib != null)
                                    {
                                        writeSamplePointcalib.Dispose();
                                    }
                                    if (fsSamplePointCalib != null)
                                    {
                                        fsSamplePointCalib.Dispose();
                                    }
                                    goto RETRY_LABEL;
                                }
                            }
                            else
                            {
                                RFPOWValue = valueRFPowerCheck;
                                RFHighPowerValue = valueRFPowerCheck;
                            }
                        }
                        if (VHCRService.SetPowerRF(RFPOWValue))
                        {
                            Thread.Sleep(delayDUTHIGHPWR);
                            strMsg = $"Save High Power Value:OK";
                            clsLogManager.LogReport(strMsg);
                            iscalibHighPower = true;
                        }
                        else
                        {
                            strMsg = $"Save High Power Value:Fail";
                            clsLogManager.LogError(strMsg);
                            if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                            {
                                m_duDisplayValue = double.MinValue;
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                if (writer != null)
                                {
                                    writer.Dispose();
                                }
                                if (fs != null)
                                {
                                    fs.Dispose();
                                }
                                if (writeSamplePointcalib != null)
                                {
                                    writeSamplePointcalib.Dispose();
                                }
                                if (fsSamplePointCalib != null)
                                {
                                    fsSamplePointCalib.Dispose();
                                }
                                goto RETRY_LABEL;
                            }

                        }
                        if (isCalibLowPower & iscalibHighPower)
                        {
                            strMsg = $"Calibration Low and High Power is Ok";
                            clsLogManager.LogReport(strMsg);
                            return TaskResult.PASS;
                        }

                    }
                    else if (typecalib == TypeCalib.Manual)
                    {
                        if (!File.Exists(fileName_samplepointcalib))
                        {
                            fsSamplePointCalib = File.Open(fileName_samplepointcalib, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                            writeSamplePointcalib = new StreamWriter(fsSamplePointCalib);
                        }
                        else
                        {
                            fsSamplePointCalib = File.Open(fileName_samplepointcalib, FileMode.Append, FileAccess.Write, FileShare.Read);
                            writeSamplePointcalib = new StreamWriter(fsSamplePointCalib);
                        }
                        while (true)
                        {
                            //CurrentAtten = CurrentAtten - AttenStep;
                            if (((RFPOWValue > 0) & (RFPOWValue <= (RFPOWLOWMin - DeltaStep2))) | ((RFPOWValue > RFPOWLOWMin) & (RFPOWValue <= (RFPOWHIGHMin - DeltaStep2))))
                            {
                                CurrentAtten = CurrentAtten - AttenStep;
                            }
                            else if (((RFPOWValue > (RFPOWLOWMin - DeltaStep2)) & (RFPOWValue <= (RFPOWLOWMin - DeltaStep3))) | ((RFPOWValue > (RFPOWHIGHMin - DeltaStep2)) & (RFPOWValue <= (RFPOWHIGHMin - DeltaStep3))))
                            {
                                CurrentAtten = CurrentAtten - AttenStep2;
                            }
                            else if (((RFPOWValue > (RFPOWLOWMin - DeltaStep3)) & (RFPOWValue <= RFPOWLOWMin)) | ((RFPOWValue > (RFPOWHIGHMin - DeltaStep3)) & (RFPOWValue <= RFPOWHIGHMin)))
                            {
                                CurrentAtten = CurrentAtten - AttenStep3;
                            }
                            if (CurrentAtten < MinAtten)
                            {
                                strMsg = $"Current Attenuation Value is lower than Min Attenuation";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }
                                    if (fs != null)
                                    {
                                        fs.Dispose();
                                    }
                                    if (writeSamplePointcalib != null)
                                    {
                                        writeSamplePointcalib.Dispose();
                                    }
                                    if (fsSamplePointCalib != null)
                                    {
                                        fsSamplePointCalib.Dispose();
                                    }
                                    goto RETRY_LABEL;
                                }
                            }

                            if (VHCRService.SetAttenuation(Math.Round(CurrentAtten, 2)))
                            {
                                Thread.Sleep(delayDUTATT);
                                strMsg = $"Set Attenuation Value={Math.Round(CurrentAtten, 2)} dB";
                                clsLogManager.LogReport(strMsg);
                            }
                            else
                            {
                                strMsg = $"Set Attenuaton Value: FAIL";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }
                                    if (fs != null)
                                    {
                                        fs.Dispose();
                                    }
                                    if (writeSamplePointcalib != null)
                                    {
                                        writeSamplePointcalib.Dispose();
                                    }
                                    if (fsSamplePointCalib != null)
                                    {
                                        fsSamplePointCalib.Dispose();
                                    }
                                    goto RETRY_LABEL;
                                }
                            }
                            // Đọc công suất đỉnh phổ
                            for (int nTry = 0; nTry < MesCnt; nTry++)
                            {

                                if (!SpecService.CalcMarkerMax(1)) throw new System.IO.IOException();
                                Thread.Sleep(300);
                                P1[nTry] = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Level);
                                Freq = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Freq);
                                clsLogManager.LogWarning("Max peak level = {0}dBm at {1} MHz", P1[nTry], Freq / 1e6);
                                Thread.Sleep(100);
                            }
                            RFPOWValue = P1.Average() - RefLevelOffset + EXAttenuator;
                            if (RFPOWValue < 0)
                            {
                                strMsg = $"Powr is negative, DUT has a problem";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }
                                    if (fs != null)
                                    {
                                        fs.Dispose();
                                    }
                                    if (writeSamplePointcalib != null)
                                    {
                                        writeSamplePointcalib.Dispose();
                                    }
                                    if (fsSamplePointCalib != null)
                                    {
                                        fsSamplePointCalib.Dispose();
                                    }
                                    goto RETRY_LABEL;
                                }
                            }
                            writeSamplePointcalib.WriteLine($"{DateTime.Today.Day.ToString("00") + DateTime.Today.Month.ToString("00") + DateTime.Today.Year.ToString()} {DutFreqTx} {Math.Round(RFPOWValue, 3)} {Math.Round(CurrentAtten, 2)}");
                            writeSamplePointcalib.Flush();
                            queuePower.Enqueue(RFPOWValue);
                            timesEnqueue++;
                            clsLogManager.LogWarning($"TX power: {RFPOWValue} dBm with MesPower:{ P1.Average()} dBm RefLevelOffset:{RefLevelOffset} dB: EXAttenuator:{EXAttenuator} dB");
                            if (timesEnqueue >= NumStep)
                            {
                                double[] arrPower = queuePower.ToArray();
                                if (arrPower[arrPower.Length - 1] < arrPower[0])
                                {
                                    strMsg = $"Power Value={arrPower[arrPower.Length - 1]} dBm:Power Value old={arrPower[0]}dBm: isn't increase when attenuation value is decrease";
                                    clsLogManager.LogError(strMsg);
                                    if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                    {
                                        m_duDisplayValue = double.MinValue;
                                        return TaskResult.FAIL;
                                    }
                                    else
                                    {
                                        reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                        if (writer != null)
                                        {
                                            writer.Dispose();
                                        }
                                        if (fs != null)
                                        {
                                            fs.Dispose();
                                        }
                                        if (writeSamplePointcalib != null)
                                        {
                                            writeSamplePointcalib.Dispose();
                                        }
                                        if (fsSamplePointCalib != null)
                                        {
                                            fsSamplePointCalib.Dispose();
                                        }
                                        goto RETRY_LABEL;
                                    }
                                }
                                queuePower.Dequeue();
                            }
                            if (isCalibLowPower == false)
                            {
                                if ((RFPOWValue >= RFPOWLOWMin) & (RFPOWValue <= RFPOWLOWMax))
                                {
                                    strMsg = $"Power Value ={RFPOWValue} in range {RFPOWLOWMin} dBm ÷ {RFPOWLOWMax} dBm";
                                    clsLogManager.LogReport(strMsg);
                                    if (VHCRService.SetPowerRF(RFPOWValue))
                                    {
                                        Thread.Sleep(delayDUTATT);
                                        strMsg = $"Save Low Power Value:OK";
                                        clsLogManager.LogReport(strMsg);
                                        isCalibLowPower = true;
                                        m_duDisplayValue = RFPOWValue;
                                    }
                                    else
                                    {
                                        strMsg = $"Save Low Power Value:Fail";
                                        clsLogManager.LogError(strMsg);
                                        if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                        {
                                            m_duDisplayValue = double.MinValue;
                                            return TaskResult.FAIL;
                                        }
                                        else
                                        {
                                            reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                            if (writer != null)
                                            {
                                                writer.Dispose();
                                            }
                                            if (fs != null)
                                            {
                                                fs.Dispose();
                                            }
                                            if (writeSamplePointcalib != null)
                                            {
                                                writeSamplePointcalib.Dispose();
                                            }
                                            if (fsSamplePointCalib != null)
                                            {
                                                fsSamplePointCalib.Dispose();
                                            }
                                            goto RETRY_LABEL;
                                        }

                                    }
                                }
                                else if (RFPOWValue > RFPOWLOWMax)
                                {
                                    strMsg = $"Power value is higher than Max Low Power Value but CalibLowPower hasn't done yet";
                                    clsLogManager.LogError(strMsg);
                                    if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                    {
                                        m_duDisplayValue = double.MinValue;
                                        return TaskResult.FAIL;
                                    }
                                    else
                                    {
                                        reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                        if (writer != null)
                                        {
                                            writer.Dispose();
                                        }
                                        if (fs != null)
                                        {
                                            fs.Dispose();
                                        }
                                        if (writeSamplePointcalib != null)
                                        {
                                            writeSamplePointcalib.Dispose();
                                        }
                                        if (fsSamplePointCalib != null)
                                        {
                                            fsSamplePointCalib.Dispose();
                                        }
                                        goto RETRY_LABEL;
                                    }
                                }
                            }
                            else
                            {
                                if (iscalibHighPower == false)
                                {
                                    if ((RFPOWValue >= RFPOWHIGHMin) & (RFPOWValue <= RFPOWHIGHMax))
                                    {
                                        strMsg = $"Power Value ={RFPOWValue} in range {RFPOWHIGHMin} dBm ÷ {RFPOWHIGHMax} dBm";
                                        clsLogManager.LogReport(strMsg);
                                        if (VHCRService.SetPowerRF(RFPOWValue))
                                        {
                                            Thread.Sleep(delayDUTHIGHPWR);
                                            strMsg = $"Save High Power Value:OK";
                                            clsLogManager.LogReport(strMsg);
                                            iscalibHighPower = true;
                                            RFHighPowerValue = RFPOWValue;
                                        }
                                        else
                                        {
                                            strMsg = $"Save High Power Value:Fail";
                                            clsLogManager.LogError(strMsg);
                                            if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                            {
                                                m_duDisplayValue = double.MinValue;
                                                return TaskResult.FAIL;
                                            }
                                            else
                                            {
                                                reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                                if (writer != null)
                                                {
                                                    writer.Dispose();
                                                }
                                                if (fs != null)
                                                {
                                                    fs.Dispose();
                                                }
                                                if (writeSamplePointcalib != null)
                                                {
                                                    writeSamplePointcalib.Dispose();
                                                }
                                                if (fsSamplePointCalib != null)
                                                {
                                                    fsSamplePointCalib.Dispose();
                                                }
                                                goto RETRY_LABEL;
                                            }

                                        }
                                    }
                                    else if (RFPOWValue > RFPOWHIGHMax)
                                    {
                                        strMsg = $"Power value is higher Max Power Value";
                                        clsLogManager.LogError(strMsg);
                                        if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                        {
                                            m_duDisplayValue = double.MinValue;
                                            return TaskResult.FAIL;
                                        }
                                        else
                                        {
                                            reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                            if (writer != null)
                                            {
                                                writer.Dispose();
                                            }
                                            if (fs != null)
                                            {
                                                fs.Dispose();
                                            }
                                            if (writeSamplePointcalib != null)
                                            {
                                                writeSamplePointcalib.Dispose();
                                            }
                                            if (fsSamplePointCalib != null)
                                            {
                                                fsSamplePointCalib.Dispose();
                                            }
                                            goto RETRY_LABEL;
                                        }
                                    }
                                }
                            }

                            if (isCalibLowPower & iscalibHighPower)
                            {
                                strMsg = $"Calibration Low and High Power is Ok";
                                clsLogManager.LogReport(strMsg);
                                return TaskResult.PASS;
                            }
                            if (Environment.TickCount - StartTime > TimeOut)
                            {

                                strMsg = $"TimeOut Calibration";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.MinValue;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                                    if (writer != null)
                                    {
                                        writer.Dispose();
                                    }
                                    if (fs != null)
                                    {
                                        fs.Dispose();
                                    }
                                    if (writeSamplePointcalib != null)
                                    {
                                        writeSamplePointcalib.Dispose();
                                    }
                                    if (fsSamplePointCalib != null)
                                    {
                                        fsSamplePointCalib.Dispose();
                                    }
                                    goto RETRY_LABEL;
                                }
                            }
                        }
                    }
                    m_duDisplayValue = double.NaN;
                    return TaskResult.FAIL;
                }
                else
                {
                    strMsg = "Devices are unvailable!";
                    clsLogManager.LogReport(strMsg);
                    if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                    {
                        m_duDisplayValue = double.MinValue;
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        reconnectDevice(DUT, SpecDevAccess, deviceinfo_VHCR, deviceinfo_SPEC_DEV);
                        if (writer != null)
                        {
                            writer.Dispose();
                        }
                        if (fs != null)
                        {
                            fs.Dispose();
                        }
                        if (writeSamplePointcalib != null)
                        {
                            writeSamplePointcalib.Dispose();
                        }
                        if (fsSamplePointCalib != null)
                        {
                            fsSamplePointCalib.Dispose();
                        }
                        goto RETRY_LABEL;
                    }
                }
            }
            catch (System.Exception ex)
            {
                strMsg = "Exception: " + ex.Message;
                clsLogManager.LogError("Excution: {0}", ex.ToString());
                if (writer != null)
                {
                    writer.Dispose();
                }
                if (fs != null)
                {
                    fs.Dispose();
                }
                if(writeSamplePointcalib!=null)
                {
                    writeSamplePointcalib.Dispose();
                }
                if(fsSamplePointCalib!=null)
                {
                    fsSamplePointCalib.Dispose();
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
                return TaskResult.FAIL;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
                if (fs != null)
                {
                    fs.Dispose();
                }
                if (writeSamplePointcalib != null)
                {
                    writeSamplePointcalib.Dispose();
                }
                if (fsSamplePointCalib != null)
                {
                    fsSamplePointCalib.Dispose();
                }
            }
        }
        public void reconnectDevice(IAccessDeviceService DUT, IAccessDeviceService SpecDevAccess, IDeviceInfor deviceinfo_VHCR, IDeviceInfor deviceinfo_SPEC_DEV)
        {
            if (DUT != null)
            {
                DUT.DisconnectDevice();
                Thread.Sleep(200);
                if (deviceinfo_VHCR != null)
                {
                    DUT.Connect2Device(deviceinfo_VHCR);
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
            clsVHCR_CalibPower task = new clsVHCR_CalibPower();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        public void LoadDataTrain(string trainfile, List<TestPoint> lTestPoint)
        {
            int counter = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(trainfile);

            while ((line = file.ReadLine()) != null)
            {
                string[] split = line.Split(' ');

                TestPoint cus = new TestPoint();

                cus.setindex(split[0]);

                cus.setFreq(double.Parse(split[1]));
                cus.setRFPower(double.Parse(split[2]));
                cus.setResponse(double.Parse(split[3]));

                lTestPoint.Add(cus);

                counter = counter + 1;
            }

            file.Close();
        }
        public void SetDataTest(List<TestPoint> uTestPoint, string index,double Freq,double RFPower)
        {
            TestPoint cus = new TestPoint();

            cus.setindex(index);

            cus.setFreq(Freq);
            cus.setRFPower(RFPower);
            cus.setResponse(-1);

            uTestPoint.Add(cus);
        }
        public TaskResult AdjPower_Increase(double initialAtten, IDutVHCR VHCRService, ISpecDeviceN9020B SpecService, double MinRequire, double MaxRequire, out double ResultCalibValue)
        {
            string strMsg;
            double CurrentAtten = initialAtten;
            double RFPOWValue = double.MinValue;
            double[] P1 = new double[MesCnt];
            double Freq = 0; // Tần số tại đỉnh phổ
            int timesEnqueue = 0;
            queuePower.Clear();
            int StartTime = Environment.TickCount;
            while (true)
            {
                //CurrentAtten = CurrentAtten - AttenStep3;
                if (((RFPOWValue > 0) & (RFPOWValue <= (MinRequire - DeltaStep2))))
                {
                    CurrentAtten = CurrentAtten - AttenStep;
                }
                else if (((RFPOWValue > (MinRequire - DeltaStep2)) & (RFPOWValue <= (MinRequire - DeltaStep3))))
                {
                    CurrentAtten = CurrentAtten - AttenStep2;
                }
                else if (((RFPOWValue > (MinRequire - DeltaStep3)) & (RFPOWValue <= MinRequire)) )
                {
                    CurrentAtten = CurrentAtten - AttenStep3;
                }
                if (CurrentAtten < MinAtten)
                {
                    strMsg = $"Current Attenuation Value is lower than Min Attenuation";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = double.NaN;
                    return TaskResult.FAIL;
                }

                if (VHCRService.SetAttenuation(Math.Round(CurrentAtten, 2)))
                {
                    Thread.Sleep(delayDUTATT);
                    strMsg = $"Set Attenuation Value={Math.Round(CurrentAtten, 2)} dB";
                    clsLogManager.LogReport(strMsg);
                }
                else
                {
                    strMsg = $"Set Attenuaton Value: FAIL";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = RFPOWValue;
                    return TaskResult.FAIL;
                }
                // Đọc công suất đỉnh phổ
                for (int nTry = 0; nTry < MesCnt; nTry++)
                {

                    if (!SpecService.CalcMarkerMax(1)) throw new System.IO.IOException();
                    Thread.Sleep(300);
                    P1[nTry] = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Level);
                    Freq = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Freq);
                    clsLogManager.LogWarning("Max peak level = {0}dBm at {1} MHz", P1[nTry], Freq / 1e6);
                    Thread.Sleep(100);
                }
                RFPOWValue = P1.Average() - RefLevelOffset + EXAttenuator;
                if (RFPOWValue < 0)
                {
                    strMsg = $"Powr is negative, DUT has a problem";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = double.NaN;
                    return TaskResult.FAIL;
                }
                writeSamplePointcalib.WriteLine($"{DateTime.Today.Day.ToString("00") + DateTime.Today.Month.ToString("00") + DateTime.Today.Year.ToString()} {DutFreqTx} {Math.Round(RFPOWValue, 3)} {Math.Round(CurrentAtten, 2)}");
                writeSamplePointcalib.Flush();
                queuePower.Enqueue(RFPOWValue);
                timesEnqueue++;
                clsLogManager.LogWarning($"TX power: {RFPOWValue} dBm with MesPower:{ P1.Average()} dBm RefLevelOffset:{RefLevelOffset} dB: EXAttenuator:{EXAttenuator} dB");
                if (timesEnqueue >= NumStep)
                {
                    double[] arrPower = queuePower.ToArray();
                    if (arrPower[arrPower.Length - 1] < arrPower[0])
                    {
                        strMsg = $"Power Value={arrPower[arrPower.Length - 1]} dBm:Power Value old={arrPower[0]}dBm: isn't increase when attenuation value is decrease";
                        clsLogManager.LogError(strMsg);
                        ResultCalibValue = RFPOWValue;
                        return TaskResult.FAIL;
                    }
                    queuePower.Dequeue();
                }
                if ((RFPOWValue >= MinRequire) & (RFPOWValue <= MaxRequire))
                {
                    strMsg = $"Power Value ={RFPOWValue} in range {MinRequire} dBm ÷ {MaxRequire} dBm";                 
                    clsLogManager.LogReport(strMsg);
                    ResultCalibValue = RFPOWValue;
                    return TaskResult.PASS;
                }
                else if (RFPOWValue > MaxRequire)
                {
                    strMsg = $"Power value is higher than Max Power Value but CalibPower hasn't done yet";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = double.NaN;
                    return TaskResult.FAIL;
                }
                if (Environment.TickCount - StartTime > TimeOut)
                {

                    strMsg = $"TimeOut Calibration";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = double.NaN;
                    return TaskResult.FAIL;
                }
            }
        }
        public TaskResult AdjPower_Decrease(double initialAtten, IDutVHCR VHCRService, ISpecDeviceN9020B SpecService, double MinRequire, double MaxRequire, out double ResultCalibValue)
        {
            string strMsg;
            double CurrentAtten = initialAtten;
            double RFPOWValue = double.MinValue;
            double[] P1 = new double[MesCnt];
            double Freq = 0; // Tần số tại đỉnh phổ
            int timesEnqueue = 0;
            queuePower.Clear();
            int StartTime = Environment.TickCount;
            while (true)
            
            {
                //CurrentAtten = CurrentAtten + AttenStep3;
                if ( (RFPOWValue >=(MaxRequire + DeltaStep2)))
                {
                    CurrentAtten = CurrentAtten + AttenStep;
                }
                else if (((RFPOWValue < (MaxRequire + DeltaStep2)) & (RFPOWValue >= (MaxRequire + DeltaStep3))))
                {
                    CurrentAtten = CurrentAtten + AttenStep2;
                }
                else if (((RFPOWValue < (MaxRequire + DeltaStep3)) & (RFPOWValue >= MaxRequire)))
                {
                    CurrentAtten = CurrentAtten + AttenStep3;
                }
                if (CurrentAtten > MaxAtten)
                {
                    strMsg = $"Current Attenuation Value is lower than Min Attenuation";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = double.NaN;
                    return TaskResult.FAIL;
                }

                if (VHCRService.SetAttenuation(Math.Round(CurrentAtten, 2)))
                {
                    Thread.Sleep(delayDUTATT);
                    strMsg = $"Set Attenuation Value={Math.Round(CurrentAtten, 2)} dB";
                    clsLogManager.LogReport(strMsg);
                }
                else
                {
                    strMsg = $"Set Attenuaton Value: FAIL";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = RFPOWValue;
                    return TaskResult.FAIL;
                }
                // Đọc công suất đỉnh phổ
                for (int nTry = 0; nTry < MesCnt; nTry++)
                {

                    if (!SpecService.CalcMarkerMax(1)) throw new System.IO.IOException();
                    Thread.Sleep(300);
                    P1[nTry] = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Level);
                    Freq = SpecService.GetValueAtIndexMaker(1, MarkerValueType.Freq);
                    clsLogManager.LogWarning("Max peak level = {0}dBm at {1} MHz", P1[nTry], Freq / 1e6);
                    Thread.Sleep(100);
                }
                RFPOWValue = P1.Average() - RefLevelOffset + EXAttenuator;
                if (RFPOWValue < 0)
                {
                    strMsg = $"Powr is negative, DUT has a problem";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = double.NaN;
                    return TaskResult.FAIL;
                }
                writeSamplePointcalib.WriteLine($"{DateTime.Today.Day.ToString("00") + DateTime.Today.Month.ToString("00") + DateTime.Today.Year.ToString()} {DutFreqTx} {Math.Round(RFPOWValue, 3)} {Math.Round(CurrentAtten, 2)}");
                writeSamplePointcalib.Flush();
                queuePower.Enqueue(RFPOWValue);
                timesEnqueue++;
                clsLogManager.LogWarning($"TX power: {RFPOWValue} dBm with MesPower:{ P1.Average()} dBm RefLevelOffset:{RefLevelOffset} dB: EXAttenuator:{EXAttenuator} dB");
                if (timesEnqueue >= NumStep)
                {
                    double[] arrPower = queuePower.ToArray();
                    if (arrPower[arrPower.Length - 1] > arrPower[0])
                    {
                        strMsg = $"Power Value={arrPower[arrPower.Length - 1]} dBm:Power Value old={arrPower[0]}dBm: isn't increase when attenuation value is decrease";
                        clsLogManager.LogError(strMsg);
                        ResultCalibValue = RFPOWValue;
                        return TaskResult.FAIL;
                    }
                    queuePower.Dequeue();
                }
                if ((RFPOWValue >= MinRequire) & (RFPOWValue <= MaxRequire))
                {
                    strMsg = $"Power Value ={RFPOWValue} in range {MinRequire} dBm ÷ {MaxRequire} dBm";
                    clsLogManager.LogReport(strMsg);
                    ResultCalibValue = RFPOWValue;
                    return TaskResult.PASS;
                }
                else if (RFPOWValue <MinRequire)
                {
                    strMsg = $"Power value is lower than Min Power Value but CalibPower hasn't done yet";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = double.NaN;
                    return TaskResult.FAIL;
                }
                if (Environment.TickCount - StartTime > TimeOut)
                {

                    strMsg = $"TimeOut Calibration";
                    clsLogManager.LogError(strMsg);
                    ResultCalibValue = double.NaN;
                    return TaskResult.FAIL;
                }
            }
        }
        #region Test Parameter
        [Category("Test Parameter")]
        [DisplayName("Initial Attenuation Value (dB)")]
        [SaveAtt()]
        public double InitAtten { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Attenuation Step (dB)")]
        [SaveAtt()]
        public double AttenStep { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Attenuation Step 2 (dB)")]
        [SaveAtt()]
        public double AttenStep2 { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Attenuation Step 3 (dB)")]
        [SaveAtt()]
        public double AttenStep3 { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delta Step 2 (dB)")]
        [SaveAtt()]
        public double DeltaStep2 { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delta Step 3 (dB)")]
        [SaveAtt()]
        public double DeltaStep3 { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Min Attenuation Value(dB)")]
        [SaveAtt()]
        public double MinAtten { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Max Attenuation Value(dB)")]
        [SaveAtt()]
        public double MaxAtten { set; get; }
        [Category("Test Parameter")]
        [DisplayName("RF Power Low max (dBm)")]
        [SaveAtt()]
        public double RFPOWLOWMax { set; get; }
        [Category("Test Parameter")]
        [DisplayName("RF Power Low min (dBm)")]
        [SaveAtt()]
        public double RFPOWLOWMin { set; get; }
        [Category("Test Parameter")]
        [DisplayName("RF Power High max (dBm)")]
        [SaveAtt()]
        public double RFPOWHIGHMax { set; get; }
        [Category("Test Parameter")]
        [DisplayName("RF Power High min (dBm)")]
        [SaveAtt()]
        public double RFPOWHIGHMin { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after setting (ms)")]
        [SaveAtt()]
        public int DelayAfterSetConfig { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Measurement counter")]
        [SaveAtt()]
        public int MesCnt { set; get; } // Số lần đo
        [Category("Test Parameter")]
        [DisplayName("Power attenuator_Cable Loss (dB)")]
        [SaveAtt()]
        public double EXAttenuator { set; get; }
        //Powerunit
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public Powerunit Result_unit { set; get; }
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
        [Category("Test Parameter")]
        [DisplayName("Delay after turn off/on automatic power control(ms)")]
        [SaveAtt()]
        public int delayDUTTURNONOFFAPC { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set Attenuation(ms)")]
        [SaveAtt()]
        public int delayDUTATT { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set High Power(ms)")]
        [SaveAtt()]
        public int delayDUTHIGHPWR { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Time Out of Calibration(ms)")]
        [SaveAtt()]
        public int TimeOut { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Number Step before check Power")]
        [SaveAtt()]
        public int NumStep { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Calib Type")]
        [SaveAtt()]
        public TypeCalib typecalib { set; get; }
        [Category("KNN")]
        [DisplayName("Link Train Data")]
        [Editor(typeof(clsTXTFileOpenEditor), typeof(UITypeEditor))]
        [SaveAtt()]
        public string trainDataFile { set; get; }
        [Category("KNN")]
        [DisplayName("K factor")]
        [SaveAtt()]
        public int kfactor { set; get; }
        [Category("KNN")]
        [DisplayName("Low RF Power Target (dBm)")]
        [SaveAtt()]
        public double LowRFPowerTarget { set; get; }
        [Category("KNN")]
        [DisplayName("High RF Power Target (dBm)")]
        [SaveAtt()]
        public double HighRFPowerTarget { set; get; }
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
        [DisplayName("Span (kHz)")]
        [SaveAtt()]
        public double Span { set; get; }

        [Category("Spectrum analyzer")]
        [DisplayName("RbW (kHz)")]
        [SaveAtt()]
        public double Rbw { set; get; }

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

        [Category("Spectrum analyzer")]
        [DisplayName("Center Frequency (GHz)")]
        [SaveAtt()]
        public double CenterFreq { set; get; }
        #endregion
    }
}
