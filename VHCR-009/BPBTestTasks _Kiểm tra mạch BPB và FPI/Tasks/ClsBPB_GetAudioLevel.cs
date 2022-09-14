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

namespace BPBTestTasks
{
    [clsTaskMetaInfo("BPB Get Audio Level", "ClsBPB_GetAudioLevel", true)]
    public class ClsBPB_GetAudioLevel : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_strDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        public ClsBPB_GetAudioLevel()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "BPB Get Audio Level";
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
            get { return "ClsBPB_GetAudioLevel"; }
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
            return $"{m_strDisplayValue} {Result_unit}";
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
            return string.Empty;
        }

        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            try
            {
                m_strDisplayValue = "NA";
                IAudioAnaDevice audioAnaDevice = (IAudioAnaDevice)DevManager.GetDevService(SysDevType.AUDIO_ANA.ToString());

                if (audioAnaDevice == null)
                {
                    clsLogManager.LogError("Can not get device service: audioAnaDevice");
                    return TaskResult.FAIL;
                }
                if (!((IAccessDeviceService)audioAnaDevice).IsDeviceConnected())
                {
                    clsLogManager.LogError("Device audioAnaDevice is not connected");
                    return TaskResult.FAIL;
                }
                double duMaxAuLevel = audioAnaDevice.GetAudioPowerLevel(AudioLevelUnit.V, 1, true);
                m_strDisplayValue = duMaxAuLevel.ToString("00.000");// + "dBm";
                return TaskResult.PASS;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Excution: {0}", ex.ToString());
                taskResult = TaskResult.FAIL;
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
            ClsBPB_GetAudioLevel task = new ClsBPB_GetAudioLevel();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
