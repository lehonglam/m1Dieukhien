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
using System.Windows.Forms;
using TRXUBoardTestTask.TypeConverters;
using CFTWinAppCore.DeviceManager.DUT;
using Option;
namespace TRXUBoardTestTask.Task
{
    [clsTaskMetaInfo("TRXU board Tx setup", "clsTRXTxSetup", true)]
    public class clsTRXTxSetup : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_duDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Setup;
        public clsTRXTxSetup()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "TRXU board Tx setup";
            TxFrequency = 15.36;
            SpecSpan = 5;
            SpecResolutionBW = 10;
            ReferenceLevel = -10;
            Atten = 10;
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
            get { return "clsTRXTxSetup"; }
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
            return "Na";
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
                //ITRXUBoard DutService = (ITRXUBoard)(DevManager.GetDevService(SysDevType.TRXU_BOARD.ToString()));
                ISpecDevice specDevice = (DevManager.GetDevService(SysDevType.SPEC_DEV.ToString())) as ISpecDevice;
                if (specDevice == null)
                {
                    clsLogManager.LogError("Can not get device service: ISpecDevice");
                    if (clsMsgHelper.ShowYesNo("Question", "Set up fail, bạn có thực hiện lại không?") == DialogResult.No)
                    {
                        return TaskResult.FAIL;
                    }
                    else
                    {
                        goto RETRY_LABEL;
                    }
                }
                bool bOK = false;
                bool bIsDevEnable = DevManager.IsDevEnable(SysDevType.TRXU_BOARD.ToString());
                m_duDisplayValue = "FAIL";
                bOK = false;
                //Set RF cable loss
                //
                IAccessDeviceService accessDeviceService = specDevice as IAccessDeviceService;
                if (accessDeviceService != null) accessDeviceService.InitDevice();

                bOK = specDevice.SetCenterFreq(TxFrequency, FreqUnit.MHz);
                Thread.Sleep(200);
                bOK = specDevice.SetResolutionBandwidth(5, FreqUnit.MHz);
                Thread.Sleep(200);
                bOK = specDevice.SetSpan(SpecSpan, FreqUnit.MHz);
                Thread.Sleep(200);
                bOK = specDevice.SetResolutionBandwidth(SpecResolutionBW, FreqUnit.kHz);
                Thread.Sleep(200);
                bOK = specDevice.SetSpecAttenuation(Atten);
                Thread.Sleep(200);
                bOK = specDevice.SetReferentLevel(ReferenceLevel);
                Thread.Sleep(200);
                //bOK = specDevice.SetupForMeasurement();
                MessageBox.Show("Cài đặt DUT ở chế độ phát");
                m_duDisplayValue = "PASS";
                //
                if (bOK)
                {
                    m_duDisplayValue = "PASS";
                    return TaskResult.PASS;
                }
                else
                {
                    m_duDisplayValue = "FAIL";
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
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Excution: {0}", ex.ToString());
                m_duDisplayValue = "FAIL";
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
            clsTRXTxSetup task = new clsTRXTxSetup();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        //
        //
        [Category("Spectrum Analyzer")]
        [DisplayName("Tx frequency(MHz)")]
        [SaveAtt()]
        public double TxFrequency { set; get; }
        //
        [Category("Spectrum Analyzer")]
        [DisplayName("Spec span(MHz)")]
        [SaveAtt()]
        public double SpecSpan { set; get; }
        //
        [Category("Spectrum Analyzer")]
        [DisplayName("Spec resolution bandwidth(kHz)")]
        [SaveAtt()]
        public double SpecResolutionBW { set; get; }
        [Category("Spectrum Analyzer")]
        [DisplayName("Reference Level(dBm)")]
        [SaveAtt()]
        public double ReferenceLevel { set; get; }
        [Category("Spectrum Analyzer")]
        [DisplayName("Attenuator (dB)")]
        [SaveAtt()]
        public double Atten { set; get; }
    }
}
