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
using CFTWinAppCore.DeviceManager.PowerDevice;
using System.Windows.Forms;
using VHCRTestTask.TypeConvert;
using CFTSeqManager.DUT;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.DeviceManager.DUTIOService;
using Option;
//using System.Windows.Forms;

namespace VHCRTestTask.Tasks.VHCR
{
    [clsTaskMetaInfo("VHCR - Hiển thị Calib công suất cao", "clsVHCR_ShowHighPowerValuecCalib", true)]
    public class clsVHCR_ShowHighPowerValuecCalib : ITask
    {
        private string m_strDescription;
        private string m_IDSpec = string.Empty;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private double m_duDisplayValue = double.MinValue;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        clsFreqTypeConv FreqTypeConv = new clsFreqTypeConv();
        private double RFHighPowerValue;
        public clsVHCR_ShowHighPowerValuecCalib()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "VHCR - Hiển thị Calib công suất cao";
            Result_unit = Powerunit.dBm;
            RFPOWHIGHMax = 36;
            RFPOWHIGHMin = 34;
            RFHighPowerValue = double.MinValue;
        }
        public enum Powerunit
        {
            dBm,
            W
        }
        [Category("#General Option")]
        [Browsable(false)]
        [SaveAtt()]
        public string Name
        {
            get { return "clsVHCR_ShowHighPowerValuecCalib"; }
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
            return $"{m_duDisplayValue.ToString("F03")} {Result_unit}";
        }
        public string GetDisplayMaxValue()
        {

            return $"{RFPOWHIGHMax.ToString("F03")} {Result_unit}";

        }
        public string GetDisplayMinValue()
        {
            return $"{RFPOWHIGHMin.ToString("F03")} {Result_unit}";
        }
        public string GetAddTaskDescription()
        {
            return $"{DutFreqTx} GHz";
        }
        public string GetValue()
        {
            return $"{m_duDisplayValue.ToString("F03")}";
        }

        public string GetMaxValue()
        {
            return $"{RFPOWHIGHMax.ToString("F03")}";
        }

        public string GetMinValue()
        {
            return $"{RFPOWHIGHMin.ToString("F03")}";
        }

        public string GetUnit()
        {
            return $"{Result_unit.ToString()}";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {            
            string strMsg = null;
            try
            {
            RETRY_LABEL:
                m_TraceService.WriteLine("Begin Test Task: " + m_strDescription + string.Format("[{0}]", GetAddTaskDescription()));
                List<ITask> lstTask = m_IModuleTask.Tasks;
                clsVHCR_CalibPower t = null;
                foreach (ITask task in m_IModuleTask.Tasks)
                {
                    t = task as clsVHCR_CalibPower;
                    if (t == null)
                    {
                        continue;
                    }    
                    else
                    {
                        if(t.DutFreqTx == DutFreqTx)
                        {
                            RFHighPowerValue = t.RFHighPowerValue;
                            strMsg = $"Get High Power Value ={RFHighPowerValue} dBm from Previous Task";
                            clsLogManager.LogReport(strMsg);
                            if ((RFHighPowerValue >= RFPOWHIGHMin) & (RFHighPowerValue <= RFPOWHIGHMax))
                            {
                                m_duDisplayValue = RFHighPowerValue;
                                strMsg = $"High Power Value ={RFHighPowerValue} dBm: PASS";
                                clsLogManager.LogReport(strMsg);
                                return TaskResult.PASS;
                            }
                            else
                            {
                                m_duDisplayValue = RFHighPowerValue;
                                strMsg = $"High Power Value ={RFHighPowerValue} dBm: Fail";
                                clsLogManager.LogError(strMsg);
                                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                                {
                                    m_duDisplayValue = double.NaN;
                                    return TaskResult.FAIL;
                                }
                                else
                                {
                                    goto RETRY_LABEL;
                                }
                            }    

                        }    
                    }    
                        
                }
                strMsg = $"Didn't  get High Power Value from Previous task";
                clsLogManager.LogError(strMsg);
                if (clsMsgHelper.ShowYesNo("Notice", strMsg + ", bạn đo lại bài này không?") == DialogResult.No)
                {
                    m_duDisplayValue = double.NaN;
                    return TaskResult.FAIL;
                }
                else
                {
                    goto RETRY_LABEL;
                }    
            }
            catch (System.Exception ex)
            {
                strMsg = "Exception: " + ex.Message;
                clsLogManager.LogError("Excution: {0}", ex.ToString());
                m_duDisplayValue = double.NaN;
                return TaskResult.FAIL;
            }
            finally
            { 
            }
        }
        public void reconnectDevice(IAccessDeviceService DUT, IAccessDeviceService SpecDevAccess, IDeviceInfor deviceinfo_VHCR, IDeviceInfor deviceinfo_SPEC_DEV)
        {
            if (DUT != null)
            {
                DUT.DisconnectDevice();
                Thread.Sleep(200);
                if (deviceinfo_VHCR != null)
                {
                    DUT.Connect2Device(deviceinfo_VHCR);
                }
            }
            if (SpecDevAccess != null)
            {
                SpecDevAccess.DisconnectDevice();
                Thread.Sleep(200);
                if (deviceinfo_SPEC_DEV != null)
                {
                    SpecDevAccess.Connect2Device(deviceinfo_SPEC_DEV);
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
            clsVHCR_ShowHighPowerValuecCalib task = new clsVHCR_ShowHighPowerValuecCalib();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        #region Test Parameter
        [Category("Test Parameter")]
        [DisplayName("RF Power High max (dBm)")]
        [SaveAtt()]
        public double RFPOWHIGHMax { set; get; }
        [Category("Test Parameter")]
        [DisplayName("RF Power High min (dBm)")]
        [SaveAtt()]
        public double RFPOWHIGHMin { set; get; }
        //Powerunit
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public Powerunit Result_unit { set; get; }
        #endregion
        #region DUT
        [Category("DUT")]
        [DisplayName("DUT Frequency Tx (GHz)")]
        [SaveAtt()]
        public double DutFreqTx { set; get; }
        #endregion
    }
}
