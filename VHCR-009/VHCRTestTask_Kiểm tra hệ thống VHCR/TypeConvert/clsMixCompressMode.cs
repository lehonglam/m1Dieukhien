/*  ----------------------------------------------------------------------------
 *  Company:	Viettel Group
 *  ----------------------------------------------------------------------------
 *  Project:	Viettel Factory Test Tool
 *  ----------------------------------------------------------------------------
 *  File:       clsMixCompressMode.cs
 *  Author:     Ngo Ngoc Khai
 *  Reviser:	NO
 *  Copyright:	Viettel Group
 *  Description:
 *  History:
 *		-> 18/06/2020-Ngo Ngoc Khai-Created
 *  ----------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace VHCRTestTask.TypeConvert
{
    class clsMixCompressMode : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> lstValue = new List<string>();
            lstValue.Add("Trong Băng");
            lstValue.Add("Dải rộng");
            lstValue.Add("Sóng Tạp");
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
