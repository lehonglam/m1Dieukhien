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
using System.IO.Ports;

namespace SequenceManager.DUT
{
    [SysDevMetaAtt(typeof(ClsDigitizerDevice), SysDevType.DIGITIZER_BOARD, "Digitizer Board Device")]
    class ClsDigitizerDevice : IDigitizerBoard,   IAccessDeviceService
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
        byte[] Footer = new byte[] { 0xFE, 0x6D };

        public ClsDigitizerDevice(clsDeviceInfor devInfo)
        {
            m_ComSocket = new SerialSocket(false);
            m_devInfo = devInfo;
            m_bShowIOLog = m_devInfo.ShowIOLog;
            m_nIOTimeOut = devInfo.IOTimeOut;
        }

        #region iDigitizerDevice
        
        public bool CheckSFPPort(int Timeout)
        {
            clsLogManager.LogWarning("CheckSFP: ");
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(DIGITIZER_COMMAND.CHECK_SFP);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.CHECK_SFP, Timeout);
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
                clsLogManager.LogError("CheckSFP: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public bool CheckFMCPort(int Timeout)
        {
            clsLogManager.LogWarning("CheckFMC: ");
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(DIGITIZER_COMMAND.CHECK_FMC);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.CHECK_FMC, Timeout);
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
                clsLogManager.LogError("CheckFMC: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public bool CheckLedOn()
        {
            clsLogManager.LogWarning("CheckLedOn: ");
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(DIGITIZER_COMMAND.CHECK_IO, new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 });
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    //byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.CHECK_IO);
                    //if (receiveMes == null)
                    //{
                    //    clsLogManager.LogError("Fail to get message from DUT");
                    //    continue;
                    //}
                    //if (receiveMes.Length < 11)
                    //{
                    //    clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 11, receiveMes.Length);
                    //    continue;
                    //}
                    //if( receiveMes[8] != 0x01)
                    //{
                    //    clsLogManager.LogError("Check Led 1 fail");
                    //}
                    //if (receiveMes[9] != 0x01)
                    //{
                    //    clsLogManager.LogError("Check Led 2 fail");
                    //}
                    //if (receiveMes[10] != 0x01)
                    //{
                    //    clsLogManager.LogError("Check Led 3 fail");
                    //}
                    //if(receiveMes[8] == 0x01&& receiveMes[9] == 0x01&& receiveMes[10] == 0x01)
                    //{
                    //    return true;
                    //}
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("CheckIO: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public bool CheckLedOff()
        {
            clsLogManager.LogWarning("CheckLedOff: ");
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(DIGITIZER_COMMAND.CHECK_IO, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    //byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.CHECK_IO);
                    //if (receiveMes == null)
                    //{
                    //    clsLogManager.LogError("Fail to get message from DUT");
                    //    continue;
                    //}
                    //if (receiveMes.Length < 11)
                    //{
                    //    clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 11, receiveMes.Length);
                    //    continue;
                    //}
                    //if (receiveMes[8] != 0x00)
                    //{
                    //    clsLogManager.LogError("Check Led 1 fail");
                    //}
                    //if (receiveMes[9] != 0x00)
                    //{
                    //    clsLogManager.LogError("Check Led 2 fail");
                    //}
                    //if (receiveMes[10] != 0x00)
                    //{
                    //    clsLogManager.LogError("Check Led 3 fail");
                    //}
                    //if (receiveMes[8] == 0x00 && receiveMes[9] == 0x00 && receiveMes[10] == 0x00)
                    //{
                    //    return true;
                    //}
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("CheckLedOff: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public bool CheckLedIO(int nLedIndex, int TimeOut, bool bIsOn = true)
        {
            byte[] arrSendData = null;
            if (bIsOn)
            {
                if (nLedIndex == 1)
                    arrSendData =new byte[] { 1, 0, 0 };
                else if (nLedIndex == 2)
                    arrSendData = new byte[] { 0, 1, 0 };
                else if (nLedIndex == 3)
                    arrSendData = new byte[] { 0, 0, 1 };
            }
            else
                arrSendData = new byte[] { 0, 0, 0 };
            bool bOk = SendMessage(DIGITIZER_COMMAND.CHECK_IO, arrSendData);
            if (!bOk)
            {
                clsLogManager.LogError("Fail to send message to DUT");
                return false;
            }
            byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.CHECK_IO, TimeOut);
            if (receiveMes == null)
            {
                clsLogManager.LogError("Fail to get message from DUT");
                return false;
            }
            if (receiveMes.Length < 11)
            {
                clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 11, receiveMes.Length);
                return false;
            }
            if (receiveMes[8] != arrSendData[0])
            {
                clsLogManager.LogError("Check Led 1 fail");
            }
            if (receiveMes[9] != arrSendData[1])
            {
                clsLogManager.LogError("Check Led 2 fail");
            }
            if (receiveMes[10] != arrSendData[2])
            {
                clsLogManager.LogError("Check Led 3 fail");
            }
            if (receiveMes[8] == arrSendData[0] && receiveMes[9] == arrSendData[1] && receiveMes[10] == arrSendData[2])
            {
                return true;
            }
            return false;
        }

        //public bool CheckLedIO(int nLedIndex, int Timeout, bool bIsOn = true)
        //{
        //    byte[] arrSendPacket = null;
        //    if (bIsOn)
        //    {
        //        if (nLedIndex == 1)
        //            arrSendPacket = CreatePacket((byte)(0x80 | 0x07), new byte[] { 1, 0, 0 });
        //        else if (nLedIndex == 2)
        //            arrSendPacket = CreatePacket((byte)(0x80 | 0x07), new byte[] { 0, 1, 0 });
        //        else if (nLedIndex == 3)
        //            arrSendPacket = CreatePacket((byte)(0x80 | 0x07), new byte[] { 0, 0, 1 });
        //    }
        //    else
        //        arrSendPacket = CreatePacket((byte)(0x80 | 0x07), new byte[] { 0, 0, 0 });
        //    m_rs232Socket.Write(arrSendPacket, 0, arrSendPacket.Length);
        //    m_rs232Socket.DiscardOutBuffer();
        //    //
        //    byte bRevCtrl = 0;
        //    byte[] arrRevPacket = ReadPacket(ref bRevCtrl);
        //    if (arrRevPacket == null) return false;
        //    if (arrRevPacket[0] == 0x01)
        //        return true;
        //    clsLogManager.LogWarning($"Received result value {arrRevPacket[0]} expecting vaue 0x01");
        //    return false;
        //}
        public bool CheckClockPort(int Timeout)
        {
            clsLogManager.LogWarning("CheckClockPort: ");
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(DIGITIZER_COMMAND.CHECK_CLOCK);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.CHECK_CLOCK, Timeout);
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
                clsLogManager.LogError("CheckClockPort: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public bool CheckUsbUart(int Timeout)
        {
            clsLogManager.LogWarning("CheckUsbUART: ");
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(DIGITIZER_COMMAND.READ_USB_UART);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.READ_USB_UART, Timeout);
                    if (receiveMes == null)
                    {
                        clsLogManager.LogError("Fail to get message from DUT");
                        continue;
                    }
                    if (receiveMes.Length < 8)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 8, receiveMes.Length);
                        continue;
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("CheckUsbUART: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public bool SendRS485(byte[] Data, int Timeout)
        {
            clsLogManager.LogWarning("SendRS485: Data {0}", clsBufferHeper.ConvertArr2Str(Data));
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(DIGITIZER_COMMAND.SEND_RS485, Data);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.SEND_RS485, Timeout);
                    if (receiveMes == null)
                    {
                        clsLogManager.LogError("Fail to get message from DUT");
                        continue;
                    }
                    if(receiveMes.Length<9)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 9, receiveMes.Length);
                        continue;
                    }
                    return receiveMes[8] == 0x01;
                }
                return false;
            }
            catch(Exception ex)
            {
                clsLogManager.LogError("SendRS485: ex {0}", ex.StackTrace);
                return false;
            }
        }
        public byte[] ReadRS485(int Timeout)
        {
            clsLogManager.LogWarning("ReadRS485: ");
            try
            {
                m_ComSocket.FlushTxRx();
                for (int i = 0; i < 3; i++)
                {
                    bool bOk = SendMessage(DIGITIZER_COMMAND.READ_RS485);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        continue;
                    }
                    byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.READ_RS485, Timeout);
                    if (receiveMes == null)
                    {
                        clsLogManager.LogError("Fail to get message from DUT");
                        continue;
                    }
                    if (receiveMes.Length < 9)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0} or more than, response Length= {1}", 9, receiveMes.Length);
                        continue;
                    }
                    byte[] byteReturn = new byte[receiveMes[5]];
                    for(int j=0;j<byteReturn.Length;j++)
                    {
                        byteReturn[j] = receiveMes[8 + j];
                    }
                    return byteReturn;
                }
                return null;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("ReadRS485: ex {0}", ex.StackTrace);
                return null;
            }
        }
        public bool CheckRS485Port(string nRs485PortName, int TimeOut, int NumberOfByteToSend, int NumberOfByteToReceive)
        {
            try
            {
                using (SerialPort m_rs485Socket = new SerialPort())
                {
                    m_rs485Socket.BaudRate = 115200;
                    m_rs485Socket.Parity = Parity.None;
                    m_rs485Socket.DataBits = 8;
                    m_rs485Socket.PortName = nRs485PortName;//string.Format("COM{0}", nRs485PortNumber);
                    m_rs485Socket.ReadBufferSize = 8 * 1024; // 5K
                    m_rs485Socket.WriteBufferSize = 8 * 1024; //5K
                    m_rs485Socket.ReadTimeout = (int)m_nIOTimeOut;
                    m_rs485Socket.StopBits = StopBits.One;
                    m_rs485Socket.Handshake = Handshake.None;
                    m_rs485Socket.Open();
                    clsLogManager.LogReport("Open com port {0}, bauldrate {1} success", m_rs485Socket.PortName, m_rs485Socket.BaudRate);
                    if (!m_rs485Socket.IsOpen)
                        throw new ArgumentException($"Can't open comport {m_rs485Socket.PortName}");
                    byte[] arrRs485SendData = new byte[NumberOfByteToReceive];
                    for (int i = 0; i < NumberOfByteToReceive; i++)
                    {
                        arrRs485SendData[i] = (byte)i;
                    }
                    //kiem tra chieu USB-RS485
                    clsLogManager.LogReport("Begin check RS485: direction USB-RS485...");
                    //send data over USB
                    bool bOk = SendMessage(DIGITIZER_COMMAND.SEND_RS485, arrRs485SendData);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        return false;
                    }
                    byte[] arrRevPacket = GetMessage(DIGITIZER_COMMAND.SEND_RS485, TimeOut);
                    if (arrRevPacket.Length < 9)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0} or more than, response Length= {1}", 9, arrRevPacket.Length);
                        return false;
                    }
                    if (arrRevPacket[8] != 0x01)
                    {
                        clsLogManager.LogError("Fail to send message over USB to RS485");
                        return false;
                    }
                    Thread.Sleep(200);
                    //read at RS485 port
                    byte[] revPacketFromRS485 = new byte[NumberOfByteToReceive];
                    //using (SerialPort m_rs485Socket = new SerialPort())
                    //{
                    //    if (!m_rs485Socket.IsOpen)
                    //    {
                    //        m_rs485Socket.BaudRate = 115200;
                    //        m_rs485Socket.Parity = Parity.None;
                    //        m_rs485Socket.DataBits = 8;
                    //        m_rs485Socket.PortName = nRs485PortName;//string.Format("COM{0}", nRs485PortNumber);
                    //        m_rs485Socket.ReadBufferSize = 8 * 1024; // 5K
                    //        m_rs485Socket.WriteBufferSize = 8 * 1024; //5K
                    //        m_rs485Socket.ReadTimeout = (int)m_nIOTimeOut;
                    //        m_rs485Socket.StopBits = StopBits.One;
                    //        m_rs485Socket.Handshake = Handshake.None;
                    //        m_rs485Socket.Open();
                    //        clsLogManager.LogReport("Open com port {0}, bauldrate {1} success", m_rs485Socket.PortName, m_rs485Socket.BaudRate);
                    //        if (!m_rs485Socket.IsOpen)
                    //            throw new ArgumentException($"Can't open comport {m_rs485Socket.PortName}");
                    //    }
                    //    //
                    //    m_rs485Socket.Read(revPacketFromRS485, 0, 19);
                    //}
                    m_rs485Socket.Read(revPacketFromRS485, 0, NumberOfByteToReceive);
                    clsLogManager.LogReport("Get data from RS485 :" + clsBufferHeper.ByteArrayToStringHex(revPacketFromRS485));
                    for (int i = 0; i < NumberOfByteToReceive; i++)
                    {
                        if (revPacketFromRS485[i] != arrRs485SendData[i])
                        {
                            clsLogManager.LogError("Fail to check byte index {0}", i);
                            return false;
                        }
                    }
                    clsLogManager.LogReport("Check RS485, direction USB-RS485 pass");
                    //kiem tra chieu RS485-USB
                    clsLogManager.LogReport("Begin check RS485: direction RS485-USB...");
                    //using (SerialPort m_rs485Socket = new SerialPort())
                    //{
                    //    if (!m_rs485Socket.IsOpen)
                    //    {
                    //        m_rs485Socket.BaudRate = 115200;
                    //        m_rs485Socket.Parity = Parity.None;
                    //        m_rs485Socket.DataBits = 8;
                    //        m_rs485Socket.PortName = nRs485PortName;//string.Format("COM{0}", nRs485PortNumber);
                    //        m_rs485Socket.ReadBufferSize = 8 * 1024; // 5K
                    //        m_rs485Socket.WriteBufferSize = 8 * 1024; //5K
                    //        m_rs485Socket.ReadTimeout = (int)m_nIOTimeOut;
                    //        m_rs485Socket.StopBits = StopBits.One;
                    //        m_rs485Socket.Handshake = Handshake.None;
                    //        m_rs485Socket.Open();
                    //        clsLogManager.LogReport("Open com port {0}, bauldrate {1} success", m_rs485Socket.PortName, m_rs485Socket.BaudRate);
                    //        if (!m_rs485Socket.IsOpen)
                    //            throw new ArgumentException($"Can't open comport {m_rs485Socket.PortName}");
                    //    }
                    //    //
                    //    m_rs485Socket.Write(arrRs485SendData, 0, arrRs485SendData.Length);
                    //    m_rs485Socket.DiscardOutBuffer();
                    //}
                    byte[] arrRs485SendData2 = new byte[NumberOfByteToSend];
                    for (int i = 0; i < NumberOfByteToSend; i++)
                    {
                        arrRs485SendData2[i] = (byte)i;
                    }
                    m_rs485Socket.DiscardOutBuffer();
                    m_rs485Socket.DiscardInBuffer();
                    m_rs485Socket.Write(arrRs485SendData2, 0, arrRs485SendData2.Length);
                    clsLogManager.LogReport("Send data to RS485 port:" + clsBufferHeper.ByteArrayToStringHex(arrRs485SendData2));
                    Thread.Sleep(200);
                    bOk = SendMessage(DIGITIZER_COMMAND.READ_RS485);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        return false;
                    }
                    arrRevPacket = GetMessage(DIGITIZER_COMMAND.READ_RS485, TimeOut);
                    if (arrRevPacket == null) return false;
                    if (arrRevPacket.Length != NumberOfByteToSend + 8)
                        throw new ArgumentException($"Received data length {arrRevPacket.Length} expecting value {NumberOfByteToSend+8}");
                    for (int i = 0; i < NumberOfByteToSend; i++)
                    {
                        if (arrRs485SendData2[i] != arrRevPacket[8 + i])
                        {
                            clsLogManager.LogError("Fail to check byte index {0}", i);
                            return false;
                        }
                    }
                    clsLogManager.LogReport("Check RS485, direction RS485-USB pass");
                    return true;
                }
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("CheckRS485Port: {0}", ex.ToString());
                return false;
            }
        }
        public bool CheckRS485_Pantilt(string nRs485PortName, int TimeOut, int NumberOfByteToSend, int NumberOfByteToReceive)
        {
            try
            {
                using (SerialPort m_rs485Socket = new SerialPort())
                {
                    m_rs485Socket.BaudRate = 115200;
                    m_rs485Socket.Parity = Parity.None;
                    m_rs485Socket.DataBits = 8;
                    m_rs485Socket.PortName = nRs485PortName;//string.Format("COM{0}", nRs485PortNumber);
                    m_rs485Socket.ReadBufferSize = 8 * 1024; // 5K
                    m_rs485Socket.WriteBufferSize = 8 * 1024; //5K
                    m_rs485Socket.ReadTimeout = (int)m_nIOTimeOut;
                    m_rs485Socket.StopBits = StopBits.One;
                    m_rs485Socket.Handshake = Handshake.None;
                    m_rs485Socket.Open();
                    clsLogManager.LogReport("Open com port {0}, bauldrate {1} success", m_rs485Socket.PortName, m_rs485Socket.BaudRate);
                    if (!m_rs485Socket.IsOpen)
                        throw new ArgumentException($"Can't open comport {m_rs485Socket.PortName}");
                    byte[] arrRs485SendData = new byte[NumberOfByteToReceive];
                    for (int i = 0; i < NumberOfByteToReceive; i++)
                    {
                        arrRs485SendData[i] = (byte)i;
                    }
                    //kiem tra chieu USB-RS485
                    clsLogManager.LogReport("Begin check RS485 pantilt: direction USB-RS485...");
                    //send data over USB
                    bool bOk = SendMessage(DIGITIZER_COMMAND.SEND_RS485_PANTILT, arrRs485SendData);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        return false;
                    }
                    byte[] arrRevPacket = GetMessage(DIGITIZER_COMMAND.SEND_RS485_PANTILT, TimeOut);
                    if (arrRevPacket.Length < 9)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0} or more than, response Length= {1}", 9, arrRevPacket.Length);
                        return false;
                    }
                    if (arrRevPacket[8] != 0x01)
                    {
                        clsLogManager.LogError("Fail to send message over USB to RS485");
                        return false;
                    }
                    Thread.Sleep(200);
                    //read at RS485 port
                    byte[] revPacketFromRS485 = new byte[NumberOfByteToReceive];
                    //using (SerialPort m_rs485Socket = new SerialPort())
                    //{
                    //    if (!m_rs485Socket.IsOpen)
                    //    {
                    //        m_rs485Socket.BaudRate = 115200;
                    //        m_rs485Socket.Parity = Parity.None;
                    //        m_rs485Socket.DataBits = 8;
                    //        m_rs485Socket.PortName = nRs485PortName;//string.Format("COM{0}", nRs485PortNumber);
                    //        m_rs485Socket.ReadBufferSize = 8 * 1024; // 5K
                    //        m_rs485Socket.WriteBufferSize = 8 * 1024; //5K
                    //        m_rs485Socket.ReadTimeout = (int)m_nIOTimeOut;
                    //        m_rs485Socket.StopBits = StopBits.One;
                    //        m_rs485Socket.Handshake = Handshake.None;
                    //        m_rs485Socket.Open();
                    //        clsLogManager.LogReport("Open com port {0}, bauldrate {1} success", m_rs485Socket.PortName, m_rs485Socket.BaudRate);
                    //        if (!m_rs485Socket.IsOpen)
                    //            throw new ArgumentException($"Can't open comport {m_rs485Socket.PortName}");
                    //    }
                    //    //
                    //    m_rs485Socket.Read(revPacketFromRS485, 0, 19);
                    //}
                    m_rs485Socket.Read(revPacketFromRS485, 0, NumberOfByteToReceive);
                    clsLogManager.LogReport("Get data from RS485 :" + clsBufferHeper.ByteArrayToStringHex(revPacketFromRS485));
                    for (int i = 0; i < NumberOfByteToReceive; i++)
                    {
                        if (revPacketFromRS485[i] != arrRs485SendData[i])
                        {
                            clsLogManager.LogError("Fail to check byte index {0}", i);
                            return false;
                        }
                    }
                    clsLogManager.LogReport("Check RS485 pantilt, direction USB-RS485 pass");
                    //kiem tra chieu RS485-USB
                    clsLogManager.LogReport("Begin check RS485 pantilt: direction RS485-USB...");
                    //using (SerialPort m_rs485Socket = new SerialPort())
                    //{
                    //    if (!m_rs485Socket.IsOpen)
                    //    {
                    //        m_rs485Socket.BaudRate = 115200;
                    //        m_rs485Socket.Parity = Parity.None;
                    //        m_rs485Socket.DataBits = 8;
                    //        m_rs485Socket.PortName = nRs485PortName;//string.Format("COM{0}", nRs485PortNumber);
                    //        m_rs485Socket.ReadBufferSize = 8 * 1024; // 5K
                    //        m_rs485Socket.WriteBufferSize = 8 * 1024; //5K
                    //        m_rs485Socket.ReadTimeout = (int)m_nIOTimeOut;
                    //        m_rs485Socket.StopBits = StopBits.One;
                    //        m_rs485Socket.Handshake = Handshake.None;
                    //        m_rs485Socket.Open();
                    //        clsLogManager.LogReport("Open com port {0}, bauldrate {1} success", m_rs485Socket.PortName, m_rs485Socket.BaudRate);
                    //        if (!m_rs485Socket.IsOpen)
                    //            throw new ArgumentException($"Can't open comport {m_rs485Socket.PortName}");
                    //    }
                    //    //
                    //    m_rs485Socket.Write(arrRs485SendData, 0, arrRs485SendData.Length);
                    //    m_rs485Socket.DiscardOutBuffer();
                    //}
                    byte[] arrRs485SendData2 = new byte[NumberOfByteToSend];
                    for (int i = 0; i < NumberOfByteToSend; i++)
                    {
                        arrRs485SendData2[i] = (byte)i;
                    }
                    m_rs485Socket.DiscardOutBuffer();
                    m_rs485Socket.DiscardInBuffer();
                    m_rs485Socket.Write(arrRs485SendData2, 0, arrRs485SendData2.Length);
                    clsLogManager.LogReport("Send data to RS485 port:" + clsBufferHeper.ByteArrayToStringHex(arrRs485SendData2));
                    Thread.Sleep(200);
                    bOk = SendMessage(DIGITIZER_COMMAND.READ_RS485_PANTILT);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        return false;
                    }
                    arrRevPacket = GetMessage(DIGITIZER_COMMAND.READ_RS485_PANTILT, TimeOut);
                    if (arrRevPacket == null) return false;
                    if (arrRevPacket.Length != NumberOfByteToSend + 8)
                        throw new ArgumentException($"Received data length {arrRevPacket.Length} expecting value {NumberOfByteToSend + 8}");
                    for (int i = 0; i < NumberOfByteToSend; i++)
                    {
                        if (arrRs485SendData2[i] != arrRevPacket[8 + i])
                        {
                            clsLogManager.LogError("Fail to check byte index {0}", i);
                            return false;
                        }
                    }
                    clsLogManager.LogReport("Check RS485 pantilt, direction RS485-USB pass");
                    return true;
                }
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("CheckRS485Port: {0}", ex.ToString());
                return false;
            }
        }

        //public GPSSignal ReadGPSData(int Timeout)
        //{
        //    clsLogManager.LogWarning("ReadGPSData: ");
        //    try
        //    {
        //        m_ComSocket.FlushTxRx();
        //        for (int i = 0; i < 3; i++)
        //        {
        //            bool bOk = SendMessage(DIGITIZER_COMMAND.READ_GPS);
        //            if (!bOk)
        //            {
        //                clsLogManager.LogError("Fail to send message to DUT");
        //                continue;
        //            }
        //            byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.READ_GPS);
        //            if (receiveMes == null)
        //            {
        //                clsLogManager.LogError("Fail to get message from DUT");
        //                continue;
        //            }
        //            if (receiveMes.Length < 9)
        //            {
        //                clsLogManager.LogError("Response data length is wrong! Expected Length= {0} or more than, response Length= {1}", 9, receiveMes.Length);
        //                continue;
        //            }
        //            if(receiveMes[8]!=0x01)
        //            {
        //                clsLogManager.LogError("Have no GPS signal");
        //                continue;
        //            }    
        //            return GPSSignal.GetGPSSignalFromMessage(receiveMes);
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLogManager.LogError("ReadGPSData: ex {0}", ex.StackTrace);
        //        return null;
        //    }
        //}
        public bool CheckGPSData(int TimeOut)
        {
            clsLogManager.LogWarning("CheckGPSData: ");
            try
            {
                m_ComSocket.FlushTxRx();
                int BeginTime=Environment.TickCount;
                while (true)
                {
                    bool bOk = SendMessage(DIGITIZER_COMMAND.READ_GPS);
                    if (!bOk)
                    {
                        clsLogManager.LogError("Fail to send message to DUT");
                        goto CHECK_TIMEOUT;
                    }
                    byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.READ_GPS, TimeOut);
                    if (receiveMes == null)
                    {
                        clsLogManager.LogError("Fail to get message from DUT");
                        goto CHECK_TIMEOUT;
                    }
                    if (receiveMes.Length < 9)
                    {
                        clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 9, receiveMes.Length);
                        goto CHECK_TIMEOUT;
                    }
                    if( receiveMes[8] == 0x01)
                    {
                        return true;
                    }
                    Thread.Sleep(1000);
                    CHECK_TIMEOUT:
                    if(Environment.TickCount - BeginTime>=TimeOut)
                    {
                        clsLogManager.LogError("Timeout to check GPS Data");
                        return false;
                    }
                }
                //for (int i = 0; i < 3; i++)
                //{
                //    bool bOk = SendMessage(DIGITIZER_COMMAND.READ_GPS);
                //    if (!bOk)
                //    {
                //        clsLogManager.LogError("Fail to send message to DUT");
                //        continue;
                //    }
                //    byte[] receiveMes = GetMessage(DIGITIZER_COMMAND.READ_GPS, TimeOut);
                //    if (receiveMes == null)
                //    {
                //        clsLogManager.LogError("Fail to get message from DUT");
                //        continue;
                //    }
                //    if (receiveMes.Length < 9)
                //    {
                //        clsLogManager.LogError("Response data length is wrong! Expected Length= {0}, response Length= {1}", 9, receiveMes.Length);
                //        continue;
                //    }
                //    return receiveMes[8] == 0x01;
                //}
                //return false;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("CheckGPSData: ex {0}", ex.StackTrace);
                return false;
            }
        }
        #endregion
        #region private
        private bool SendMessage(DIGITIZER_COMMAND digitizer_COMMAND, byte[] Data)
        {
            try
            {
                byte[] mes = CreateMessage((byte)digitizer_COMMAND, Data);
                m_ComSocket.FlushTxRx();
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
        private bool SendMessage(DIGITIZER_COMMAND digitizer_COMMAND)
        {
            try
            {
                byte[] mes = CreateMessage((byte)digitizer_COMMAND);
                //m_ComSocket.FlushTxRx();
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
        private byte[] GetMessage(DIGITIZER_COMMAND digitizer_COMMAND, int timeOut=5000)
        {
            if (m_devInfo.ShowIOLog)
            {
                clsLogManager.LogReport("Get Message From DUT:...");
            }
            byte[] ReceiveHeader = new byte[] { 0x21, 0x03, 0x01, 0x19, 0x00 };
            ReceiveHeader[4] = (byte)((int)digitizer_COMMAND & 0x0F);
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

            byte[] mes = new byte[Data.Length + 8];
            for (int i = 0; i < Header.Length; i++)
            {
                mes[i] = Header[i];
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
