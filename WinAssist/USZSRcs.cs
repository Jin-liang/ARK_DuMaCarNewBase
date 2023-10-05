using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ARKDuMaCar_New_Main.Frame
{
    public partial class USZSRcs : UserControl
    {

        const int WM_CHAR = 0x0102;

        [DllImport("User32.dll")]
       // private static extern int SetActiveWindow(int hwnd);
        static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public USZSRcs()
        {
            InitializeComponent();
        }
        public void Setbtn(Button btn)
        {
           // btn.BackgroundImage = global::ARKDuMaCar_New_Main.Properties.Resources.蓝色;
            btn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            btn.ForeColor = Color.WhiteSmoke;
        }

        public Action<string> SetText;

        public IntPtr FHandle;
        private void doneData(Button btncaption)
        {                        
            string  SCaption=btncaption.Text ;
            IntPtr getptr = SetActiveWindow(FHandle);
            if (SCaption != "")
            // Sendkeys(PChar(SCaption), false);
            {
                SendMessage(FHandle, WM_CHAR, SCaption[0], 1);
            }
            else
            {
                SendMessage(FHandle, WM_CHAR, 8, 1);
            }
            if (SetText != null)
            {
                SetText(SCaption);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }

        private void BTNBK_Click(object sender, EventArgs e)
        {
            doneData((Button)sender);
        }
    }

    
}
