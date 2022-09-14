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
    [clsTaskMetaInfo("Verify digitizer board clock power", "clsVerifyDigitizerClockPower", true)]
    public class clsVerifyDigitizerClockPower : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_duDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        public clsVerifyDigitizerClockPower()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "Verify digitizer board clock power";
            MinClockPower = 0;
            CenterFrequency = 15.36;
            SpecSpan = 5;
            SpecResolutionBandWidth = 10;
            TimeOut = 30000;
            ReferenceLevel = 10;
            ReferenceLevelOffset = 30;
            Result_unit = "dBm";
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
            get { return "clsVerifyDigitizerClockPower"; }
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
            return $"{MinClockPower.ToString("F03")}";
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
            return $"{m_duDisplayValue} {Result_unit}";
        }
        public string GetDisplayMaxValue()
        {
            return "NA";
        }
        public string GetDisplayMinValue()
        {
            return $"{MinClockPower.ToString("F03")} {Result_unit}";
        }
        public string GetAddTaskDescription()
        {
            return $"{CenterFrequency.ToString("F03")} MHz";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            m_TraceService.WriteLine(" Test Task: " + Description + CenterFrequency.ToString("F03")+ "\n");
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.DIGITIZER_BOARD.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_DIGITIZER = DevManager.GetDevRunTimeInfo(SysDevType.DIGITIZER_BOARD.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_SPEC_DEV = DevManager.GetDevRunTimeInfo(SysDevType.SPEC_DEV.ToString()) as IDeviceInfor;
            IAccessDeviceService SpecdevAccess = DevManager.GetDevService(SysDevType.SPEC_DEV.ToString()) as IAccessDeviceService;
            RETRY_WHEN_FAIL:
            try
            {
           
                IDigitizerBoard DutService = (IDigitizerBoard)(DevManager.GetDevService(SysDevType.DIGITIZER_BOARD.ToString())) as IDigitizerBoard;
                ISpecDevice specDevice = (ISpecDevice)(DevManager.GetDevService(SysDevType.SPEC_DEV.ToString())) as ISpecDevice;
                if(DutService==null)
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
                if (specDevice == null)
                {
                    clsLogManager.LogError("Can not get device service: ISpecDevice");
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
                m_duDisplayValue = string.Empty;
                bOK = false;
                //specDevice.SetSpecMeasurementMode(MeasurementMode.Continuous);
                if(!specDevice.SetCenterFreq(CenterFrequency, FreqUnit.MHz)) throw new System.IO.IOException();
                if(!specDevice.SetResolutionBandwidth(SpecResolutionBandWidth, FreqUnit.kHz)) throw new System.IO.IOException();
                if(!specDevice.SetSpan(SpecSpan, FreqUnit.MHz)) throw new System.IO.IOException();
                if(!specDevice.SetRefLevelOffset(ReferenceLevelOffset)) throw new System.IO.IOException();
                if(!specDevice.SetReferentLevel(ReferenceLevel)) throw new System.IO.IOException();
                bOK = DutService.CheckClockPort(TimeOut);
                Thread.Sleep(1000);
                if(!specDevice.CalcMarkerMax()) throw new System.IO.IOException();
                if(!specDevice.CalcMarkerAtFre(CenterFrequency, FreqUnit.MHz)) throw new System.IO.IOException();
                double duPower = specDevice.GetValueAtMaker(MarkerValueType.Level);
                double duFreq = specDevice.GetValueAtMaker(MarkerValueType.Freq);
                m_duDisplayValue = $"{duPower.ToString("F03")}";
                if (duPower >= MinClockPower)
                    bOK = true;
                else
                    bOK = false;
                //
                if(!bOK)
                {
                    if (clsMsgHelper.ShowYesNo("Question", "Kết quả không đạt, bạn có muốn thử lại không?") == DialogResult.Yes)
                        goto RETRY_WHEN_FAIL;
                }
                if (bOK)
                    return TaskResult.PASS;
                else
                    return TaskResult.FAIL;
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
                    if (SpecdevAccess != null)
                    {
                        SpecdevAccess.DisconnectDevice();
                        Thread.Sleep(200);
                        if (deviceinfo_SPEC_DEV != null)
                            SpecdevAccess.Connect2Device(deviceinfo_SPEC_DEV);
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
            clsVerifyDigitizerClockPower task = new clsVerifyDigitizerClockPower();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        //
        [Category("Test Parameter")]
        [DisplayName("Min clock power(dBm)")]
        [SaveAtt()]
        public double MinClockPower { set; get; }
        //
        [Category("Test Parameter")]
        [DisplayName("Clock center frequenccy(MHz)")]
        [SaveAtt()]
        public double CenterFrequency { set; get; }
        //
        [Category("Test Parameter")]
        [DisplayName("Spec span(MHz)")]
        [SaveAtt()]
        public double SpecSpan { set; get; }
        //
        [Category("Test Parameter")]
        [DisplayName("Spec resolution bandwidth(kHz)")]
        [SaveAtt()]
        public double SpecResolutionBandWidth { set; get; }
        [Category("Test Parameter")]
        [DisplayName("TimeOut(ms)")]
        [SaveAtt()]
        public int TimeOut { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Reference Level Offset(dBm)")]
        [SaveAtt()]
        public double ReferenceLevelOffset { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Reference Level(dBm)")]
        [SaveAtt()]
        public double ReferenceLevel { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
