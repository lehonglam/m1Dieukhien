using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CFTWinAppCore.DeviceManager.DUT;
using LogLibrary;
using CFTWinAppCore.DeviceManager.PowerDevice;
using Option;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading;
using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.Helper;
using ExternLib.SerialPort;
namespace SequenceManager.DUT
{
    [SysDevMetaAtt(typeof(ClsBPBDevice), SysDevType.DUT, "BPB Device")]
    class ClsBPBDevice : IBPBDevice,   IAccessDeviceService
    {
        private SerialSocket m_ComSocket = null;
        private bool m_bIsConnected = false;
        private int m_nIOTimeOut = 2000; //5 second
        private bool m_bShowIOLog = true;
        //private int m_nTryCnt = 3;
        private string m_strComPort;
        private int m_nBauldRate;
        private clsDeviceInfor m_devInfo;
        byte[] Header = new byte[] { 0x21, 0x01, 0x03, 0x19, 0x00, 0x00, 0xFE, 0x6D };
        byte[] Footer = new byte[] {0xFE, 0x6D };

        public enum BPB_COMMNAND_FORMAT
        {
            HeaderIndex=0,
            ControlIndex=4,
            LenIndex=5,
            FooterIndex=6,
            DataIndex=8
        }
        public ClsBPBDevice(clsDeviceInfor devInfo)
        {
            m_ComSocket = new SerialSocket(false);
            m_devInfo = devInfo;
            m_bShowIOLog = m_devInfo.ShowIOLog;
            m_nIOTimeOut = devInfo.IOTimeOut;
        }

        #region iBPBDevice
        public IPAddress CheckEthernetPort(ETHERNET_PORT eTHERNET_PORT)
        {
            clsLogManager.LogWarning("CheckEthernetPort: ETHERNET_PORT {0}", eTHERNET_PORT.ToString());
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(BPB_COMMAND.CHECK_ETHERNET, new byte[] { (byte)eTHERNET_PORT });
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(BPB_COMMAND.CHECK_ETHERNET);
                    if (receiveMes == null)
                    {
                        clsLogManager.LogError("Fail to get message from DUT");
                        continue;
                    }
                    if(receiveMes.Length<13)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 13, receiveMes.Length);
                        continue;
                    }
                    if(receiveMes[8]!=(byte)eTHERNET_PORT)
                    {
                        clsLogManager.LogError("Ethernet port index is wrong! Expected index= {0}, received= {1}", (int)eTHERNET_PORT, (int)receiveMes[8]);
                        continue;
                    }
                    IPAddress iPAddress = new IPAddress(new byte[] { receiveMes[9], receiveMes[10] , receiveMes[11] , receiveMes[12] });
                    clsLogManager.LogWarning("Readed IP Address: {0}", iPAddress.ToString());
                    return iPAddress;
                }
                return null;
            }
            catch(Exception ex)
            {
                clsLogManager.LogError("CheckEthernetPort: ex {0}", ex.StackTrace);
                return null;
            }
        }
        public bool CheckE1Port(E1_PORT e1_PORT)
        {
            clsLogManager.LogWarning("CheckE1Port: E1_PORT {0}", e1_PORT.ToString());
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(BPB_COMMAND.CHECK_E1, new byte[] { (byte)e1_PORT });
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(BPB_COMMAND.CHECK_E1,60000);
                    if (receiveMes == null)
                    {
                        clsLogManager.LogError("Fail to get message from DUT");
                        continue;
                    }
                    if (receiveMes.Length < 10)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 10, receiveMes.Length);
                        continue;
                    }
                    if (receiveMes[8] != (byte)e1_PORT)
                    {
                        clsLogManager.LogError("E1 port index is wrong! Expected index= {0}, received= {1}", (int)e1_PORT, (int)receiveMes[8]);
                        continue;
                    }
                    return receiveMes[9]==0x01;
                }
                return false;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("CheckE1Port: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public bool CheckCPRIPort()
        {
            clsLogManager.LogWarning("CheckCPRIPort: ");
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(BPB_COMMAND.CHECK_CPRI);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(BPB_COMMAND.CHECK_CPRI);
                    if (receiveMes == null)
                    {
                        clsLogManager.LogError("Fail to get message from DUT");
                        continue;
                    }
                    if (receiveMes.Length < 9)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 9, receiveMes.Length);
                        continue;
                    }
                    return receiveMes[8] == 0x01;
                }
                return false;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("CheckCPRIPort: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public bool SetAudioVolume(byte bVolumeLevel)
        {
            clsLogManager.LogWarning("SetAudioVolume: Volume Level {0}", bVolumeLevel.ToString("X2"));
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(BPB_COMMAND.SET_VOLUME_LEVEL, new byte[] { bVolumeLevel});
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(BPB_COMMAND.SET_VOLUME_LEVEL);
                    if (receiveMes == null)
                    {
                        clsLogManager.LogError("Fail to get message from DUT");
                        continue;
                    }
                    if (receiveMes.Length < 9)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 9, receiveMes.Length);
                        continue;
                    }
                    return receiveMes[8] == bVolumeLevel;
                }
                return false;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("SetAudioVolume: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public bool SetAudioLoop(AUDIO_LOOP_MODE aUDIO_LOOP_MODE)
        {
            clsLogManager.LogWarning("SetAudioLoop: AUDIO_LOOP_MODE {0}", aUDIO_LOOP_MODE.ToString());
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(BPB_COMMAND.CONFIG_AUDIO_LOOP, new byte[] { (byte)aUDIO_LOOP_MODE });
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(BPB_COMMAND.CONFIG_AUDIO_LOOP);
                    if (receiveMes == null)
                    {
                        clsLogManager.LogError("Fail to get message from DUT");
                        continue;
                    }
                    if (receiveMes.Length < 9)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 9, receiveMes.Length);
                        continue;
                    }
                    return receiveMes[8] == (byte)aUDIO_LOOP_MODE;
                }
                return false;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("SetAudioLoop: ex {0}", ex.StackTrace);
                return false;
            }
        }

        #endregion
        #region private
        private bool SendMessage(BPB_COMMAND bPB_COMMAND, byte[] Data)
        {
            try
            {
                byte[] mes = CreateMessage((byte)bPB_COMMAND, Data);
                m_ComSocket.Transmit(mes, 0, mes.Length);
                if (m_devInfo.ShowIOLog)
                {
                    clsLogManager.LogReport("Send Message :" + clsBufferHeper.ByteArrayToStringHex(mes));
                }
                return true;
            }
            catch(Exception ex)
            {
                clsLogManager.LogError("SendMessage: ex {0}", ex.StackTrace);
                return false;
            }
        }
        private bool SendMessage(BPB_COMMAND bPB_COMMAND)
        {
            try
            {
                byte[] mes = CreateMessage((byte)bPB_COMMAND);
                m_ComSocket.Transmit(mes, 0, mes.Length);
                if (m_devInfo.ShowIOLog)
                {
                    clsLogManager.LogReport("Send Message :" + clsBufferHeper.ByteArrayToStringHex(mes));
                }
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("SendMessage: ex {0}", ex.StackTrace);
                return false;
            }
        }
        private byte[] GetMessage(BPB_COMMAND bPB_COMMAND, int timeOut=5000)
        {
            if (m_devInfo.ShowIOLog)
            {
                clsLogManager.LogReport("Get Message From DUT:...");
            }
            byte[] ReceiveHeader = new byte[] { 0x21, 0x01, 0x03, 0x19, 0x00 };
            ReceiveHeader[4] = (byte)((int)bPB_COMMAND & 0x0F);
            byte[] mes = m_ComSocket.GetVHCR_PCBAMessage(ReceiveHeader, Footer, timeOut);
            if (mes != null)
            {
                if (m_devInfo.ShowIOLog)
                {
                    clsLogManager.LogReport("Get Message From DUT: " + clsBufferHeper.ByteArrayToStringHex(mes) + "\n");
                }
            }
            return mes;
        }
        private byte[] CreateMessage(byte CTRL, byte[] Data)
        {
            //byte[] mes = new byte[Data.Length + 8];
            //for(int i=0;i<SendHeader.Length;i++)
            //{
            //    mes[i] = SendHeader[i];
            //}
            //mes[(int)BPB_COMMNAND_FORMAT.ControlIndex] = CTRL;
            //mes[(int)BPB_COMMNAND_FORMAT.LenIndex] =(byte) Data.Length;
            //for (int i = 0; i < SendHeader.Length; i++)
            //{
            //    mes[(int)BPB_COMMNAND_FORMAT.FooterIndex + i] = Footer[i];
            //}
            //for (int i = 0; i < Data.Length; i++)
            //{
            //    mes[(int)BPB_COMMNAND_FORMAT.DataIndex+i] = Data[i];
            //}
            //return mes;
            //byte[] mes = new byte[] {0x21, 0x03, 0x01, 0x93, 0x00, 0x00, 0xAB, 0xC6 };
            
            byte[] mes = new byte[Data.Length + 8];
            for(int i=0;i<Header.Length;i++)
            {
                mes[ i] = Header[i];
            }
            mes[4] = CTRL;
            mes[5] = (byte)Data.Length;
            for (int i = 0; i < Data.Length; i++)
            {
                mes[8 + i] = Data[i];
            }
            return mes;
        }
        private byte[] CreateMessage(byte CTRL)
        {
            //byte[] mes = new byte[Data.Length + 8];
            //for(int i=0;i<SendHeader.Length;i++)
            //{
            //    mes[i] = SendHeader[i];
            //}
            //mes[(int)BPB_COMMNAND_FORMAT.ControlIndex] = CTRL;
            //mes[(int)BPB_COMMNAND_FORMAT.LenIndex] =(byte) Data.Length;
            //for (int i = 0; i < SendHeader.Length; i++)
            //{
            //    mes[(int)BPB_COMMNAND_FORMAT.FooterIndex + i] = Footer[i];
            //}
            //for (int i = 0; i < Data.Length; i++)
            //{
            //    mes[(int)BPB_COMMNAND_FORMAT.DataIndex+i] = Data[i];
            //}
            //return mes;
            byte[] mes = new byte[] { 0x21, 0x01, 0x03, 0x19, CTRL, 0x00, 0xFE, 0x6D };
            return mes;
        }
        #endregion 
        #region IAccessDevice
        public bool Connect2Device(IDeviceInfor devInfo)
        {
            clsLogManager.LogWarning("{0}: Connect To Device. Address: {1}, AddParam: {2}", devInfo.DeviceDescription, devInfo.DevAddress, devInfo.AddtionDevPara);
            try
            {
                m_devInfo = (clsDeviceInfor)devInfo;
                m_strComPort = devInfo.DevAddress;
                m_nBauldRate = Int32.Parse(devInfo.AddtionDevPara);
                bool bResult = false;
                int nComPort = int.Parse(m_strComPort.ToUpper().Replace("COM", ""));
                if (m_ComSocket == null)
                {
                    m_ComSocket = new SerialSocket(false);
                }
                if (m_ComSocket.IsOpen) m_ComSocket.Close();
                bResult = m_ComSocket.OpenComPort(nComPort, m_nBauldRate);
                if (!bResult)
                {
                    m_bIsConnected = false;
                    return false;
                }
                m_nIOTimeOut = devInfo.IOTimeOut;
                m_bIsConnected = true;
                return m_bIsConnected;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Connect2Device: {0}", ex.ToString());
                m_bIsConnected = false;
                return false;
            }
        }
        public bool InitDevice()
        {
            try
            {
                if (string.IsNullOrEmpty(m_strComPort)) return true;
                if (m_ComSocket == null)
                {
                    m_ComSocket = new SerialSocket(false);
                }
                if (m_ComSocket.IsOpen) m_ComSocket.Close();
                int nComPort = int.Parse(m_strComPort.ToUpper().Replace("COM", ""));
                bool bResult = m_ComSocket.OpenComPort(nComPort, m_nBauldRate);
                if (!bResult)
                {
                    m_bIsConnected = false;
                    return false;
                }
                m_bIsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("InitDevice: ex {0}", ex.Message);
                return false;
            }
        }
        public bool IsConnected()
        {
            m_bShowIOLog = m_devInfo.ShowIOLog;
            if (!m_ComSocket.IsOpen)
                return false;
            //if (PingDevice())
            //{
            //    m_bIsConnectedDut = true;
            return true;
            //}
            //else
            //{
            //    m_bIsConnectedDut = false;
            //    return false;
            //}
        }
        public void DisconnectDevice()
        {
            if (m_ComSocket == null) return;
            if (m_ComSocket.IsOpen)
            {
                m_ComSocket.Close();
                m_bIsConnected = false;
            }
        }
        public string GetDeviceFriendlyName()
        {
            return "BPB Device";
        }
        public object GetIOLibrary()
        {
            return m_ComSocket;
        }
        public bool IsDeviceConnected()
        {
            try
            {
                return m_bIsConnected;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("IsDeviceConnected: {0}", ex.ToString());
                return false;
            }
        }
        public bool IsDeviceSupportCusDevProperties()
        {
            return false;
        }
        public bool SetIOTimeOut(int nTimeOut)
        {
            return true;
        }
        public bool ShowDeviceProperties()
        {
            return false;
        }
        public void ShowDeviceTestDialog()
        {

        }
        public bool IsSupportMesService(Type typeOfMsService)
        {
            return false;
        }
        public bool ShowIOLog
        {
            set { m_bShowIOLog = value; }
            get { return m_bShowIOLog; }
        }
        #endregion
    }
}
