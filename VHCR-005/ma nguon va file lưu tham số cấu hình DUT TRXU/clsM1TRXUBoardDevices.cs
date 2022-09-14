using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.Helper;
using ExternLib.SerialPort;
using InstrumentIOService;
using LogLibrary;
using Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeneralTool;
namespace CFTSeqManager.DUT
{
    [SysDevMetaAtt(typeof(clsM1TRXUBoardDevices), SysDevType.TRXU_BOARD, "M1 TRXU Board Device")]
    public class clsM1TRXUBoardDevices : ITRXUBoard, IAccessDeviceService
    {
        private TcpClient m_socketClient = null;
        private NetworkStream m_nsStream = null;
        bool is_connected = false;
        private clsDeviceInfor m_devInfo;
        //private clsCrcHelper crcByte = new clsCrcHelper();
        int m_nIOTimeOut = 2000;
        public clsM1TRXUBoardDevices(clsDeviceInfor devInfo)
        {
            m_devInfo = devInfo;
            ShowIOLog = true;
        }

        public bool Connect2Device(IDeviceInfor devInfo)
        {
            try
            {
                int nPort = 8000;
                string strIPAddress = m_devInfo.DevAddress;//"127.0.0.1";
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(strIPAddress), nPort);
                if (m_socketClient != null)
                {
                    if (!m_socketClient.Connected)
                    {
                        m_socketClient.Close();
                        m_socketClient = null;
                        m_nsStream.Close();
                        m_nsStream.Dispose();
                    }
                }
                is_connected = Connect(endPoint, 2000);
                return is_connected;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Connect2Device: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }
        public void DisconnectDevice()
        {
            if (m_socketClient != null)
            {
                m_socketClient.Close();
                m_nsStream.Close();
                m_nsStream.Dispose();
                is_connected = false;
            }
        }
        private bool Connect(IPEndPoint remoteEndPoint, int timeoutMSec)
        {
            try
            {
                if (m_socketClient == null)
                {
                    m_socketClient = new TcpClient();
                    // Don't allow another socket to bind to this port.
                    m_socketClient.Client.ExclusiveAddressUse = true;

                    // The socket will linger for 10 seconds after  
                    // Socket.Close is called.
                    //m_socketClient.Client.LingerState = new LingerOption(true, 10);

                    // Disable the Nagle Algorithm for this tcp socket.
                    m_socketClient.Client.NoDelay = true;

                    // Set the receive buffer size to 8k
                    m_socketClient.Client.ReceiveBufferSize = 8192;

                    // Set the timeout for synchronous receive methods to  
                    // 1 second (1000 milliseconds.)
                    //m_socketClient.Client.ReceiveTimeout = 1000;

                    // Set the send buffer size to 8k.
                    m_socketClient.Client.SendBufferSize = 8192;

                    // Set the timeout for synchronous send methods 
                    // to 1 second (1000 milliseconds.)			
                    //m_socketClient.Client.SendTimeout = 1000;

                    // Set the Time To Live (TTL) to 42 router hops.
                    m_socketClient.Client.Ttl = 42;
                }
                IAsyncResult asyncResult = m_socketClient.BeginConnect(remoteEndPoint.Address, remoteEndPoint.Port, null, null);
                if (asyncResult.AsyncWaitHandle.WaitOne(timeoutMSec, false))
                {
                    try
                    {
                        m_socketClient.EndConnect(asyncResult);
                        m_nsStream = m_socketClient.GetStream();
                        //m_StreamReader = new StreamReader(m_nsStream);
                        //m_IsConnectionSuccessful = true;
                        //DeviceActived = true;
                        clsLogManager.LogReport("Connect to {0}, port {1} successful", remoteEndPoint.Address.ToString(), remoteEndPoint.Port);
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        m_socketClient.Close();
                        m_socketClient = null;
                        throw ex;
                    }
                }
                else
                {
                    m_socketClient.Close();
                    m_socketClient = null;
                    throw new TimeoutException("TimeOut Exception");
                }
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("Connect: {0}", ex.ToString());
                if (m_socketClient != null)
                {
                    m_socketClient.Close();
                    m_socketClient = null;
                }
                return false;
            }
        }
        private bool WriteBytes(byte[] arrData)
        {
            m_nsStream.Write(arrData, 0, arrData.Length);
            m_nsStream.Flush();
            return true;
        }
        private byte[] ReadPacketData(short nHeader)
        {
            byte[] arrPacketData = null;
            byte[] arrTmp = null;
            short nReadHeader = 0;
            int nDataLength = 0;
            while (true)
            {
                arrTmp = ReadData(4);//Read header
                nDataLength = 0;
                if (arrTmp == null) break;
                nReadHeader = BitConverter.ToInt16(arrTmp, 0); //
                nReadHeader = IPAddress.NetworkToHostOrder(nReadHeader);
                if (nReadHeader == nHeader)
                {
                    nDataLength = BitConverter.ToInt16(arrTmp, 2); //
                    nDataLength = IPAddress.NetworkToHostOrder(nDataLength);
                    clsLogManager.LogReport("Packet data length: {0}", nDataLength);
                    break;
                }
                else
                    clsLogManager.LogWarning("Read packet header {0} expect header {1}", nReadHeader, nHeader);
            }
            if (nDataLength > 0)
            {
                arrPacketData = ReadData(nDataLength);
                if (arrPacketData != null)
                {
                    if (ShowIOLog)
                    {
                        string strText = clsBufferHeper.ConvertArr2HexStrSingLine<byte>(arrPacketData, arrPacketData.Length);
                        WriteLog($"[R]: {strText}");
                    }
                }
            }
            return arrPacketData;
        }
        private byte[] ReadData(int nLengthToRead)
        {
            string strResult = string.Empty;
            int nBeginTime = Environment.TickCount;
            int nReadCnt = 0;
            byte[] arrData = new byte[nLengthToRead];
            try
            {
                IAsyncResult result = m_nsStream.BeginRead(arrData, 0, arrData.Length, null, null);
                result.AsyncWaitHandle.WaitOne(m_nIOTimeOut);
                nReadCnt = m_nsStream.EndRead(result);
                if (result.IsCompleted)
                    return arrData;
                return null;
            }
            catch (SocketException ex)
            {
                //(int)SocketError.TimedOut
                clsLogManager.LogError("ReadData: {0}", ex.ToString());
                return null;
            }
        }
        public byte[] GetMessageByte(byte[] byteHeader, int nTimeOut)
        {
            byte[] bufIn = null;
            byte[] bufOut = null;
            //byte[] returnByte = null;
            int nPos = 0;
            int nBegin = Environment.TickCount;
            int nCurrTimeOut = 0;
            try
            {
                int len = 0;
                bufIn = new byte[1024];
                bufOut = new byte[1024];
                int indexHeader = 0;
                //int indexEndByte = 0;
                bool readHeaderSuccess = false;
                //get message header
                while (true)
                {
                    if (m_socketClient.Available > 0)
                    {
                        len = m_socketClient.Available;
                        m_nsStream.Read(bufIn, 0, len);
                        if (len <= 0)
                        {
                            goto CHECK_TIMEOUT; ;
                        }
                        else
                        {
                            for (int i = 0; i < len; i++)
                            {
                                bufOut[nPos++] = bufIn[i];
                            }
                            //check header: 
                            //clsLogManager.LogReport("RX:{0}",ASCIIEncoding.ASCII.GetString(bufOut));
                            if (!readHeaderSuccess)
                            {
                                //clsLogManager.LogReport("Check header, RX: " + clsBufferHelper.ConvertArr2Str(bufOut));
                                for (int k = 0; k <= bufOut.Length - byteHeader.Length; k++)
                                {
                                    for (int i = 0; i < byteHeader.Length; i++)
                                    {
                                        if (bufOut[k + i] != byteHeader[i])
                                            break;
                                        else
                                        {
                                            if (i < byteHeader.Length - 1)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                readHeaderSuccess = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (readHeaderSuccess)
                                    {
                                        indexHeader = k;
                                        break;
                                    }
                                }
                            }
                        }
                        if (readHeaderSuccess)
                        {
                            //return
                            byte[] headerByte = new byte[byteHeader.Length+2];
                            for (int i = 0; i < headerByte.Length; i++)
                            {
                                headerByte[i] = bufOut[indexHeader + i];
                            }
                            int dataLength = 16*(int)headerByte[2] + (int)headerByte[3];
                            if (nPos < dataLength + 4) goto CHECK_TIMEOUT;

                            byte[] resultByte = new byte[dataLength + 4];
                            for (int i = 0; i < resultByte.Length; i++)
                            {
                                resultByte[i] = bufOut[indexHeader + i];
                            }
                            return resultByte;
                        }
                    }
                    CHECK_TIMEOUT:
                    nCurrTimeOut = Environment.TickCount - nBegin;
                    if (nCurrTimeOut >= nTimeOut)
                    {
                        clsLogManager.LogError("ReadString: Timeout for reading data from tcp server ");
                        break;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("ReadString: {0}", ex.ToString());
                return null;
            }
            finally
            {
                bufIn = null;
                m_nsStream.Flush();
                ClearTcpReceivedBuffer();
            }
        }
        private void ClearTcpReceivedBuffer()
        {
            int nBufferCnt = m_socketClient.Available;
            if (nBufferCnt > 0)
            {
                byte[] arrRevData = new byte[nBufferCnt + 1];
                m_nsStream.Read(arrRevData, 0, arrRevData.Length);
                //clsLogManager.LogWarning("Tcp received buffer is not empty->clear {0} bytes", nBufferCnt);
            }
        }
        public bool IsConnected()
        {
            clsLogManager.LogReport("IsConnected");
            return is_connected;
        }

        public bool SetTxPowerMode(VHCR_Power power)
        {
            try
            {
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                data_to_send[0] = 0x14; data_to_send[1] = 0x01;
                data_to_send[2] = 0x00; data_to_send[3] = 0x23;
                data_to_send[4] = 0x00; data_to_send[5] = 0x00; data_to_send[6] = 0x00; data_to_send[7] = 0x01;
                data_to_send[8] = 0x00; data_to_send[9] = 0x00; data_to_send[10] = 0x00; data_to_send[11] = 0x00;
                data_to_send[12] = 0x00; data_to_send[13] = 0x00; data_to_send[14] = 0x00; data_to_send[15] = (byte)power;
                data_to_send[16] = 0x01;
                data_to_send[17] = 0xff; data_to_send[18] = 0xff; data_to_send[19] = 0xff;
                data_to_send[20] = 0xff; data_to_send[21] = 0xff; data_to_send[22] = 0xff; data_to_send[23] = 0xff;
                data_to_send[24] = 0xff; data_to_send[25] = 0xff; data_to_send[26] = 0xff; data_to_send[27] = 0xff;
                data_to_send[28] = 0x00; data_to_send[29] = 0x00; data_to_send[30] = 0x00; data_to_send[31] = 0x00;
                data_to_send[32] = 0x02;
                data_to_send[33] = 0x03;
                data_to_send[34] = 0x04;
                data_to_send[35] = 0x0A;
                data_to_send[36] = 0x00;
                data_to_send[37] = 0xff; data_to_send[38] = 0xff;
                WriteBytes(data_to_send);
                if (ShowIOLog)
                {
                    string strText = clsBufferHeper.ConvertArr2HexStrSingLine<byte>(data_to_send, data_to_send.Length);
                    WriteLog($"[S]: {strText}");
                }
                clsLogManager.LogReport("SetPower");
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("SetPower: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }
        public bool ShowIOLog { set; get; }
        public bool Reset()
        {
            clsLogManager.LogReport("Reset");
            is_connected = false;
            return true;
        }
        public string getVersion(UInt32 timeout)
        {
            clsLogManager.LogReport("getVersion");
            return "V1.0";
        }
        public void Close()
        {
            is_connected = false;
        }
        public bool SetTxChanelConfig(VHCR_Wareform wareform, VHCR_Mod Modulation, 
                                VHCR_Bandwidth Bandwidth, UInt64 Tx_freq,
                                UInt64 Rx_freq, UInt32 Tx_hop, UInt32 Rx_hop)
        {
            try
            {
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                data_to_send[0] = 0x14; data_to_send[1] = 0x3E;
                data_to_send[2] = 0x00; data_to_send[3] = 0xAC;
                data_to_send[4] = 0x00; data_to_send[5] = 0x00; data_to_send[6] = 0x00; data_to_send[7] = 0x00;
                data_to_send[8] = 0x00; data_to_send[9] = 0x00; data_to_send[10] = 0x00; data_to_send[11] = (byte)wareform;
                data_to_send[12] = 0x00; data_to_send[13] = 0x00; data_to_send[14] = 0x00; data_to_send[15] = (byte)Modulation;
                data_to_send[16] = 0x00; data_to_send[17] = 0x00; data_to_send[18] = 0x00; data_to_send[19] = (byte)Bandwidth;
                data_to_send[20] = 0x00; data_to_send[21] = 0x00; data_to_send[22] = 0x00; data_to_send[23] = 0x00;
                data_to_send[24] = (byte)(Tx_freq >> 24); data_to_send[25] = (byte)(Tx_freq >> 16); data_to_send[26] = (byte)(Tx_freq >> 8); data_to_send[27] = (byte)Tx_freq;
                data_to_send[28] = (byte)(Rx_freq >> 24); data_to_send[29] = (byte)(Rx_freq >> 16); data_to_send[30] = (byte)(Rx_freq >> 8); data_to_send[31] = (byte)Rx_freq;
                for (int i = 0; i < 64; i++)
                    data_to_send[32 + i] = 0;
                for (int i = 0; i < 64; i++)
                    data_to_send[96 + i] = 0;
                data_to_send[160] = 0x00; data_to_send[161] = 0x00; data_to_send[162] = 0x00; data_to_send[163] = (byte)Tx_hop;
                data_to_send[164] = 0x00; data_to_send[165] = 0x00; data_to_send[166] = 0x00; data_to_send[167] = (byte)Rx_hop;
                data_to_send[168] = 0x00; data_to_send[169] = 0x00; data_to_send[170] = 0x00; data_to_send[171] = 0x00;
                data_to_send[172] = 0x00; data_to_send[173] = 0x00; data_to_send[174] = 0x00; data_to_send[175] = 0x00;
                WriteBytes(data_to_send);
                if (ShowIOLog)
                {
                    string strText = clsBufferHeper.ConvertArr2HexStrSingLine<byte>(data_to_send, data_to_send.Length);
                    WriteLog($"[S]: {strText}");
                }
                clsLogManager.LogReport("SetParamChanel");
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("SetParamChanel: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }
        public bool StartRxBER()
        {
            try
            {
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                data_to_send[0] = 0x07; data_to_send[1] = 0x12;
                data_to_send[2] = 0x00; data_to_send[3] = 0x04;
                data_to_send[4] = 0x01; data_to_send[5] = 0x2F; data_to_send[6] = 0x00; data_to_send[7] = 0x00;
                WriteBytes(data_to_send);
                if (ShowIOLog)
                {
                    string strText = clsBufferHeper.ConvertArr2HexStrSingLine<byte>(data_to_send, data_to_send.Length);
                    WriteLog($"[S]: {strText}");
                }
                clsLogManager.LogReport("switchEnalbeBER");
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("switchEnalbeBER: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }

        public bool SetTxModulationMode(bool bIsCWMode = true)
        {
            try
            {
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                data_to_send[0] = 0x07; data_to_send[1] = 0x12;
                data_to_send[2] = 0x00; data_to_send[3] = 0x02;
                data_to_send[4] = 0x01; data_to_send[5] = 0x2e; data_to_send[6] = 0x00; data_to_send[7] = 0x00;
                WriteBytes(data_to_send);
                if (ShowIOLog)
                {
                    string strText = clsBufferHeper.ConvertArr2HexStrSingLine<byte>(data_to_send, data_to_send.Length);
                    WriteLog($"[S]: {strText}");
                }
                clsLogManager.LogReport("StartCW");
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("switchEnalbeBER: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }

        public double GetBER(int timeout)
        {
            //try
            //{
            //    byte[] data_to_send = new byte[264];
            //    for (int i = 0; i < 264; i++)
            //        data_to_send[i] = 0;
            //    data_to_send[0] = 0x07; data_to_send[1] = 0x26;
            //    data_to_send[2] = 0x00; data_to_send[3] = 0x00;
            //    //m_IOService.SetIOTimeout(timeout);
            //    //byte[] ret = m_IOService.SendAndRead(data_to_send);
            //    //if (ret == null) return -1;
            //    //clsLogManager.LogReport("getBER");
            //    return 0;
            //}
            //catch (Exception ex)
            //{
            //    clsLogManager.LogError("getBER: {0}", ex.ToString());
            //    is_connected = false;
            //    return -1;
            //}
            try
            {
                RETRY_LABEL:
                m_nIOTimeOut = timeout;
                byte[] data = new byte[4];
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                data_to_send[0] = 0x07; data_to_send[1] = 0x26;
                data_to_send[2] = 0x00; data_to_send[3] = 0x00;
                //clsLogManager.LogReport("Get Ber Mess: {0}",clsBufferHeper.ConvertArr2HexStrSingLine<byte>(data_to_send, data_to_send.Length));
                for(int j = 0; j < 3; j++)
                {
                    WriteBytes(data_to_send);
                    //clsLogManager.LogReport("Wait BER from DUT");
                    byte[] ret = GetMessageByte(new byte[] { 0x07, 0x26 }, timeout);
                    if (ret == null) continue;
                    if (ret[0] == 0x07 && ret[1] == 0x26)
                    {
                        if(ret.Length<20)
                        {
                            clsLogManager.LogReport("Message Length is incorrect expected length is 20, current length is {0}", ret.Length);
                            continue;
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            data[3 - i] = ret[16 + i];
                        }

                        clsLogManager.LogReport($"BER Data = [{data[0]:x}, {data[1]:x}, {data[2]:x}, {data[3]:x}]");
                        return BitConverter.ToSingle(data, 0);
                    }
                    else
                    {
                        clsLogManager.LogReport("GetBER Fail Header!");
                        if (clsMsgHelper.ShowYesNo("Question", "GetBER Fail Header, bạn có thực hiện lại") == DialogResult.No)
                        {
                            return float.MaxValue;
                        }
                        else
                        {
                            goto RETRY_LABEL;
                        }
                    }
                }
                return float.MaxValue;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("getBER: {0}", ex.ToString());
                is_connected = false;
                return float.MaxValue;
            }
        }
        private void WriteLog(string strText)
        {
            if (ShowIOLog == false)
                return;
            clsLogManager.LogReport(strText);
        }
        public void WriteLog(string strFmt, params object[] arg)
        {
            string strCmd = string.Format(strFmt, arg);
            WriteLog(strCmd);
        }

        public bool InitDevice()
        {
            return true;
        }

        public bool IsDeviceConnected()
        {
            return is_connected;
        }

        public bool SetIOTimeOut(int nTimeOut)
        {
            return true;
        }

        public object GetIOLibrary()
        {
            return m_socketClient;
        }

        public void ShowDeviceTestDialog()
        {
            throw new NotImplementedException();
        }

        public bool ShowDeviceProperties()
        {
            throw new NotImplementedException();
        }

        public bool IsDeviceSupportCusDevProperties()
        {
            return false;
        }

        public string GetDeviceFriendlyName()
        {
            return "M1 TRXU Board";
        }
    }
}
