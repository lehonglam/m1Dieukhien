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
    [clsTaskMetaInfo("TRXU board Tx compress", "clsTRXTxCompress", true)]
    public class clsTRXTxCompress : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_duDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        public clsTRXTxCompress()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "TRXU board Tx compress";
            MinDeltaPower = 45;
            TxFrequency = 3600;
            RxFrequency = 3720;
            TxImageFrequency = 7.68;
            SpecSpan = 5;
            SpecResolutionBW = 10;
            TxPowerMode = "LOW";
            Atten = 10;
            VHCR_Bandwidth = VHCR_Bandwidth.FIX_20M;
            VHCR_Mod = VHCR_Mod.FIX_QAM64;
            delayDUTPower = 6000;
            delayDUTSetCW = 10000;
            Result_unit = "dB";
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
            get { return "clsTRXTxCompress"; }
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
            return $"{MinDeltaPower.ToString("F03")}";
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
            return $"{MinDeltaPower} {Result_unit}";
        }
        public string GetAddTaskDescription()
        {
            return TxFrequency.ToString()+"MHz";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            m_TraceService.WriteLine(" Test Task: " + Description + " Frequency=" + TxFrequency.ToString() + "MHz \n");
            IAccessDeviceService DUT = DevManager.GetDevService(SysDevType.TRXU_BOARD.ToString()) as IAccessDeviceService;
            IDeviceInfor deviceinfo_TRXU = DevManager.GetDevRunTimeInfo(SysDevType.TRXU_BOARD.ToString()) as IDeviceInfor;
            IDeviceInfor deviceinfo_SPEC_DEV = DevManager.GetDevRunTimeInfo(SysDevType.SPEC_DEV.ToString()) as IDeviceInfor;
            IAccessDeviceService SpecdevAccess = DevManager.GetDevService(SysDevType.SPEC_DEV.ToString()) as IAccessDeviceService;
            RETRY_WHEN_FAIL:
            try
            {
            
                ITRXUBoard DutService = (DevManager.GetDevService(SysDevType.TRXU_BOARD.ToString())) as ITRXUBoard;
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
                        goto RETRY_WHEN_FAIL;
                    }
                }
                if (DutService == null)
                {
                    clsLogManager.LogError("Can not get device service: ITRXUBoard");
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
                bool bIsDevEnable = DevManager.IsDevEnable(SysDevType.TRXU_BOARD.ToString());
                
                m_duDisplayValue = string.Empty;
                bOK = false;
                double dutTxFreqConvert = 13800 - TxFrequency;
                double dutRxFreqConvert = 13800 - RxFrequency;
                //Set RF cable loss
                //specDevice.SetSpecMeasurementMode(MeasurementMode.Continuous);
                if(!specDevice.SetSpecAttenuation(Atten)) throw new System.IO.IOException();
                if(!specDevice.SetCenterFreq(TxFrequency, FreqUnit.MHz)) throw new System.IO.IOException();
                if(!specDevice.SetResolutionBandwidth(SpecResolutionBW, FreqUnit.kHz)) throw new System.IO.IOException();
                if(!specDevice.SetSpan(SpecSpan, FreqUnit.MHz)) throw new System.IO.IOException();
                //
                Thread.Sleep(1000);
                bOK = DutService.SetTxChanelConfig(VHCR_Wareform.FIX, VHCR_Mod, VHCR_Bandwidth, (ulong)(dutTxFreqConvert * 1e3), (ulong)(dutRxFreqConvert * 1e3), 0, 0);
                Thread.Sleep(1000);
                if (TxPowerMode.Equals("LOW"))
                    bOK = DutService.SetTxPowerMode(VHCR_Power.LOW);
                else if (TxPowerMode.Equals("MEDIUM"))
                    bOK = DutService.SetTxPowerMode(VHCR_Power.MEDIUM);
                else if (TxPowerMode.Equals("HIGH"))
                    bOK = DutService.SetTxPowerMode(VHCR_Power.HIGH);
                //
                Thread.Sleep(delayDUTPower);
                bOK = DutService.SetTxModulationMode();
                Thread.Sleep(delayDUTSetCW);
                if(!specDevice.CalcMarkerMax()) throw new System.IO.IOException();
                double duPower = specDevice.GetValueAtMaker(MarkerValueType.Level);
                double duFreq = specDevice.GetValueAtMaker(MarkerValueType.Freq);
                //m_duDisplayValue = $"{duPower}";
                //
                double duImgFequency = TxFrequency - TxImageFrequency;
                if(!specDevice.CalcMarkerAtFre(duImgFequency, FreqUnit.MHz)) throw new System.IO.IOException();
                Thread.Sleep(500);
                double duImgPower = specDevice.GetValueAtMaker(MarkerValueType.Level);
                duPower = duPower - duImgPower;
                m_duDisplayValue = $"{duPower.ToString("F03")}";
                if (duPower >= MinDeltaPower)
                {
                    clsLogManager.LogReport("Result PASS: Delta={0}dB >= Value require={1} dB", duPower, MinDeltaPower);
                    bOK = true;
                }
                else
                {
                    clsLogManager.LogError("Result FAIL: Delta={0}dB< Value require={1} dB", duPower, MinDeltaPower);
                    bOK = false;
                }
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
                clsLogManager.LogError(ex.ToString());
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
                        if (deviceinfo_TRXU != null)
                            DUT.Connect2Device(deviceinfo_TRXU);
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
            clsTRXTxCompress task = new clsTRXTxCompress();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        //
        [Category("Test Parameter")]
        [DisplayName("Min image power")]
        [SaveAtt()]
        public double MinDeltaPower { set; get; }
        //
        [Category("Test Parameter")]
        [DisplayName("Attenuator (dB)")]
        [SaveAtt()]
        public double Atten { set; get; }
        //
        [Category("DUT")]
        [DisplayName("Tx power mode")]
        [TypeConverter(typeof(clsTxPowerModeTc))]
        [SaveAtt()]
        public string TxPowerMode { set; get; }
        //
        [Category("DUT")]
        [DisplayName("Tx frequenccy(MHz)")]
        [SaveAtt()]
        public double TxFrequency { set; get; }
        [Category("DUT")]
        [DisplayName("Rx frequency(MHz)")]
        [SaveAtt()]
        public double RxFrequency { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Tx image frequency(MHz)")]
        [SaveAtt()]
        public double TxImageFrequency { set; get; }
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
        [Category("DUT")]
        [DisplayName("VHCR Bandwidth")]
        [SaveAtt()]
        public VHCR_Bandwidth VHCR_Bandwidth { set; get; }
        [Category("DUT")]
        [DisplayName("VHCR Mod")]
        [SaveAtt()]
        public VHCR_Mod VHCR_Mod { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT power")]
        [SaveAtt()]
        public int delayDUTPower { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Delay after set DUT CW mode")]
        [SaveAtt()]
        public int delayDUTSetCW { set; get; }
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
