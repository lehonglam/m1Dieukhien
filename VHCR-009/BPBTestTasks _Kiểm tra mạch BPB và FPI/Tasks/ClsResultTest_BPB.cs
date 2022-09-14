using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CFTWinAppCore.Task;
using CFTWinAppCore.Sequences;
using CFTWinAppCore.DataLog;
using System.ComponentModel;
using LogLibrary;
using System.Threading;
using GeneralTool;
using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.DeviceManager.DUTIOService;
using System.Windows.Forms;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.Helper;
using Option;
using BPBTestTasks;
namespace BPBTestTasks
{
    [clsTaskMetaInfo("Ket qua kiem tra", "ClsResultTest_BPB", true)]
    public class ClsResultTest_BPB : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_duDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Pass_Fail_Value;
        public ClsResultTest_BPB()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "Ket qua kiem tra";
            typeCommunication = TypeCommunication.BPB_FPI_E1;
        }
        public enum TypeCommunication
        {
            BPB_FPI_E1,
            BPB_FPI_Ethernet
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
            get { return "ClsResultTest_BPB"; }
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
            return $"{m_duDisplayValue}";
        }

        public string GetMaxValue()
        {
            return "NA";
        }

        public string GetMinValue()
        {
            return "NA";
        }

        public string GetUnit()
        {
            return "";
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
            //if (m_duDisplayValue < 1)
            //    return m_duDisplayValue.ToString("0.##");
            //else
            return m_duDisplayValue;
        }
        public string GetDisplayMaxValue()
        {
            //return string.Format("{0} V", Maxpass.ToString());
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
            return "";
        }

        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            try
            {
                RETRY_LABEL:
                m_TraceService.WriteLine("Begin Test Task: " + m_strDescription + string.Format("[{0}]", GetAddTaskDescription()));
                List<ITask> lstTask = m_IModuleTask.Tasks;
                bool testResult = true;
                bool test = false;
                if (typeCommunication == TypeCommunication.BPB_FPI_E1)
                {
                    ClsBPB_CheckE1Function taskE1 = null;
                    foreach(ITask task in m_IModuleTask.Tasks)
                    {
                        taskE1 = task as ClsBPB_CheckE1Function;
                        if (taskE1 == null)
                            continue;
                        else
                        {
                            m_TraceService.WriteLine($"Task  {taskE1.Description}[{taskE1.GetAddTaskDescription()}] : {taskE1.GetTaskResult()}");
                            if (taskE1.GetTaskResult() == TaskResult.FAIL)
                                test = false;
                            else
                                test = true;
                            testResult &= testResult & test;
                        }    
                    }    
                }
                else if (typeCommunication==TypeCommunication.BPB_FPI_Ethernet)
                {
                    ClsBPB_CheckEthernetFunction taskEthernet = null;
                    foreach (ITask task in m_IModuleTask.Tasks)
                    {
                        taskEthernet = task as ClsBPB_CheckEthernetFunction;
                        if (taskEthernet == null)
                            continue;
                        else
                        {
                            m_TraceService.WriteLine($"Task  {taskEthernet.Description}[{taskEthernet.GetAddTaskDescription()}] : {taskEthernet.GetTaskResult()}");
                            if (taskEthernet.GetTaskResult() == TaskResult.FAIL)
                                test = false;
                            else
                                test = true;
                            testResult &= testResult & test;
                        }
                    }
                }
                if (testResult == true)
                {
                    m_duDisplayValue = "PASS";
                    m_TraceService.WriteLine("Result test = PASS");
                    taskResult = TaskResult.PASS ;
                    return taskResult;
                }
                else
                {
                    m_duDisplayValue = "FAIL";
                    m_TraceService.WriteLine("Result test = FAIL");
                    taskResult = TaskResult.FAIL;
                    return taskResult;
                }
                    
                    
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Excution: {0}", ex.ToString());
                return TaskResult.FAIL;
            }
            finally
            {

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
            ClsResultTest_BPB task = new ClsResultTest_BPB();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }

        [Category("Test parameters")]
        [DisplayName("Loại giao tiếp")]
        [SaveAtt()]
        public TypeCommunication typeCommunication { set; get; }

        #region Private function

        #endregion
    }
}
