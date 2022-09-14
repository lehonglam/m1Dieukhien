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
    [clsTaskMetaInfo("FPI Check Led", "ClsFPI_CheckLed", true)]
    public class ClsFPI_CheckLed : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_strDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Pass_Fail_Value;
        public ClsFPI_CheckLed()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "FPI Check Led";
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
            get { return "ClsFPI_CheckLed"; }
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
            return m_strDisplayValue.ToString();
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
            m_TraceService.WriteLine(" Test Task: " + Description + "\n");
            try
            {
                RETRY_LABEL:
                m_strDisplayValue = "FAIL";
                System.Windows.Forms.DialogResult dialogResult = clsMsgHelper.ShowYesNo("Check Led", "3 LED mặt máy và 4 LED cổng E1 có nháy không?");
                //System.Windows.Forms.DialogResult dialogResult= clsYesNoBox.ShowMsg("Check Led", "Các đèn báo hiệu có sáng?");
                if(dialogResult==System.Windows.Forms.DialogResult.OK||dialogResult== System.Windows.Forms.DialogResult.Yes)
                {
                    m_strDisplayValue = "PASS";
                    return TaskResult.PASS;
                }
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
            ClsFPI_CheckLed task = new ClsFPI_CheckLed();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
