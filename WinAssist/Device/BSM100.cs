using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WinAssist.Comm;

namespace WinAssist.Device
{
    public class BSM100 : CenterCom
    {
        public string DevVersionName = "";
        public BSM100()
        {
            //默认Y310
            DevVersionName = "BSM100";
        }
        public BSM100(string strVersion)
        {
            //默认Y310
            DevVersionName = strVersion;
        }

        public override void AnayzeData(byte[] bytes, List<byte> newbytes, byte Modeltype)
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
                            if (DevID.ToString() == "21")
                            {
                                //查辅柜的称重数量
                                for (int i = 1; i < 21; i++)
                                {
                                    QMessage.Enqueue(i.ToString());
                                }
                            }
                            if (DevID.ToString() == "100")
                            {
                                //查主柜的称重数量
                                if (receiveBytes[4] == 0x01 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("22");
                                    QMessage.Enqueue("23");
                                    QMessage.Enqueue("24");
                                }
                                if (receiveBytes[4] == 0x02 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("25");
                                    QMessage.Enqueue("26");
                                    QMessage.Enqueue("27");
                                }
                                if (receiveBytes[4] == 0x03 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("28");
                                    QMessage.Enqueue("29");
                                    QMessage.Enqueue("30");
                                }
                                if (receiveBytes[4] == 0x04 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("31");
                                    QMessage.Enqueue("32");
                                    QMessage.Enqueue("33");
                                }
                                if (receiveBytes[4] == 0x05 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("34");
                                    QMessage.Enqueue("35");
                                    QMessage.Enqueue("36");
                                }
                                if (receiveBytes[4] == 0x06 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("37");
                                    QMessage.Enqueue("38");
                                    QMessage.Enqueue("39");
                                }
                                if (receiveBytes[4] == 0x07 && receiveBytes[5] == 0x01)
                                {
                                    QMessage.Enqueue("40");
                                   
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

                                    if (DevID < 21)
                                    {
                                        _tempCount.Add(1, weightNums1.ToString());
                                        SetReceiveType(_tempCount, CmdType.Cmd_FWeightCount);
                                    }//查主柜一个盘
                                    else if (DevID > 21&& DevID < 49)
                                    {
                                        weightNums2 = receiveBytes[7];
                                        weightNums2 <<= 8;
                                        weightNums2 += receiveBytes[8];
                                        
                                      
                                        _tempCount.Add(1, weightNums1.ToString());
                                        _tempCount.Add(2, weightNums2.ToString());
                                        //三个盘
                                        if (DevID != 40)
                                        {
                                            ushort weightNums3 = 0;
                                            weightNums3 = receiveBytes[9];
                                            weightNums3 <<= 8;
                                            weightNums3 += receiveBytes[10];
                                            _tempCount.Add(3, weightNums3.ToString());
                                        }
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
     
    }
}
