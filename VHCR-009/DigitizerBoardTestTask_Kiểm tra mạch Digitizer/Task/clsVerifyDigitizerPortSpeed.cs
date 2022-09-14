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
    [clsTaskMetaInfo("Verify digitizer board port speed", "clsVerifyDigitizerPortSpeed", true)]
    public class clsVerifyDigitizerPortSpeed : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_duDisplayValue = "FAIL";
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Pass_Fail_Value;
        public clsVerifyDigitizerPortSpeed()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "Verify digitizer board port speed";
            PortType = "SFP";
            RS485PortName = string.Empty;
            TimeOut = 30000;
            NumberOfByteToSend = 50;
            Result_unit = "";
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

        [Category("#General Option")]
        [Browsable(false)]
        public string Name
        {
            get { return "clsVerifyDigitizerPortSpeed"; }
        }
        [Category("#General Option")]
        [DisplayName("Type Mes Value")]
        [SaveAtt()]
        public int TypeMes
        {
            get
            {
                return (int)m_type_testvalue;
            }
            set
            {
                m_type_testvalue = (Type_TestValue)value ;
            }
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
            return $"{Result_unit}";
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
            return m_duDisplayValue;
        }
        public string GetDisplayMaxValue()
        {
            return "NA";
        }
        public string GetDisplayMinValue()
        {
            return "NA";
        }
        public string GetAddTaskDescription()
        {
            return $"";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            m_TraceService.WriteLine(" Test Task: " + Description + "\n");
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DIGITIZER_BOARD.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_DIGITIZER = DevManager.GetDevRunTimeInfo(SysDevType.DIGITIZER_BOARD.ToString()) as IDeviceInfor;
            RETRY_WHEN_FAIL:
            try
            {
            
                m_duDisplayValue = "FAIL";
                IDigitizerBoard DutService = (DevManager.GetDevService(SysDevType.DIGITIZER_BOARD.ToString())) as IDigitizerBoard;
                if (DutService == null)
                {
                    clsLogManager.LogError("Can not get device service: IDigitizerBoard");
                    if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        taskResult = TaskResult.FAIL;
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_WHEN_FAIL;
                    }
                }
                bool bOK=false;
                bool bIsDevEnable = DevManager.IsDevEnable(SysDevType.DIGITIZER_BOARD.ToString());
               
                if (PortType.Equals("SFP"))
                {
                    bOK = DutService.CheckSFPPort(TimeOut);
                }
                else if (PortType.Equals("FMC"))
                {
                    bOK = DutService.CheckFMCPort(TimeOut);
                }
                else if (PortType.Equals("GPS"))
                {
                    bOK = DutService.CheckGPSData(TimeOut);
                }
                else if (PortType.Equals("USB"))
                {
                    bOK = DutService.CheckUsbUart(TimeOut);
                }
                else if (PortType.Equals("RS485"))
                {
                    if (string.IsNullOrEmpty(RS485PortName))
                        throw new ArgumentException("RS485 port name is NULL or empty!!!");
                    bOK = DutService.CheckRS485Port(RS485PortName, TimeOut, NumberOfByteToSend, NumberOfByteToReceive);
                }
                else if (PortType.Equals("RS485 Pantilt"))
                {
                    if (string.IsNullOrEmpty(RS485PortName))
                        throw new ArgumentException("RS485 pantilt port name is NULL or empty!!!");
                    bOK = DutService.CheckRS485_Pantilt(RS485PortName, TimeOut, NumberOfByteToSend, NumberOfByteToReceive);
                }
                else
                    throw new ArgumentException($"Unknow port type {PortType}");
                //
                if(!bOK)
                {
                    if (clsMsgHelper.ShowYesNo("Question", "Kết quả không đạt, bạn có muốn thử lại không?") == DialogResult.Yes)
                        goto RETRY_WHEN_FAIL;
                }
                if (bOK)
                {
                    m_duDisplayValue = "PASS";
                    taskResult = TaskResult.PASS;
                    return TaskResult.PASS;
                }
                else
                {
                    taskResult = TaskResult.FAIL;
                    return TaskResult.FAIL;
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
                        if (deviceinfo_DIGITIZER != null)
                            DUT.Connect2Device(deviceinfo_DIGITIZER);
                    }
                    goto RETRY_WHEN_FAIL;
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
            clsVerifyDigitizerPortSpeed task = new clsVerifyDigitizerPortSpeed();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        //
        [Category("Test Parameter")]
        [DisplayName("Port Type")]
        [TypeConverter(typeof(clsDigitizerBoardPortTc))]
        [SaveAtt()]
        public string PortType { set; get; }
        //
        [Category("Test Parameter")]
        [DisplayName("RS485 Port Name")]
        [TypeConverter(typeof(clsComPortNameTc))]
        [SaveAtt()]
        public string RS485PortName { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Number Of Byte To Send(Check RS485-USB)")]
        [SaveAtt()]
        public int NumberOfByteToSend { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Number Of Byte To Send(Check USB-RS485)")]
        [SaveAtt()]
        public int NumberOfByteToReceive { set; get; }

        [Category("Test Parameter")]
        [DisplayName("TimeOut(ms)")]
        [SaveAtt()]
        public int TimeOut { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
