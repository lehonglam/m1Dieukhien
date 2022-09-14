using CFTWinAppCore.DeviceManager;
using ExternLib.SerialPort;
using LogLibrary;
using Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneralTool;
using System.Windows.Forms;

namespace CFTSeqManager.HelperDevice
{
    [SysDevMetaAtt(typeof(clsVhcrJigFEMRX), SysDevType.JIGDEV, "JIG_VHCR_FEMRX")]
    class clsVhcrJigFEMRX:IJIG_FEMRX
    {
        private SerialSocket m_ComSocket = null;
        private int m_nBauldRate;
        private clsDeviceInfor m_devInfo;
        private bool m_bShowIOLog = true;
        public clsVhcrJigFEMRX(clsDeviceInfor devInfo)
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
        public bool ClosePort()
        {
            if (m_ComSocket.IsOpen == true)
            {
                m_ComSocket.Close();
                return true;
            }
            else
                return false;
        }
        public bool ShowIOLog
        {
            set { m_bShowIOLog = value; }
            get { return m_bShowIOLog; }
        }
        public bool IsJIGFEMRX(int timeout)
        {
            string strCmd = "TypeJIG?\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                if (ret_data.Contains("JIG FEMRX"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool AD4372_SetFrequency(float freq, int timeout)
        {
            retry_label:
            string strCmd = "FEMRX_ADF4372 Frequency" + " " + "W " + freq.ToString() + "\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                string[] split_str = ret_data.Split(new char[] { ' ', '\n' });
                if (split_str[0] == "FEMRX_ADF4372"
                    && split_str[1] == "Frequency")
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD4372_SetFrequency recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD4372_SetFrequency lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD4372_SetFrequency no data recieve");
            return false;
        }
        public bool ADF4372_IsConnected(int timeout)
        {
            string strCmd = "FEMRX_ADF4372 ConnectCheck R 0\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                if (ret_data.Contains("FEMRX_ADF4372 Connect OK 0\n"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool ADF4372_POWERUP(int timeout)
        {
            retry_label:
            string strCmd = "FEMRX_ADF4372 PowerUpChannel W 1\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                if (ret_data.Contains("FEMRX_ADF4372 PowerUpChannel 0 0\n"))
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("ADF4372_POWERUP recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "ADF4372_POWERUP lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool ADF4372_POWERDOWN(int timeout)
        {
            retry_label:
            string strCmd = "FEMRX_ADF4372 PowerUpChannel W 0\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                if (ret_data.Contains("FEMRX_ADF4372 PowerUpChannel 0 0\n"))
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("ADF4372_POWERDOWN recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "ADF4372_POWERDOWN lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool PE43705B_SetAtt(float value, int timeout)
        {
            retry_label:
            string strCmd = "FEMRX_PE43705B ATT" + " " + "W " + value.ToString("00.00") + "\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                string[] split_str = ret_data.Split(new char[] { ' ', '\n' });
                if (split_str[0] == "FEMRX_PE43705B"
                    && split_str[1] == "ATT")
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("PE43705B_SetAtt recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "PE43705B_SetAtt lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("PE43705B_SetAtt no data recieve");
            return false;
        }
        public bool FEMRX_IO_WritePin(string param, int value, int timeout)
        {
            retry_label:
            string strCmd = "FEMRX_IO " + param + " " + "W " + value.ToString() + "\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                string[] split_str = ret_data.Split(new char[] { ' ', '\n' });
                if (split_str[0] == "FEMRX_IO"
                    && split_str[1] == param)
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("FEMRX_IO_WritePin recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "FEMRX_IO_WritePin lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("FEMRX_IO_WritePin no data recieve");
            return false;
        }
        public bool FEMRX_IO_ReadPin(out int value, string param, int timeout)
        {
            retry_label:
            string strCmd = "FEMRX_IO " + param + " " + "R 0" + "\n";
            value = 0;
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                string[] split_str = ret_data.Split(new char[] { ' ', '\n' });
                if (split_str[0] == "FEMRX_IO"
                    && split_str[1] == param)
                {
                    value = Convert.ToInt32(split_str[2]);
                    return true;
                }
                else
                {
                    clsLogManager.LogError("FEMRX_IO_ReadPin recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "FEMRX_IO_ReadPin lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_ReadPin no data recieve");
            return false;
        }
    }

}
