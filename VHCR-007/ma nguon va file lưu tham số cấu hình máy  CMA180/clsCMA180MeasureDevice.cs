using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.Helper;
using ExternLib;
using InstrumentIOService;
using LogLibrary;
using Option;

namespace CFTSeqManager.MeasurementDevice
{

    [SysDevMetaAtt(typeof(clsCMA180MeasureDevice), SysDevType.ALL_MESS, "CMA180")]
    public class clsCMA180MeasureDevice : IAccessDeviceService, IAudioAnaDevice, IRFPowerDevice, ISignalGenDevice/*, ISpecDevice*/,
        IFreqCounter, IDevMeasurementMode
    {

        //
        private IISMessage m_IOService = null;
        private clsDeviceInfor m_devInfo;
        private bool m_bIsConfigLoaded = false;
        private bool m_bIsDoPreset = false;
        private int m_nMeasurementAvgCnt = 10;
        private bool m_bIsEnableAvgMesMode = false;
        private bool m_bCheckCommandComplete = true;
        private bool MTone = false;
        //
        public clsCMA180MeasureDevice(clsDeviceInfor devInfo)
        {
            m_devInfo = devInfo;
            IsEnableAvgMesMode = false;
            MeasurementAvgCnt = 10;
        }
        #region Device Base
        public bool InitDevice()
        {
            bool bOK = false;
            //m_IOService.Write("DO PRESETS:Load Defaults");
            //m_IOService.SendCommand("*RST");
            bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:RFSettings:CONNector RFCom");
            bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:AF:SCOunt 2");
            bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:RF:SCOunt 2");
            bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:REPetition CONTinuous");
            return bOK;
        }
        public bool Connect2Device(IDeviceInfor devInfo)
        {
            if (IsDeviceConnected())
                return true;
            else
            {
                if (m_IOService != null)
                {
                    m_IOService.Dispose();
                    m_IOService = null;
                }
            }
            //
            string strDevAddress = devInfo.DevAddress;
            if (m_IOService == null)
            {
                m_IOService = clsFactoryIOService.CreateIOService(devInfo.IOServiceType);
                m_IOService.EnableLog = devInfo.ShowIOLog;
                if (m_IOService is clsCusTcpService)
                {
                    clsCusTcpService tcpService = m_IOService as clsCusTcpService;
                    tcpService.RegisterCommandSuccessCode("1");
                    tcpService.RegisterResponseSplitCode(null);
                    tcpService.ResgisterOpcCommand("*OPC?");
                    //tcpService.RegisterIDNCommand("*IDN?\n");

                    tcpService.RegisterEOLCommand("\n");
                    int nPort = 5025;
                    if (!int.TryParse(m_devInfo.AddtionDevPara, out nPort))
                        nPort = 5025;
                    strDevAddress = string.Format("{0}:{1}", m_devInfo.DevAddress, nPort);
                }
            }
            //string st11 = m_IOService.SendAndRead("*IDN?");
            //clsLogManager.LogReport(st11);
            bool bResult = m_IOService.Connect(strDevAddress, devInfo.IOTimeOut);
            return bResult;
        }
        public bool IsDeviceConnected()
        {
            try
            {
                if (m_IOService == null) return false;
                if (!m_IOService.DeviceActived) return false;
                string strResult = m_IOService.GetIDN();
                if (string.IsNullOrEmpty(strResult))
                    return false;
                return true;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("IsDeviceConnected: {0}", ex.ToString());
                return false;
            }
        }
        public void DisconnectDevice()
        {
            if (m_IOService == null) return;
            m_IOService.Dispose();
            m_IOService = null;
        }
        public bool SetIOTimeOut(int nTimeOut)
        {
            m_IOService.SetIOTimeout(nTimeOut);
            return true;
        }
        public object GetIOLibrary()
        {
            return m_IOService;
        }
        [DisplayName("Display device IO Log")]
        [Category("Device Properties")]
        public bool ShowIOLog
        {
            set
            {
                if (m_IOService != null)
                    m_IOService.EnableLog = value;
            }
            get
            {
                if (m_IOService == null) return false;
                return m_IOService.EnableLog;
            }
        }
        [DisplayName("Check command complete")]
        [Category("Device Properties")]
        public bool CheckCmdComplte
        {
            set
            {
                m_bCheckCommandComplete = value;
                if (m_IOService != null)
                    m_IOService.CheckComp = value;
            }
            get
            {
                return m_bCheckCommandComplete;
            }
        }
        [DisplayName("Get device information when do connection")]
        [Category("Device Properties")]
        public bool GetDeviceInfo { set; get; }
        public void ShowDeviceTestDialog()
        {

        }
        public bool ShowDeviceProperties()
        {
            clsFrmGeneralConfig dlg = new clsFrmGeneralConfig(this);
            if (dlg.ShowDialog() == DialogResult.OK)
                SaveDevicePropertiesToFile();
            return true;
        }
        public bool IsDeviceSupportCusDevProperties()
        {
            return true;
        }
        public string GetDeviceFriendlyName()
        {
            return "R&S CMA180";
        }
        #endregion Device Base
        #region Power sensor
        public bool SetupForPowerMeasurement(bool Antenport = false)
        {
            bool bOk = m_IOService.Write("CONFigure:BASE:SCENario TXTest"); // chọn test Tx (khi máy phát audio và đo RF (bài đo tuyến phát DUT) cần thực hiện lệnh này trước khi test
            return bOk;
        }
        public bool SetPowerAttenuation(double duAtt)
        {
            bool bOk = m_IOService.Write("CONFigure:GPRF:MEAS:RFSettings:EATTenuation {0}", duAtt); // set suy hao bên ngoài trước khi đưa tin hiệu vào máy
            return bOk;
        }
        public bool SetMeasurementFreq(double duFreq, FreqUnit freqUnit = FreqUnit.Hz) // Set tần số đo RF
        {
            bool bResult = false;
            //double duFreqMes = 0;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    bResult = m_IOService.SendCommand("CONFigure:GPRF:MEAS:RFSettings:FREQuency {0} Hz", duFreq);
                    break;
                case FreqUnit.kHz:
                    bResult = m_IOService.SendCommand("CONFigure:GPRF:MEAS:RFSettings:FREQuency {0} KHz", duFreq);
                    break;
                case FreqUnit.MHz:
                    bResult = m_IOService.SendCommand("CONFigure:GPRF:MEAS:RFSettings:FREQuency {0} MHz", duFreq);

                    break;
            }

            return bResult;
        }
        public double GetPowerValues(PowerUnit powerUnit = PowerUnit.Wat, int Wait = 1000) // Đọc về công suất trung bình
        {
            try
            {
                //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
                //if (str.ToUpper().Contains("TXT"))
                //{
                //    m_IOService.SendCommand("CONFigure: BASE:SCENario RXT");
                //}
                double duMesData = 0;

                string strCurrMeterMode = m_IOService.SendAndRead("READ:GPRF:MEAS:POWer:AVERage?");
                string[] ArrResult = strCurrMeterMode.Split(',');
                duMesData = double.Parse(ArrResult[1]); // tách power - unit dBm
                switch (powerUnit)
                {
                    case PowerUnit.dBm:
                        break;
                    case PowerUnit.Wat:
                        duMesData = clsRFPowerUnitHelper.Dbm2Wat(duMesData);
                        break;
                    case PowerUnit.uV:
                        break;
                }
                return duMesData;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("GetPowerValues() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public void WaitMeasurementComplete(int nWaitTimeOut, double duDeltaPwr, PowerUnit powerUnit = PowerUnit.Wat)
        {
            Thread.Sleep(nWaitTimeOut);
            //return duCurrRFPwr;
        }
        public double[] GetPwrStatisticalData(int nCnt, PowerUnit powerUnit = PowerUnit.Wat)
        {
            double duMesData = 0;
            double[] arrResult = new double[3];
            double[] arrMesData = new double[nCnt];
            for (int i = 0; i < nCnt; i++)
            {
                duMesData = GetPowerValues(powerUnit);
                arrMesData[i] = duMesData;
                Thread.Sleep(200);
            }
            //Find min
            duMesData = arrMesData[0];
            for (int i = 1; i < arrMesData.Length; i++)
            {
                if (arrMesData[i] < duMesData) duMesData = arrMesData[i];
            }
            arrResult[0] = duMesData;
            //Find Max
            duMesData = arrMesData[0];
            for (int i = 1; i < arrMesData.Length; i++)
            {
                if (arrMesData[i] > duMesData) duMesData = arrMesData[i];
            }
            arrResult[2] = duMesData;
            //Aver
            duMesData = 0;
            for (int i = 0; i < arrMesData.Length; i++)
            {
                duMesData += arrMesData[i];
            }
            duMesData = duMesData / arrMesData.Length;
            arrResult[1] = duMesData;
            return arrResult;
        }
        public bool SetupAVERageManual(int count = 4)
        {
            clsLogManager.LogError("No Implementation");
            return false;
        }
        public bool SetInputLevelRange(InputPowerRange inputRange)
        {
            return true;
        }
        #endregion Power sensor
        #region ISpecDevice - Phân tích phổ (Còn thiếu nhiều function)
        public bool SetupForMeasurement()
        {
            bool bOK = true;

            return bOK;
        }
        public bool SetSpecMeasurementMode(MeasurementMode mesMode = MeasurementMode.Single)
        {
            bool bOK = false;
            return bOK;
        }
        public bool SetCenterFreq(double duFreq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            bool bOK = false;
            string strCmd = string.Empty;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    strCmd = string.Format("CONFigure:AFRF:MEAS:RFSettings:FREQuency {0}Hz", duFreq);
                    break;
                case FreqUnit.kHz:
                    strCmd = string.Format("CONFigure:AFRF:MEAS:RFSettings:FREQuency {0}kHz", duFreq);
                    break;
                case FreqUnit.MHz:
                    strCmd = string.Format("CONFigure:AFRF:MEAS:RFSettings:FREQuency {0}MHz", duFreq);
                    break;
            }
            bOK = m_IOService.SendCommand(strCmd);
            return bOK;
        }

        public double GetCenterFreq(FreqUnit freqUnit = FreqUnit.Hz)
        {
            double duFreq = 0;

            return duFreq;
        }
        public bool CalcNextMarkerMax(int nMaxker = 1, SearchDirection seaDirection = SearchDirection.None)
        {
            throw new NotImplementedException();
        }
        public bool CalcMarkerMax(int nMaxker = 1)
        {
            throw new NotImplementedException();
        }
        public bool CalcMarkerAtFre(double duFreq, FreqUnit freqUnit = FreqUnit.MHz, int nMaxker = 1)
        {
            throw new NotImplementedException();
        }
        public bool SetSpan(double SpanValue = 10, FreqUnit freqUnit = FreqUnit.kHz)
        {
            bool bOK = false;

            return bOK;
        }
        public bool SetReferentLevel(double ReferValue = 20)
        {
            bool bOK = false;

            return bOK;
        }
        public bool EnableMarker(bool bOnOff, int nMakerIndex = 1)
        {
            throw new NotImplementedException();
        }
        public bool SetSpecAttenuation(double AttenuatonValue = 40)
        {
            bool bOK = false;

            return bOK;
        }
        public bool SetRefLevelOffset(double ReferOffset = 30)
        {
            throw new Exception();
        }

        public double GetValueAtMaker(MarkerValueType valueType)
        {
            throw new NotImplementedException();
        }
        public double GetValueAtIndexMaker(int IndexMarker, MarkerValueType valueType = MarkerValueType.Level)
        {
            //string makerValue = null;
            //if (valueType == MarkerValueType.Level)
            //{
            //    makerValue = m_IOService.SendAndRead("CALC:MARK" + IndexMarker.ToString() + ":Y?");
            //}
            //else
            //{
            //    makerValue = m_IOService.SendAndRead("CALC:MARK" + IndexMarker.ToString() + ":X?");
            //}
            //return double.Parse(makerValue);
            throw new NotImplementedException();
        }

        public bool SetStartFreq(double duStartFreq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            bool bOK = false;

            return bOK;
        }

        public bool SetStopFreq(double duStopFreq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            bool bOK = false;

            return bOK;
        }

        public bool SetResolutionBandwidth(double duRbW, FreqUnit freqUnit = FreqUnit.Hz)
        {
            throw new NotImplementedException();
        }
        public bool SetVBW(double VBW, FreqUnit freqUnit = FreqUnit.kHz)
        {
            return false;
        }
        //public bool SetRefLevelOffset(double ReferOffset = 30)
        //{
        //    throw new NotImplementedException();
        //}
        public bool Set_TraceMode(TraceMode mode, bool enable = true)
        {
            throw new NotImplementedException();
        }
        #endregion ISpecDevice

        #region IAudioAnaDevice
        public bool SetAF_GenAndMes()
        {
            bool bOK = false;
            try
            {//AUDio 
                bOK = m_IOService.SendCommand("CONFigure:BASE:SCENario AUDio"); // Select Audio mode
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT1:ENABle ON");//
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT2:ENABle OFF");//
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:STATe OFF");
                bOK &= m_IOService.SendCommand("SOURce:AFRF:MEAS:AIN1:ENABle ON");
                bOK &= m_IOService.SendCommand("SOURce:AFRF:MEAS:AIN2:ENABle OFF");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN1:FILTer:HPASs OFF"); // 
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN1:FILTer:LPASs OFF");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN2:FILTer:HPASs OFF"); // 
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN2:FILTer:LPASs OFF");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN1:FILTer:WEIGhting OFF");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN2:FILTer:WEIGhting OFF");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:AF:SCOunt 1");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:REPetition SINGleshot");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:RESult:FFT OFF");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:RESult:OSCilloscope OFF");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN1:ARANging ON");
               // m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN1:MLEVel 15");
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetAF_GenAndMes() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool AudioGenSetup(bool MultiTone) // Test TX
        {
            try
            {
                bool bOK = false;
                bOK = m_IOService.Write("CONFigure:BASE:SCENario TXTest"); // Select TXT mode
                Thread.Sleep(1000);
                if (MultiTone == true)
                {
                    bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:TMODe MTONe"); // 
                    MTone = true;
                }
                else
                {
                    bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:TMODe STONe");
                    MTone = false;
                }
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT1:ENABle OFF");//
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT2:ENABle OFF");//
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:STATe OFF");

                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:STATe OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:MTONe:TONE:ALL:ENABle OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator2:MTONe:TONE:ALL:ENABle OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator3:MTONe:TONE:ALL:ENABle OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator4:MTONe:TONE:ALL:ENABle OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:MTONe:LEVel INDividual");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:MTONe:ENABle OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF");
                bOK &= m_IOService.Write("CONFigure:GPRF:MEAS:POWer:SCOunt 3");
                bOK &= m_IOService.Write("ABORt:AFRF:MEAS:MEV");
                bOK &= m_IOService.SendCommand("CONFigure:GPRF:MEAS:RFSettings:ENPower 50"); //CONFigure:AFRF:MEAS:MEV:RESult:FFT <FFTEnable>
                bOK &= m_IOService.Write("CONFigure:AFRF:MEAS:RFSettings:CONNector RFCOM");
                bOK &= SetAFScalarCount(15);
                bOK &= SetAFSpectrumCount(2);
                bOK &= SetRFStaCount(15);
                bOK &= m_IOService.Write("CONFigure:AFRF:MEAS:MEV:RESult:FFT OFF"); // 
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:DEModulation:FILTer:DFRequency 1000 Hz");
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("AudioGenSetup() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetAudioGen_Singletone(double freq, FreqUnit freqUnit = FreqUnit.kHz, double level = 1, AudioLevelUnit duAudioLevelUnit = AudioLevelUnit.MV, bool State = true)
        {
            try
            {
                return false;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetAudioGen_Singletone() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetMultitone(bool multitone)
        {
            try
            {
                bool bOK = false;
                if (multitone == true)
                {
                    bOK = m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:TMODe MTONe"); // 
                    MTone = true;
                }
                else
                {
                    bOK = m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:TMODe STONe");
                    bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:MTONe:TONE:ALL:ENABle OFF");
                    bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator2:MTONe:TONE:ALL:ENABle OFF");
                    MTone = false;
                }
                return bOK;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool Set_GCoupling(AudioGenIndex AFInput = AudioGenIndex.GEN_1)
        {
            bool bOK = false;
            try
            {
                if (AFInput != AudioGenIndex.OFF)
                {
                    if (AFInput == AudioGenIndex.GEN_1)
                    {
                        bOK = m_IOService.Write("CONFigure:AFRF:MEAS:DEModulation:GCOupling GEN1, OFF"); // 
                    }
                    else
                    {
                        bOK = m_IOService.Write("CONFigure:AFRF:MEAS:DEModulation:GCOupling GEN2, OFF"); // 
                    }
                }
                else
                {
                    bOK = m_IOService.Write("CONFigure:AFRF:MEAS:DEModulation:GCOupling OFF, OFF"); // 
                }
                return bOK;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Set_GCoupling() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool AudioAnaSetup(AudioGenIndex AFInput = AudioGenIndex.GEN_1)
        {
            bool bOK = false;
            try
            {
                string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
                if (!str.ToUpper().Contains("RX"))
                {
                    m_IOService.SendCommand("CONFigure:BASE:SCENario RXTest");
                }
                //bool bOK = m_IOService.Write("CONFigure:BASE:SCENario RXTest"); // Select RXT mode
                Thread.Sleep(2000);
                //Off filter
                bOK = m_IOService.Write("CONFigure:AFRF:MEAS:AIN1:FILTer:HPASs OFF"); // 
                bOK &= m_IOService.Write("CONFigure:AFRF:MEAS:AIN1:FILTer:LPASs OFF");
                bOK &= m_IOService.Write("CONFigure:AFRF:MEAS:AIN2:FILTer:HPASs OFF"); // 
                bOK &= m_IOService.Write("CONFigure:AFRF:MEAS:AIN2:FILTer:LPASs OFF");
                bOK &= m_IOService.Write("CONFigure:AFRF:MEAS:AIN1:FILTer:WEIGhting OFF");
                bOK &= m_IOService.Write("CONFigure:AFRF:MEAS:AIN2:FILTer:WEIGhting OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:MEAS:AIN1:ENABle ON");
                bOK &= m_IOService.Write("SOURce:AFRF:MEAS:AIN2:ENABle ON"); //
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:RFSettings:RF:ENABle OFF");
                bOK &= SetAFScalarCount(20);
                bOK &= SetAFSpectrumCount(2);
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN1:FILTer:DFRequency 1000 Hz");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN2:FILTer:DFRequency 1000 Hz");
                bOK &= m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN1:MLEVel 15");
                bOK &= m_IOService.Write("CONFigure:AFRF:MEAS:MEV:RESult:FFT OFF");
                return bOK;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("AudioAnaSetup() catched an exception: " + ec.Message);
                return false;
            }
        }
        public double GetAudioPowerLevel(AudioLevelUnit duAudioLevel = AudioLevelUnit.MV, int nMeasureCnt = 1, bool bWaitStable = false) // Đọc về mức trung bình của tín hiệu audio (mV hoặc V)
        {
            double curVol = 0;
            string strResult = null;
            double[] arrResult = new double[nMeasureCnt];
            int nBeginIndex = 0;

            for (int i = nBeginIndex; i < arrResult.Length; i++)
            {
                strResult = m_IOService.SendAndRead("READ:AFRF:MEAS:MEV:AIN1:AFSignal:CURRent?");
                string[] strlevel = strResult.Split(',');
                if (strlevel[2] != "NAV")
                    arrResult[i] = double.Parse(strlevel[2]);
                else
                {
                    clsLogManager.LogError("Error get audiopowerlevel:{0}", strResult);
                    return double.MinValue;
                }
            }
            //curVol = arrResult.RootMeanSquare();
            curVol = arrResult.Averages();
            switch (duAudioLevel)
            {
                case AudioLevelUnit.MV:
                    curVol = curVol * 1000;
                    break;
                case AudioLevelUnit.V:
                    break;
            }
            clsLogManager.LogReport("Audio:{0}", curVol);
            return curVol;
        }
        //public double WaitMeasurementAudioLevelStable(int nWaitTimeOut = 2000, double duDelta = 0.05)
        //{
        //    string strResult = m_IOService.SendAndRead("READ:AFRF:MEAS:MEV:AIN1:AFSignal:AVERage?");

        //     string [] arrResult= strResult.Split(',');
        //    double duPrevAudioLevel = double.Parse(arrResult[2]);

        //    return duPrevAudioLevel;
        //}
        public double WaitMeasurementAudioFreqStable(string strGetCmd, int nWaitTimeOut = 2000, double duDelta = 0.05) // truyền vào "strGetCmd"
        {
            string strResult = m_IOService.SendAndRead(strGetCmd);
            double duPrevAudioFreq = double.Parse(strResult);
            double duCurrAudioFreq = 0;
            int nPassCnt = 0;
            int nBeginTime = Environment.TickCount;
            int nTimeOut = 0;
            while (true)
            {
                Thread.Sleep(1000);
                strResult = m_IOService.SendAndRead(strGetCmd);
                duCurrAudioFreq = double.Parse(strResult);
                if (duCurrAudioFreq < 1)
                    duDelta = 0.1 * duCurrAudioFreq; //10%
                else
                    duDelta = 0.05 * duCurrAudioFreq; //5%
                if (Math.Abs(duCurrAudioFreq - duPrevAudioFreq) <= duDelta)
                    nPassCnt++;
                if (nPassCnt >= 2) break;
                duPrevAudioFreq = duCurrAudioFreq;
                nTimeOut = Environment.TickCount - nBeginTime;
                if (nTimeOut >= nWaitTimeOut) break;
            }
            return duCurrAudioFreq;
        }
        public double WaitMeasurementFrequencyDeviationStable(int nWaitTimeOut = 5000, double duDelta = 0.05) // Chờ ổn định tần số
        {
            string strResult = m_IOService.SendAndRead("GET MONITOR:Deviation+"); // ?
            double duPrevFreqDeviation = double.Parse(strResult);
            double duCurrFreqDeviation = 0;
            int nPassCnt = 0;
            int nBeginTime = Environment.TickCount;
            int nTimeOut = 0;
            while (true)
            {
                Thread.Sleep(300);
                strResult = m_IOService.SendAndRead("GET MONITOR:Deviation+");
                duCurrFreqDeviation = double.Parse(strResult);
                if (duCurrFreqDeviation < 1)
                    duDelta = 0.1 * duCurrFreqDeviation; //10%
                else
                    duDelta = 0.05 * duCurrFreqDeviation; //10%
                if (Math.Abs(duCurrFreqDeviation - duPrevFreqDeviation) <= duDelta) // Độ lệch tần số giữa 2 tần số liên tiếp < 10% hoặc 5% 
                    nPassCnt++;
                if (nPassCnt >= 2) break;
                duPrevFreqDeviation = duCurrFreqDeviation;
                nTimeOut = Environment.TickCount - nBeginTime;
                if (nTimeOut >= nWaitTimeOut) break;
            }
            return duCurrFreqDeviation;
        }
        public double WaitMeasurementAudioSinadStable(AudioSource audioSource = AudioSource.External, int nWaitTimeOut = 2000, double duDelta = 0.1) // Chờ Sinad ổn định, dùng cho bài đọc SINAD âm tần
        {
            string cmd = string.Empty;
            if (audioSource == AudioSource.External)
            {
                cmd = "FETCh:AFRF:MEAS:MEV:SQUality:AIN1:AVERage?";
            }    
            else
            {
                cmd = "FETCh:AFRF:MEAS:MEV:SQUality:DEMLeft:AVERage?";
            }    
            string strResult = m_IOService.SendAndRead(cmd);//
            string[] ArrResult_1 = strResult.Split(',');
            double duPrevAudioSinadLevel = double.Parse(ArrResult_1[4]);
            double duCurrSinadLevel = 0;
            int nPassCnt = 0;
            int nBeginTime = Environment.TickCount;
            int nTimeOut = 0;
            while (true)
            {
                Thread.Sleep(300);
                strResult = m_IOService.SendAndRead(cmd);
                string[] ArrResult = strResult.Split(',');
                duCurrSinadLevel = double.Parse(ArrResult[4]);
                if (duCurrSinadLevel < 1)
                    duDelta = 0.1 * duCurrSinadLevel; //10%
                else
                    duDelta = 0.05 * duCurrSinadLevel; //5%
                if (Math.Abs(duCurrSinadLevel - duPrevAudioSinadLevel) <= duDelta)
                    nPassCnt++;
                if (nPassCnt >= 2) break;
                duPrevAudioSinadLevel = duCurrSinadLevel;
                nTimeOut = Environment.TickCount - nBeginTime;
                if (nTimeOut >= nWaitTimeOut) break;
            }
            return duCurrSinadLevel;
        }
        public bool SetAudioFreq(double duAudioFreq, AudioGenIndex GenIndex = AudioGenIndex.GEN_1, FreqUnit freqUnit = FreqUnit.kHz) // Set tần số AF (1 tone hoặc 2 tones)
        {
            bool bOK = false;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    break;
                case FreqUnit.kHz:
                    duAudioFreq = duAudioFreq * 1e3;
                    break;
                case FreqUnit.MHz:
                    duAudioFreq = duAudioFreq * 1e6;
                    break;
            }
            string strCmd = string.Empty;

            switch (GenIndex)
            {
                case AudioGenIndex.GEN_1:
                    if (MTone)
                    {
                        bOK = m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:MTONe:FREQuency {0}", duAudioFreq); // Trang 369 - Usermanual
                    }
                    else
                    {
                        bOK = m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:FREQuency {0}", duAudioFreq); // Trang 340 - Usermanual
                    }
                    break;
                case AudioGenIndex.GEN_2:
                    if (MTone)
                    {
                        string mmm = m_IOService.SendAndRead("SOURce:AFRF:GEN:IGENerator1:MTONe:FREQuency?");
                        string[] cc = mmm.Split(',');
                        bOK = m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:MTONe:FREQuency {0},{1}", cc[0], duAudioFreq); // Trang 369 - Usermanual
                    }
                    else
                    {
                        bOK = m_IOService.Write("SOURce:AFRF:GEN:IGENerator2:FREQuency {0}", duAudioFreq); // Trang 340 - Usermanual
                    }
                    break;
                case AudioGenIndex.GEN_ALL:
                    if (MTone)
                    {
                        bOK = m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:MTONe:FREQuency {0},{1}", duAudioFreq, duAudioFreq);
                        //
                    }
                    else
                    {
                        bOK = m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:FREQuency {0}", duAudioFreq); // Trang 340 - Usermanual
                        bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator2:FREQuency {0}", duAudioFreq); // Trang 340 - Usermanual
                    }
                    break;
            }
            return bOK;
        }
        public bool SetAudioLevel(double duAudioLevel, AudioGenIndex GenIndex = AudioGenIndex.GEN_1, AudioLevelUnit duAudioLevelUnit = AudioLevelUnit.MV) // Set mức âm tần AF ra
        {
            bool bOK = true;
            switch (duAudioLevelUnit)
            {
                case AudioLevelUnit.MV:
                    duAudioLevel = duAudioLevel / 1000;
                    break;
                case AudioLevelUnit.V:
                    break;
            }
            string strCmd = string.Empty;

            //
            //double duFactor = ((double)(12.0 / 14.0));
            //double duAudioPeak = duAudioLevel * (2 * Math.Sqrt(2));
            //duAudioPeak = duFactor * duAudioPeak;
            //
            if (GenIndex == AudioGenIndex.GEN_1)
            {
                if (MTone)
                {
                    bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:IGENerator1:MTONe:ILEVel {0}", duAudioLevel.ToString("E")); //// Trang 369 - Usermanual
                }
                else
                {
                    bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT1:LEVel {0}", duAudioLevel.ToString("E")); //// Trang 369 - Usermanual
                }
            }
            else if (GenIndex == AudioGenIndex.GEN_2)
            {
                if (MTone)
                {
                    string mmm = m_IOService.SendAndRead("SOURce:AFRF:GEN:IGENerator1:MTONe:ILEVel?");
                    string[] cc = mmm.Split(',');
                    bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:IGENerator1:MTONe:ILEVel {0},{1}", cc[0], duAudioLevel.ToString("E"));
                }
                else
                {
                    bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT2:LEVel {0}", duAudioLevel.ToString("E")); //// Trang 369 - Usermanual
                }
            }
            return bOK;
        }
        public bool OutputAudioSignal(double duAudioLevel, double nFreq, AudioGenIndex GenIndex = AudioGenIndex.GEN_1, FreqUnit freqUnit = FreqUnit.kHz, AudioLevelUnit AudioUnit = AudioLevelUnit.MV, int nChannel = 1)
        {
            // Gộp phần set tần số và mức âm tần vào 1 bài set up AF out put
            bool bOK = false;
            bOK = SetAudioLevel(duAudioLevel, GenIndex, AudioUnit);
            if (!bOK) return bOK;
            bOK = SetAudioFreq(nFreq, GenIndex, freqUnit);
            if (!bOK) return bOK;
            bOK = EnableOutput(true, GenIndex);
            return bOK;
        }
        public bool EnableOutput(bool bOnOff, AudioGenIndex GenIndex = AudioGenIndex.GEN_1, AudioPort auPort = AudioPort.InOut) // Giữ nguyên trạng thái port GenIndex, off các tone khác hoặc off tất cả các port
        {
            bool bOK = false;
            //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
            //if (!str.ToUpper().Contains("TXT"))
            //{
            //    m_IOService.SendCommand("CONFigure: BASE:SCENario TXT");
            //}
            string mmm = m_IOService.SendAndRead("SOURce:AFRF:GEN:IGENerator1:MTONe:ENABle?");//
            string[] cc = mmm.Split(',');
            if (bOnOff == false)
            {
                if (MTone)
                {
                    if (GenIndex == AudioGenIndex.GEN_1) // Nếu không thay đổi trạng thái GEN_1
                        bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:IGENerator1:MTONe:ENABle OFF,{0},OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF", cc[1]); // Page 369
                    else if (GenIndex == AudioGenIndex.GEN_2) // Nếu không thay đổi trạng thái GEN_2
                        bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:IGENerator1:MTONe:ENABle {0},OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF", cc[0]);
                    else if (GenIndex == AudioGenIndex.GEN_ALL) // Nếu không thay đổi trạng thái của tất cả port => auto off
                        bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:IGENerator1:MTONe:ENABle OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF");
                }
                if (GenIndex == AudioGenIndex.GEN_1)
                {
                    bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT1:ENABle OFF");//
                }
                else if (GenIndex == AudioGenIndex.GEN_2)
                {
                    bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT2:ENABle OFF");//
                }
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:STATe OFF");
            }
            else
            {
                if (MTone)
                {
                    if (GenIndex == AudioGenIndex.GEN_1) // ON Gen1, không thay đổi GEN2
                        bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:IGENerator1:MTONe:ENABle ON,{0},OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF", cc[1]); // Page 369
                    else if (GenIndex == AudioGenIndex.GEN_2) // ON Gen2, không thay đổi GEN1
                        bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:IGENerator1:MTONe:ENABle {0},ON,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF,OFF", cc[0]);
                    bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT1:ENABle ON");//
                }
                else
                {
                    if (GenIndex == AudioGenIndex.GEN_1)
                    {
                        bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT1:ENABle ON");//
                    }
                    else if (GenIndex == AudioGenIndex.GEN_2)
                    {
                        bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT2:ENABle ON");//
                    }
                }
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:STATe ON");
            }

            return bOK;
        }

        public double GetAudioSinadLevel(int nMeasureCnt = 1, bool bWaitStable = false) //
        {
            double curSinad = 0;
            string strResult = null;
            double[] arrResult = new double[nMeasureCnt];
            Exception ex = null;
            string strCurrMeterMode = string.Empty;//m_IOService.SendAndRead("GET METER:Subzone");
            //m_IOService.SendCommand("GO SYSTEM:METER");

            //if (bWaitStable)
            //    WaitMeasurementAudioSinadStable();
            for (int i = 0; i < arrResult.Length; i++)
            {
                strResult = m_IOService.SendAndRead("FETCh:AFRF:MEAS:MEV:SQUality:AIN1:AVERage?"); // Trang 417 - User manual
                string[] ArrResult = strResult.Split(',');
                arrResult[i] = double.Parse(ArrResult[4]);
                curSinad += arrResult[i];
                Thread.Sleep(200);
            }
            curSinad = curSinad / arrResult.Length;
            return curSinad;
        }
        public double GetAudioDistorLevel(int nMeasureCnt = 1, AudioSource audioSource = AudioSource.External, bool bWaitStable = false, double ExpectedVoltage = 1) // Đọc về giá trị mức méo (dB)
        {
            string strResult = null;
            try
            {
                double curDistor = 0;
                double[] arrResult = new double[1];
                string strCurrMeterMode = string.Empty;//m_IOService.SendAndRead("GET METER:Subzone");
                string strQuery = string.Empty;
                if (bWaitStable)
                {
                    WaitMeasurementAudioSinadStable(audioSource);
                }    
                if (audioSource == AudioSource.Demodulation)
                {
                    strQuery = "FETCh:AFRF:MEAS:MEV:SQUality:DEMLeft:AVERage?"; // Lệnh hỏi harmonic distortion (%, dB), SINAD (dB)       
                }
                else
                {
                    strQuery = "FETCh:AFRF:MEAS:MEV:SQUality:AIN1:AVERage?"; // Lệnh hỏi harmonic distortion (%, dB), SINAD (dB) 
                }
                for (int i = 0; i < arrResult.Length; i++)
                {
                    strResult = m_IOService.SendAndRead(strQuery);
                    string[] ArrResult = strResult.Split(',');
                    arrResult[i] = double.Parse(ArrResult[1]); // index = 0 tức là đọc về distortion %
                    curDistor += arrResult[i];
                    //Thread.Sleep(300);
                }
                curDistor = curDistor / arrResult.Length;
                return curDistor;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("strResult:" + strResult);
                clsLogManager.LogError("GetAudioDistorLevel() catched an exception: " + ec.Message);
                return double.MaxValue;
            }
        }
        //filter page 329
        public bool SetAudioFilter(double duLowPass, double duHightPass, AudioFilter filter = AudioFilter.BP, FreqUnit freqUnit = FreqUnit.kHz, AudioSource audiSource = AudioSource.External)
        {   // Bài đo thu (RX Test) - Phát RF thu AF
            bool bOK = false;
            try
            {
                #region Mr An
                //m_IOService.Write("CONFigure:BASE:SCENario RXTest"); // Chọn kịch bản thu
                //m_IOService.Write("CONFigure:AFRF:MEAS:MEV:RESult:FFT OFF"); // ???
                //m_IOService.Write("CONFigure:AFRF:MEAS:MEV:RESult:OSCilloscope OFF");//???
                //return true;
                #endregion
                #region Tuannn23
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        if (duLowPass > 1000)
                        {
                            duLowPass = duLowPass / 1000; // quy LowPass về kHz
                        }
                        break;
                    case FreqUnit.kHz:
                        duHightPass = duHightPass / 1000; // quy HPass về Hz
                        break;
                    case FreqUnit.MHz:
                        duLowPass = duLowPass * 1e3;
                        duHightPass = duHightPass * 1e6;
                        break;
                }
                //string str_HP = string.Empty;
                string str_LP = string.Empty;
                int IntLP = (int)(duLowPass);


                //int IntLP = (int)(duLowPass);
                //int LP_dec = (int)((duLowPass - IntLP)*10);
                if (duLowPass == IntLP)
                {
                    if (duLowPass < 20)
                    {
                        str_LP = duLowPass.ToString() + "K";
                    }
                    else
                    {
                        str_LP = duLowPass.ToString();
                    }
                }
                else
                {
                    int LP_dec = (int)((duLowPass * 10 % 10));
                    str_LP = IntLP.ToString() + "K" + LP_dec.ToString();
                }
                if (audiSource == AudioSource.External) // Filter AF analyzer
                {
                    switch (filter)
                    {
                        case AudioFilter.Default:
                            if (duLowPass != 0 && duHightPass != 0)
                            {
                                bOK = m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:HPASs F{0}", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:LPASs F{0}", str_LP)); // kHz: OFF | F3K | 3K4 | F4K | F15K
                            }
                            else
                            {
                                bOK = m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:LPASs OFF", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:HPASs OFF", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                            }
                            break;
                        case AudioFilter.HP:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:HPASs F{0}", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                            if (duLowPass == 0)
                            {
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:LPASs OFF", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                            }
                            break;
                        case AudioFilter.LP:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:LPASs F{0}", str_LP)); // kHz: OFF | F3K | 3K4 | F4K | F15K
                            if (duHightPass == 0)
                            {
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:HPASs OFF", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                            }
                            break;
                        case AudioFilter.BP:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:HPASs F{0}", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN1:FILTer:LPASs F{0}", str_LP)); // kHz: OFF | F3K | 3K4 | F4K | F15K
                            break;
                    }
                    return true;
                }
                if (audiSource == AudioSource.Demodulation) // AF demodulation
                {
                    switch (filter)
                    {
                        case AudioFilter.Default:
                            if (duLowPass != 0 && duHightPass != 0)
                            {
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:HPASs F{0}", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:LPASs F{0}", str_LP)); // kHz: OFF | F3K | 3K4 | F4K | F15K
                            }
                            else
                            {
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:HPASs OFF", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:LPASs OFF", str_LP)); // kHz: OFF | F3K | 3K4 | F4K | F15K
                            }
                            break;
                        case AudioFilter.HP:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:HPASs F{0}", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                            if (duLowPass == 0)
                            {
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:LPASs OFF", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                            }
                            break;
                        case AudioFilter.LP:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:LPASs F{0}", str_LP)); // kHz: OFF | F3K | 3K4 | F4K | F15K
                            if (duHightPass == 0)
                            {
                                bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:HPASs OFF", duHightPass.ToString())); // kHz -> Hz: OFF | F6 | F50 | F300
                            }
                            break;
                        case AudioFilter.BP:
                            return false;
                    }
                    return bOK;
                }

                return false;
                #endregion
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetAudioFilter() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetAudio_Filter(AFFilter_LP duLowPass, AFFilter_HP duHightPass, AudioGenIndex GenIndex = AudioGenIndex.GEN_1, AudioFilter filter = AudioFilter.BP, FreqUnit freqUnit = FreqUnit.kHz, AudioSource audiSource = AudioSource.External)
        {
            bool bOK = false;
            try
            {
                string LPFilter = duLowPass.ToString();
                string HPFilter = duHightPass.ToString();
                if (duLowPass == AFFilter_LP.F3_4K)
                {
                    LPFilter = "F3K4";
                }
                if (audiSource == AudioSource.AudioGen) // Filter AF analyzer
                {
                    switch (filter)
                    {
                        case AudioFilter.Default:
                            bOK = m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN{0}:FILTer:HPASs {1}", GenIndex, HPFilter)); // kHz -> Hz: OFF | F6 | F50 | F300
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN{0}:FILTer:LPASs {1}", GenIndex, LPFilter)); // kHz: OFF | F3K | F4K | F15K
                            break;
                        case AudioFilter.HP:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN{0}:FILTer:HPASs {1}", GenIndex, HPFilter)); // kHz -> Hz: OFF | F6 | F50 | F300
                            break;
                        case AudioFilter.LP:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN{0}:FILTer:LPASs {1}", GenIndex, LPFilter)); // kHz: OFF | F3K | F4K | F15K
                            break;
                        case AudioFilter.BP:
                            return false;
                    }
                    return true;
                }
                if (audiSource == AudioSource.External) // AF demodulation
                {
                    switch (filter)
                    {
                        case AudioFilter.Default:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:HPASs {0}", HPFilter)); // kHz -> Hz: OFF | F6 | F50 | F300
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:LPAS {0}", LPFilter)); // kHz: OFF | F3K | F4K | F15K
                            break;
                        case AudioFilter.HP:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:HPASs {0}", HPFilter)); // kHz -> Hz: OFF | F6 | F50 | F300
                            break;
                        case AudioFilter.LP:
                            bOK &= m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:DEModulation:FILTer:LPAS {0}", LPFilter)); // kHz: OFF | F3K | F4K | F15K
                            break;
                        case AudioFilter.BP:
                            return false;
                    }
                    return bOK;
                }

                return false;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetAudioFilter() catched an exception " + ec.Message);
                return false;
            }
        }
        public bool SetAudioWeightingFilter(AudioGenIndex GenIndex = AudioGenIndex.GEN_1, AudioWeightingFilter filter = AudioWeightingFilter.CCITT) //Selects the weighting filter in the RF input path - tuannn23
        {
            bool bOK = false;
            try
            {
                // ("INIT:AFRF:MEAS:MEV");
                string str_GenIndex = "1";
                if (GenIndex == AudioGenIndex.GEN_2)
                {
                    str_GenIndex = "2";
                }
                else
                {
                    str_GenIndex = "1";
                }
                #region Tuannn23
                switch (filter)
                {
                    case AudioWeightingFilter.None:
                        bOK = m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN{0}:FILTer:WEIGhting OFF", str_GenIndex));
                        break;
                    case AudioWeightingFilter.CCITT:
                        bOK = m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN{0}:FILTer:WEIGhting CCITt", str_GenIndex));
                        break;
                    case AudioWeightingFilter.C_Message:
                        bOK = m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN{0}:FILTer:WEIGhting CMESsage", str_GenIndex)); // kHz
                        break;
                    case AudioWeightingFilter.Default:
                        bOK = m_IOService.Write(string.Format("CONFigure:AFRF:MEAS:AIN{0}:FILTer:WEIGhting AWEighting", str_GenIndex)); // kHz
                        break;
                }
                return bOK;
                #endregion
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetAudioWeightingFilter() catched an exception: " + ec.Message);
                return false;
            }
        }
        public double GetAudioFreqErr(double duInputFreq, FreqUnit InFreqUnit = FreqUnit.kHz, AudioSource audioSource = AudioSource.External, AudioType auType = AudioType.Normal, bool bWaitStable = false) // Get AF frequency
        {

            double duValue = 0;
            string strFreqErr = string.Empty;

            strFreqErr = m_IOService.SendAndRead("READ:AFRF:MEAS:MEV:AIN1:AFSignal:AVERage?"); // Page 413: return Reliablity, frequency, Effective level of AC, level of DC
            string[] ArrFreqErr = strFreqErr.Split(',');
            if (!double.TryParse(ArrFreqErr[1], out duValue)) // Tách lấy tần số AF
                duValue = 0;

            switch (InFreqUnit)
            {
                case FreqUnit.MHz:
                    duValue = duValue - duInputFreq * 1e6; // Sai số tần số sẽ bằng tần số đo được trừ đi tần số tham chiếu (tham số nạp vào)
                    break;
                case FreqUnit.kHz:
                    duValue = duValue - duInputFreq * 1e3;
                    break;
                case FreqUnit.Hz:
                    duValue = duValue - duInputFreq;
                    break;
            }
            return duValue;
        }
        public bool Set_AudioRef(double AFFreq, AudioGenIndex index = AudioGenIndex.GEN_1, FreqUnit freqUnit = FreqUnit.kHz)
        {
            try
            {
                double Freq = 0;
                switch (freqUnit)
                {
                    case FreqUnit.MHz:
                        Freq = AFFreq * 1e6; // Sai số tần số sẽ bằng tần số đo được trừ đi tần số tham chiếu (tham số nạp vào)
                        break;
                    case FreqUnit.kHz:
                        Freq = AFFreq * 1e3;
                        break;
                    case FreqUnit.Hz:
                        Freq = AFFreq;
                        break;
                }
                return m_IOService.SendCommand("CONFigure:AFRF:MEAS:AIN1:FILTer:DFRequency {0} Hz", Freq);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Set_AudioRef() catched an exception: " + ec.Message);
                return false;
            }
        }
        public double Get_MeasuredAudioFreq(AudioGenIndex index = AudioGenIndex.GEN_1, AudioSource audioSource = AudioSource.External, AudioType auType = AudioType.Normal, int MesCnt = 1, FreqUnit InFreqUnit = FreqUnit.kHz)
        {
            try
            {
                string cmd = string.Empty;
                double[] AFFreq = new double[MesCnt];
                switch (index)
                {
                    case AudioGenIndex.GEN_1:
                        cmd = "FETCh:AFRF:MEAS:MEV:AIN1:AFSignal:AVERage?";
                        break;
                    case AudioGenIndex.GEN_2:
                        cmd = "FETCh:AFRF:MEAS:MEV:AIN2:AFSignal:AVERage?";
                        break;
                    case AudioGenIndex.GEN_ALL:
                        cmd = "FETCh:AFRF:MEAS:MEV:AIN1:AFSignal:AVERage?";
                        break;
                }
                for (int i = 0; i < AFFreq.Length; i++)
                {
                    string StrReturn = m_IOService.SendAndRead(cmd);
                    clsLogManager.LogReport("Measured AF frequency: {0}", StrReturn);
                    string[] ArrFreq = StrReturn.Split(',');
                    if (!double.TryParse(ArrFreq[1], out AFFreq[i])) // Tách lấy tần số AF
                    {
                        return double.MinValue;
                    }
                    Thread.Sleep(200);
                }
                double result = AFFreq.Average();
                switch (InFreqUnit)
                {
                    case FreqUnit.MHz:
                        result = result * 1e6; // Sai số tần số sẽ bằng tần số đo được trừ đi tần số tham chiếu (tham số nạp vào)
                        break;
                    case FreqUnit.kHz:
                        result = result * 1e3;
                        break;
                }
                return result;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Get_MeasuredAudioFreq() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public double Get_DemodAudioFreq(FreqUnit InFreqUnit, int Mescnt = 3)
        {
            try
            {
                double[] duArr = new double[Mescnt];
                string[] ArrFreq = null;
                string StrReturn = string.Empty;
                string cmd = "FETCh:AFRF:MEAS:MEV:DEMLeft:AFSignal:AVERage?";
                for (int i = 0; i < Mescnt; i++)
                {
                    StrReturn = m_IOService.SendAndRead(cmd);
                    clsLogManager.LogReport("Demodulated AF frequency: {0}", StrReturn);
                    ArrFreq = StrReturn.Split(',');
                    if (!double.TryParse(ArrFreq[1], out duArr[i])) // Tách lấy tần số AF
                    {
                        return double.MinValue;
                    }
                    Thread.Sleep(300);
                }
                double result = duArr.Average();
                switch (InFreqUnit)
                {
                    case FreqUnit.MHz:
                        result = result * 1e6; // Sai số tần số sẽ bằng tần số đo được trừ đi tần số tham chiếu (tham số nạp vào)
                        break;
                    case FreqUnit.kHz:
                        result = result * 1e3;
                        break;
                }
                return result;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        #endregion IAudioAnaDevice

        #region ISignalGenDevice
        public bool SignalGenSetup()
        {
            bool bOK = true;

            return bOK;
        }
        public bool Select_OutputPort(OutputPort port)
        {
            try
            {
                //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
                //if (str.ToUpper().Contains("TXT"))
                //{
                //    m_IOService.SendCommand("CONFigure: BASE:SCENario RXTest");
                //}
                string strCmd = string.Empty;
                switch (port)
                {
                    case OutputPort.RFOut:
                        strCmd = string.Format("SOURce:AFRF:GEN:RFSettings:CONNector RFOut"); // Page 345 - default unit Hz
                        break;
                    default:
                        strCmd = string.Format("SOURce:AFRF:GEN:RFSettings:CONNector RFCom");
                        break;
                }
                return m_IOService.SendCommand(strCmd);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Enable_OutputPort catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool Enable_RFOut(bool enable)
        {
            //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
            //if (str.ToUpper().Contains("TXT"))
            //{
            //    m_IOService.SendCommand("CONFigure:BASE:SCENario RXTest");
            //}
            try
            {
                if (enable)
                {
                    return m_IOService.SendCommand("SOURce:AFRF:GEN:RFSettings:RF:ENABle ON");
                }
                else
                {
                    return m_IOService.SendCommand("SOURce:AFRF:GEN:RFSettings:RF:ENABle OFF");
                }
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Enable_RFOut() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool EnableOutput(bool bOnOff, SignalPort DevPort = SignalPort.InOut) // Starts or stops the generator. 
        {
            bool bOK = false;
            string strcommand = null;

            if (bOnOff)
                strcommand = "SOURce:AFRF:GEN:STATe ON"; // Trang 343 - User manual
            else
                strcommand = "SOURce:AFRF:GEN:STATe OFF";

            bOK = m_IOService.SendCommand(strcommand);
            clsLogManager.LogReport(strcommand);
            return bOK;

        }
        public bool SetModulationType(ModulatioType modType = ModulatioType.OFF) // Page 348 - select RF signal mode
        {
            //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
            //if (!str.ToUpper().Contains("TXT"))
            //{
            //    m_IOService.SendCommand("CONFigure: BASE:SCENario TXT");
            //}
            bool bOK = true;
            switch (modType)
            {
                case ModulatioType.CW:
                    bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:MSCHeme CW"); // Constant wave signal (unmodulated RF carrier): tín hiệu sóng bất biến
                    break;
                case ModulatioType.AM:
                    bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:MSCHeme AM"); //amplitude modulation: điều chế biên độ
                    break;
                case ModulatioType.FM:
                    bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:MSCHeme FM"); // Frequency modulation: điều chế tần số
                    break;
            }
            return bOK;
        }
        public bool SetOutputPower(double pwr, PowerUnit powerUnit = PowerUnit.Wat) // Out put power
        {
            //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
            //if (!str.ToUpper().Contains("TXT"))
            //{
            //    m_IOService.SendCommand("CONFigure: BASE:SCENario TXT");
            //    Thread.Sleep(1000);
            //}
            double duPDbm = 0;

            switch (powerUnit)
            {
                case PowerUnit.dBm:
                    duPDbm = pwr;
                    break;
                case PowerUnit.Wat:
                    duPDbm = clsRFPowerUnitHelper.Wat2Dbm(pwr);
                    break;
                case PowerUnit.uV:
                    duPDbm = clsRFPowerUnitHelper.Vol2Dbm(pwr / 1e6);
                    break;
                case PowerUnit.mV:
                    duPDbm = clsRFPowerUnitHelper.Vol2Dbm(pwr / 1e3);
                    break;
            }

            bool bOK = m_IOService.SendCommand("SOURce:AFRF:GEN:RFSettings:LEVel {0}", duPDbm.ToString("F2")); // Page 345 (default unit dBm)
            //clsLogManager.LogReport("SOURce:AFRF:GEN:RFSettings:LEVel {0}", duPDbm.ToString("F2"));
            Thread.Sleep(100);
            return bOK;
        }
        public bool SetRFFrequency(double Freq, FreqUnit freqUnit = FreqUnit.Hz) // RF frequency
        {
            try
            {
                //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
                //if (!str.ToUpper().Contains("TXT"))
                //{
                //    m_IOService.SendCommand("CONFigure: BASE:SCENario TXT");
                //    Thread.Sleep(1000);
                //}
                string strCmd = string.Empty;
                /*string strTemp = GetCurrSysMode();
                if (strTemp.IndexOf("Generate") < 0)
                {
                    SignalGenSetup();
                }*/
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        strCmd = string.Format("SOURce:AFRF:GEN:RFSettings:FREQuency {0}Hz", Freq); // Page 345 - default unit Hz
                        break;
                    case FreqUnit.kHz:
                        strCmd = string.Format("SOURce:AFRF:GEN:RFSettings:FREQuency {0}kHz", Freq);
                        break;
                    case FreqUnit.MHz:
                        strCmd = string.Format("SOURce:AFRF:GEN:RFSettings:FREQuency {0}MHz", Freq.ToString("F05"));
                        break;
                }

                return m_IOService.SendCommand(strCmd);
                //return R800SetCmdHelper(strCmd, "GET RF:Generate Frequency", Freq.ToString("F05"));
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetRFFrequency() catched an exception: " + ec.Message);
                return false;
            }
        }

        public double GetCurrPowerLevel() // not used
        {
            string strResult = "00";
            double duTemp = double.Parse(strResult);
            return duTemp;
        }

        public bool OutputCWSignal(double duPwr, double nFreq, FreqUnit freqUnit = FreqUnit.Hz, PowerUnit powerUnit = PowerUnit.Wat) // Gộp bước setup tín hiệu CW
        {
            if (!SetOutputPower(duPwr, powerUnit))
                return false;
            if (!SetRFFrequency(nFreq, freqUnit))
                return false;
            return true;
        }
        public bool OutputAMSignal(clsAMSigDes amDes)
        {
            throw new NotImplementedException();
        }

        public bool OutputFMSignal(clsFMSigDes fmDes)
        {
            R800SetCmdHelper("GO SYSTEM:AUDIO"); // couldn't find this command in User manual
            if (!SetModulationType(ModulatioType.FM)) // Chọn mode điều chế tần số
                return false;
            switch (fmDes.Unit) // Set up tần số
            {
                case FreqUnit.Hz:
                    fmDes.DEViation /= 1e3;
                    break;
                case FreqUnit.kHz:
                    fmDes.FREQuency *= 1e3;
                    break;
                case FreqUnit.MHz:
                    fmDes.DEViation *= 1e3;
                    fmDes.FREQuency *= 1e6;
                    break;
            }
            //
            string strCmd = string.Empty;
            bool bOK = true;
            if (fmDes.DEViation > 0)
            {
                strCmd = string.Format("SET AUDIO:Gen Tone A={0}", fmDes.DEViation.ToString("F01")); // couldn't find this command in User manual
                bOK = R800SetCmdHelper(strCmd, "GET AUDIO:Gen Tone A", fmDes.DEViation.ToString("F01")); // couldn't find this command in User manual
            }
            //
            if (fmDes.FREQuency > 0)
            {
                strCmd = string.Format("SET AUDIO:Tone A Frequency ={0}", fmDes.FREQuency.ToString("F01")); // couldn't find this command in User manual
                bOK = R800SetCmdHelper(strCmd, "GET AUDIO:Tone A Frequency", fmDes.FREQuency.ToString("F01")); // couldn't find this command in User manual
            }
            return true;
        }
        public double GetMaxOutputLevel(SignalPort DevPort = SignalPort.InOut) // Theo thông số máy đo CMA180
        {
            if (DevPort == SignalPort.InOut) //Page 7 - datasheet
                return -9;
            else
                return 16;
        }
        public double GetMinOutputLevel(SignalPort DevPort = SignalPort.InOut) // Theo thông số máy đo CMA180
        {
            if (DevPort == SignalPort.InOut) ////Page 7 - datasheet
                return -158;
            else
                return -132;
        }
        public bool SetRFOffset(double duOffset, SignalPort DevPort = SignalPort.InOut)
        {
            bool bOK = true;

            if (duOffset == 0)
            {
                R800CmdHelper("GET SETUP:RF Level Offset", "Off", "SET SETUP:RF Level Offset=Off");// couldn't find this command in User manual
            }
            else
            {
                R800CmdHelper("GET SETUP:RF Level Offset", "On", "SET SETUP:RF Level Offset=On");// couldn't find this command in User manual
                string strSetCmd = string.Format("SET SETUP:RF In/Out Offset={0}", duOffset);
                if (DevPort == SignalPort.InOut)
                    R800CmdHelper("GET SETUP:RF In/Out Offset", duOffset.ToString(), strSetCmd);// couldn't find this command in User manual
                else
                {
                    strSetCmd = string.Format("SET SETUP:RF Gen Out Offset={0}", duOffset);// couldn't find this command in User manual
                    R800CmdHelper("GET SETUP:RF Gen Out Offset", duOffset.ToString(), strSetCmd);
                }
            }
            return bOK;
        }
        public bool SetFreqDev(double FreqDev, FreqUnit freqUnit = FreqUnit.Hz)
        {
            try
            {
                //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
                //if (str.ToUpper().Contains("TXT"))
                //{
                //    m_IOService.SendCommand("CONFigure: BASE:SCENario RXTest");
                //    Thread.Sleep(1000);
                //}
                string strCmd = string.Empty;
                /*string strTemp = GetCurrSysMode();
                if (strTemp.IndexOf("Generate") < 0)
                {
                    SignalGenSetup();
                }*/
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        strCmd = string.Format("SOURce:AFRF:GEN:MODulator:FDEViation {0}Hz", FreqDev); // Page 345 - default unit Hz
                        break;
                    case FreqUnit.kHz:
                        strCmd = string.Format("SOURce:AFRF:GEN:MODulator:FDEViation {0}kHz", FreqDev);
                        break;
                    case FreqUnit.MHz:
                        strCmd = string.Format("SOURce:AFRF:GEN:MODulator:FDEViation {0}MHz", FreqDev.ToString("F05"));
                        break;
                }

                return m_IOService.SendCommand(strCmd);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetFreqDev catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool Set_AFModulation(double AFFreq, AudioGenIndex index = AudioGenIndex.GEN_3, FreqUnit freqUnit = FreqUnit.Hz)
        {
            try
            {
                //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
                //if (!str.ToUpper().Contains("TXT"))
                //{
                //    m_IOService.SendCommand("CONFigure: BASE:SCENario RXTest");
                //    Thread.Sleep(1000);
                //}
                string strCmd = string.Empty;
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        strCmd = string.Format("SOURce:AFRF:GEN:IGENerator3:FREQuency {0}Hz", AFFreq); // Page 345 - default unit Hz
                        break;
                    case FreqUnit.kHz:
                        strCmd = string.Format("SOURce:AFRF:GEN:IGENerator3:FREQuency {0}kHz", AFFreq);
                        break;
                    case FreqUnit.MHz:
                        return false;
                }
                return m_IOService.SendCommand(strCmd);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Set_AFModulation() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool Enable_AFModulation(bool EnableLeft, bool EnableRight)
        {
            try
            {
                //string str = m_IOService.SendAndRead("CONFigure:BASE:SCENario?");
                //if (!str.ToUpper().Contains("TXT"))
                //{
                //    m_IOService.SendCommand("CONFigure: BASE:SCENario RXTest");
                //    Thread.Sleep(1000);
                //}
                string Enable_Left = string.Empty;
                string Enable_Right = string.Empty;
                if (EnableLeft)
                {
                    Enable_Left = "ON";
                }
                else
                {
                    Enable_Left = "OFF";
                }
                if (EnableRight)
                {
                    Enable_Right = "ON";
                }
                else
                {
                    Enable_Right = "OFF";
                }
                return m_IOService.SendCommand("SOURce:AFRF:GEN:MODulator:ENABle {0}, {1}", Enable_Left, Enable_Right);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Enable_AFModulation() catched an exception: " + ec.Message);
                return false;
            }
        }
        public string GetSN_fromConfig()
        {
            return m_devInfo.DeviceSN;
        }

        public bool Set_Sweep(double FreqStart, double FreqStop, FreqUnit freqUnit, int Dwelltime, double RFLevel, int NumberofStep)
        {
            return false;
        }
        public bool ON_Sweep(bool bON)
        {
            return false;
        }
        public bool Set_FreqMode(FreqMode mode)
        {
            return false;
        }
        public bool EnablePlayARBFile(string strFileName)
        {
            throw new NotImplementedException();
        }

        public bool SetARBClockFrequency(double Freq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            throw new NotImplementedException();
        }
        #endregion ISignalGenDevice

        #region IFreqCounter
        public bool Select_InputPort(InputPort port = InputPort.RFCom)
        {
            try
            {
                return m_IOService.Write("CONFigure:AFRF:MEAS:RFSettings:CONNector " + port.ToString());
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Select_InputPort() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetDemodulatioType(DemodulationType type, double RFFreq, FreqUnit unit = FreqUnit.MHz)
        {
            try
            {
                double Freq = 0;
                bool bOK = false;
                switch (unit) // Quy đổi về đơn vị MHz
                {
                    case FreqUnit.MHz:
                        Freq = RFFreq;
                        break;
                    case FreqUnit.kHz:
                        Freq = RFFreq / 1e3;
                        break;
                    case FreqUnit.Hz:
                        Freq = Freq / 1e6;
                        break;
                }
                if (RFFreq > 0)
                {
                    SetMeasurementFreq(Freq, unit);
                }
                bOK = m_IOService.SendCommand("CONFigure:AFRF:MEAS:DEModulation " + type.ToString());
                return bOK;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetDemodulatioType() catched an exception: " + ec.Message);
                return false;
            }
        }
        public double GetFrequencyError(double duTransFreq, int nChannel = 1, FreqUnit freqUnit = FreqUnit.Hz) // Hàm tính sai số lần số giữa tần số sóng mang và tần số tham chiếu - kiểm tra công thức???
        {
            double duFre = 0;
            int Mescnt = 1;
            string strValue = string.Empty;
            string[] arrValue = null;
            double[] duArr = new double[Mescnt];
            for (int i = 0; i < Mescnt; i++)
            {
                strValue = m_IOService.SendAndRead("READ:AFRF:MEAS:MEV:RFCarrier:AVERage?"); // Return reliability, FreqError, PowerRMS, PowerPEP
                clsLogManager.LogReport("Respone: " + strValue);
                arrValue = strValue.Split(',');

                //string strValue = m_IOService.SendAndRead("GET MONITOR:Deviation+");
                duArr[i] = double.Parse(arrValue[1]); // Tách Carrier frequency error 
                //Thread.Sleep(200);
            }
            duFre = duArr.Average();
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    break;
                case FreqUnit.kHz:
                    duFre = duFre / 1e3; // Không cần trừ đi tần số tham chiếu
                    break;
                case FreqUnit.MHz:
                    duFre = duFre / 1e6;
                    break;
            }
            clsLogManager.LogReport("Freq Error = {0}", duFre);
            return duFre;
        }
        public double GetFrequencyDeviation(double duTransFreq, int nChannel = 1, FreqUnit freqUnit = FreqUnit.Hz, bool bWaitStable = false)// Đo độ lệch tần số mode FM
        {
            //double duFre = 0;

            //if (bWaitStable)
            //    WaitMeasurementFrequencyDeviationStable();
            ////DISPLAY:Deviation Average 
            //string strValue = m_IOService.SendAndRead("GET MONITOR:Deviation+"); // couldn't find this command in User manual
            //duFre = double.Parse(strValue);
            //switch (freqUnit)
            //{
            //    case FreqUnit.Hz:
            //        duFre = duFre * 1e3;
            //        break;
            //    case FreqUnit.kHz:
            //        break;
            //    case FreqUnit.MHz:
            //        duFre = duFre / 1e3;
            //        break;
            //}
            //return duFre;
            try
            {
                string cmd = "FETCh:AFRF:MEAS:MEV:DEModulation:FDEViation:AVERage?"; //FETCh:AFRF:MEAS:MEV:DEModulation:FDEViation:AVERage?
                string str_Response = string.Empty;
                int Mescnt = 3;
                //switch (type) // Quy đổi về đơn vị MHz
                //{
                //    case DemodulationType.PM:
                //        cmd = "FETCh:AFRF:MEAS:MEV:DEModulation:PDEViation:AVERage?";
                //        break;
                //    default:
                //        cmd = "FETCh:AFRF:MEAS:MEV:DEModulation:FDEViation:AVERage?";
                //        break;
                //}
                double FreqDev = double.MinValue;
                double[] Arr = new double[Mescnt];
                for (int i = 0; i < 3; i++)
                {
                    str_Response = m_IOService.SendAndRead(cmd);
                    string[] ArrResult = str_Response.Split(',');
                    clsLogManager.LogReport("Measurement device responded Fre.dev =  {0}Hz", ArrResult[2]);
                    if (!double.TryParse(ArrResult[2], out Arr[i])) // Tách lấy tần số AF
                    {
                        return double.MinValue;
                    }
                    Arr[i] = double.Parse(ArrResult[2]);
                    Thread.Sleep(200);
                }
                FreqDev = Arr.Average();
                switch (freqUnit)
                {
                    case FreqUnit.Hz: //Default unit: Hz 
                        break;
                    case FreqUnit.kHz:
                        FreqDev = FreqDev / 1e3;
                        break;
                    case FreqUnit.MHz:
                        FreqDev = FreqDev / 1e6;
                        break;
                }
                return FreqDev;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("GetFrequencyDeviation() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public double[] GetStatisticalData(double duTransFreq, int nChannel = 1, int nCnt = 10, FreqUnit freqUnit = FreqUnit.Hz)
        {
            double[] arrData = new double[nCnt];
            for (int i = 0; i < nCnt; i++)
            {
                arrData[i] = GetFrequencyError(duTransFreq, nChannel, freqUnit);
                Thread.Sleep(200);
            }
            return arrData;
        }
        #endregion IFreqCounter
        #region IDevMeasurementMode
        public bool SetMeasurementMode(MeasurementMode mesMode = MeasurementMode.Single)
        {
            return true;
        }
        public bool SetMeasurementAverageCnt(int nCnt)
        {

            return true;
        }
        public bool SetAFScalarCount(int nCnt)
        {
            bool bOK = false;
            if (nCnt > 0 && nCnt <= 1000)
                bOK=m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:AF:SCOunt {0}", nCnt);
            else
                bOK=m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:AF:SCOunt 10");
            return bOK;

        }
        public bool SetAFSpectrumCount(int nCnt)
        {
            bool bOK = false;
            if (nCnt > 0 && nCnt <= 1000)
                bOK=m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:AFFFt:SCOunt {0}", nCnt);
            else
                bOK=m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:AFFFt:SCOunt 10");
            return bOK;
        }
        private bool SetRFStaCount(int nCnt)
        {
            bool bOK = false;
            if (nCnt > 0 && nCnt <= 1000)
                bOK=m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:RF:SCOunt {0}", nCnt);
            else
                bOK=m_IOService.SendCommand("CONFigure:AFRF:MEAS:MEV:RF:SCOunt 10");
            return true;
        }
        #endregion
        #region Private function
        public bool init_Measurement(bool init)
        {
            bool bOK = false;
            if (init)
            {
                bOK = m_IOService.SendCommand("INIT:AFRF:MEAS:MEV");
                Thread.Sleep(1100);
            }
            else
            {
                bOK = m_IOService.SendCommand("STOP:AFRF:MEAS:MEV");
            }
            return bOK;
        }
        public bool SetupRF_GenAndMes(InputPort port = InputPort.RFCom, OutputPort outport = OutputPort.RFOut)
        {
            try
            {
                bool bOK = false;
                bOK = m_IOService.Write("CONFigure:BASE:SCENario DXTest"); // Select TXT mode
                Thread.Sleep(1000);
                // OFF audio generator
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT1:ENABle OFF");//
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:AOUT2:ENABle OFF");//
                bOK &= m_IOService.SendCommand("SOURce:AFRF:GEN:STATe OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator1:MTONe:TONE:ALL:ENABle OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator2:MTONe:TONE:ALL:ENABle OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator3:MTONe:TONE:ALL:ENABle OFF");
                bOK &= m_IOService.Write("SOURce:AFRF:GEN:IGENerator4:MTONe:TONE:ALL:ENABle OFF");
                bOK &= m_IOService.Write("CONFigure:GPRF:MEAS:POWer:SCOunt 3");
                bOK &= m_IOService.Write("ABORt:AFRF:MEAS:MEV");
                m_IOService.SendCommand("CONFigure:GPRF:MEAS:RFSettings:ENPower 50"); //CONFigure:AFRF:MEAS:MEV:RESult:FFT <FFTEnable>
                //
                bOK &= SetAFScalarCount(15);
                bOK &= SetAFSpectrumCount(2);
                bOK &= SetRFStaCount(15);
                // select out/in
                bOK &= Select_OutputPort(outport);
                bOK &= Select_InputPort(port);
                return bOK;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetupRF_GenAndMes() catched an exception: " + ec.Message);
                return false;
            }
        }
        private string GetCurrSysMode()
        {
            return m_IOService.SendAndRead("GET SYSTEM:Mode");
        }
        private bool R800CmdHelper(string strGetCmd, string strValue, string strSetcmd, int nStardWaitTime = 100)
        {
            bool bOK = true;
            string current = m_IOService.SendAndRead(strGetCmd);
            if (current.IndexOf(strValue) < 0)
                bOK = R800SetCmdHelper(strSetcmd, strGetCmd, strValue, nStardWaitTime);
            return bOK;
        }
        private bool R800SetCmdHelper(string strCmd, string strCheckCmd, string strOK, int nStardWaitTime = 100)
        {
            int nBeginTime = 0;
            int nTimeOut = 0;
            string strCurr = string.Empty;
            //
            if (string.IsNullOrEmpty(strCheckCmd))
            {
                strCheckCmd = "NOOP";
                strOK = "0";
            }
            m_IOService.Write(strCmd);
            if (nStardWaitTime > 0)
                Thread.Sleep(nStardWaitTime);
            strCurr = m_IOService.ReadString();
            if (strCurr.IndexOf(strOK) >= 0)
            {
                return true;
            }
            //else
            //   clsLogManager.LogWarning("R800SetCmdHelper:Set command {0} current value is {1}, OK value is {2}", strCmd, strCurr, strOK);
            nBeginTime = Environment.TickCount;
            while (true)
            {

                strCurr = m_IOService.SendAndRead(strCheckCmd);
                if (strCurr.IndexOf(strOK) >= 0)
                {
                    return true;
                }
                nTimeOut = Environment.TickCount - nBeginTime;
                if (nTimeOut > m_devInfo.IOTimeOut)
                {
                    TimeoutException te = new TimeoutException(string.Format("Time out for waiting cmd {0} complete", strCmd));
                    throw te;
                }
                if (nStardWaitTime > 0)
                    Thread.Sleep(nStardWaitTime);
            }
        }
        private bool R800SetCmdHelper(string strCmd, int nStardWaitTime = 100)
        {
            m_IOService.Write(strCmd);
            string strCurr = m_IOService.ReadString();
            if (nStardWaitTime > 0)
                Thread.Sleep(nStardWaitTime);
            return true;
        }
        private bool WaitR8000BFree(int nMaxWaitTime)
        {
            int nBeginTime = Environment.TickCount;
            int nTimeOut = 0;
            string strResult = null;
            while (true)
            {
                strResult = m_IOService.SendAndRead("NOOP");
                if (strResult.IndexOf('0') >= 0)
                    return true;
                nTimeOut = Environment.TickCount - nBeginTime;
                if (nTimeOut >= nMaxWaitTime)
                    return false;
                Thread.Sleep(300);
            }
        }
        private void LoadDevicePropertiesFromFile()
        {
            try
            {
                if (m_bIsConfigLoaded) return;
                clsXmlConfiguration config = new clsXmlConfiguration();
                string strFileName = string.Format("{0}\\DeviceProperties\\CMA180.config", Application.StartupPath);
                if (!File.Exists(strFileName))
                    return;
                if (!config.LoadFromFile(strFileName))
                    return;
                foreach (clsXmlEntry entry in config.NodeList)
                {
                    if (entry.NodeName == "DeviceProperties")
                    {
                        entry.Entry2Object(this);
                    }
                }
                m_bIsConfigLoaded = true;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("LoadDevicePropertiesFromFile: {0}", ex.ToString());
            }
        }
        private void SaveDevicePropertiesToFile()
        {
            try
            {
                clsXmlConfiguration config = new clsXmlConfiguration("DeviceConfiguration");
                string strDicName = string.Format("{0}\\DeviceProperties", Application.StartupPath);
                if (!System.IO.Directory.Exists(strDicName))
                    System.IO.Directory.CreateDirectory(strDicName);
                string strFileName = string.Format("{0}\\CMA180.config", strDicName);
                //
                clsXmlEntry entry = new clsXmlEntry("DeviceProperties", string.Empty);
                entry.AddSimpleEntry(this);
                config.NodeList.Add(entry);
                config.Save2File(strFileName);
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("SaveDevicePropertiesToFile: {0}", ex.ToString());
            }
        }
        #endregion
        #region Device properties
        [DisplayName("Enable average measurement")]
        [Category("Device Properties")]
        public bool IsEnableAvgMesMode
        {
            set
            {
                m_bIsEnableAvgMesMode = value;
                m_bIsDoPreset = false;
            }
            get { return m_bIsEnableAvgMesMode; }
        }

        [DisplayName("Measurement count")]
        [Category("Device Properties")]
        public int MeasurementAvgCnt
        {
            set
            {
                m_nMeasurementAvgCnt = value;
                m_bIsDoPreset = false;
            }
            get { return m_nMeasurementAvgCnt; }
        }
        public double GetSpan(FreqUnit freqUnit = FreqUnit.kHz)
        {
            clsLogManager.LogError("CMA180 not support get span");
            return double.MinValue;
        }
        public string GetTraceData()
        {
            return "CMA180 not support get trace Data";
        }
        public bool SetspecTraceMode(SpecTraceMode SpectraceMode = SpecTraceMode.Maxhold)
        {
            clsLogManager.LogError("CMA180 not support get trace Data");
            return false;
        }
        public bool SetScale(int DIV)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region IAllMess
        #endregion
    }
}
