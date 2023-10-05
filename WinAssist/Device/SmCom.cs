using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WinAssist.Comm;

namespace WinAssist.Device
{
    /// <summary>
    /// 扫码处理
    /// </summary>
    public  class SmCom
    {
        /// <summary>
        /// 扫描设备
        /// </summary>
        public SerialPort _spSM;

        public Action<string> smInfo;

        public void initCominfo(string portname, string BaudRate, string Parity, string DataBits, string StopBits)
        {
            int _BaudRate = int.Parse(BaudRate);
            Parity _Parity = (Parity)Enum.Parse(typeof(Parity), Parity, true);
            int _DataBits = int.Parse(DataBits);
            StopBits _StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBits, true);
            //string portname = _ini.IniReadValue("YuMiaoData", "PortName_SM");
            //int _BaudRate = int.Parse(_ini.IniReadValue("YuMiaoData", "BaudRate_SM"));
            //Parity _Parity = (Parity)Enum.Parse(typeof(Parity), _ini.IniReadValue("YuMiaoData", "Parity_SM"), true);
            //int _DataBits = int.Parse(_ini.IniReadValue("YuMiaoData", "DataBits_SM"));
            //StopBits _StopBits = (StopBits)Enum.Parse(typeof(StopBits), _ini.IniReadValue("YuMiaoData", "StopBits_SM"), true);

            _spSM = new SerialPort(portname, _BaudRate, _Parity, _DataBits, _StopBits);
            _spSM.ReadTimeout = SerialPort.InfiniteTimeout;
            _spSM.ReadBufferSize = 4096;
            _spSM.DataReceived += _spReceive_DataReceived_SM;
          
            try
            {
                _spSM.Open();
            }
            catch (Exception el)
            {
                
            }

        }

        Byte[] receivedData_SM = new Byte[500];        //创建接收字节数组
        int leng_SM = 0;
        private void _spReceive_DataReceived_SM(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(200);
                leng_SM = _spSM.BytesToRead;
                if (leng_SM > 0)
                {
                    receivedData_SM = new byte[leng_SM];
                    leng_SM = _spSM.Read(receivedData_SM, 0, receivedData_SM.Length);
                    if (smInfo != null)
                    {
                        string datas = System.Text.Encoding.GetEncoding("gb2312").GetString(receivedData_SM, 0, receivedData_SM.Length);//System.Text.Encoding.ASCII.GetString(receiveBytes, 0, receiveBytesLength);// (str1);// 
                        datas = Regex.Replace(datas, @"\r", "");
                        datas = Regex.Replace(datas, @"\n", "");
                        smInfo(datas);
                    }
                }
            }
            catch(Exception el)
            {
                LogManager.WriteLogSave("COMErr", "扫码数据" + el.Message.ToString());
            }

        }

    }
}
