using ARKDuMaCar_New_Main.SystemA;
using DataDal;
using DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinAssist.Comm;

namespace WinAssist
{
    public partial class Form1 : Form
    {
        string strtype = "";
      
        ClientServer _RunClientServer;
        MessageContext _MessageContext;
        string ClientState = "";
        //读取配置文件
        public Form1()
        {
            InitializeComponent();
            this.Visible = false;
            this.WindowState = FormWindowState.Minimized;
            this.notifyIcon1.Visible = true;
            initTitle();
        }

        public void initTitle()
        {
            this.txtPortName.Text = GetConnectionValue("DevVersion");
            this.txtTitle.Text =  GetConnectionValue("VerTitle");
            this.txtVerID.Text = GetConnectionValue("Version");
        }

        public void UpdateTitle()
        {
            UpdateConnectionStringsConfig("DevVersion", this.txtPortName.Text);
            UpdateConnectionStringsConfig("VerTitle", this.txtTitle.Text);
            UpdateConnectionStringsConfig("Version", this.txtVerID.Text);
        }
        public void InitComState(string Val)
        {
            StrCom.ARKShowDlg(Val);
        }
        //系统信息
        public void IComState(string Val)
        {
            switch (Val)
            {
                case "关闭程序":
                    ClientState = "程序已关闭";
                    if (this._RunClientServer != null)
                    {
                        this._RunClientServer.StopServer();
                    }
                    this.Close();
                    break;
                case "启动程序":
                    ClientState = "程序已启动";
                    break;
            }
        }
        //配置信息
        private void FunctionSet(object sender, EventArgs e)
        {

            Control _Control = sender as Control;
            switch (_Control.Name)
            {
                case "btnCHide":
                    this.Visible = false;
                    this.WindowState = FormWindowState.Minimized;
                    this.notifyIcon1.Visible = true;
                    break;
              
                case "btnDevManager":
                    FExitCorform fe = new FExitCorform();
                    fe.uszsRcs1.FHandle = fe.txbPWD.Handle;
                    if (DialogResult.OK == fe.ShowDialog())
                    {
                        ShowsystemManger();
                    }
                    fe.Dispose();
                    fe = null;

                    break;
                case "btnAppSet":
                   //  _RunClientServer.SendClientCmd("testsss");
                    UpdateTitle();
                    MessageBox.Show("配置更新成功！");
                    break;
            }
        }

        //
        public void ShowsystemManger()
        {
            if( _RunClientServer !=null)
            {
                _RunClientServer.StopServer();
            }
            systemManger _systemManger = new systemManger();
            _systemManger.Devicetype = strtype;
            _systemManger.ShowInTaskbar = false;
            _systemManger.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
                IpcServerChannel channel = new IpcServerChannel("ArkServerChannel");
                ChannelServices.RegisterChannel(channel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemoteObject), "RemoteObject", WellKnownObjectMode.SingleCall);

                RemoteObject.SendMessageEvent += new SendMessageHandler(TestMessageMarshal_SendMessageEvent);
                RemoteObject.Version = this.txtVerID.Text;
                RemoteObject.VerTitle = this.txtTitle.Text;
                RemoteObject.DevVersion = this.txtPortName.Text;
               
            try
            {
                //接收客户端的信息
                
                strtype = txtPortName.Text;
                _RunClientServer = new ClientServer();
                _RunClientServer.Devicetype = this.txtPortName.Text; 
                _RunClientServer.initconfig();
                _RunClientServer._ShowState += new Action<string>(InitComState);
                _RunClientServer._ShowMess += new Action<string>(IComState);
                RemoteObject.SqlServerString = _RunClientServer.sqlconnStr;
                UpdateConnectionStringsConfig("SqlServerString", _RunClientServer.sqlconnStr);
              
            }
            catch (Exception el)
            {
                LogManager.WriteLogSave("ClientErr", el.Message.ToString());
            }
        }



        #region 接收到客户端指令处理的信息
        void TestMessageMarshal_SendMessageEvent(string messge)
        {
            DataAya(messge);
        }
        /// <summary>
        /// 解析客户端 接收信息
        /// </summary>
        /// <param name="DataContext"></param>
        public void DataAya(string DataContext)
        {
            try
            {
                LogManager.WriteLogSave("ClientMsage", DataContext);
                _MessageContext = JsonHelper.DeserializeJsonToObject<MessageContext>(DataContext);
                if (_MessageContext != null)
                {
                    _RunClientServer.SendDevice(_MessageContext);
                }
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("客户端数据指令异常", DataContext, _MessageContext, el);
            }
        }
        #endregion

        private void PortName_SelectedIndexChanged(object sender, EventArgs e)
        {
            strtype = txtPortName.Text;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
               // _RunRecclient.Abort();
            }
            catch 
            {
                //_RunRecclient.DisableComObjectEagerCleanup();
            }
        }

        private void ContextFuntion(object sender, EventArgs e)
        {
            ToolStripMenuItem tempsender = sender as ToolStripMenuItem;
            switch (tempsender.Name)
            {
                case "tsOpenAssist":
                    this.Visible = true;
                    this.WindowState = FormWindowState.Normal;
                    this.notifyIcon1.Visible = false;
                    break;
                case "tsReSetAssist":
                    Application.Restart();
                    break;
            }
        }
        //隐藏信息
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.notifyIcon1.Visible = true;
            }
            else
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.notifyIcon1.Visible = false;
            }
        }

        private void tsReSetAssist_Click(object sender, EventArgs e)
        {
            if (this._RunClientServer != null)
            {
                this._RunClientServer.StopServer();
            }
            this.Close();
        }

        //获取配置信息
        public  string GetConnectionValue(string key)
        {
            if (ConfigurationManager.ConnectionStrings[key] != null)
                return ConfigurationManager.ConnectionStrings[key].ConnectionString;
            return string.Empty;
        }
        //更新配置
        public void UpdateConnectionStringsConfig(string key, string conString)
        {
            bool isModified = false;    //记录该连接串是否已经存在 
            if (ConfigurationManager.ConnectionStrings[key] != null)
            {
                isModified = true;
            }
            //新建一个连接字符串实例 
            ConnectionStringSettings mySettings = new ConnectionStringSettings(key, conString);

            // 打开可执行的配置文件*.exe.config 
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 如果连接串已存在，首先删除它 
            if (isModified)
            {
                config.ConnectionStrings.ConnectionStrings.Remove(key);
            }
            // 将新的连接串添加到配置文件中. 
            config.ConnectionStrings.ConnectionStrings.Add(mySettings);
            // 保存对配置文件所作的更改 
            config.Save(ConfigurationSaveMode.Modified);
            // 强制重新载入配置文件的ConnectionStrings配置节  
            ConfigurationManager.RefreshSection("connectionStrings");
        }
   }
}
