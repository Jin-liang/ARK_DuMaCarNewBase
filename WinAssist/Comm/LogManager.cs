using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinAssist.Comm
{
    public class LogManager
    {
        private static bool isDebug = true;
        private static string logPath = string.Empty;

        private static string _dirPath = Application.StartupPath.TrimEnd('\\') + "\\Log\\";   


        /// <summary>
        /// 保存日志的文件夹
        /// </summary>
        public static string LogPath
        {
            get
            {
                if (logPath == string.Empty)
                {
                    logPath = AppDomain.CurrentDomain.BaseDirectory;
                }
                return logPath;
            }
            set { logPath = value; }
        }

        private static string logFilePrefix = string.Empty;
        /// <summary>
        /// 日志文件前缀
        /// </summary>
        public static string LogFilePrefix
        {
            get { return logFilePrefix; }
            set { logFilePrefix = value; }
        }

        public static void WriteErrorLog(string className, string funName, object objParam, Exception ex)
        {
            try
            {
                string strParam = string.Empty;
                if (objParam is string[])
                {
                    foreach (string param in objParam as string[])
                    {
                        strParam += param + "\r\n";
                    }
                }
                else
                {
                    strParam = objParam.ToString();
                }
                string strText = "\r\n类名称---函数名称\t" + className + "---" + funName;
                strText += "\r\n参数值：" + strParam;
                strText += "\r\n异常类型：" + ex.ToString();
                strText += "\r\n异常来源：" + ex.Source;
                strText += "\r\n异常信息：" + ex.Message;
                strText += "\r\n***************-------------------------*********************";
               
                WriteLogSave("Err", strText);
            }
            catch (Exception ex1)
            {
                Debug.Print(ex1.Message);
            }
        }

        private static void CheckDirectory()
        {
            if (!Directory.Exists(_dirPath))
            {
                Directory.CreateDirectory(_dirPath);
            }
        }
        public static void WriteLog(string strText, bool bSuccess)
        {
            try
            {
                string filename = bSuccess ? "Success_Log_" : "Error_Log_";
                filename = filename + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                strText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + strText + "\r\n";
                CheckDirectory();
                if (bSuccess) filename = "Info\\" + filename;
                else filename = "Error\\" + filename;
                using (FileStream fs = new FileStream(_dirPath + filename, FileMode.Append, FileAccess.Write))
                {
                    byte[] buff = Encoding.Default.GetBytes(strText);
                    fs.Write(buff, 0, buff.Length);
                    fs.Close();

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }

        }

        /// <summary>
        /// 写日志
        /// </summary>
        public static bool WriteLog(string logFile, string msg)
        {
            try
            {
                if (isDebug)
                {
                    System.IO.StreamWriter sw = System.IO.File.AppendText(
                        LogPath + LogFilePrefix + logFile + " " +
                        DateTime.Now.ToString("yyyyMMdd") + ".Log"
                        );
                    sw.WriteLine(/*DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:") + */msg);
                    sw.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        /// <summary>
        /// 写日志
        /// </summary>
        public static bool WriteLogSave(string logFile, string msg)
        {
            try
            {
                if (isDebug)
                {
                    string LogPaths = LogPath + LogFilePrefix + @"\Log\" + DateTime.Now.ToString("yyyyMMdd") + @"\";
                    if (!Directory.Exists(LogPaths))
                    {
                        Directory.CreateDirectory(LogPaths);
                    }
                    System.IO.StreamWriter sw = System.IO.File.AppendText(
                        LogPaths + logFile + "_" +
                        DateTime.Now.ToString("yyyyMMdd_HH") + ".Log"
                        );
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + msg);
                    sw.Close();
                    return true;
                }
            }
            catch
            { return false; }
            return false;
        }
        /// <summary>
        /// 写日志
        /// </summary>
        public static bool WriteLogSaveComys(string comID, string logFile, string msg)
        {
            try
            {
                //if (!Directory.Exists(@txtFileSaveDir.Text))//若文件夹不存在则新建文件夹   
                //{
                //    Directory.CreateDirectory(@txtFileSaveDir.Text); //新建文件夹   
                //}  
                System.IO.StreamWriter sw = System.IO.File.AppendText(
                    LogPath + LogFilePrefix + @"\Log\" + comID + "__" + logFile + "_" +
                    DateTime.Now.ToString("yyyyMMdd") + ".Log"
                    );
                sw.WriteLine(/*DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + */msg);
                sw.Close();
                return true;

            }
            catch
            { return false; }


        }
        /// <summary>
        /// 写日志
        /// </summary>
        public static bool WriteLogSaveComy(string comID, string logFile, string msg)
        {
            try
            {
                if (!Directory.Exists(LogPath + LogFilePrefix + @"\Log\" + comID))//若文件夹不存在则新建文件夹   
                {
                    Directory.CreateDirectory(LogPath + LogFilePrefix + @"\Log\" + comID); //新建文件夹   
                }
                System.IO.StreamWriter sw = System.IO.File.AppendText(
                    LogPath + LogFilePrefix + @"\Log\" + comID + @"\" + logFile + "_" + comID + "_" +
                    DateTime.Now.ToString("yyyyMMdd") + ".Log"
                    );
                sw.WriteLine(/*DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + */msg);
                sw.Close();
                return true;

            }
            catch
            { return false; }
        }

        /// <summary>
        /// 写日志 默认错误日志
        /// </summary>
        /// <param name="Msg"></param>
        public static void WriteLog(string Msg)
        {
            WriteLog(LogFile.Error, Msg);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="ex">异常信息</param>
        public static void WriteLog(Exception ex)
        {
            WriteLog(LogFile.Error, ex.ToString());
        }

        /// <summary>
        /// 写日志
        /// </summary>
        public static bool WriteLog(LogFile logFile, string msg)
        {
            return WriteLog(logFile.ToString(), msg);
        }

        public static void WriteLogDeubg(string msg, string path = "")
        {
            try
            {
                if (isDebug)
                {
                    if (path == "")
                        path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Debug.log";

                    System.IO.StreamWriter sw = System.IO.File.AppendText(path);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + msg + Environment.NewLine);
                    sw.Close();
                }
            }
            catch
            { }
        }
        public void Read(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                Console.WriteLine(line.ToString());
            }
        }
        public void Write(string path, string Text)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            //获得字节数组
            byte[] data = System.Text.Encoding.Default.GetBytes(Text);
            //开始写入
            fs.Write(data, 0, data.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }

      
        /// <summary>   
        /// 取得一个文本文件的编码方式。   
        /// </summary>   
        /// <param name="fileName">文件名。</param>   
        /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>   
        /// <returns></returns>   
        public static Encoding GetEncoding(string fileName, Encoding defaultEncoding)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            Encoding targetEncoding = GetEncoding(fs, defaultEncoding);
            fs.Close();
            return targetEncoding;
        }

        /// <summary>   
        /// 取得一个文本文件流的编码方式。   
        /// </summary>   
        /// <param name="stream">文本文件流。</param>   
        /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>   
        /// <returns></returns>   
        public static Encoding GetEncoding(FileStream stream, Encoding defaultEncoding)
        {
            Encoding targetEncoding = defaultEncoding;
            if (stream != null && stream.Length >= 2)
            {
                //保存文件流的前4个字节   
                byte byte1 = 0;
                byte byte2 = 0;
                byte byte3 = 0;
                byte byte4 = 0;
                //保存当前Seek位置   
                long origPos = stream.Seek(0, SeekOrigin.Begin);
                stream.Seek(0, SeekOrigin.Begin);

                int nByte = stream.ReadByte();
                byte1 = Convert.ToByte(nByte);
                byte2 = Convert.ToByte(stream.ReadByte());
                if (stream.Length >= 3)
                {
                    byte3 = Convert.ToByte(stream.ReadByte());
                }
                if (stream.Length >= 4)
                {
                    byte4 = Convert.ToByte(stream.ReadByte());
                }
                //根据文件流的前4个字节判断Encoding   
                //Unicode {0xFF, 0xFE};   
                //BE-Unicode {0xFE, 0xFF};   
                //UTF8 = {0xEF, 0xBB, 0xBF};   
                if (byte1 == 0xFE && byte2 == 0xFF)//UnicodeBe   
                {
                    targetEncoding = Encoding.BigEndianUnicode;
                }
                if (byte1 == 0xFF && byte2 == 0xFE && byte3 != 0xFF)//Unicode   
                {
                    targetEncoding = Encoding.Unicode;
                }
                if (byte1 == 0xEF && byte2 == 0xBB && byte3 == 0xBF)//UTF8   
                {
                    targetEncoding = Encoding.UTF8;
                }
                //恢复Seek位置         
                stream.Seek(origPos, SeekOrigin.Begin);
            }
            return targetEncoding;
        }
    }
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogFile
    {
        Trace,
        Warning,
        Error,
        SQL,
        Info,
        摄像机ID
    }
}




