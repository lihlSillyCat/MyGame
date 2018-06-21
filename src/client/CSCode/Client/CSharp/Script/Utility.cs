using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;
using System.Net;
using System.Text;
using System.Security.Cryptography;

using System.Runtime.InteropServices;
using XLua;

namespace War.Script
{
    [LuaCallCSharp]
    public class Utility
    {

        public static uint BKDRHash(string strToHash)
        {
            uint seed = 131;
            uint hash = 0;

            int strLength = strToHash.Length;
            for (int i = 0; i < strLength; ++i)
            {
                hash = (hash * seed + strToHash[i]) % 32749;
            }

            return hash;
        }

        //取得手机运行读写的路径
        public static string GetPersistentDataPath()
        {
            return Application.persistentDataPath;
        }

         //退出游戏
         public static void QuitGame()
        {
            Application.Quit();
        }

        //是否在编辑器中
        public static bool IsInEditor()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return true;
#else
              return false;
#endif
        }

        public static string GetMacAddress()
        {
            // NetworkInterface.GetAllNetworkInterfaces有bug，可能会抛异常或者阻塞，现在服务器没有使用mac
            return "nomacaddress";
        }

        //判断文件是否存在
        public static bool FileIsExist(string path)
        {
            return System.IO.File.Exists(path);
        }

        //读取文件中的Text
        public static string ReadTextFile(string path)
        {
            string res = "";
            if (FileIsExist(path))
                res = System.IO.File.ReadAllText(path);
            return res;

        }

        //写入Text到文件中
        public static bool WriteTextFile(string path,string szData)
        {
            bool res = false;
            if(null != path && ""!=path)
            {
                System.IO.File.WriteAllText(path, szData);
                res = true;
            }
            return res;
        }


        //获取当前运行设备CPU个数
        public static int GetCPUCount()
        {
            return SystemInfo.processorCount;
        }

        //获取当前运行设备CPU类型
        public static string GetCPUType()
        {
#if UNITY_ANDROID
                string cpuName = "";
                AndroidJavaObject androidObject = new AndroidJavaObject("com.q1.androidtools.AndroidUtils");
                AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                if (activity == null || androidObject == null)
                    return cpuName;
                cpuName = androidObject.Call<String>("getCupName");
                return cpuName;
#else
            return SystemInfo.processorType;
#endif
        }

        //获取当前运行设备内存大小
        public static int GetSystemMemory()
        {
            return SystemInfo.systemMemorySize;
        }

        //获取当前运行设备机型
        public static string GetDeviceModel()
        {
            return SystemInfo.deviceModel;
        }

        ////获取当前运行设备的操作系统
        public static string GetOperatingSystem()
        {
            return SystemInfo.operatingSystem;
        }

#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void IOS_CopyTextToClipboard(string text);
#endif

        // 复制到剪贴板
        public static void CopyTextToClipboard(string text)
        {
#if UNITY_ANDROID

            AndroidJavaObject androidObject = new AndroidJavaObject("com.q1.androidtools.AndroidUtils");
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null || androidObject == null)
                return ;
            androidObject.Call("copyTextToClipboard", activity, text);
#endif

#if UNITY_IPHONE
		IOS_CopyTextToClipboard(text);
#endif

        }


        // 从剪贴板中获取文本
        public static string GetTextFromClipboard()
        {
#if UNITY_ANDROID
            String text = "";
            AndroidJavaObject androidObject = new AndroidJavaObject("com.q1.androidtools.AndroidUtils");
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            if (activity != null && androidObject != null)
                text=androidObject.Call<String>("getTextFromClipboard");
            return text;
#else
            return "";
#endif
        }
        static DateTime baseTime = new DateTime(1970, 1, 1);
        public static double GetNowMiliseconds()
        {
            return (double)((DateTime.UtcNow.Ticks - baseTime.Ticks) / 10000);
        }
        public static double GetBatteryLevel()
        {
            return SystemInfo.batteryLevel;
        }
        public static double GetBatteryStatus()
        {
            return (double)SystemInfo.batteryStatus;
        }

        public static Vector2 GetTouchPosition()
        {
            return Input.mousePosition;
        }

        /// <summary>  
        /// SHA1 加密，返回大写字符串  
        /// </summary>  
        /// <param name="content">需要加密字符串</param>  
        /// <returns>返回40位UTF8 大写</returns>  
        public static string SHA1(string content)
        {
            return SHA1(content, Encoding.UTF8);
        }

        /// <summary>  
        /// SHA1 加密，返回大写字符串  
        /// </summary>  
        /// <param name="content">需要加密字符串</param>  
        /// <param name="encode">指定加密编码</param>  
        /// <returns>返回40位大写字符串</returns>  
        public static string SHA1(string content, Encoding encode)
        {
            try
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] bytes_in = encode.GetBytes(content);
                byte[] bytes_out = sha1.ComputeHash(bytes_in);
                sha1.Clear();
                (sha1 as IDisposable).Dispose();
                string result = BitConverter.ToString(bytes_out);
                result = result.Replace("-", "");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("SHA1加密出错：" + ex.Message);
            }
        }

        // 随机数算法
        static UInt64 nNext = 1;
        public static void SetSeed(UInt64 seed)
        {
            nNext = seed;
        }

        public static Double GenerateNum()
        {
            return (((nNext = nNext * 214013L + 2531011L) >> 16) & 0x7fff);
        }

        /// <summary>
        /// 整型转位数组
        /// </summary>
        /// <returns></returns>
        public static LuaTable IntToBiteArray(Int64 num)
        {
            LuaTable tab = LuaManager.Instance.luaEnv.NewTable();
            Int64 t = 1;
            for(int i = 0; i < 32; ++i)
            {
                t = 1 << i;
                if((num & t) > 0)
                {
                    tab.Set<int, int>(i, 1);
                }
                else
                {
                    tab.Set<int, int>(i, 0);
                }
            }

            return tab;
        }

        /// <summary>
        /// 同步两个对象的位置和角度，在lua中直接写会有性能损耗，所以尽可能的使用这种方式来实现
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void SyncPositionAndRotate(Transform src, Transform dst)
        {
            dst.position = src.position;
            dst.rotation = src.rotation;
        }

        /// <summary>
        /// 获取主机的IP地址
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <returns></returns>
        public static string GetHostAddresses(string serverAddress)
        {
            try
            {
                IPAddress[] address = Dns.GetHostAddresses(serverAddress);
                if (address.Length > 0)
                {
                    return address[0].ToString();
                }
            }
            catch(Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            return "";
        }

        /// <summary>
        /// 执行UI的事件
        /// </summary>
        /// <param name="go">要执行UI事件的对象</param>
        /// <param name="nType">事件的类型</param>
        public static void ExecuteUIEvents(GameObject go, int nType=-1)
        {
            if(null == go)
            {
                return;
            }
        }

        /// <summary>
        /// hex转换到color
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color HexToColor(string hex)
        {
            byte br = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte bg = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte bb = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte cc = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            float r = br / 255f;
            float g = bg / 255f;
            float b = bb / 255f;
            float a = cc / 255f;
            return new Color(r, g, b, a);
        }

    }
}