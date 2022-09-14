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
    [clsTaskMetaInfo("FPI Audio Tx Distortion", "ClsFPI_AudioTxDistortion", true)]
    public class ClsFPI_AudioTxDistortion : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_strDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        public ClsFPI_AudioTxDistortion()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "FPI Audio Tx Distortion";

            AudioFrequency = 1000;
            AFLevel = 15;
            AFOut_port = AudioGenIndex.GEN_1;
            RetryTimes = 3;
            DelayBeforeCheckLevel = 0;
            DistortionLimit = 3;
            Result_unit = "%";
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
            get { return "ClsFPI_AudioTxDistortion"; }
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
            return $"{DistortionLimit.ToString("F03")}";
        }

        public string GetMinValue()
        {
            return "NA";
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
            return $"{m_strDisplayValue} {Result_unit}";
        }
        public string GetDisplayMaxValue()
        {
            return $"{DistortionLimit.ToString("F03")} {Result_unit}";
        }
        public string GetDisplayMinValue()
        {
            return "NA";
        }
        public TaskResult GetTaskResult()
        {
            return taskResult;
        }
        public string GetAddTaskDescription()
        {
            return $"{AFLevel}mV, {AudioFrequency}Hz";
        }

        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            m_TraceService.WriteLine(" Test Task: " + Description + "\n");
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_BPB = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_audio = DevManager.GetDevRunTimeInfo(SysDevType.AUDIO_ANA.ToString()) as IDeviceInfor;
            IAccessDeviceService audioAccess = DevManager.GetDevService(SysDevType.AUDIO_ANA.ToString()) as IAccessDeviceService;
            RETRY_LABEL:
            m_strDisplayValue = "N/A";
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
                //bool bOk = audioAnaDevice.SetAF_GenAndMes();
                bool bOk = audioAnaDevice.SetMultitone(false);
                bOk &=  audioAnaDevice.SetAudioLevel(AFLevel, AFOut_port);
                bOk &=  audioAnaDevice.SetAudioFreq(AudioFrequency, AFOut_port, FreqUnit.Hz);
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
                if(DelayBeforeCheckLevel>0)
                {
                    clsLogManager.LogWarning("DelayBeforeCheck: {0}ms...", DelayBeforeCheckLevel);
                    Thread.Sleep(DelayBeforeCheckLevel);
                }
                double duDistortion = double.MaxValue;

                for (int i = 0; i < RetryTimes; i++)
                {
                    duDistortion = audioAnaDevice.GetAudioDistorLevel();
                    m_strDisplayValue = $"{duDistortion.ToString("F03")}";
                    if (duDistortion<=DistortionLimit)
                        break;
                    Thread.Sleep(DelayBeforeCheckLevel);
                }
                if (duDistortion <= DistortionLimit)
                    return TaskResult.PASS;

                if (clsMsgHelper.ShowYesNo("Question", "Bài đo lỗi, bạn có thực hiện lại không?") == DialogResult.No)
                {
                    return TaskResult.FAIL;
                }
                else
                {
                    goto RETRY_LABEL;
                }
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
            ClsFPI_AudioTxDistortion task = new ClsFPI_AudioTxDistortion();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        [Category("Task Parameter")]
        [DisplayName("Audio Frequency(Hz)")]
        [SaveAtt()]
        public double AudioFrequency { set; get; }
        [Category("Audio generator")]
        [DisplayName("AF level (mV)")]
        [SaveAtt()]
        public double AFLevel { set; get; }

        [Category("Audio generator")]
        [DisplayName("AF output")]
        [SaveAtt()]
        public AudioGenIndex AFOut_port { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Delay Before CheckLevel(ms)")]
        [SaveAtt()]
        public int DelayBeforeCheckLevel { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Retry Times")]
        [SaveAtt()]
        public int RetryTimes { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Distortion Limit(%)")]
        [SaveAtt()]
        public double DistortionLimit { get; set; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //

    }
}
