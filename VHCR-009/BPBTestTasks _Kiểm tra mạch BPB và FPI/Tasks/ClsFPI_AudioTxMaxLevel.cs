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
    [clsTaskMetaInfo("FPI Audio Tx Max Level", "ClsFPI_AudioTxMaxLevel", true)]
    public class ClsFPI_AudioTxMaxLevel : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_strDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        public ClsFPI_AudioTxMaxLevel()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "FPI Audio Tx Max Level";
            CTRL_VolumeLevel = false;
            AudioFrequency = 1000;
            AFLevel = 15;
            AFOut_port = AudioGenIndex.GEN_1;
            RetryTimes = 3;
            DelayBeforeCheckLevel = 1000;
            AudioLevelLimit = 7;
            VolumeLevel = 8;
            Result_unit = "V";
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
            get { return "ClsFPI_AudioTxMaxLevel"; }
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
            return "NA";
        }

        public string GetMinValue()
        {
            return $"{AudioLevelLimit.ToString("F03")}";
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
            return "NA";
        }
        public string GetDisplayMinValue()
        {
            return $"{AudioLevelLimit.ToString("F03")} {Result_unit}";
        }
        public TaskResult GetTaskResult()
        {
            return taskResult;
        }
        public string GetAddTaskDescription()
        {
            return string.Empty;
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
                //bOk = audioAnaDevice.SetAF_GenAndMes();
                bOk = audioAnaDevice.SetMultitone(false);
                bOk &= audioAnaDevice.SetAudioLevel(AFLevel, AFOut_port);
                bOk &= audioAnaDevice.SetAudioFreq(AudioFrequency, AFOut_port, FreqUnit.Hz);
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

                if (DelayBeforeCheckLevel > 0)
                {
                    clsLogManager.LogWarning("DelayBeforeCheck: {0}ms...", DelayBeforeCheckLevel);
                    Thread.Sleep(DelayBeforeCheckLevel);
                }
                double duMaxAuLevel = 0;
                for (int i = 0; i < RetryTimes; i++)
                {
                    duMaxAuLevel = audioAnaDevice.GetAudioPowerLevel(AudioLevelUnit.MV, 1, true)/1000;
                    m_strDisplayValue = $"{duMaxAuLevel.ToString("F03")}";
                    if (duMaxAuLevel >= AudioLevelLimit)
                        break;
                    Thread.Sleep(DelayBeforeCheckLevel);
                }
                if (duMaxAuLevel >= AudioLevelLimit)
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
            ClsFPI_AudioTxMaxLevel task = new ClsFPI_AudioTxMaxLevel();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }

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
        [DisplayName("Audio Level Limit(V)")]
        [SaveAtt()]
        public double AudioLevelLimit { get; set; }
        [Category("Task Parameter")]
        [DisplayName("Volume Level")]
        [SaveAtt()]
        public double VolumeLevel { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Audio Frequency(Hz)")]
        [SaveAtt()]
        public double AudioFrequency { set; get; }
        [Category("Audio generator")]
        [DisplayName("AF level (mV)")]
        [SaveAtt()]
        public double AFLevel { set; get; }
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
