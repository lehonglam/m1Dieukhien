using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DigitizerBoardTestTask.TypeConverters
{
    public class clsDigitizerBoardPortTc : StringConverter
    {
         public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> lstValue = new List<string>();
            lstValue.Add("SFP");
            lstValue.Add("FMC");
            lstValue.Add("GPS");
            lstValue.Add("RS485");
            lstValue.Add("RS485 Pantilt");
            lstValue.Add("USB");
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
