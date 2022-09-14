using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CFTWinAppCore.DeviceManager;

namespace VHCRTestTask.TypeConvert
{
    public class clsFreqTypeConv
    {
        public double FreqTypeConv(double Freq, FreqUnit InF, FreqUnit OutF)
        {
            double duFreqOut = Freq;
            switch (OutF)
            {
                case FreqUnit.Hz:
                    {
                        switch (InF)
                        {
                            case FreqUnit.Hz:
                                duFreqOut = Freq * 1e0;
                                break;
                            case FreqUnit.kHz:
                                duFreqOut = Freq * 1e3;
                                break;
                            case FreqUnit.MHz:
                                duFreqOut = Freq * 1e6;
                                break;
                            case FreqUnit.GHz:
                                duFreqOut = Freq * 1e9;
                                break;
                        }
                        break;
                    }
                case FreqUnit.kHz:
                    {
                        switch (InF)
                        {
                            case FreqUnit.Hz:
                                duFreqOut = Freq / 1e3;
                                break;
                            case FreqUnit.kHz:
                                duFreqOut = Freq * 1e0;
                                break;
                            case FreqUnit.MHz:
                                duFreqOut = Freq * 1e3;
                                break;
                            case FreqUnit.GHz:
                                duFreqOut = Freq * 1e6;
                                break;
                        }
                        break;
                    }
                case FreqUnit.MHz:
                    {
                        switch (InF)
                        {
                            case FreqUnit.Hz:
                                duFreqOut = Freq / 1e6;
                                break;
                            case FreqUnit.kHz:
                                duFreqOut = Freq / 1e3;
                                break;
                            case FreqUnit.MHz:
                                duFreqOut = Freq * 1e0;
                                break;
                            case FreqUnit.GHz:
                                duFreqOut = Freq * 1e3;
                                break;
                        }
                        break;
                    }
                case FreqUnit.GHz:
                    {
                        switch (InF)
                        {
                            case FreqUnit.Hz:
                                duFreqOut = Freq / 1e9;
                                break;
                            case FreqUnit.kHz:
                                duFreqOut = Freq / 1e6;
                                break;
                            case FreqUnit.MHz:
                                duFreqOut = Freq / 1e3;
                                break;
                            case FreqUnit.GHz:
                                duFreqOut = Freq * 1e0;
                                break;
                        }
                        break;
                    }
            }
            return duFreqOut;
        }
    }
}
