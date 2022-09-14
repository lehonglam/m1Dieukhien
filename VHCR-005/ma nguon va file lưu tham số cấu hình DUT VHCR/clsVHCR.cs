using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.DeviceManager.DUT;
using ExternLib.SerialPort;
using InstrumentIOService;
using LogLibrary;
using Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneralTool;
using System.Windows.Forms;
using System.Threading;
namespace CFTSeqManager.DUT
{
    [SysDevMetaAtt(typeof(clsVHCR), SysDevType.DUT, "DUT VHCR")]
    public class clsVHCR : IDutVHCR, IAccessDeviceService
    {
        IISMessage m_IOService;
        bool is_connected = false;
        private clsDeviceInfor m_devInfo;
        //private clsCrcHelper crcByte = new clsCrcHelper();

        public clsVHCR(clsDeviceInfor devInfo)
        {
            m_devInfo = devInfo;
        }
        public void DisconnectDevice()
        {
            if (m_IOService == null) return;
            m_IOService.Dispose();
            m_IOService = null;
            is_connected = false;
        }
        public string GetDeviceFriendlyName()
        {
            return "VHCR";
        }
        public object GetIOLibrary()
        {
            return m_IOService;
        }
        public bool InitDevice()
        {
            return false;
        }
        public bool IsDeviceConnected()
        {
            try
            {
                if (m_IOService == null) return false;
                if (!m_IOService.DeviceActived) return false;
                return true;
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
            m_IOService.SetIOTimeout(nTimeOut);
            return true;
        }
        public void ShowDeviceTestDialog()
        {
            throw new NotImplementedException();
        }
        public bool ShowDeviceProperties()
        {
            return false;
        }
        public bool Connect2Device(IDeviceInfor devInfo)
        {
            try
            {
                m_IOService = clsFactoryIOService.CreateIOService(devInfo.IOServiceType);
                is_connected = m_IOService.Connect(devInfo.DevAddress, 3000);
                m_IOService.ReadString();
                return is_connected;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Connect2Device: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }

        public bool IsConnected()
        {
            clsLogManager.LogReport("IsConnected");
            return is_connected;
        }

        public bool SetPower(VHCR_Power power)
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
                m_IOService.Write(data_to_send);
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
        public bool SetParamChanel(VHCR_Wareform wareform, VHCR_Mod Modulation, 
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
                m_IOService.Write(data_to_send);
                clsLogManager.LogReport("SetParamChanel:{0}",BitConverter.ToString(data_to_send));
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("SetParamChanel: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }
        public bool switchEnalbeBER()
        {
            try
            {
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                data_to_send[0] = 0x07; data_to_send[1] = 0x12;
                data_to_send[2] = 0x00; data_to_send[3] = 0x04;
                data_to_send[4] = 0x01; data_to_send[5] = 0x2F; data_to_send[6] = 0x00; data_to_send[7] = 0x00;
                m_IOService.Write(data_to_send);
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

        public bool StartCW()
        {
            try
            {
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                data_to_send[0] = 0x07; data_to_send[1] = 0x12;
                data_to_send[2] = 0x00; data_to_send[3] = 0x02;
                data_to_send[4] = 0x01; data_to_send[5] = 0x2e; data_to_send[6] = 0x00; data_to_send[7] = 0x00;
                m_IOService.Write(data_to_send);
                clsLogManager.LogReport("StartCW");
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("StartCW: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }

        public float getBER(int timeout)
        {
            try
            {
                RETRY_LABEL:
                byte[] data = new byte[4];
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                data_to_send[0] = 0x07; data_to_send[1] = 0x26;
                data_to_send[2] = 0x00; data_to_send[3] = 0x00;
                m_IOService.SetIOTimeout(timeout);
                clsLogManager.LogReport($"SendtoGetBer: {BitConverter.ToString(data_to_send,0,data_to_send.Length)}");
                byte[] ret = m_IOService.SendAndRead(data_to_send);
                clsLogManager.LogReport($"getBER: {string.Join("", BitConverter.ToString(ret).Replace("-", ""))}, {ret.Count()}");
                if (ret[0] == 0x07 && ret[1] == 0x26)
                {                   
                    for(int i = 0; i < 4; i++)
                    {
                        data[3-i] = ret[16 + i];
                    }

                    clsLogManager.LogReport($"BER Data = [{data[0]:x}, {data[1]:x}, {data[2]:x}, {data[3]:x}]");
                    return BitConverter.ToSingle(data, 0);
                    //return BitConverter.ToSingle(ret, 16);
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
                        Thread.Sleep(6000);
                        goto RETRY_LABEL;
                    }

                }
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("getBER: {0}", ex.ToString());
                is_connected = false;
                return float.MaxValue;
            }
        }
        /// <summary>
        /// Turn on/off Automatic power control
        ///<para>ONOFF=true: Turn on</para> 
        ///<para>ONOFF=false: Turn off</para> 
        /// </summary>
        /// <param name="ONOFF"></param>
        /// <returns></returns>
        public bool AutomaticPowerControl(bool ONOFF)
        {
            try
            {
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                data_to_send[0] = 0x07; data_to_send[1] = 0x12;
                data_to_send[2] = 0x00; data_to_send[3] = 0x05;
                data_to_send[4] = 0x60; data_to_send[5] = 0x03; data_to_send[6] = 0x00; data_to_send[7] = 0x01;
                if (ONOFF == false)
                {
                    data_to_send[8] = 0x01;
                    m_IOService.Write(data_to_send);
                    clsLogManager.LogReport($"AutomaticPowerControl:OFF");
                }
                else
                {
                    data_to_send[8] = 0x02;
                    m_IOService.Write(data_to_send);
                    clsLogManager.LogReport($"AutomaticPowerControl:ON");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("StartCW: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }
        /// <summary>
        /// Set Attenuation 
        /// <para>Input Range : 5dB to 35dB</para>
        /// </summary>
        /// <param name="ATT"></param>
        /// <returns></returns>
        public bool SetAttenuation(double ATT)
        {
            try
            {
                if((ATT<5)|(ATT>35))
                {
                    clsLogManager.LogReport($"Attenuation value is not in allow range (5 dB to 35 dB)");
                    return false;
                }
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                byte[] temp = new byte[2];
                data_to_send[0] = 0x07; data_to_send[1] = 0x12;
                data_to_send[2] = 0x00; data_to_send[3] = 0x06;
                data_to_send[4] = 0x60; data_to_send[5] = 0x01; data_to_send[6] = 0x00; data_to_send[7] = 0x02;
                int attx100 = (int)(ATT * 100);
                temp = BitConverter.GetBytes(attx100); 
                data_to_send[8] = temp[1]; data_to_send[9] = temp[0];
                m_IOService.Write(data_to_send);
                    clsLogManager.LogReport($"Attenuation value={ATT} dB");
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("StartCW: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }
        /// <summary>
        /// Set PowerRF
        /// <para>Input Range: 28dBm to 36 dBm</para>
        /// </summary>
        /// <param name="PWR"></param>
        /// <returns></returns>
        public bool SetPowerRF(double PWR)
        {
            try
            {
                if ((PWR <28) | (PWR > 36))
                {
                    clsLogManager.LogReport($"Power value is not in allow range (28 dBm to 36 dBm)");
                    return false;
                }
                byte[] data_to_send = new byte[264];
                for (int i = 0; i < 264; i++)
                    data_to_send[i] = 0;
                byte[] temp = new byte[2];
                data_to_send[0] = 0x07; data_to_send[1] = 0x12;
                data_to_send[2] = 0x00; data_to_send[3] = 0x06;
                data_to_send[4] = 0x60; data_to_send[5] = 0x02; data_to_send[6] = 0x00; data_to_send[7] = 0x02;
                int PWRx10 = (int)(PWR * 10);
                temp = BitConverter.GetBytes(PWRx10);
                data_to_send[8] = temp[1]; data_to_send[9] = temp[0];
                m_IOService.Write(data_to_send);
                clsLogManager.LogReport($"Power value={PWR} dBm");
                return true;
            }
            catch (Exception ex)
            {
                clsLogManager.LogError("StartCW: {0}", ex.ToString());
                is_connected = false;
                return false;
            }
        }
    }
}
