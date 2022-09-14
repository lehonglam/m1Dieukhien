using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace VHCRTestTask.TypeConvert
{
    public class clsDutMode : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> lstValue = new List<string>();
            lstValue.Add("USB");
            lstValue.Add("LSB");
            lstValue.Add("CW");
            lstValue.Add("NCW");
            lstValue.Add("TEST");
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
