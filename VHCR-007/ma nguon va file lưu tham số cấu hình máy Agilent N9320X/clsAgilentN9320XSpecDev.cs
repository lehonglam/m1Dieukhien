using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CFTWinAppCore.DeviceManager;
using InstrumentIOService;
using LogLibrary;
using Option;

namespace CFTSeqManager.MeasurementDevice
{
    [SysDevMetaAtt(typeof(clsAgilentN9320XSpecDev), SysDevType.SPEC_DEV, "Agilent N9320X Spectrum")]
    public class clsAgilentN9320XSpecDev : IAccessDeviceService, ISpecDevice, IFreqCounter
    {
        private IISMessage m_IOService = null;
        private clsDeviceInfor m_devInfo;
        public clsAgilentN9320XSpecDev(clsDeviceInfor devInfo)
        {
            m_devInfo = devInfo;
        }
        #region Device Base
        public  bool InitDevice()
        {
            bool bOK = false;
            string strQuery = m_IOService.SendAndRead(":CONFigure?");
            if (strQuery.IndexOf("FM") >= 0)
                bOK = m_IOService.SendCommand(":CONFigure:SANalyzer");
            bOK = m_IOService.SendCommand("*CLS");
            bOK = m_IOService.SendCommand(":SYST:PRES");
            //m_IOService.SendCommand("*RST");
            //m_IOService.SendCommand(":SYSTem:BRIGhtness LOW");
            bOK = m_IOService.SendCommand("UNIT:POW DBM");
            bOK = m_IOService.SendCommand(":CALCulate:MARKer{0}:STATe ON", 1);
            /*if (m_devInfo.ShowIOLog)
            {
                m_IOService.SendCommand(":DISPlay:ENABle ON");
            }
            else
                m_IOService.SendCommand(":DISPlay:ENABle OFF");*/
            return bOK;
        }
        public  bool Connect2Device(IDeviceInfor devInfo)
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
                if (m_IOService is clsCusTcpService)
                {
                    clsCusTcpService tcpService = m_IOService as clsCusTcpService;
                    tcpService.RegisterCommandSuccessCode("1");
                    tcpService.RegisterResponseSplitCode(null);
                    tcpService.ResgisterOpcCommand("*OPC?");
                    int nPort = 5025;
                    if (!int.TryParse(m_devInfo.AddtionDevPara, out nPort))
                        nPort = 5025;
                    strDevAddress = string.Format("{0}:{1}", m_devInfo.DevAddress, nPort);
                }
            }
            bool bResult = m_IOService.Connect(strDevAddress, devInfo.IOTimeOut);
            return bResult;
        }
        public  bool IsDeviceConnected()
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
        public  void DisconnectDevice()
        {
            if (m_IOService == null) return;
            m_IOService.Dispose();
            m_IOService = null;
        }
        public  bool SetIOTimeOut(int nTimeOut)
        {
            m_IOService.SetIOTimeout(nTimeOut);
            return true;
        }
        public object GetIOLibrary()
        {
            return m_IOService;
        }
        public bool ShowIOLog
        {
            set { m_IOService.EnableLog = value; }
            get { return m_IOService.EnableLog; }
        }
        public void ShowDeviceTestDialog()
        {

        }
        public bool ShowDeviceProperties()
        {
            return false;
        }
        public bool IsDeviceSupportCusDevProperties()
        {
            return false;
        }
        public string GetDeviceFriendlyName()
        {
            return "N9320";
        }
        #endregion Device Base
        #region ISpecDevice
        public bool SetupForMeasurement()
        {
            bool bOK = false;
            bOK = m_IOService.SendCommand("INIT:CONT 0");
            string strQuery = m_IOService.SendAndRead(":CONFigure?");
            if(strQuery.IndexOf("SAN") < 0)
            	bOK = m_IOService.SendCommand(":CONFigure:SANalyzer");
           return bOK;
        }

        public bool setMarkerFre(Double duFreq,FreqUnit freqUnit=FreqUnit.Hz) // Đặt tần số Marker
        {
            try
            {
                double duFreqInput = duFreq;
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        duFreqInput = duFreq;
                        break;
                    case FreqUnit.kHz:
                        duFreqInput = duFreq * 1e3;
                        break;
                    case FreqUnit.MHz:
                        duFreqInput = duFreq * 1e6;
                        break;
                }
                bool bOK = m_IOService.SendCommand(":CALC:MARK:X {0}", duFreqInput);
                return bOK;
            }
            catch
                        
            {
                return false;
            }
        }
        public bool SetCenterFreq(double duFreq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            try
            {
                double duFreqInput = duFreq;
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        duFreqInput = duFreq;
                        break;
                    case FreqUnit.kHz:
                        duFreqInput = duFreq * 1e3;
                        break;
                    case FreqUnit.MHz:
                        duFreqInput = duFreq*1e6;
                        break;
                }
                bool bOK = m_IOService.SendCommand("SENS:FREQ:CENT {0}", duFreqInput);
                return bOK;
            }
            catch
            {
                return false;
            }
            
        }

        public string GetTraceData()
        {
            try
            {
                string str = null;
                str= m_IOService.SendAndRead(":TRAC:DATA? TRACE1");
                return str;
            }
            catch
            {
                return null;
            }
        }


        public double GetCenterFreq(FreqUnit freqUnit = FreqUnit.Hz)
        {
            try
            {
                string strFre=m_IOService.SendAndRead("SENS:FREQ:CENT?");

                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        return double.Parse(strFre);
                    case FreqUnit.kHz:
                        return double.Parse(strFre) / 1000;
                    case FreqUnit.MHz:
                        return double.Parse(strFre) / 1e6;
                    default:
                        return double.Parse(strFre);
                }
            }
            catch
            {
                clsLogManager.LogError("Get Frequency center of spectrum FAIL");
                return Double.MinValue;
            }
        }

        public bool CalcMarkerMax(int nMaxker = 1)
        {
            try
            {
                // m_IOService.SendCommand("INIT:IMM");
                return m_IOService.SendCommand("CALC:MARK:MAX"); // Set marker to the maximum peak -- CALC:MARK1:MAX 
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("CalcMarkerMax() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool init_Measurement(bool init)
        {
            bool bOK = m_IOService.SendCommand("INIT:IMM");
            return bOK;
        }
        public bool SetSpecMeasurementMode(MeasurementMode mesMode = MeasurementMode.Single)
        {
            bool bOK = false;
            if (mesMode == MeasurementMode.Single)
            {
                bOK = m_IOService.SendCommand("INIT:CONT 0");
            }
            else
            {
                bOK = m_IOService.SendCommand("INIT:CONT 1");
            }
            return bOK;
        }
        public bool CalcNextMarkerMax(int nMaxker = 1, SearchDirection seaDirection = SearchDirection.None)
        {
            bool bOK = true;
            string strCmd = string.Empty;
            switch (seaDirection)
            {
                case SearchDirection.Left:
                    strCmd = string.Format(":CALCulate:MARKer{0}:MAXimum:LEFT", nMaxker); //Places the selected marker on the next highest signal peak to the left of the current marked peak
                    break;
                case SearchDirection.Right:
                    strCmd = string.Format(":CALCulate:MARKer{0}:MAXimum:RIGHt", nMaxker); // Places the selected marker on the next highest signal peak to the left of the current marked peak
                    break;
                case SearchDirection.None:
                    strCmd = string.Format(":CALCulate:MARKer{0}:MAXimum:NEXT", nMaxker); // Places the selected marker on the next highest signal peak from the current marked peak
                    break;
            }
            //m_IOService.SendCommand("INIT:IMM");
            bOK = m_IOService.SendCommand(strCmd);
            return bOK;
        }
        public bool CalcMarkerAtFre(double duFreq, FreqUnit freqUnit = FreqUnit.MHz, int nMaxker = 1)
        {
            double MFre = 0;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    MFre = duFreq / 1e6;
                    break;
                case FreqUnit.kHz:
                    MFre = duFreq / 1e3;
                    break;
                case FreqUnit.MHz:
                    MFre = duFreq;
                    break;
            }
            return m_IOService.SendCommand(":CALCulate:MARKer{0}:X  {1}MHZ;", nMaxker, MFre);
        }
        public bool SetSpan(double SpanValue = 10, FreqUnit freqUnit = FreqUnit.kHz)
        {
            double duSpanValue = SpanValue;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    duSpanValue = SpanValue;
                    break;
                case FreqUnit.kHz:
                    duSpanValue = SpanValue*1e3;
                    break;
                case FreqUnit.MHz:
                    duSpanValue = SpanValue * 1e6;
                    break;
            }
            return m_IOService.SendCommand("SENS:FREQ:SPAN {0}Hz;", duSpanValue);
        }
        public  double GetSpan(FreqUnit freqUnit = FreqUnit.kHz)
        {
            try
            {
                string strSpan = m_IOService.SendAndRead("SENS:FREQ:SPAN?");
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        return double.Parse(strSpan);
                    case FreqUnit.kHz:
                        return double.Parse(strSpan) / 1e3;
                        
                    case FreqUnit.MHz:
                        return double.Parse(strSpan) / 1e6;
                    default:
                        return double.Parse(strSpan);
                        
                }
                
            }
            catch
            {
                clsLogManager.LogError("Get Span of Spectrum FAIL");
                return double.MinValue;
            }
        }
        public bool SetReferentLevel(double ReferValue = 20)
        {
            bool bOK = m_IOService.SendCommand(":DISPlay:WINDow:TRACe:Y:SCALe:RLEVel {0} dBm", ReferValue);
            double duPeakThread = ReferValue -90;
            //m_IOService.SendCommand(":CALCulate:MARKer:PEAK:THReshold {0}", duPeakThread);
             return bOK;
        }
        public bool SetRefLevelOffset(double ReferOffset = 0)
        {
            try
            {
                bool bOK = m_IOService.SendCommand(":DISPlay:WINDow:TRACe:Y:SCALe:RLEVel:OFFSet {0}", ReferOffset);
                return bOK;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetRefLevelOffset() catched an exception: " + ec.Message);
                return false;
            }
        }
        public double Read_RefLevelOffSet()
        {
            try
            {
                double duValue = double.MinValue;
                string RefLevel_Offset = m_IOService.SendAndRead(":DISPlay:WINDow:TRACe:Y:SCALe:RLEVel:OFFSet?");
                if (!double.TryParse(RefLevel_Offset, out duValue))
                {
                    return double.MinValue;
                }
                else
                {
                    return duValue;
                }
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Read_RefLevelOffSet() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public bool EnableMarker(bool bOnOff, int nMakerIndex = 1)
        {
            string strONOFF = "OFF";
            if (bOnOff)
            {
                strONOFF = "ON";
            }
            return m_IOService.SendCommand(":CALCulate:MARKer{0}:STATe {1}", nMakerIndex,strONOFF);
        }
        public bool SetSpecAttenuation(double AttenuatonValue = 40)
        {
            bool bOK = false;
            //m_IOService.SendCommand("DANalyse:POWer:ATTenuation:AUTO OFF");
            if (AttenuatonValue >= 0)
            {
                bOK = m_IOService.SendCommand(":SENSe:POWer:RF:ATTenuation {0}", AttenuatonValue);
            }
            else
            {
                bOK = m_IOService.SendCommand(":SENSe:POWer:RF:ATTenuation:AUTO 1");
            }
            return bOK;
        }
        public double GetValueAtMaker(MarkerValueType valueType= MarkerValueType.Level)
        {
            try
            {
                string makerValue = null;
                if (valueType == MarkerValueType.Level)
                {
                    makerValue = m_IOService.SendAndRead("CALC:MARK:Y?");
                }
                else
                {
                    makerValue = m_IOService.SendAndRead("CALC:MARK:X?");
                }
                return double.Parse(makerValue);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("GetValueAtMaker() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public double GetValueAtIndexMaker(int IndexMarker,  MarkerValueType valueType = MarkerValueType.Level)
        {
            string makerValue = null;
            if (valueType == MarkerValueType.Level)
            {
                makerValue = m_IOService.SendAndRead("CALC:MARK" + IndexMarker.ToString() + ":Y?");
            }
            else
            {
                makerValue = m_IOService.SendAndRead("CALC:MARK" + IndexMarker.ToString() + ":X?");
            }
            return double.Parse(makerValue);
        }

        public bool SetStartFreq(double duStartFreq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            double duInputFreq = duStartFreq;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    break;
                case FreqUnit.kHz:
                    duInputFreq = duStartFreq * 1e3;
                    break;
                case FreqUnit.MHz:
                    duInputFreq = duStartFreq * 1e6;
                    break;
            }
            return m_IOService.SendCommand(":SENSe:FREQuency:STARt {0}", duInputFreq);
        }

        public bool SetStopFreq(double duStopFreq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            double duInputFreq = duStopFreq;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    break;
                case FreqUnit.kHz:
                    duInputFreq = duStopFreq * 1e3;
                    break;
                case FreqUnit.MHz:
                    duInputFreq = duStopFreq * 1e6;
                    break;
            }
            return m_IOService.SendCommand(":SENSe:FREQuency:STOP {0}", duInputFreq);
        }

        public bool SetResolutionBandwidth(double duRbW, FreqUnit freqUnit = FreqUnit.kHz)
        {
            double duRBwInput = duRbW;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    duRBwInput = duRbW / 1e3;
                    break;
                case FreqUnit.kHz:
                    duRBwInput = duRbW;
                    break;
                case FreqUnit.MHz:
                    duRBwInput = duRbW *1e3;
                    break;
            }            
	            return m_IOService.SendCommand(":SENSe:BANDwidth:RESolution {0}khz", duRBwInput);
        }
        public bool SetVBW(double VBW, FreqUnit freqUnit = FreqUnit.kHz)
        {
            try
            {
                double duVBW = VBW;
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        duVBW = VBW / 1e3;
                        break;
                    case FreqUnit.kHz:
                        duVBW = VBW;
                        break;
                    case FreqUnit.MHz:
                        duVBW = VBW * 1e3;
                        break;
                }
                return m_IOService.SendCommand(":SENSe:BANDwidth:RESolution {0}khz", duVBW);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVBW() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool Set_TraceMode(TraceMode mode, bool enable = true)
        {
            bool bOK = false;
            try
            {
                string cmd = string.Format(":TRACe1:MODE {0}", mode.ToString());
                bOK= m_IOService.SendCommand(cmd);
                if (bOK == false)
                    return false;
                Thread.Sleep(500);
                string response = m_IOService.SendAndRead(":TRACe1:MODE?");
                clsLogManager.LogReport("MODE? => {0}", response);
                if (mode.ToString().ToUpper().Contains(response.ToUpper()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Set_TraceMode() catched an exception: " + ec.Message);
                return false;
            }
        }
        public string GetSN_fromConfig()
        {
            return m_devInfo.DeviceSN;
        }
        public double ReadAtten()
        {
            try
            {
                string strAtt = string.Empty;
                double duAtt = double.MinValue;
                strAtt = m_IOService.SendAndRead(":SENSe:POWer:RF:ATTenuation?");
                if (double.TryParse(strAtt, out duAtt))
                {
                    return duAtt;
                }
                else
                {
                    return double.MinValue;
                }
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("ReadAtten() error: " + ec.Message);
                return double.MinValue;
            }
        }
        #endregion ISpecDevice
        #region IFreqCounter
        public bool Select_InputPort(InputPort port = InputPort.RFCom)
        {
            try
            {
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Select_InputPort() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetMeasurementFreq(double duFreq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            bool bResult = false;
            double duFreqMes = 0;
            double duValue = 0;
            string strQuery = m_IOService.SendAndRead(":CONFigure?");
            if(strQuery.IndexOf("FM") <0)
                bResult=m_IOService.SendCommand(":CONFigure:FM");
            bResult=m_IOService.SendCommand("DANalyse:CONTinuous OFF");
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    duFreqMes = duFreq;
                    break;
                case FreqUnit.kHz:
                    duFreqMes = duFreq * 1e3;
                    break;
                case FreqUnit.MHz:
                    duFreqMes = duFreq * 1e6;
                    break;
            }
            bResult = m_IOService.SendCommand("DANalyse:CARR:FREQuency {0}", duFreqMes);
            bResult = m_IOService.SendCommand("DANalyse:FM:IFBWidth 120 KHZ");
            return bResult;
        }
public double GetFrequencyError(double duTransFreq, int nChannel = 1, FreqUnit freqUnit = FreqUnit.Hz)
        {
            double duValue = 0;
            //m_IOService.SendCommand("RECE:FRESN 0");
            string strResult = m_IOService.SendAndRead("MEASU:TXOffset?");
            duValue = double.Parse(strResult);
            duValue = duValue*1000;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    break;
                case FreqUnit.kHz:
                    duValue = duValue / 1e3;
                    break;
                case FreqUnit.MHz:
                    duValue = duValue / 1e6;
                    break;
            }
            return duValue;
        }

        public double[] GetStatisticalData(double duTransFreq, int nChannel = 1, int nCnt = 10, FreqUnit freqUnit = FreqUnit.Hz)
        {
            throw new NotImplementedException();
        }
        public double GetFrequencyDeviation(double duTransFreq, int nChannel = 1, FreqUnit freqUnit = FreqUnit.Hz, bool bWaitStable = false)
        {
            double duValue = 0;
            string strQuery = m_IOService.SendAndRead(":CONFigure?");
            if(strQuery.IndexOf("FM") <0)
            	m_IOService.SendCommand(":CONFigure:FM");
            m_IOService.SendCommand("DANalyse:CONTinuous OFF");
            m_IOService.SendCommand("DANalyse:FM:AVERage OFF");
            //
            /*double duFreqInput = duTransFreq;
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        duFreqInput = duTransFreq;
                        break;
                    case FreqUnit.kHz:
                        duFreqInput = duTransFreq * 1e3;
                        break;
                    case FreqUnit.MHz:
                        duFreqInput = duTransFreq*1e6;
                        break;
                }
            m_IOService.SendCommand("DANalyse:CARR:FREQuency {0}", duFreqInput);*/
            if(bWaitStable)
            	WaitMeasurementFrequencyDeviationStable();
            m_IOService.SendCommand("DANalyse:IMMediate");
            string strResult = m_IOService.SendAndRead("DANalyse:CARR:FDUL?");
            duValue = double.Parse(strResult);
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    break;
                case FreqUnit.kHz:
                    duValue = duValue / 1e3;
                    break;
                case FreqUnit.MHz:
                    duValue = duValue / 1e6;
                    break;
            }
            return duValue;
        }
        public double WaitMeasurementFrequencyDeviationStable(int nWaitTimeOut = 5000, double duDelta = 0.05)
        {
            double duCurrFreqDeviation = 0;
            int nPassCnt = 0;
            int nBeginTime = Environment.TickCount;
            int nTimeOut = 0;
            //
            m_IOService.SendCommand("DANalyse:IMMediate");
            string strResult = m_IOService.SendAndRead("DANalyse:CARR:FDUL?");
            double duPrevFreqDeviation = double.Parse(strResult);
            while (true)
            {
                Thread.Sleep(300);
                m_IOService.SendCommand("DANalyse:IMMediate");
                strResult = m_IOService.SendAndRead("DANalyse:CARR:FDUL?");
                duCurrFreqDeviation = double.Parse(strResult);
                if (duCurrFreqDeviation < 1)
                    duDelta = 0.1 * duCurrFreqDeviation; //10%
                else
                    duDelta = 0.05 * duCurrFreqDeviation; //10%
                if (Math.Abs(duCurrFreqDeviation - duPrevFreqDeviation) <= duDelta)
                    nPassCnt++;
                if (nPassCnt >= 1) break;
                duPrevFreqDeviation = duCurrFreqDeviation;
                nTimeOut = Environment.TickCount - nBeginTime;
                if (nTimeOut >= nWaitTimeOut) break;
            }
            return duCurrFreqDeviation;
        }
        public bool SetspecTraceMode(SpecTraceMode SpectraceMode=SpecTraceMode.Maxhold)
        {
            string strTraceMode = "MAXHold";
            switch (SpectraceMode)
            {
                case SpecTraceMode.write:
                    strTraceMode = "write";
                    break;
                case SpecTraceMode.Maxhold:
                    strTraceMode = "MAXHold";
                    break;
                case SpecTraceMode.Minhold:
                    strTraceMode = "MINHold";
                    break;
                default:
                    break;
            }
            return m_IOService.SendCommand(string.Format(":TRACe1:MODE {0}",strTraceMode));
      }
        public bool SetDemodulatioType(DemodulationType type, double RFFreq, FreqUnit unit = FreqUnit.MHz)
        {
            throw new NotImplementedException();
        }
        public bool SetScale(int DIV)
        {
            bool bOK = false;
            try
            {
                return m_IOService.SendCommand(":DISPlay:WINDow:TRACe:Y:PDIVision {0}", DIV);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetScale() catched an exception: " + ec.Message);
                return false;
            }
        }
        #endregion
    }
}
