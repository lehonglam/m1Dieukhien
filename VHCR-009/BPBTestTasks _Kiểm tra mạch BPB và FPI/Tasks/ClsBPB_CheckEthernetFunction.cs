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
    [clsTaskMetaInfo("BPB Check Ethernet Function", "ClsBPB_CheckEthernetFunction", true)]
    public class ClsBPB_CheckEthernetFunction : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_strDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Setup;
        public ClsBPB_CheckEthernetFunction()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "BPB Check Ethernet Function";
            ETHERNET_PORT = CFTWinAppCore.DeviceManager.DUT.ETHERNET_PORT.PORT1;
            TimeOut = 30;
            IPAddress = "192.168.1.100";
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
            get { return "ClsBPB_CheckEthernetFunction"; }
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
            return ETHERNET_PORT.ToString();
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
                        taskResult = TaskResult.FAIL;
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
                        taskResult = TaskResult.FAIL;
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL;
                    }
                }
                bool bOk = false;
                MessageBox.Show("Kết nối Ethernet Port "+ETHERNET_PORT.ToString()+" với cổng LAN máy tính có IP="+IPAddress.ToString());
                int BeginTime=Environment.TickCount;
                while(true)
                {
                    IPAddress iPAddress = bPBDevice.CheckEthernetPort(ETHERNET_PORT);
                    if (iPAddress != null)
                    {
                        string IPCurr= iPAddress.ToString(); 
                        if(IPCurr == IPAddress)
                        {
                            m_strDisplayValue = "PASS";
                            taskResult = TaskResult.PASS;
                            return TaskResult.PASS;
                        }
                        else
                        {
                            goto CHECK_TIMEOUT;
                        }
                    }
                    else
                    {
                        m_strDisplayValue = "FAIL";
                    }
                    CHECK_TIMEOUT:
                    if(Environment.TickCount - BeginTime>=TimeOut*1000)
                    {
                        clsLogManager.LogError("Timeout to check ethernet port {0}", ETHERNET_PORT.ToString());
                        m_strDisplayValue = "FAIL";
                        break;
                       // return TaskResult.FAIL;
                    }
                    System.Threading.Thread.Sleep(DelayBetweenRetry*1000);
                }
                if (clsMsgHelper.ShowYesNo("Question", "Bài đo lỗi, bạn có thực hiện lại không?") == DialogResult.No)
                {
                    taskResult = TaskResult.FAIL;
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
                    taskResult = TaskResult.FAIL;
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
            ClsBPB_CheckEthernetFunction task = new ClsBPB_CheckEthernetFunction();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }

        [Category("Task Parameter")]
        [DisplayName("ETHERNET PORT")]
        [SaveAtt()]
        public ETHERNET_PORT ETHERNET_PORT { set; get; }
        [Category("Task Parameter")]
        [DisplayName("TimeOut To Check(s)")]
        [SaveAtt()]
        public int TimeOut { set; get; }
        [Category("Task Parameter")]
        [DisplayName("IP Address")]
        [SaveAtt()]
        public string IPAddress { set; get; }
        [Category("Task Parameter")]
        [DisplayName("Delay Between Retry(s)")]
        [SaveAtt()]
        public int DelayBetweenRetry { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
