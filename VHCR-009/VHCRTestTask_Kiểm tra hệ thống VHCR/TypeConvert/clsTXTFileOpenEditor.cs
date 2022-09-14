using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Windows.Forms;
using FileDialogs;
using System.IO;

namespace VHCRTestTask.TypeConvert
{
    public class clsTXTFileOpenEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle
           GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context,
                                IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService wfes =
               provider.GetService(typeof(IWindowsFormsEditorService)) as
               IWindowsFormsEditorService;

            if (wfes != null)
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;
                if(string.IsNullOrEmpty(value as string))
                    dlg.InitialDirectory = Application.StartupPath;
                else
                {
                    string strDicPath = Path.GetDirectoryName(value as string);
                    dlg.InitialDirectory = strDicPath;
                }
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    value = dlg.FileName;
                }
                dlg.Dispose();
            }
            return value;
        }
    }
}
