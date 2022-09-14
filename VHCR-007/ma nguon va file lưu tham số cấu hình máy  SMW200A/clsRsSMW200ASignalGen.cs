using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CFTWinAppCore.DeviceManager;
using InstrumentIOService;
using LogLibrary;
using Option;
using CFTWinAppCore.Helper;
using System.Threading;

namespace CFTSeqManager.MeasurementDevice
{
    [SysDevMetaAtt(typeof(clsRsSMW200ASignalGen), SysDevType.SIG_GEN, "SMW200A Signal Gen")]
    public class clsRsSMW200ASignalGen : IAccessDeviceService, ISignalGenDevice
    {
        private IISMessage m_IOService = null;
        private clsDeviceInfor m_devInfo;

        public clsRsSMW200ASignalGen(clsDeviceInfor devInfo)
        {
            m_devInfo = devInfo;
        }
        #region IAccessDeviceService
        public bool InitDevice()
        {
            if (m_IOService.SendCommand("*RST"))
                return true;
            else
                return false;
        }

        public bool Connect2Device(IDeviceInfor devInfo)
        {
            bool bResult = false;
            if (IsDeviceConnected())
                return true;
            //
            if (m_IOService == null)
            {
                m_IOService = clsFactoryIOService.CreateIOService(devInfo.IOServiceType);
                m_IOService.EnableLog = m_devInfo.ShowIOLog;
            }
            if (m_IOService is clsCusTcpService)
            {
                clsCusTcpService tcpService = m_IOService as clsCusTcpService;
                int nPort = 5025;
                if (!int.TryParse(m_devInfo.AddtionDevPara, out nPort))
                    nPort = 5025;
                string strDevAddress = string.Format("{0}:{1}", m_devInfo.DevAddress, nPort);
                bResult = m_IOService.Connect(strDevAddress, devInfo.IOTimeOut);
            }
            else
                bResult = m_IOService.Connect(devInfo.DevAddress, devInfo.IOTimeOut);
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
            throw new NotImplementedException();
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
            return "Signal gen SMW200A";
        }
        #endregion
        #region ISignalGenDevice
        public bool SignalGenSetup()
        {
            bool status = false;
            status = m_IOService.SendCommand("IQ:STAT OFF");
            status = m_IOService.SendCommand("SOURce:BB:ARBitrary:PRESet");
            return status;
        }

        public bool SetOutputPower(double pwr, PowerUnit powerUnit = PowerUnit.Wat)
        {
            string strCmd = string.Empty;
            switch (powerUnit)
            {
                case PowerUnit.dBm:
                    strCmd = string.Format(":POW {0}DBM", pwr);
                    break;
                case PowerUnit.mV:
                    strCmd = string.Format(":POW {0}MV", pwr);
                    break;
                case PowerUnit.uV:
                    strCmd = string.Format(":POW {0}UV", pwr);
                    break;
                case PowerUnit.Wat:
                    strCmd = string.Format(":POW {0}DBM", clsRFPowerUnitHelper.Wat2Dbm(pwr));
                    break;
            }
            bool bOK = m_IOService.SendCommand(strCmd);
            Thread.Sleep(200);
            return bOK;
        }

        public bool SetRFFrequency(double Freq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            double DevFreq = 0;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    DevFreq = Freq;
                    break;
                case FreqUnit.kHz:
                    DevFreq = Freq * 1e3;
                    break;
                case FreqUnit.MHz:
                    DevFreq = Freq * 1e6;
                    break;
                case FreqUnit.GHz:
                    DevFreq = Freq * 1e9;
                    break;

            }
            bool bOK = m_IOService.SendCommand(":FREQ {0}HZ", DevFreq);
            Thread.Sleep(500);
            return bOK;
        }

        public double GetCurrPowerLevel()
        {
            throw new NotImplementedException();
        }

        public bool OutputCWSignal(double duPwr, double nFreq, FreqUnit freqUnit = FreqUnit.Hz, PowerUnit powerUnit = PowerUnit.Wat)
        {
            m_IOService.SendCommand(":FREQuency:MODE CW");
            m_IOService.SendCommand("SOURce:MODulation:ALL:STATe OFF");
            if (!SetRFFrequency(nFreq, freqUnit))
                return false;
            if (!SetOutputPower(duPwr, powerUnit))
                return false;
            return true;
        }

        public bool EnableOutput(bool bOnOff, SignalPort DevPort = SignalPort.InOut)
        {
            bool status = false;
            if (bOnOff)
            {
                status = m_IOService.SendCommand(":OUTPut ON");
                Thread.Sleep(300);
                return status;
            }
            else
                return m_IOService.SendCommand(":OUTPut OFF");
        }

        public bool OutputAMSignal(clsAMSigDes amDes)
        {
            throw new NotImplementedException();
        }

        public bool OutputFMSignal(clsFMSigDes fmDes)
        {
            if (!SetModulationType(ModulatioType.FM))
                return false;
            switch (fmDes.Unit)
            {
                case FreqUnit.Hz:
                    fmDes.DEViation /= 1e3;
                    fmDes.FREQuency /= 1e3;
                    break;
                case FreqUnit.kHz:
                    break;
                case FreqUnit.MHz:
                    fmDes.DEViation *= 1e3;
                    fmDes.FREQuency *= 1e3;
                    break;
            }
            if (fmDes.FREQuency > 0)
                m_IOService.SendCommand("FM:MODF:VALUE {0}KHZ;SIN", fmDes.FREQuency.ToString("F03"));
            if (fmDes.DEViation > 0)
                m_IOService.SendCommand("FM:DEVN {0}KHZ", fmDes.DEViation.ToString("F03"));
            return true;
        }
        public double GetMaxOutputLevel(SignalPort DevPort = SignalPort.InOut)
        {
            return 18;
        }
        public double GetMinOutputLevel(SignalPort DevPort = SignalPort.InOut)
        {
            return -144;
        }
        public bool SetModulationType(ModulatioType modType = ModulatioType.OFF)
        {
            bool status = false;
            switch (modType)
            {
                case ModulatioType.OFF:
                    status = m_IOService.SendCommand("MOD:OFF");
                    status &= m_IOService.SendCommand("IQ:STAT OFF");
                    //  m_IOService.SendCommand("FM:OFF");
                    break;
                case ModulatioType.AM:
                    status = m_IOService.SendCommand("MOD:ON");
                    status &= m_IOService.SendCommand("AM:ON");
                    break;
                case ModulatioType.FM:
                    status = m_IOService.SendCommand("MOD:ON");
                    status &= m_IOService.SendCommand("FM:ON");
                    break;
            }
            return status;
        }
        public bool EnablePlayARBFile(string strFileName)
        {
            bool status = false;
            status = m_IOService.SendCommand($"SOURce:BB:ARBitrary:WAVeform:SELect \"{strFileName}\"");
            string strRet = m_IOService.SendAndRead("SOURce:BB:ARBitrary:WAVeform:POINts?");
            if (string.IsNullOrEmpty(strRet))
                throw new ArgumentException($"Can't load the file {strFileName}");
            int nPoint = int.Parse(strRet);
            if (nPoint <= 0)
                throw new ArgumentException($"Can't load the file {strFileName}");
            status &= m_IOService.SendCommand(":SOURce:POWer:ALC:STATe OFF");
            //m_IOService.SendCommand("MOD:ON");
            status &= m_IOService.SendCommand(":IQ:STAT ON");
            status &= m_IOService.SendCommand("SOURce:BB:ARBitrary:STATe ON");
            return status;
        }
        public bool SetARBClockFrequency(double Freq, FreqUnit freqUnit = FreqUnit.Hz)
        {
            double DevFreq = 0;
            switch (freqUnit)
            {
                case FreqUnit.Hz:
                    DevFreq = Freq;
                    break;
                case FreqUnit.kHz:
                    DevFreq = Freq * 1e3;
                    break;
                case FreqUnit.MHz:
                    DevFreq = Freq * 1e6;
                    break;
            }
            bool bOK = m_IOService.SendCommand(":SOURce:BB:ARBitrary:CLOCk {0}HZ", DevFreq);
            Thread.Sleep(500);
            return bOK;
        }
        public bool SetRFOffset(double duOffset, SignalPort DevPort = SignalPort.InOut)
        {
            bool status = m_IOService.SendCommand("SOURce:POWer:ATTenuation  {0}DB", duOffset);
            return status;
        }
        public bool Select_OutputPort(OutputPort port)
        {
            return true;
        }
        public bool Enable_RFOut(bool enable)
        {
            return true;
        }
        public bool SetFreqDev(double FreqDev, FreqUnit freqUnit = FreqUnit.Hz)
        {
            throw new Exception();
        }
        public bool Set_AFModulation(double AFFreq, AudioGenIndex index = AudioGenIndex.GEN_3, FreqUnit freqUnit = FreqUnit.Hz)
        {
            throw new Exception();
        }
        public bool Enable_AFModulation(bool EnableLeft, bool EnableRight)
        {
            throw new Exception();
        }
        public string GetSN_fromConfig()
        {
            return m_devInfo.DeviceSN;
        }
        public bool Set_Sweep(double FreqStart, double FreqStop, FreqUnit freqUnit, int Dwelltime, double RFLevel, int NumberofStep)
        {
            bool status = false;
            try
            {
                status = m_IOService.SendCommand(":LIST:TYPE STEP"); Thread.Sleep(100);//Sets sig gen LIST type to step
                status &= m_IOService.SendCommand($":FREQ:STAR {FreqStart} {freqUnit.ToString()}"); Thread.Sleep(100); // Sets start frequency
                status &= m_IOService.SendCommand($":FREQ:STOP {FreqStop} {freqUnit.ToString()}"); Thread.Sleep(100); // Sets stop frequency
                status &= m_IOService.SendCommand($":SWE:SPAC LIN"); Thread.Sleep(100); // Sets number of steps=
                status &= m_IOService.SendCommand($":SWE:POIN {NumberofStep}"); Thread.Sleep(100); // Sets number of steps=
                status &= m_IOService.SendCommand($":SWE:DWEL {Dwelltime} ms"); Thread.Sleep(100); //Sets dwell time
                status &= m_IOService.SendCommand($":POW:AMPL {RFLevel} dBm"); Thread.Sleep(100);  //Sets the power level
                return status;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Set_Sweep() error: " + ec.Message);
                return false;
            }
        }
        public bool ON_Sweep(bool bON)
        {
            bool status = false;
            try
            {
                if (bON)
                {
                    status = m_IOService.Write(":FREQuency:MODE LIST"); Thread.Sleep(100);
                    status &= m_IOService.Write(":OUTP:STAT ON"); Thread.Sleep(100);//Turns RF output on :FREQuency:MODE
                    //m_IOService.SendCommand("INIT:CONT ON"); Thread.Sleep(50);//Begins the step sweep operation
                    return status;
                }
                else
                {
                    status = m_IOService.SendCommand(":FREQuency:MODE CW"); Thread.Sleep(50);
                    status &= m_IOService.SendCommand(":OUTP:STAT OFF"); Thread.Sleep(50);//Turns RF output on
                    //m_IOService.SendCommand("INIT:CONT OFF"); Thread.Sleep(50);//Begins the step sweep operation
                    return status;
                }
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("ON_Sweep() error: " + ec.Message);
                return false;
            }
        }
        public bool Set_FreqMode(FreqMode mode)
        {//
            bool status = false;
            try
            {
                status = m_IOService.SendCommand($"FREQ:MODE {mode.ToString()}"); Thread.Sleep(50);
                return status;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Set_FreqMode() error: " + ec.Message);
                return false;
            }
        }
        #endregion ISignalGenDevice
    }
}
