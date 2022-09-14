using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
namespace VHCRTestTask.TypeConvert
{
    public class clsListComport : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<WinDeviceInfo>  ComPorts = clsWin32DeviceMgmt.GetAllCOMPorts();
            List<string> lstValue = new List<string>();
            for (int i = 0; i < ComPorts.Count; i++)
            {
                lstValue.Add(ComPorts[i].name);
            }
            StandardValuesCollection sv = new StandardValuesCollection(lstValue);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    public class clsList_IDMeasurement : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            //static string[] list_IDMeasurement = { "VRS631S.19.03.ATU.015", "VRS631S.19.03.ATU.016", "VRS631S.19.03.VRS631S.024", "VRS631S.19.03.VRS631S.025", "VRS631S.19.03.DKX631S", "VRS631S.19.03.DKX631S.019" };
            List<string> list_IDMeasurement = new List<string>();
            list_IDMeasurement.Add("VRS631S.19.03.ATU.015");
            list_IDMeasurement.Add("VRS631S.19.03.ATU.016");
            list_IDMeasurement.Add("VRS631S.19.03.VRS631S.024");
            list_IDMeasurement.Add("VRS631S.19.03.VRS631S.025");
            list_IDMeasurement.Add("VRS631S.19.03.DKX631S.018");
            list_IDMeasurement.Add("VRS631S.19.03.DKX631S.019");
            StandardValuesCollection sv = new StandardValuesCollection(list_IDMeasurement);
            return sv;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    public class clsListNameCOMPort : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            string[] ListNamePort = SerialPort.GetPortNames();
            List<string> lstValue = new List<string>();
            for (int i = 0; i < ListNamePort.Length; i++)
            {
                lstValue.Add(ListNamePort[i]);
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
