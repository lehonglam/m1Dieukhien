using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using CFTWinAppCore.Task;
using CFTWinAppCore.Sequences;
using CFTWinAppCore.DataLog;
using CFTWinAppCore.DeviceManager;
using LogLibrary;
using GeneralTool;
using CFTWinAppCore.Helper;
using System.Windows.Forms;
using VHCRTestTask.TypeConvert;
using CFTWinAppCore.DeviceManager.DUTIOService;
using CFTSeqManager.DUT;
using Option;
using CFTWinAppCore.DeviceManager.DUT;

namespace VHCRTestTask.Tasks.VHCR
{
    [clsTaskMetaInfo("VHCR - Setup Rx", "clsVHCR_Rx_Setup", true)]
    public class clsVHCR_Rx_Setup : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        public string m_duDisplayValue = "";
        private Type_TestValue m_type_testvalue = Type_TestValue.Setup;
        clsFreqTypeConv FreqTypeConv = new clsFreqTypeConv();


        public clsVHCR_Rx_Setup()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - Rx Setup";

            DelayAfterSetConfig = 1000;
        }
        [Category("#General Option")]
        [Browsable(false)]
        public string Name
        {
            get { return "clsVHCR_Rx_Setup"; }
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
        [Category("#General Option")]
        [Browsable(false)]
        public int TypeMes
        {
            get
            {
                return (int)m_type_testvalue;
            }
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
        public string GetValue()
        {
            return m_duDisplayValue;
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
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            ISignalGenDeviceE8267D SigGenService_2nd = null;
            ISignalGenDevice SigGenService = null;
            string strMsg = null;
            m_duDisplayValue = "FAIL";
            m_TraceService.WriteLine("Begin Test Task: " + m_strDescription + string.Format("[{0}]", GetAddTaskDescription()));
            RETRY_LABEL:
            try
                {
                    IDeviceInfor deviceinfor2 = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN_2nd.ToString()) as IDeviceInfor;
                    IDeviceInfor deviceinfor = DevManager.GetDevRunTimeInfo(SysDevType.SIG_GEN.ToString()) as IDeviceInfor ;
                    if (deviceinfor2!=null)
                    {
                        SigGenService_2nd = DevManager.GetDevService(SysDevType.SIG_GEN_2nd.ToString()) as ISignalGenDeviceE8267D;
                        if (SigGenService_2nd != null)
                        {
                            SigGenService_2nd.EnableOutput(false);
                        SigGenService_2nd.SetModulationType(ModulatioType.OFF);
                            clsLogManager.LogReport("Turn Off Signal Generator 2");
                        }
                        else
                        {
                            clsLogManager.LogError("Can not get device service: ISignalGenDeviceE8267D for SigGen2");
                            if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                             goto RETRY_LABEL;
                            }
                    }
                    }
                    if (deviceinfor!=null)
                    {
                        SigGenService = DevManager.GetDevService(SysDevType.SIG_GEN.ToString()) as ISignalGenDevice;
                        if (SigGenService != null)
                        {
                            SigGenService.EnableOutput(false);
                            clsLogManager.LogReport("Turn Off Signal Generator 1");
                        }
                        else
                        {
                            clsLogManager.LogError("Can not get device service: ISignalGenDeviceE8267D for SigGen1");
                            if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                            {
                                return TaskResult.FAIL;
                            }
                            else
                            {
                                goto RETRY_LABEL;
                            }
                    }
                    }
                //MessageBox.Show("chuyển DUT sang chế độ thu");
                clsLogManager.LogWarning(Description + ": PASS");
                m_duDisplayValue = "PASS";
                return TaskResult.PASS;
                }
                catch (System.Exception ex)
                {
                    m_duDisplayValue = "FAIL";
                    strMsg = "Exception: " + ex.Message;
                    clsLogManager.LogError("Excution: {0}", ex.ToString());

                }
                finally
                {
                    //off siggen
                    if (SigGenService_2nd != null)
                    {
                        SigGenService_2nd.EnableOutput(false);
                    }
                    if (SigGenService != null)
                    {
                        SigGenService.EnableOutput(false);
                    }

                }
            m_duDisplayValue = "FAIL";
            return TaskResult.FAIL;
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
            clsVHCR_Rx_Setup task = new clsVHCR_Rx_Setup();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }


        #region Test Parameter

        [Category("Test Parameter")]
        [DisplayName("Delay after setting (ms)")]
        [SaveAtt()]
        public int DelayAfterSetConfig { set; get; }

        [Category("Test Parameter")]
        [DisplayName("TimeOut Test Task (ms)")]
        [SaveAtt()]
        public int TimeOut { set; get; }

        #endregion       
    }
}
