using DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinAssist.Comm
{
    public class StrCom
    {

        public static void ARKShowDlg(string msg)
        {
            FrmShowDlg _FrmShowDlg = new FrmShowDlg(msg);
            _FrmShowDlg.ShowDialog();
            _FrmShowDlg.label2.Text = msg;
            _FrmShowDlg.Dispose();
            _FrmShowDlg = null;
        }

        ///发送指令到客户端
        public static void SendClientObj(MessageContext _MessageContext)
        {
            string sendMsg = DataDal.JsonHelper.SerializeObject(_MessageContext);
            LogManager.WriteLogSave("sendMsg",  ">>" + sendMsg.ToString());

            RemoteObject.SendClientMessage(sendMsg);
        }

        ///更新配置
        public static void Updateinfo(MessageContext _MessageContext)
        {

            RemoteObject.Version = "";

            //RemoteObject.
        }

        ///发送指令到客户端
        public static void SendClientObj(string msg)
        {

            try
            {
                LogManager.WriteLogSave("sendMsg", ">>" + msg.ToString());
                ClientRemote obj = (ClientRemote)Activator.GetObject(typeof(ClientRemote), "ipc://ArkClientChannel/ClientRemote");
                if (obj == null)
                {
                    return;
                }
                RemoteObject.SendClientMessage(msg);
                //obj.SendMessage(sendMsg);
            }
            catch (Exception el)
            {
                LogManager.WriteLogSave("SendIPCErr", el.Message.ToString());
            }
           
           
        }
        /// <summary>
        /// 通过设备编号转换成数组
        /// </summary>
        /// <param name="equip"></param>
        /// <returns></returns>
        public static byte[] mCRC16(ushort equip)
        {
            byte[] meq = new byte[2];
            meq[0] = (byte)(equip >> 8);
            meq[1] = (byte)(equip & 0xFF);
            return meq;
        }

        /// <summary>
        /// 16进制数转换成浮点数据
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        static unsafe public float toFloat(uint i)
        {
            uint* p = &i;
            return *((float*)p);
        }





        /// <summary>
        /// 16进制数转换成浮点数据
        /// </summary>
        /// <param name="Hex"></param>
        /// <returns></returns>
        public static float HexStrToFloat(string Hex)
        {
            uint num = uint.Parse(Hex, System.Globalization.NumberStyles.AllowHexSpecifier);
            byte[] floatValues = BitConverter.GetBytes(num);
            return BitConverter.ToSingle(floatValues, 0);
        }

        /// <summary>
        /// 浮点数据转换成16进制数
        /// </summary>
        /// <param name="fdata"></param>
        /// <returns></returns>
        public static string FloatToHexStr(float fdata)
        {
            byte[] bytes = BitConverter.GetBytes(fdata);//把浮点型转换为字节类型
            Array.Reverse(bytes);//反转一维数组中某部分元素的顺序
            string result = BitConverter.ToString(bytes).Replace("-", "");
            return result;
        }

        /// <summary>
        /// 字符串转16进制
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StrToHex(string str)
        {
            string strTemp = "";
            if (str == "")
                return "";
            byte[] bTemp = System.Text.Encoding.Default.GetBytes(str);

            for (int i = 0; i < bTemp.Length; i++)
            {
                strTemp += bTemp[i].ToString("X");
            }
            return strTemp;
        }


        /// <summary>
        /// byte[]转换成16进制string 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes, int n)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < n; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }
    }

    /// <summary>
    /// CRC校验信息
    /// </summary>
    unsafe public class ComCRC
    {
        /* CRC 高位字节值表 */
        static byte[] auchCRCHi = { 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40 
        };
        /* CRC低位字节值表*/
        static byte[] auchCRCLo = { 
        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 
        0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 
        0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 
        0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 
        0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4, 
        0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3, 
        0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 
        0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 
        0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 
        0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 
        0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED, 
        0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26, 
        0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 
        0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 
        0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 
        0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 
        0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 
        0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5, 
        0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 
        0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 
        0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 
        0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 
        0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B, 
        0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C, 
        0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 
        0x43, 0x83, 0x41, 0x81, 0x80, 0x40 
        };


        /// <summary>
        /// 山东版本CRC
        /// </summary>
        /// <param name="crc_pointer"></param>
        /// <param name="data_length"></param>
        /// <returns></returns>
        public static ushort crc(string crc_pointer, ushort data_length)
        {
            ushort k, k0, bit_flag;
            ushort int_crc = 0xffff;
            for (k = 0; k < data_length; k++)
            {
                int_crc ^= (crc_pointer[k]);
                for (k0 = 0; k0 < 8; k0++)
                {
                    bit_flag = (ushort)(int_crc & 0x0001);
                    int_crc >>= 1;
                    if (bit_flag == 1)
                        int_crc ^= 0xa001;
                }
            }
            return (int_crc);
        }





        /// <summary>
        /// 计算信息值的校验值
        /// </summary>
        /// <param name="puchMsg">数据</param>
        /// <param name="usDataLen">内计算的长度</param>
        /// <returns></returns>
        public static ushort CRC16(byte[] puchMsg, int usDataLen)
        {
            byte uchCRCHi = 0xFF;
            byte uchCRCLo = 0xFF;
            byte uIndex;
            int point = 0;
            while (usDataLen > 0)
            {
                usDataLen--;
                uIndex = (byte)(uchCRCHi ^ puchMsg[point]);
                uchCRCHi = (byte)(uchCRCLo ^ auchCRCHi[uIndex]);
                uchCRCLo = auchCRCLo[uIndex];
                point++;
            }
            return (ushort)(uchCRCHi + uchCRCLo * 256);
        }

        //public static byte crc7(byte[] ptr, int len)
        //{
        //    byte crc;
        //    int i;
        //    crc = 0;
        //    for (int j = 0; j < len; j++)
        //    {
        //        crc ^= ptr[j];
        //        for (i = 0; i < 8; i++)
        //        {
        //            if ((crc & 0x01) == 1)
        //            {
        //                crc = (byte)((crc >> 1) ^ 0x8C);
        //            }
        //            else
        //            {
        //                crc >>= 1;
        //            }
        //        }
        //    }
        //    return crc;
        //}

        private const UInt16 X25_INIT_CRC = 0xffff;
        private const UInt16 X25_VALIDATE_CRC = 0xf0b8;
        //#region
        //  private static void crc_accumulate(UInt16 data,  UInt16 *crcAccum)
        //  {
        //      /*Accumulate one byte of data into the CRC*/
        //      UInt16 tmp;

        //      tmp = (UInt16)(data ^ (ushort)(*crcAccum & 0xff));
        //      tmp ^= (UInt16)(tmp << 4);
        //      *crcAccum = (UInt16)((*crcAccum >> 8) ^ (tmp << 8) ^ (tmp << 3) ^ (tmp >> 4));
        //  }

        //  private static void crc_init(UInt16* crcAccum)
        //  {
        //      *crcAccum = X25_INIT_CRC;
        //  }

        //  public static UInt16 crc_calculate(UInt16 *pBuffer, UInt16 length)
        //  {

        //      UInt16 crcTmp=0;
        //      UInt16*  pTmp;
        //      int i;
        //      pTmp = pBuffer;
        //      /* init crcTmp */
        //      crc_init(ref crcTmp);

        //      for (i = 0; i < length; i++)
        //      {
        //          crc_accumulate(*pTmp++,ref crcTmp);
        //      }
        //      return (crcTmp);
        //  }
        //#endregion
        public static void crc_accumulate(UInt16 data, UInt16* crcAccum)
        {
            /*Accumulate one byte of data into the CRC*/
            UInt16 tmp;

            tmp = (ushort)(data ^ (UInt16)(*crcAccum & 0xff));
            tmp ^= (ushort)(tmp << 4);
            *crcAccum = (ushort)((*crcAccum >> 8) ^ (tmp << 8) ^ (tmp << 3) ^ (tmp >> 4));
        }

        /**
         * @brief Initiliaze the buffer for the X.25 CRC
         *
         * @param crcAccum the 16 bit X.25 CRC
         */
        public static void crc_init(UInt16* crcAccum)
        {
            *crcAccum = X25_INIT_CRC;
        }


        /**
         * @brief Calculates the X.25 checksum on a byte buffer
         *
         * @param  pBuffer buffer containing the byte array to hash
         * @param  length  length of the byte array
         * @return the checksum over the buffer bytes
         **/
        public static UInt16 crc_calculate(UInt16* pBuffer, int length)//此方法还有问题
        {
            UInt16 crcTmp;
            UInt16* pTmp;
            pTmp = pBuffer;
            crc_init(&crcTmp);

            for (int i = 0; i < length; i++)
            {
                crc_accumulate(*pTmp++, &crcTmp);
            }
            return (crcTmp);
        }


        public static byte crc7(byte[] u8Data, Int32 u32Len)
        {
            byte crc = 0; //（初始值为0）
            byte j;
            int k = 0;
            while (u32Len > 0)
            {
                crc = (byte)(crc ^ u8Data[k]);
                for (j = 0; j < 8; j++)
                {
                    if ((crc & 0x80) > 0)
                        crc = (byte)((crc << 1) ^ 0xE5);//多项式值为E5,被校验值左移
                    else
                        crc <<= 1;
                }
                k++;
                //u8Data++;
                u32Len--;
            }

            return crc;
        }

        public static byte crc7(byte[] u8Data, int start, Int32 u32Len)
        {
            byte crc = 0; //（初始值为0）
            byte j;
            int k = 0;
            while (u32Len > 0)
            {
                crc = (byte)(crc ^ u8Data[k + start]);
                for (j = 0; j < 8; j++)
                {
                    if ((crc & 0x80) > 0)
                        crc = (byte)((crc << 1) ^ 0xE5);//多项式值为E5,被校验值左移
                    else
                        crc <<= 1;
                }
                k++;
                //u8Data++;
                u32Len--;
            }

            return crc;
        }


        public static ushort crc8(byte[] u8Data, int start, Int32 u32Len)
        {
            ushort crc = 0; //（初始值为0）

            while (u32Len > start)
            {
                crc += u8Data[u32Len - 1];
                u32Len--;
            }

            return crc;
        }
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }


      
    }
}