using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace VHCRTestTask.TypeConvert
{
    public class clsLowPassFilter : DoubleConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<double> lstValue = new List<double>();
            lstValue.Add(3.4);
            lstValue.Add(15);
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    public class clsHighPassFilter : DoubleConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<double> lstValue = new List<double>();
            lstValue.Add(300);
            lstValue.Add(100);
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
