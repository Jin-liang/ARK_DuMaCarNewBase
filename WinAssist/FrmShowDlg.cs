using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinAssist
{
    public partial class FrmShowDlg : Form
    {

        public FrmShowDlg(string message)
        {
            InitializeComponent();
            if (message.Length > 30)
                label2.Text = string.Format("{0}\n{1}", message.Substring(0, 28), message.Substring(28, message.Length - 28));// ;
            else
                label2.Text = message;
        }
        int k = 60;
        private void timer1_Tick(object sender, EventArgs e)
        {
            k -= 1;
            button6.Text = string.Format("确  定 {0}", k);
            if (k < 1) DialogResult = DialogResult.Cancel;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnDrugOut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
