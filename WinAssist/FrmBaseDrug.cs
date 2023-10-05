

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WinAssist
{
    /// <summary>
    /// Class FrmBase.
    /// Implements the <see cref="System.Windows.Forms.Form" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(System.ComponentModel.Design.IDesigner))]
    public partial class FrmBaseDrug : Form
    {
        /// <summary>
        /// Gets or sets the hot keys.
        /// </summary>
        /// <value>The hot keys.</value>
        [Description("定义的热键列表"), Category("自定义")]
        public Dictionary<int, string> HotKeys { get; set; }
        /// <summary>
        /// Delegate HotKeyEventHandler
        /// </summary>
        /// <param name="strHotKey">The string hot key.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public delegate bool HotKeyEventHandler(string strHotKey);
        /// <summary>
        /// 热键事件
        /// </summary>
        [Description("热键事件"), Category("自定义")]
        public event HotKeyEventHandler HotKeyDown;
        #region 字段属性

        /// <summary>
        /// 失去焦点关闭
        /// </summary>
        bool _isLoseFocusClose = false;
        /// <summary>
        /// 是否重绘边框样式
        /// </summary>
        private bool _redraw = false;
        /// <summary>
        /// 是否显示圆角
        /// </summary>
        private bool _isShowRegion = false;
        /// <summary>
        /// 边圆角大小
        /// </summary>
        private int _regionRadius = 10;
        /// <summary>
        /// 边框颜色
        /// </summary>
        private Color _borderStyleColor;
        /// <summary>
        /// 边框宽度
        /// </summary>
        private int _borderStyleSize;
        /// <summary>
        /// 边框样式
        /// </summary>
        private ButtonBorderStyle _borderStyleType;
        /// <summary>
        /// 是否显示模态
        /// </summary>
        private bool _isShowMaskDialog = false;
        /// <summary>
        /// 蒙版窗体
        /// </summary>
        /// <value><c>true</c> if this instance is show mask dialog; otherwise, <c>false</c>.</value>
        [Description("是否显示蒙版窗体")]
        public bool IsShowMaskDialog
        {
            get
            {
                return this._isShowMaskDialog;
            }
            set
            {
                this._isShowMaskDialog = value;
            }
        }
        /// <summary>
        /// 边框宽度
        /// </summary>
        /// <value>The size of the border style.</value>
        [Description("边框宽度")]
        public int BorderStyleSize
        {
            get
            {
                return this._borderStyleSize;
            }
            set
            {
                this._borderStyleSize = value;
            }
        }
        /// <summary>
        /// 边框颜色
        /// </summary>
        /// <value>The color of the border style.</value>
        [Description("边框颜色")]
        public Color BorderStyleColor
        {
            get
            {
                return this._borderStyleColor;
            }
            set
            {
                this._borderStyleColor = value;
            }
        }
        /// <summary>
        /// 边框样式
        /// </summary>
        /// <value>The type of the border style.</value>
        [Description("边框样式")]
        public ButtonBorderStyle BorderStyleType
        {
            get
            {
                return this._borderStyleType;
            }
            set
            {
                this._borderStyleType = value;
            }
        }
        /// <summary>
        /// 边框圆角
        /// </summary>
        /// <value>The region radius.</value>
        [Description("边框圆角")]
        public int RegionRadius
        {
            get
            {
                return this._regionRadius;
            }
            set
            {
                this._regionRadius = Math.Max(value, 1);
            }
        }
        /// <summary>
        /// 是否显示自定义绘制内容
        /// </summary>
        /// <value><c>true</c> if this instance is show region; otherwise, <c>false</c>.</value>
        [Description("是否显示自定义绘制内容")]
        public bool IsShowRegion
        {
            get
            {
                return this._isShowRegion;
            }
            set
            {
                this._isShowRegion = value;
            }
        }
        /// <summary>
        /// 是否显示重绘边框
        /// </summary>
        /// <value><c>true</c> if redraw; otherwise, <c>false</c>.</value>
        [Description("是否显示重绘边框")]
        public bool Redraw
        {
            get
            {
                return this._redraw;
            }
            set
            {
                this._redraw = value;
            }
        }

        /// <summary>
        /// The is full size
        /// </summary>
        private bool _isFullSize = true;
        /// <summary>
        /// 是否全屏
        /// </summary>
        /// <value><c>true</c> if this instance is full size; otherwise, <c>false</c>.</value>
        [Description("是否全屏")]
        public bool IsFullSize
        {
            get { return _isFullSize; }
            set { _isFullSize = value; }
        }
        /// <summary>
        /// 失去焦点自动关闭
        /// </summary>
        /// <value><c>true</c> if this instance is lose focus close; otherwise, <c>false</c>.</value>
        [Description("失去焦点自动关闭")]
        public bool IsLoseFocusClose
        {
            get
            {
                return this._isLoseFocusClose;
            }
            set
            {
                this._isLoseFocusClose = value;
            }
        }
        #endregion

        /// <summary>
        /// Gets a value indicating whether this instance is desing mode.
        /// </summary>
        /// <value><c>true</c> if this instance is desing mode; otherwise, <c>false</c>.</value>
        private bool IsDesingMode
        {
            get
            {
                bool ReturnFlag = false;
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                    ReturnFlag = true;
                else if (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv")
                    ReturnFlag = true;
                return ReturnFlag;
            }
        }

        #region 初始化
        /// <summary>
        /// Initializes a new instance of the <see cref="FrmBase" /> class.
        /// </summary>
        public FrmBaseDrug()
        {
            InitializeComponent();
            base.SetStyle(ControlStyles.UserPaint, true);
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            base.SetStyle(ControlStyles.DoubleBuffer, true);
            //base.HandleCreated += new EventHandler(this.FrmBase_HandleCreated);
            //base.HandleDestroyed += new EventHandler(this.FrmBase_HandleDestroyed);        
            this.KeyDown += FrmBase_KeyDown;
          
        }

        /// <summary>
        /// Handles the FormClosing event of the FrmBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs" /> instance containing the event data.</param>
        void FrmBase_FormClosing(object sender, FormClosingEventArgs e)
        {
           
        }


        /// <summary>
        /// Handles the Load event of the FrmBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void FrmBase_Load(object sender, EventArgs e)
        {
            UpdateControl();
           
            SetWindowRegion();
        }

        #endregion

        #region 方法区

       
        public void UpdateControl()
        {
            lbtiptext.Text = this.Text;
            lbtiptext.Location=new Point( paltitle.Location.X+10,paltitle.Location.Y+3);
            this.BackColor = Color.Gray;
            foreach (Control con in this.Controls)
            {
                if (con is Label)
                {
                    SetLabel(con as Label);
                }
                if (con is Button)
                {
                    Setbtn(con as Button);
                }
                if (con is DataGridView)
                {
                    dataGridView(con as DataGridView);
                }
                if (con is GroupBox)
                {
                    Setframe(con as GroupBox);
                }
                if (con is TabControl)
                {
                    tabControl(con as TabControl);
                }
            }
        }
        public void tabControl(TabControl tabControl1)
        {
            foreach (TabPage tps in tabControl1.TabPages)
            {
                tps.BackColor = Color.Gray;
                foreach (Control contrl in tps.Controls)
                {
                    if (contrl is GroupBox)
                    {
                        Setframe(contrl as GroupBox);
                    }
                    if (contrl is Panel)
                    {
                        Setpal(contrl as Panel);
                    }
                    if (contrl is Button)
                    {
                        Setbtn(contrl as Button);
                    }
                    if (contrl is DataGridView)
                    {
                        dataGridView(contrl as DataGridView);
                    }
                    if (contrl is GroupBox)
                    {
                        Setframe(contrl as GroupBox);
                    }
                    if (contrl is Label)
                    {
                        SetLabel(contrl as Label);
                    }
                }
            }
        }

        public void Setpal(Panel pl)
        {
            pl.BackColor = Color.Gray;
            foreach (Control contrl in pl.Controls)
            {
                if (contrl is Label)
                {
                    SetLabel(contrl as Label);
                }
                if (contrl is Button)
                {
                    Setbtn(contrl as Button);
                }
                if (contrl is DataGridView)
                {
                    dataGridView(contrl as DataGridView);
                }
            }
        }


        public void Setframe(GroupBox gb)
        {
            gb.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            gb.ForeColor = System.Drawing.Color.Gray;
            foreach (Control con in gb.Controls)
            {
                if (con is Label)
                {
                    SetLabel(con as Label);
                }
                if (con is Button)
                {
                    Setbtn(con as Button);
                }
                if (con is DataGridView)
                {
                    dataGridView(con as DataGridView);
                }
                if (con is GroupBox)
                {
                    Setframe(con as GroupBox);
                }
                if (con is TabControl)
                {
                    tabControl(con as TabControl);
                }


            }
           // gb.Font = 
        }
        private void dataGridView(DataGridView dataGridView)
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightCyan;

            dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView.RowsDefaultCellStyle.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridView.RowsDefaultCellStyle.ForeColor = System.Drawing.Color.Gray;
            dataGridView.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.BackgroundColor = System.Drawing.Color.White;
            dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;//211, 223, 240
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
           
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.GridColor = System.Drawing.SystemColors.GradientInactiveCaption;
            dataGridView.ReadOnly = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowTemplate.Height = 25;
            dataGridView.RowTemplate.ReadOnly = true;

            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
      

        public void SetLabel(Label lb)
        {
            lb.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            lb.ForeColor = Color.WhiteSmoke;
        }
        public void Setbtn(Button btn)
        {
           // btn.BackgroundImage = global::ARKDuMaCar_New_Main.Properties.Resources.蓝色;
            btn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.CadetBlue;
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            btn.ForeColor = Color.WhiteSmoke;
        }

        /// <summary>
        /// Handles the OnMouseActivity event of the hook control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        void hook_OnMouseActivity(object sender, MouseEventArgs e)
        {
            try
            {
                if (this._isLoseFocusClose && e.Clicks > 0)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left || e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        if (!this.IsDisposed)
                        {
                            if (!this.ClientRectangle.Contains(this.PointToClient(e.Location)))
                            {
                                base.Close();
                            }
                        }
                    }
                }
            }
            catch { }
        }


        /// <summary>
        /// 全屏
        /// </summary>
        public void SetFullSize()
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }
        /// <summary>
        /// Does the escape.
        /// </summary>
        public virtual void DoEsc()
        {
            base.Close();
        }

        /// <summary>
        /// Does the enter.
        /// </summary>
        protected virtual void DoEnter()
        {
        }

      
        /// <summary>
        /// 将窗体显示为模式对话框，并将当前活动窗口设置为它的所有者。
        /// </summary>
        /// <returns><see cref="T:System.Windows.Forms.DialogResult" /> 值之一。</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public new DialogResult ShowDialog()
        {
            return base.ShowDialog();
        }
        #endregion

        #region 事件区


        /// <summary>
        /// 关闭时发生
        /// </summary>
        /// <param name="e">一个包含事件数据的 <see cref="T:System.EventArgs" />。</param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
           
        }

        /// <summary>
        /// 快捷键
        /// </summary>
        /// <param name="msg">通过引用传递的 <see cref="T:System.Windows.Forms.Message" />，它表示要处理的 Win32 消息。</param>
        /// <param name="keyData"><see cref="T:System.Windows.Forms.Keys" /> 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理。</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            int num = 256;
            int num2 = 260;
            bool result;
            if (msg.Msg == num | msg.Msg == num2)
            {
                if (keyData == (Keys)262259)
                {
                    result = true;
                    return result;
                }
                if (keyData != Keys.Enter)
                {
                    if (keyData == Keys.Escape)
                    {
                        this.DoEsc();
                    }
                }
                else
                {
                    this.DoEnter();
                }
            }
            result = false;
            if (result)
                return result;
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Handles the KeyDown event of the FrmBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        protected void FrmBase_KeyDown(object sender, KeyEventArgs e)
        {
            if (HotKeyDown != null && HotKeys != null)
            {
                bool blnCtrl = false;
                bool blnAlt = false;
                bool blnShift = false;
                if (e.Control)
                    blnCtrl = true;
                if (e.Alt)
                    blnAlt = true;
                if (e.Shift)
                    blnShift = true;
                if (HotKeys.ContainsKey(e.KeyValue))
                {
                    string strKey = string.Empty;
                    if (blnCtrl)
                    {
                        strKey += "Ctrl+";
                    }
                    if (blnAlt)
                    {
                        strKey += "Alt+";
                    }
                    if (blnShift)
                    {
                        strKey += "Shift+";
                    }
                    strKey += HotKeys[e.KeyValue];

                    if (HotKeyDown(strKey))
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                }
            }
        }

        /// <summary>
        /// 重绘事件
        /// </summary>
        /// <param name="e">包含事件数据的 <see cref="T:System.Windows.Forms.PaintEventArgs" />。</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this._isShowRegion)
            {
                this.SetWindowRegion();
            }
            base.OnPaint(e);
            if (this._redraw)
            {
                ControlPaint.DrawBorder(e.Graphics, base.ClientRectangle, this._borderStyleColor, this._borderStyleSize, this._borderStyleType, this._borderStyleColor, this._borderStyleSize, this._borderStyleType, this._borderStyleColor, this._borderStyleSize, this._borderStyleType, this._borderStyleColor, this._borderStyleSize, this._borderStyleType);
            }
        }
        #endregion


        #region 窗体拖动    English:Form drag
        /// <summary>
        /// Releases the capture.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="wMsg">The w MSG.</param>
        /// <param name="wParam">The w parameter.</param>
        /// <param name="lParam">The l parameter.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        /// <summary>
        /// The wm syscommand
        /// </summary>
        public const int WM_SYSCOMMAND = 0x0112;
        /// <summary>
        /// The sc move
        /// </summary>
        public const int SC_MOVE = 0xF010;
        /// <summary>
        /// The htcaption
        /// </summary>
        public const int HTCAPTION = 0x0002;

        /// <summary>
        /// 通过Windows的API控制窗体的拖动
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        public static void MouseDown(IntPtr hwnd)
        {
            ReleaseCapture();
            SendMessage(hwnd, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }
        #endregion

        /// <summary>
        /// 在构造函数中调用设置窗体移动
        /// </summary>
        /// <param name="cs">The cs.</param>
        protected void InitFormMove(params Control[] cs)
        {
            foreach (Control c in cs)
            {
                if (c != null && !c.IsDisposed)
                    c.MouseDown += c_MouseDown;
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the c control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        void c_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDown(this.Handle);
        }

        public bool IsCls = true;
        private void SetFuntion(object sender, EventArgs e)
        {
            Control _Control = sender as Control;
            switch (_Control.Name)
            {
                case "lbClose":
                    DoEsc();
                    //if (IsCls)
                    //{
                    //    this.Close();
                    //}
                    //else
                    //{
                    //    DoEsc();
                    //}
                    break;
            }
            
        }
       
        

        public void SetWindowRegion()
        {
            System.Drawing.Drawing2D.GraphicsPath FormPath;
            FormPath = new System.Drawing.Drawing2D.GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            FormPath = GetRoundedRectPathp(rect, 6);
            this.Region = new Region(FormPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect">窗体大小</param>
        /// <param name="radius">圆角大小</param>
        /// <returns></returns>
        private GraphicsPath GetRoundedRectPathp(Rectangle rect, int radius)
        {
            int diameter = radius;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
            GraphicsPath path = new GraphicsPath();

            path.AddArc(arcRect, 180, 90);//左上角

            arcRect.X = rect.Right - diameter;//右上角
            path.AddArc(arcRect, 270, 90);

            arcRect.Y = rect.Bottom - diameter;// 右下角
            path.AddArc(arcRect, 0, 90);

            arcRect.X = rect.Left;// 左下角
            path.AddArc(arcRect, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
