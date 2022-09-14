using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using CFTWinAppCore.Task;
using CFTWinAppCore.Sequences;
using CFTWinAppCore.DataLog;
using LogLibrary;
using GeneralTool;
using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.DeviceManager.DUTIOService;
using DigitizerBoardTestTask.TypeConverters;
using System.Windows.Forms;
using Option;
namespace DigitizerBoardTestTask.Task
{
    [clsTaskMetaInfo("Testdynamictask", "clstestdynamictask", true)]
    public class clstestdynamictask : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_duDisplayValue = "FAIL";
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Setup;
        public clstestdynamictask()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "Verify digitizer board led on";
        }
        [Browsable(false)]
        public bool SuperAdmin
        {
            get;
            set;
        }
        public TaskResult GetTaskResult()
        {
            return taskResult;
        }

        [Category("General Option")]
        [Browsable(false)]
        public string Name
        {
            get { return "clstestdynamictask"; }
        }

        public int TypeMes
        {
            get
            {
                return (int)m_type_testvalue;
            }
        }
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
        public string GetValue()
        {
            return m_duDisplayValue;
        }

        public string GetMaxValue()
        {
            return "-";
        }

        public string GetMinValue()
        {
            return "-";
        }

        public string GetUnit()
        {
            return Result_unit;
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
        [Category("General Option")]
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
        [Category("General Option")]
        [DisplayName("Allow Excution")]
        [SaveAtt()]
        public bool AllowExcution
        {
            set;
            get;
        }

        public string GetDisplayValue()
        {
            return m_duDisplayValue;
        }
        public string GetDisplayMaxValue()
        {
            return "-";
        }
        public string GetDisplayMinValue()
        {
            return "-";
        }
        public string GetAddTaskDescription()
        {
            return string.Empty;
        }
		public string GetScriptName()
		{
			return "Test task";
		}
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            try
            {
                m_duDisplayValue = "FAIL";
                IDigitizerBoard DutService = (IDigitizerBoard)(DevManager.GetDevService(SysDevType.DIGITIZER_BOARD.ToString()));
                bool bOK=false;
                bool bIsDevEnable = DevManager.IsDevEnable(SysDevType.DIGITIZER_BOARD.ToString());
                bOK = DutService.CheckLedOn();
                if (clsMsgHelper.ShowYesNo("Question", "Tất cả LED có sáng không?") == DialogResult.Yes)
                {
                    bOK = true;
                }
                else
                {
                    bOK = false;
                }
                if (bOK)
                {
                    m_duDisplayValue = "PASS";
                    return TaskResult.PASS;
                }
                else
                    return TaskResult.FAIL;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Excution: {0}", ex.ToString());
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
            clstestdynamictask task = new clstestdynamictask();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
