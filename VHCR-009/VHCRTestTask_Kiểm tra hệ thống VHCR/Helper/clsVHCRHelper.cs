using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.DeviceManager.DUT;
using CFTWinAppCore.DeviceManager.DUTIOService;
using CFTWinAppCore.Helper;
using CFTWinAppCore.Sequences;
using CFTWinAppCore.Task;
using LogLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VHCRTestTask.Tasks;


namespace VHCRTestTask
{
    public class clsVHCRHelper
    {
        public static int SearchNumberClosest(double[] arrData, double duTarget, bool bLog = false)
        {
            int nIndex = 0;
            double duTemp = Math.Abs(arrData[0] - duTarget);
            for (int i = 0; i < arrData.Length; i++)
            {
                if (duTemp > Math.Abs(arrData[i] - duTarget))
                {
                    duTemp = Math.Abs(arrData[i] - duTarget);
                    nIndex = i;
                }
            }
            if(bLog)
                clsLogManager.LogReport("Sarch Num {0}, closest number at {1} value {2}", duTarget, nIndex, arrData[nIndex]);
            return nIndex;
        }
        public static int SearchIndex(double[] arrData, double duTarget)
        {
            int nIndex = -1;
            for (int i = 0; i < arrData.Length; i++)
            {
                if (arrData[i] == duTarget)
                {
                    nIndex = i;
                    break;
                }
            }
            return nIndex;
        }
        public static int SearchMax(double[] arrData, bool bLog = false)
        {
            int nIndex = 0;
            double duTemp = arrData[0];
            for (int i = 0; i < arrData.Length; i++)
            {
                if (duTemp < arrData[i])
                {
                    duTemp = arrData[i];
                    nIndex = i;
                }
            }
            if(bLog)
                clsLogManager.LogReport("Max num {0}, at index {1}", arrData[nIndex], nIndex);
            return nIndex;
        }
        public static int SearchMin(double[] arrData, bool bLog = false)
        {
            int nIndex = 0;
            double duTemp = arrData[0];
            for (int i = 0; i < arrData.Length; i++)
            {
                if (duTemp > arrData[i])
                {
                    duTemp = arrData[i];
                    nIndex = i;
                }
            }
            if (bLog)
                clsLogManager.LogReport("Min num {0}, at index {1}", arrData[nIndex], nIndex);
            return nIndex;
        }

        #region Khainn1
        //public static int Get_WBHFTuneResult(ISequenceManager seq, double Freq, out bool result)
        //{
        //    try
        //    {
        //        result = false;
        //        List<ITask> lstTask = seq.Tasks;
        //        clsWBHF_Tuning TuneTask = null;
        //        for (int i = 0; i < lstTask.Count; i++)
        //        {
        //            TuneTask = lstTask[i] as clsWBHF_Tuning;
        //            if (TuneTask == null) continue;

        //            if (!TuneTask.ListResult.ContainsKey(Freq))
        //            {
        //                result = false;
        //                clsLogManager.LogWarning("Get_WBHFTuneResult(): ListResult doesn't contain key {0}", Freq);
        //                return -1; // Ăng ten kiểm tra không chứa tần số test
        //            }
        //            else
        //            {
        //                result = TuneTask.ListResult[Freq];
        //            }
        //            break;
                    
        //        }
        //        if (TuneTask == null)
        //        {
        //            result = false;
        //            return -99; // Chưa chạy bài dieu huong
        //        }
        //        return 1;
        //    }
        //    catch (Exception ec)
        //    {
        //        clsLogManager.LogError("Get_WBHFTuneResult() catched an exception: " + ec.Message);
        //        result = false;
        //        return -99;
        //    }
        //}
        //public static int Get_WBHFTuneFail_Times(ISequenceManager seq)
        //{
        //    try
        //    {
        //        List<ITask> lstTask = seq.Tasks;
        //        clsATU_DieuHuong TuneTask = null;
        //        for (int i = 0; i < lstTask.Count; i++)
        //        {
        //            TuneTask = lstTask[i] as clsATU_DieuHuong;
        //            if (TuneTask == null)
        //            {
        //                continue;
        //            }
        //            else
        //            {
        //                break;
        //            }
        //        }
        //        if (TuneTask == null)
        //        {
        //            return -99; // Chưa chạy bài dieu huong
        //        }
        //        return TuneTask.Final_Failtime;
        //    }
        //    catch (Exception ec)
        //    {
        //        clsLogManager.LogError("Get_WBHFTuneFail_Times() catched an exception: " + ec.Message);
        //        return -99;
        //    }
        //}
        //public static bool Set_DUTMode(IDutVHCR DUT, VHCR_Mod Mode, out string str)
        //{
        //    bool bOK = false;
        //    if (Mode == VHCR_Mod.FIX_BPSK)
        //    {
        //        bOK = DUT.SetParamChanel(;
        //    }
        //    else
        //    {
        //        if (Mode == DUTOperaMode.FIXS)
        //        {
        //            bOK = DUT.WBHF_API_1_1("FIX/S", out str);
        //        }
        //        else
        //        {
        //            bOK = DUT.WBHF_API_1_1(Mode.ToString(), out str);
        //        }
        //    }
        //    clsLogManager.LogReport("WBHF_API_1_1 (set mode) message: " + str);
        //    return bOK;
        //}

        

        //public static double Get_HPATxCurrent(ISequenceManager seq, double Freq, int DUTpower)
        //{
        //    try
        //    {
        //        List<ITask> lstTask = seq.Tasks;
        //        clsHPA_TxPower sensiTask = null;
        //        for (int i = 0; i < lstTask.Count; i++)
        //        {
        //            sensiTask = lstTask[i] as clsHPA_TxPower;
        //            if (sensiTask == null) continue;
        //            if ((sensiTask.Dutpower == DUTpower) && (sensiTask.DUTFreq == Freq))
        //                break;
        //        }
        //        if (sensiTask == null) return 0;
        //        return sensiTask.InputCurrent;
        //    }
        //    catch (Exception ec)
        //    {
        //        clsLogManager.LogError("Get_HPATxCurrent() catched an exception: " + ec.Message);
        //        return double.MinValue;
        //    }
        //}
        //public static double Get_HPARxCurrent(ISequenceManager seq, double Freq, double DUTpower)
        //{
        //    try
        //    {
        //        List<ITask> lstTask = seq.Tasks;
        //        clsHPA_InPutPower sensiTask = null;
        //        for (int i = 0; i < lstTask.Count; i++)
        //        {
        //            sensiTask = lstTask[i] as clsHPA_InPutPower;
        //            if (sensiTask == null) continue;
        //            if ((sensiTask.Dutpower == DUTpower) && (sensiTask.DUTFreq == Freq))
        //                break;
        //        }
        //        if (sensiTask == null) return 0;
        //        return sensiTask.InputCurrent;
        //    }
        //    catch (Exception ec)
        //    {
        //        clsLogManager.LogError("Get_HPARxCurrent() catched an exception: " + ec.Message);
        //        return double.MinValue;
        //    }
        //}
        //public static double Get_WBHF_TxCurrent(ISequenceManager seq, double Freq, double DUTpower)
        //{
        //    try
        //    {
        //        List<ITask> lstTask = seq.Tasks;
        //        clsWBHF_TxPower sensiTask = null;
        //        for (int i = 0; i < lstTask.Count; i++)
        //        {
        //            sensiTask = lstTask[i] as clsWBHF_TxPower;
        //            if (sensiTask == null) continue;
        //            if ((sensiTask.Dutpower == DUTpower) && (sensiTask.DUTFreq == Freq))
        //                break;
        //        }
        //        if (sensiTask == null)
        //        {
        //            clsLogManager.LogError($"Get_HPARxCurrent(): Chưa thực hiện bài đo công suất phát mức {DUTpower} tại tần số {Freq}MHz");
        //            return double.MinValue;
        //        }
        //        return sensiTask.InputCurrent;
        //    }
        //    catch (Exception ec)
        //    {
        //        clsLogManager.LogError("Get_HPARxCurrent() catched an exception: " + ec.Message);
        //        return double.MinValue;
        //    }
        //}
        public static double ConvertUnitAttenuator(double RFLevel, double Attenuator, PowerUnit unitSource, PowerUnit unitTarget)
        {
            double RFlevel_dBm = double.MinValue;
            double RFlevel_out = double.MinValue;
            switch (unitSource)
            {
                case PowerUnit.mV:
                    RFlevel_dBm = clsRFPowerUnitHelper.Vol2Dbm(RFLevel / 1000) + Attenuator;
                    break;
                case PowerUnit.Wat:
                    RFlevel_dBm = clsRFPowerUnitHelper.Wat2Dbm(RFLevel) + Attenuator;
                    break;
                case PowerUnit.uV:
                    RFlevel_dBm = clsRFPowerUnitHelper.Vol2Dbm(RFLevel / 1000000) + Attenuator;
                    break;
                case PowerUnit.dBm:
                    RFlevel_dBm = RFLevel + Attenuator;
                    break;
                
            }
            switch (unitTarget)
            {
                case PowerUnit.mV:
                    RFlevel_out = clsRFPowerUnitHelper.Dbm2Vol(RFlevel_dBm) * 1000;
                    break;
                case PowerUnit.Wat:
                    RFlevel_out = clsRFPowerUnitHelper.Dbm2Wat(RFlevel_dBm);
                    break;
                case PowerUnit.uV:
                    RFlevel_out = clsRFPowerUnitHelper.Dbm2Vol(RFlevel_dBm) * 1000000;
                    break;
                case PowerUnit.dBm:
                    RFlevel_out = RFlevel_dBm;
                    break;
            }
            return RFlevel_out;
        }

        public static int SetFreqInBandFilter(BandFilter Band)
        {
            int Freq = 0;  //MHz
            switch(Band)
            {
                case BandFilter.B_1_5to2_2:
                    Freq = 2;
                    break;
                case BandFilter.B_2_2to3_2:
                    Freq = 3;
                    break;
                case BandFilter.B_3_2to4_6:
                    Freq = 4;
                    break;
                case BandFilter.B_4_6to6_7:
                    Freq = 6;
                    break;
                case BandFilter.B_6_7to9_8:
                    Freq = 8;
                    break;
                case BandFilter.B_9_8to14_2:
                    Freq = 10;
                    break;
                case BandFilter.B14_2to20_6:
                    Freq = 16;
                    break;
                case BandFilter.B20_6to30:
                    Freq = 25;
                    break;
            }
            return Freq;
        }
        #endregion

    }
}
