using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ARKDuMaCar_New_Main.SystemA
{
    public partial class FExitCorform : Form
    {
        public FExitCorform()
        {
            InitializeComponent();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (this.txbPWD.Text == "1")
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void txbPWD_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13) btOK_Click(btOK, null);
        }
    }
}
