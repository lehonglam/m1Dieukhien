using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using ExternLib;

namespace VHCRTestTask
{
    public class SaveAtt : Attribute
    {
        public SaveAtt()
        {

        }
    }
    public static class clsSaveTaskParaHelper
    {
        public static void SaveObj2XmlNodeAtt(System.Xml.XmlNode ObjNode, object obj)
        {
            SaveAtt token = null;
            object[] tokenList = null;
            PropertyInfo[] pro = obj.GetType().GetProperties();
            string strValue = string.Empty;
            TypeConverter t = null;
            foreach (PropertyInfo pi in pro)
            {
                token = null;
                tokenList = pi.GetCustomAttributes(typeof(SaveAtt), false);
                for (int i = 0; i < tokenList.Length; i++)
                {
                    if (tokenList[i] is SaveAtt)
                    {
                        token = tokenList[i] as SaveAtt;
                        break;
                    }
                }
                if (token == null) continue;
                if (!pi.CanRead) continue;
                t = TypeDescriptor.GetConverter(pi.GetValue(obj, null));
                if (t.CanConvertTo(typeof(string)))
                {
                    strValue = t.ConvertToString(pi.GetValue(obj, null));
                    XmlHelper.CreateAttribute(ObjNode, pi.Name, strValue);
                }
            }
        }
        public static void ParseObjParameter2XmlNode(System.Xml.XmlNode ObjNode, object obj)
        {
            PropertyInfo[] pro = obj.GetType().GetProperties();
            string strValue = string.Empty;
            TypeConverter t = null;
            foreach (PropertyInfo pi in pro)
            {
                if (!pi.CanWrite)
                    continue;
                if (ObjNode.ChildNodes[0].Attributes[pi.Name] == null)
                    continue;
                t = TypeDescriptor.GetConverter(pi.GetValue(obj, null));
                pi.SetValue(obj, t.ConvertFromString(ObjNode.ChildNodes[0].Attributes[pi.Name].Value), null);
            }
        }
        public static void CopyObjProperties(object scrObj, object destObj)
        {
            PropertyInfo[] scrPros = scrObj.GetType().GetProperties();
            PropertyInfo[] destPros = destObj.GetType().GetProperties();
            foreach (PropertyInfo pi in destPros)
            {
                if (!pi.CanWrite) continue;
                for (int i = 0; i < scrPros.Length; i++ )
                {
                    if (scrPros[i].Name == pi.Name)
                    {
                        pi.SetValue(destObj, scrPros[i].GetValue(scrObj, null), null);
                        break;
                    }
                }
            }
        }
    }
}
