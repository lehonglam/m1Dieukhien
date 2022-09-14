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
    [clsTaskMetaInfo("TRXU board Tx flatness power", "clsTRXTxFlatnessPower", true)]
    public class clsTRXTxFlatnessPower : ITask
    {
        private string m_strDescription;
        private ISequenceManager m_IModuleTask = null;
        private bool m_bStopWhenFail = false;
        private ITraceData m_TraceService;
        private string m_duDisplayValue = string.Empty;
        private TaskResult taskResult = TaskResult.FAIL;
        private string m_IDSpec = string.Empty;
        private Type_TestValue m_type_testvalue = Type_TestValue.Number_Value;
        public clsTRXTxFlatnessPower()
        {
            m_bStopWhenFail = true;
            AllowExcution = true;
            m_strDescription = "TRXU board Tx flatness power";
            MaxFlatnessPower = 2;
            TxFrequencyList = "3600;3640;3680;3720;3760;3800";
            TxPowerMode = "LOW";
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
            get { return "clsTRXTxFlatnessPower"; }
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
            return $"{MaxFlatnessPower.ToString("F03")}";
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
            return $"{m_duDisplayValue} {Result_unit}";
        }
        public string GetDisplayMaxValue()
        {
            return $"{MaxFlatnessPower} {Result_unit}";
        }
        public string GetDisplayMinValue()
        {
            return "NA";
        }
        public string GetAddTaskDescription()
        {
            return $"{TxPowerMode}";
        }
        public TaskResult Excution(CFTWinAppCore.DeviceManager.IDeviceManager DevManager)
        {
            m_TraceService.WriteLine(" Test Task: " + Description + "\n");
            try
            {
                //ITRXUBoard DutService = (ITRXUBoard)(DevManager.GetDevService(SysDevType.TRXU_BOARD.ToString()));
                //ISpecDevice specDevice = (ISpecDevice)(DevManager.GetDevService(SysDevType.SPEC_DEV.ToString()));
                bool bOK=false;
                //bool bIsDevEnable = DevManager.IsDevEnable(SysDevType.TRXU_BOARD.ToString());
                m_duDisplayValue = string.Empty;
                List<double> lstCWPowers = new List<double>();
                string[] strArrFreqs = TxFrequencyList.Split(';');
                foreach(string str in strArrFreqs)
                {
                    string strVarName = $"{TxPowerMode}_{str}_CWRF_POWER";
                    object objPwr = m_IModuleTask.GetGlobalVar(strVarName);
                    if (objPwr == null)
                        throw new ArgumentException($"Can't get the RF CW power for frequency {str}");
                    lstCWPowers.Add(double.Parse(objPwr.ToString()));
                }
                double duMinPower = lstCWPowers.Min();
                double duMaxPower = lstCWPowers.Max();
                double duFlatnessPower = duMaxPower - duMinPower;
                m_duDisplayValue = duFlatnessPower.ToString("F03");
                if (duFlatnessPower <= MaxFlatnessPower)
                {
                    clsLogManager.LogReport("Result PASS: Delta={0} dB <= Value require={1} dB", duFlatnessPower, MaxFlatnessPower);
                    bOK = true;
                }
                else
                {
                    clsLogManager.LogError("Result FAIL: Delta={0} dB> Value require={1} dB", duFlatnessPower, MaxFlatnessPower);
                    bOK = false;
                }
                if (bOK)
                    return TaskResult.PASS;
                else
                    return TaskResult.FAIL;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Excution: {0}", ex.ToString());
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
            clsTRXTxFlatnessPower task = new clsTRXTxFlatnessPower();
            clsSaveTaskParaHelper.CopyObjProperties(this, task);
            return task;
        }
        //
        [Category("Test Parameter")]
        [DisplayName("Max flatness power")]
        [SaveAtt()]
        public double MaxFlatnessPower { set; get; }
        //
        [Category("DUT")]
        [DisplayName("Tx power mode")]
        [TypeConverter(typeof(clsTxPowerModeTc))]
        [SaveAtt()]
        public string TxPowerMode { set; get; }
        //
        [Category("Test Parameter")]
        [DisplayName("Tx frequency list(MHz)")]
        [SaveAtt()]
        public string TxFrequencyList { set; get; }
        //
        [Category("Test Parameter")]
        [DisplayName("Unit Result")]
        [SaveAtt()]
        public string Result_unit { set; get; } //
    }
}
