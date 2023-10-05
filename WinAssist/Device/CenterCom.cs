using DataModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using WinAssist.Comm;

namespace WinAssist.Device
{
    //中控板块设置
    public abstract class CenterCom
    {
        //
        public SerialPort _sp;
        public Action<string,int> DataLog;
        public Action<byte[]> ReceivedData;
        private DeviceList _DevObj;
        public Action<object, CmdType> ReceiveType;
        ///型号一变 协议边
        public string DeviceVersion = "";
        private delegate void DelegateMessage(string addr, byte waddr);

        private delegate void DelegateRun(byte[] bytes, int length);

        //查数量
        public static Queue<string> QMessage;

        //写多查询
        public static Queue<string> WMessage;

        //写多查询
        public static Queue<string> LedMessage;

        Thread _Thread;
        /// <summary>
        /// 包序号
        /// </summary>
        byte RepackNo = 0;
        /// <summary>
        /// 开锁编号
        /// </summary>
        byte KSNO = 1;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PortName"></param>
        /// <param name="BaudRate"></param>
        /// <param name="Parity"></param>
        /// <param name="DataBits"></param>
        /// <param name="StopBits"></param>

        public void SetParam(string PortName, string BaudRate, string Parity, string DataBits, string StopBits)
        {
            int _BaudRate = int.Parse(BaudRate);
            Parity _Parity = (Parity)Enum.Parse(typeof(Parity), Parity, true);
            int _DataBits = int.Parse(DataBits);
            StopBits _StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBits, true);
            _Thread = new Thread(ExeRun);
            _Thread.Start();
            QMessage = new Queue<string>();
            WMessage = new Queue<string>();
            LedMessage = new Queue<string>();
            _sp = new SerialPort(PortName, _BaudRate, _Parity, _DataBits, _StopBits);
            _sp.ReadTimeout = SerialPort.InfiniteTimeout;
            _sp.ReadBufferSize = 4096;
            _sp.DataReceived += _sp_DataReceived;
            _DevObj = new DeviceList();
            _DevObj.DevList = new List<DrugboxType>();
            _sp.Open();
            
        }
        //处理多个查询
        bool tp = false;
        public void ExeRun()
        {
            while (true)
            {
                if (QMessage.Count > 0)
                {
                    if (tp)
                    {
                        Thread.Sleep(1000);
                        tp = false;
                    }
                    else
                    {  
                        Thread.Sleep(150);
                        string message = QMessage.Dequeue();
                        SendReadCount(message, 0);
                      
                    }
                }
                else
                {
                    tp = true;
                }
               
                if (WMessage.Count > 0)
                {
                    string message = WMessage.Dequeue();
                    WriteMainLedTip(message);
                    Thread.Sleep(200);
                }
                if (LedMessage.Count > 0)
                {
                    string message = LedMessage.Dequeue();
                    SendSendSm_LED2(message);
                    Thread.Sleep(200);
                }
                
            }
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stopsp()
        {
            try
            {
                if (_Thread != null)
                {
                    _Thread.Abort();
                }
                if (_sp != null)
                {
                    _sp.Close();
                }
            }
            catch
            {
            }
        }
        public void SendLog(string _text,int i)
        {
            DataLog(_text,i);
        }

        Byte[] receivedData = new Byte[512];        //创建接收字节数组
        int leng = 0;
        List<byte> _ReceiveData = new List<byte>();
        //回复包
        void _sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                leng = _sp.BytesToRead;
              
                if (leng > 0)
                {  
                    receivedData = new byte[leng];
                    leng = _sp.Read(receivedData, 0, receivedData.Length);
                    //过滤非A5 88 
                    for (int i = 0; i < receivedData.Length; i++)
                    {     
                        _ReceiveData.Add(receivedData[i]);
                    }
                    //合并设备丢包时。13有效byte
                    if (_ReceiveData.Count % 13 == 0)
                    { 
                        byte[] ReadiveData = _ReceiveData.ToArray();
                        DelegateRun _DelegateRun = new DelegateRun(Reback);
                        _DelegateRun.BeginInvoke(ReadiveData, ReadiveData.Length, null, null);
                        //Reback(ReadiveData, ReadiveData.Length);
                        LogManager.WriteLogSave("Read", StrCom.byteToHexStr(ReadiveData, ReadiveData.Length));
                        if (DataLog != null)
                        {
                            SendLog(StrCom.byteToHexStr(ReadiveData, ReadiveData.Length), 0);
                        }
                        _ReceiveData.Clear();
                    }
                }
            }
            catch(Exception el)
            {
                LogManager.WriteErrorLog("中控接收数据异常", "接收数据异常", _sp, el);
            }
        }


        /// <summary>
        /// 必回信息包
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        private void Reback(byte[] bytes, int length)
        {
            byte gnm = bytes[7];
            byte getreadID = bytes[4];
            gnm = (byte)(gnm << 1);
            gnm = (byte)(gnm >> 1);
            byte hubs = (byte)(bytes[7] >> 7); ///回复标识
            ushort Addr = getreadID;
            List<byte> newbytes = new List<byte>();
            for (int k = 0; k < bytes.Length / 13; k++)
            {
                newbytes.AddRange(bytes.ToList().GetRange(13 * k + 5, 8));
            }
            if (hubs == 0)//回复
            {
                byte newgnm = 1;
                newgnm <<= 7;
                newgnm += gnm;
                byte[] receiveBytestemp = newbytes.GetRange(0, newbytes[1]).ToArray();
                byte[] sendbytes = GetRepackBytes(bytes, newgnm, newbytes[3], receiveBytestemp[receiveBytestemp.Length - 3]);// newbytes[newbytes[1] - 3]);//Convert.ToByte(boxid.Text)
                ComSend(sendbytes, sendbytes.Length);
                // return;
            }
            //查询设备信息
            if (newbytes[1] == 0x0A || newbytes[1] == 0x09)
            {
                if (newbytes[2] == 0x00)
                {
                    bool IsAdd = true;
                    for (int i = 0; i < _DevObj.DevList.Count; i++)
                    {
                        if (_DevObj.DevList[i]._Drugbox.BoxID == getreadID)
                        {
                            IsAdd = false;
                        }
                    }
                    if (IsAdd)
                    {
                        DrugboxType _temp = new DrugboxType();
                        _temp._Drugbox = new t_Drugbox_Info();
                        _temp._Drugbox.BoxID = getreadID;
                        //精准或称重 
                        if (bytes[9] == 0x02)
                        {
                            _temp._Drugbox.BoxType = "1";
                        }
                        else
                        {
                            _temp._Drugbox.BoxType = "2";
                        }
                       
                        _temp._Drugbox.IsRun = 1;
                        _temp._Drugbox.LockID = 0;
                        _temp._Drugbox.BoxState = 0;
                        _DevObj.DevList.Add(_temp);
                    }
                }
            }
            //丢到前端处理
            if (ReceivedData != null)
            {
                ReceivedData(bytes);
            }
            //数据解析
            try
            {
                AnayzeData(bytes, newbytes, gnm);
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("数据解析数据异常", "数据解析", _sp, el);
            }
            
        }

        public DeviceList ReadDevObj
        {
            get { return _DevObj; }
        }

        public DeviceList SetDevObj
        {
            set { _DevObj = value; }
        }


        /// <summary>
        /// 回复包
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        public  byte[] GetRepackBytes(byte[] receives, byte gnm, byte feature, byte packno)
        {
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x08, gnm, feature, 0x00, packno };//receives[receives[1]-3]
            ;
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            List<byte> sendbytes = new List<byte>() { 0x88, 0x00, 0x00, receives[3], receives[4] };
            sendbytes.AddRange(bytescontants);
            return sendbytes.ToArray();
        }


        /// <summary>
        /// 串口发送包
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        private void ComSend(byte[] bytes, int length)
        {
            try
            {
                if (_sp != null && _sp.IsOpen)
                    _sp.Write(bytes, 0, length);
               // LogManager.WriteLogSave("Send", StrCom.byteToHexStr(bytes, bytes.Length));
                if (DataLog != null)
                {
                    SendLog(StrCom.byteToHexStr(bytes, bytes.Length), 1);
                }

            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("中控发送数据异常", "发送数据异常", _sp, el);
            }
        }

        /// <summary>
        /// 串口回复包
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        private void ComBackSend(byte[] bytes, int length)
        {
            try
            {
                if (_sp != null && _sp.IsOpen)
                    _sp.Write(bytes, 0, length);
               // LogManager.WriteLogSave("SendBack", StrCom.byteToHexStr(bytes, bytes.Length));
                if (DataLog != null)
                {
                    SendLog(StrCom.byteToHexStr(bytes, bytes.Length), 1);
                }

            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("中控发送数据异常", "发送数据异常", _sp, el);
            }
        }

        /// <summary>
        /// 查询设备指令
        /// </summary>
        public void ReadDevID()
        {
            List<byte> sendbytes = new List<byte>() { 0x87, 0x00, 0x00, 0x00, 0x00, 0x5A, 0x07, 0x00, 0x00, 0x00, 0x79, 0x7D, 0x00 };
            ComSend(sendbytes.ToArray(), sendbytes.Count);
        }

        //不同型号不同锁控
        public virtual int LockID(List<int> temp)
        {
            //称重就是开锁
            int _UboxID = 0;
            List<int> LockID = new List<int>();

            foreach (int tint in temp)
            {
                int templockid = GetMboxID(tint);
                if (!LockID.Contains(templockid))
                {
                    LockID.Add(templockid);
                }
            }

            foreach (int i in LockID)
            {
                if (i == 8)
                {
                    OpenLock("200", "1");
                    _UboxID = 300;
                }
                else
                {
                    OpenLock("100", i.ToString());
                }
                Thread.Sleep(200);
            }

            return _UboxID;
        }

        /// <summary>
        /// 转换锁号
        /// </summary>
        /// <param name="mb"></param>
        /// <returns></returns>
        public virtual int GetMboxID(int Gemb)
        {
            int lockID = 1;
            if (Gemb > 25)
            {
                lockID = 8;
                return lockID;
            }
            int tempcount = Gemb - 7;
            tempcount = tempcount / 3;
            lockID = tempcount + 1;
            return lockID;
        }


      
        public virtual void AnayzeData(byte[] bytes,List<byte> newbytes, byte Modeltype)
        {
            byte hubs = (byte)(bytes[7] >> 7); ///回复标识
            byte DevID = bytes[4];//设备地址
            byte[] receiveBytes = newbytes.GetRange(0, newbytes[1]).ToArray();
            List<object> listobj = new List<object>();
            listobj.Add(DevID);//添加
            switch (Modeltype)
            {
                case 0x07://精准药盒模块  

                    switch (receiveBytes[3])
                    {
                        case 0x00: //查询当前药品数量
                            string arr = string.Empty;
                            arr = Convert.ToString(newbytes[7], 2).PadLeft(8, '0') + Convert.ToString(newbytes[8], 2).PadLeft(8, '0') + Convert.ToString(newbytes[9], 2).PadLeft(8, '0');
                            int leng = arr.Length - arr.Replace("1", "").Length;
                            listobj.Add(arr);
                            listobj.Add(leng);
                            SetReceiveType(listobj, CmdType.Cmd_QCount);
                            break;
                        case 0x01: //精准出药 
                            if (hubs == 1) return;
                               Int32 datas = newbytes[7];
                                            datas <<= 8;
                                            datas += newbytes[8];
                                            datas <<= 8;
                                            datas += newbytes[9];
                            string arr2 = Convert.ToString(newbytes[7], 2).PadLeft(8, '0') + Convert.ToString(newbytes[8], 2).PadLeft(8, '0') + Convert.ToString(newbytes[9], 2).PadLeft(8, '0');
                            int leng2 = arr2.Length - arr2.Replace("1", "").Length;
                            listobj.Add(arr2);
                            listobj.Add(leng2);
                            SetReceiveType(listobj, CmdType.Cmd_OutCount);            
                            break;

                        case 0x02: //统计数量
                            if (hubs == 1) return;
                            Int32 boxdatas = newbytes[7];
                            boxdatas <<= 8;
                            boxdatas += newbytes[8];
                            boxdatas <<= 8;
                            boxdatas += newbytes[9];
                            arr = Convert.ToString(newbytes[7], 2).PadLeft(8, '0') + Convert.ToString(newbytes[8], 2).PadLeft(8, '0') + Convert.ToString(newbytes[9], 2).PadLeft(8, '0');
                            string packcons = string.Format("地址 {0} 装药的有：", DevID.ToString()) + arr.ToString();
                            SetReceiveType(packcons, CmdType.Cmd_TJCount);  
                            break;

                        case 0x03: //加药
                        case 0x08: //退药返回信息
                            if (hubs == 1) return;
                            if (receiveBytes[1] != 0x07)
                            {
                                arr = Convert.ToString(newbytes[7], 2).PadLeft(8, '0') + Convert.ToString(newbytes[8], 2).PadLeft(8, '0') + Convert.ToString(newbytes[9], 2).PadLeft(8, '0');
                                int lengcount = arr.Length - arr.Replace("1", "").Length;

                                listobj.Add(arr);
                                listobj.Add(lengcount);
                                SetReceiveType(listobj, CmdType.Cmd_InCount); 
                            }
                            break;

                        case 0x06: //读药盒规格信息
                            listobj.Add(receiveBytes[5]);
                            SetReceiveType(listobj, CmdType.Cmd_BoxSpec); 
                            break;
                        case 0xFF: //指示灯控制
                            #region
                            string ErrMsg = string.Empty;
                            switch (receiveBytes[4])
                            {
                                case 1:
                                    ErrMsg = "数量不够";
                                    break;
                                case 2:
                                    ErrMsg = "超出范围";
                                    break;
                                case 3:
                                    ErrMsg = "当前繁忙";
                                    break;
                                case 4:
                                    ErrMsg = "超时";
                                    break;
                                case 5:
                                    ErrMsg = "解锁错误";
                                    break;
                                case 6:
                                    ErrMsg = "定位错误";
                                    break;
                                case 7:
                                    ErrMsg = "统计错误";
                                    break;
                                case 8:
                                    ErrMsg = "匹配错误";
                                    break;
                                case 9:
                                    ErrMsg = "加药枪错误";
                                    break;
                                case 10:
                                    ErrMsg = "操作错误";
                                    break;
                            }
                            SetReceiveType(ErrMsg, CmdType.Cmd_ErrMsg);
                           // CommanClass.comm.CommFunc.ARKShowDlg(ErrMsg);
                            #endregion
                            break;
                    }
                    break;

                case 0x03://出药完成，药盒返回出药状态
                    break;
                case 0x08://加药盒返回规格信息
                    switch (receiveBytes[3])
                    {
                        case 0x04: //读加药盒规格信息
                            listobj.Add(receiveBytes[4]);
                            SetReceiveType(listobj, CmdType.Cmd_DrugBoxSpec);
                            break;
                    }
                    break;
                case 0x00://设备控制

                    break;

                case 0x01://锁模块 一般就是称重，或回收箱
                    switch (receiveBytes[3])
                    {
                        case 0x00: //关锁 状态
                          //  LogManager.WriteLogSave("GS", DeviceVersion.ToString() + ">>" + DevID.ToString()+">>"+StrCom.byteToHexStr(receiveBytes, receiveBytes.Length));
                            //关锁查询数量
                            Thread.Sleep(50);
                            if (DevID.ToString() == "200")
                            {
                                if (receiveBytes[4] == 0x07 && receiveBytes[5]==0x01)
                                {
                                    //查辅柜的称重数量
                                    for (int i = 26; i < 32; i++)
                                    {
                                        QMessage.Enqueue(i.ToString());
                                    }
                                }
                            } 
                            if (DevID.ToString() == "100")
                            {
                                //查主柜的称重数量
                                if (receiveBytes[4] == 0x01 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("7");
                                    QMessage.Enqueue("8");
                                }
                                if (receiveBytes[4] == 0x02 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("9");
                                    QMessage.Enqueue("10");
                                }
                                if (receiveBytes[4] == 0x03 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("11");
                                    QMessage.Enqueue("12");
                                }
                            }
                            //移动小车按编号
                            //SendReadCount(DevID.ToString(), 0);
                            break;
                        case 0x01: // 开锁 状态
                            SetReceiveType(DevID.ToString(), CmdType.Cmd_KS);
                            break;
                    }
                    break;

                case 0x05://电子称
                    switch (receiveBytes[3])
                    {
                        case 0x00: //查重量 
                            Dictionary<int, string> _tempWeights = new Dictionary<int, string>();
                            _tempWeights.Add(0, DevID.ToString());//称重设备编号
                            switch (receiveBytes[4])
                            {
                                case 0x00://查全部重量
                                    Int32 weights = receiveBytes[5];
                                    weights <<= 8;
                                    weights += receiveBytes[6];
                                    weights <<= 8;
                                    weights += receiveBytes[7];
                                    decimal getweights = ((receiveBytes[5] >> 7) & 0x01) == 1 ? (decimal)(weights - 16777215) / 100 : (decimal)weights / 100;

                                    weights = receiveBytes[8];
                                    weights <<= 8;
                                    weights += receiveBytes[9];
                                    weights <<= 8;
                                    weights += receiveBytes[10];
                                    decimal getweights2 = ((receiveBytes[8] >> 7) & 0x01) == 1 ? (decimal)(weights - 16777215) / 100 : (decimal)weights / 100;

                                    if (DeviceVersion == "Y300")
                                    {
                                        //两个盘
                                        _tempWeights.Add(1, getweights.ToString());
                                        _tempWeights.Add(2, getweights2.ToString());

                                    }
                                    if (DeviceVersion == "Y310")
                                    {
                                        weights = receiveBytes[11];
                                        weights <<= 8;
                                        weights += receiveBytes[12];
                                        weights <<= 8;
                                        weights += receiveBytes[13];
                                        decimal getweights3 = ((receiveBytes[11] >> 7) & 0x01) == 1 ? (decimal)(weights - 16777215) / 100 : (decimal)weights / 100;

                                        //三个盘
                                        _tempWeights.Add(1, getweights.ToString());
                                        _tempWeights.Add(2, getweights2.ToString());
                                        _tempWeights.Add(3, getweights3.ToString());
                                    }
                                    //LogManager.WriteLogSave("test2", DeviceVersion.ToString() + ">>" + getweights2.ToString());

                                    SetReceiveType(_tempWeights, CmdType.Cmd_AllWeight);
                                    break;
                                case 0x01://查单个重量
                                case 0x02://
                                case 0x03://
                                    Int32 weights2 = receiveBytes[5];
                                    weights2 <<= 8;
                                    weights2 += receiveBytes[6];
                                    weights2 <<= 8;
                                    weights2 += receiveBytes[7];
                                    decimal gweight = ((receiveBytes[5] >> 7) & 0x01) == 1 ? (decimal)(weights2 - 16777215) / 100 : (decimal)weights2 / 100;
                                   
                                    _tempWeights.Add(1, gweight.ToString());
                                    SetReceiveType(_tempWeights, CmdType.Cmd_Weight);
                                    break;
                            }
                            break;
                        case 0x01: //标定
                            SetReceiveType(DevID.ToString() + "标定成功", CmdType.Cmd_WeightBD);
                            break;
                        case 0x02: //去皮
                            SetReceiveType(DevID.ToString() + "清零成功", CmdType.Cmd_WeightQL);
                            break;
                        case 0x04: //设置单重
                            // CommFunc.ARKShowDlg("单重设置成功！");
                            SetReceiveType(DevID.ToString() + "设置单重", CmdType.Cmd_SetWeight);
                            break;
                        case 0x05: //查询数量
                            Dictionary<int, string> _tempCount = new Dictionary<int, string>();
                              _tempCount.Add(0, DevID.ToString());//称重设备编号
                            ushort weightNums1 = 0;
                            ushort weightNums2 = 0;

                           // LogManager.WriteLogSave("ReadCount", DeviceVersion.ToString() + ">>" + StrCom.byteToHexStr(receiveBytes, receiveBytes.Length));

                            #region
                            if (receiveBytes[4] == 1 || receiveBytes[4] == 0)
                            {
                                if (receiveBytes[2] == 0x05)
                                {
                                    break;
                                }
                                weightNums1 = receiveBytes[5];
                                weightNums1 <<= 8;
                                weightNums1 += receiveBytes[6];
                                try
                                {
                                    //查辅柜一个盘
                                    if (DevID > 25)
                                    {
                                        _tempCount.Add(1, weightNums1.ToString());
                                        SetReceiveType(_tempCount, CmdType.Cmd_FWeightCount);
                                    }//查主柜一个盘
                                    else if (DevID > 6 && DevID < 13)
                                    {
                                        weightNums2 = receiveBytes[7];
                                        weightNums2 <<= 8;
                                        weightNums2 += receiveBytes[8];
                                        //三个盘
                                        _tempCount.Add(1, weightNums1.ToString());
                                        _tempCount.Add(2, weightNums2.ToString());
                                        ushort weightNums3 = 0;
                                        weightNums3 = receiveBytes[9];
                                        weightNums3 <<= 8;
                                        weightNums3 += receiveBytes[10];
                                        _tempCount.Add(3, weightNums3.ToString());
                                        //weightNums2 = receiveBytes[7];
                                        //weightNums2 <<= 8;
                                        //weightNums2 += receiveBytes[8];
                                        //if (DeviceVersion == "Y300")
                                        //{
                                        //    //两个盘
                                        
                                        //}
                                        //if (DeviceVersion == "Y310")
                                        //{
                                        //    //三个盘
                                        //    ushort weightNums3 = 0;
                                        //    weightNums3 = receiveBytes[9];
                                        //    weightNums3 <<= 8;
                                        //    weightNums3 += receiveBytes[10];
                                        //    _tempCount.Add(3, weightNums3.ToString());
                                        //}
                                        SetReceiveType(_tempCount, CmdType.Cmd_WeightMainCount);
                                    }
                                }
                                catch (Exception el)
                                {
                                    LogManager.WriteLogSave("ReadErr", DevID.ToString() + ">>" + el.Message.ToString());
                                
                                }

                            }
                            #endregion
                            break;
                    }
                    break;
                case 0x70://在线升级
                    #region

                    #endregion
                    break;
            }
        }
     
        #region 电子称操作
        /// <summary>
        /// 设备号开锁
        /// </summary>
        ///  <param name="adr">设备ID</param>
        public void OpenLock(string adr)
        {
            byte[] sendbytes = GetKSBytes(0x01, 0x01, Convert.ToUInt16(adr.ToString()));//Convert.ToByte(boxid.Text)
            ComSend(sendbytes, sendbytes.Length);
        }

        /// <summary>
        /// 设备号开锁
        /// </summary>
        ///  <param name="adr">设备ID</param>
        ///  <param name="adrno">编号ID</param>
        public void OpenLock(string adr, string IndexNo)
        {
            try
            {

                byte[] sendbytes = GetKSBytes(0x01, 0x01, byte.Parse(IndexNo),ushort.Parse( adr));//Convert.ToByte(boxid.Text)


                ComSend(sendbytes, sendbytes.Length);
            }
            catch 
            {

            }
            
        }

        //IO延时处理
        public void GetSendLockIO(ushort addr)
        {
            setRepackNo();
            // 0B B8
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x0B, 0x09, 0x03, 0x29, 0x01, 0x17, 0x70 };

            bytescontants.Add(RepackNo);

            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants.GetRange(0, 8));
            head = 8;
            head <<= 4;
            head += (byte)(bytescontants.Count - 8);
            List<byte> sendbytes2 = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes2.AddRange(bytescontants.GetRange(8, bytescontants.Count - 8));
            for (int k = 0; k < 8 - bytescontants.Count % 8; k++)
                sendbytes2.Add(0);
            sendbytes.AddRange(sendbytes2);
            LogManager.WriteLogSave("Sendys", StrCom.byteToHexStr(sendbytes.ToArray(), sendbytes.Count));
            ComSend(sendbytes.ToArray(), sendbytes.Count);

        }

                      

        private void SendKS(ushort addr, byte IndexNo)
        {
            try
            {

                byte[] sendbytes = GetKSBytes(0x01, 0x01, IndexNo, addr);//Convert.ToByte(boxid.Text)

              

            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 设备号开关灯
        /// </summary>
        ///  <param name="adr">设备ID</param>
        public void OpenKD(string adr, int BoxNo,bool bl)
        {
            byte[] sendbytes;
            if (bl)
            {
                sendbytes = SendKD(0x05, 0x03, (byte)BoxNo, 0xFF, Convert.ToUInt16(adr.ToString()));
            }
            else
            {
                sendbytes = SendKD(0x05, 0x03, 0, 0, Convert.ToUInt16(adr.ToString())); ;//Convert.ToByte(boxid.Text)
            }
            ComSend(sendbytes, sendbytes.Length);
        }
        /// <summary>
        /// 辅柜
        /// </summary>
        /// <param name="drugcount"></param>
        /// <param name="showtype"></param>
        /// <param name="addr"></param>

        public void WriteLedTip(byte drugcount, byte showtype, ushort addr)
        {
            byte[] sendbytes = GetSendCTinfoBytes_TFT_LCDState(drugcount, showtype, addr);//Convert.ToByte(boxid.Text)
            ComSend(sendbytes, sendbytes.Length);
        }

        /// <summary>
        /// 辅柜
        /// </summary>
        /// <param name="drugcount"></param>
        /// <param name="showtype"></param>
        /// <param name="addr"></param>

        public void WriteLedTipText(string drugText, byte showtype, ushort addr)
        {
            byte[] sendbytes = GetSendCTinfoBytes_TFT_LCD(drugText, showtype, addr);//Convert.ToByte(boxid.Text)
            ComSend(sendbytes, sendbytes.Length);
        }

        /// <summary>
        /// 主柜提示
        /// </summary>
        /// <param name="drugcount"></param>
        /// <param name="showtype"></param>
        /// <param name="addr"></param>

        public void WriteMainLedTip(object Val)
        {
           // LogManager.WriteLogSave("LedTip", Val.ToString());
            byte[] sendbytes = GetSendCTinfoBytes_LCDState(Val);//Convert.ToByte(boxid.Text)
            ComSend(sendbytes, sendbytes.Length);
        }
        /// <summary>
        /// 主柜提示
        /// </summary>
        /// <param name="drugcount"></param>
        /// <param name="showtype"></param>
        /// <param name="addr"></param>

        public void WriteMainLedTipCurr(string Val)
        {
            WMessage.Enqueue(Val);
        }
        /// <summary>
        /// 查询称重
        /// </summary>
        ///  <param name="adr">设备ID</param>
        ///  <param name="wadr">托盘Id</param>
        public void SendWeigth(string adr,byte wadr)
        {
            byte[] sendbytes = GetWeigthSendBytes(0x05, 0x00, wadr, Convert.ToUInt16(adr));//Convert.ToByte(boxid.Text)
            ComSend(sendbytes, sendbytes.Length);
        }
        /// <summary>
        /// 称重标定
        /// </summary>
        /// <param name="addr">设备ID</param>
        /// <param name="waddr">托盘地址</param>
        /// <param name="Scaletype">标定类型</param>
        public void SendScale(string addr, byte waddr, byte Scaletype)
        {
            byte[] sendbytes = GetweigthBGBytes(0x05, 0x01, waddr, Scaletype, Convert.ToUInt16(addr));//
            ComSend(sendbytes, sendbytes.Length);
        }
        /// <summary>
        /// 电子称清零
        /// </summary>
        /// <param name="addr">设备ID</param>
        /// <param name="waddr">托盘地址</param>
        public void SendWeigthClear(string addr, byte waddr)
        {
           byte[] sendbytes = GetWeigthSendBytes(0x05, 0x02, waddr, Convert.ToUInt16(addr));//
            ComSend(sendbytes, sendbytes.Length);
        }
        /// <summary>
        /// 设置电子称单重
        /// </summary>
        /// <param name="addr">设备ID</param>
        /// <param name="waddr">托盘地址</param>
        public void SendWeigthOneSet(byte weigthNo, ushort fvalue, ushort addr)
        {
            byte[] sendbytes = GetWeigthOneSet( weigthNo,  fvalue,  addr);//
            ComSend(sendbytes, sendbytes.Length);
        }

        /// <summary>
        /// 设置单重
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetWeigthOneSet(byte weigthNo, ushort fvalue, ushort addr)
        {
            setRepackNo();
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x0A, 0x05, 0x04, weigthNo, (byte)((fvalue & 0xFF00) >> 8), (byte)(fvalue & 0xFF), RepackNo };
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants.GetRange(0, 8));
            head = 8;
            head <<= 4;
            head += (byte)(bytescontants.Count - 8);
            List<byte> sendbytes2 = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes2.AddRange(bytescontants.GetRange(8, bytescontants.Count - 8));
            for (int k = 0; k < 8 - bytescontants.Count % 8; k++)
                sendbytes2.Add(0);
            sendbytes.AddRange(sendbytes2);
            return sendbytes.ToArray();
        }


        /// <summary>
        /// 电子称写LED屏
        /// </summary>
        /// <param name="addr">设备ID</param>
        /// <param name="waddr">托盘地址</param>
        public void SendSendCTinfoBytes_LED(string context,byte LedLine,string addr )
        {
            byte[] sendbytes = GetSendCTinfoBytes_LED(context, LedLine, ushort.Parse(addr));
            ComSend(sendbytes, sendbytes.Length);
        }

        /// <summary>
        /// 电子称写LED大屏
        /// </summary>
        /// <param name="addr">设备ID</param>
        /// <param name="waddr">托盘地址</param>
        public void SendSendBig_LED(string context, byte LedLine, string addr)
        {
            byte[] sendbytes = GetSendCTinfoBytes_LEDUpdate(context, LedLine, 0x01, ushort.Parse(addr));
            ComSend(sendbytes, sendbytes.Length);
        }
         
        /// <summary>
        /// 电子称写LED小屏
        /// </summary>
        /// <param name="addr">设备ID</param>
        /// <param name="waddr">托盘地址</param>
        public void SendSendSm_LED(string context, string addr)
        {
            string _comntext = context  + addr;
            LedMessage.Enqueue(_comntext);
        }
        /// <summary>
        /// 电子称写LED小屏
        /// </summary>
        /// <param name="addr">设备ID</param>
        /// <param name="waddr">托盘地址</param>
        public void SendSendSm_LED2(string Smcontext)
        {
            byte[] sendbytes = GetSendCTinfoBytes_LCD(Smcontext);
            ComSend(sendbytes, sendbytes.Length);
        }

        /// <summary>
        /// 设置LCD屏显示信息
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetSendCTinfoBytes_LCD(object drugname)
        {
            string[] datas = drugname.ToString().Split('#');
            ushort addr = ushort.Parse(datas[2]);
            //      string viewinfo, byte HH, ushort addr
            setRepackNo();
            byte[] viewinfobytes = System.Text.Encoding.GetEncoding("gb2312").GetBytes(datas[0]);
            int viewinfobyteslen = viewinfobytes.Length;
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, 0x03, 0x01, byte.Parse(datas[1]) };
            bytescontants.AddRange(viewinfobytes);
            bytescontants.Add(RepackNo);
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));

            byte head = 8;
            head <<= 4;
            head += 8;

            int counnum = bytescontants.Count % 8 == 0 ? bytescontants.Count / 8 : bytescontants.Count / 8 + 1;
            int ys = bytescontants.Count % 8;

            List<byte> sendbytes = new List<byte>();
            for (int k = 0; k < counnum; k++)
            {
                if (ys == 0)
                {
                    if (k < counnum)
                    {
                        sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                        sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                    }
                }
                else
                {
                    if (k < counnum - 1)
                    {
                        sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                        sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                    }
                    else
                    {

                        head = 8;
                        head <<= 4;
                        head += (byte)(ys == 0 ? 8 : ys);
                        sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                        sendbytes.AddRange(bytescontants.GetRange(k * 8, ys));

                        for (int n = 0; n < 8 - ys; n++)
                            sendbytes.Add(0);

                    }
                }
            }
            return sendbytes.ToArray();
        }
        /// <summary>
        ///  设置LED屏显示信息
        /// </summary>
        /// <param name="viewinfo"></param>
        /// <param name="HH">设置A-J</param>
      
        /// <returns></returns>
        public byte[] GetSendCTinfoBytes_LEDUpdate(string viewinfo, byte HH, byte showtype, ushort addr)
        {
            setRepackNo();
            byte[] viewinfobytes = System.Text.Encoding.GetEncoding("gb2312").GetBytes(viewinfo);
            int viewinfobyteslen = viewinfobytes.Length;
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, 0x02, 0x01, HH, showtype };
            bytescontants.AddRange(viewinfobytes);
            bytescontants.Add(RepackNo);
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            int counnum = bytescontants.Count % 8 == 0 ? bytescontants.Count / 8 : bytescontants.Count / 8 + 1;
            int ys = bytescontants.Count % 8;

            List<byte> sendbytes = new List<byte>();
            for (int k = 0; k < counnum; k++)
            {
                if (k < counnum - 1)
                {
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                }
                else
                {
                    head = 8;
                    head <<= 4;
                    head += (byte)(ys == 0 ? 8 : ys);
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, ys == 0 ? 8 : ys));
                    if (ys != 0)
                    {
                        for (int n = 0; n < 8 - ys; n++)
                            sendbytes.Add(0);
                    }
                }
            }
            return sendbytes.ToArray();
        }


        /// <summary>
        ///  设置LED屏显示信息
        /// </summary>
        /// <param name="viewinfo"></param>
        /// <param name="HH">设置A-J</param>
        /// <param name="showtype">0x31	左移0x32	右移	0x33	静止
        //0x34	雪花
        //0x35	上移
        //0x36	下移
        //0x37	闪烁</param>
        /// <param name="addr"></param>
        /// <returns></returns>
        public byte[] GetSendCTinfoBytes_LED(string viewinfo, byte HH, byte showtype, ushort addr)
        {
            setRepackNo();
            byte[] viewinfobytes = System.Text.Encoding.GetEncoding("gb2312").GetBytes(viewinfo);
            int viewinfobyteslen = viewinfobytes.Length;
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, 0x02, 0x21, HH, showtype };
            bytescontants.AddRange(viewinfobytes);
            bytescontants.Add(RepackNo);
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            int counnum = bytescontants.Count % 8 == 0 ? bytescontants.Count / 8 : bytescontants.Count / 8 + 1;
            int ys = bytescontants.Count % 8;

            List<byte> sendbytes = new List<byte>();
            for (int k = 0; k < counnum; k++)
            {
                if (k < counnum - 1)
                {
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                }
                else
                {
                    head = 8;
                    head <<= 4;
                    head += (byte)(ys == 0 ? 8 : ys);
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, ys == 0 ? 8 : ys));
                    if (ys != 0)
                    {
                        for (int n = 0; n < 8 - ys; n++)
                            sendbytes.Add(0);
                    }
                }
            }
            return sendbytes.ToArray();
        }


        /// <summary>
        /// 设置LED大屏显示信息
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        public byte[] GetSendCTinfoBytes_SetLED(byte[] viewinfo, ushort addr)
        {
            setRepackNo();
            byte[] viewinfobytes = viewinfo;

            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, 0x02, 0x24 };
            bytescontants.AddRange(viewinfobytes);
            bytescontants.Add(RepackNo);
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            int counnum = bytescontants.Count % 8 == 0 ? bytescontants.Count / 8 : bytescontants.Count / 8 + 1;
            int ys = bytescontants.Count % 8;

            List<byte> sendbytes = new List<byte>();
            for (int k = 0; k < counnum; k++)
            {
                if (k < counnum - 1)
                {
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                }
                else
                {
                    head = 8;
                    head <<= 4;
                    head += (byte)(ys == 0 ? 8 : ys);
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, ys == 0 ? 8 : ys));
                    if (ys != 0)
                    {
                        for (int n = 0; n < 8 - ys; n++)
                            sendbytes.Add(0);
                    }
                }
            }
            return sendbytes.ToArray();
        }
        /// <summary>
        /// 设置LCD屏显示信息
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetSendCTinfoBytes_LED(string context, byte LedLine, ushort addr)
        {
            setRepackNo();
            
            byte[] viewinfobytes = System.Text.Encoding.GetEncoding("gb2312").GetBytes(context);
            int viewinfobyteslen = viewinfobytes.Length;
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, 0x03, 0x01, LedLine };
            bytescontants.AddRange(viewinfobytes);
            bytescontants.Add(RepackNo);
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));

            byte head = 8;
            head <<= 4;
            head += 8;

            int counnum = bytescontants.Count % 8 == 0 ? bytescontants.Count / 8 : bytescontants.Count / 8 + 1;
            int ys = bytescontants.Count % 8;

            List<byte> sendbytes = new List<byte>();
            for (int k = 0; k < counnum; k++)
            {
                if (ys == 0)
                {
                    if (k < counnum)
                    {
                        sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                        sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                    }
                }
                else
                {
                    if (k < counnum - 1)
                    {
                        sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                        sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                    }
                    else
                    {

                        head = 8;
                        head <<= 4;
                        head += (byte)(ys == 0 ? 8 : ys);
                        sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                        sendbytes.AddRange(bytescontants.GetRange(k * 8, ys));

                        for (int n = 0; n < 8 - ys; n++)
                            sendbytes.Add(0);

                    }
                }
            }
            return sendbytes.ToArray();
        }
        /// <summary>
        /// 设置LCD屏小屏显示信息
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetSendCTinfoBytes_LCDState(object drugname)
        {
            string[] datas = drugname.ToString().Split('#');
            ushort addr = ushort.Parse(datas[3]);
            //      string viewinfo, byte HH, ushort addr
            setRepackNo();
            byte[] viewinfobytes = new byte[3];

            viewinfobytes[0] = byte.Parse(datas[1]);
            viewinfobytes[1] = 0x00;
            viewinfobytes[2] = byte.Parse(datas[0]);
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, 0x03, 0x04, byte.Parse(datas[2]) };
            bytescontants.AddRange(viewinfobytes);
            bytescontants.Add(RepackNo);
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));

            byte head = 8;
            head <<= 4;
            head += 8;

            int counnum = bytescontants.Count % 8 == 0 ? bytescontants.Count / 8 : bytescontants.Count / 8 + 1;
            int ys = bytescontants.Count % 8;

            List<byte> sendbytes = new List<byte>();
            for (int k = 0; k < counnum; k++)
            {
                if (ys == 0)
                {
                    if (k < counnum)
                    {
                        sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                        sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                    }
                }
                else
                {
                    if (k < counnum - 1)
                    {
                        sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                        sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                    }
                    else
                    {

                        head = 8;
                        head <<= 4;
                        head += (byte)(ys == 0 ? 8 : ys);
                        sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                        sendbytes.AddRange(bytescontants.GetRange(k * 8, ys));

                        for (int n = 0; n < 8 - ys; n++)
                            sendbytes.Add(0);
                    }
                }
            }
            return sendbytes.ToArray();
        }

        /// <summary>
        /// 锁操作
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetKSBytes(byte gnm, byte feature, ushort addr)
        {
            setRepackNo();
            
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x08, gnm, feature, KSNO, RepackNo };//receives[receives[1]-3]
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));

            List<byte> sendbytes = new List<byte>() { 0x88, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants);
            return sendbytes.ToArray();
        }
        /// <summary>
        /// 锁操作
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetKSBytes(byte gnm, byte feature, byte KSNo, ushort addr)
        {
            setRepackNo();
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x08, gnm, feature, KSNo, RepackNo };//receives[receives[1]-3]
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));

            List<byte> sendbytes = new List<byte>() { 0x88, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants);
            return sendbytes.ToArray();
        }

      

        /// <summary>
        /// 称重,清零
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetWeigthSendBytes(byte gnm, byte feature, byte fvalue, ushort addr)
        {
            setRepackNo();
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x08, gnm, feature, fvalue, RepackNo };
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));

            byte head = 8;
            head <<= 4;
            head += (byte)bytescontants.Count;
            bytescontants.Add(0x00);
            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants);
            return sendbytes.ToArray();
        }

        /// <summary>
        /// 设置LCD屏显示信息
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        public byte[] GetSendCTinfoBytes_TFT_LCD(string viewinfo, byte HH, ushort addr)
        {
            setRepackNo();
            byte[] viewinfobytes = System.Text.Encoding.GetEncoding("gb2312").GetBytes(viewinfo);
            int viewinfobyteslen = viewinfobytes.Length;
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, 0x04, 0x01, HH };
            bytescontants.AddRange(viewinfobytes);
            bytescontants.Add(RepackNo);
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            int counnum = bytescontants.Count % 8 == 0 ? bytescontants.Count / 8 : bytescontants.Count / 8 + 1;
            int ys = bytescontants.Count % 8;

            List<byte> sendbytes = new List<byte>();
            for (int k = 0; k < counnum; k++)
            {
                if (k < counnum - 1)
                {
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                }
                else
                {
                    head = 8;
                    head <<= 4;
                    head += (byte)(ys == 0 ? 8 : ys);
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, ys == 0 ? 8 : ys));
                    if (ys != 0)
                    {
                        for (int n = 0; n < 8 - ys; n++)
                            sendbytes.Add(0);
                    }
                }
            }
            return sendbytes.ToArray();
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        /// <param name="showtype">0：关闭提示	1：提示加药	2：提示取药</param>
        /// <param name="addr">地址</param>
        /// <returns></returns>
        public byte[] GetSendCTinfoBytes_TFT_LCDState(byte drugcount, byte showtype, ushort addr)
        {
            setRepackNo();
            byte[] viewinfobytes = new byte[2];
            viewinfobytes[0] = 0x00;
            viewinfobytes[1] = drugcount;
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, 0x04, 0x03, showtype };
            bytescontants.AddRange(viewinfobytes);
            bytescontants.Add(RepackNo);
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            int counnum = bytescontants.Count % 8 == 0 ? bytescontants.Count / 8 : bytescontants.Count / 8 + 1;
            int ys = bytescontants.Count % 8;

            List<byte> sendbytes = new List<byte>();
            for (int k = 0; k < counnum; k++)
            {
                if (k < counnum - 1)
                {
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, 8));
                }
                else
                {
                    head = 8;
                    head <<= 4;
                    head += (byte)(ys == 0 ? 8 : ys);
                    sendbytes.AddRange(new byte[] { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) });
                    sendbytes.AddRange(bytescontants.GetRange(k * 8, ys == 0 ? 8 : ys));
                    if (ys != 0)
                    {
                        for (int n = 0; n < 8 - ys; n++)
                            sendbytes.Add(0);
                    }
                }
            }
            return sendbytes.ToArray();
        }
       

        /// <summary>
        /// 开,关 灯
        /// </summary>
        /// <param name="add r"></param>
        private byte[] SendKD(byte gnm, byte feature, byte dxh, byte zt, ushort addr)
        {
            setRepackNo();
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, gnm, feature, dxh, zt, RepackNo };
            // (byte)(datas), (byte)(datas >> 8), (byte)(datas >> 16), (byte)(datas >> 24), (byte)(datas >> 32), (byte)(datas >> 40),
            //  (byte)(datas >> 40), (byte)(datas >> 32), (byte)(datas >> 24), (byte)(datas >> 16), (byte)(datas >> 8), (byte)(datas)
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants.GetRange(0, 8));
            head = 8;
            head <<= 4;
            head += (byte)(bytescontants.Count - 8);
            List<byte> sendbytes2 = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes2.AddRange(bytescontants.GetRange(8, bytescontants.Count - 8));
            for (int k = 0; k < 8 - bytescontants.Count % 8; k++)
                sendbytes2.Add(0);
            sendbytes.AddRange(sendbytes2);
            return sendbytes.ToArray();
          
        }

        /// <summary>
        /// 称重标定
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetweigthBGBytes(byte gnm, byte feature, byte sumall, byte typen, ushort addr)
        {

            setRepackNo();
            
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, gnm, feature, sumall, typen, RepackNo };

            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants.GetRange(0, 8));
            head = 8;
            head <<= 4;
            head += (byte)(bytescontants.Count - 8);
            List<byte> sendbytes2 = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes2.AddRange(bytescontants.GetRange(8, bytescontants.Count - 8));
            for (int k = 0; k < 8 - bytescontants.Count % 8; k++)
                sendbytes2.Add(0);
            sendbytes.AddRange(sendbytes2);
            return sendbytes.ToArray();
        }

        //回收箱开锁
        public void SendKS_only(string addr)
        {
            byte[] sendbytes = GetKSBytes(0x01, 0x01, Convert.ToUInt16(addr));//Convert.ToByte(boxid.Text)
            //LogManager.WriteLogSave("SendKS_only", CommanClass.comm.CRC.byteToHexStr(sendbytes));
            ComSend(sendbytes, sendbytes.Length);
        }

      
        /// <summary>
        /// 读重量
        /// </summary>
        /// <param name="addr"></param>
        private void SendReadCount(string addr, byte waddr)
        {
            byte[] sendbytes = GetWeigthSendBytes(0x05, 0x05, waddr, Convert.ToUInt16(addr));
            ComSend(sendbytes, sendbytes.Length);
        }


        #endregion
        /// <summary>
        /// 分析出数据给配置
        /// </summary>
        /// <param name="bytes"></param>
       #region 精准药盒处理过程
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="RType"></param>
        public void SetReceiveType(object obj, CmdType RType)
        {
            if (ReceiveType != null)
            {
                ReceiveType(obj, RType);
            }
        }



        /// <summary>
        /// 精准亮灯
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetSendBytes(byte gnm, byte feature, ushort addr, byte byte1, byte byte2)
        {

            setRepackNo();
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, gnm, feature, byte1, byte2, RepackNo };
            // (byte)(datas), (byte)(datas >> 8), (byte)(datas >> 16), (byte)(datas >> 24), (byte)(datas >> 32), (byte)(datas >> 40),
            //  (byte)(datas >> 40), (byte)(datas >> 32), (byte)(datas >> 24), (byte)(datas >> 16), (byte)(datas >> 8), (byte)(datas)
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants.GetRange(0, 8));
            head = 8;
            head <<= 4;
            head += (byte)(bytescontants.Count - 8);
            List<byte> sendbytes2 = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes2.AddRange(bytescontants.GetRange(8, bytescontants.Count - 8));
            for (int k = 0; k < 8 - bytescontants.Count % 8; k++)
                sendbytes2.Add(0);
            sendbytes.AddRange(sendbytes2);
            return sendbytes.ToArray();
        }
        /// <summary>
        /// 精准亮灯
        /// </summary>
        /// <param name="addr"></param>
        public void sendLD(ushort addr, byte state)
        {
            try
            {
                byte[] sendbytes = GetSendBytes(0x07, 0x04, addr, 0x01, state);//Convert.ToByte(boxid.Text)

                ComSend(sendbytes, sendbytes.Length);
            }
            catch 
            {
               
            }
        }

        /// <summary>
        /// 精准药品添加
        /// </summary>
        /// <param name="adr">设备ID</param>
        public void DrugAdd(string adr)
        {
            try
            {
                byte[] sendbytes = GetSendBytes(0x07, 0x03, Convert.ToUInt16(adr));  //Convert.ToByte(boxid.Text)

                //Readboxspec(adr, qty);

                ComSend(sendbytes, sendbytes.Length);
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("中控药品加入异常", "药品加入异常", _sp, el);
            }
        }
        /// <summary>
        /// 读取精准药盒规格
        /// </summary>
        /// <param name="adr">设备ID</param>
        public void ReadDrugbox(string adr)
        {
            byte[] sendbytes = GetSendSetBytes_YQXX(0x07, 0x06, Convert.ToUInt16(adr));//Convert.ToByte(boxid.Text)
            ComSend(sendbytes, sendbytes.Length);
        }
        /// <summary>
        /// 读取加药盒规格
        /// </summary>
        /// <param name="adr">设备ID</param>
        public void Readboxspec(string adr)
        {
            byte[] sendbytes = GetSendSetBytes_YQXX(0x08, 0x04, Convert.ToUInt16(adr));//Convert.ToByte(boxid.Text)
            ComSend(sendbytes, sendbytes.Length);
        }

        /// <summary>
        /// 退药
        /// </summary>
        /// <param name="adr"></param>
        /// <param name="quantity"></param>
        public void ReadboxspecReturn(string adr, int quantity)
        {

            byte[] sendbytes = GetSendBytes_druReturn(0x07, 0x08, ushort.Parse(adr), quantity);// Convert.ToUInt64(stateparam,2));//Convert.ToByte(boxid.Text)
            ComSend(sendbytes, sendbytes.Length);


        }

        /// <summary>
        /// 退药
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetSendBytes_druReturn(byte gnm, byte feature, ushort addr, Int32 datas)
        {
            setRepackNo();
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x0D, gnm, feature, 0x00, 0x00, 0x00, (byte)(datas >> 16), (byte)(datas >> 8), (byte)datas, RepackNo };
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants.GetRange(0, 8));
            head = 8;
            head <<= 4;
            head += (byte)(bytescontants.Count - 8);
            List<byte> sendbytes2 = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes2.AddRange(bytescontants.GetRange(8, bytescontants.Count - 8));
            for (int k = 0; k < 8 - bytescontants.Count % 8; k++)
                sendbytes2.Add(0);
            sendbytes.AddRange(sendbytes2);
            return sendbytes.ToArray();
        }


       
        /// <summary>
        /// 查询药盒药品数量
        /// </summary>
        /// <param name="adr">设备ID</param>
        public void QDrugCount(string adr)
        {
            byte[] sendbytes = GetSendBytes(0x07, 0x00, Convert.ToUInt16(adr));
            ComSend(sendbytes, sendbytes.Length);
        }

        /// <summary>
        /// 查询规格
        /// </summary>
        /// <param name="boxid"></param>
        public void sendCheckBoxSpec(object boxid)
        {
            try
            {
                byte[] sendbytes = GetSendSetBytes_YQXX(0x07, 0x06, Convert.ToUInt16(boxid.ToString()));//Convert.ToByte(boxid.Text)
                ComSend(sendbytes, sendbytes.Length);
            }
            catch (Exception ex)
            {
                LogManager.WriteErrorLog("中控精准药品出药异常", "精准药品出药异常", _sp, ex);
            }
        }
        /// <summary>
        /// 精准药品出药
        /// </summary>
        /// <param name="adr">设备ID</param>
        /// <param name="outcount">出药数量</param>
        public void DrugOut(string adr, string outcount)
        {
            try
            {
                byte[] sendbytes = GetSendBytes_drugOut(0x07, 0x01, Convert.ToUInt16(adr), Convert.ToInt32(outcount));// Convert.ToUInt64(stateparam,2));//Convert.ToByte(boxid.Text)
                ComSend(sendbytes, sendbytes.Length);
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("中控精准药品出药异常", "精准药品出药异常", _sp, el);
            }
        }
        /// <summary>
        /// 精准药盒统计
        /// </summary>
        /// <param name="adr">设备ID</param>
        public void DrugStatisic(string adr)
        {
            try
            {
                byte[] sendbytes = GetSendBytes(0x07, 0x02, Convert.ToUInt16(adr));
              
                ComSend(sendbytes, sendbytes.Length);
            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("中控精准药品出药异常", "精准药品出药异常", _sp, el);
            }
        }
        /// <summary>
        /// 精准取药
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetSendBytes_drugOut(byte gnm, byte feature, ushort addr, Int32 datas)
        {
            setRepackNo();
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, gnm, feature, 0x01, (byte)datas, RepackNo };
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += 8;

            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants.GetRange(0, 8));
            head = 8;
            head <<= 4;
            head += (byte)(bytescontants.Count - 8);
            List<byte> sendbytes2 = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes2.AddRange(bytescontants.GetRange(8, bytescontants.Count - 8));
            for (int k = 0; k < 8 - bytescontants.Count % 8; k++)
                sendbytes2.Add(0);
            sendbytes.AddRange(sendbytes2);
            return sendbytes.ToArray();
        }


        /// <summary>
        /// 获取药枪信息
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetSendSetBytes_YQXX(byte gnm, byte feature, ushort addr)
        {
            setRepackNo();
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x09, gnm, feature, RepackNo };
            bytescontants[1] = (byte)(bytescontants.Count + 2);
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += (byte)bytescontants.Count;
            for (int k = 0; k < 8 - bytescontants.Count % 8; k++)
                bytescontants.Add(0);
            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants);

            return sendbytes.ToArray();
        }
        /// <summary>
        /// 功能码,特征码
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetSendBytes(byte gnm, byte feature, ushort addr)
        {
            setRepackNo();
            
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x07, gnm, feature, RepackNo };
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));

            byte head = 8;
            head <<= 4;
            head += (byte)bytescontants.Count;
            bytescontants.Add(0x00);
            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants);
            return sendbytes.ToArray();
        }



        /// <summary>
        /// 功能码,特征码
        /// </summary>
        /// <param name="gnm"></param>
        /// <returns></returns>
        private byte[] GetSendBytesQDrugCount(byte gnm, byte feature, ushort addr)
        {
            setRepackNo();
            List<byte> bytescontants = new List<byte>() { 0x5A, 0x07, gnm, feature, RepackNo };
            ushort CRC = ComCRC.CRC16(bytescontants.ToArray(), bytescontants.Count);
            bytescontants.Add((byte)((CRC & 0xFF00) >> 8));
            bytescontants.Add((byte)(CRC & 0xFF));
            byte head = 8;
            head <<= 4;
            head += (byte)bytescontants.Count;
            bytescontants.Add(0x00);
            List<byte> sendbytes = new List<byte>() { head, 0x00, 0x00, (byte)((addr & 0xFF00) >> 8), (byte)(addr & 0xFF) };
            sendbytes.AddRange(bytescontants);
            return sendbytes.ToArray();
        }
        private void setRepackNo()
        {

            if (RepackNo < 255)
            {
               ++RepackNo;
            }
            else
            {
                RepackNo = 0;
            }

        }

        #endregion

    }  
    public enum CmdType
        {
            /// <summary>
            /// 开锁回复
            /// </summary>
            Cmd_KS,
        /// <summary>
        /// 关锁回复
        /// </summary>
            Cmd_GS,
        /// <summary>
        /// 查询精准盒药数
        /// </summary>
            Cmd_QCount,
            /// <summary>
            /// 精准盒出药数量
            /// </summary>
            Cmd_OutCount,
            /// <summary>
            /// 精准盒加药数量
            /// </summary>
            Cmd_InCount,
            /// <summary>
            /// 精准盒异常
            /// </summary>
            Cmd_InERRCount,
            /// <summary>
            /// 精准盒统计数量
            /// </summary>
            Cmd_TJCount,
            /// <summary>
            /// 药品规格
            /// </summary>
            Cmd_BoxSpec,
            /// <summary>
            /// 加药药品规格
            /// </summary>
            Cmd_DrugBoxSpec,
          
            /// <summary>
            /// 称重取药
            /// </summary>
            Cmd_TakeDrugEnd_Weigth,
            /// <summary>
            /// 显示药盒数量
            /// </summary>
            Cmd_SendBoxDrugs,
            Cmd_CheckBoxSpec,
            Cmd_CheckBoxSpec_HF,
            /// <summary>
            /// 称重
            /// </summary>
            Cmd_Weight,
            /// <summary>
            /// 称重数量
            /// </summary>
            Cmd_WeightCount,
            /// <summary>
            /// 称重数量
            /// </summary>
            Cmd_WeightMainCount,
            /// <summary>
            /// 辅柜数量
            /// </summary>
            Cmd_FWeightCount,
            /// <summary>
            /// 整盘称重
            /// </summary>
            Cmd_AllWeight,
            /// <summary>
            /// 标定
            /// </summary>
            Cmd_WeightBD,
            /// <summary>
            /// 清零
            /// </summary>
            Cmd_WeightQL,
            /// <summary>
            /// 设置单重
            /// </summary>
            Cmd_SetWeight,
            /// <summary>
            /// 退药
            /// </summary>
             Cmd_DrugsOut,
            /// <summary>
            /// 加药
            /// </summary>
            Cmd_DrugsIn,
            /// <summary>
            /// 亮灯
            /// </summary>
            Cmd_LD,
            /// <summary>
            /// 查询设备
            /// </summary>
            Cmd_ReadDevID,
             /// <summary>
            /// 查询设备
            /// </summary>
            Cmd_ErrMsg,
            Cmd_ReView
        }
}
