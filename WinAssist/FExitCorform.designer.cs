namespace ARKDuMaCar_New_Main.SystemA
{
    partial class FExitCorform
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txbPWD = new System.Windows.Forms.TextBox();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.uszsRcs1 = new ARKDuMaCar_New_Main.Frame.USZSRcs();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "请输入权限密码：";
            // 
            // txbPWD
            // 
            this.txbPWD.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txbPWD.Location = new System.Drawing.Point(136, 18);
            this.txbPWD.Name = "txbPWD";
            this.txbPWD.PasswordChar = '*';
            this.txbPWD.Size = new System.Drawing.Size(126, 26);
            this.txbPWD.TabIndex = 1;
            this.txbPWD.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txbPWD_KeyPress);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(282, 22);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 36);
            this.btOK.TabIndex = 2;
            this.btOK.Text = "确定(&O)";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(280, 88);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(77, 38);
            this.btCancel.TabIndex = 3;
            this.btCancel.Text = "取消(&C)";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // uszsRcs1
            // 
            this.uszsRcs1.Location = new System.Drawing.Point(33, 57);
            this.uszsRcs1.Name = "uszsRcs1";
            this.uszsRcs1.Size = new System.Drawing.Size(232, 196);
            this.uszsRcs1.TabIndex = 4;
            // 
            // FExitCorform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Turquoise;
            this.ClientSize = new System.Drawing.Size(378, 265);
            this.Controls.Add(this.uszsRcs1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.txbPWD);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FExitCorform";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "配置管理确认窗口";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        public System.Windows.Forms.TextBox txbPWD;
        public Frame.USZSRcs uszsRcs1;
    }
}