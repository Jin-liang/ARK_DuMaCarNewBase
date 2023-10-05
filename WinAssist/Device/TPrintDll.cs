using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WinAssist.Device
{
    public class TPrintDll
    {
        int POS_ES_INVALIDPAPA = -1;//参数错误
        int POS_ES_OVERTIME = -5;//超时

        [DllImport("POS_SDK.dll", EntryPoint = "POS_Port_OpenA")]
        public static extern int POS_Port_OpenA(string SzName, int iPort, bool bFile, string
               SzFilePath); //打开打印机端口


        [DllImport("POS_SDK.dll", EntryPoint = "POS_Port_Close")]

        public static extern int POS_Port_Close(int iPrinterID);  //关闭打印机端口

        [DllImport("POS_SDK.dll", EntryPoint = "POS_Output_PrintFontStringA")]
        public static extern int POS_Output_PrintFontStringA(int iPrinterID, int iFont, int iThich, int iWidth, int iHeight,
                                 int iUnderLine, string IpString); //打印字符串

        [DllImport("POS_SDK.dll", EntryPoint = "POS_Output_PrintStringA")]
        public static extern int POS_Output_PrintStringA(int iPrinterID, string IpString); //打印字符串

        [DllImport("POS_SDK.dll", EntryPoint = "POS_Control_FeedLines")]
        public static extern int POS_Control_FeedLines(int iPrinterID, int iLines); //走纸换行

        [DllImport("POS_SDK.dll", EntryPoint = "POS_Control_AlignType")]

        public static extern int POS_Control_AlignType(int iPrinterID, int iAlignType); //控制整体打印位置

        [DllImport("POS_SDK.dll", EntryPoint = "POS_Output_PrintOneDimensionalBarcodeA")] //打印一维条码

        public static extern int POS_Output_PrintOneDimensionalBarcodeA(int iPrinterID, int iType, int iWidth,
                                 int iHeight, int hri, string BarcodeValue);

        [DllImport("POS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "POS_Control_CutPaper")]
        public static extern Int32 POS_Control_CutPaper(Int32 printID, Int32 type, Int32 len);
    }
}

