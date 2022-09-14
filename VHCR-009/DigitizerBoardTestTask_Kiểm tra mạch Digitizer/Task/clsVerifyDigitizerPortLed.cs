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
    [clsTaskMetaInfo("Verify digitizer board port led", "clsVerifyDigitizerPortLed", true)]
    public class clsVerifyDigitizerPortLed : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        //private double m_duDisplayValue = double.MaxValue;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Pass_Fail_Value;
        private string m_duDisplayValue = "";
        public clsVerifyDigitizerPortLed()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "Verify digitizer board port led";
            LedIndex = 1;
            TimeOut = 30000;
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
            get { return "clsVerifyDigitizerPortLed"; }
        }
        [Browsable(false)]
        public int TypeMes
        {
            get
            {
                return (int)m_type_testvalue;
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
            return $"{m_duDisplayValue}";
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
            return $"@Led{LedIndex}";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            m_TraceService.WriteLine(" Test Task: " + Description + "\n");
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DIGITIZER_BOARD.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_DIGITIZER = DevManager.GetDevRunTimeInfo(SysDevType.DIGITIZER_BOARD.ToString()) as IDeviceInfor;
            RETRY_WHEN_FAIL:
            try
            {
            
                IDigitizerBoard DutService = (DevManager.GetDevService(SysDevType.DIGITIZER_BOARD.ToString())) as IDigitizerBoard;
                if (DutService == null)
                {
                    clsLogManager.LogError("Can not get device service: IDigitizerBoard");
                    if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_WHEN_FAIL;
                    }
                }
                bool bOK=false;
                bool bIsDevEnable = DevManager.IsDevEnable(SysDevType.DIGITIZER_BOARD.ToString());
               
                if (LedIndex < 0 || LedIndex > 3)
                    throw new ArgumentException($"Led index {LedIndex} out of range [1, 3]");
                bOK = DutService.CheckLedIO(LedIndex,TimeOut, true);
                if (clsMsgHelper.ShowYesNo("Question", $"Led {LedIndex} có sáng không?") == DialogResult.Yes)
                {
                    bOK = true;
                    DutService.CheckLedIO(LedIndex,TimeOut, false);
                }
                else
                {
                    bOK = false;
                }
                //
                if (!bOK)
                {
                    if (clsMsgHelper.ShowYesNo("Question", "Kết quả không đạt, bạn có muốn thử lại không?") == DialogResult.Yes)
                        goto RETRY_WHEN_FAIL;
                }
                if (bOK)
                {
                    m_duDisplayValue = "PASS";
                    return TaskResult.PASS;
                }
                else
                {
                    m_duDisplayValue = "FAIL";
                    return TaskResult.FAIL;
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
            clsVerifyDigitizerPortLed task = new clsVerifyDigitizerPortLed();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        //
        [Category("Test Parameter")]
        [DisplayName("Led Index")]
        [SaveAtt()]
        public int LedIndex { set; get; }
        //
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
