
namespace WinAssist
{
    /// <summary>
    /// Class FrmBase.
    /// Implements the <see cref="System.Windows.Forms.Form" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    partial class FrmBaseDrug
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBaseDrug));
            this.paltitle = new System.Windows.Forms.Panel();
            this.lbtiptext = new System.Windows.Forms.Label();
            this.lbClose = new System.Windows.Forms.Label();
            this.paltitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // paltitle
            // 
            this.paltitle.BackgroundImage = global::WinAssist.Properties.Resources.导航栏;
            this.paltitle.Controls.Add(this.lbtiptext);
            this.paltitle.Controls.Add(this.lbClose);
            this.paltitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.paltitle.Location = new System.Drawing.Point(0, 0);
            this.paltitle.Name = "paltitle";
            this.paltitle.Size = new System.Drawing.Size(1084, 45);
            this.paltitle.TabIndex = 2;
            // 
            // lbtiptext
            // 
            this.lbtiptext.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbtiptext.AutoSize = true;
            this.lbtiptext.BackColor = System.Drawing.Color.Transparent;
            this.lbtiptext.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold);
            this.lbtiptext.ForeColor = System.Drawing.Color.White;
            this.lbtiptext.Location = new System.Drawing.Point(196, 9);
            this.lbtiptext.Name = "lbtiptext";
            this.lbtiptext.Size = new System.Drawing.Size(0, 26);
            this.lbtiptext.TabIndex = 42;
            // 
            // lbClose
            // 
            this.lbClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbClose.AutoSize = true;
            this.lbClose.BackColor = System.Drawing.Color.Transparent;
            this.lbClose.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold);
            this.lbClose.ForeColor = System.Drawing.Color.White;
            this.lbClose.Location = new System.Drawing.Point(1022, 8);
            this.lbClose.Name = "lbClose";
            this.lbClose.Size = new System.Drawing.Size(50, 26);
            this.lbClose.TabIndex = 41;
            this.lbClose.Text = "返回";
            this.lbClose.Click += new System.EventHandler(this.SetFuntion);
            // 
            // FrmBaseDrug
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.ClientSize = new System.Drawing.Size(1084, 709);
            this.Controls.Add(this.paltitle);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmBaseDrug";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmBase";
            this.Load += new System.EventHandler(this.FrmBase_Load);
            this.paltitle.ResumeLayout(false);
            this.paltitle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel paltitle;
        private System.Windows.Forms.Label lbClose;
        private System.Windows.Forms.Label lbtiptext;
    }
}