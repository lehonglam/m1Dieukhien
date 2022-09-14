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
using System.Windows.Forms;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.Helper;
using Option;
using System.Threading;

namespace BPBTestTasks
{
    [clsTaskMetaInfo("BPB Check CPRI Function", "ClsBPB_CheckCPRIFunction", true)]
    public class ClsBPB_CheckCPRIFunction : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_strDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Pass_Fail_Value;
        public ClsBPB_CheckCPRIFunction()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "BPB Check CPRI Function";
            RetryTimes = 3;
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
            get { return "ClsBPB_CheckCPRIFunction"; }
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
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DUT.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_BPB = DevManager.GetDevRunTimeInfo(SysDevType.DUT.ToString()) as IDeviceInfor;
            RETRY_LABEL:
            try
            {
               
                m_strDisplayValue = "FAIL";
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
                bool bOk = false;
                for (int i = 0; i < RetryTimes; i++)
                {
                   bOk= bPBDevice.CheckCPRIPort();
                    if (bOk)
                    {
                        break;
                    }
                }
                if (!bOk)
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
                    if (DUT != null)
                    {
                        DUT.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_BPB != null)
                            DUT.Connect2Device(deviceinfo_BPB);
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
            ClsBPB_CheckCPRIFunction task = new ClsBPB_CheckCPRIFunction();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }

        [Category("Task Parameter")]
        [DisplayName("Number Of Retry")]
        [SaveAtt()]
        public int RetryTimes { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
