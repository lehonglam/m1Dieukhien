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
    [clsTaskMetaInfo("BPB Check Audio Level", "ClsBPB_CheckAudioLevel", true)]
    public class ClsBPB_CheckAudioLevel : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_strDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        public ClsBPB_CheckAudioLevel()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "BPB Check Audio Level";
            RetryTimes = 3;
            AUDIO_LOOP_MODE = AUDIO_LOOP_MODE.LOOP;
            DelayAfterStartAudio = 0;
            MinLimit = 5;
            AudioFrequency = 1000;
            VolumeLevel = 8;
            AFOut_port = AudioGenIndex.GEN_1;
            AFLevel = 15;
            Result_unit = "V";
            CTRL_VolumeLevel = false;
            //ReferencePowerLevel = 10000;
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
            get { return "ClsBPB_CheckAudioLevel"; }
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
            return $"{MinLimit.ToString(".000")}";
        }

        public string GetUnit()
        {
            return Result_unit;
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
            return "NA";
        }
        public string GetDisplayMinValue()
        {
            return $"{MinLimit.ToString(".000")} {Result_unit}";
        }
        public TaskResult GetTaskResult()
        {
            return taskResult;
        }
        public string GetAddTaskDescription()
        {
            return AUDIO_LOOP_MODE.ToString();
        }

        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            IBPBDevice bPBDevice = DevManager.GetDevService(SysDevType.DUT.ToString())as IBPBDevice;
            IAudioAnaDevice audioAnaDevice = DevManager.GetDevService(SysDevType.AUDIO_ANA.ToString()) as IAudioAnaDevice;
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_BPB = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_audio= DevManager.GetDevRunTimeInfo(SysDevType.AUDIO_ANA.ToString()) as IDeviceInfor;
            IAccessDeviceService audioAccess = DevManager.GetDevService(SysDevType.AUDIO_ANA.ToString()) as IAccessDeviceService;
            RETRY_LABEL:
            try
            {
                m_strDisplayValue = "NA";
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
                if(!audioAnaDevice.SetMultitone(false)) throw new System.IO.IOException();
                if(!audioAnaDevice.SetAudioLevel(AFLevel, AFOut_port)) throw new System.IO.IOException();
                if(!audioAnaDevice.SetAudioFreq(AudioFrequency, AFOut_port, FreqUnit.Hz)) throw new System.IO.IOException();
                if(!audioAnaDevice.EnableOutput(true, AFOut_port)) throw new System.IO.IOException();
                //ISpecDevice specDevice = (ISpecDevice)DevManager.GetDevService(SysDevType.SPEC_DEV.ToString());
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
                if (audioAnaDevice == null)
                {
                    clsLogManager.LogError("Can not get device service: audioAnaDevice");
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
                //audioAnaDevice.AudioAnaSetup();
                //double dudBm = clsRFPowerUnitHelper.Vol2Dbm(ReferencePowerLevel / 1000);
                //clsLogManager.LogWarning("Expected Power Level: {0}mV -> {1}dBm", ReferencePowerLevel, dudBm);
                //specDevice.SetSpecExpectPower(dudBm);
                //specDevice.SetReferentLevel(dudBm);
                if (SleepTimeBeforeTest > 0)
                {
                    clsLogManager.LogReport("Sleep before test: {0}ms...", SleepTimeBeforeTest);
                    Thread.Sleep(SleepTimeBeforeTest);
                }
                bool bOk = false;
                double duMaxAuLevel = -1;
                for (int j = 0; j < RetryTimes; j++)
                {
                    bOk = bPBDevice.SetAudioLoop(AUDIO_LOOP_MODE);
                    if (!bOk)
                    {
                        m_strDisplayValue = "NA";
                        continue;
                    }
                    Thread.Sleep(1000);
                    break;
                }
                if (CTRL_VolumeLevel == true)
                {
                  for (int j = 0; j < RetryTimes; j++)
                    {
                            bOk = bPBDevice.SetAudioVolume((byte)VolumeLevel);
                            if (!bOk)
                            {
                                m_strDisplayValue = "NA";
                                continue;
                            }
                            Thread.Sleep(1000);
                            break;
                    }
                }
                    Thread.Sleep(DelayAfterStartAudio);

                    for (int j = 0; j < RetryTimes; j++)
                    {
                        duMaxAuLevel = audioAnaDevice.GetAudioPowerLevel(AudioLevelUnit.V, 1, true);
                        m_strDisplayValue = duMaxAuLevel.ToString("F03");// + "dBm";
                        if (MinLimit <= duMaxAuLevel)
                            break;
                    }
                if ((MinLimit > duMaxAuLevel)|(!bOk))
                {
                    if (clsMsgHelper.ShowYesNo("Question", "Bài đo lỗi, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL;
                    }
                }

                if (MinLimit <= duMaxAuLevel)
                {
                    return TaskResult.PASS;
                }
                return TaskResult.FAIL;
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
            ClsBPB_CheckAudioLevel task = new ClsBPB_CheckAudioLevel();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }

        [Category("Task Parameter")]
        [DisplayName("Number Of Retry")]
        [SaveAtt()]
        public int RetryTimes { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Min limit(V)")]
        [SaveAtt()]
        public double MinLimit { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Audio Frequency(Hz)")]
        [SaveAtt()]
        public double AudioFrequency { set; get; }
        [Category("Task Parameter")]
        [DisplayName("AUDIO LOOP MODE")]
        [SaveAtt()]
        public AUDIO_LOOP_MODE AUDIO_LOOP_MODE { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Delay After Start Audio(ms)")]
        [SaveAtt()]
        public int DelayAfterStartAudio { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Sleep Time Before Test(ms)")]
        [SaveAtt()]
        public int SleepTimeBeforeTest { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Volume Level")]
        [SaveAtt()]
        public double VolumeLevel { set; get; }
        [Category("Audio generator")]
        [DisplayName("AF output")]
        [SaveAtt()]
        public AudioGenIndex AFOut_port { set; get; }
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
