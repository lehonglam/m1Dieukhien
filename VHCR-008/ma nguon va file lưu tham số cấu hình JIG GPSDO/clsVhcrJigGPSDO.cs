using CFTWinAppCore.DeviceManager;
using ExternLib.SerialPort;
using LogLibrary;
using Option;
using CFTWinAppCore.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFTSeqManager.HelperDevice
{
    [SysDevMetaAtt(typeof(clsVhcrJigGPSDO), SysDevType.JIGDEV, "JIG_VHCR_GPSDO")]
    public class clsVhcrJigGPSDO
    {
        private SerialSocket m_ComSocket = null;
        private bool m_bShowIOLog = true;
        private int m_nBauldRate;
        private clsDeviceInfor m_devInfo;

        public clsVhcrJigGPSDO(clsDeviceInfor devInfo)
        {
            m_ComSocket = new SerialSocket(false);
            m_devInfo = devInfo;
        }
        public bool Connect2Device(IDeviceInfor devInfo)
        {
            try
            {
                bool bResult = false;
                string strComPort = string.Format("COM{0}", devInfo.DevAddress);
                int nComPort = int.Parse(devInfo.DevAddress);
                m_nBauldRate = int.Parse(devInfo.AddtionDevPara);
                if (!m_ComSocket.IsOpen)
                {
                    bResult = m_ComSocket.OpenComPort(nComPort, m_nBauldRate);
                    if (!bResult)
                        return false;
                }
                return true;

            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Connect2Device: {0}", ex.ToString());
                return false;
            }
        }
        public bool IsConnected()
        {
            return m_ComSocket.IsOpen;
        }
        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public bool SetSerial(string sn, int timeout)
        {
            string data_to_send = "$SERW" + ((byte)sn.Length).ToString("X2") + sn+"\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(data_to_send);
            byte[] data = m_ComSocket.ReadData(8, (uint)timeout);
            if (data != null)
            {
                string ret = ByteArrayToString(data);
                if (ret.Contains("SERW01S") == true)
                    return true;
                else if (ret.Contains("SERW01F") == true)
                    return false;
                else
                {
                    clsLogManager.LogError("SetSerial recieve data error");
                    return false;
                }
            }
            clsLogManager.LogError("SetSerial no data recieve");
            return false;
        }

        public string ReadSerial(int timeout)
        {
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit("$SERR00");
            byte[] data = m_ComSocket.ReadData(16, (uint)timeout);
            if (data != null)
            {
                string ret = ByteArrayToString(data);
                if (ret.Contains("$SERW") == true)
                    return ret.Substring(7);
                else
                {
                    clsLogManager.LogError("ReadSerial recieve data error");
                    return "";
                }
            }
            clsLogManager.LogError("ReadSerial no data recieve");
            return null;
        }
        public bool ReadCalibStatus(int timeout)
        {
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit("$CALR00");
            byte[] data = m_ComSocket.ReadData(8, (uint)timeout);
            if (data != null)
            {
                string ret = ByteArrayToString(data);
                if (ret.Contains("$CALW01S") == true)
                    return true;
                else if (ret.Contains("$CALW01F") == true)
                    return false;
                else
                {
                    clsLogManager.LogError("ReadCalibStatus recieve data error");
                    return false;
                }
            }
            clsLogManager.LogError("ReadCalibStatus no data recieve");
            return false;
        }
        public bool SaveCalib(int timeout)
        {
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit("$CALW00");
            byte[] data = m_ComSocket.ReadData(8, (uint)timeout);
            if (data != null)
            {
                string ret = ByteArrayToString(data);
                if (ret.Contains("$CALW01S") == true)
                    return true;
                else if (ret.Contains("$CALW01F") == true)
                    return false;
                else
                {
                    clsLogManager.LogError("SaveCalib recieve data error");
                    return false;
                }
            }
            clsLogManager.LogError("SaveCalib no data recieve");
            return false;
        }
        public string ReadVerFvJig(int timeout)
        {
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit("$VERR00");
            byte[] data = m_ComSocket.ReadData(11, (uint)timeout);
            if (data != null)
            {
                string ret = ByteArrayToString(data);
                if (ret.Contains("$VERW") == true)
                {
                    return ret.Substring(7);
                }
                else
                {
                    clsLogManager.LogError("SaveCalib recieve data error");
                    return null;
                }
            }
            clsLogManager.LogError("SaveCalib no data recieve");
            return null;
        }
        public bool Reset(int timeout)
        {
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit("$RSTW00");
            return true;
        }
        //
        public bool ShowIOLog
        {
            set { m_bShowIOLog = value; }
            get { return m_bShowIOLog; }
        }

    }
}
