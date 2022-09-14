using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace VHCRTestTask.TypeConvert
{
    public class clsFilterBand : StringConverter
    {
         public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> lstValue = new List<string>();
            lstValue.Add("30 - 40MHz");
            lstValue.Add("40 - 52MHz");
            lstValue.Add("52 - 66MHz");
            lstValue.Add("66 - 88MHz");
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    public class clsResponseRange : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> lstValue = new List<string>();
            lstValue.Add("0.3 - 3kHz");
            lstValue.Add("150Hz");
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
