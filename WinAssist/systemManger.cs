

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinAssist.Comm;
using WinAssist.Device;

namespace WinAssist
{
    public partial class systemManger : FrmBaseDrug
    {
        //配置文件名
        string fcom = "ComY310.xml";
        string fdata = "DataY310.xml";
        string fCenterdata = "CenterY310.xml";
        string _Devicetype = "";
        /// <summary>
        /// 
        /// </summary>
        public string Devicetype
        {
            set { _Devicetype = value; }
            get { return _Devicetype; }
        }
        public void SetFilepath(string tile)
        {
            fcom = "Com" + tile ;
            fdata = "Data" + tile ;
            fCenterdata = "Center" + tile ;
        }
        public systemManger()
        {
            InitializeComponent();
        }

        DataTable dt = new DataTable("ComY310");
        DataTable ddt = new DataTable("DataY310");
        DataTable _comdata = new DataTable("CenterY310");

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void initData()
        {
            switch (this.Devicetype)
            {
                case "Y310":
                     SetFilepath("Y310.xml");
                     tpcount = 3;
                    break;
                case "J200":
                    SetFilepath("J200.xml");
                     tpcount = 2;
                    break;
                case "BM200":
                    SetFilepath("BM200.xml");
                    tpcount = 3;
                    break;
                default :
                    SetFilepath("Y300.xml");
                    tpcount = 2;
                    break;

            }
           
            //com
            string[] commstr = GetComList();
            if (commstr != null)
            {
                if (commstr.Length > 0)
                {
                    foreach (string skey in commstr)
                    {
                        cmbCom.Items.Add(skey);
                    }
                    if (cmbCom.Items.Count == 0)
                    {
                        MessageBox.Show("未找到计算机串口！");
                        return;
                    }
                    cmbCom.SelectedIndex = 0;
                }
            }
            cmbDevName.SelectedIndex = 0;
            cmbDataBits.SelectedIndex = 3;
            cmbBaudRate.SelectedIndex = 8;
            cmbStopBits.SelectedIndex = 0;
            cmbParity.SelectedIndex = 0;
            cmbdatatype.SelectedIndex = 0;
            string defaultfile = "";

            fcom = Application.StartupPath + @"\Config\" + fcom;
            fdata = Application.StartupPath + @"\Config\" + fdata;
            fCenterdata = Application.StartupPath + @"\Config\" + fCenterdata;
            if (File.Exists(fcom))
            {
                dt = DataTableManager.ReadFromXml(fcom);
            }
            else
            {
                defaultfile = Application.StartupPath + @"\Config\ComY300.xml";
                dt = DataTableManager.ReadFromXml(defaultfile);
            }
            this.DGConfig.DataSource = dt;

            if (File.Exists(fdata))
            {
                ddt = DataTableManager.ReadFromXml(fdata);
            }
            else
            {
                defaultfile = Application.StartupPath + @"\Config\DataY300.xml";
                ddt = DataTableManager.ReadFromXml(defaultfile);
            }
            this.DGDBConntion.DataSource = ddt;

            if (File.Exists(fCenterdata))
            {
                _comdata = DataTableManager.ReadFromXml(fCenterdata);
            }
            else
            {
                defaultfile = Application.StartupPath + @"\Config\CenterY300.xml";
                _comdata = DataTableManager.ReadFromXml(defaultfile);
            }
            this.CenterDB.DataSource = _comdata;
            //this.CenterDB.Columns[2].Width = 180;
            //初始化样式
            dataGridView(this.DGConfig);
            dataGridView(this.DGDBConntion);
            dataGridView(this.CenterDB);
            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.Alignment = TabAlignment.Left;
            //
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Normal;
            this.tabControl1.ItemSize = new System.Drawing.Size(35, 110);
        }
        /// <summary>
        /// 获取串口数
        /// </summary>
        /// <returns></returns>
        public string[] GetComList()
        {
            string[] str = null;
            try
            {
                RegistryKey keyCom = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
                string[] sSubKeys = keyCom.GetValueNames();
                str = new string[sSubKeys.Length];
                for (int i = 0; i < sSubKeys.Length; i++)
                {
                    str[i] = (string)keyCom.GetValue(sSubKeys[i]);
                }
            }
            catch (Exception el)
            {
                MessageBox.Show("ComErr:"+el.Message.ToString());
            }
            return str;
        }

       
        //配置管理
        private void FunctionSet(object sender, EventArgs e)
        {
            Control _Control = sender as Control;
            switch (_Control.Name)
            {
                case "btnComAdd":
                    commAdd();
                    break;
                case "btnComDel":
                    if (DGConfig.SelectedRows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i][0].ToString() == DGConfig.SelectedRows[0].Cells[0].Value.ToString())
                                dt.Rows.RemoveAt(i);
                        }
                    }
                    break;
                case "btnDataAdd":
                    DBConntionAdd();
                    break;
                case "btnDataDel":
                    if (DGDBConntion.SelectedRows.Count > 0)
                    {
                        for (int i = 0; i < ddt.Rows.Count; i++)
                        {
                            if (ddt.Rows[i][0].ToString() == DGDBConntion.SelectedRows[0].Cells[0].Value.ToString())
                                ddt.Rows.RemoveAt(i);
                        }
                    }
                    break;
                case "PicClose":
                    SaveConfigfile();
                    this.DialogResult = DialogResult.OK;
                    break;
                case "btnUpdataDB"://更新数据库
                    updateDB();
                    break;

            }
        }

        /// <summary>
        /// 读取设备信息更新数据库
        /// </summary>
        /// 
        int tpcount = 3;//默认为3的托盘
        int jzcount = 20;//精准数量
        int BoxdrugID = 4231;//默认药品
        public void updateDB()
        {
            DataDal.t_Drugbox_Info _dalbox_Info = new DataDal.t_Drugbox_Info();
            DataDal.DAL.t_Drugbox_HSinfo _dalHSinfo = new DataDal.DAL.t_Drugbox_HSinfo();
            DataDal.DAL.t_Drugbox_Accurateinfo _t_Drugbox = new DataDal.DAL.t_Drugbox_Accurateinfo();
            DataDal.DAL.t_Drugbox_CZinfo _dalCZinfo = new DataDal.DAL.t_Drugbox_CZinfo();
            _dalbox_Info.DeleteAll();
            _dalHSinfo.DeleteAll();
            _t_Drugbox.DeleteAll();
            _dalCZinfo.DeleteAll(); 
            int jz = 0;
            int cz = 0;
            int hs = 0;
            try
            {
                foreach (DrugboxType _temp in _centercom.ReadDevObj.DevList)
                {
                    foreach (DataRow dr in _comdata.Rows)
                    {
                        if (dr[1].ToString() == _temp._Drugbox.BoxID.ToString())
                        {
                            if (dr[2].ToString() == "回收箱")
                            {
                                hs++;
                                _temp._HS = new DataModel.t_Drugbox_HSinfo();
                                _temp._HS.BoxID = cz;
                                _temp._HS.BoxMID = _temp._Drugbox.BoxID;
                                _temp._HS.DrugCount = 0;
                                _temp._HS.DrugMax = 200;
                                _dalHSinfo.Add(_temp._HS);
                                ShowComm("添加" + _temp._HS.ToString(), 0);
                                _temp._Drugbox.BoxPoint = "1";
                                _temp._Drugbox.BoxType = "3";
                            }
                            if (dr[2].ToString() == "精准药盒")
                            {
                                for (int i = 0; i < jzcount; i++)
                                {
                                    jz++;
                                    _temp._Accurate = new DataModel.t_Drugbox_Accurateinfo();
                                    _temp._Accurate.BoxID = jz;
                                    _temp._Accurate.BoxMID = _temp._Drugbox.BoxID;
                                    _temp._Accurate.Drug_spec = "";
                                    _temp._Accurate.DrugCount = 0;
                                    _temp._Accurate.DrugID = BoxdrugID;
                                    _temp._Accurate.DrugMax = 20;
                                    _temp._Accurate.BoxDrug_date = DateTime.Now.AddYears(1);
                                    _temp._Drugbox.BoxPoint = "1";
                                    _temp._Drugbox.BoxType = "1";
                                    _t_Drugbox.Add(_temp._Accurate);
                                }
                                ShowComm("添加" + _temp._Accurate.ToString(), 0);
                            }
                            if (dr[2].ToString() == "称重药盒")
                            {
                                for (int m = 0; m < tpcount; m++)
                                {
                                    cz++;
                                    _temp._CZ = new DataModel.t_Drugbox_CZinfo();
                                    _temp._CZ.BoxID = cz;
                                    _temp._CZ.BoxMID = _temp._Drugbox.BoxID;
                                    _temp._CZ.BoxUintID = m + 1;
                                    _temp._CZ.BoxUintWeight = "0";
                                    _temp._CZ.BoxUWeight = "0";
                                    _temp._CZ.Drug_spec = "1";
                                    _temp._CZ.DrugCount = 0;
                                    _temp._CZ.DrugID = BoxdrugID;
                                    _temp._CZ.DrugMax = 500;
                                    _temp._CZ.BoxDrug_date = DateTime.Now.AddYears(1);
                                    _dalCZinfo.Add(_temp._CZ);
                                    ShowComm("添加" + _temp._CZ.ToString(), 0);
                                }
                                _temp._Drugbox.BoxPoint = tpcount.ToString();
                                _temp._Drugbox.BoxType = "2";
                            }
                            _dalbox_Info.Add(_temp._Drugbox);
                        }
                    }
                }

            }
            catch (Exception el)
            {
                MessageBox.Show("数据导入异常");
                LogManager.WriteErrorLog("数据导入异常", "数据导入", _centercom.ReadDevObj.DevList, el);
            }
        }



        #region 控件样式 配置信息
        private void dataGridView(DataGridView dataGridView)
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.DimGray;
            dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;//211, 223, 240
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.CadetBlue;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Blue;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Gray;
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.EnableHeadersVisualStyles = false;
            //dataGridView.GridColor = System.Drawing.SystemColors.GradientInactiveCaption;
            dataGridView.GridColor = System.Drawing.SystemColors.Control;
            dataGridView.ReadOnly = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowTemplate.Height = 38;
            dataGridView.RowTemplate.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        //窗体样式
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            StringFormat sf = new StringFormat();

            //   set   the   Alignment   to   center 
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            //获取TabControl主控件的工作区域 

            Rectangle rec = tabControl1.ClientRectangle;

            Font fntTab;
            Brush bshBack;
            Brush bshFore;
            if (e.Index == this.tabControl1.SelectedIndex)    //当前Tab页的样式
            {
                fntTab = new Font(e.Font, FontStyle.Bold);
                bshBack = new SolidBrush(Color.Gray);
                bshFore = Brushes.Black;
            }
            else    //其余Tab页的样式
            {
                fntTab = e.Font;
                bshBack = new SolidBrush(Color.White);
                bshFore = new SolidBrush(Color.Black);
            }
            //画样式
            string tabName = this.tabControl1.TabPages[e.Index].Text;
            //  StringFormat sftTab = new StringFormat();
            e.Graphics.FillRectangle(bshBack, e.Bounds);
            Rectangle recTab = e.Bounds;
            recTab = new Rectangle(recTab.X, recTab.Y + 4, recTab.Width, recTab.Height - 4);
            e.Graphics.DrawString(tabName, fntTab, bshFore, recTab, sf);

        }

        //ddt.Columns.Add("序号");
        //ddt.Columns.Add("数据库类型");
        //ddt.Columns.Add("是否启用");
        //ddt.Columns.Add("IP地址");
        //ddt.Columns.Add("用户名");
        //ddt.Columns.Add("数据库名");
        //ddt.Columns.Add("密码");
        //dr = ddt.NewRow();
        //dr[0] = ddt.Rows.Count.ToString();
        //dr[1] = "Test5";
        //dr[2] = "Test4";
        //ddt.Rows.Add(dr);
        //dr = ddt.NewRow();
        //dr[0] = ddt.Rows.Count.ToString();
        //dr[1] = "Test4";
        //dr[2] = "Test5";
        //ddt.Rows.Add(dr);

        //
        //dt.Columns.Add("序号");
        //dt.Columns.Add("设备名称");
        //dt.Columns.Add("串口号");
        //dt.Columns.Add("波特率");
        //dt.Columns.Add("数据位");
        //dt.Columns.Add("停止位");
        //dt.Columns.Add("校验位");
        //DataRow dr = dt.NewRow();
        //dr[0] = dt.Rows.Count.ToString();
        //dr[1] = "Test5";
        //dr[2] = "Test4";
        //dt.Rows.Add(dr);
        //dr = dt.NewRow();
        //dr[0] = dt.Rows.Count.ToString();
        //dr[1] = "Test4";
        //dr[2] = "Test5";
        //dt.Rows.Add(dr);
        #endregion

        public void SaveConfigfile()
        {
           
            DataTableManager.WriteToXml(dt, fcom);
            DataTableManager.WriteToXml(ddt, fdata);
            DataTableManager.WriteToXml(_comdata, fCenterdata);



        }
        //comm数据
        private void DGConfig_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                cmbDevName.SelectedItem = DGConfig.Rows[e.RowIndex].Cells[1].Value; 
                cmbCom.SelectedItem = DGConfig.Rows[e.RowIndex].Cells[2].Value;
                cmbBaudRate.SelectedItem = DGConfig.Rows[e.RowIndex].Cells[3].Value;
                cmbDataBits.SelectedItem = DGConfig.Rows[e.RowIndex].Cells[4].Value;
                cmbStopBits.SelectedItem = DGConfig.Rows[e.RowIndex].Cells[5].Value;
                cmbParity.SelectedItem = DGConfig.Rows[e.RowIndex].Cells[6].Value;
            }
        }

        public void commAdd()
        {   
            foreach (DataRow tempdr in dt.Rows)
            {
                if (tempdr[2].ToString() == cmbCom.SelectedItem.ToString())
                {
                    MessageBox.Show("两个设备不能占一个端口！");
                    return;
                }
            }
            DataRow dr = dt.NewRow();
            dr[0] = getID(dt).ToString();
            dr[1] = cmbDevName.SelectedItem;
            dr[2] = cmbCom.SelectedItem;
            dr[3] = cmbBaudRate.SelectedItem;
            dr[4] = cmbDataBits.SelectedItem;
            dr[5] = cmbStopBits.SelectedItem;
            dr[6] = cmbParity.SelectedItem;
            dt.Rows.Add(dr);
        }

        public void DBConntionAdd()
        {
            foreach (DataRow tempdr in ddt.Rows)
            {
                if (tempdr[3].ToString() == txtServerIP.Text)
                {
                    MessageBox.Show("两个数据库不能同时用一个IP");
                    return;
                }
            }
            DataRow dr = ddt.NewRow();
            dr[0] = getID(ddt).ToString();
            dr[1] = cmbdatatype.SelectedItem;
            dr[2] = txtServerName.Text;
            dr[3] = txtServerIP.Text ;
            dr[4] = txtUserName.Text;
            dr[5] = txtUserPwd.Text;
           // dr[6] = chkIsrun.Checked ? "启用" : "禁用";
            ddt.Rows.Add(dr);


        }
      
        public int getID(DataTable tempdt)
        {
            int count = 0;
            if (tempdt.Rows.Count > 0)
            {
                foreach (DataRow dr in tempdt.Rows)
                {
                    if (int.Parse(dr[0].ToString()) > count)
                    {
                        count = int.Parse(dr[0].ToString());
                    }
                }
                count++;
            }

            return count;
        }
        #region 公共解析
        public class SerialInfo
        {
            public string DevName = "";
            public string DevCom = "";
            public string DevBaudRate = "";
            public string DevDataBits = "";
            public string DevStopBits = "";
            public string DevParity = "";
        }

        public SerialInfo getSerialObj(string DevName)
        {
            SerialInfo _tempobj =null;
            foreach (DataRow dr in (DGConfig.DataSource as DataTable).Rows)
            {
                if (dr[1].ToString() == DevName)
                {
                    _tempobj = new SerialInfo();
                    //_tempobj.
                    _tempobj.DevName = DevName;
                    _tempobj.DevCom = dr[2].ToString();
                    _tempobj.DevBaudRate = dr[3].ToString();
                    _tempobj.DevDataBits = dr[4].ToString();
                    _tempobj.DevStopBits = dr[5].ToString();
                    _tempobj.DevParity = dr[6].ToString();
                }
            }

            return _tempobj;
        }

        #endregion

        #region 中控处理

        CenterCom _centercom;
        Dictionary<int, int> devID;
        public void InitComm()
        {
            SerialInfo _SerialInfo = getSerialObj("中控机");
            if (_SerialInfo != null)
            {
                if (_centercom == null)
                {
                    _centercom = new ArkCenterCom();
                    _centercom.DataLog = new Action<string, int>(ShowComm);
                    _centercom.ReceivedData = new Action<byte[]>(ReceivedDataExe);
                    _centercom.ReceiveType = new Action<object, CmdType>(ReceiveType);
                    //版本不一样指令不一样#
                    if (this.Devicetype == "ArkcarY300" || this.Devicetype == "ArkBM100")
                    {
                        _centercom.DeviceVersion = "Y300";
                    }
                    _centercom.SetParam(_SerialInfo.DevCom,
                    _SerialInfo.DevBaudRate,
                    _SerialInfo.DevParity,
                    _SerialInfo.DevDataBits,
                    _SerialInfo.DevStopBits);
                    devID = new Dictionary<int, int>();
                    //查询设备
                    _centercom.ReadDevID();
                }
                else
                {
                    //查询设备
                    _centercom.ReadDevID();
                }

            }
        }
        private void FunctionCom(object sender, EventArgs e)
        {
              Control _Control = sender as Control;
              switch (_Control.Name)
              {
                  case "btnInit":
                      InitComm();
                      break;
                  case "btnReadDevList":
                      _comdata.Rows.Clear();
                      int i = 0;
                      foreach (DrugboxType _temp in _centercom.ReadDevObj.DevList)
                      {
                          i++;
                          DataRow dr = _comdata.NewRow();
                          dr[0] = i.ToString();
                          dr[1] = _temp._Drugbox.BoxID.ToString();
                          dr[2] = _temp._Drugbox.BoxType == "1" ? "精准药盒" : "称重药盒";
                          _comdata.Rows.Add(dr);
                      }
                      break;
                  case "btnComconfigSet":
                      for (int n = 0; n < _comdata.Rows.Count; n++)
                      {
                          if (_comdata.Rows[n][1].ToString() == txtModelID.Text)
                          {
                              _comdata.Rows[n][0] = txtOrderID.Text;
                              _comdata.Rows[n][2] = cmbdevtype.SelectedItem.ToString();
                          }
                      }
                      //InitComm();
                      break;
                  case "btnOpenlock"://开锁
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("开锁需要称重设备ID");
                          return;
                      }
                      else
                      {
                          _centercom.OpenLock(txtModelID.Text.Trim());
                      }

                      break;

                  case "btnReadSpec"://读取规格
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("读取规格需要精准设备ID");
                          return;
                      }
                      _centercom.ReadDrugbox(txtModelID.Text.Trim());
                      break;
                  case "btnQDrugCount"://查询药数
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("查询药数需要精准设备ID");
                          return;
                      }
                      _centercom.QDrugCount(txtModelID.Text.Trim());
                      break;
                  case "BtnDrugIn"://加药
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("读取加药需要精准设备ID");
                          return;
                      }
                      _centercom.DrugAdd(txtModelID.Text.Trim());
                      break;
                  case "BtnDrugOut"://取药
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("读取取药需要精准设备ID");
                          return;
                      }
                     /// _centercom.DrugOut(txtModelID.Text.Trim(), nmbDrugCount.Value.ToString());
                      break;
                  case "btnStatisic"://统计
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("药盒统计需要精准设备ID");
                          return;
                      }
                      _centercom.DrugStatisic(txtModelID.Text.Trim());
                      break;
                  case "AllbtnStatisic"://全部统计
                      foreach (DataRow dr in _comdata.Rows)
                      {
                          if (dr[2].ToString() == "精准药盒")
                          {
                              _centercom.DrugStatisic(dr[1].ToString());
                          }
                      }
                      break;

                  case "btnWeight"://称重
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("称重需要称重设备ID");
                          return;
                      }
                      _centercom.SendWeigth(txtModelID.Text.Trim(), byte.Parse(cmbTPID.SelectedItem.ToString()));
                      break;
                  case "btnScale"://标定
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("标定需要称重设备ID");
                          return;
                      }
                      byte Scaletype = (byte)(rbScale1.Checked ? 0 : 1);
                      _centercom.SendScale(txtModelID.Text.Trim(), byte.Parse(cmbTPID.SelectedItem.ToString()), Scaletype);
                      break;
                  case "btnClear"://清零
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("清零需要称重设备ID");
                          return;
                      }
                      _centercom.SendWeigthClear(txtModelID.Text.Trim(), byte.Parse(cmbTPID.SelectedItem.ToString()));
                      break;
                  case "btnWriteLed"://写LED
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("写LED需要称重设备ID");
                          return;
                      }
                      _centercom.SendSendCTinfoBytes_LED(txtLedcontext.Text.Trim(), byte.Parse(nmbLine.Value.ToString()), txtModelID.Text.Trim());
                      break;

                  case "btnHSKS"://回收箱开锁
                      if (txtModelID.Text.Trim() == "")
                      {
                          MessageBox.Show("回收箱需要回收箱设备ID");
                          return;
                      }
                      _centercom.SendKS_only(txtModelID.Text.Trim());
                      break;
              }
        }

        public void ShowComm(string Log, int type)
        {
            Log += "\n";
            if (type == 1)
            {
                this.txtReceivedCom.AppendText(Log);
            }
            else
            {
                this.txtSendCom.AppendText(Log);
            }
        }
        //
        private void CenterDB_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
               txtOrderID.Text = CenterDB.Rows[e.RowIndex].Cells[0].Value.ToString();
               txtModelID.Text  = CenterDB.Rows[e.RowIndex].Cells[1].Value.ToString();
               cmbdevtype.SelectedItem = CenterDB.Rows[e.RowIndex].Cells[2].Value;
            }

        }

        public void ReceivedDataExe(byte[] bytes)
        {
            if (bytes.Length > 12)
            {
                switch (bytes[10])
                {
                        //查询设备指令
                    case 0:
                        if (devID.ContainsKey(bytes[4]))
                        {

                        }
                        break;
                }
            }
          
        }


        public void ReceiveType(object obj, CmdType RType)
        {
            switch (RType)
            {
                case CmdType.Cmd_OutCount://退药
                    ShowComm(obj.ToString(),0);
                    break;
                case CmdType.Cmd_Weight:
                //称重
                    txtWeight.Text = obj.ToString();
                    break;
                case CmdType.Cmd_InCount:  //加药
                    ShowComm(obj.ToString(), 0);
                    break;
                case CmdType.Cmd_BoxSpec:  //药品规格
                      string sepc = string.Empty;
                      if (obj.ToString() == "1") sepc = "1mL";
                      else if (obj.ToString() == "2") sepc = "2mL";
                      else if (obj.ToString() == "3") sepc = "5mL";
                      else if (obj.ToString() == "4") sepc = "10mL";
                     // txtReadSpec.Text = sepc;
                    break;
                case CmdType.Cmd_QCount:  //查询药数
                 
                    break;

                case CmdType.Cmd_TJCount:  //统计药数
                    ShowComm(obj.ToString(), 0);
                    break;
                    
                    
            }
        }


        #endregion

        private void DGDBConntion_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            txtReceivedCom.Clear();
            txtSendCom.Clear();
        }

        private void systemManger_Load(object sender, EventArgs e)
        {
            initData();
        }

        /// <summary>
        /// 读取配置链接设备
        /// </summary>
        /// <param name="DevName">版本名称</param>
        /// <returns>返回中控串口实体</returns>
        public string getDataConntionInfo(string DevName)
        {
            string _tempobj = "Data Source=127.0.0.1;Initial Catalog=ArkCarDB;User ID=sa;password=123;";
            DataTable _tempdt = DataTableManager.ReadFromXml(fdata);
            foreach (DataRow dr in _tempdt.Rows)
            {
                if (dr[1].ToString() == DevName)
                {
                    _tempobj = "Data Source=" + dr[3].ToString() + ";Initial Catalog="
                        + dr[3].ToString() + ";User ID=" + dr[4].ToString() + ";password=" + dr[5].ToString() + ";";
                }
            }
            return _tempobj;
        }

        /// <summary>
        /// 数据库操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FuntionData(object sender, EventArgs e)
        {
              Control _Control = sender as Control;
              switch (_Control.Name)
              {
                  case "btnDataCreate":
                      //commAdd();
                      break;
                  case "btnsqlClear":
                      //commAdd();
                      break;
              }
        }

        public override void DoEsc()
        {
            SaveConfigfile();
            base.DoEsc();
        }

       
    }
}
