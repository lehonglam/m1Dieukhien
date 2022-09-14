using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CFTWinAppCore.DeviceManager;
using InstrumentIOService;
using LogLibrary;
using Option;
using System.Windows.Forms;
namespace CFTSeqManager.MeasurementDevice
{
    [SysDevMetaAtt(typeof(clsAgilentN9020BSpecDev_Vsa), SysDevType.SPEC_DEV_Vsa, "Agilent N9020B Spectrum Vsa")]
    public class clsAgilentN9020BSpecDev_Vsa : IAccessDeviceService, ISpecDeviceN9020B_Vsa
    {
        private IISMessage m_IOService = null;
        private clsDeviceInfor m_devInfo;
        public clsAgilentN9020BSpecDev_Vsa(clsDeviceInfor devInfo)
        {
            m_devInfo = devInfo;
        }
        #region Device Base
        public bool InitDevice()
        {
            bool bOK = m_IOService.SendCommand("*CLS");
            bOK &= m_IOService.SendCommand("*RST");
            return true;
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
            return "N9020B_Vsa";
        }
        #endregion Device Base
        #region ISpecDevice
        public bool SetupForMeasurement()
        {
            bool bOK = false;
            //bOK = m_IOService.SendCommand("INIT:CONT 0");
            string strQuery = m_IOService.SendAndRead(":MEASure:CONF?");
            if (strQuery.IndexOf("DDEM") < 0)
                bOK = m_IOService.SendCommand(":MEASure:CONFigure DDEMod");
            return true;
        }

        public bool SetMarkerFre(Double duFreq, FreqUnit freqUnit = FreqUnit.Hz) // Đặt tần số Marker
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
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand(":CALC:MARK:X {0}", duFreqInput);
                else if (strQuery == "OBW")
                    m_IOService.SendCommand(":CALC:OBW:MARK:X {0}", duFreqInput);
                else if (strQuery == "ACP")
                    m_IOService.SendCommand(":CALC:ACP:MARK:X {0}", duFreqInput);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public double GetMarkerFre() // Đặt tần số Marker
        {
            try
            {
                string strFre = null;
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    strFre = m_IOService.SendAndRead(":CALC:MARK:X?");
                else if (strQuery == "OBW")
                    strFre = m_IOService.SendAndRead(":CALC:OBW:MARK:X?");
                else if (strQuery == "ACP")
                    strFre = m_IOService.SendAndRead(":CALC:ACP:MARK:X?");
                return double.Parse(strFre);
            }
            catch
            {
                return double.MinValue;
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
                        duFreqInput = duFreq * 1e6;
                        break;
                    case FreqUnit.GHz:
                        duFreqInput = duFreq * 1e9;
                        break;
                }
                m_IOService.SendCommand("SENS:FREQ:CENT {0}", duFreqInput);
                return true;
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
                str = m_IOService.SendAndRead(":TRAC:DATA? TRACE1");
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
                string strFre = m_IOService.SendAndRead("SENS:FREQ:CENT?");

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
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand("CALC:MARK:MAX");
                else if (strQuery == "OBW")
                    m_IOService.SendCommand("CALC:OBW:MARK:MAX");
                else if (strQuery == "ACP")
                    m_IOService.SendCommand("CALC:ACP:MARK:MAX");
                return true;

            }
            catch (Exception ec)
            {
                clsLogManager.LogError("CalcMarkerMax() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool init_Measurement(bool init)
        {
            m_IOService.SendCommand("INIT:IMM");
            return true;
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
            string strQuery = m_IOService.SendAndRead(":CONFigure?");
            switch (seaDirection)
            {
                case SearchDirection.Left:
                    if (strQuery == "SAN")
                        strCmd = string.Format(":CALCulate:MARKer{0}:MAXimum:LEFT", nMaxker);
                    else if (strQuery == "OBW")
                        strCmd = string.Format(":CALCulate:OBW:MARKer{0}:MAXimum:LEFT", nMaxker);
                    else if (strQuery == "ACP")
                        strCmd = string.Format(":CALCulate:ACP:MARKer{0}:MAXimum:LEFT", nMaxker);
                    break;
                case SearchDirection.Right:
                    if (strQuery == "SAN")
                        strCmd = string.Format(":CALCulate:MARKer{0}:MAXimum:RIGHt", nMaxker);
                    else if (strQuery == "OBW")
                        strCmd = string.Format(":CALCulate:OBW:MARKer{0}:MAXimum:RIGHt", nMaxker);
                    else if (strQuery == "ACP")
                        strCmd = string.Format(":CALCulate:ACP:MARKer{0}:MAXimum:RIGHt", nMaxker);
                    break;
                case SearchDirection.None:
                    if (strQuery == "SAN")
                        strCmd = string.Format(":CALCulate:MARKer{0}:MAXimum:NEXT", nMaxker);
                    else if (strQuery == "OBW")
                        strCmd = string.Format(":CALCulate:OBW:MARKer{0}:MAXimum:NEXT", nMaxker);
                    else if (strQuery == "ACP")
                        strCmd = string.Format(":CALCulate:ACP:MARKer{0}:MAXimum:NEXT", nMaxker);
                    break;
            }
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
            string strQuery = m_IOService.SendAndRead(":CONFigure?");
            if (strQuery == "SAN")
                m_IOService.SendCommand(":CALCulate:MARKer{0}:X  {1}MHZ;", nMaxker, MFre);
            else if (strQuery == "OBW")
                m_IOService.SendCommand(":CALCulate:OBW:MARKer{0}:X  {1}MHZ;", nMaxker, MFre);
            else if (strQuery == "ACP")
                m_IOService.SendCommand(":CALCulate:ACP:MARKer{0}:X  {1}MHZ;", nMaxker, MFre);
            return true;
        }
        public bool SetSpan(double SpanValue, FreqUnit freqUnit = FreqUnit.Hz)
        {
            try
            {
                double duSpanValue = SpanValue;
                switch (freqUnit)
                {
                    case FreqUnit.Hz:
                        duSpanValue = SpanValue;
                        break;
                    case FreqUnit.kHz:
                        duSpanValue = SpanValue * 1e3;
                        break;
                    case FreqUnit.MHz:
                        duSpanValue = SpanValue * 1e6;
                        break;
                }
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand("SENS:FREQ:SPAN {0};", duSpanValue);
                else if (strQuery == "OBW")
                    m_IOService.SendCommand("SENS:OBW:FREQ:SPAN {0};", duSpanValue);
                else if (strQuery == "ACP")
                    m_IOService.SendCommand("SENS:ACP:FREQ:SPAN {0};", duSpanValue);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetSpan() catched an exception: " + ec.Message);
                return false;
            }
        }
        public double GetSpan(FreqUnit freqUnit = FreqUnit.Hz)
        {
            try
            {
                string strSpan = null;
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    strSpan = m_IOService.SendAndRead("SENS:FREQ:SPAN?");
                else if (strQuery == "OBW")
                    strSpan = m_IOService.SendAndRead("SENS:OBW:FREQ:SPAN?");
                else if (strQuery == "ACP")
                    strSpan = m_IOService.SendAndRead("SENS:ACP:FREQ:SPAN?");

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
        public bool SetReferentLevel(double ReferValue)
        {
            try
            {
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand(":DISPlay:WINDow:TRACe:Y:SCALe:RLEVel {0}dBm", ReferValue);
                else if (strQuery == "OBW")
                    m_IOService.SendCommand(":DISPlay:OBW:WINDow:TRACe:Y:SCALe:RLEVel {0}dBm", ReferValue);
                else if (strQuery == "ACP")
                    m_IOService.SendCommand(":DISPlay:ACP:WINDow:TRACe:Y:SCALe:RLEVel {0}dBm", ReferValue);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetReferentLevel() catched an exception: " + ec.Message);
                return false;
            }
        }

        public bool SetRefLevelOffset(double ReferOffset)
        {
            try
            {
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand(":DISPlay:WINDow:TRACe:Y:SCALe:RLEVel:OFFSet {0}", ReferOffset);
                else if (strQuery == "OBW")
                    m_IOService.SendCommand(":DISPlay:OBW:WINDow:TRACe:Y:SCALe:RLEVel:OFFSet {0}", ReferOffset);
                else if (strQuery == "ACP")
                    m_IOService.SendCommand(":DISPlay:ACP:WINDow:TRACe:Y:SCALe:RLEVel:OFFSet {0}", ReferOffset);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetRefLevelOffset() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool EnableMarker(bool bOnOff, int nMakerIndex = 1)
        {
            string strONOFF = "OFF";
            if (bOnOff)
            {
                strONOFF = "ON";
            }
            string strQuery = m_IOService.SendAndRead(":CONFigure?");
            if (strQuery == "SAN")
                m_IOService.SendCommand(":CALCulate:MARKer{0}:STATe {1}", nMakerIndex, strONOFF);
            else if (strQuery == "OBW")
                m_IOService.SendCommand(":CALCulate:OBW:MARKer{0}:STATe {1}", nMakerIndex, strONOFF);
            else if (strQuery == "ACP")
                m_IOService.SendCommand(":CALCulate:ACP:MARKer{0}:STATe {1}", nMakerIndex, strONOFF);
            return true;
        }
        public bool SetSpecAttenuation(double AttenuatonValue = 40)
        {
            if (AttenuatonValue >= 0)
            {
                m_IOService.SendCommand(":SENSe:POWer:RF:ATTenuation {0}dB", AttenuatonValue);
            }
            else
            {
                m_IOService.SendCommand(":SENSe:POWer:RF:ATTenuation:AUTO 1");
            }
            return true;
        }
        public double GetValueAtMaker(MarkerValueType valueType = MarkerValueType.Level)
        {
            try
            {
                string makerValue = null;
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (valueType == MarkerValueType.Level)
                {
                    if (strQuery == "SAN")
                        makerValue = m_IOService.SendAndRead("CALC:MARK:Y?");
                    else if (strQuery == "OBW")
                        makerValue = m_IOService.SendAndRead("CALC:OBW:MARK:Y?");
                    else if (strQuery == "ACP")
                        makerValue = m_IOService.SendAndRead("CALC:ACP:MARK:Y?");
                }
                else
                {
                    if (strQuery == "SAN")
                        makerValue = m_IOService.SendAndRead("CALC:MARK:X?");
                    else if (strQuery == "OBW")
                        makerValue = m_IOService.SendAndRead("CALC:OBW:MARK:X?");
                    else if (strQuery == "ACP")
                        makerValue = m_IOService.SendAndRead("CALC:ACP:MARK:X?");
                }
                return double.Parse(makerValue);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("GetValueAtMaker() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public double GetValueAtIndexMaker(int IndexMarker, MarkerValueType valueType = MarkerValueType.Level)
        {
            string makerValue = null;
            string strQuery = m_IOService.SendAndRead(":CONFigure?");
            if (valueType == MarkerValueType.Level)
            {
                makerValue = m_IOService.SendAndRead("CALC:MARK" + IndexMarker.ToString() + ":Y?");
                if (strQuery == "SAN")
                    makerValue = m_IOService.SendAndRead("CALC:MARK" + IndexMarker.ToString() + ":Y?");
                else if (strQuery == "OBW")
                    makerValue = m_IOService.SendAndRead("CALC:OBW:MARK" + IndexMarker.ToString() + ":Y?");
                else if (strQuery == "ACP")
                    makerValue = m_IOService.SendAndRead("CALC:ACP:MARK" + IndexMarker.ToString() + ":Y?");
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
            try
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
                        duRBwInput = duRbW * 1e3;
                        break;
                }
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand(":SENSe:BANDwidth:RESolution {0}khz", duRBwInput);
                else if (strQuery == "OBW")
                    m_IOService.SendCommand(":SENSe:OBW:BANDwidth:RESolution {0}khz", duRBwInput);
                else if (strQuery == "ACP")
                    m_IOService.SendCommand(":SENSe:ACP:BANDwidth:RESolution {0}khz", duRBwInput);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetResolutionBandwidth() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetResolutionBandwidthAuto()
        {
            try
            {
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand(":SENSe:BANDwidth:RESolution:AUTO ON");
                else if (strQuery == "OBW")
                    m_IOService.SendCommand(":SENSe:OBW:BANDwidth:RESolution:AUTO ON");
                else if (strQuery == "ACP")
                    m_IOService.SendCommand(":SENSe:ACP:BANDwidth:RESolution:AUTO ON");
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetResolutionBandwidth() catched an exception: " + ec.Message);
                return false;
            }
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
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand(":SENSe:BANDwidth:VID {0}khz", duVBW);
                else if (strQuery == "OBW")
                    m_IOService.SendCommand(":SENSe:OBW:BANDwidth:VID {0}khz", duVBW);
                else if (strQuery == "ACP")
                    m_IOService.SendCommand(":SENSe:ACP:BANDwidth:VID {0}khz", duVBW);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVBW() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool Set_TraceMode(TraceMode mode, bool enable = true)
        {
            try
            {
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand(string.Format(":TRACe1:MODE {0}", mode.ToString()));
                else if (strQuery == "OBW")
                    m_IOService.SendCommand(string.Format(":TRAC:OBW:TYPE {0}", mode.ToString()));
                else if (strQuery == "ACP")
                    m_IOService.SendCommand(string.Format(":TRAC:ACP:TYPE {0}", mode.ToString()));
                Thread.Sleep(500);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Set_TraceMode() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetspecTraceMode(SpecTraceMode SpectraceMode = SpecTraceMode.write)
        {
            string strTraceMode = "WRITe";
            switch (SpectraceMode)
            {
                case SpecTraceMode.write:
                    strTraceMode = "WRITe";
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
            string strQuery = m_IOService.SendAndRead(":CONFigure?");
            if (strQuery == "SAN")
                m_IOService.SendCommand(string.Format(":TRACe1:TYPE {0}", strTraceMode));
            else if (strQuery == "OBW")
                m_IOService.SendCommand(string.Format(":TRACe1:OBW:TYPE {0}", strTraceMode));
            else if (strQuery == "ACP")
                m_IOService.SendCommand(string.Format(":TRACe1:ACP:TYPE {0}", strTraceMode));
            return true;
        }
        public bool SetScale(int DIV)
        {
            try
            {
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand(":DISPlay:WINDow:TRACe:Y:PDIVision {0}", DIV);
                else if (strQuery == "OBW")
                    m_IOService.SendCommand(":DISPlay:OBW:WINDow:TRACe:Y:PDIVision {0}", DIV);
                else if (strQuery == "ACP")
                    m_IOService.SendCommand(":DISPlay:ACP:WINDow:TRACe:Y:PDIVision {0}", DIV);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetScale() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetSweepPoint(int swPoint)
        {
            try
            {
                string strQuery = m_IOService.SendAndRead(":CONFigure?");
                if (strQuery == "SAN")
                    m_IOService.SendCommand(":SWE:POIN {0}", swPoint);
                else if (strQuery == "OBW")
                    m_IOService.SendCommand(":OBW:SWE:POIN {0}", swPoint);
                else if (strQuery == "ACP")
                    m_IOService.SendCommand(":ACP:SWE:POIN {0}", swPoint);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetSweepPoint() catched an exception: " + ec.Message);
                return false;
            }
        }
        /*
         * Measurement type:
         * VECTor | ADEMod | DDEMod | GSM | CUSTomofdm | RFID | LTE | C2K | TDSCdma |
         * EVDO | WCDMa | W16D | W16E | W11A | W11B | W11N | MBOFdm | TEDS | CDEMod | LTEA | SPECtrum |
         * RADar | PULSe | CQUality | DOCSis | DOCupstream | P5G | NR5G | NBIot | HRPuwb | CSOunding | 
         * FLEXframe | ADVancedradar
         */
        public bool SetVsaMeasurementType(string mod = "DDEM")
        {
            try
            {
                string strQuery = m_IOService.SendAndRead(":MEASure:CONF?");
                if (strQuery != mod)
                    m_IOService.SendCommand(":MEASure:CONFigure {0}", mod);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetupVsaMode() catched an exception: " + ec.Message);
                return false;
            }
        }
        /*
         * Digital Demod format
         * "Qam16" | "Qam32" | "Qam64" | "Qam256" | "Qpsk" | "DifferentialQpsk" | "Pi4DifferentialQpsk" |
         * "OffsetQpsk" | "Bpsk" | "Psk8" | "Msk" | "Msk2" | "Fsk2" | "Fsk4" | "DvbQam16" | "DvbQam32" | "DvbQam64" | "Vsb8" |
         * "Vsb16" | "Edge" | "Fsk8" | "Fsk16" | "Qam128" | "DifferentialPsk8" | "Qam512" | "Qam1024" | "Apsk16" | "Apsk16Dvb" |
         * "Apsk32" | "Apsk32Dvb" | "DvbQam128" | "DvbQam256" | "Pi8DifferentialPsk8" | "CpmFM" | "Star16Qam" | "Star32Qam" |
         * "CustomApsk" | "ShapedOffsetQpsk" | "Qam2048" | "Qam4096"
         */
        public bool SetVsaDdemFormat(string format = "Pi4DifferentialQpsk")
        {
            try
            {
                string strQuery = m_IOService.SendAndRead(":DDEMod:FORMat:MOD?");
                if (strQuery != format)
                    m_IOService.SendCommand(":DDEMod:FORMat:MOD \"{0}\"", format);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaDdemFormat() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaDdemSymbolRate(double rate)
        {
            try
            {
                m_IOService.SendCommand(":DDEMod:FORMat:SRATe {0}", rate);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaDdemSymbolRate() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaDdemPointPerSymbol(double point)
        {
            try
            {
                m_IOService.SendCommand(":DDEMod:FORMat:SYMBol:POINts {0}", point);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaDdemPointPerSymbol() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaDdemResultLenght(double length)
        {
            try
            {
                m_IOService.SendCommand(":DDEMod:FORMat:RLENgth {0}", length);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaDdemResultLenght() catched an exception: " + ec.Message);
                return false;
            }
        }
        public double GetVsaDdemResultEVM()
        {
            try
            {
                string strFre = null;
                string strQuery = m_IOService.SendAndRead(":TRACe4:DATA:TABLe:VALue?");
                string[] arrStrResult = strQuery.Split(',');
                return double.Parse(arrStrResult[0]);
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("GetVsaDdemResultEVM() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public bool SetSWE(double Sweeptime, TimeUnit timeUnit)
        {
            throw new NotImplementedException();
        }
        public bool SetVsaSPAN(double Freq, FreqUnit freqUnit = FreqUnit.MHz)
        {
            try
            {
                m_IOService.SendCommand($":FREQuency:SPAN {Freq} {freqUnit}");
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaSPAN() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaRng(double leveldBm)
        {
            try
            {
                double VoltageValue = Math.Pow(10, (leveldBm - 10) / 20);
                m_IOService.SendCommand($":INPut:ANALog:RANGe:CHANnel1 {VoltageValue}");
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaRef() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaAutoRng()
        {
            try
            {
                m_IOService.SendCommand(":INPut:ANALog:RANGe:AUTO:CHANnel1");
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaAutoRng() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool PresetVsa()
        {
            try
            {
                m_IOService.SendCommand(":SYSTem:PRESet:DISPlay");
               // m_IOService.SendCommand(":SYSTem:PRESet:MEASurement");
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("PresetVsa() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaAutoCal(bool OnOff)
        {
            try
            {
                if (OnOff == true)
                {
                    m_IOService.SendCommand(":CAL:AUTO 1");
                }
                else
                {
                    m_IOService.SendCommand(":CAL:AUTO 0");
                }
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaAutoCal() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVSALayout(int row, int col)
        {
            try
            {
                m_IOService.SendCommand($":DISPlay:LAYout {row}, {col}");
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetLayout() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaDdemFilter(VSATypeFilter typeFilter)
        {
            try
            {
                m_IOService.SendCommand(":DDEMod:FILTer \"{0}\"", typeFilter.ToString());
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaDdemFilter() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaDdemFilterRef(VSATypeFilterRef typeFilterRef)
        {
            try
            {
                m_IOService.SendCommand(":DDEMod:FILTer:REFerence \"{0}\"", typeFilterRef.ToString());
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaDdemFilterRef() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaDdemFilterABT(double abt)
        {
            try
            {
                m_IOService.SendCommand(":DDEMod:FILTer:ABT {0}", abt);
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaDdemFilterABT() catched an exception: " + ec.Message);
                return false;
            }
        }
        public bool SetVsaInputDataHW()
        {
            try
            {
                m_IOService.SendCommand(":INPut:DATA HW");
                return true;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SetVsaInputDataHW() catched an exception: " + ec.Message);
                return false;
            }
        }
        #endregion ISpecDevice  
    }
}
