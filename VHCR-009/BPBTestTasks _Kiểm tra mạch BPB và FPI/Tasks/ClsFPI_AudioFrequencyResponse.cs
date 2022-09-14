using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CFTWinAppCore.Task;
using CFTWinAppCore.Sequences;
using CFTWinAppCore.DataLog;
using System.ComponentModel;
using LogLibrary;
using System.Net;
using GeneralTool;
using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.DeviceManager.DUTIOService;
using System.Threading;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.Helper;
using Option;
using System.Windows.Forms;

namespace BPBTestTasks
{
    [clsTaskMetaInfo("FPI Audio Frequency Response", "ClsFPI_AudioFrequencyResponse", true)]
    public class ClsFPI_AudioFrequencyResponse : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_strDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        public ClsFPI_AudioFrequencyResponse()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "FPI Audio Frequency Response";
            CTRL_VolumeLevel = false;
            DUTFrequency = 1000;
            AFLevel = 15;
            AFOut_port = AudioGenIndex.GEN_1;
            DelayBeforeCheck = 1000;
            DelayAfterSetConfig = 100;
            VolumeLevel = 11;
            LowTolarance = -3;
            HighTolarance = 3;
            AFFreq_Start1 = 300;
            AFFreq_Stop1 = 1000;
            StepFreq1 = 10;
            AFFreq_Start2 = 1000;
            AFFreq_Stop2 = 3000;
            StepFreq2 = 20;
            Result_unit = "dB";
        }
        [Browsable(false)]
        public bool SuperAdmin
        {
            get;
            set;
        }

        [Browsable(false)]
        public string Name
        {
            get { return "ClsFPI_AudioFrequencyResponse"; }
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
            return $"{m_strDisplayValue}";
        }

        public string GetMaxValue()
        {
            return $"{HighTolarance.ToString()}";
        }

        public string GetMinValue()
        {
            return $"{LowTolarance.ToString()}";
        }

        public string GetUnit()
        {
            return $"{Result_unit}";
        }
        [Category("#General Option")]
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
            return $"{m_strDisplayValue.ToString()} {Result_unit}";
        }
        public string GetDisplayMaxValue()
        {
            return $"{HighTolarance.ToString()} {Result_unit}";
        }
        public string GetDisplayMinValue()
        {
            return $"{LowTolarance.ToString()} {Result_unit}";
        }
        public TaskResult GetTaskResult()
        {
            return taskResult;
        }
        public string GetAddTaskDescription()
        {
            return $"{AFFreq_Start1} Hz to {AFFreq_Stop2} Hz";
        }

        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            m_TraceService.WriteLine(" Test Task: " + Description +  "\n");
            
                
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_BPB = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_audio = DevManager.GetDevRunTimeInfo(SysDevType.AUDIO_ANA.ToString()) as IDeviceInfor;
            IAccessDeviceService audioAccess = DevManager.GetDevService(SysDevType.AUDIO_ANA.ToString()) as IAccessDeviceService;
            RETRY_LABEL:
            m_strDisplayValue = "N/A";
            if ((AFFreq_Stop1 <= AFFreq_Start1) || (AFFreq_Start2 <= AFFreq_Stop1) || (AFFreq_Stop2 <= AFFreq_Start2))
            {
                clsLogManager.LogError("Set up frequency range is wrong");
                if (clsMsgHelper.ShowYesNo("Question", "Thiết lập dải tần chưa đúng, bạn có thực hiện lại không?") == DialogResult.No)
                {
                    return TaskResult.FAIL;
                }
                else
                {
                    goto RETRY_LABEL;
                }
            }
            Dictionary<double, double> dicTolerance = new Dictionary<double, double>();
            IAudioAnaDevice audioAnaDevice = DevManager.GetDevService(SysDevType.AUDIO_ANA.ToString()) as IAudioAnaDevice;
            if (audioAnaDevice == null)
            {
                clsLogManager.LogError("Can not get device service: IAudioAnaDevice");
                if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                {
                    return TaskResult.FAIL;
                }
                else
                {
                    goto RETRY_LABEL;
                }
            }
            if (!((IAccessDeviceService)audioAnaDevice).IsDeviceConnected())
            {
                clsLogManager.LogError("Device audioAnaDevice is not connected");
                if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                {
                    return TaskResult.FAIL;
                }
                else
                {
                    goto RETRY_LABEL;
                }
            }
            IBPBDevice bPBDevice = DevManager.GetDevService(SysDevType.DUT.ToString()) as IBPBDevice;
            if (bPBDevice == null)
            {
                clsLogManager.LogError("Can not get device service: IBPBDevice");
                if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                {
                    return TaskResult.FAIL;
                }
                else
                {
                    goto RETRY_LABEL;
                }
            }
            if (!((IAccessDeviceService)bPBDevice).IsDeviceConnected())
            {
                clsLogManager.LogError("Device is not connected");
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
                bool bOk = false;
                if (CTRL_VolumeLevel == true)
                {
                    bOk = bPBDevice.SetAudioVolume((byte)VolumeLevel);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to SetAudioVolume");
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
                //bool bOk = audioAnaDevice.SetAF_GenAndMes();
                bOk = audioAnaDevice.SetMultitone(false);
                bOk &= audioAnaDevice.SetAudioLevel(AFLevel, AFOut_port);
                bOk &= audioAnaDevice.SetAudioFreq(DUTFrequency, AFOut_port, FreqUnit.Hz);
                bOk &= audioAnaDevice.EnableOutput(true, AFOut_port);
                if (!bOk)
                {
                    clsLogManager.LogError("Fail to setup audio generator!");
                    if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL;
                    }
                }

                if (DelayBeforeCheck > 0)
                {
                    clsLogManager.LogWarning("DelayBeforeCheck: {0}ms...", DelayBeforeCheck);
                    Thread.Sleep(DelayBeforeCheck);
                }
                double duDutLevel = audioAnaDevice.GetAudioPowerLevel(AudioLevelUnit.MV, 1, true);
                clsLogManager.LogReport("Freq:{0} Hz:Vref: {1} mV", DUTFrequency.ToString(), duDutLevel.ToString());
                Thread.Sleep(DelayBeforeCheck);
                //                double duDutLeveldBm = clsRFPowerUnitHelper.Vol2Dbm(duDutLevel / 1000);
                double currLowTolarance = 0;
                double currHighTolarance = 0;
                for (double freq = AFFreq_Start1; freq <= AFFreq_Stop1; freq += StepFreq1)
                {
                    //Set freq
                    if(!audioAnaDevice.SetAudioFreq(freq, AFOut_port, FreqUnit.Hz)) throw new System.IO.IOException();
                    Thread.Sleep(DelayAfterSetConfig);
                    clsLogManager.LogReport("Freq: {0} Hz", freq.ToString());
                    double duCurrLevel = audioAnaDevice.GetAudioPowerLevel();
                    double currTolarance = ratioVoltagetodB(duCurrLevel, duDutLevel);
                    dicTolerance.Add(freq, currTolarance);
                    clsLogManager.LogReport("Vref: {0} mV:Vin: {1} mV:Delta: {2} dB", duDutLevel.ToString(), duCurrLevel.ToString(),currTolarance.ToString());
                }
                for (double freq = AFFreq_Start2; freq <= AFFreq_Stop2; freq += StepFreq2)
                {
                    //Set freq
                    if(!audioAnaDevice.SetAudioFreq(freq, AFOut_port, FreqUnit.Hz)) throw new System.IO.IOException();
                    Thread.Sleep(DelayAfterSetConfig);
                    clsLogManager.LogReport("Freq: {0} Hz", freq.ToString());
                    double duCurrLevel = audioAnaDevice.GetAudioPowerLevel();
                    double currTolarance = ratioVoltagetodB(duCurrLevel, duDutLevel);
                    if(dicTolerance.ContainsKey(freq))
                    {
                        dicTolerance.Remove(freq);
                    }    
                    dicTolerance.Add(freq, currTolarance);
                    clsLogManager.LogReport("Vref: {0} mV:Vin: {1} mV:Delta: {2} dB", duDutLevel.ToString(), duCurrLevel.ToString(), currTolarance.ToString());
                }
                currHighTolarance = dicTolerance.Values.Max();
                currLowTolarance = dicTolerance.Values.Min();
                var freqmin = dicTolerance.FirstOrDefault(x => x.Value == currLowTolarance).Key;
                var freqmax = dicTolerance.FirstOrDefault(x => x.Value == currHighTolarance).Key;
                if (currHighTolarance > HighTolarance || currLowTolarance < LowTolarance)
                {
                    
                    clsLogManager.LogReport("Result FAIL: Tolenrance MIN={0} dB at Freq={1} Hz:Tolenrance MAX= {2} at Freq={3} Hz", currLowTolarance.ToString(), freqmin.ToString(), currHighTolarance.ToString(), freqmax.ToString());
                    if(Math.Abs(currHighTolarance)>=Math.Abs(currLowTolarance))
                        m_strDisplayValue = $"{currHighTolarance.ToString("F03")}";
                    else
                        m_strDisplayValue = $"{currLowTolarance.ToString("F03")}";

                    if (clsMsgHelper.ShowYesNo("Question", "Bài đo lỗi, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL;
                    }
                }
                clsLogManager.LogReport("Result PASS: Tolenrance MIN={0} dB at Freq={1} Hz:Tolenrance MAX= {2} dB at Freq={3} Hz", currLowTolarance.ToString(), freqmin.ToString(), currHighTolarance.ToString(), freqmax.ToString());
                if (Math.Abs(currHighTolarance) >= Math.Abs(currLowTolarance))
                    m_strDisplayValue = $"{currHighTolarance.ToString("F03")}";
                else
                    m_strDisplayValue = $"{currHighTolarance.ToString("F03")}";
                return TaskResult.PASS;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Excution: {0}", ex.ToString());
                if (clsMsgHelper.ShowYesNo("Question", "Lỗi quá trình đo, bạn có thực hiện lại") == DialogResult.No)
                {
                    //  m_duDisplayValue = "FAIL";
                    return TaskResult.FAIL;
                }
                else
                {
                    if (DUT != null)
                    {
                        DUT.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_BPB != null)
                            DUT.Connect2Device(deviceinfo_BPB);
                    }
                    if (audioAccess != null)
                    {
                        audioAccess.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_audio != null)
                            audioAccess.Connect2Device(deviceinfo_audio);
                    }
                    goto RETRY_LABEL;
                }
            }
            finally
            {
                audioAnaDevice.EnableOutput(false, AFOut_port);
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
            ClsFPI_AudioFrequencyResponse task = new ClsFPI_AudioFrequencyResponse();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        public double ratioVoltagetodB (double Vin, double Vref)
        {
            return 20 * Math.Log10(Vin / Vref);
        }

        [Category("Audio generator")]
        [DisplayName("AF output")]
        [SaveAtt()]
        public AudioGenIndex AFOut_port { set; get; }
        [Category("Audio generator")]
        [DisplayName("AF level (mV)")]
        [SaveAtt()]
        public double AFLevel { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Delay Before Check(ms)")]
        [SaveAtt()]
        public int DelayBeforeCheck { set; get; }

        [Category("Task Parameter")]
        [DisplayName("Volume Level")]
        [SaveAtt()]
        public double VolumeLevel { set; get; }
        [Category("Task Parameter")]
        [DisplayName("DUT frequency(Hz)")]
        [SaveAtt()]
        public double DUTFrequency { set; get; }
        [Category("Audio generator")]
        [DisplayName("Step Frequency 1(Hz)")]
        [SaveAtt()]
        public double StepFreq1 { set; get; }
        [Category("Audio generator")]
        [DisplayName("Start Frequency 1(Hz)")]
        [SaveAtt()]
        public double AFFreq_Start1 { set; get; } //
        [Category("Audio generator")]
        [DisplayName("Stop Frequency 1(Hz)")]
        [SaveAtt()]
        public double AFFreq_Stop1 { set; get; }
        [Category("Audio generator")]
        [DisplayName("Step Frequency 2(Hz)")]
        [SaveAtt()]
        public double StepFreq2 { set; get; }
        [Category("Audio generator")]
        [DisplayName("Start Frequency 2(Hz)")]
        [SaveAtt()]
        public double AFFreq_Start2 { set; get; } //
        [Category("Audio generator")]
        [DisplayName("Stop Frequency 2(Hz)")]
        [SaveAtt()]
        public double AFFreq_Stop2 { set; get; }
        [Category("Task Parameter")]
        [DisplayName("High Tolerance(dB)")]
        [SaveAtt()]
        public double HighTolarance { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Low Tolerance(dB)")]
        [SaveAtt()]
        public double LowTolarance { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after setting (ms)")]
        [SaveAtt()]
        public int DelayAfterSetConfig { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Control Volume Level")]
        [SaveAtt()]
        public bool CTRL_VolumeLevel { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
