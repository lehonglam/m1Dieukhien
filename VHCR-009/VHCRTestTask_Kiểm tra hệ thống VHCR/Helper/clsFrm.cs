using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRP712TestTask.Helper
{
    public partial class clsFrm : Form
    {
        public bool Result = false;
        public clsFrm()
        {
            InitializeComponent();
            Result = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Result = true;
            Close();
            Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Result = false;
            Close();
            Dispose();
        }

        public DialogResult ShowDialog(string msgtext)
        {
            label2.Text = msgtext;
            ShowDialog();
            return DialogResult;
        }

    }
}
