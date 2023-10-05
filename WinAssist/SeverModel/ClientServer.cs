using DataModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using WinAssist.Comm;
using WinAssist.Device;
using System.Windows.Forms;
using System.Collections;
using System.IO.Ports;

namespace WinAssist
{
    public  class ClientServer
    {
         //LogManager.WriteLogSave("Cmd_FWeightCount", "查询数量" + UserBoxId.ToString() +"_"+ _listHs[0].ToString());
                   
        public CenterCom _MainCenter;

        public SmCom _SmCom;//扫码

        public CFingerCom _CFingerCom;//指纹处理
        public CTemp _CTemp;//温度处理
       
        //配置文件名
        string fcom = "ComY310.xml";
        string fdata = "DataY310.xml";
        string fCenterdata = "CenterY310.xml";

        DataTable dt = new DataTable("ComY310");
        DataTable ddt = new DataTable("DataY310");
        DataTable _comdata = new DataTable("CenterY310");
       
        //消息内容
        MessageContext _MessageContext;
        string _Devicetype = "";
        public Action<string> _ShowState;
        public Action<string> _ShowMess;

        //用户编号
        int UserID = 0;
        int UserBoxId = 0;
        List<int> usertype = new List<int>();

        //下医嘱订单使用
        string DrugstoreOrderID = "";

        //辅柜
        /// <summary>
        /// 中控设备类型
        /// </summary>
        public string Devicetype
        {
            set { _Devicetype = value; }
            get { return _Devicetype; }
        }
        /// <summary>
        /// 设置配置路径
        /// </summary>
        /// <param name="tile"></param>
        public void SetFilepath(string tile)
        {
            fcom = "Com" + tile;
            fdata = "Data" + tile;
            fCenterdata = "Center" + tile;
        }
        public void StopServer()
        {
            if (_MainCenter != null)
            {
                _MainCenter.Stopsp();
            }
        }

        public string sqlconnStr = "";

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public void initconfig()
        {

            switch (this.Devicetype)
            {
                case "Y310":
                    SetFilepath("Y310.xml");

                    break;
                case "J200":
                    SetFilepath("J200.xml");
                   
                    break;
                case "BM200":
                    SetFilepath("BM200.xml");
                   
                    break;
                default:
                    SetFilepath("Y300.xml");
                   
                    break;

            }
           
            fcom = Application.StartupPath + @"\Config\" + fcom;
            fdata = Application.StartupPath + @"\Config\" + fdata;
            sqlconnStr = getDataConntionInfo("本地数据库");
            //数据链接
           
            //串口配置
            SerialInfo _SerialInfo = getSerialObj("中控机");
            if (_SerialInfo != null)
            {
                if (_MainCenter == null)
                {
                    if (Devicetype == "BM200")
                    {
                        _MainCenter = new BSM200();
                    }
                    if (Devicetype == "J200")
                    {
                        _MainCenter = new J200();
                    }
                    if (Devicetype == "Y310")
                    {
                        _MainCenter = new Y310();
                    }
                    _MainCenter.ReceiveType = new Action<object, CmdType>(ReceiveType);
                    //版本不一样指令不一样#
                    _MainCenter.DeviceVersion = Devicetype;
                  
                    try
                    {
                        _MainCenter.SetParam(_SerialInfo.DevCom,
                        _SerialInfo.DevBaudRate,
                        _SerialInfo.DevParity,
                        _SerialInfo.DevDataBits,
                        _SerialInfo.DevStopBits);
                        _MainCenter.ReadDevID();
                      
                    }
                    catch (Exception el)
                    {
                        ReceiveZK("打开异常:" + el.Message.ToString());
                        LogManager.WriteLogSave("COMErr", "打开异常"+el.Message.ToString());
                        if (_ShowState != null)
                        {
                            //打开中控串口异常！
                            _ShowState("打开中控串口异常！");
                        }
                    }
                }
            }
            else //配置中控程序
            {
                ReceiveZK("没找的中控串口配置！" );
                if (_ShowState != null)
                {
                    //没有找到中控串口！
                    _ShowState("没有找到中控串口！");
                }

            }

            _SerialInfo = getSerialObj("二维码扫描机");
            if (_SerialInfo != null)
            {
                if (_SmCom == null)
                {
                    _SmCom = new SmCom();
                    _SmCom.smInfo = new Action<string>(ReceiveSM);
                    //版本不一样指令不一样#

                    try
                    {
                        _SmCom.initCominfo(_SerialInfo.DevCom,
                          _SerialInfo.DevBaudRate,
                          _SerialInfo.DevParity,
                          _SerialInfo.DevDataBits,
                          _SerialInfo.DevStopBits);

                    }
                    catch (Exception el)
                    {
                        LogManager.WriteLogSave("COMErr", "扫码打开异常" + el.Message.ToString());
                        if (_ShowState != null)
                        {
                            _ShowState("扫码打开异常！");
                        }
                    }
                }
            }
            else //配置中控程序
            {
                if (_ShowState != null)
                {
                    //没有找到中控串口！
                    _ShowState("没有找到扫码设备！");
                }

            }

            _SerialInfo = getSerialObj("打印机");
            if (_SerialInfo != null)
            {
                printl = _SerialInfo.DevCom.ToUpper();
            }
            else //配置中控程序
            {
                if (_ShowState != null)
                {
                    //没有找到中控串口！
                    _ShowState("没有找到打印机设备！");
                }

            }

            _SerialInfo = getSerialObj("指纹机");
            if (_SerialInfo != null)
            {
                if (_CFingerCom == null)
                {
                    _CFingerCom = new CFingerCom();
                    _CFingerCom.FingerComInfo = new Action<int,string>(ReceiveFinger);
                    //版本不一样指令不一样#

                    try
                    {
                        _CFingerCom.initCominfo(_SerialInfo.DevCom,
                          _SerialInfo.DevBaudRate,
                          _SerialInfo.DevParity,
                          _SerialInfo.DevDataBits,
                          _SerialInfo.DevStopBits);

                    }
                    catch (Exception el)
                    {
                        LogManager.WriteLogSave("COMErr", "扫码打开异常" + el.Message.ToString());
                        if (_ShowState != null)
                        {
                            _ShowState("扫码打开异常！");
                        }
                    }
                }
            }
            else //配置中控程序
            {
                if (_ShowState != null)
                {
                    //没有找到中控串口！
                    _ShowState("没有找到扫码设备！");
                }

            }

            _SerialInfo = getSerialObj("温度计");
            if (_SerialInfo != null)
            {
                if (_CTemp == null)
                {
                    _CTemp = new CTemp();
                    _CTemp.ComInfo = new Action<string>(ReceiveTemp);
                    //版本不一样指令不一样#

                    try
                    {
                        _CTemp.initCominfo(_SerialInfo.DevCom,
                          _SerialInfo.DevBaudRate,
                          _SerialInfo.DevParity,
                          _SerialInfo.DevDataBits,
                          _SerialInfo.DevStopBits);

                    }
                    catch (Exception el)
                    {
                        LogManager.WriteLogSave("COMErr", "扫码打开异常" + el.Message.ToString());
                        if (_ShowState != null)
                        {
                            _ShowState("扫码打开异常！");
                        }
                    }
                }
            }
            else //配置中控程序
            {
                if (_ShowState != null)
                {
                    //没有找到中控串口！
                    _ShowState("没有找到扫码设备！");
                }

            }
            //注册通道
        }

        /// <summary>
        /// 温度丢给客户端信息
        /// </summary>
        /// <param name="info"></param>
        public void ReceiveTemp(string info)
        {
            DataContext _Context = new DataContext();
            _Context.BoxMID = "1";
            _Context.DContext = info;
            SendClientMessage("系统模块", "温度信息", _Context);
        }
        /// <summary>
        /// 扫码丢给客户端信息
        /// </summary>
        /// <param name="info"></param>
        public void ReceiveSM(string info)
        {
            DataContext _Context = new DataContext();
            _Context.BoxMID ="1";
            _Context.DContext = info;
            SendClientMessage("系统模块", "扫码信息", _Context);
        }

        /// <summary>
        /// 扫码丢给客户端信息
        /// </summary>
        /// <param name="info"></param>
        public void ReceiveZK(string info)
        {
            DataContext _Context = new DataContext();
            _Context.BoxMID = "1";
            _Context.DContext = info;
            SendClientMessage("系统模块", "中控异常", _Context);
        }
        
        

        /// <summary>
        /// 指纹数据丢给客户端信息
        /// </summary>
        /// <param name="info"></param>
        public void ReceiveFinger(int state,string info)
        {
            DataContext _Context = new DataContext();
            switch (state)
            {
                case 0:
                    _Context.BoxMID = "0";
                    _Context.DContext = info;
                    SendClientMessage("系统模块", "指纹登记", _Context);
                    break;
                case 1:
                    _Context.BoxMID = "1";
                    _Context.DContext = info;
                    SendClientMessage("系统模块", "指纹登记", _Context);
                    break;
                case 2:
                    _Context.BoxMID = "0";
                    _Context.DContext = info;
                    SendClientMessage("系统模块", "指纹登陆", _Context);
                    break;
                case 3:
                    _Context.BoxMID = "1";
                    _Context.DContext = info;
                    SendClientMessage("系统模块", "指纹登陆", _Context);
                    break;
                case 5:
                    _Context.BoxMID = "0";
                    _Context.DContext = info;
                    SendClientMessage("系统模块", "指纹删除", _Context);
                    break;
                case 6:
                    _Context.BoxMID = "1";
                    _Context.DContext = info;
                    SendClientMessage("系统模块", "指纹删除", _Context);
                    break;

            }
          
        }
        /// <summary>
        /// 读取中控 返回状态并丢给客户端
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="RType"></param>
        public void ReceiveType(object obj, CmdType RType)
        {
            List<object> _listobj = new List<object>();
            Dictionary<int, string> _listHs = new Dictionary<int, string>();
            DataContext _Context;
            if (obj is List<object>)
            {
                _listobj = obj as List<object>;
            }
            if (obj is Dictionary<int, string>)
            {
                _listHs = obj as Dictionary<int, string>;
             }
            switch (RType)
            {
                case CmdType.Cmd_OutCount://精准出药
                    // ShowComm(obj.ToString(), 0);
                    UpdateAccurateCountStoreOut(_listobj[0].ToString(), _listobj[1].ToString());
                    _Context = new DataContext();
                    _Context.BoxMID = _listobj[0].ToString();
                    _Context.DContext = _listobj[2].ToString();
                    SendClientMessage("系统模块", "精准出药", _Context);
                    break;
                case CmdType.Cmd_AllWeight:  //多盘称重
                    //更新称重和药盒数量
                    UpdateStore(obj);
                    break;
                case CmdType.Cmd_InCount:  //加药
                    //更新精准药盒数量
                    //UpdateAccurateinfo(_listobj[0].ToString(), sepc);
                    UpdateAccurateCountStoreIn(_listobj[0].ToString(), _listobj[1].ToString(), _listobj[2].ToString());
                    _Context = new DataContext();
                    _Context.BoxMID = _listobj[0].ToString();
                    _Context.DContext = _listobj[2].ToString();
                    SendClientMessage("精准模块", "精准加药", _Context);
                    break;
                case CmdType.Cmd_InERRCount:  //加异常药
                    //更新精准药盒数量
                    _Context = new DataContext();
                    SendClientMessage("精准模块", "精准加药异常", _Context);
                    break;
                case CmdType.Cmd_BoxSpec:  //药品规格
                    string sepc = string.Empty;
                    if (_listobj[1].ToString() == "1") sepc = "1mL";
                    else if (_listobj[1].ToString() == "2") sepc = "2mL";
                    else if (_listobj[1].ToString() == "3") sepc = "5mL";
                    else if (_listobj[1].ToString() == "4") sepc = "10mL";
                    //更新规格
                    UpdateAccurateinfo(_listobj[0].ToString(), sepc);
                    _Context = new DataContext();
                    _Context.BoxMID = _listobj[0].ToString();
                    _Context.DContext = sepc;
                    SendClientMessage("精准模块", "读取规格", _Context);
                    break;
                case CmdType.Cmd_DrugBoxSpec:  //药品规格
                    string sepcs = string.Empty;
                    if (_listobj[1].ToString() == "1") sepcs = "1mL";
                    else if (_listobj[1].ToString() == "2") sepcs = "2mL";
                    else if (_listobj[1].ToString() == "3") sepcs = "5mL";
                    else if (_listobj[1].ToString() == "4") sepcs = "10mL";
                    _Context = new DataContext();
                    _Context.BoxMID = _listobj[0].ToString();
                    _Context.DContext = sepcs;
                    SendClientMessage("精准模块", "读取加药规格", _Context);
                    break;
                case CmdType.Cmd_QCount:  //查询药数
                    UpdateAccurateDrugCount(_listobj[0].ToString(), _listobj[1].ToString(), _listobj[2].ToString());

                    _Context = new DataContext();
                    _Context.BoxMID = _listobj[0].ToString();
                    _Context.DContext = _listobj[1].ToString() + "_" + _listobj[2].ToString();

                    SendClientMessage("精准模块", "读取数量", _Context);
                    break;

                case CmdType.Cmd_TJCount:  //统计药数
                    //ShowComm(obj.ToString(), 0);
                    _Context = new DataContext();
                    _Context.BoxMID = _listobj[0].ToString();
                    _Context.DContext = _listobj[1].ToString();
                    UpdateAccurateDrugCount(_listobj[0].ToString(), _listobj[1].ToString(), _listobj[2].ToString());
                    SendClientMessage("精准模块", "统计数量", _Context);
                    break;
                case CmdType.Cmd_GS:  //关锁处理
                    _Context = new DataContext();
                    _Context.BoxMID = _listHs[0].ToString();
                    _Context.DContext = "";
                    SendClientMessage("称重模块", "关锁处理", _Context);
                    break;
                case CmdType.Cmd_Weight:  //查询称重
                    _Context = new DataContext();
                    _Context.BoxMID = _listHs[0].ToString();
                    _Context.DContext = _listHs[1].ToString();
                    SendClientMessage("称重模块", "读取重量", _Context);
                    break;

                case CmdType.Cmd_WeightCount:  //查询数量
                    _Context = new DataContext();
                    _Context.BoxMID = _listHs[0].ToString();
                    int n = _listHs.Count;
                    string dcontext = "";
                    for (int i = 1; i < n; i++)
                    {
                        if (int.Parse(_listHs[i]) < 300)
                        {
                            dcontext += _listHs[i].ToString() + "#";
                        }
                        else
                        {
                            _Context.DContext = _listHs[i];
                            SendClientMessage("报警模块", "超出300", _Context);
                            return;
                        }
                    }
                    dcontext = dcontext.TrimEnd('#');
                    //更新药盒数据 
                    UpdateCZBoxCountStoreIn(_listHs);
                    _Context.DContext = dcontext;
                    SendClientMessage("称重模块", "读取数量", _Context);
                    break;
                case CmdType.Cmd_WeightMainCount:  //主柜查询数量
                    _Context = new DataContext();
                    _Context.BoxMID = _listHs[0].ToString();
                    n = _listHs.Count;
                    dcontext = "";
                    //处理订单
                    if (DrugstoreOrderID != "")
                    {
                        OrderStoreIn(_listHs,0);
                    }
                    for (int i = 1; i < n; i++)
                    {
                        dcontext += _listHs[i].ToString() + "#";
                    }
                    dcontext = dcontext.TrimEnd('#');
                    //更新药盒数据 
                    UpdateCZBoxCountStoreIn(_listHs);
                    _Context.DContext = dcontext;
                    SendClientMessage("称重模块", "主柜读取数量", _Context);
                    break;

                case CmdType.Cmd_FWeightCount:  //辅柜查询数量

                    //辅助取药使用
                    if (UserBoxId.ToString() == "300")
                    {
                        if (usertype.Contains(int.Parse(_listHs[0].ToString())))
                        {
                            _Context = new DataContext();
                            _Context.BoxMID = _listHs[0].ToString();
                            _Context.DContext = _listHs[1].ToString();
                            SendClientMessage("称重模块", "辅柜读取数量", _Context);
                            Thread.Sleep(50);
                            _MainCenter.WriteLedTip(0x00, 0x00, ushort.Parse(_listHs[0].ToString()));
                            Thread.Sleep(50);
                        }
                        OrderStoreIn(_listHs,1);
                    }
                    else
                    {
                        if (UserBoxId.ToString() == _listHs[0].ToString())
                        {
                            _Context = new DataContext();
                            _Context.BoxMID = _listHs[0].ToString();
                            _Context.DContext = _listHs[1].ToString();
                            SendClientMessage("称重模块", "辅柜读取数量", _Context);
                        }
                        else
                        {
                            //更新入库出库数据库
                            UpdateFStoreInOrOunt(_listHs[0].ToString(), _listHs[1].ToString());
                        }
                    }
                    //更新药盒数据 
                    UpdateCZBoxCountStoreIn(_listHs);
                    break;
            }
        }

       
        /// <summary>
        /// 订单主柜记录处理
        /// </summary>
        public void OrderStoreIn(Dictionary<int, string> listDev,int typeInt)
        {
            DataSet dscz = _czDal.GetList(" BoxMID =" + listDev[0].ToString());
            foreach (DataRow dr in dscz.Tables[0].Rows)
            {
                int indexUint = int.Parse(dr["BoxUintID"].ToString());
                if(int.Parse(dr["DrugCount"].ToString()) ==int.Parse(listDev[indexUint]))
                {
                    continue;
                }
                if (int.Parse(dr["DrugCount"].ToString()) > int.Parse(listDev[indexUint]))
                {
                    //取药
                    _dataDrugstore_Out = new t_Drugstore_Out();
                    _dataDrugstore_Out.storeOutID = _dalDrugstore_Out.GetRecordCount("") + 1000;
                    _dataDrugstore_Out.BoxID = int.Parse(dr["BoxID"].ToString());
                    _dataDrugstore_Out.Boxtype = 2;
                    int tempDrugID = 0;
                    if (dr["DrugID"].ToString().Trim() != "")
                    {
                        tempDrugID = int.Parse(dr["DrugID"].ToString().Trim());
                    }
                    _dataDrugstore_Out.DrugID = tempDrugID;
                    _dataDrugstore_Out.OutCount = int.Parse(dr["DrugCount"].ToString()) - int.Parse(listDev[indexUint]);
                    _dataDrugstore_Out.OutDate = DateTime.Now;
                    _dataDrugstore_Out.OrderID = int.Parse(DrugstoreOrderID);
                    _dataDrugstore_Out.UserID = UserID;
                    _dalDrugstore_Out.Add(_dataDrugstore_Out);
                }
                else
                {
                    _dataDrugstore_In = new t_Drugstore_In();
                    _dataDrugstore_In.storeInID = 1000 + _dalDrugstore_In.GetRecordCount("");
                    _dataDrugstore_In.BoxType = 2;
                    int tempDrugID = 0;
                    if (dr["DrugID"].ToString().Trim() != "")
                    {
                        tempDrugID = int.Parse(dr["DrugID"].ToString().Trim());
                    }
                    _dataDrugstore_In.DrugID = tempDrugID;
                    _dataDrugstore_In.InCount = int.Parse(listDev[indexUint]) - int.Parse(dr["DrugCount"].ToString());
                    _dataDrugstore_In.InDate = DateTime.Now;
                    _dataDrugstore_In.OrderID = int.Parse(DrugstoreOrderID); ;
                    _dataDrugstore_In.UserID = UserID;
                    _dataDrugstore_In.BoxID = int.Parse(dr["BoxID"].ToString());
                    _dataDrugstore_In.Remark = DrugstoreOrderID;
                    _dalDrugstore_In = new DataDal.DAL.t_Drugstore_In();
                    _dalDrugstore_In.Add(_dataDrugstore_In);
                }
                //辅柜 1个盘
                if (typeInt == 1)
                {
                    break;
                }
            }
        }
        t_Drugstore_In _dataDrugstore_In;
        DataDal.DAL.t_Drugstore_In _dalDrugstore_In =new DataDal.DAL.t_Drugstore_In();
        DataModel.t_Drugbox_CZinfo _czinfo;
        public void UpdateFStoreInOrOunt(string BoxMID, string DrugCount)
        {
            DataSet _tempds = _czDal.GetList(" BoxMID =" + BoxMID.ToString());
            foreach (DataRow dr in _tempds.Tables[0].Rows)
            {
                _czinfo = _czDal.DataRowToModel(dr);
                break;
            }
            int tempdrugcount = int.Parse(DrugCount) - (int)_czinfo.DrugCount;
            if (tempdrugcount != 0)
            {
                //showtitle += "托盘：" + setcount.ToString();
                //入药与出药
                if (tempdrugcount > 0)
                {
                 
                    _dataDrugstore_In = new t_Drugstore_In();  
                    _dataDrugstore_In.storeInID = 1000 + _dalDrugstore_In.GetRecordCount("");
                    _dataDrugstore_In.BoxType = 2;
                    _dataDrugstore_In.DrugID = (int)_czinfo.DrugID;
                    _dataDrugstore_In.InCount = tempdrugcount;
                    _dataDrugstore_In.InDate = DateTime.Now;
                    _dataDrugstore_In.OrderID = 0;
                    _dataDrugstore_In.UserID = UserID;
                    _dataDrugstore_In.BoxID = _czinfo.BoxID;
                    _dalDrugstore_In = new DataDal.DAL.t_Drugstore_In();
                    _dalDrugstore_In.Add(_dataDrugstore_In);
                }
                else
                { 
                    _dataDrugstore_Out = new t_Drugstore_Out();
                    _dataDrugstore_Out.storeOutID = 1000 + _dalDrugstore_Out.GetRecordCount("");
                    _dataDrugstore_Out.DrugID = (int)_czinfo.DrugID;
                    _dataDrugstore_Out.Boxtype = 2;
                    _dataDrugstore_Out.OutCount = -tempdrugcount;
                    _dataDrugstore_Out.OutDate = DateTime.Now;
                    _dataDrugstore_Out.OrderID = 0;
                    _dataDrugstore_Out.BoxID = _czinfo.BoxID;

                    _dataDrugstore_Out.UserID = UserID;
                    _dalDrugstore_Out.Add(_dataDrugstore_Out);
                   
                }
            }
        }



        public void SendClientMessage(string _CmdType, string _CmdMessage, DataContext _Context)
        {
            _MessageContext = new MessageContext();
            _MessageContext.ID = 1;
            _MessageContext.CmdType = _CmdType;
            _MessageContext.CmdMessage = _CmdMessage;
            _MessageContext.Context = _Context;

            StrCom.SendClientObj(_MessageContext);

        }
        /// <summary>
        /// 串口配置信息
        /// </summary>
        public class SerialInfo
        {
            public string DevName = "";
            public string DevCom = "";
            public string DevBaudRate = "";
            public string DevDataBits = "";
            public string DevStopBits = "";
            public string DevParity = "";
        }

        /// <summary>
        /// 读取配置链接设备
        /// </summary>
        /// <param name="DevName">版本名称</param>
        /// <returns>返回中控串口实体</returns>
        public SerialInfo getSerialObj(string DevName)
        {
            SerialInfo _tempobj = null;
            DataTable _tempdt = DataTableManager.ReadFromXml(fcom);
            foreach (DataRow dr in _tempdt.Rows)
            {
                if (dr[1].ToString() == DevName)
                {
                    _tempobj = new SerialInfo();
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
                        + dr[2].ToString() + ";User ID=" + dr[4].ToString() + ";password=" + dr[5].ToString() + ";";
                }
            }
            return _tempobj;
        }

       
        /// <summary>
        /// 客户端发送过来处理的信息
        /// </summary>
        /// <param name="_MessageContext"></param>
        public void SendDevice(MessageContext _MessageContext)
        {
            switch (_MessageContext.CmdType)
            {
                case "精准模块":
                    SetJZModel(_MessageContext);
                    break;
                case "称重模块":
                    SetCZModel(_MessageContext);
                    break;
                case "系统模块":
                    SetSysModel(_MessageContext);
                    break;
            }
           
        }

        public void SetJZModel(MessageContext _MessageContext)
        {
            switch (_MessageContext.CmdMessage)
            {
                case "更新精准药盒":
                    //开锁加药或出药
                    UpdateBoxState(_MessageContext.Context.BoxMID);
                    //记录药盒药品数量
                    break;
                case "更新全部药盒":
                    //开锁加药或出药
                    DataSet tempds = _BoxDal.GetList("");
                    foreach (DataRow dr in tempds.Tables[0].Rows)
                    {
                        UpdateBoxState(dr[0].ToString());
                        Thread.Sleep(200);
                    }
                    //记录药盒药品数量
                    break;
                case "读取规格":
                    _MainCenter.ReadDrugbox(_MessageContext.Context.BoxMID);
                    break;
                case "读取加药盒规格":
                    _MainCenter.Readboxspec(_MessageContext.Context.BoxMID);
                    break;
                case "读取数量":
                    _MainCenter.QDrugCount(_MessageContext.Context.BoxMID);
                    break;
                case "查询规格":
                    _MainCenter.sendCheckBoxSpec(_MessageContext.Context.BoxMID);
                    break;
                case "精准加药":
                    _MainCenter.DrugAdd(_MessageContext.Context.BoxMID);
                    break;
                case "精准退药":
                    _MainCenter.ReadboxspecReturn(_MessageContext.Context.BoxMID, int.Parse(_MessageContext.Context.DContext));
                    break;
                case "精准亮灯":
                    byte bl = (byte)(_MessageContext.Context.DContext == "1" ? 0x01 : 0x00);
                    _MainCenter.sendLD(ushort.Parse(_MessageContext.Context.BoxMID), bl);
                    break;
                case "盘点数量":
                    _MainCenter.DrugStatisic(_MessageContext.Context.BoxMID);
                    break;
                case "盘点全部数量":
                    DataTable dt = _BoxDal.GetList(" BoxType=1").Tables[0];
                   foreach (DataRow dr in dt.Rows)
                   {
                       _MainCenter.DrugStatisic(dr["BoxID"].ToString());
                       Thread.Sleep(50);
                   }
                    break;

            }
        }

        public void SetCZModel(MessageContext _MessageContext)
        {
            switch (_MessageContext.CmdMessage)
            {

                //移动小车退药开锁加药或出药
                case "称重开灯开锁":
                      _MainCenter.OpenLock(_MessageContext.Context.BoxMID);
                      _MainCenter.OpenKD(_MessageContext.Context.BoxMID, int.Parse(_MessageContext.Context.BoxID), true);
                    break;
                case "开锁":
                    //移动小车开锁加药或出药
                    //_MainCenter.sen 
                    _MainCenter.OpenLock(_MessageContext.Context.BoxMID);
                    _MainCenter.OpenKD(_MessageContext.Context.BoxMID, 0, true);
                    //记录药盒药品数量
                    break;
                case "病区开锁":
                    //病区开锁加药或出药
                    //_MainCenter.sen
                  
                        if (_MessageContext.Context.BoxID == "200")
                        {
                            _MainCenter.GetSendLockIO(200);
                        }
                        if (_MessageContext.Context.DContext != null)
                        {
                            if (_MessageContext.Context.DContext.Trim() != "")
                            {
                                UserID = int.Parse(_MessageContext.Context.DContext.Trim());
                            }
                        }
                    
                    //记录药盒药品数量
                    break;
                case "Led入库提示":
                    //病区开锁加药或出药
                    //_MainCenter.sen 
                    _MainCenter.WriteLedTip(0x00, 0x01, ushort.Parse(_MessageContext.Context.BoxMID));
                    UserBoxId = ushort.Parse(_MessageContext.Context.BoxMID);
                    //记录药盒药品数量
                    break;
                case "Led关闭提示":
                    //病区开锁辅助加药或出药
                    //_MainCenter.sen 
                    Thread.Sleep(100);
                    _MainCenter.WriteLedTip(0x00, 0x00, ushort.Parse(_MessageContext.Context.BoxMID));
                    Thread.Sleep(50);
                    //记录药盒药品数量
                    break;
                case "Led出库提示":
                    //病区开锁辅助加药或出药
                    //_MainCenter.sen 
                     Thread.Sleep(100);
                    _MainCenter.WriteLedTip(byte.Parse(_MessageContext.Context.DContext),
                        0x02, ushort.Parse(_MessageContext.Context.BoxMID));

                    UserBoxId = ushort.Parse(_MessageContext.Context.BoxID);//
                   
                    break;
               

                case "主柜Led入库提示":
                    //病区开锁加药或出药
                    //_MainCenter.sen 
                    _MainCenter.WriteLedTip(0x00, 0x01, ushort.Parse(_MessageContext.Context.BoxMID));
                    UserBoxId = ushort.Parse(_MessageContext.Context.BoxMID);
                    //记录药盒药品数量
                    break;
                case "主柜Led出库提示":
                    //病区开锁
                    _MainCenter.WriteMainLedTipCurr(_MessageContext.Context.DContext);
                    //记录药盒药品数量
                    break;

                case "主柜写Led小屏":
                    string[] mainstr = _MessageContext.Context.DContext.Split('#');
                    int intled = 0;
                    Thread.Sleep(100);
                    if (this.Devicetype == "Y310")
                    {
                        intled = 1;
                        _MainCenter.SendSendSm_LED("安瑞科科技有限公司"+ "#" + intled.ToString() + "#", _MessageContext.Context.BoxMID);
                        Thread.Sleep(10);
                        foreach (string strled in mainstr)
                        {
                            string[] mstr = strled.Split('-');
                            intled++;
                            _MainCenter.SendSendSm_LED(mstr[0] +mstr[1]+ "#" + intled.ToString() + "#", _MessageContext.Context.BoxMID);
                            Thread.Sleep(10);
                        }
                    }
                    else
                    {
                        foreach (string strled in mainstr)
                        {
                            string[] mstr = strled.Split('-');
                            intled++;
                            _MainCenter.SendSendSm_LED(mstr[0] + "#" + intled.ToString() + "#", _MessageContext.Context.BoxMID);
                            Thread.Sleep(10);
                            intled++;
                            _MainCenter.SendSendSm_LED(mstr[1] + "#" + intled.ToString() + "#", _MessageContext.Context.BoxMID);
                            Thread.Sleep(10);
                        }
                    }
                    break;
                  case "写辅柜Lcd小屏":
                    _MainCenter.WriteLedTipText(_MessageContext.Context.DContext,
                       byte.Parse(_MessageContext.Context.BoxID), ushort.Parse(_MessageContext.Context.BoxMID.ToString()));
                   
                    break;
                case "写入Led小屏状态":
                    //病区开锁加药或出药
                    //_MainCenter.sen 
                    string[] strl = _MessageContext.Context.DContext.Split('#');
                    int m =0;
                    foreach (string tempstr in strl)
                    {
                        m++;
                        _MainCenter.SendSendSm_LED("0#0#" + tempstr + "#" + m.ToString(), _MessageContext.Context.BoxMID);
                        Thread.Sleep(100);
                    }
                    //记录药盒药品数量
                    break;
                case "写入Led小屏":
                    //病区开锁加药或出药
                    //_MainCenter.sen 
                    _MainCenter.WriteLedTip(0x00, 0x00, ushort.Parse(_MessageContext.Context.BoxMID));
                    //记录药盒药品数量
                    break;
                case "读取重量":
                    byte tby = byte.Parse(_MessageContext.Context.BoxID);
                    _MainCenter.SendWeigth(_MessageContext.Context.BoxMID, tby);
                    break;

                case "设置单量":
                    byte tbSet = byte.Parse(_MessageContext.Context.BoxID);
                    ushort dwight = (ushort)(decimal.Parse(_MessageContext.Context.DContext) * 100);
                    _MainCenter.SendWeigthOneSet(tbSet, dwight, ushort.Parse(_MessageContext.Context.BoxMID));
                    break;
                case "设置清零":
                    byte tbSetClear = byte.Parse(_MessageContext.Context.BoxID);
                    _MainCenter.SendWeigthClear(_MessageContext.Context.BoxMID, tbSetClear);
                    break;
                case "写入LED大屏":
                    string[] DContextled = _MessageContext.Context.DContext.Split('#');
                    _MainCenter.SendSendBig_LED(DContextled[0], 0x01, _MessageContext.Context.BoxMID);
                    Thread.Sleep(100);
                    _MainCenter.SendSendBig_LED(DContextled[1], 0x02, _MessageContext.Context.BoxMID);
                     Thread.Sleep(100);
                    _MainCenter.SendSendBig_LED(DContextled[2], 0x03, _MessageContext.Context.BoxMID);
                    break;
                case "空盘写入LED大屏":
                    _MainCenter.SendSendBig_LED(_MessageContext.Context.DContext, 0x01, _MessageContext.Context.BoxMID);
                    break;
             


            }
        }

        public void SetSysModel(MessageContext _MessageContext)
        {
            switch (_MessageContext.CmdMessage)
            {
                case "关闭程序":
                    if (_ShowMess != null)
                    {
                        _ShowMess(_MessageContext.CmdMessage);
                    }
                    break;
                case "打印":
                    //在这里清空订单处理
                    DrugstoreOrderID = "";
                    PintServer(_MessageContext.Context.DContext, _MessageContext.Context.BoxMID);
                    break;
                case "条码打印":
                    PrintServer(_MessageContext.Context.DContext);
                    break;
                case "打印服务":
                    PrintContext(_MessageContext.Context.DContext);
                    break;
                case "指纹登记":
                    
                    RegFingerServer(_MessageContext.Context.DContext);
                    break;
                case "指纹登陆":
                    LoginFingerServer();
                    break;
                case "指纹删除":
                    DelFingerServer(_MessageContext.Context.DContext);
                    break;
                case "指纹清空":
                    DelFingerServer();
                    break;
                case "单重重置":
                    string UintSet = _MessageContext.Context.DContext;
                    break;
                case "温度计查询":
                    QryWD();
                    break;
                case "订单取药":
                    //临时/处方取药用户ID
                    UserID = int.Parse(_MessageContext.Context.BoxID);
                    //临时/处方取药订单编号
                    DrugstoreOrderID = _MessageContext.Context.BoxMID;
                    string[] strp = _MessageContext.Context.DContext.Split('#');
                    string findsql = "BoxID in(";
                    Dictionary<string, string> boxIDinfo = new Dictionary<string, string>();
                    string strsqljz = "";
                    foreach (string strl in strp)
                    {
                        string[] boxinfo = strl.Split(':');
                        if (boxinfo[3] == "1")
                        {
                            strsqljz += boxinfo[0] + ",";
                            boxIDinfo.Add(boxinfo[0], boxinfo[2]);
                        }
                    }
                    if (strsqljz != "")
                    {
                        findsql += strsqljz.TrimEnd(',') + ")";
                        DataSet ds = (new DataDal.t_Drugbox_Info()).GetList(findsql);
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            //精准模块
                            if (dr["BoxType"].ToString() == "1")
                            {
                                _MainCenter.DrugOut(dr["BoxID"].ToString(), boxIDinfo[dr["BoxID"].ToString()]);
                                Thread.Sleep(50);
                            }
                        }
                    }
                    usertype.Clear();
                    foreach (string strl in strp)
                    {
                        string[] boxinfo = strl.Split(':');
                        if (boxinfo[3] == "2")
                        {
                            int tempBoxMID = _czDal.GetModel(int.Parse(boxinfo[0])).BoxMID;
                            if (!usertype.Contains(tempBoxMID))
                            { 
                                usertype.Add(tempBoxMID);
                            }
                            //开灯
                            if (this.Devicetype == "Y310")
                            {
                                _MainCenter.OpenKD(tempBoxMID.ToString(), (int)_czDal.GetModel(int.Parse(boxinfo[0])).BoxUintID, true);
                            }
                        }
                    }
                    //辅柜取药记录
                    UserBoxId = _MainCenter.LockID(usertype);
                    break;
            }

        }
       

      
        public int GetLockID(string MboxID)
        {
            int lockID = 1;
            switch (MboxID)
            {
                case "7":
                case "8":
                    lockID = 1;
                    break;
                case "9":
                case "10":
                    lockID = 2;
                    break;
                case "11":
                case "12":
                    lockID = 3;
                    break;
            }
            return lockID;
        }
        #region 药盒信息处理
        DataDal.DAL.t_Drugbox_CZinfo _czDal = new DataDal.DAL.t_Drugbox_CZinfo();
        DataDal.DAL.t_Drugbox_Accurateinfo _jzDal = new DataDal.DAL.t_Drugbox_Accurateinfo();
        DataDal.t_Drugbox_Info _BoxDal = new DataDal.t_Drugbox_Info();
        DataModel.t_Drugbox_CZinfo _czdata;
        DataModel.t_Drugbox_Accurateinfo _jzdata;
        DataModel.t_Drugbox_Info _Boxdata;

        DataDal.DAL.t_Drugstore_Out _dalDrugstore_Out=new DataDal.DAL.t_Drugstore_Out();
        DataModel.t_Drugstore_Out _dataDrugstore_Out;
        /// <summary>
        /// 更新药盒库存和药盒数量
        /// </summary>
        /// <param name="_obj">更新的内容</param>
        public void UpdateStore(object  _obj)
        {
            if (_obj is Dictionary<int, string>)
            {
                Dictionary<int, string> templist = _obj as Dictionary<int, string>;
                DataSet tempds = _czDal.GetList(" BoxMID=" + templist[0].ToString());
                foreach (DataRow dr in tempds.Tables[0].Rows)
                {
                    _czdata = _czDal.DataRowToModel(dr);
                    foreach (int skey in templist.Keys)
                    {
                        if (_czdata.BoxUintID == skey)
                        {
                            _czdata.BoxUWeight = templist[skey].ToString();
                            int W1 = int.Parse(int.Parse(_czdata.Drug_spec) == 0 ? "0" : string.Format("{0:f0}", float.Parse(templist[skey].ToString()) / int.Parse(_czdata.Drug_spec)));// - _CurrInfo.weithtB1
                            _czdata.DrugCount = W1;                         
                            _czDal.Update(_czdata);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 更新药盒状态
        /// </summary>
        /// <param name="BoxMID">药盒编号</param>
        public void UpdateBoxState(string BoxMID)
        {
          _Boxdata =  _BoxDal.GetModel(int.Parse(BoxMID));
          if (_Boxdata != null)
          {
              //为精准药盒
              if (_Boxdata.BoxType == "1")
              {
                  _MainCenter.QDrugCount(BoxMID);
                  Thread.Sleep(50);
                  _MainCenter.ReadDrugbox(BoxMID);
                  Thread.Sleep(50);
              }
              //称重药盒
              if (_Boxdata.BoxType == "2")
              {
                  _MainCenter.SendWeigth(BoxMID, 0);
              }
          }
        }
        /// <summary>
        /// 更新精准药盒规格
        /// </summary>
        /// <param name="Val"></param>
        public void UpdateAccurateinfo(string DevID, string sepc)
        {
            try
            {    
                _jzDal.Update(sepc, int.Parse(DevID));
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("精准药盒规格异常", "更新异常", _jzDal, el);
            }
        }
       


        /// <summary>
        /// 更新精准药盒药品数量
        /// </summary>
        /// <param name="Val"></param>
        public void UpdateAccurateDrugCount(string DevID, string DrugCount, string DrugCount2)
        {
            try
            {
                char[] stl = DrugCount.ToArray();
                Array.Reverse(stl);
                int tmpindex = 0;
                DataTable tempstr = _jzDal.GetList(" BoxMID=" + DevID).Tables[0];

                foreach (DataRow dr in tempstr.Rows)
                {
                    _jzDal.UpdateDrugCount2(stl[tmpindex].ToString(),
                        int.Parse(DevID), int.Parse(dr["BoxID"].ToString()));
                    tmpindex++;
                }
                _jzDal.UpdateDrugCount(DrugCount2, int.Parse(DevID));
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("精准药盒状态更新异常", "更新异常", _jzDal, el);
            }
        }

        /// <summary>
        /// 更新精准药盒药品数量以及返回加药数量
        /// </summary>
        /// <param name="Val"></param>
        public void UpdateAccurateCountStoreIn(string DevID, string DrugCount,string DrugCount2)
        { 
            try
            {
                char[] stl = DrugCount.ToArray();
                Array.Reverse(stl);
                int tmpindex = 0;
                DataTable tempstr = _jzDal.GetList(" BoxMID=" + DevID).Tables[0];
                
                foreach (DataRow dr in tempstr.Rows)
                {
                    if (stl[tmpindex].ToString() == "1")
                    {
                        _jzDal.UpdateDrugCount2("1",
                            int.Parse(DevID), int.Parse(dr["BoxID"].ToString()));
                    }
                    tmpindex++;
                }
                _dataDrugstore_In = new t_Drugstore_In();
               _dataDrugstore_In.storeInID =1000+ _dalDrugstore_In.GetRecordCount("");
                _dataDrugstore_In.BoxID = int.Parse(DevID.ToString());
                _dataDrugstore_In.DrugID =int.Parse(tempstr.Rows[0]["DrugID"].ToString());
                _dataDrugstore_In.InCount = int.Parse(DrugCount2);
                _dataDrugstore_In.BoxType = 1;
                _dataDrugstore_In.InDate = DateTime.Now;
                _dataDrugstore_In.OrderID = 0;
                _dataDrugstore_In.UserID = UserID;

             
                _dalDrugstore_In.Add(_dataDrugstore_In);
                int count =_jzDal.GetRecordCount(" BoxMID=" + DevID+" and DrugCount=1");
                _jzDal.UpdateDrugCount(count.ToString(), int.Parse(DevID));

                
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("精准药盒状态更新异常", "更新异常", _jzDal, el);
            }
        }

        /// <summary>
        /// 更新精准药盒药品数量以及更新出药数量
        /// </summary>
        /// <param name="Val"></param>
        public void UpdateAccurateCountStoreOut(string DevID, string DrugCount)
        {
            try
            {
                char[] stl = DrugCount.ToArray();
                Array.Reverse(stl);
                int tmpindex = 0;
                DataTable tempstr = _jzDal.GetList(" BoxMID=" + DevID).Tables[0];

                foreach (DataRow dr in tempstr.Rows)
                {
                    if (stl[tmpindex].ToString() == "1")
                    {
                        _jzDal.UpdateDrugCount2("0",
                            int.Parse(DevID), int.Parse(dr["BoxID"].ToString()));
                    }
                    tmpindex++;
                }
                int count = _jzDal.GetRecordCount(" BoxMID=" + DevID + " and DrugCount=1");
                _jzDal.UpdateDrugCount(count.ToString(), int.Parse(DevID));
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("精准药盒状态更新异常", "更新异常", _jzDal, el);
            }
        }
        /// <summary>
        /// 更新称重药盒信息
        /// </summary>
        /// <param name="DevID">编号</param>
        /// <param name="DrugCount"></param>
        public void UpdateCZBoxCountStoreIn(Dictionary<int, string> _listupdate)
        {
            try
            {
               int tempcount= _listupdate.Count;
               for(int i=1;i<tempcount;i++)
               {
                   _czDal.Update(_listupdate[i], i.ToString(), _listupdate[0]);
               }
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("称重药盒状态更新异常", "更新异常", _jzDal, el);
            }
        }


        #endregion

        //
        #region 打印服务
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void PintServer(string context, string UserName)
        {
            try
            {
               // LogManager.WriteLogSave("test2", printl.ToString() + ">>" + context.ToString());
                // iPrinterID = TPrintDll.POS_Port_OpenA(string.Format("{0}:9600,N,8,1", RTU80SWSD.RTU80SWSD_MainView.WDJ1_Com), 1000, false, "");//1000        POS_PT_LPT = 1001;  POS_PT_USB = 1002;  POS_PT_NET = 1003;
                Int32 iPrinterID;
                iPrinterID = TPrintDll.POS_Port_OpenA("usb_sp1", 1002, false, "");
                //if (printl == "")
                //{
                //    iPrinterID = TPrintDll.POS_Port_OpenA("usb_sp1", 1002, false, "");
                //}
                //else
                //{
                //    iPrinterID = TPrintDll.POS_Port_OpenA(printl + ":9600,N,8,1", 1000, false, "");//

                //}
                if (iPrinterID < 0)
                {
                    return;
                }
                TPrintDll.POS_Control_AlignType(iPrinterID, 0);
                TPrintDll.POS_Output_PrintFontStringA(iPrinterID, 0, 0, 0, 0, 0, string.Format("用户姓名：{0}  \r\n", UserName));
                TPrintDll.POS_Output_PrintFontStringA(iPrinterID, 0, 0, 0, 0, 0, "==============================\r\n");

                context = context.TrimEnd('#');
                string[] printstrl = context.Split('#');
                foreach (string strp in printstrl)
                {
                    string[] spint = strp.Split('>');
                    TPrintDll.POS_Output_PrintFontStringA(iPrinterID, 0, 0, 0, 0, 0, spint[0] + "  \r\n");
                    TPrintDll.POS_Output_PrintOneDimensionalBarcodeA(iPrinterID, 4074, 2, 50, 4013, spint[1].Trim() + " \r\n");
                }

                TPrintDll.POS_Output_PrintFontStringA(iPrinterID, 0, 0, 0, 0, 0, "==============================\r\n");
                TPrintDll.POS_Output_PrintFontStringA(iPrinterID, 0, 0, 0, 0, 0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                TPrintDll.POS_Control_CutPaper(iPrinterID, 1, 3);
                TPrintDll.POS_Port_Close(iPrinterID);
            }
            catch (Exception ex)
            {
                LogManager.WriteErrorLog("打印失败", "打印失败！", ex, ex);
            }
        }
        public string printl = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void PrintServer(string context)
        {
            try
            {
                // iPrinterID = TPrintDll.POS_Port_OpenA(string.Format("{0}:9600,N,8,1", RTU80SWSD.RTU80SWSD_MainView.WDJ1_Com), 1000, false, "");//1000        POS_PT_LPT = 1001;  POS_PT_USB = 1002;  POS_PT_NET = 1003;
                Int32 iPrinterID;
                iPrinterID = TPrintDll.POS_Port_OpenA("usb_sp1", 1002, false, "");
                //if (!printl.ToUpper().Contains("COM"))
                //{
                //    iPrinterID = TPrintDll.POS_Port_OpenA("usb_sp1", 1002, false, "");
                //}
                //else
                //{
                //    iPrinterID = TPrintDll.POS_Port_OpenA(printl + ":9600,N,8,1", 1000, false, "");//
                //}
              
                if (iPrinterID < 0)
                {
                    return;
                }
                TPrintDll.POS_Control_AlignType(iPrinterID, 0);
                TPrintDll.POS_Output_PrintOneDimensionalBarcodeA(iPrinterID, 4074, 2, 50, 4013, context.Trim() + " \r\n");
                TPrintDll.POS_Control_CutPaper(iPrinterID, 1, 3);
                TPrintDll.POS_Port_Close(iPrinterID);
            }
            catch (Exception ex)
            {
                LogManager.WriteLogSave("printl", ex.Message.ToString());
                LogManager.WriteErrorLog("打印失败", "打印失败！", ex, ex);
            }


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void PrintContext(string context)
        {
            try
            {
                // iPrinterID = TPrintDll.POS_Port_OpenA(string.Format("{0}:9600,N,8,1", RTU80SWSD.RTU80SWSD_MainView.WDJ1_Com), 1000, false, "");//1000        POS_PT_LPT = 1001;  POS_PT_USB = 1002;  POS_PT_NET = 1003;
                Int32 iPrinterID;
                iPrinterID = TPrintDll.POS_Port_OpenA("usb_sp1", 1002, false, "");
                //if (printl == "")
                //{
                //    iPrinterID = TPrintDll.POS_Port_OpenA("usb_sp1", 1002, false, "");
                //}
                //else
                //{
                //    iPrinterID = TPrintDll.POS_Port_OpenA(printl + ":9600,N,8,1", 1000, false, "");//
                //}
                if (iPrinterID < 0)
                {
                    return;
                }
                TPrintDll.POS_Control_AlignType(iPrinterID, 0);
                TPrintDll.POS_Output_PrintFontStringA(iPrinterID, 0, 0, 0, 0, 0, context);
                TPrintDll.POS_Control_CutPaper(iPrinterID, 1, 3);
                TPrintDll.POS_Port_Close(iPrinterID);
            }
            catch (Exception ex)
            {
                LogManager.WriteLogSave("printl", ex.Message.ToString());
                LogManager.WriteErrorLog("打印失败", "打印失败！", ex, ex);
            }


        }
        #endregion

        #region 指纹设置

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void RegFingerServer(string context)
        {
            try
            {
                _CFingerCom.ReFingerState(ushort.Parse(context));
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("指纹注册异常", "指纹注册", _CFingerCom, el);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void DelFingerServer(string context)
        {
            try
            {
                _CFingerCom.delfinger(ushort.Parse(context));
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("指纹注册异常", "指纹注册", _CFingerCom, el);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void DelFingerServer()
        {
            try
            {
                _CFingerCom.SendDelFinger();
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("指纹删除异常", "指纹删除", _CFingerCom, el);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void LoginFingerServer()
        {
            try
            {
                _CFingerCom.SendLoginFinger();
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("指纹注册异常", "指纹注册", _CFingerCom, el);
            }
        }
        #endregion

        #region 温度计
        /// <summary>
        /// 查询温度计
        /// </summary>
        /// <param name="context"></param>
        public void QryWD()
        {
            try
            {
                _CTemp.SendQry();
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("温度计查询失败", "温度计", _CFingerCom, el);
            }
        }
        #endregion

    }
}
