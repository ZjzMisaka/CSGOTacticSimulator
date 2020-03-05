using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 版权声明：本文为CSDN博主「chuangand」的原创文章，遵循 CC 4.0 BY-SA 版权协议，转载请附上原文出处链接及本声明。
/// 原文链接：https://blog.csdn.net/chuangand/article/details/48056015
/// </summary>

namespace CSGOTacticSimulator.Helper
{
    class IniHelper
    {
        private static string IniFilePath = Path.Combine(Global.GlobalDictionary.basePath, "infos/setting.ini");

        /// <summary>
        /// 写操作
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);


        /// <summary>
        /// 读操作
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="def">未读取到的默认值</param>
        /// <param name="retVal">读取到的值</param>
        /// <param name="size">大小</param>
        /// <param name="filePath">路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 读ini文件
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="defValue">未读取到值时的默认值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string ReadIni(string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", temp, 255, IniFilePath);
            return temp.ToString();
        }

        /// <summary>
        /// 写入ini文件
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static void WriteIni(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, IniFilePath);
        }
        /// <summary>
        /// 删除节
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static long DeleteSection(string section)
        {
            return WritePrivateProfileString(section, null, null, IniFilePath);
        }

        /// <summary>
        /// 删除键
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static long DeleteKey(string section, string key)
        {
            return WritePrivateProfileString(section, key, null, IniFilePath);
        }
    }
}
