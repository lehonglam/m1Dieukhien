using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CFTWinAppCore.DeviceManager;
using CFTWinAppCore.Sequences;
using CFTWinAppCore.Task;
using CFTWinAppCore.Helper;
using LogLibrary;
using VHCRTestTask.Tasks;

using MathNet.Numerics.Interpolation;
using CFTWinAppCore.DataLog;
using GeneralTool;
using CFTWinAppCore.DeviceManager.PowerDevice;
using System.Windows.Forms;
using CFTWinAppCore.DeviceManager.DUTIOService;
using Krypton.Toolkit;
using VHCRTestTask.Tasks.VHCR;
using CFTWinAppCore.DeviceManager.DUT;

namespace VHCRTestTask
{
    public enum RFPowerGetResultMode
    {
        Peak,
        RMS,
        Averages
    }

    public enum GainNumber
    {
        Gain_1,
        Gain_2,
        Gain_3
    }
    public enum Type // tương tự, số
    {
        Analog,
        Digital
    }
    public enum CircuitType
    {
        EFilter,
        Tunable
    }

    public enum BandFilter
    {
        B_1_5to2_2,
        B_2_2to3_2,
        B_3_2to4_6,
        B_4_6to6_7,
        B_6_7to9_8,
        B_9_8to14_2,
        B14_2to20_6,
        B20_6to30
    }

    public enum BandwidthAtten
    {
        _3dB,
        _30dB
    }
    public class clsTaskHelper
    {
        #region Task Helper function
        public static double GetRxSensitivity(ISequenceManager seq, double duFreq, VHCR_Mod strDutMode)
        {
            try
            {
                List<ITask> lstTask = seq.Tasks;
                clsVHCR_Rx_Sensitivity sensiTask = null;
                double result = double.MinValue;
                for (int i = 0; i < lstTask.Count; i++)
                {
                    sensiTask = lstTask[i] as clsVHCR_Rx_Sensitivity;
                    if (sensiTask == null) continue;
                    if (sensiTask.DutFreqRx == duFreq && sensiTask.DutMode == strDutMode)
                    {
                        break;
                    }
                }
                if (sensiTask == null || sensiTask.m_duDisplayValue == double.MinValue || sensiTask.m_duDisplayValue == double.MaxValue) return double.MaxValue;
                result = sensiTask.m_duDisplayValue;
                return result;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("GetRxSensitivity() catched an exception: " + ec.Message);
                return double.MaxValue;
            }
        }

        public void CheckFunc(bool FuncResult, string Func)
        {
            if(FuncResult)
            {
                clsLogManager.LogReport($"{Func} PASS");
            }
            else
            {
                clsLogManager.LogReport($"{Func} FAIL");
            }
        }
        //public static double Get_FreqErr_2023(ISequenceManager seq)
        //{
        //    try
        //    {
        //        List<ITask> lstTask = seq.Tasks;
        //        clsRxSetupWBHF sensiTask = null;
        //        for (int i = 0; i < lstTask.Count; i++)
        //        {
        //            sensiTask = lstTask[i] as clsRxSetupWBHF;
        //            if (sensiTask == null)
        //            {
        //                continue;
        //            }
        //            else
        //                break;
        //        }
        //        if (sensiTask == null) return 0;
        //        return sensiTask.FreOffset_2023;
        //    }
        //    catch (Exception ec)
        //    {
        //        clsLogManager.LogError("Get_FreqErr_2023() catched an exception: " + ec.Message);
        //        return double.MinValue;
        //    }
        //}
        //public static double Get_712TxCurrent(ISequenceManager seq, double Freq, int DUTpower)
        //{
        //    try
        //    {
        //        List<ITask> lstTask = seq.Tasks;
        //        cls712_TxPower sensiTask = null;
        //        for (int i = 0; i < lstTask.Count; i++)
        //        {
        //            sensiTask = lstTask[i] as cls712_TxPower;
        //            if (sensiTask == null) continue;
        //            if ((sensiTask.Dutpower == DUTpower) && (sensiTask.DUTFreq == Freq))
        //                break;
        //        }
        //        if (sensiTask == null) return 0;
        //        return sensiTask.InputCurrent;
        //    }
        //    catch (Exception ec)
        //    {
        //        clsLogManager.LogError("Get_712TxCurrent() catched an exception: " + ec.Message);
        //        return double.MinValue;
        //    }
        //}
        //public static double Get_FreqErr_DaoTan(ISequenceManager seq, double Freq, DUTOperaMode mode)
        //{
        //    try
        //    {
        //        List<ITask> lstTask = seq.Tasks;
        //        cls712_TxFreqDev_DaoTan sensiTask = null;
        //        for (int i = 0; i < lstTask.Count; i++)
        //        {
        //            sensiTask = lstTask[i] as cls712_TxFreqDev_DaoTan;
        //            if (sensiTask == null) continue;
        //            if (sensiTask.DUTFreq == Freq && sensiTask.DutMode == mode)
        //                break;
        //        }
        //        if (sensiTask == null) return 0;
        //        return sensiTask.FreqErr_DaoTan;
        //    }
        //    catch (Exception ec)
        //    {
        //        clsLogManager.LogError("Get_FreqErr_DaoTan() catched an exception: " + ec.Message);
        //        return double.MinValue;
        //    }
        //}

        public static double GetPowerLevel(IRFPowerDevice IOService, int nMesCnt = 5, RFPowerGetResultMode resultMode = RFPowerGetResultMode.Peak, PowerUnit powerUnit = PowerUnit.Wat)
        {
            bool PowerFail = false;
            double duOuput = 0;
            double[] arrResult = new double[nMesCnt];
            for (int i = 0; i < arrResult.Length; i++)
            {
                arrResult[i] = IOService.GetPowerValues(powerUnit);
                if (arrResult[i] < 0)
                {
                    PowerFail = true;
                }
                Thread.Sleep(200);
            }
            switch (resultMode)
            {
                case RFPowerGetResultMode.Averages:
                    duOuput = arrResult.Averages();
                    break;
                case RFPowerGetResultMode.Peak:
                    duOuput = arrResult.Peak();
                    break;
                case RFPowerGetResultMode.RMS:
                    if (!PowerFail)
                    {
                        duOuput = arrResult.RootMeanSquare();
                    }
                    else
                    {
                        duOuput = -1 * arrResult.RootMeanSquare();
                    }
                    break;
            }
            return duOuput;
        }
        public static void WaitRFPowerReduce(IRFPowerDevice RfPowerDev, double duReturnLevel = 1)
        {
            double duCurrRFPwr = RfPowerDev.GetPowerValues(PowerUnit.Wat);
            if (duCurrRFPwr < duReturnLevel) return;
            int nBeginTime = Environment.TickCount;
            int nTimeOut = 0;
            while (true)
            {
                Thread.Sleep(100);
                duCurrRFPwr = RfPowerDev.GetPowerValues(PowerUnit.Wat);
                if (duCurrRFPwr < duReturnLevel) break;
                nTimeOut = Environment.TickCount - nBeginTime;
                if (nTimeOut >= 5000)
                    break;
            }
        }
        public static double WaitRFPowerStable(IRFPowerDevice RfPowerDev, int nDelayEachLoop, int nWaitTimeOut = 10000, double duDeltaPwr = 0.5, int nMaxPassCnt = 3, PowerUnit powerUnit = PowerUnit.Wat)
        {
            double duPrevRFPwr = RfPowerDev.GetPowerValues(powerUnit);
            double duCurrRFPwr = 0;
            int nPassCnt = 0;
            int nBeginTime = Environment.TickCount;
            int nTimeOut;
            double duRealDeltaPower = 0;
            while (true)
            {
                if (nDelayEachLoop > 0)
                {
                    Thread.Sleep(nDelayEachLoop);
                }
                duCurrRFPwr = RfPowerDev.GetPowerValues(powerUnit);
                clsLogManager.LogReport("{0, 25}{1, 25}", "--", duCurrRFPwr.ToString("F04"));
                if (powerUnit == PowerUnit.dBm)
                {
                    if (Math.Abs(duCurrRFPwr - duPrevRFPwr) <= duDeltaPwr)
                    {
                        nPassCnt++;
                        if (nPassCnt >= nMaxPassCnt)
                            break;
                    }
                }
                else
                {
                    if (Math.Abs(clsRFPowerUnitHelper.Wat2Dbm(duCurrRFPwr) - clsRFPowerUnitHelper.Wat2Dbm(duPrevRFPwr)) <= duDeltaPwr)
                        nPassCnt++;
                    if (nPassCnt >= nMaxPassCnt)
                        break;
                }
                duPrevRFPwr = duCurrRFPwr;
                nTimeOut = Environment.TickCount - nBeginTime;
                if (nTimeOut > nWaitTimeOut)
                {
                    clsLogManager.LogError("Time out for waiting the rf power stable");
                    break;
                }
            }
            return duCurrRFPwr;
        }        
        //public static double SearchRFFrequencyLinear(ISignalGenDevice SigGen, IAudioAnaDevice AudioDev, double duMinAudio, double duMaxAudio, double nBeginRFFreq, double DutFreq,
        //    double nFactor, ITraceData TraceService, int nMesCnt = 5, bool bIsUpSearch = true, AudioGenIndex genIndex = AudioGenIndex.GEN_1, int nDelayAfterSetAudio = 200, int nSearchTimeout = 10000)
        //{
        //    double CurrRFFreq = nBeginRFFreq;
        //    double duCurrAudioLevel = double.MinValue;
        //    double duSweepFactor = nFactor;
        //    int nBeginTime = Environment.TickCount;
        //    int nTimeOut;
        //    bool bSwitch2Linear = false;
        //    bool IsInterValueExit = false;
        //    double duWorkingFreq = 0;
        //    double duSmoothFactor = ((duSweepFactor * 100) / 2) / 1e6;
        //    int nInterCnt = 0;
        //    Dictionary<double, double> knownSamples = new Dictionary<double, double>();
        //    clsLogManager.LogReport("Target range is {0, 10}{1, 25}", duMinAudio, duMaxAudio);
        //    //Audio level is key, RF power is value
        //    if (bIsUpSearch)
        //        duWorkingFreq = DutFreq + CurrRFFreq;
        //    else
        //        duWorkingFreq = DutFreq - CurrRFFreq;
        //    SigGen.SetRFFrequency(duWorkingFreq, FreqUnit.MHz);
        //    if (nDelayAfterSetAudio > 0)
        //    {
        //        Thread.Sleep(nDelayAfterSetAudio);
        //    }
        //    duCurrAudioLevel = AudioDev.GetAudioPowerLevel(AudioLevelUnit.MV, nMesCnt, true);
        //    //knownSamples.Add(duWorkingFreq, duCurrAudioLevel);
        //    knownSamples.Add(CurrRFFreq, duCurrAudioLevel);
        //    TraceService.WriteLine("{0, 25}{1, 25}{2, 25}", CurrRFFreq.ToString("F06"), duCurrAudioLevel.ToString("F04"),
        //                            duWorkingFreq);
        //    while ((duCurrAudioLevel < duMinAudio) || (duCurrAudioLevel > duMaxAudio))
        //    {
        //        if (!bSwitch2Linear)
        //        {
        //            if (duCurrAudioLevel > duMaxAudio)
        //            {
        //                CurrRFFreq = CurrRFFreq * duSweepFactor;
        //                CurrRFFreq = Math.Round(CurrRFFreq, 6);
        //            }
        //            else if (duCurrAudioLevel < duMinAudio)
        //            {
        //                CurrRFFreq = CurrRFFreq / duSweepFactor;
        //                CurrRFFreq = Math.Round(CurrRFFreq, 6);
        //            }
        //            if (knownSamples.ContainsKey(CurrRFFreq/* +DutFreq*/))
        //            {
        //                bSwitch2Linear = true;
        //                continue;
        //            }
        //        }
        //        else
        //        {
        //            if (!IsInterValueExit)
        //            {
        //                if ((nInterCnt < 1))
        //                {
        //                    //CurrRFFreq = LinearSpline.InterpolateInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate((duMinAudio + duMaxAudio) / 2);
        //                    //clsLogManager.LogReport("LinearSpline: x={0}=>y={1}", (duMinAudio + duMaxAudio) / 2, CurrRFFreq);
        //                    //
        //                    //if (knownSamples.Count < 5)
        //                    //{
        //                    CurrRFFreq = MathNet.Numerics.Interpolate.Linear(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate((duMinAudio + duMaxAudio) / 2);
        //                    TraceService.WriteLine("Linear: x={0, 15}=>y={1, 21}", (duMinAudio + duMaxAudio) / 2, CurrRFFreq.ToString("F06"));
        //                    //}
        //                    //else
        //                    //{
        //                    //  CurrRFFreq = CubicSpline.InterpolateAkimaInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate((duMinAudio + duMaxAudio) / 2);
        //                    //  TraceService.WriteLine("Akima: x={0, 15}=>y={1, 21}", (duMinAudio + duMaxAudio) / 2, CurrRFFreq);
        //                    //}
        //                    nInterCnt++;
        //                    CurrRFFreq = Math.Round(CurrRFFreq, 6);
        //                }
        //                else
        //                {
        //                    //
        //                    //bContUSeInter = false;
        //                    if (duCurrAudioLevel < duMinAudio)
        //                    {
        //                        CurrRFFreq = Math.Round((CurrRFFreq - duSmoothFactor), 6);
        //                    }
        //                    else
        //                    {
        //                        CurrRFFreq = Math.Round((CurrRFFreq + duSmoothFactor), 6);
        //                    }
        //                    if (knownSamples.ContainsKey(CurrRFFreq/* +DutFreq*/))
        //                    {
        //                        nInterCnt = 0;
        //                        IsInterValueExit = false;
        //                        duSmoothFactor -= 10 / 1e6;
        //                        if (duSmoothFactor < 5 / 1e6)
        //                            duSmoothFactor = 5 / 1e6;
        //                        continue;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (duCurrAudioLevel < duMinAudio)
        //                {
        //                    CurrRFFreq = Math.Round((CurrRFFreq - duSmoothFactor), 6)/* + DutFreq*/;
        //                }
        //                else
        //                {
        //                    CurrRFFreq = Math.Round((CurrRFFreq + duSmoothFactor), 6)/* + DutFreq*/;
        //                }
        //            }
        //            //CurrRFFreq = Math.Abs(CurrRFFreq - DutFreq);
        //        }
        //        //
        //        if (bIsUpSearch)
        //            duWorkingFreq = DutFreq + CurrRFFreq;
        //        else
        //            duWorkingFreq = DutFreq - CurrRFFreq;
        //        SigGen.SetRFFrequency(duWorkingFreq, FreqUnit.MHz);
        //        if (nDelayAfterSetAudio > 0)
        //        {
        //            Thread.Sleep(nDelayAfterSetAudio);
        //        }
        //        duCurrAudioLevel = AudioDev.GetAudioPowerLevel(AudioLevelUnit.MV, nMesCnt);
        //        if (knownSamples.ContainsKey(/*duWorkingFreq*/CurrRFFreq))
        //        {
        //            knownSamples.Remove(/*duWorkingFreq*/CurrRFFreq);
        //            knownSamples.Add(CurrRFFreq/*duWorkingFreq*/, duCurrAudioLevel);
        //            IsInterValueExit = true;
        //        }
        //        else
        //        {
        //            knownSamples.Add(CurrRFFreq/*duWorkingFreq*/, duCurrAudioLevel);
        //            IsInterValueExit = false;
        //        }
        //        TraceService.WriteLine("{0, 25}{1, 25}{2, 25}", CurrRFFreq.ToString("F06"), duCurrAudioLevel.ToString("F04"),
        //                                duWorkingFreq);
        //        if ((duCurrAudioLevel >= duMinAudio) && (duCurrAudioLevel <= duMaxAudio))
        //            break;
        //        nTimeOut = Environment.TickCount - nBeginTime;
        //        if (nTimeOut >= nSearchTimeout) //Search 10 second
        //        {
        //            string strErrMsg = string.Format("Timeout for search subtable Rf frequency to make the audio level in range [{0}, {1}]", duMinAudio.ToString("F04"), duMaxAudio.ToString("F04"));
        //            TraceService.WriteLine(strErrMsg);
        //            // 
        //            double duAcceptDistance = ((duMinAudio + duMaxAudio) / 2) - duMinAudio;
        //            double duDistance = 0;
        //            double duPrevDistance = double.MaxValue;
        //            int nBestIndex = -1;
        //            for (int i = 0; i < knownSamples.Count; i++)
        //            {
        //                duDistance = Math.Abs(knownSamples.Values.ElementAt(i) - duMinAudio);
        //                if (duDistance <= duAcceptDistance)
        //                {
        //                    if (duDistance < duPrevDistance)
        //                    {
        //                        nBestIndex = i;
        //                        duPrevDistance = duDistance;
        //                    }
        //                }
        //            }
        //            if (nBestIndex != -1)
        //            {
        //                CurrRFFreq = Math.Abs(knownSamples.Keys.ElementAt(nBestIndex)/* - DutFreq*/);
        //            }
        //            else
        //            {
        //                duDistance = double.MaxValue;
        //                double duCurrDistance = 0;
        //                if (nBestIndex == -1)
        //                {
        //                    for (int i = 0; i < knownSamples.Count; i++)
        //                    {
        //                        duCurrDistance = Math.Abs(knownSamples.Values.ElementAt(i) - duMaxAudio);
        //                        if (duCurrDistance < duDistance)
        //                        {
        //                            duDistance = duCurrDistance;
        //                            nBestIndex = i;
        //                        }
        //                    }
        //                    CurrRFFreq = Math.Abs(knownSamples.Keys.ElementAt(nBestIndex)/* - DutFreq*/);
        //                }
        //            }
        //            TraceService.WriteLine("Choose freq offset {0} to make audio level {1}", CurrRFFreq.ToString("F06"),
        //                                       knownSamples.Values.ElementAt(nBestIndex));
        //            break;
        //        }
        //    }
        //    return CurrRFFreq;
        //}
        //public static double SearchAudioLevelLinear(IRFPowerDevice RfPowerDev, IAudioAnaDevice AudioDev, double duMinPwr, double duMaxPwr, double nBeginAudioLevel,
        //    double nFactor, ITraceData TraceService, ref double duDesTargetPwr, int nMesCnt = 5, AudioGenIndex genIndex = AudioGenIndex.GEN_1, int nDelayAfterSetAudio = 200, int nSearchTimeout = 10000, PowerUnit powerUnit = PowerUnit.Wat, double stepscanAulevel = 1.3)
        //{
        //    double Average1 = double.MinValue;
        //    double CurrAuLevel = nBeginAudioLevel;
        //    double duCurrRFPwr = double.MinValue;
        //    double duSweepFactor = nFactor;
        //    int nBeginTime = Environment.TickCount;
        //    int nTimeOut;
        //    bool bSwitch2Linear = false;
        //    int nInterCnt = 0;
        //    Dictionary<double, double> knownSamples = new Dictionary<double, double>();
        //    //Audio level is key, RF power is value
        //    try
        //    {
        //        duCurrRFPwr = clsTaskHelper.GetPowerLevel(RfPowerDev, nMesCnt, RFPowerGetResultMode.RMS, powerUnit);
        //        knownSamples.Add(nBeginAudioLevel, duCurrRFPwr);
        //        duDesTargetPwr = duCurrRFPwr;
        //        while ((duCurrRFPwr < duMinPwr) || (duCurrRFPwr > duMaxPwr))
        //        {
        //            if (!bSwitch2Linear)
        //            {
        //                if (duCurrRFPwr > duMaxPwr)
        //                {
        //                    CurrAuLevel = CurrAuLevel / duSweepFactor;
        //                }
        //                else if (duCurrRFPwr < duMinPwr)
        //                {
        //                    CurrAuLevel = CurrAuLevel * duSweepFactor;
        //                }
        //                if (knownSamples.ContainsKey(CurrAuLevel))
        //                {
        //                    bSwitch2Linear = true;
        //                    continue;
        //                }
        //            }
        //            else
        //            {
        //                if (nInterCnt < 3)
        //                {
        //                    CurrAuLevel = LinearSpline.InterpolateInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate((duMinPwr + duMaxPwr) / 2);
        //                    TraceService.WriteLine("LinearSpline: x={0}=>y={1}", (duMinPwr + duMaxPwr) / 2, CurrAuLevel);
        //                    nInterCnt++;
        //                }
        //                if ((knownSamples.ContainsKey(CurrAuLevel)) || (nInterCnt >= 3)) // Nếu trùng giá trị level đã test trước đó hoăc nội suy 3 lần không được => Thay đổi gía trị âm tần
        //                {
        //                    if (knownSamples.ContainsKey(CurrAuLevel / stepscanAulevel) && knownSamples.ContainsKey(CurrAuLevel * stepscanAulevel))
        //                    {
        //                        Average1 = (CurrAuLevel / stepscanAulevel + CurrAuLevel * stepscanAulevel) / 2;
        //                        //if (knownSamples.ContainsKey(Check_CurrAuLevel) && !knownSamples.ContainsKey(CurrAuLevel)) // Nếu sau khi tính trung bình mà vẫn ra kết quả trong list danh sách đã có -> tính với giá trị trung bình và tiếp tục nội suy
        //                        //{
        //                        //    clsLogManager.LogWarning("Check_CurrAuLevel: {0} has in used", Check_CurrAuLevel);
        //                        //    if (knownSamples[Check_CurrAuLevel] > duMaxPwr)
        //                        //    {
        //                        //        CurrAuLevel = Check_CurrAuLevel / stepscanAulevel;
        //                        //        clsLogManager.LogReport("Check_CurrAuLevel/stepscanAulevel");
        //                        //    }
        //                        //    else
        //                        //    {
        //                        //        CurrAuLevel = Check_CurrAuLevel * stepscanAulevel;
        //                        //        clsLogManager.LogReport("Check_CurrAuLevel*stepscanAulevel");
        //                        //    }
        //                        #region old version
        //                        //if ((knownSamples[Check_CurrAuLevel] > duMaxPwr && knownSamples[CurrAuLevel] < duMinPwr) || (knownSamples[CurrAuLevel] > duMaxPwr && knownSamples[Check_CurrAuLevel] < duMinPwr))
        //                        //{
        //                        //    CurrAuLevel = (CurrAuLevel + Check_CurrAuLevel) / 2;
        //                        //    clsLogManager.LogWarning("Avarage2: {0}", CurrAuLevel);
        //                        //}
        //                        //else
        //                        //{
        //                        //    if (knownSamples[Check_CurrAuLevel] > duMaxPwr)
        //                        //    {
        //                        //        CurrAuLevel = Check_CurrAuLevel - 0.025;
        //                        //        clsLogManager.LogReport("Check_CurrAuLevel - 0.025");
        //                        //    }
        //                        //    if (knownSamples[Check_CurrAuLevel] < duMinPwr)
        //                        //    {
        //                        //        CurrAuLevel = Check_CurrAuLevel + 0.025;
        //                        //        clsLogManager.LogReport("Check_CurrAuLevel + 0.025");
        //                        //    }
        //                        //}
        //                        #endregion
        //                        //}
        //                        //else
        //                        //{
        //                        clsLogManager.LogWarning("Average1 = {0}", Average1);
        //                        CurrAuLevel = Average1;
        //                        if (knownSamples.ContainsKey(CurrAuLevel)) // Nếu kết quả trong list danh sách đã có -> tiếp tục nội suy
        //                        {
        //                            clsLogManager.LogReport("Average has been in used! ---> reset nInterCnt");
        //                            nInterCnt = 0;
        //                        }
        //                        else
        //                        {
        //                            nInterCnt = 3;
        //                        }
        //                        //}
        //                    }
        //                    else
        //                    {
        //                        if (duCurrRFPwr > duMaxPwr)
        //                        {
        //                            CurrAuLevel = CurrAuLevel / stepscanAulevel;
        //                            clsLogManager.LogReport("CurrAuLevel/stepscanAulevel");
        //                        }
        //                        else
        //                        {
        //                            CurrAuLevel = CurrAuLevel * stepscanAulevel;
        //                            clsLogManager.LogReport("CurrAuLevel*stepscanAulevel");
        //                        }
        //                        if (knownSamples.ContainsKey(CurrAuLevel)) // Nếu kết quả trong list danh sách đã có -> tiếp tục nội suy
        //                        {
        //                            clsLogManager.LogReport("Reset nInterCnt");
        //                            nInterCnt = 0;
        //                        }
        //                        else
        //                        {
        //                            nInterCnt = 3;
        //                        }
        //                    }
        //                }
        //            }
        //            AudioDev.SetAudioLevel(CurrAuLevel, genIndex, AudioLevelUnit.MV);
        //            if (nDelayAfterSetAudio > 0)
        //            {
        //                Thread.Sleep(nDelayAfterSetAudio);
        //            }
        //            duCurrRFPwr = clsTaskHelper.GetPowerLevel(RfPowerDev, nMesCnt, RFPowerGetResultMode.RMS, powerUnit);
        //            if (knownSamples.ContainsKey(CurrAuLevel))
        //            {
        //                knownSamples.Remove(CurrAuLevel);
        //                knownSamples.Add(CurrAuLevel, duCurrRFPwr);
        //            }
        //            else
        //                knownSamples.Add(CurrAuLevel, duCurrRFPwr);
        //            TraceService.WriteLine("{0, 25}{1, 25}", CurrAuLevel.ToString("F04"), duCurrRFPwr.ToString("F04"));
        //            duDesTargetPwr = duCurrRFPwr;
        //            if ((duCurrRFPwr >= duMinPwr) && (duCurrRFPwr <= duMaxPwr))
        //                break;
        //            nTimeOut = Environment.TickCount - nBeginTime;
        //            if (nTimeOut >= nSearchTimeout) //Search 10 second
        //            {
        //                TraceService.WriteLine("Timeout for search subtable audio level to make the RF power in range [{0}, {1}]", duMinPwr.ToString("F04"), duMaxPwr.ToString("F04"));
        //                double duCurrDistance = 0;
        //                double duPrevDistance = double.MaxValue;
        //                int nBestIndex = -1;
        //                double duAcceptDistance = 1.5 * ((duMinPwr + duMaxPwr) / 2 - duMinPwr);
        //                for (int i = 0; i < knownSamples.Count; i++)
        //                {
        //                    duCurrDistance = Math.Abs(knownSamples.Values.ElementAt(i) - ((duMinPwr + duMaxPwr) / 2));
        //                    if ((duCurrDistance < duPrevDistance) && (duCurrDistance <= duAcceptDistance))
        //                    {
        //                        nBestIndex = i;
        //                        duPrevDistance = duCurrDistance;
        //                        duDesTargetPwr = knownSamples.Values.ElementAt(i);
        //                    }
        //                }
        //                if (nBestIndex != -1)
        //                {
        //                    CurrAuLevel = knownSamples.Keys.ElementAt(nBestIndex);
        //                    duDesTargetPwr = knownSamples.Values.ElementAt(nBestIndex);
        //                    TraceService.WriteLine("Choose audio level is {0} make RF power is {1}", CurrAuLevel,
        //                                          knownSamples.Values.ElementAt(nBestIndex));
        //                    AudioDev.SetAudioLevel(CurrAuLevel, genIndex, AudioLevelUnit.MV);
        //                    break;
        //                }
        //                else
        //                    return double.MinValue;
        //            }
        //        }
        //        return CurrAuLevel;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        clsLogManager.LogError("SearchAudioLevelLinear: {0}", ex.ToString());
        //        return double.MinValue;
        //    }
        //}
        public static double SearchRFLevelLinear(ISignalGenDevice SigGenDev, IAudioAnaDevice AudioDev, double duMinSINAD, double duMaxSINAD, double nBeginRFPwr,
            double nFactor, ITraceData TraceService, double AdjustRFlevel, bool AcceptMaxLevel, out double SINADValue, int nMesCnt = 5, int nDelayAfterSetAudio = 200, int nSearchTimeout = 10000, PowerUnit powerUnit = PowerUnit.uV, double Attenuator = 0, SignalPort port = SignalPort.InOut)
        {
            try
            {
                double CurrRfLevel = nBeginRFPwr; // Mức RF set up ban đầu
                double duSINADLevel = double.MinValue;
                double duSweepFactor = nFactor; // Hệ số thay đổi RF level (nhân hoặc chia CurrRfLevel)
                int nBeginTime = Environment.TickCount;
                int nTimeOut;
                bool bSwitch2Linear = false; // thực hiện tuyến tính + nội suy hay chưa
                Dictionary<double, double> knownSamples = new Dictionary<double, double>(); // Dictionary lưu danh sách RFlevel và Sinad tương ứng
                double duTargetValue = (duMaxSINAD + duMinSINAD) / 2; // Giá trị mong muốn điều chỉnh RFlevel để đạt đc 
                int nLnearCnt = 0;
                bool bIsInterValueExit = false; // Bằng true nếu giá trị RF vừa tuyến tính trung với giá trị đã khảo sát => không tuyến tính ở bước tiếp theo mà công thêm hoặc trừ đi RF level 1 lượng = AdjustRFlevel 
                double duMinLevelOfSinGen = SigGenDev.GetMinOutputLevel(port); // Công suất phát tối thiểu - dBm 
                double RFLevelMax = SigGenDev.GetMaxOutputLevel(port);  // Công suất phát tối đa - dBm      
                bool bReachMin = false; // đã từng giảm xuống mức min của máy phát
                bool bReachMax = false; // đã từng tăng mức lên mức max của máy phát
                switch (powerUnit)
                {
                    case PowerUnit.mV:
                        duMinLevelOfSinGen = clsRFPowerUnitHelper.Dbm2Vol(duMinLevelOfSinGen) * 1e3;
                        RFLevelMax = clsRFPowerUnitHelper.Dbm2Vol(RFLevelMax) * 1e3;
                        break;
                    case PowerUnit.Wat:
                        duMinLevelOfSinGen = clsRFPowerUnitHelper.Dbm2Wat(duMinLevelOfSinGen);
                        RFLevelMax = clsRFPowerUnitHelper.Dbm2Wat(RFLevelMax);
                        break;
                    case PowerUnit.uV:
                        duMinLevelOfSinGen = clsRFPowerUnitHelper.Dbm2Vol(duMinLevelOfSinGen) * 1e6;
                        RFLevelMax = clsRFPowerUnitHelper.Dbm2Vol(RFLevelMax) * 1e6;
                        break;
                }
                //
                TraceService.WriteLine("{0, 25}{1, 25}{2, 25}", "RF level (uV)", "SINAD (dB)", "Attenuator (dB)");
                SigGenDev.SetOutputPower(CurrRfLevel, powerUnit); // Set công suất đầu ra theo mức RF hiện tại
                if (nDelayAfterSetAudio > 0)
                    Thread.Sleep(nDelayAfterSetAudio);
                //SINAD is key, RF power is value
                duSINADLevel = AudioDev.GetAudioSinadLevel(nMesCnt); // Get SINAD
                SINADValue = duSINADLevel;
                knownSamples.Add(CurrRfLevel, duSINADLevel);
                TraceService.WriteLine("{0, 25}{1, 25}{2, 25}", CurrRfLevel.ToString("F04"), duSINADLevel.ToString("F04"), Attenuator.ToString());
                //
                while ((duSINADLevel < duMinSINAD) || (duSINADLevel > duMaxSINAD)) //
                {
                    if (!bSwitch2Linear) // Khảo sát giá trị
                    {
                        if (duSINADLevel > duMaxSINAD)
                        {
                            CurrRfLevel /= duSweepFactor;
                        }
                        else if (duSINADLevel < duMinSINAD)
                        {
                            CurrRfLevel *= duSweepFactor;
                        }
                        if (knownSamples.ContainsKey(CurrRfLevel)) // Bắt đầu tuyến tính hóa nếu lặp lại giá trị set up RF level 
                        {
                            bSwitch2Linear = true;
                            continue;
                        }
                    }
                    else // Tuyến tính hóa tập giá trị
                    {
                        //if(nLnearCnt >=3)
                        //	duTargetValue = duMinAudioLevel;
                        if (!bIsInterValueExit)
                        {
                            if (nLnearCnt < 1) // nLnearCnt  reset khi giá trị RFlevel điều chỉnh ra bằng giá trị đã khảo sát
                            {
                                if (knownSamples.Count < 5)
                                {
                                    // Nội suy
                                    CurrRfLevel = LogLinear.InterpolateInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate(duTargetValue);
                                    TraceService.WriteLine("LogLinear: x={0}=>y={1}", duTargetValue, CurrRfLevel);
                                }
                                else
                                {
                                    //CurrRfLevel = CubicSpline.InterpolateAkimaInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate(duTargetValue);
                                    CurrRfLevel = LogLinear.InterpolateInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate(duTargetValue);
                                    TraceService.WriteLine("LogLinear: x={0}=>y={1}", duTargetValue, CurrRfLevel);
                                }
                                nLnearCnt++;
                            }
                            else
                            {
                                if (duSINADLevel < duMinSINAD)
                                    CurrRfLevel += AdjustRFlevel;
                                else
                                    CurrRfLevel -= AdjustRFlevel;
                                if (knownSamples.ContainsKey(CurrRfLevel))
                                {
                                    nLnearCnt = 0;
                                    continue; // tiếp tục nội suy
                                }
                            }
                        }
                        else
                        {
                            if (duSINADLevel < duMinSINAD)
                                CurrRfLevel = CurrRfLevel + AdjustRFlevel;
                            else
                                CurrRfLevel = CurrRfLevel - AdjustRFlevel;
                        }
                        //nLnearCnt++;
                    }
                    if (CurrRfLevel <= duMinLevelOfSinGen) // Nếu giảm mức RF đến mức nhỏ nhất của máy phát
                    {
                        clsLogManager.LogWarning("Minimum level of sigen device is reaching -> use min level is {0}", duMinLevelOfSinGen.ToString("F03"));
                        if (!bReachMin)
                        {
                            CurrRfLevel = duMinLevelOfSinGen;
                            bReachMin = true;
                        }
                        else // Nếu lần thứ 2 RF level được điều chỉnh <=  duMinLevelOfSinGen
                        {
                            //if (check_Dic(knownSamples, duMaxSINAD, true)) // check có giá trị nào lớn hơn max sinad hay ko?
                            //{
                            //    CurrRfLevel = duMinLevelOfSinGen;
                            //    SINADValue = duSINADLevel;
                            //}
                            //else
                            //{
                            CurrRfLevel = double.MaxValue;
                            SINADValue = double.MaxValue;
                            //}
                            break;
                        }
                    }
                    if (CurrRfLevel >= RFLevelMax) // Nếu giảm mức RF đến mức lớn nhất của máy phát
                    {
                        clsLogManager.LogWarning("Maximum level of sigen device is reaching -> use max level is {0}", RFLevelMax.ToString("F03"));
                        if (!bReachMax)
                        {
                            CurrRfLevel = RFLevelMax;
                            bReachMax = true;
                        }
                        else // Nếu lần thứ 2 RF level được điều chỉnh >=  RFLevelMax
                        {
                            //if (check_Dic(knownSamples, duMaxSINAD, false))
                            //{
                            //    CurrRfLevel = RFLevelMax;
                            //    SINADValue = duSINADLevel;
                            //}
                            //else
                            //{
                            //    CurrRfLevel = double.MaxValue;
                            //    SINADValue = double.MaxValue;
                            //}
                            if (AcceptMaxLevel)
                            {
                                CurrRfLevel = RFLevelMax;
                                SINADValue = duSINADLevel;
                            }
                            else
                            {
                                CurrRfLevel = double.MaxValue;
                                SINADValue = double.MaxValue;
                            }
                            break;
                        }
                    }
                    SigGenDev.SetOutputPower(CurrRfLevel, powerUnit);
                    if (nDelayAfterSetAudio > 0)
                    {
                        Thread.Sleep(nDelayAfterSetAudio);
                    }
                    duSINADLevel = AudioDev.GetAudioSinadLevel(nMesCnt);
                    if (knownSamples.ContainsKey(CurrRfLevel))
                    {
                        knownSamples.Remove(CurrRfLevel);
                        knownSamples.Add(CurrRfLevel, duSINADLevel);
                        bIsInterValueExit = true; // Nếu trùng
                    }
                    else
                    {
                        knownSamples.Add(CurrRfLevel, duSINADLevel);
                        bIsInterValueExit = false;
                    }
                    TraceService.WriteLine("{0, 25}{1, 25}{2, 25}", CurrRfLevel.ToString("F04"), duSINADLevel.ToString("F04"), Attenuator.ToString());
                    if ((duSINADLevel >= duMinSINAD) && (duSINADLevel <= duMaxSINAD))
                    {
                        clsLogManager.LogWarning("Search RF level: done!");
                        SINADValue = duSINADLevel;
                        break;
                    }
                    if ((CurrRfLevel == duMinLevelOfSinGen) && (duSINADLevel > duMaxSINAD))
                    {
                        clsLogManager.LogError("CurrRfLevel = minimum level of sigen device >> SINAD = {0} > {1}", duSINADLevel, duMaxSINAD);
                        CurrRfLevel = double.MaxValue;
                        SINADValue = double.MaxValue;
                        break;
                    }
                    if ((CurrRfLevel == RFLevelMax) && (duSINADLevel < duMinSINAD))
                    {
                        clsLogManager.LogWarning("CurrRfLevel = maximum level of sigen device >> SINAD = {0} < {1}", duSINADLevel, duMinSINAD);
                        if (!AcceptMaxLevel)
                        {
                            CurrRfLevel = double.MaxValue;
                            SINADValue = double.MaxValue;
                        }
                        break;
                    }
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout) //Search 10 second
                    {
                        if (AcceptMaxLevel)
                        {
                            continue;
                        }
                        else
                        {
                            TraceService.WriteLine("Timeout for search subtable Rf level to make the SINAD in range [{0}, {1}]", duMinSINAD.ToString("F04"), duMaxSINAD.ToString("F04"));
                            CurrRfLevel = double.MinValue;
                            SINADValue = double.MinValue;
                            break;
                        }
                        //double duAcceptDistance = ((duMinSINAD + duMaxSINAD) / 2) - duMinSINAD;
                        //clsLogManager.LogReport("duAcceptDistance = {0}", duAcceptDistance);
                        //double duPrevDistance = double.MaxValue;
                        //double duCurrDistance = 0;
                        //int nBestIndex = -1;
                        //for (int i = 0; i < knownSamples.Count; i++) // Tìm kiếm giá trị SINAD có độ lệch cho với duMinSINAD nhỏ nhất và thỏa mãn nhỏ hơn duAcceptDistance
                        //{
                        //    duCurrDistance = Math.Abs(knownSamples.Values.ElementAt(i) - duMinSINAD);
                        //    if (duCurrDistance <= duAcceptDistance)
                        //    {
                        //        if (duCurrDistance < duPrevDistance)
                        //        {
                        //            nBestIndex = i;
                        //            duPrevDistance = duCurrDistance;
                        //        }
                        //    }
                        //}
                        //if (nBestIndex != -1)
                        //{
                        //    CurrRfLevel = knownSamples.Keys.ElementAt(nBestIndex);
                        //}
                        //else
                        //{
                        //    CurrRfLevel = nBeginRFPwr * 2;
                        //    nBestIndex = -1;
                        //    duPrevDistance = double.MaxValue;
                        //    duCurrDistance = 0;
                        //    for (int i = 0; i < knownSamples.Count; i++) // Tìm kiếm giá trị SINAD có độ lệch cho với duMaxSINAD nhỏ nhất và thỏa mãn nhỏ hơn duAcceptDistance
                        //    {
                        //        duCurrDistance = Math.Abs(knownSamples.Values.ElementAt(i) - duMaxSINAD);
                        //        if (duCurrDistance < duPrevDistance)
                        //        {
                        //            CurrRfLevel = knownSamples.Keys.ElementAt(i);
                        //            nBestIndex = i;
                        //            duPrevDistance = duCurrDistance;
                        //        }
                        //    }
                        //    if (nBestIndex == -1)
                        //    {
                        //        CurrRfLevel = double.MaxValue;
                        //        clsLogManager.LogError("There is no RF level value reach to expected duAcceptDistance");
                        //        break;
                        //    }
                        //    else
                        //    {
                        //        CurrRfLevel = knownSamples.Keys.ElementAt(nBestIndex); // Tìm giá trị có độ lệch so với giá trị max nhỏ hơn độ lệch cho phép và là nhỏ nhất
                        //    }

                        //}
                        //TraceService.WriteLine("Choose RF level {0} make SINAD is {1}", CurrRfLevel, knownSamples.Values.ElementAt(nBestIndex));
                        //break;
                    }
                    Thread.Sleep(100);
                }
                return CurrRfLevel;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SearchRFLevelLinear() catched an exception: " + ec.Message);
                SINADValue = double.MaxValue;
                return double.MaxValue;
            }
        }

        private static bool check_Dic(Dictionary<double, double> knownSamples, double refvalue, bool CheckMax)
        {
            try
            {
                bool bCheck = false;
                foreach (double key in knownSamples.Keys)
                {
                    if (CheckMax) // Kiểm tra xem đã từng set RF level nào mà sinad đo được lớn hơn max sinad hay chưa
                    {
                        if (knownSamples[key] > refvalue)
                        {
                            bCheck = true;
                        }
                    }
                    else
                    {
                        if (knownSamples[key] < refvalue)
                        {
                            bCheck = true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("check_Dic() catched an exception: " + ec.Message);
                return false;
            }
        }

        public static double SearchRFLevelLinear_AllMes(IDevMeasurementMode allmes, ISignalGenDevice SigGenDev, IAudioAnaDevice AudioDev, double duMinSINAD, double duMaxSINAD, double nBeginRFPwr,
            double nFactor, ITraceData TraceService, double AdjustRFlevel, bool AcceptMaxLevel, out double SinadValue, int nMesCnt = 5, int nDelayAfterSetAudio = 200, int nSearchTimeout = 10000, PowerUnit powerUnit = PowerUnit.uV, double Attenuator = 0, SignalPort port = SignalPort.InOut)
        {
            try
            {
                double CurrRfLevel = nBeginRFPwr; // Mức RF set up ban đầu
                double duSINADLevel = double.MinValue;
                double duSweepFactor = nFactor; // Hệ số thay đổi RF level (nhân hoặc chia CurrRfLevel)
                int nBeginTime = Environment.TickCount;
                int nTimeOut;
                bool bSwitch2Linear = false; // thực hiện tuyến tính + nội suy hay chưa
                Dictionary<double, double> knownSamples = new Dictionary<double, double>(); // Dictionary lưu danh sách RFlevel và Sinad tương ứng
                double duTargetValue = (duMaxSINAD + duMinSINAD) / 2; // Giá trị mong muốn điều chỉnh RFlevel để đạt đc 
                int nLnearCnt = 0;
                bool bIsInterValueExit = false; // Bằng true nếu giá trị RF vừa tuyến tính trung với giá trị đã khảo sát => không tuyến tính ở bước tiếp theo mà công thêm hoặc trừ đi RF level 1 lượng = AdjustRFlevel 
                double duMinLevelOfSinGen = SigGenDev.GetMinOutputLevel(port); // Công suất phát tối thiểu - dBm 
                double RFLevelMax = SigGenDev.GetMaxOutputLevel(port);  // Công suất phát tối đa - dBm                                            
                switch (powerUnit)
                {
                    case PowerUnit.mV:
                        duMinLevelOfSinGen = clsRFPowerUnitHelper.Dbm2Vol(duMinLevelOfSinGen) * 1e3;
                        RFLevelMax = clsRFPowerUnitHelper.Dbm2Vol(RFLevelMax) * 1e3;
                        break;
                    case PowerUnit.Wat:
                        duMinLevelOfSinGen = clsRFPowerUnitHelper.Dbm2Wat(duMinLevelOfSinGen);
                        RFLevelMax = clsRFPowerUnitHelper.Dbm2Wat(RFLevelMax);
                        break;
                    case PowerUnit.uV:
                        duMinLevelOfSinGen = clsRFPowerUnitHelper.Dbm2Vol(duMinLevelOfSinGen) * 1e6;
                        RFLevelMax = clsRFPowerUnitHelper.Dbm2Vol(RFLevelMax) * 1e6;
                        break;
                }
                //
                TraceService.WriteLine("{0, 25}{1, 25}{2, 25}", "RF level (uV)", "SINAD (dB)", "Attenuator (dB)");
                SigGenDev.SetOutputPower(CurrRfLevel, powerUnit); // Set công suất đầu ra theo mức RF hiện tại
                if (allmes != null)
                {
                    allmes.init_Measurement(true);
                }
                else
                {
                    if (nDelayAfterSetAudio > 0)
                        Thread.Sleep(nDelayAfterSetAudio);
                }
                //SINAD is key, RF power is value
                duSINADLevel = AudioDev.GetAudioSinadLevel(nMesCnt); // Get SINAD
                SinadValue = duSINADLevel;
                knownSamples.Add(CurrRfLevel, duSINADLevel);
                TraceService.WriteLine("{0, 25}{1, 25}{2, 25}", CurrRfLevel.ToString("F04"), duSINADLevel.ToString("F04"), Attenuator.ToString());
                //
                while ((duSINADLevel < duMinSINAD) || (duSINADLevel > duMaxSINAD)) //
                {
                    if (!bSwitch2Linear) // Khảo sát giá trị
                    {
                        if (duSINADLevel > duMaxSINAD)
                        {
                            CurrRfLevel = CurrRfLevel / duSweepFactor;
                        }
                        else if (duSINADLevel < duMinSINAD)
                        {
                            CurrRfLevel = CurrRfLevel * duSweepFactor;
                        }
                        if (knownSamples.ContainsKey(CurrRfLevel)) // Bắt đầu tuyến tính hóa nếu lặp lại giá trị set up RF level 
                        {
                            bSwitch2Linear = true;
                            continue;
                        }
                    }
                    else // Tuyến tính hóa tập giá trị
                    {
                        //if(nLnearCnt >=3)
                        //	duTargetValue = duMinAudioLevel;
                        if (!bIsInterValueExit)
                        {
                            if (nLnearCnt < 1) // nLnearCnt  reset khi giá trị RFlevel điều chỉnh ra bằng giá trị đã khảo sát
                            {
                                if (knownSamples.Count < 5)
                                {
                                    // Nội suy
                                    CurrRfLevel = LogLinear.InterpolateInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate(duTargetValue);
                                    TraceService.WriteLine("LogLinear: x={0}=>y={1}", duTargetValue, CurrRfLevel);
                                }
                                else
                                {
                                    //CurrRfLevel = CubicSpline.InterpolateAkimaInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate(duTargetValue);
                                    CurrRfLevel = LogLinear.InterpolateInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate(duTargetValue);
                                    TraceService.WriteLine("LogLinear: x={0}=>y={1}", duTargetValue, CurrRfLevel);
                                }
                                nLnearCnt++;
                            }
                            else
                            {
                                if (duSINADLevel < duMinSINAD)
                                    CurrRfLevel = CurrRfLevel + AdjustRFlevel;
                                else
                                    CurrRfLevel = CurrRfLevel - AdjustRFlevel;
                                if (knownSamples.ContainsKey(CurrRfLevel))
                                {
                                    nLnearCnt = 0;
                                    continue; // tiếp tục nội suy
                                }
                            }
                        }
                        else
                        {
                            if (duSINADLevel < duMinSINAD)
                                CurrRfLevel = CurrRfLevel + AdjustRFlevel;
                            else
                                CurrRfLevel = CurrRfLevel - AdjustRFlevel;
                        }
                        //nLnearCnt++;
                    }
                    if (CurrRfLevel <= duMinLevelOfSinGen) // Nếu giảm mức RF đến mức nhỏ nhất của máy phát
                    {
                        clsLogManager.LogWarning("Minimum level of sigen device is reaching -> use min level is {0}", duMinLevelOfSinGen.ToString("F03"));
                        CurrRfLevel = duMinLevelOfSinGen;
                    }
                    if (CurrRfLevel >= RFLevelMax) // Nếu giảm mức RF đến mức lớn nhất của máy phát
                    {
                        clsLogManager.LogWarning("Maximum level of sigen device is reaching -> use max level is {0}", RFLevelMax.ToString("F03"));
                        CurrRfLevel = RFLevelMax;
                    }
                    SigGenDev.SetOutputPower(CurrRfLevel, powerUnit);
                    if (allmes != null)
                    {
                        allmes.init_Measurement(true);
                    }
                    else
                    {
                        if (nDelayAfterSetAudio > 0)
                            Thread.Sleep(nDelayAfterSetAudio);
                    }
                    duSINADLevel = AudioDev.GetAudioSinadLevel(nMesCnt);
                    if (knownSamples.ContainsKey(CurrRfLevel))
                    {
                        knownSamples.Remove(CurrRfLevel);
                        knownSamples.Add(CurrRfLevel, duSINADLevel);
                        bIsInterValueExit = true; // Nếu trùng
                    }
                    else
                    {
                        knownSamples.Add(CurrRfLevel, duSINADLevel);
                        bIsInterValueExit = false;
                    }
                    TraceService.WriteLine("{0, 25}{1, 25}{2, 25}", CurrRfLevel.ToString("F04"), duSINADLevel.ToString("F04"), Attenuator.ToString());
                    if ((duSINADLevel >= duMinSINAD) && (duSINADLevel <= duMaxSINAD))
                    {
                        clsLogManager.LogWarning("Search RF level: done!");
                        SinadValue = duSINADLevel;
                        break;
                    }
                    if ((CurrRfLevel == duMinLevelOfSinGen) && (duSINADLevel > duMaxSINAD))
                    {
                        clsLogManager.LogError("CurrRfLevel = minimum level of sigen device >> SINAD = {0} > {1}", duSINADLevel, duMaxSINAD);
                        CurrRfLevel = double.MaxValue;
                        SinadValue = double.MaxValue;
                        break;
                    }
                    if ((CurrRfLevel == RFLevelMax) && (duSINADLevel < duMinSINAD))
                    {
                        clsLogManager.LogWarning("CurrRfLevel = maximum level of sigen device >> SINAD = {0} < {1}", duSINADLevel, duMinSINAD);
                        if (!AcceptMaxLevel)
                        {
                            CurrRfLevel = double.MaxValue;
                            SinadValue = double.MaxValue;
                        }
                        break;
                    }
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout) //Search 10 second
                    {
                        if (AcceptMaxLevel)
                        {
                            continue;
                        }
                        else
                        {
                            TraceService.WriteLine("Timeout for search subtable Rf level to make the SINAD in range [{0}, {1}]", duMinSINAD.ToString("F04"), duMaxSINAD.ToString("F04"));
                            CurrRfLevel = double.MinValue;
                            SinadValue = double.MinValue;
                            break;
                        }
                        //double duAcceptDistance = ((duMinSINAD + duMaxSINAD) / 2) - duMinSINAD;
                        //clsLogManager.LogReport("duAcceptDistance = {0}", duAcceptDistance);
                        //double duPrevDistance = double.MaxValue;
                        //double duCurrDistance = 0;
                        //int nBestIndex = -1;
                        //for (int i = 0; i < knownSamples.Count; i++) // Tìm kiếm giá trị SINAD có độ lệch cho với duMinSINAD nhỏ nhất và thỏa mãn nhỏ hơn duAcceptDistance
                        //{
                        //    duCurrDistance = Math.Abs(knownSamples.Values.ElementAt(i) - duMinSINAD);
                        //    if (duCurrDistance <= duAcceptDistance)
                        //    {
                        //        if (duCurrDistance < duPrevDistance)
                        //        {
                        //            nBestIndex = i;
                        //            duPrevDistance = duCurrDistance;
                        //        }
                        //    }
                        //}
                        //if (nBestIndex != -1)
                        //{
                        //    CurrRfLevel = knownSamples.Keys.ElementAt(nBestIndex);
                        //}
                        //else
                        //{
                        //    CurrRfLevel = nBeginRFPwr * 2;
                        //    nBestIndex = -1;
                        //    duPrevDistance = double.MaxValue;
                        //    duCurrDistance = 0;
                        //    for (int i = 0; i < knownSamples.Count; i++) // Tìm kiếm giá trị SINAD có độ lệch cho với duMaxSINAD nhỏ nhất và thỏa mãn nhỏ hơn duAcceptDistance
                        //    {
                        //        duCurrDistance = Math.Abs(knownSamples.Values.ElementAt(i) - duMaxSINAD);
                        //        if (duCurrDistance < duPrevDistance)
                        //        {
                        //            CurrRfLevel = knownSamples.Keys.ElementAt(i);
                        //            nBestIndex = i;
                        //            duPrevDistance = duCurrDistance;
                        //        }
                        //    }
                        //    if (nBestIndex == -1)
                        //    {
                        //        CurrRfLevel = double.MaxValue;
                        //        clsLogManager.LogError("There is no RF level value reach to expected duAcceptDistance");
                        //        break;
                        //    }
                        //    else
                        //    {
                        //        CurrRfLevel = knownSamples.Keys.ElementAt(nBestIndex); // Tìm giá trị có độ lệch so với giá trị max nhỏ hơn độ lệch cho phép và là nhỏ nhất
                        //    }

                        //}
                        //TraceService.WriteLine("Choose RF level {0} make SINAD is {1}", CurrRfLevel, knownSamples.Values.ElementAt(nBestIndex));
                        //break;
                    }
                    Thread.Sleep(100);
                }
                return CurrRfLevel;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SearchRFLevelLinear_AllMes() catched an exception: " + ec.Message);
                SinadValue = double.MaxValue;
                return double.MaxValue;
            }
        }
        public static double SearchRFLevel(ISignalGenDevice SigGenDev, IAudioAnaDevice AudioDev, double duMinSINADLevel, double duMaxSINADLevel, double nBeginRFPwr,
            double attenuation/*dB*/, double MinSINAD, int TimeOut, bool AcceptMaxLevel, out double SinadValue, int nMesCnt = 5, int nDelayAfterSetAudio = 200, PowerUnit powerUnit = PowerUnit.uV)
        {
            try
            {
                double duCurrValue = nBeginRFPwr; // Giá trị công suất RF ban đầu
                double JumpFactor = duCurrValue / 2; // Bước tăng giảm: Giá trị công thêm hoặc trừ đi vào duCurrValue để kiểm tra SINAD
                bool Upbit = false;
                bool Downbit = false;
                double duSINAD = 0;
                Dictionary<double, double> KnowSamples = new Dictionary<double, double>();
                int nBeginTime = Environment.TickCount;
                int nTimeOut = 0;
                double duRFLevelMin = SigGenDev.GetMinOutputLevel();
                double RFLevelMax = SigGenDev.GetMaxOutputLevel();
                // convert attenuation dB to uV

                while (true)
                {
                    SigGenDev.SetOutputPower((duCurrValue + attenuation), powerUnit); // Set RF level
                    if (nDelayAfterSetAudio > 0)
                        Thread.Sleep(nDelayAfterSetAudio);
                    duSINAD = AudioDev.GetAudioSinadLevel(nMesCnt); //Get SINAD 
                    SinadValue = duSINAD;
                    clsLogManager.LogReport("{0, 25}{1, 25}", duCurrValue.ToString("F02"), duSINAD.ToString("F02"));
                    if (KnowSamples.ContainsKey(duCurrValue + attenuation)) // Nếu đã từng đo SINAD tại giá trị công suất vừa set -> bỏ giá trị cũ, cập nhật giá trị SINAD mới đo đc
                    {
                        KnowSamples.Remove(duCurrValue + attenuation);
                    }
                    KnowSamples.Add((duCurrValue + attenuation), duSINAD); // Cập nhật
                    if (duMinSINADLevel <= duCurrValue && duMaxSINADLevel >= duCurrValue) // Nếu SINAD đọc về nằm trong dải mong muốn
                    {
                        clsLogManager.LogWarning("Search RF level: done!");
                        break;
                    }
                    if (duSINAD > duMaxSINADLevel) //Nếu SINAD đọc về lớn hơn dải mong muốn
                    {
                        if (duCurrValue < duRFLevelMin) // Nếu giảm mức RF xuống tối thiểu mà SINAD vẫn lớn hơn dải mong muốn => Fail
                        {
                            clsLogManager.LogError("Noise is too large");
                            SinadValue = double.MinValue;
                            return double.MinValue;
                        }
                        if (Downbit)
                        {
                            Downbit = false;
                            JumpFactor = JumpFactor / 2;
                        }
                        if (JumpFactor <= 0.01) JumpFactor = 0.01;
                        duCurrValue = duCurrValue - JumpFactor;
                        //if (duCurrValue <= 0.07) duCurrValue = 0.075; // Đảm bảo duCurrValue không giảm xuống quá nhỏ
                        if (duCurrValue <= MinSINAD) duCurrValue = MinSINAD; // Đảm bảo duCurrValue không giảm xuống quá nhỏ
                        Upbit = true;
                    }
                    else
                    if (duSINAD < duMinSINADLevel) // Nếu SINAD đọc về nhỏ hơn giá trị mong muốn
                    {
                        if (duCurrValue == RFLevelMax - attenuation)
                        {
                            clsLogManager.LogWarning("CurrRfLevel = maximum level of sigen device >> SINAD = {0} < {1}", duSINAD, duMinSINADLevel);
                            if (!AcceptMaxLevel)
                            {
                                duCurrValue = double.MaxValue;
                                SinadValue = double.MaxValue;
                            }
                            break;
                        }
                        if (Upbit) // Nếu trước đo vừa giảm duCurrValue
                        {
                            Upbit = false;
                            JumpFactor = JumpFactor / 2;
                        }
                        if (JumpFactor <= 0.01) JumpFactor = 0.01;
                        duCurrValue = duCurrValue + JumpFactor;
                        if (duCurrValue + attenuation > RFLevelMax)
                        {
                            duCurrValue = RFLevelMax - attenuation;
                        }
                        Downbit = true;
                    }
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= TimeOut) //10 Second
                    {
                        if (AcceptMaxLevel)
                        {
                            continue;
                        }
                        clsLogManager.LogWarning("Timeout for searching RF level to make SINAD in range [{0}, {1}]",
                                                duMinSINADLevel, duMaxSINADLevel);
                        duCurrValue = double.MinValue;
                        SinadValue = double.MinValue;
                        break;
                        //int nBestIndex = -1;
                        //double duAcceptDistance = (duMaxSINADLevel + duMinSINADLevel) / 2 - duMinSINADLevel; // độ lệch cho phép = trung bình (min & max) - giá trị Min
                        //clsLogManager.LogReport("duAcceptDistance = {0}", duAcceptDistance);
                        //double duCurrDeltaValue;
                        //double duPrevDeltaValue = double.MaxValue;
                        //for (int i = 0; i < KnowSamples.Count; i++) // Tìm giá trị có độ lệch so với giá trị min nhỏ hơn độ lệch cho phép và là nhỏ nhất
                        //{
                        //    duCurrDeltaValue = Math.Abs(KnowSamples.Values.ElementAt(i) - duMinSINADLevel);
                        //    if (duCurrDeltaValue <= duAcceptDistance)
                        //    {
                        //        if (duCurrDeltaValue < duPrevDeltaValue)
                        //        {
                        //            duPrevDeltaValue = duCurrDeltaValue;
                        //            nBestIndex = i;
                        //        }
                        //    }
                        //}
                        //if (nBestIndex != -1) // Nếu tìm đc giá trị thỏa mãn sai lệch => lấy giá trị duCurrValue
                        //{
                        //    duCurrValue = KnowSamples.Keys.ElementAt(nBestIndex);
                        //}
                        //else
                        //{
                        //    duPrevDeltaValue = double.MaxValue;
                        //    for (int i = 0; i < KnowSamples.Count; i++)
                        //    {
                        //        duCurrDeltaValue = Math.Abs(KnowSamples.Values.ElementAt(i) - duMaxSINADLevel);
                        //        if (duCurrDeltaValue < duPrevDeltaValue)
                        //        {
                        //            nBestIndex = i;
                        //            duPrevDeltaValue = duCurrDeltaValue;
                        //        }
                        //    }
                        //    if (nBestIndex == -1)
                        //    {
                        //        duCurrValue = double.MaxValue;
                        //        clsLogManager.LogError("There is no RF level value reach to expected duAcceptDistance");
                        //        break;
                        //    }
                        //    duCurrValue = KnowSamples.Keys.ElementAt(nBestIndex); // Tìm giá trị có độ lệch so với giá trị max nhỏ hơn độ lệch cho phép và là nhỏ nhất
                        //}
                        //clsLogManager.LogWarning("Choose RF level is {0} make SINAD is {1}", duCurrValue.ToString("F02"),
                        //                         KnowSamples.Values.ElementAt(nBestIndex));
                        //break;
                    }// end of while
                }
                return duCurrValue;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SearchRFLevel() catched an exception: " + ec.Message);
                SinadValue = double.MaxValue;
                return double.MaxValue;
            }
        }
        public static double SearchRFLevel_AllMes(IDevMeasurementMode allmes, ISignalGenDevice SigGenDev, IAudioAnaDevice AudioDev, double duMinSINADLevel, double duMaxSINADLevel, double nBeginRFPwr,
    double attenuation/*dB*/, double MinSINAD, int TimeOut, bool AcceptMaxLevel, out double SinadValue, int nMesCnt = 5, int nDelayAfterSetAudio = 200, PowerUnit powerUnit = PowerUnit.uV)
        {
            try
            {
                double duCurrValue = nBeginRFPwr; // Giá trị công suất RF ban đầu
                double JumpFactor = duCurrValue / 2; // Bước tăng giảm: Giá trị công thêm hoặc trừ đi vào duCurrValue để kiểm tra SINAD
                bool Upbit = false;
                bool Downbit = false;
                double duSINAD = 0;
                Dictionary<double, double> KnowSamples = new Dictionary<double, double>();
                int nBeginTime = Environment.TickCount;
                int nTimeOut = 0;
                double duRFLevelMin = SigGenDev.GetMinOutputLevel();
                double RFLevelMax = SigGenDev.GetMaxOutputLevel();
                // convert attenuation dB to uV

                while (true)
                {
                    SigGenDev.SetOutputPower((duCurrValue + attenuation), powerUnit); // Set RF level
                    if (allmes != null)
                    {
                        allmes.init_Measurement(true);
                    }
                    else
                    {
                        if (nDelayAfterSetAudio > 0)
                            Thread.Sleep(nDelayAfterSetAudio / 2);
                    }
                    duSINAD = AudioDev.GetAudioSinadLevel(nMesCnt); //Get SINAD 
                    SinadValue = duSINAD;
                    clsLogManager.LogReport("{0, 25}{1, 25}", duCurrValue.ToString("F02"), duSINAD.ToString("F02"));
                    if (KnowSamples.ContainsKey(duCurrValue + attenuation)) // Nếu đã từng đo SINAD tại giá trị công suất vừa set -> bỏ giá trị cũ, cập nhật giá trị SINAD mới đo đc
                    {
                        KnowSamples.Remove(duCurrValue + attenuation);
                    }
                    KnowSamples.Add((duCurrValue + attenuation), duSINAD); // Cập nhật
                    if (duMinSINADLevel <= duCurrValue && duMaxSINADLevel >= duCurrValue) // Nếu SINAD đọc về nằm trong dải mong muốn
                    {
                        clsLogManager.LogWarning("Search RF level: done!");
                        SinadValue = duSINAD;
                        break;
                    }
                    if (duSINAD > duMaxSINADLevel) //Nếu SINAD đọc về lớn hơn dải mong muốn
                    {
                        if (duCurrValue < duRFLevelMin) // Nếu giảm mức RF xuống tối thiểu mà SINAD vẫn lớn hơn dải mong muốn => Fail
                        {
                            clsLogManager.LogError("Noise is too large");
                            SinadValue = double.MinValue;
                            return double.MinValue;
                        }
                        if (Downbit)
                        {
                            Downbit = false;
                            JumpFactor = JumpFactor / 2;
                        }
                        if (JumpFactor <= 0.01) JumpFactor = 0.01;
                        duCurrValue = duCurrValue - JumpFactor;
                        //if (duCurrValue <= 0.07) duCurrValue = 0.075; // Đảm bảo duCurrValue không giảm xuống quá nhỏ
                        if (duCurrValue <= MinSINAD) duCurrValue = MinSINAD; // Đảm bảo duCurrValue không giảm xuống quá nhỏ
                        Upbit = true;
                    }
                    else
                    if (duSINAD < duMinSINADLevel) // Nếu SINAD đọc về nhỏ hơn giá trị mong muốn
                    {
                        if (duCurrValue == RFLevelMax - attenuation)
                        {
                            clsLogManager.LogWarning("CurrRfLevel = maximum level of sigen device >> SINAD = {0} < {1}", duSINAD, duMinSINADLevel);
                            if (!AcceptMaxLevel)
                            {
                                SinadValue = double.MaxValue;
                                duCurrValue = double.MaxValue;
                            }
                            break;
                        }
                        if (Upbit) // Nếu trước đo vừa giảm duCurrValue
                        {
                            Upbit = false;
                            JumpFactor = JumpFactor / 2;
                        }
                        if (JumpFactor <= 0.01) JumpFactor = 0.01;
                        duCurrValue = duCurrValue + JumpFactor;
                        if (duCurrValue + attenuation > RFLevelMax)
                        {
                            duCurrValue = RFLevelMax - attenuation;
                        }
                        Downbit = true;
                    }
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= TimeOut) //10 Second
                    {
                        if (AcceptMaxLevel)
                        {
                            continue;
                        }
                        clsLogManager.LogWarning("Timeout for searching RF level to make SINAD in range [{0}, {1}]",
                                                duMinSINADLevel, duMaxSINADLevel);
                        SinadValue = double.MinValue;
                        duCurrValue = double.MinValue;
                        break;
                        //int nBestIndex = -1;
                        //double duAcceptDistance = (duMaxSINADLevel + duMinSINADLevel) / 2 - duMinSINADLevel; // độ lệch cho phép = trung bình (min & max) - giá trị Min
                        //clsLogManager.LogReport("duAcceptDistance = {0}", duAcceptDistance);
                        //double duCurrDeltaValue;
                        //double duPrevDeltaValue = double.MaxValue;
                        //for (int i = 0; i < KnowSamples.Count; i++) // Tìm giá trị có độ lệch so với giá trị min nhỏ hơn độ lệch cho phép và là nhỏ nhất
                        //{
                        //    duCurrDeltaValue = Math.Abs(KnowSamples.Values.ElementAt(i) - duMinSINADLevel);
                        //    if (duCurrDeltaValue <= duAcceptDistance)
                        //    {
                        //        if (duCurrDeltaValue < duPrevDeltaValue)
                        //        {
                        //            duPrevDeltaValue = duCurrDeltaValue;
                        //            nBestIndex = i;
                        //        }
                        //    }
                        //}
                        //if (nBestIndex != -1) // Nếu tìm đc giá trị thỏa mãn sai lệch => lấy giá trị duCurrValue
                        //{
                        //    duCurrValue = KnowSamples.Keys.ElementAt(nBestIndex);
                        //}
                        //else
                        //{
                        //    duPrevDeltaValue = double.MaxValue;
                        //    for (int i = 0; i < KnowSamples.Count; i++)
                        //    {
                        //        duCurrDeltaValue = Math.Abs(KnowSamples.Values.ElementAt(i) - duMaxSINADLevel);
                        //        if (duCurrDeltaValue < duPrevDeltaValue)
                        //        {
                        //            nBestIndex = i;
                        //            duPrevDeltaValue = duCurrDeltaValue;
                        //        }
                        //    }
                        //    if (nBestIndex == -1)
                        //    {
                        //        duCurrValue = double.MaxValue;
                        //        clsLogManager.LogError("There is no RF level value reach to expected duAcceptDistance");
                        //        break;
                        //    }
                        //    duCurrValue = KnowSamples.Keys.ElementAt(nBestIndex); // Tìm giá trị có độ lệch so với giá trị max nhỏ hơn độ lệch cho phép và là nhỏ nhất
                        //}
                        //clsLogManager.LogWarning("Choose RF level is {0} make SINAD is {1}", duCurrValue.ToString("F02"),
                        //                         KnowSamples.Values.ElementAt(nBestIndex));
                        //break;
                    }// end of while
                }
                return duCurrValue;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("SearchRFLevel_AllMes() catched an exception: " + ec.Message);
                SinadValue = double.MaxValue;
                return double.MaxValue;
            }
        }
        public static double SearchAudioLevelSpline(IRFPowerDevice RfPowerDev, IAudioAnaDevice AudioDev, double duMinPwr, double duMaxPwr, double nBeginAudioLevel,
            double nFactor, int nMesCnt = 5, AudioGenIndex genIndex = AudioGenIndex.GEN_1, int nDelayAfterSetAudio = 200, int nSearchTimeout = 10000, PowerUnit powerUnit = PowerUnit.Wat)
        {
            double CurrAuLevel = nBeginAudioLevel;
            double duCurrRFPwr = double.MinValue;
            double duSweepFactor = nFactor;
            int nBeginTime = Environment.TickCount;
            int nTimeOut;
            bool bSw2Linear = false;
            Dictionary<double, double> knownSamples = new Dictionary<double, double>();
            //
            try
            {


                duCurrRFPwr = clsTaskHelper.GetPowerLevel(RfPowerDev, nMesCnt, RFPowerGetResultMode.RMS, powerUnit);
                knownSamples.Add(nBeginAudioLevel, duCurrRFPwr);
                while ((duCurrRFPwr < duMinPwr) || (duCurrRFPwr > duMaxPwr))
                {
                    if (!bSw2Linear)
                    {
                        if (duCurrRFPwr > duMaxPwr)
                        {
                            CurrAuLevel = CurrAuLevel / duSweepFactor;
                        }
                        else if (duCurrRFPwr < duMinPwr)
                        {
                            CurrAuLevel = CurrAuLevel * duSweepFactor;
                        }
                        if (knownSamples.ContainsKey(CurrAuLevel))
                        {
                            bSw2Linear = true;
                            continue;
                        }
                    }
                    else
                    {
                        if (knownSamples.Count < 5)
                        {
                            CurrAuLevel = LinearSpline.InterpolateInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate((duMinPwr + duMaxPwr) / 2);
                            clsLogManager.LogReport("LinearSpline: X={0}=>Y={1}", (duMinPwr + duMaxPwr) / 2, CurrAuLevel);
                        }
                        else
                        {
                            CurrAuLevel = CubicSpline.InterpolateNaturalInplace(knownSamples.Values.ToArray(), knownSamples.Keys.ToArray()).Interpolate((duMinPwr + duMaxPwr) / 2);
                            clsLogManager.LogReport("CubicSpline: X={0}=>Y={1}", (duMinPwr + duMaxPwr) / 2, CurrAuLevel);
                        }
                    }
                    AudioDev.SetAudioLevel(CurrAuLevel, genIndex, AudioLevelUnit.MV);
                    if (nDelayAfterSetAudio > 0)
                    {
                        Thread.Sleep(nDelayAfterSetAudio);
                    }
                    duCurrRFPwr = clsTaskHelper.GetPowerLevel(RfPowerDev, nMesCnt, RFPowerGetResultMode.RMS, powerUnit);
                    knownSamples.Add(CurrAuLevel, duCurrRFPwr);
                    duCurrRFPwr = Math.Round(duCurrRFPwr, 1);
                    clsLogManager.LogReport("{0, 25}{1, 25}", CurrAuLevel.ToString("F04"), duCurrRFPwr.ToString("F04"));
                    if ((duCurrRFPwr >= duMinPwr) && (duCurrRFPwr <= duMaxPwr))
                        break;
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout) //Search 10 second
                    {
                        clsLogManager.LogError("Timeout for search subtable audio level to make the RF power in range [{0}, {1}]", duMinPwr.ToString("F04"), duMaxPwr.ToString("F04"));
                        double duNewMinPwr = duMinPwr - 0.01 * ((duMinPwr + duMaxPwr) / 2);
                        double duNewMaxPwr = duMaxPwr + 0.01 * ((duMinPwr + duMaxPwr) / 2);
                        int nBestIndex = -1;
                        for (int i = 0; i < knownSamples.Count; i++)
                        {
                            if ((knownSamples.Values.ElementAt(i) >= duNewMinPwr) && (knownSamples.Values.ElementAt(i) <= duNewMaxPwr))
                            {
                                nBestIndex = i;
                                break;
                            }
                        }
                        if (nBestIndex == -1)
                        {
                            CurrAuLevel = knownSamples.Keys.ElementAt(nBestIndex);
                            break;
                        }
                        else
                            return double.MinValue;
                    }
                }
                return CurrAuLevel;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("SearchAudioLevelSpline: {0}", ex.ToString());
                return double.MinValue;
            }
        }
        /// <summary>
        /// Điều chỉnh RF level để AF level đạt trong khoảng mong muốn
        /// </summary>
        /// <param name="allmes"></param>
        /// <param name="SigGenDevice"></param>
        /// <param name="AudioDevice"></param>
        /// <param name="RFlevel"></param>
        /// <param name="MinStopSearch"></param>
        /// <param name="MaxStopSearch"></param>
        /// <param name="RFSweepStep"></param>
        /// <param name="DelayAfterSet"></param>
        /// <param name="nSearchTimeout"></param>
        /// <param name="MesCnt"></param>
        /// <returns></returns>
        public static double Downsearch_AFLevelbyRFlevel(IDevMeasurementMode allmes, ISignalGenDevice SigGenDevice, IAudioAnaDevice AudioDevice, double RFlevel, double MinStopSearch, double MaxStopSearch, double RFSweepStep, int DelayAfterSet, int nSearchTimeout, int MesCnt = 1, PowerUnit powerUnit = PowerUnit.uV)
        {
            try
            {
                bool Upbit = false;
                bool Downbit = false;
                double UPResult = 0;
                double duAuCurrentRfLv = 0;
                int countloop = 0;
                double CurrLevel = RFlevel + RFSweepStep; // mức RF điều chỉnh (dBm)
                double MinStep = 1e-6;
                if (powerUnit == PowerUnit.uV)
                {
                    MinStep = 0.01;
                }
                else
                {
                    MinStep = 0.025; // dbm
                }
                //Up search
                clsLogManager.LogWarning("Tăng mức của máy phát sóng cao tần để đạt mức âm tần trong dải [{0}; {1}]", MinStopSearch, MaxStopSearch);
                int nBeginTime = Environment.TickCount;
                int nTimeOut;
                while (true)
                {
                    SigGenDevice.SetOutputPower(CurrLevel, powerUnit);
                    if (allmes != null)
                    {
                        allmes.init_Measurement(true);
                    }
                    {
                        Thread.Sleep(DelayAfterSet);
                    }
                    //GetAudioRxLevel(IOService, arrResult, ref duAuCurrentRfLv);
                    duAuCurrentRfLv = AudioDevice.GetAudioPowerLevel(AudioLevelUnit.MV, MesCnt);
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", "RF level (dBm)", " Audio level (mV)", "RFSweepStep (dB)");
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", CurrLevel.ToString("F02"), duAuCurrentRfLv.ToString(), (RFSweepStep).ToString());
                    if (duAuCurrentRfLv > MaxStopSearch)
                    {
                        if (Upbit) // Nếu đã tăng RF level và mức âm tần lớn hơn khoảng mong muốn => kiểm tra bước điều chỉnh và giảm mức RF
                        {
                            Upbit = false;
                            if (RFSweepStep <= MinStep)
                            {
                                RFSweepStep = MinStep;
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                            }
                        }
                        CurrLevel = CurrLevel - RFSweepStep;
                        if (CurrLevel < RFlevel) CurrLevel = RFlevel;

                        Downbit = true;

                    }
                    else if (duAuCurrentRfLv < MinStopSearch)
                    {
                        if (Downbit) // Nếu đã tăng mức RF và mức âm tần vẫn nhỏ hơn khoảng mong muốn => kiểm tra bước điều chỉnh và tăng mức RF
                        {
                            Downbit = false;
                            if (RFSweepStep <= MinStep) // 
                            {
                                RFSweepStep = MinStep;
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                            }
                        }
                        CurrLevel = CurrLevel + RFSweepStep;
                        Upbit = true;
                    }
                    else
                    {
                        UPResult = CurrLevel;
                        clsLogManager.LogReport("UpResult: {0}", UPResult.ToString());
                        break;
                    }
                    countloop++;
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout) //Timeout
                    {
                        clsLogManager.LogReport("Time out UP search RF level");
                        UPResult = double.MinValue;
                        break;
                    }
                } // End of while
                return UPResult;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Downsearch_AFLevelbyRFlevel() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        /// <summary>
        /// Hàm điều chỉnh RF level (signal gen 2) để đạt được dải Sinad mong muốn (Tăng RF level thì sinad bị giảm)
        /// </summary>
        /// <param name="SigGenDevice"></param>
        /// <param name="AudioDevice"></param>
        /// <param name="RFlevel"></param>
        /// <param name="MinStopSearch"></param>
        /// <param name="MaxStopSearch"></param>
        /// <param name="RFSweepStep"></param>
        /// <param name="DelayAfterSet"></param>
        /// <param name="nSearchTimeout"></param>
        /// <param name="MesCnt"></param>
        /// <returns></returns>
        public static double Upsearch_RFlevel(IDevMeasurementMode allmes, ISignalGenDevice SigGenDevice, IAudioAnaDevice AudioDevice, double RFlevel, double MinStopSearch, double MaxStopSearch, double RFSweepStep, int DelayAfterSet, int nSearchTimeout, int MesCnt = 1, PowerUnit unit = PowerUnit.dBm)
        {
            try
            {
                bool Upbit = false;
                bool Downbit = false;
                double UPResult = 0;
                double duAuCurrentRfLv = 0;
                int countloop = 0;
                double CurrLevel = RFlevel + RFSweepStep; // mức RF điều chỉnh (dBm)
                double MinStep = 1e-6;
                if (unit == PowerUnit.dBm)
                {
                    MinStep = 0.01;
                }
                else
                {
                    MinStep = 0.001; // uV
                }
                //Up search
                clsLogManager.LogWarning("Tăng mức của máy phát sóng cao tần để đạt Sinad[{0}; {1}]", MinStopSearch, MaxStopSearch);
                int nBeginTime = Environment.TickCount;
                int nTimeOut;
                while (true)
                {
                    SigGenDevice.SetOutputPower(CurrLevel, PowerUnit.dBm);
                    if (allmes != null)
                    {
                        allmes.init_Measurement(true);
                    }
                    {
                        Thread.Sleep(DelayAfterSet);
                    }
                    //GetAudioRxLevel(IOService, arrResult, ref duAuCurrentRfLv);
                    duAuCurrentRfLv = AudioDevice.GetAudioSinadLevel(MesCnt);
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", "RF level (dBm)", " Sinad (dB)", "RFSweepStep(dB)");
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", CurrLevel.ToString("F02"), duAuCurrentRfLv.ToString(), (RFSweepStep).ToString());
                    if (duAuCurrentRfLv > MaxStopSearch)
                    {
                        if (Downbit) // Nếu đã giảm mức đi và sinad lớn hơn khoảng mong muốn => giảm bước điều chỉnh RF trước khi tăng tần số
                        {
                            Downbit = false;
                            if (RFSweepStep <= MinStep) // 
                            {
                                RFSweepStep = MinStep;
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                            }
                        }
                        CurrLevel = CurrLevel + RFSweepStep;
                        Upbit = true;
                    }
                    else if (duAuCurrentRfLv < MinStopSearch)
                    {
                        if (Upbit) // Nếu đã tăng tần số đi và mức âm tần nhỏ hơn khoảng mong muốn => giảm bước điều chỉnh RF trước khi giảm tần số
                        {
                            Upbit = false;
                            if (RFSweepStep <= MinStep)
                            {
                                RFSweepStep = MinStep;
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                            }
                        }
                        CurrLevel = CurrLevel - RFSweepStep;
                        if (CurrLevel < RFlevel) CurrLevel = RFlevel;

                        Downbit = true;
                    }
                    else
                    {
                        UPResult = CurrLevel;
                        clsLogManager.LogReport("UpResult:{0}", UPResult.ToString());
                        break;
                    }
                    countloop++;
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout) //Search 10 second
                    {
                        clsLogManager.LogReport("Time out UP search RF level");
                        UPResult = double.MinValue;
                        break;
                    }
                } // End of while
                return UPResult;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Upsearch_RFlevel() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        /// <summary>
        /// Hàm điều chỉnh RF level để đạt được dải Sinad mong muốn (Tăng RF level thì sinad tăng)
        /// </summary>
        /// <param name="SigGenDevice"></param>
        /// <param name="AudioDevice"></param>
        /// <param name="RFlevel"></param>
        /// <param name="MinStopSearch"></param>
        /// <param name="MaxStopSearch"></param>
        /// <param name="RFSweepStep"></param>
        /// <param name="DelayAfterSet"></param>
        /// <param name="nSearchTimeout"></param>
        /// <param name="MesCnt"></param>
        /// <returns></returns>
        public static double Upsearch_RFlevel_sinad(IDevMeasurementMode allmes, ISignalGenDevice SigGenDevice, IAudioAnaDevice AudioDevice, double RFlevel, double MinStopSearch, double MaxStopSearch, double RFSweepStep, int DelayAfterSet, int nSearchTimeout, double RFLevelmax, bool AcceptLevelmax, int MesCnt = 1, PowerUnit unit = PowerUnit.dBm)
        {
            try
            {
                bool Upbit = false;
                bool Downbit = false;
                double UPResult = 0;
                double duAuCurrentRfLv = 0;
                int countloop = 0;
                double CurrLevel = RFlevel + RFSweepStep; // mức RF điều chỉnh (dBm)
                double MinStep = 1e-6;
                if (unit == PowerUnit.dBm)
                {
                    MinStep = 0.1; // dB
                }
                else
                {
                    MinStep = 0.001; // uV
                }
                //Up search
                int Minstep_reachMin = 0;
                int MinStep_reachMax = 0;
                clsLogManager.LogWarning("Tăng mức của máy phát sóng cao tần để đạt Sinad [{0}; {1}]", MinStopSearch, MaxStopSearch);
                int nBeginTime = Environment.TickCount;
                int nTimeOut;
                while (true)
                {
                    SigGenDevice.SetOutputPower(CurrLevel, PowerUnit.dBm);
                    if (allmes != null)
                    {
                        allmes.init_Measurement(true);
                    }
                    {
                        Thread.Sleep(DelayAfterSet);
                    }
                    //GetAudioRxLevel(IOService, arrResult, ref duAuCurrentRfLv);
                    duAuCurrentRfLv = AudioDevice.GetAudioSinadLevel(MesCnt);
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", "RF level (dBm)", " Sinad (dB)", "RFSweepStep(dB)");
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", CurrLevel.ToString("F03"), duAuCurrentRfLv.ToString(), (RFSweepStep).ToString());
                    if (duAuCurrentRfLv > MaxStopSearch)
                    {                       
                        if (Upbit) // Tăng mức lên và sinad lớn hơn khoảng mong muốn => Giảm mức (bước giảm đi để tránh vòng lặp)
                        {
                            Upbit = false;
                            if (RFSweepStep <= MinStep) // 
                            {
                                RFSweepStep = MinStep;
                                MinStep_reachMax++; // Tăng với mức nhỏ nhất mà vẫn bị over
                                clsLogManager.LogWarning("Reach min step over ++");
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                            }
                        }
                        CurrLevel = CurrLevel - RFSweepStep;
                        if (CurrLevel < RFlevel) CurrLevel = RFlevel;
                        Downbit = true;
                    }
                    else if (duAuCurrentRfLv < MinStopSearch)
                    {
                        if (Upbit && CurrLevel == RFLevelmax)
                        {
                            if (AcceptLevelmax)
                            {
                                clsLogManager.LogWarning("Return RF max level!");
                                CurrLevel = RFLevelmax;
                                UPResult = RFLevelmax;
                                break;
                            }
                            else
                            {
                                clsLogManager.LogWarning("Reach RF max level!");
                                CurrLevel = double.MaxValue;
                                break;
                            }
                        }
                        if (Downbit) // giảm mức đi và sinad nhỏ hơn khoảng mong muốn => tăng mức (bước giảm đi để tránh vòng lặp)
                        {
                            Downbit = false;
                            if (RFSweepStep <= MinStep)
                            {
                                RFSweepStep = MinStep;
                                Minstep_reachMin++; // giảm với bước min rồi mà vẫn bắt đc sinad < mong muốn
                                clsLogManager.LogWarning("Reach min step lower ++");
                                if (MinStep_reachMax >= 2 && Minstep_reachMin >= 2)
                                {
                                    clsLogManager.LogWarning("RF level step = {0} can't reach sinad [{1}; {2}] => choose RF level = {3} dBm", RFSweepStep, MinStopSearch, MaxStopSearch, CurrLevel);
                                    UPResult = CurrLevel;
                                    clsLogManager.LogReport("DounResult = {0}", UPResult.ToString());
                                    break;
                                }
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                            }
                        }
                        CurrLevel = CurrLevel + RFSweepStep;
                        if (CurrLevel > RFLevelmax)
                        {
                            CurrLevel = RFLevelmax;
                        }

                        Upbit = true;
                    }
                    else
                    {
                        UPResult = CurrLevel;
                        clsLogManager.LogReport("UpResult: {0}", UPResult.ToString());
                        break;
                    }
                    countloop++;
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout) //Search 10 second
                    {
                        clsLogManager.LogReport("Time out UP search RF level");
                        if (CurrLevel == RFLevelmax)
                        {
                            if (AcceptLevelmax)
                            {
                                clsLogManager.LogWarning("Return RF level max");
                                UPResult = RFLevelmax;
                            }
                        }
                        else
                        {
                            UPResult = double.MinValue;
                        }
                        break;
                    }
                } // End of while
                return UPResult;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Upsearch_RFlevel_sinad() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        /// <summary>
        /// Hàm điều chỉnh RF level để đạt được dải Sinad mong muốn (Tăng RF level thì sinad tăng)
        /// </summary>
        /// <param name="SigGenDevice"></param>
        /// <param name="AudioDevice"></param>
        /// <param name="RFlevel"></param>
        /// <param name="MinStopSearch"></param>
        /// <param name="MaxStopSearch"></param>
        /// <param name="RFSweepStep"></param>
        /// <param name="DelayAfterSet"></param>
        /// <param name="nSearchTimeout"></param>
        /// <param name="MesCnt"></param>
        /// <returns></returns>
        public static double Downsearch_RFlevel_Sinad(IDevMeasurementMode allmes, ISignalGenDevice SigGenDevice, IAudioAnaDevice AudioDevice, double RFlevel, double MinStopSearch, double MaxStopSearch, double RFSweepStep, int DelayAfterSet, int nSearchTimeout, double RFLevelmin, bool AcceptLevelmin, int MesCnt = 1, PowerUnit unit = PowerUnit.dBm)
        {
            try
            {
                bool Upbit = false;
                bool Downbit = false;
                double DownResult = 0;
                double duAuCurrentRfLv = 0;
                int countloop = 0;
                double CurrLevel = RFlevel - RFSweepStep; // mức RF điều chỉnh (dBm)
                double MinStep = 1e-6;
                int Minstep_reachMin = 0;
                int MinStep_reachMax = 0;
                if (unit == PowerUnit.dBm)
                {
                    MinStep = 0.1; // dB
                }
                else
                {
                    MinStep = 0.001; // uV
                }
                //Up search
                clsLogManager.LogWarning("Giảm mức của máy phát sóng cao tần để đạt Sinad [{0}; {1}]", MinStopSearch, MaxStopSearch);
                int nBeginTime = Environment.TickCount;
                int nTimeOut;
                while (true)
                {
                    SigGenDevice.SetOutputPower(CurrLevel, PowerUnit.dBm);
                    if (allmes != null)
                    {
                        allmes.init_Measurement(true);
                    }
                    {
                        Thread.Sleep(DelayAfterSet);
                    }
                    //GetAudioRxLevel(IOService, arrResult, ref duAuCurrentRfLv);
                    duAuCurrentRfLv = AudioDevice.GetAudioSinadLevel(MesCnt);
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", "RF level (dBm)", " Sinad (dB)", "RFSweepStep(dB)");
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", CurrLevel.ToString("F03"), duAuCurrentRfLv.ToString(), (RFSweepStep).ToString());
                    if (duAuCurrentRfLv > MaxStopSearch)
                    {
                        if (Downbit && CurrLevel == RFLevelmin) // Nếu giảm liên tiếp RF level ở ngưỡng min RF thì break
                        {
                            if (AcceptLevelmin)
                            {
                                CurrLevel = RFLevelmin;
                                DownResult = RFLevelmin;
                                clsLogManager.LogWarning("Return RF min level!");
                                break;
                            }
                            else
                            {
                                CurrLevel = double.MinValue;
                                clsLogManager.LogWarning("Reach RF min level!");
                                break;
                            }
                        }
                        if (Upbit) // // Tăng mức lên và sinad lớn hơn khoảng mong muốn => giảm mức (bước giảm đi để tránh vòng lặp)
                        {
                            Upbit = false;
                            if (RFSweepStep <= MinStep) // 
                            {
                                RFSweepStep = MinStep;
                                MinStep_reachMax++; // tăng theo minstep và bắt phải giá trị > max sinad
                                clsLogManager.LogWarning("Reach min step over ++");
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                            }
                        }
                        CurrLevel = CurrLevel - RFSweepStep;
                        //if (CurrLevel > RFlevel) CurrLevel = RFlevel;
                        if (CurrLevel < RFLevelmin) CurrLevel = RFLevelmin;
                        Downbit = true;
                    }
                    else if (duAuCurrentRfLv < MinStopSearch)
                    {
                        if (Downbit) // giảm mức đi và sinad nhỏ hơn khoảng mong muốn => tăng mức (bước giảm đi để tránh vòng lặp)
                        {
                            Downbit = false;
                            if (RFSweepStep <= MinStep)
                            {
                                RFSweepStep = MinStep; 
                                Minstep_reachMin++; // Giảm theo minstep và bắt phải giá trị < min sinad
                                clsLogManager.LogWarning("Reach min step lower ++");
                                if (MinStep_reachMax >= 2 && Minstep_reachMin >= 2)
                                {
                                    clsLogManager.LogWarning("RF level step = {0} can't reach sinad [{1}; {2}] => choose RF level = {3}", RFSweepStep, MinStopSearch, MaxStopSearch, CurrLevel);
                                    DownResult = CurrLevel;
                                    clsLogManager.LogReport("DounResult = {0}", DownResult.ToString());
                                    break;
                                }
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                            }
                        }
                        CurrLevel = CurrLevel + RFSweepStep;
                        if (CurrLevel > RFlevel) CurrLevel = RFlevel;
                        Upbit = true;
                    }
                    else
                    {
                        DownResult = CurrLevel;
                        clsLogManager.LogReport("DownResult = {0}", DownResult.ToString());
                        break;
                    }
                    countloop++;
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout) //Search 10 second
                    {
                        clsLogManager.LogReport("Time out UP search RF level");
                        if (CurrLevel == RFLevelmin)
                        {
                            if (AcceptLevelmin)
                            {
                                clsLogManager.LogWarning("Return RF level min");
                                DownResult = RFLevelmin;
                            }
                        }
                        else
                        {
                            DownResult = double.MinValue;
                        }
                        break;
                    }
                } // End of while
                return DownResult;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Downsearch_RFlevel_Sinad() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        /// <summary>
        /// Hàm tăng tần số RF để điện áp âm tần đạt trong khoảng yêu cầu
        /// </summary>
        /// <param name="SigGenDevice">Máy phát cao tần</param>
        /// <param name="AudioDevice">Máy đo âm tần</param>
        /// <param name="DutFreq">Tần số DUT</param>
        /// <param name="RfFreq">Tần số phát RF</param>
        /// <param name="MinStopSearch">Ngưỡng điện áp dưới cần điều chỉnh để đạt tới</param>
        /// <param name="MaxStopSearch">Ngưỡng điện áp trên cần điều chỉnh để đạt tới</param>
        /// <param name="RFSweepStep">Bước điều chỉnh âm tần</param>
        /// <param name="DelayAfterSet">Thời gian delay sau khi thiết lập máy đo</param>
        /// <param name="nSearchTimeout">Thời gian điều chỉnh tối đa</param>
        /// <param name="MesCnt">Số lần đo mức âm tần để lấy trung bình</param>
        /// <returns></returns>
        public static double Upsearch(IDevMeasurementMode allmes, ISignalGenDevice SigGenDevice, IAudioAnaDevice AudioDevice, double RfFreq, double MinStopSearch, double MaxStopSearch, double RFSweepStep, int DelayAfterSet, int nSearchTimeout, int MesCnt = 1, double Freq_Offset = 0)
        {
            try
            {
                bool Upbit = false;
                bool Downbit = false;
                double UPResult = 0;
                double duAuCurrentRfLv = 0;
                int countloop = 0;
                RFSweepStep = RFSweepStep / 1e6; // Hz to MHz
                double CurrFreq = RfFreq + RFSweepStep; // Tần số RF điều chỉnh (MHz)

                int Minstep_reachMin = 0;
                int MinStep_reachMax = 0;
                //Up search
                clsLogManager.LogWarning("Xác định tần số f1 bằng cách tăng tần số của máy phát sóng cao tần để đạt âm lượng [{0}; {1}]", MinStopSearch, MaxStopSearch);
                int nBeginTime = Environment.TickCount;
                int nTimeOut;
                while (true)
                {
                    SigGenDevice.SetRFFrequency(CurrFreq + Freq_Offset, FreqUnit.MHz);
                    if (allmes != null)
                    {
                        allmes.init_Measurement(true);
                    }
                    {
                        Thread.Sleep(DelayAfterSet);
                    }
                    //GetAudioRxLevel(IOService, arrResult, ref duAuCurrentRfLv);
                    duAuCurrentRfLv = AudioDevice.GetAudioPowerLevel(AudioLevelUnit.MV, MesCnt);
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", "Frequency(MHz)", " Audio level (mV)", "RFSweepStep(Hz)");
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", CurrFreq.ToString(), duAuCurrentRfLv.ToString("F03"), (RFSweepStep * 1e6).ToString());
                    if (duAuCurrentRfLv > MaxStopSearch) // Lớn hơn khoảng mong muốn -> tiếp tục tăng
                    {
                        if (Downbit) // Nếu đã giảm tần số đi và mức âm tần lớn hơn khoảng mong muốn => giảm bước điều chỉnh RF trước khi tăng tần số
                        {
                            Downbit = false;
                            if (RFSweepStep <= 1e-6) // 1 Hz
                            {
                                RFSweepStep = 1e-6;
                                MinStep_reachMax++;
                                clsLogManager.LogWarning("Reach Max ++");
                                if (MinStep_reachMax >= 2 && Minstep_reachMin >= 2)
                                {
                                    clsLogManager.LogWarning("RF freqStep = {0} Hz can't reach [{1}; {2}] => choose RF frequency = {3} MHz", RFSweepStep * 1e6, MinStopSearch, MaxStopSearch, CurrFreq);
                                    UPResult = CurrFreq;
                                    clsLogManager.LogReport("UPResult: {0}", UPResult.ToString());
                                    break;
                                }
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                                if (RFSweepStep < 1e-6)
                                {
                                    RFSweepStep = 1e-6;
                                }
                            }
                        }
                        CurrFreq = Math.Round(CurrFreq + RFSweepStep, 6); // MHz
                        Upbit = true;
                    }
                    else if (duAuCurrentRfLv < MinStopSearch)
                    {
                        if (Upbit) // Nếu đã tăng tần số đi và mức âm tần nhỏ hơn khoảng mong muốn => giảm bước điều chỉnh RF trước khi giảm tần số
                        {
                            Upbit = false;
                            if (RFSweepStep <= 1e-6)
                            {
                                RFSweepStep = 1e-6;
                                Minstep_reachMin++;
                                clsLogManager.LogWarning("Reach Min ++");
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                                if (RFSweepStep < 1e-6)
                                {
                                    RFSweepStep = 1e-6;
                                }
                            }
                        }
                        CurrFreq = Math.Round(CurrFreq - RFSweepStep, 6);
                        if (CurrFreq < RfFreq) CurrFreq = RfFreq;

                        Downbit = true;
                    }
                    else
                    {
                        UPResult = CurrFreq;
                        clsLogManager.LogReport("UpResult: {0}", UPResult.ToString());
                        break;
                    }
                    countloop++;
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout) //Search 10 second
                    {
                        clsLogManager.LogReport("Time out Up search RF frequency to reach AF level [{0}; {1}]", MinStopSearch, MaxStopSearch);
                        // Check
                        if (MinStep_reachMax >= 1 && Minstep_reachMin >= 1)
                        {
                            clsLogManager.LogWarning("RF freqStep = {0} Hz can't reach [{1}; {2}] => choose RF frequency = {3} MHz", RFSweepStep * 1e6, MinStopSearch, MaxStopSearch, CurrFreq);
                            UPResult = CurrFreq;
                            clsLogManager.LogReport("UpResult: {0}", UPResult.ToString());
                            break;
                        }
                        UPResult = double.MinValue;
                        break;
                    }
                } // End of while
                return UPResult;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Upsearch() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        /// <summary>
        /// Hàm giảm TẦN SỐ RF để điện áp âm tần đạt trong khoảng yêu cầu
        /// </summary>
        /// <param name="SigGenDevice">Máy phát cao tần</param>
        /// <param name="AudioDevice">Máy đo âm tần</param>
        /// <param name="DutFreq">Tần số DUT</param>
        /// <param name="RfFreq">Tần số phát RF</param>
        /// <param name="MinStopSearch">Ngưỡng điện áp dưới cần điều chỉnh để đạt tới</param>
        /// <param name="MaxStopSearch">Ngưỡng điện áp trên cần điều chỉnh để đạt tới</param>
        /// <param name="RFSweepStep">Bước điều chỉnh âm tần</param>
        /// <param name="DelayAfterSet">Thời gian delay sau khi thiết lập máy đo</param>
        /// <param name="nSearchTimeout">Thời gian điều chỉnh tối đa</param>
        /// <param name="MesCnt">Số lần đo mức âm tần để lấy trung bình</param>
        /// <returns></returns>
        public static double Downsearch(IDevMeasurementMode Allmess, ISignalGenDevice SigGenDevice, IAudioAnaDevice AudioDevice, double RfFreq, double MinStopSearch, double MaxStopSearch, double RFSweepStep, int DelayAfterSet, int nSearchTimeout, int MesCnt = 1, double Freq_Offset = 0)
        {
            try
            {
                //Down search
                clsLogManager.LogWarning("Xác định tần số f2 bằng cách giảm tần số của máy phát sóng cao tần [{0}; {1}]", MinStopSearch, MaxStopSearch);
                bool Upbit = false;
                bool Downbit = false;
                double DowResult = 0;
                double duAuCurrentRfLv = 0;
                int countloop = 0;
                RFSweepStep = RFSweepStep / 1e6; // Hz to MHz
                double CurrFreq = RfFreq - RFSweepStep + Freq_Offset;
                Dictionary<double, double> RFFreq_AFLevel = new Dictionary<double, double>();

                int Minstep_reachMin = 0;
                int MinStep_reachMax = 0;
                //switch (powerUnit)
                //{
                //    case PowerUnit.mV:
                //        duMinLevelOfSinGen = clsRFPowerUnitHelper.Dbm2Vol(duMinLevelOfSinGen) * 1e3;
                //        RFLevelMax = clsRFPowerUnitHelper.Dbm2Vol(RFLevelMax) * 1e3;
                //        break;
                //    case PowerUnit.Wat:
                //        duMinLevelOfSinGen = clsRFPowerUnitHelper.Dbm2Wat(duMinLevelOfSinGen);
                //        RFLevelMax = clsRFPowerUnitHelper.Dbm2Wat(RFLevelMax);
                //        break;
                //    case PowerUnit.uV:
                //        duMinLevelOfSinGen = clsRFPowerUnitHelper.Dbm2Vol(duMinLevelOfSinGen) * 1e6;
                //        RFLevelMax = clsRFPowerUnitHelper.Dbm2Vol(RFLevelMax) * 1e6;
                //        break;
                //}
                int nBeginTime = Environment.TickCount;
                int nTimeOut;
                while (true)
                {
                    //CurrFreq = Math.Round(CurrFreq/* + Freq_Offset*/, 1);
                    SigGenDevice.SetRFFrequency(CurrFreq/* + Freq_Offset*/, FreqUnit.MHz);
                    if (Allmess != null)
                    {
                        Allmess.init_Measurement(true);
                    }
                    else
                    {
                        if (DelayAfterSet > 0)
                            Thread.Sleep(DelayAfterSet);
                    }
                    //GetAudioRxLevel(IOService, arrResult, ref duAuCurrentRfLv);
                    duAuCurrentRfLv = AudioDevice.GetAudioPowerLevel(AudioLevelUnit.MV, MesCnt);
                    //// Add dictionnary
                    //if (RFFreq_AFLevel.ContainsKey(CurrFreq/* + Freq_Offset*/))
                    //{
                    //    RFFreq_AFLevel.Remove(duAuCurrentRfLv);
                    //}
                    //RFFreq_AFLevel.Add(CurrFreq/* + Freq_Offset*/, duAuCurrentRfLv);

                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", "Frequency(MHz)", " Audio level (mV)", "RFSweepStep (Hz)");
                    clsLogManager.LogReport("{0, 25}{1, 25}{2, 25}", CurrFreq.ToString(), duAuCurrentRfLv.ToString("F03"), (RFSweepStep * 1e6).ToString("F04"));
                    if (duAuCurrentRfLv > MaxStopSearch) // Lớn hơn khoảng mong muốn -> tiếp tục giảm
                    {
                        if (Upbit) // Nếu đã tăng tần số đi và mức âm tần lớn hơn khoảng mong muốn => giảm bước điều chỉnh RF trước khi giảm tần số
                        {
                            Upbit = false;
                            if (RFSweepStep <= 1e-6) // 1 Hz
                            {
                                RFSweepStep = 1e-6;
                                MinStep_reachMax++;
                                clsLogManager.LogWarning("Reach Max ++");
                                if (MinStep_reachMax >= 2 && Minstep_reachMin >= 2)
                                {
                                    clsLogManager.LogWarning("RF freqStep = {0} Hz can't reach [{1}; {2}] => choose RF frequency = {3} MHz", RFSweepStep * 1e6, MinStopSearch, MaxStopSearch, CurrFreq);
                                    DowResult = CurrFreq;
                                    clsLogManager.LogReport("DounResult:{0}", DowResult.ToString());
                                    break;
                                }
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                                if (RFSweepStep < 1e-6)
                                {
                                    RFSweepStep = 1e-6;
                                }
                            }
                        }
                        CurrFreq = Math.Round(CurrFreq - RFSweepStep, 6); // MHz
                        Downbit = true;
                    }
                    else
                    if (duAuCurrentRfLv < MinStopSearch) // Nhỏ hơn min -> tăng tần số
                    {
                        if (Downbit) // Giảm RF freq thì trên khoảng, tăng RF freq thì dưới khoảng => tăng RF freq với bước nhỏ hơn (để tăng AF level)
                        {
                            Downbit = false;
                            if (RFSweepStep <= 1e-6)
                            {
                                RFSweepStep = 1e-6;
                                Minstep_reachMin++;
                                clsLogManager.LogWarning("Reach Min ++");
                            }
                            else
                            {
                                RFSweepStep = RFSweepStep / 2;
                                if (RFSweepStep < 1e-6)
                                {
                                    RFSweepStep = 1e-6;
                                }
                            }
                        }
                        CurrFreq = Math.Round(CurrFreq + RFSweepStep, 6); // MHz
                        if (CurrFreq > RfFreq)
                            CurrFreq = RfFreq;
                        Upbit = true;
                    }
                    else
                    {
                        DowResult = CurrFreq;
                        clsLogManager.LogReport("DounResult: {0}", DowResult.ToString());
                        break;
                    }
                    countloop++;
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout) //Search 10 second
                    {
                        clsLogManager.LogReport("Time out Down search RF frequency to reach AF level [{0}; {1}]", MinStopSearch, MaxStopSearch);
                        // Check
                        if (MinStep_reachMax >= 1 && Minstep_reachMin >= 1)
                        {
                            clsLogManager.LogWarning("RF freqStep = {0} Hz can't reach [{1}; {2}] => choose RF frequency = {3} MHz", RFSweepStep * 1e6, MinStopSearch, MaxStopSearch, CurrFreq);
                            DowResult = CurrFreq;
                            clsLogManager.LogReport("DounResult: {0}", DowResult.ToString());
                            break;
                        }
                        DowResult = double.MinValue;
                        break;
                    }
                }
                return DowResult;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Downsearch() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public static double UpSearchRFlevel(ISignalGenDevice SigGenService, IDevMeasurementMode Allmess, IAudioAnaDevice AudioService, double MinStopSearch, double SearchRfLevelStart, PowerUnit RfUnit, double RFLevelStep, int DelayAfterSet, int nSearchTimeout)
        {
            try
            {
                clsLogManager.LogWarning("Xác định tần số mức cao tần bằng cách tăng mức cao tần của máy phát sóng cao tần");
                bool IsInter = false;
                double duAuCurrentRfLv = 0;
                Dictionary<double, double> knowSamples = new Dictionary<double, double>();

                clsLogManager.LogReport("Search RF level to make audio");
                clsLogManager.LogReport("{0, 25}{1, 25}", "RF Level (uV) ", "Audio level (mV)");

                int nTimeOut = 0;
                double duBegin = SearchRfLevelStart;
                double duMaxLevelOfSigen = clsRFPowerUnitHelper.Dbm2Vol(SigGenService.GetMaxOutputLevel()) * 1e6;
                double duMinLevelOfSigen = clsRFPowerUnitHelper.Dbm2Vol(SigGenService.GetMinOutputLevel()) * 1e6;

                if (duBegin > duMaxLevelOfSigen)
                    duBegin = duMaxLevelOfSigen;

                int nBeginTime = Environment.TickCount;
                while (true)
                {
                    SigGenService.SetOutputPower(duBegin, RfUnit);

                    if (Allmess != null)
                    {
                        Allmess.init_Measurement(true);
                    }
                    else if (DelayAfterSet > 0)
                    {
                        Thread.Sleep(DelayAfterSet);
                    }

                    duAuCurrentRfLv = AudioService.GetAudioPowerLevel(AudioLevelUnit.MV, 3, true);
                    clsLogManager.LogWarning("{0, 25}{1, 25}", duBegin.ToString("F04"), duAuCurrentRfLv.ToString("F04"));


                    if (duAuCurrentRfLv >= MinStopSearch / 2)
                    {
                        RFLevelStep = RFLevelStep / 2;
                        if (duAuCurrentRfLv > MinStopSearch)
                            break;
                    }
                    else if (duAuCurrentRfLv >= MinStopSearch * 3 / 4)
                    {
                        RFLevelStep = RFLevelStep / 4;
                        if (duAuCurrentRfLv > MinStopSearch)
                            break;
                    }

                    if (duAuCurrentRfLv < MinStopSearch)
                    {
                        duBegin = duBegin + RFLevelStep;
                        if (duBegin >= duMaxLevelOfSigen)
                        {
                            duBegin = duMaxLevelOfSigen;
                            break;
                        }
                    }
                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout)
                    {
                        clsLogManager.LogReport("Time out UP search RF level");
                        duBegin = double.MaxValue;
                        break;
                    }
                } // End of while
                return duBegin;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("UpSearchRFlevel() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public static double DownSearchRFlevel(ISignalGenDevice SigGenService, IDevMeasurementMode Allmess, IAudioAnaDevice AudioService, double MinStopSearch/*, double MaxStopSearch*/, double RfLevel, PowerUnit RfUnit, double RFLevelStep, int DelayAfterSet, int nSearchTimeout)
        {
            try
            {
                clsLogManager.LogWarning("Xác định tần số mức cao tần bằng cách giảm mức cao tần của máy phát sóng cao tần");
                bool IsInter = false;
                double duAuCurrentRfLv = 0;
                Dictionary<double, double> knowSamples = new Dictionary<double, double>();

                clsLogManager.LogReport("Search RF level to make audio ");
                clsLogManager.LogReport("{0, 25}{1, 25}", "RF Level (uV) ", "Audio level (mV)");

                int nTimeOut = 0;
                double duBegin = RfLevel;
                double duMaxLevelOfSigen = clsRFPowerUnitHelper.Dbm2Vol(SigGenService.GetMaxOutputLevel()) * 1e3;
                double duMinLevelOfSigen = clsRFPowerUnitHelper.Dbm2Vol(SigGenService.GetMinOutputLevel()) * 1e3;

                if (duBegin > duMaxLevelOfSigen)
                    duBegin = duMaxLevelOfSigen;

                int nBeginTime = Environment.TickCount;
                while (true)
                {
                    SigGenService.SetOutputPower(duBegin, RfUnit);
                    if (Allmess != null)
                    {
                        Allmess.init_Measurement(true);
                    }
                    else if (DelayAfterSet > 0)
                    {
                        Thread.Sleep(DelayAfterSet);
                    }

                    duAuCurrentRfLv = AudioService.GetAudioPowerLevel(AudioLevelUnit.MV, 3, true);
                    clsLogManager.LogWarning("{0, 25}{1, 25}", duBegin.ToString("F04"), duAuCurrentRfLv.ToString("F04"));

                    if (duAuCurrentRfLv <= MinStopSearch * 1.5)
                    {
                        RFLevelStep = RFLevelStep / 2;
                        if (duAuCurrentRfLv < MinStopSearch)
                            break;
                    }
                    else if (duAuCurrentRfLv <= MinStopSearch * 1.2)
                    {
                        RFLevelStep = RFLevelStep / 4;
                        if (duAuCurrentRfLv < MinStopSearch)
                            break;
                    }

                    if (duAuCurrentRfLv >= MinStopSearch)
                    {
                        duBegin = duBegin - RFLevelStep;
                        if (duBegin <= duMinLevelOfSigen)
                        {
                            duBegin = duMinLevelOfSigen;
                            break;
                        }
                    }

                    nTimeOut = Environment.TickCount - nBeginTime;
                    if (nTimeOut >= nSearchTimeout)
                    {
                        clsLogManager.LogReport("Time out Down search RF level");
                        duBegin = double.MaxValue;
                        break;
                    }
                } // End of while
                return duBegin;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Downsearch() catched an exception: " + ec.Message);
                return double.MinValue;
            }
        }
        public static double Get_Current(CFTWinAppCore.DeviceManager.IDeviceManager DevManager, bool AutoGetCurrent)
        {
            try
            {
                double InputCurrent = double.MaxValue;

                if (!AutoGetCurrent) // Nhập tay
                {
                    clsFrmInput dlg = new clsFrmInput(typeof(double));
                    //dlg.TimeshowForm = Timeshow;
                    dlg.ShowDialog();
                    InputCurrent = double.Parse(dlg.InputValue);
                }
                else // Tự động đọc dòng
                {
                    if (DevManager.IsDevEnable(SysDevType.POWER_SUPPLY.ToString()))
                    {
                        IPowerSupplyDevice PowerDev = (IPowerSupplyDevice)(DevManager.GetDevService(SysDevType.POWER_SUPPLY.ToString()));
                        InputCurrent = PowerDev.GetPowerCurrent(PowerSupplyOutputPort.PORT_1);
                        Thread.Sleep(200);
                    }
                    else
                    {
                        MessageBox.Show("Error: Power supply is unvailable!");
                        InputCurrent = double.MaxValue;
                    }
                }
                return InputCurrent;
            }
            catch (Exception ec)
            {
                clsLogManager.LogError("Get_Current() catched an exception: " + ec.Message);
                return double.MaxValue;
            }
        }
        #endregion
        #region Math function
        public static double[] Str2DoubleArr(string strInput)
        {
            try
            {
                string[] arrTemp = strInput.Split(',');
                double[] arrResult = new double[arrTemp.Length];
                for (int i = 0; i < arrTemp.Length; i++)
                {
                    arrResult[i] = double.Parse(arrTemp[i]);
                }
                return arrResult;
            }
            catch (System.Exception ex)
            {
                clsLogManager.LogError("Str2DoubleArr: {0}, Input String {1}", ex.ToString(), strInput);
                return null;
            }
        }
        public static double Interpolate(double x0, double y0, double x1, double y1, double x)
        {
            double duValue = y0 * (x - x1) / (x0 - x1) + y1 * (x - x0) / (x1 - x0);
            clsLogManager.LogReport("Line Interpolate: X={0} -> Y={1}", x, duValue);
            return duValue;
        }
        //
        public static double Linear(double x, double x0, double x1, double y0, double y1)
        {
            if ((x1 - x0) == 0)
            {
                return (y0 + y1) / 2;
            }
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }
        /*public static double[] FindRange(double[] arrRange, double duValue)
        {
            double[] arrResult = null;
            for (int i = 0; i < arrRange.Length - 1; i++)
            {
                if ((arrRange[i] >= duValue) && (arrRange[i + 1] <= duValue))
                {
                    arrResult = new double[2];
                    arrResult[0] = arrRange[i];
                    arrResult[1] = arrRange[i + 1];
                    return arrResult;
                }
            }
            return arrResult;
        }*/
        public static int[] FindRange(double[] arrRange, double duValue)
        {
            int[] arrResult = null;
            for (int i = 0; i < arrRange.Length - 1; i++)
            {
                if ((arrRange[i] <= duValue) && (arrRange[i + 1] >= duValue))
                {
                    arrResult = new int[2];
                    arrResult[0] = i;
                    arrResult[1] = i + 1;
                    return arrResult;
                }
            }
            return arrResult;
        }
        public static int[] FindRange(IEnumerable<double> arrRange, double duValue)
        {
            int[] arrResult = null;
            for (int i = 0; i < arrRange.Count() - 1; i++)
            {
                if ((arrRange.ElementAt(i) <= duValue) && (arrRange.ElementAt(i + 1) >= duValue))
                {
                    arrResult = new int[2];
                    arrResult[0] = i;
                    arrResult[1] = i + 1;
                    return arrResult;
                }
            }
            return arrResult;
        }
        public static double SpLine(List<KeyValuePair<double, double>> knownSamples, double z)
        {
            int np = knownSamples.Count;
            if (np > 1)
            {
                double[] a = new double[np];
                double x1;
                double x2;
                double y;
                double[] h = new double[np];
                for (int i = 1; i <= np - 1; i++)
                {
                    h[i] = knownSamples[i].Key - knownSamples[i - 1].Key;
                }
                if (np > 2)
                {
                    double[] sub = new double[np - 1];
                    double[] diag = new double[np - 1];
                    double[] sup = new double[np - 1];
                    for (int i = 1; i <= np - 2; i++)
                    {
                        diag[i] = (h[i] + h[i + 1]) / 3;
                        sup[i] = h[i + 1] / 6;
                        sub[i] = h[i] / 6;
                        a[i] = (knownSamples[i + 1].Value - knownSamples[i].Value) / h[i + 1] -
                               (knownSamples[i].Value - knownSamples[i - 1].Value) / h[i];
                    }
                    // SolveTridiag is a support function, see Marco Roello's original code
                    // for more information at
                    // http://www.codeproject.com/useritems/SplineInterpolation.asp
                    SolveTridiag(sub, diag, sup, ref a, np - 2);
                }

                int gap = 0;
                double previous = 0.0;
                // At the end of this iteration, "gap" will contain the index of the interval
                // between two known values, which contains the unknown z, and "previous" will
                // contain the biggest z value among the known samples, left of the unknown z
                for (int i = 0; i < knownSamples.Count; i++)
                {
                    if (knownSamples[i].Key < z && knownSamples[i].Key > previous)
                    {
                        previous = knownSamples[i].Key;
                        gap = i + 1;
                    }
                }
                x1 = z - previous;
                x2 = h[gap] - x1;
                y = ((-a[gap - 1] / 6 * (x2 + h[gap]) * x1 + knownSamples[gap - 1].Value) * x2 +
                    (-a[gap] / 6 * (x1 + h[gap]) * x2 + knownSamples[gap].Value) * x1) / h[gap];
                return y;
            }
            return 0;
        }
        private static void SolveTridiag(double[] sub, double[] diag, double[] sup, ref double[] b, int n)
        {
            /*                  solve linear system with tridiagonal n by n matrix a
								using Gaussian elimination *without* pivoting
								where   a(i,i-1) = sub[i]  for 2<=i<=n
										a(i,i)   = diag[i] for 1<=i<=n
										a(i,i+1) = sup[i]  for 1<=i<=n-1
								(the values sub[1], sup[n] are ignored)
								right hand side vector b[1:n] is overwritten with solution 
								NOTE: 1...n is used in all arrays, 0 is unused */
            int i;
            /*                  factorization and forward substitution */
            for (i = 2; i <= n; i++)
            {
                sub[i] = sub[i] / diag[i - 1];
                diag[i] = diag[i] - sub[i] * sup[i - 1];
                b[i] = b[i] - sub[i] * b[i - 1];
            }
            b[n] = b[n] / diag[n];
            for (i = n - 1; i >= 1; i--)
            {
                b[i] = (b[i] - sup[i] * b[i + 1]) / diag[i];
            }
        }
        #endregion

    }
}
