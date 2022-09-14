using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DigitizerBoardTestTask.TypeConverters
{
    public class clsComPortNameTc : StringConverter
    {
         public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> lstValue = new List<string>();
            string[] strArr = System.IO.Ports.SerialPort.GetPortNames();
            foreach(string str in strArr)
            {
                lstValue.Add(str);
            }
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
