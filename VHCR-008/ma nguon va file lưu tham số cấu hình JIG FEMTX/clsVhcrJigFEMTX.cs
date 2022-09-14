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
    [SysDevMetaAtt(typeof(clsVhcrJigFEMTX), SysDevType.JIGDEV, "JIG_VHCR_FEMTX")]
    public class clsVhcrJigFEMTX : IJIG_FEMTX
    {
        private SerialSocket m_ComSocket = null;
        private int m_nBauldRate;
        private clsDeviceInfor m_devInfo;
        private bool m_bShowIOLog = true;
        public clsVhcrJigFEMTX(clsDeviceInfor devInfo)
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
        public bool IsJIGFEMTX(int timeout)
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
                if(ret_data.Contains("JIG FEMTX"))
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
        public bool AD7293_IsConnected(int timeout)
        {
            string strCmd = "FEMTX_AD7293 ConnectCheck R 0\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                if (ret_data.Contains("FEMTX_AD7293 Connect OK 0\n"))
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
        public bool AD7420_IsConnected(int timeout)
        {
            string strCmd = "FEMTX_AD7420 ConnectCheck R 0\n";
            m_ComSocket.FlushTxRx();
            m_ComSocket.Transmit(strCmd);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Send:{0}", strCmd);
            string ret_data = m_ComSocket.ReadString((uint)timeout);
            if (m_bShowIOLog)
                clsLogManager.LogReport("Recv:{0}", ret_data);
            if (ret_data != null)
            {
                if (ret_data.Contains("FEMTX_AD7420 Connect OK 0\n"))
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
        public bool AD7293_GetStatus(out float tempINT, out float tempD0, out float tempD1, out int RS0, out int RS3, out int ADC0,out int ADC1,out int ADC2, out int ADC3, int timeout)
        {
            retry_label:
            string strCmd = "FEMTX_AD7293 " + "AD7293_Status" + " " + "R 0" + "\n";
            tempINT = 255.87f;
            tempD0 = 255.87f;
            tempD1 = 255.87f;
            RS0 = 5553;
            RS3 = 5553;
            ADC0 = 9326;
            ADC1 = 9326;
            ADC2 = 9326;
            ADC3 = 9326;
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == "AD7293_Status")
                {
                    string[] split_str_temp_INT = split_str[2].Split(new char[] { ':' });
                    tempINT = Convert.ToSingle(split_str_temp_INT[1]);
                    string[] split_str_temp_D0 = split_str[3].Split(new char[] { ':' });
                    tempD0 = Convert.ToSingle(split_str_temp_D0[1]);
                    string[] split_str_temp_D1 = split_str[4].Split(new char[] { ':' });
                    tempD1 = Convert.ToSingle(split_str_temp_D1[1]);
                    string[] split_str_temp_RS0 = split_str[5].Split(new char[] { ':' });
                    RS0 = Convert.ToInt32(split_str_temp_RS0[1]);
                    string[] split_str_temp_RS3 = split_str[6].Split(new char[] { ':' });
                    RS3 = Convert.ToInt32(split_str_temp_RS3[1]);
                    string[] split_str_temp_ADC0 = split_str[7].Split(new char[] { ':' });
                    ADC0 = Convert.ToInt32(split_str_temp_ADC0[1]);
                    string[] split_str_temp_ADC1 = split_str[8].Split(new char[] { ':' });
                    ADC1 = Convert.ToInt32(split_str_temp_ADC1[1]);
                    string[] split_str_temp_ADC2 = split_str[9].Split(new char[] { ':' });
                    ADC2 = Convert.ToInt32(split_str_temp_ADC2[1]);
                    string[] split_str_temp_ADC3 = split_str[10].Split(new char[] { ':' });
                    ADC3 = Convert.ToInt32(split_str_temp_ADC3[1]);
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_GetStatus recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_GetStatus lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_GetStatus no data recieve");
            return false;
        }
        public bool AD7293_SetVoltage(string param, int value, int timeout)
        {
            retry_label:
            string val0 =Math.Abs(value).ToString().PadLeft(4, '0');
            string val = " ";
            if (value < 0) val = "-" + val0;
            else val = "+" + val0;           
            string strCmd = "FEMTX_AD7293 " + param + " " + "W" + " " + val + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == param
                    && split_str[2] == "M"
                    /*&& split_str[3] == value.ToString()*/)
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_SetVoltage recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_SetVoltage lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                        return false;
                }
            }
            clsLogManager.LogError("AD7293_SetVoltage no data recieve");
            return false;
        }
        public bool AD7293_SetTempHighLimit(uint value, int timeout)
        {
        retry_label:
            string val0 = Math.Abs(value).ToString().PadLeft(3, '0');
            string strCmd = "FEMTX_AD7293 " + "SetTempHighLimit " + "W" + " " + val0 + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == "SetTempHighLimit")
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_SetTempHighLimit recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_SetTempHighLimit lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_SetTempHighLimit no data recieve");
            return false;
        }
        public bool AD7293_SetVoltageHighLimit(uint channel,uint value, int timeout)
        {
        retry_label:
            string val0 = Math.Abs(value).ToString().PadLeft(4, '0');;

            string strCmd = "";
            if(channel==0)
                strCmd= "FEMTX_AD7293 " + "SetVIN0HighLimit " + "W" + " " + val0 + "\n";
            else if (channel == 1)
                strCmd = "FEMTX_AD7293 " + "SetVIN1HighLimit " + "W" + " " + val0 + "\n";
            else if (channel == 2)
                strCmd = "FEMTX_AD7293 " + "SetVIN2HighLimit " + "W" + " " + val0 + "\n";
            else if (channel == 3)
                strCmd = "FEMTX_AD7293 " + "SetVIN3HighLimit " + "W" + " " + val0 + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && (split_str[1].Contains("SetVIN")))
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_SetVoltageHighLimit recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_SetVoltageHighLimit lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_SetVoltageHighLimit no data recieve");
            return false;
        }
        public bool AD7293_SetCurrentHighLimit(uint channel, uint value, int timeout)
        {
        retry_label:
            string val0 = Math.Abs(value).ToString().PadLeft(4, '0');
            string strCmd = "";
            if (channel == 0)
                strCmd = "FEMTX_AD7293 " + "SetCurrent0HighLimit " + "W" + " " + val0 + "\n";
            else if (channel == 1)
                strCmd = "FEMTX_AD7293 " + "SetCurrent1HighLimit " + "W" + " " + val0 + "\n";
            else if (channel == 2)
                strCmd = "FEMTX_AD7293 " + "SetCurrent2HighLimit " + "W" + " " + val0 + "\n";
            else if (channel == 3)
                strCmd = "FEMTX_AD7293 " + "SetCurrent3HighLimit " + "W" + " " + val0 + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && (split_str[1].Contains("SetCurrent")))
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_SetCurrentHighLimit recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_SetCurrentHighLimit lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_SetCurrentHighLimit no data recieve");
            return false;
        }
        public bool AD7293_GetAlerts(out uint Temp, out uint Curr, out uint Voltage,int timeout)
        {
        retry_label:
            string strCmd = "FEMTX_AD7293 " + "GetAlerts" + " " + "R 0" + "\n";
            Temp = 0xffff;
            Curr = 0xffff;
            Voltage = 0xffff;
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == "GetAlerts")
                {
                    string[] split_str_temp = split_str[2].Split(new char[] { ':' });
                    Temp = Convert.ToUInt16(split_str_temp[1]);
                    string[] split_str_Curr = split_str[3].Split(new char[] { ':' });
                    Curr = Convert.ToUInt16(split_str_Curr[1]);
                    string[] split_str_Voltage = split_str[4].Split(new char[] { ':' });
                    Voltage = Convert.ToUInt16(split_str_Voltage[1]);
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_GetAlerts recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_GetAlerts lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_GetAlerts no data recieve");
            return false;
        }
        public bool AD7293_GetHighLimit(out uint Temp, out uint Curr0, out uint Curr1, out uint Curr2, out uint Curr3, out uint VIN0, out uint VIN1, out uint VIN2, out uint VIN3, int timeout)
        {
        retry_label:
            string strCmd = "FEMTX_AD7293 " + "GetHighLimit" + " " + "R 0" + "\n";
            Temp = 0;
            Curr0 = 0;
            Curr1 = 0;
            Curr2 = 0;
            Curr3 = 0;
            VIN0 = 0;
            VIN1 = 0;
            VIN2 = 0;
            VIN3 = 0;
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == "GetHighLimit")
                {
                    string[] split_str_temp = split_str[2].Split(new char[] { ':' });
                    Temp = Convert.ToUInt16(split_str_temp[1]);
                    string[] split_str_Curr0 = split_str[3].Split(new char[] { ':' });
                    Curr0 = Convert.ToUInt16(split_str_Curr0[1]);
                    string[] split_str_Curr1 = split_str[4].Split(new char[] { ':' });
                    Curr1 = Convert.ToUInt16(split_str_Curr1[1]);
                    string[] split_str_Curr2 = split_str[5].Split(new char[] { ':' });
                    Curr2 = Convert.ToUInt16(split_str_Curr2[1]);
                    string[] split_str_Curr3 = split_str[6].Split(new char[] { ':' });
                    Curr3 = Convert.ToUInt16(split_str_Curr3[1]);
                    string[] split_str_VIN0 = split_str[7].Split(new char[] { ':' });
                    VIN0 = Convert.ToUInt16(split_str_VIN0[1]);
                    string[] split_str_VIN1 = split_str[8].Split(new char[] { ':' });
                    VIN1 = Convert.ToUInt16(split_str_VIN1[1]);
                    string[] split_str_VIN2 = split_str[9].Split(new char[] { ':' });
                    VIN2 = Convert.ToUInt16(split_str_VIN2[1]);
                    string[] split_str_VIN3 = split_str[10].Split(new char[] { ':' });
                    VIN3 = Convert.ToUInt16(split_str_VIN3[1]);
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_GetHighLimit recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_GetHighLimit lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_GetHighLimit no data recieve");
            return false;
        }
        public bool AD7293_CheckProtect(out bool isProtect, int timeout)
        {
        retry_label:
            string strCmd = "FEMTX_AD7293 " + "CheckProtect" + " " + "R 0" + "\n";
            isProtect = true;
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == "CheckProtect")
                {
                    int value = Convert.ToInt32(split_str[2]);
                    if (value == 1)
                        isProtect = true;
                    else
                        isProtect = false;
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_CheckProtect recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_CheckProtect lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_CheckProtect no data recieve");
            return false;
        }
        public bool AD7293_SetAutoProtect(bool autoProtect, int timeout)
        {
        retry_label:
            string strCmd = "";
            if (autoProtect==true)
                strCmd= "FEMTX_AD7293 " + "MonitorAndProtect" + " " + "W 1" + "\n";
            else
                strCmd = "FEMTX_AD7293 " + "MonitorAndProtect" + " " + "W 0" + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == "MonitorAndProtect")
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_SetAutoProtect recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_SetAutoProtect lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_SetAutoProtect no data recieve");
            return false;
        }
        public bool AD7293_ResetAlerts(int timeout)
        {
        retry_label:
            string strCmd = "FEMTX_AD7293 " + "ResetAlert" + " " + "R 0" + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == "ResetAlert")
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_ResetAlerts recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_ResetAlerts lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_ResetAlerts no data recieve");
            return false;
        }
        public bool AD7293_ReadCurrent(out int value, string param, int timeout)
        {
            retry_label:
            string strCmd = "FEMTX_AD7293 " + param + " " + "R 0" + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == param)
                {
                    value = Convert.ToInt32(split_str[2]);
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_ReadCurrent recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_ReadCurrent lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_ReadCurrent no data recieve");
            return false;
        }
        public bool AD7293_ReadTemprature(out float value, string param, int timeout)
        {
            retry_label:
            string strCmd = "FEMTX_AD7293 " + param + " " + "R 0" + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == param)
                {
                    value = Convert.ToSingle(split_str[2]);
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_ReadTemprature recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_ReadTemprature lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_ReadTemprature no data recieve");
            return false;
        }
        public bool AD7293_WritePin(string param, int value, int timeout)
        {
            retry_label:
            string strCmd = "FEMTX_AD7293 " + param + " " + "W " + value + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == param)
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_WritePin recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_WritePin lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_WritePin no data recieve");
            return false;
        }
        public bool AD7293_VD_HPA_ON(bool onoff, int timeout)
        {
            retry_label:
            string strCmd = "";
            if (onoff==true)
                strCmd = "FEMTX_AD7293 " + "VD_HPA " + "W " + "ON" + "\n";
            else
                strCmd = "FEMTX_AD7293 " + "VD_HPA " + "W " + "OFF" + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == "VD_HPA")
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_VD_HPA_ON recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_VD_HPA_ON lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_VD_HPA_ON no data recieve");
            return false;
        }
        public bool AD7293_ReadPin(out int value, string param, int timeout)
        {
            retry_label:
            string strCmd = "FEMTX_AD7293 " + param + " " + "R 0" + "\n";
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
                if (split_str[0] == "FEMTX_AD7293"
                    && split_str[1] == param)
                {
                    value = Convert.ToInt32(split_str[2]);
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7293_ReadPin recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7293_ReadPin lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7293_ReadPin no data recieve");
            return false;
        }
        public bool AD7420_ReadTemperature(out float value, int timeout)
        {
            retry_label:
            string strCmd = "FEMTX_AD7420" + " " + "R 0 0" + "\n";
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
                if (split_str[0] == "FEMTX_AD7420")
                {
                    value = Convert.ToSingle(split_str[1]);
                    return true;
                }
                else
                {
                    clsLogManager.LogError("AD7420_ReadTemperature recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "AD7420_ReadTemperature lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("AD7420_ReadTemperature no data recieve");
            return false;
        }
        public bool PE43705B_SetAtt(float value, int timeout)
        {
            retry_label:
            string strCmd = "FEMTX_PE43705B ATT" + " " + "W " + value.ToString("00.00") + "\n";
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
                string[] split_str = ret_data.Split(new char[] { ' ', '\r' });
                if (split_str[0] == "FEMTX_PE43705B"
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
        public bool FEMTX_IO_Write(string param, int value, int timeout)
        {
            retry_label:
            string strCmd = "FEMTX_IO " + param + " " + "W " + value + "\n";
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
                if (split_str[0] == "FEMTX_IO"
                    && split_str[1] == param)
                {
                    return true;
                }
                else
                {
                    clsLogManager.LogError("FEMTX_IO_Write recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "FEMTX_IO_Write lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }

                    return false;
                }
            }
            clsLogManager.LogError("FEMTX_IO_Write no data recieve");
            return false;
        }
        public bool FEMTX_IO_ReadPin(out int value, string param, int timeout)
        {
            retry_label:
            string strCmd = "FEMTX_IO " + param + " " + "R 0" + "\n";
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
                if (split_str[0] == "FEMTX_IO"
                    && split_str[1] == param)
                {
                    value = Convert.ToInt32(split_str[2]);
                    return true;
                }
                else
                {
                    clsLogManager.LogError("FEMTX_IO_ReadPin recieve data error");
                    if (clsMsgHelper.ShowYesNo("Notice", "FEMTX_IO_ReadPin lỗi, có thực hiện lại không?") == DialogResult.Yes)
                    {
                        goto retry_label;
                    }
                    return false;
                }
            }
            clsLogManager.LogError("FEMTX_IO_ReadPin no data recieve");
            return false;
        }
        public void InitEventHandle(ExternLib.SerialPort.ReceivedDataEventHandler e)
        {
            m_ComSocket.OnReceivedData += e;
        }
        public void StartAsyncRead()
        {
            m_ComSocket.StartAsyncRead();
        }
    }

}
