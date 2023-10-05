using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WinAssist.Comm;
namespace WinAssist.Device
{

    //温度计数
    public  class CTemp
    {
        private SerialPort _spFinger;

        public Action< string> ComInfo;
        public CTemp()
        {
        }
        public Action<int, string> FingerComInfo;
        public void initCominfo(string portname, string BaudRate, string Parity, string DataBits, string StopBits)
        {
            int _BaudRate = int.Parse(BaudRate);
            Parity _Parity = (Parity)Enum.Parse(typeof(Parity), Parity, true);
            int _DataBits = int.Parse(DataBits);
            StopBits _StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBits, true);

            _spFinger = new SerialPort(portname, _BaudRate, _Parity, _DataBits, _StopBits);
            _spFinger.ReadTimeout = SerialPort.InfiniteTimeout;
            _spFinger.ReadBufferSize = 4096;
            _spFinger.DataReceived += _spReceive_DataReceived;
            try
            {
                _spFinger.Open();
            }
            catch (Exception el)
            {

            }

        }

        Byte[] receivedData = new Byte[200];        //创建接收字节数组
        int leng = 0;
        private void _spReceive_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                leng = _spFinger.BytesToRead;
                if (leng > 0)
                {
                    receivedData = new byte[leng];
                    leng = _spFinger.Read(receivedData, 0, receivedData.Length);
                    AddReceiveBytes(receivedData, leng);
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiveBytes"></param>
        /// <param name="length"></param>
        private void AddReceiveBytes(byte[] receiveBytes, int length)
        {

            try
            {
                DateTime dt = DateTime.Now;
                if (receiveBytes[0] == 0x01 && receiveBytes[1] == 0x03)
                {
                    int p1 = (int)receiveBytes[3] << 8;
                    p1 += (int)receiveBytes[4];
                    decimal Dtempvalue = p1;

                    if (ComInfo != null)
                    {
                        ComInfo(Dtempvalue.ToString());
                    }
                }

            }
            catch //(Exception ee)
            {
                // ErrorLog.WriteErrorLog("CClient_Communicate", "DoWork", null, ee);
            }
            finally
            {
                //GC.Collect();           
            }
        }
        /// <summary>
        /// 查询温度
        /// </summary>
        public void SendQry()
        {
            byte[] sendtemp1 = new byte[] {0x01, 0x03, 0x00,0x00,0x00, 0x02, 0xC4, 0x0B };
            ComSend(sendtemp1, sendtemp1.Length);
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
                if (_spFinger != null && _spFinger.IsOpen)
                    _spFinger.Write(bytes, 0, length);
             //   LogManager.WriteLogSave("WDSend", StrCom.byteToHexStr(bytes, bytes.Length));

            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("指纹发送据异常", "指纹发送", _spFinger, el);
            }
        }
    }
}
