using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WinAssist.Comm;

namespace WinAssist.Device
{
    public class CFingerCom
    {
        private SerialPort _spFinger;

        private string userNo;

        public Action<int,string> FingerComInfo;
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

        /// <summary>
        /// 手指编号
        /// </summary>
        int maxfings = -1;

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

        private void AddReceiveBytes(byte[] receiveBytes, int length)
        {

            try
            {
                DateTime dt = DateTime.Now;
                if (receiveBytes[0] == 0xAA && receiveBytes[1] == 0x55)
                {
                    byte _case = receiveBytes[2];
                    List<byte> sendfingers = new List<byte>();
                    switch (_case)
                    {
                        case 0x04:
                            
                            if (receiveBytes[6] == 0x00)
                            {// "该指纹注册成功！";
                                 maxfings = (int)receiveBytes[8];
                                 if (maxfings < 240)
                                 {
                                     if (FingerComInfo != null)
                                     {
                                         FingerComInfo(1, receiveBytes[8].ToString());
                                     }
                                 }
                                
                            }
                            else if (receiveBytes[6] == 0x01)
                            {
                                showmessage(receiveBytes[8]);
                            }
                            break;
                        case 0x02:
                            if (receiveBytes[6] == 0x00)
                            {
                                // "该指纹登陆成功！";
                                 maxfings = (int)receiveBytes[8];
                                 if (maxfings < 240)
                                 {
                                     FingerComInfo(3, receiveBytes[8].ToString());
                                 }
                            }
                            else if (receiveBytes[6] == 0x01)
                            {
                                // "该指纹登陆失败！";
                                FingerComInfo(2, "指纹登陆失败！" + receiveBytes[8].ToString());
                            }
                            break;

                        case 0x05:
                            if (receiveBytes[6] == 0x00)
                            {
                                // "该指纹删除成功！";
                                FingerComInfo(5, receiveBytes[8].ToString());
                            }
                            else if (receiveBytes[6] == 0x01)
                            {
                                // "该指纹删除失败！";
                                FingerComInfo(6, receiveBytes[8].ToString());
                            }
                            break;

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
        private void showmessage(byte index)
        {
            string mes = "";
            switch (index)
            {
                case 1:
                    mes = "操作失败！";
                    break;
                case 4:
                    mes = "指文数据库已满！";
                    break;
                case 5:
                    mes = "无此用户！";
                    break;
                case 6:
                    mes = "用户已存在！";
                    break;
                case 7:
                    mes = "指文已存在！";
                    break;
                case 8:
                    mes = "采集超时！";
                    break;

                case 0x60:
                    mes = "指定的编号无效！";
                    break;
                case 0x23:
                    mes = "指令超时！";
                    break;
                case 0x14:
                    mes = "指定的编号有模板！";

                    break;
                case 0x41:
                    mes = "指令已被取消！";
                    break;
                case 0x21:
                    mes = "指纹图像质量不好！";
                    break;
                case 0x30:
                    mes = "录入指纹模板失败！";
                    break;
                case 0x19:
                    mes = "该指纹已登记！";
                    break;
                case 0x51:
                    mes = "软件内部错误！";
                    break;
                case 0xFF:
                    mes = "软件内部错误！";
                    break;
            }
            if (FingerComInfo != null)
            {
                FingerComInfo(0, mes);
            }
        }

        public void ReFingerState(ushort mfings)
        {

            Run_Command_1P((ushort)(CommonDefine.CMD_ENROLL_ONETIME_CODE), mfings);
            List<byte> templist = new List<byte>();
            for (int i = 0; i < 24; i++)
            {
                templist.Add(m_pPacketBuffer[i]);
            }

            ComSend(templist.ToArray(), templist.Count);
        }

        public bool Run_Command_1P(ushort p_nCmd, ushort p_nData)
        {
            bool w_bRet = false;

            // Disable Disconnect Button
            // Assemble Command Packet
            InitPacket(p_nCmd, true);
            SET_PACKET_LEN((ushort)2, false);
            SET_PACKET_CMDDATA(p_nData, 0, false);
            AddCheckSum(true);
            // Return
            return w_bRet;
        }

        public bool Run_Command_NP(ushort p_nCmd)
        {
            bool w_bRet = false;


            // Assemble command packet
            InitPacket(p_nCmd, true);
            AddCheckSum(true);

            // Display command information
            // Run Send Thread
            // Return
            return w_bRet;
        }

        public void delfinger(ushort fingerid)
        {
            Run_Command_1P((ushort)(CommonDefine.CMD_CLEAR_TEMPLATE_CODE), fingerid);
            List<byte> templist = new List<byte>();
            for (int i = 0; i < 24; i++)
            {
                templist.Add(m_pPacketBuffer[i]);
            }
            ComSend(templist.ToArray(), templist.Count);
        }
        

        public void SendDelFinger()
        {
            // List<byte> sendfingers = new List<byte>() { 0xF5, 0x0C, 0x00, 0x00, 0x00, 0x00 };//{ 0x55, 0xAA, 0x03, 0x01, 0x02, 0x00, (byte)((maxfings & 0xFF00) >> 8), (byte)(maxfings & 0xFF), 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //sendfingers.Add(getorcrc(sendfingers, 1, 6));
            //sendfingers.Add(0xF5);
            Run_Command_NP(CommonDefine.CMD_CLEAR_ALLTEMPLATE_CODE);
            List<byte> templist = new List<byte>();
            for (int i = 0; i < 24; i++)
            {
                templist.Add(m_pPacketBuffer[i]);
            }
            ComSend(templist.ToArray(), templist.Count);
        }

        public void SendLoginFinger()
        {
            Run_Command_NP(CommonDefine.CMD_IDENTIFY_CODE);
            ComSend(m_pPacketBuffer, m_pPacketBuffer.Count());
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
                LogManager.WriteLogSave("ZWSend", StrCom.byteToHexStr(bytes, bytes.Length));

            }
            catch (Exception el)
            {
                LogManager.WriteErrorLog("指纹发送据异常", "指纹发送", _spFinger, el);
            }
        }

        #region 指纹枚举


        byte[] m_pPacketBuffer = new byte[99];
        byte[] m_pPacketBuffer0 = new byte[99];
        public void InitPacket(ushort p_nCMD, bool p_bCmdData)
        {
            // Clear Buffer
            Array.Clear(m_pPacketBuffer, 0, 99);

            // Make Packet
            if (p_bCmdData)
                SET_PACKET_PREFIX(CommonDefine.CMD_PREFIX_CODE, false);
            else
                SET_PACKET_PREFIX(CommonDefine.CMD_DATA_PREFIX_CODE, false);
            SET_PACKET_COMMAND(p_nCMD, false);
        }

        public void SET_PACKET_LEN(ushort p_nLen, bool p_bCancel)
        {
            if (p_bCancel)
            {
                m_pPacketBuffer0[4] = LOBYTE(p_nLen);
                m_pPacketBuffer0[5] = HIBYTE(p_nLen);
            }
            else
            {
                m_pPacketBuffer[4] = LOBYTE(p_nLen);
                m_pPacketBuffer[5] = HIBYTE(p_nLen);
            }
        }

        public void SET_PACKET_CMDDATA(ushort p_nValue, int p_nOffset, bool p_bCancel)
        {
            if (p_bCancel)
            {
                m_pPacketBuffer0[6 + p_nOffset] = LOBYTE(p_nValue);
                m_pPacketBuffer0[7 + p_nOffset] = HIBYTE(p_nValue);
            }
            else
            {
                m_pPacketBuffer[6 + p_nOffset] = LOBYTE(p_nValue);
                m_pPacketBuffer[7 + p_nOffset] = HIBYTE(p_nValue);
            }
        }

        public void SET_PACKET_PREFIX(ushort p_nPrefix, bool p_bCancel)
        {
            if (p_bCancel)
            {
                m_pPacketBuffer0[0] = LOBYTE(p_nPrefix);
                m_pPacketBuffer0[1] = HIBYTE(p_nPrefix);
            }
            else
            {
                m_pPacketBuffer[0] = LOBYTE(p_nPrefix);
                m_pPacketBuffer[1] = HIBYTE(p_nPrefix);
            }
        }

        public void SET_PACKET_COMMAND(ushort p_nCmd, bool p_bCancel)
        {
            if (p_bCancel)
            {
                m_pPacketBuffer0[2] = LOBYTE(p_nCmd);
                m_pPacketBuffer0[3] = HIBYTE(p_nCmd);
            }
            else
            {
                m_pPacketBuffer[2] = LOBYTE(p_nCmd);
                m_pPacketBuffer[3] = HIBYTE(p_nCmd);
            }
        }


        public byte LOBYTE(ushort p_nValue)
        {
            byte w_nRet;

            // Get Low Byte
            w_nRet = (byte)(p_nValue & 0xff);

            // Return
            return w_nRet;
        }

        public byte HIBYTE(ushort p_nValue)
        {
            byte w_nRet;

            // Get High Byte
            w_nRet = (byte)(p_nValue >> 8);

            // Return
            return w_nRet;
        }

        public ushort AddCheckSum(bool p_bCmdData)
        {
            ushort w_nRet = 0;
            ushort w_nLen;
            int w_nI;

            // Get Length
            if (p_bCmdData)
                w_nLen = CommonDefine.ST_COM_PACKET_LEN;
            else
                w_nLen = GET_DATAPACKET_LEN(false);

            // Calculate CheckSum
            for (w_nI = 0; w_nI < w_nLen; w_nI++)
                w_nRet += (ushort)m_pPacketBuffer[w_nI];

            // Set CheckSum
            m_pPacketBuffer[w_nLen] = LOBYTE(w_nRet);
            m_pPacketBuffer[w_nLen + 1] = HIBYTE(w_nRet);

            // Return
            return w_nRet;
        }

        public ushort GET_DATAPACKET_LEN(bool p_bCancel)
        {
            return (ushort)(GET_PACKET_LEN(p_bCancel) + 6);
        }

        public ushort GET_PACKET_LEN(bool p_bCancel)
        {
            ushort w_nRet;

            if (p_bCancel)
                w_nRet = MAKEWORD(m_pPacketBuffer0[4], m_pPacketBuffer0[5]);
            else
                w_nRet = MAKEWORD(m_pPacketBuffer[4], m_pPacketBuffer[5]);

            return w_nRet;
        }

       


        public ushort MAKEWORD(byte p_nLoByte, byte p_nHiByte)
        {
            ushort w_nRet;

            w_nRet = (ushort)(((p_nHiByte << 8) & 0xFF00) + (p_nLoByte & 0x00FF));

            return w_nRet;
        }
        public class CommonDefine
        {
            // Packet Prefix
            public const int CMD_PREFIX_CODE = (0xAA55);
            public const int RCM_PREFIX_CODE = (0x55AA);
            public const int CMD_DATA_PREFIX_CODE = (0xA55A);
            public const int RCM_DATA_PREFIX_CODE = (0x5AA5);

            // Command
            public const int CMD_VERIFY_CODE = (0x0101);
            public const int CMD_IDENTIFY_CODE = (0x0102);
            public const int CMD_ENROLL_CODE = (0x0103);
            public const int CMD_ENROLL_ONETIME_CODE = (0x0104);
            public const int CMD_CLEAR_TEMPLATE_CODE = (0x0105);
            public const int CMD_CLEAR_ALLTEMPLATE_CODE = (0x0106);
            public const int CMD_GET_EMPTY_ID_CODE = (0x0107);
            public const int CMD_GET_BROKEN_TEMPLATE_CODE = (0x0109);
            public const int CMD_READ_TEMPLATE_CODE = (0x010A);
            public const int CMD_WRITE_TEMPLATE_CODE = (0x010B);
            public const int CMD_GET_FW_VERSION_CODE = (0x0112);
            public const int CMD_FINGER_DETECT_CODE = (0x0113);
            public const int CMD_FEATURE_OF_CAPTURED_FP_CODE = (0x011A);
            public const int CMD_IDENTIFY_TEMPLATE_WITH_FP_CODE = (0x011C);
            public const int CMD_SLED_CTRL_CODE = (0x0124);
            public const int CMD_IDENTIFY_FREE_CODE = (0x0125);
            public const int CMD_SET_DEVPASS_CODE = (0x0126);
            public const int CMD_VERIFY_DEVPASS_CODE = (0x0127);
            public const int CMD_GET_ENROLL_COUNT_CODE = (0x0128);
            public const int CMD_CHANGE_TEMPLATE_CODE = (0x0129);
            public const int CMD_UP_IMAGE_CODE = (0x012C);
            public const int CMD_VERIFY_WITH_DOWN_TMPL_CODE = (0x012D);
            public const int CMD_IDENTIFY_WITH_DOWN_TMPL_CODE = (0x012E);
            public const int CMD_FP_CANCEL_CODE = (0x0130);
            public const int CMD_ADJUST_SENSOR_CODE = (0x0137);
            public const int CMD_IDENTIFY_WITH_IMAGE_CODE = (0x0138);
            public const int CMD_VERIFY_WITH_IMAGE_CODE = (0x0139);
            public const int CMD_SET_PARAMETER_CODE = (0x013A);
            public const int CMD_EXIT_DEVPASS_CODE = (0x013B);
            // public const int     CMD_SET_COMMNAD_VALID_FLAG_CODE			= (0x013C);
            // public const int     CMD_GET_COMMNAD_VALID_FLAG_CODE			= (0x013D);
            public const int CMD_TEST_CONNECTION_CODE = (0x0150);
            public const int RCM_INCORRECT_COMMAND_CODE = (0x0160);
            public const int CMD_ENTER_ISPMODE_CODE = (0x0171);

            // Error Code
            public const int ERR_SUCCESS = (0);
            public const int ERR_FAIL = (1);
            public const int ERR_CONTINUE = (2);
            public const int ERR_VERIFY = (0x11);
            public const int ERR_IDENTIFY = (0x12);
            public const int ERR_TMPL_EMPTY = (0x13);
            public const int ERR_TMPL_NOT_EMPTY = (0x14);
            public const int ERR_ALL_TMPL_EMPTY = (0x15);
            public const int ERR_EMPTY_ID_NOEXIST = (0x16);
            public const int ERR_BROKEN_ID_NOEXIST = (0x17);
            public const int ERR_INVALID_TMPL_DATA = (0x18);
            public const int ERR_DUPLICATION_ID = (0x19);
            public const int ERR_TOO_FAST = (0x20);
            public const int ERR_BAD_QUALITY = (0x21);
            public const int ERR_SMALL_LINES = (0x22);
            public const int ERR_TIME_OUT = (0x23);
            public const int ERR_NOT_AUTHORIZED = (0x24);
            public const int ERR_GENERALIZE = (0x30);
            public const int ERR_COM_TIMEOUT = (0x40);
            public const int ERR_FP_CANCEL = (0x41);
            public const int ERR_INTERNAL = (0x50);
            public const int ERR_MEMORY = (0x51);
            public const int ERR_EXCEPTION = (0x52);
            public const int ERR_INVALID_TMPL_NO = (0x60);
            public const int ERR_INVALID_PARAM = (0x70);
            public const int ERR_NO_RELEASE = (0x71);
            public const int ERR_INVALID_OPERATION_MODE = (0x72);
            public const int ERR_NOT_SET_PWD = (0x74);
            public const int ERR_FP_NOT_DETECTED = (0x75);
            public const int ERR_ADJUST_SENSOR = (0x76);

            // Return Value
            public const int GD_NEED_FIRST_SWEEP = (0xFFF1);
            public const int GD_NEED_SECOND_SWEEP = (0xFFF2);
            public const int GD_NEED_THIRD_SWEEP = (0xFFF3);
            public const int GD_NEED_RELEASE_FINGER = (0xFFF4);
            public const int GD_TEMPLATE_NOT_EMPTY = (0x01);
            public const int GD_TEMPLATE_EMPTY = (0x00);
            public const int GD_DETECT_FINGER = (0x01);
            public const int GD_NO_DETECT_FINGER = (0x00);
            public const int GD_DOWNLOAD_SUCCESS = (0xA1);

            // Packet
            public const int MAX_DATA_LEN = (600); /*512*/
            public const int ST_COM_PACKET_LEN = (22);
            public const int ST_COMMAND_LEN = (66);
            public const int ST_CMDDATA_INDEX = (6);
            public const int ST_RCMDATA_INDEX = (8);

            // Communication
            public const int COMM_TIMEOUT = (15000);

            // Mutex
            public const int MUTEX_TIMEOUT = (1500);

            // Time
            public const int GD_MAX_FP_TIME_OUT = (60);
            public const int GD_DEFAUT_FP_TIME_OUT = (5);
            public const int GD_MIN_FP_TIME_OUT = (0);

            // Template
            public const int GD_MAX_RECORD_COUNT = (3000);
            public const int GD_TEMPLATE_SIZE = (570);
            public const int GD_RECORD_SIZE = (GD_TEMPLATE_SIZE);

            // Messages
            public const int WM_RECEIVE = (30000 + 0x05);
            public const int WM_DISPLAY_PACKET = (30000 + 0x06);
            public const int WM_USER_UPDATE_0 = (30000 + 0x07);

            // Strings
            public const string MSG_TITLE = "SZ_OEMHost";

            // Image
            public const int GD_IMAGE_RECEIVE_UINT = (498);
            public const int IMAGE_SEND_UNIT = (498);
        }

        // Struct
        [StructLayout(LayoutKind.Sequential)]
        public struct ST_COM_PACKET         // 22Byte
        {
            public ushort m_nPrefix;        // 2Byte
            public ushort m_nCmd_Rcm;       // 2Byte
            public ushort m_nDataLen;       // 2Byte
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] m_pCMDData;       // 16Byte
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct ST_COM_PACKET_RET     // 22Byte
        {
            public ushort m_nPrefix;        // 2Byte
            public ushort m_nCmd_Rcm;       // 2Byte
            public ushort m_nDataLen;       // 2Byte
            public ushort m_nRet;           // 2Byte
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public byte[] m_pRcmData;       // 14Byte
        }

    }
        #endregion

}
    
