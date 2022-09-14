using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace VHCRTestTask.TypeConvert
{
    public class clsAttennaType : StringConverter
    {
         public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> lstValue = new List<string>();
            lstValue.Add("Ăng ten 2 cực");
            lstValue.Add("Ăng ten chếch");
            lstValue.Add("Ăng ten cần");
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
