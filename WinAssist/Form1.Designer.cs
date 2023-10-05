namespace WinAssist
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtVerID = new System.Windows.Forms.TextBox();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.btnDevManager = new System.Windows.Forms.Button();
            this.btnAppSet = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsOpenAssist = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsReSetAssist = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCHide = new System.Windows.Forms.Button();
            this.txtPortName = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.textBox1.Location = new System.Drawing.Point(240, 269);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(331, 35);
            this.textBox1.TabIndex = 28;
            this.textBox1.Text = "18";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(69, 274);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 28);
            this.label2.TabIndex = 27;
            this.label2.Text = "显示字体大小";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(69, 188);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 28);
            this.label1.TabIndex = 25;
            this.label1.Text = "设备型号:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(73, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 28);
            this.label3.TabIndex = 22;
            this.label3.Text = "标题：";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(69, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 28);
            this.label4.TabIndex = 21;
            this.label4.Text = "版本号：";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtVerID
            // 
            this.txtVerID.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.txtVerID.Location = new System.Drawing.Point(240, 120);
            this.txtVerID.Name = "txtVerID";
            this.txtVerID.Size = new System.Drawing.Size(331, 35);
            this.txtVerID.TabIndex = 23;
            this.txtVerID.Text = "V1.50";
            // 
            // txtTitle
            // 
            this.txtTitle.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.txtTitle.Location = new System.Drawing.Point(240, 56);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(331, 35);
            this.txtTitle.TabIndex = 24;
            this.txtTitle.Text = "毒麻小车智能管理平台";
            // 
            // btnDevManager
            // 
            this.btnDevManager.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDevManager.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.btnDevManager.ForeColor = System.Drawing.Color.White;
            this.btnDevManager.Location = new System.Drawing.Point(240, 365);
            this.btnDevManager.Name = "btnDevManager";
            this.btnDevManager.Size = new System.Drawing.Size(149, 64);
            this.btnDevManager.TabIndex = 3;
            this.btnDevManager.Text = "硬件管理";
            this.btnDevManager.UseVisualStyleBackColor = true;
            this.btnDevManager.Click += new System.EventHandler(this.FunctionSet);
            // 
            // btnAppSet
            // 
            this.btnAppSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAppSet.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.btnAppSet.ForeColor = System.Drawing.Color.White;
            this.btnAppSet.Location = new System.Drawing.Point(66, 365);
            this.btnAppSet.Name = "btnAppSet";
            this.btnAppSet.Size = new System.Drawing.Size(133, 64);
            this.btnAppSet.TabIndex = 0;
            this.btnAppSet.Text = "更新配置";
            this.btnAppSet.UseVisualStyleBackColor = true;
            this.btnAppSet.Click += new System.EventHandler(this.FunctionSet);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "设备管理助手";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsOpenAssist,
            this.toolStripMenuItem1,
            this.tsReSetAssist});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(125, 54);
            // 
            // tsOpenAssist
            // 
            this.tsOpenAssist.Name = "tsOpenAssist";
            this.tsOpenAssist.Size = new System.Drawing.Size(124, 22);
            this.tsOpenAssist.Text = "打开程序";
            this.tsOpenAssist.Click += new System.EventHandler(this.ContextFuntion);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(121, 6);
            // 
            // tsReSetAssist
            // 
            this.tsReSetAssist.Name = "tsReSetAssist";
            this.tsReSetAssist.Size = new System.Drawing.Size(124, 22);
            this.tsReSetAssist.Text = "退出程序";
            this.tsReSetAssist.Click += new System.EventHandler(this.tsReSetAssist_Click);
            // 
            // btnCHide
            // 
            this.btnCHide.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCHide.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.btnCHide.ForeColor = System.Drawing.Color.White;
            this.btnCHide.Location = new System.Drawing.Point(422, 365);
            this.btnCHide.Name = "btnCHide";
            this.btnCHide.Size = new System.Drawing.Size(149, 64);
            this.btnCHide.TabIndex = 30;
            this.btnCHide.Text = "隐藏";
            this.btnCHide.UseVisualStyleBackColor = true;
            this.btnCHide.Click += new System.EventHandler(this.FunctionSet);
            // 
            // txtPortName
            // 
            this.txtPortName.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold);
            this.txtPortName.Location = new System.Drawing.Point(240, 188);
            this.txtPortName.Name = "txtPortName";
            this.txtPortName.Size = new System.Drawing.Size(331, 35);
            this.txtPortName.TabIndex = 31;
            this.txtPortName.Text = "Y310";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(716, 479);
            this.ControlBox = false;
            this.Controls.Add(this.txtPortName);
            this.Controls.Add(this.btnCHide);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnDevManager);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnAppSet);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtVerID);
            this.Controls.Add(this.label4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "毒麻小车程序助手";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDevManager;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtVerID;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnAppSet;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsOpenAssist;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tsReSetAssist;
        private System.Windows.Forms.Button btnCHide;
        private System.Windows.Forms.TextBox txtPortName;

    }
}

