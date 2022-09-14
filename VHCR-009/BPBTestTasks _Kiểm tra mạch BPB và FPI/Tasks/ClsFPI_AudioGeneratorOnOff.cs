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
    [clsTaskMetaInfo("FPI Audio Generator On Off", "ClsFPI_AudioGeneratorOnOff", true)]
    public class ClsFPI_AudioGeneratorOnOff : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_strDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Setup;
        public ClsFPI_AudioGeneratorOnOff()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "FPI Audio Generator On Off";
            AFOut_port = AudioGenIndex.GEN_1;
            EnableOutput = false;
            Result_unit = "";
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
            get { return "ClsFPI_AudioGeneratorOnOff"; }
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
            return $"NA";
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
            return m_strDisplayValue;
        }
        public string GetDisplayMaxValue()
        {
            return "NA";
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
            if (EnableOutput) return "ON";
            return "OFF";
        }

        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            m_TraceService.WriteLine(" Test Task: " + Description + "\n");
            IDeviceInfor deviceinfo_audio = DevManager.GetDevRunTimeInfo(SysDevType.AUDIO_ANA.ToString()) as IDeviceInfor;
            IAccessDeviceService audioAccess = DevManager.GetDevService(SysDevType.AUDIO_ANA.ToString()) as IAccessDeviceService;
            RETRY_LABEL:
            m_strDisplayValue = "FAIL";
            IAudioAnaDevice audioAnaDevice =DevManager.GetDevService(SysDevType.AUDIO_ANA.ToString()) as IAudioAnaDevice;
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
            try
            {
                bool bOk = audioAnaDevice.EnableOutput(EnableOutput, AFOut_port);
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
                m_strDisplayValue = "PASS";
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
            ClsFPI_AudioGeneratorOnOff task = new ClsFPI_AudioGeneratorOnOff();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }

        [Category("Audio generator")]
        [DisplayName("AF output")]
        [SaveAtt()]
        public AudioGenIndex AFOut_port { set; get; }
        [Category("Audio generator")]
        [DisplayName("Enable Output")]
        [SaveAtt()]
        public bool EnableOutput { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //

    }
}
