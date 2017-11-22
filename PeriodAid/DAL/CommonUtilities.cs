using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System.Drawing.Imaging;
using System.IO;

namespace PeriodAid.DAL
{
    public class CommonUtilities
    {
        private static char[] Chars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'R', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                                          'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                                          '1','2','3','4','5','6','7','8','9','0' };
        private static char[] Digits = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

        #region 获取当前时间戳
        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns>long</returns>
        public static long generateTimeStamp()
        {
            TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
        #endregion


        #region 生成随机字符串(24个字符)
        /// <summary>
        /// 生成随机字符串(24个字符)
        /// </summary>
        /// <returns>随机字符串</returns>
        public static string generateNonce()
        {

            return generateNonce(24);
        }
        #endregion


        #region 生成随机字符串
        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="num">制定位数</param>
        /// <returns>随机字符串</returns>
        public static string generateNonce(int num)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random((int)generateTimeStamp());
            string code = "";
            random = new Random();
            for (int j = 0; j < num; j++)
            {
                code = code + Chars[random.Next(0, Chars.Length)];
            }
            return code;
        }
        #endregion


        public static string generateDigits(int num)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random((int)generateTimeStamp());
            string code = "";
            random = new Random();
            for (int j = 0; j < num; j++)
            {
                code = code + Digits[random.Next(0, Digits.Length)];
            }
            return code;
        }

        #region SHA1加密算法
        /// <summary>
        /// SHA1加密算法
        /// </summary>
        /// <param name="source_data">原文</param>
        /// <returns>密文</returns>
        public static string encrypt_SHA1(string source_data)
        {
            byte[] StrRes = Encoding.Default.GetBytes(source_data);
            HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            StrRes = iSHA.ComputeHash(StrRes);
            StringBuilder EnText = new StringBuilder();
            foreach (byte iByte in StrRes)
            {
                EnText.AppendFormat("{0:x2}", iByte);
            }
            return EnText.ToString();
        }
        #endregion

        public static void writeLog(string s)
        {
            StreamWriter log = new StreamWriter("d:\\ceshi.txt", true);
            log.WriteLine("time:" + s);
            log.Close();
        }

        #region MD5加密算法
        /// <summary>
        /// MD5加密算法
        /// </summary>
        /// <param name="source_data">原文</param>
        /// <returns>密文</returns>
        public static string encrypt_MD5(String source_data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source_data);
            byte[] md5data = md5.ComputeHash(data);
            md5.Clear();
            string str = "";
            for (int i = 0; i < md5data.Length; i++)
            {
                str += md5data[i].ToString("x").PadLeft(2, '0');
            }
            return str;
        }
        #endregion

        public static MemoryStream generate_QR_Code(string url)
        {
            ErrorCorrectionLevel Ecl = ErrorCorrectionLevel.M; //误差校正水平 
            //string Content = strContent;//待编码内容
            QuietZoneModules QuietZones = QuietZoneModules.Two;  //空白区域 
            int ModuleSize = 12;//大小
            var encoder = new QrEncoder(Ecl);
            QrCode qr;
            MemoryStream ms = new MemoryStream();
            if (encoder.TryEncode(url, out qr))//对内容进行编码，并保存生成的矩阵
            {
                var render = new GraphicsRenderer(new FixedModuleSize(ModuleSize, QuietZones));
                render.WriteToStream(qr.Matrix, ImageFormat.Png, ms);
            }
            ms.Close();
            return ms;
        }

        public static string getFirst(string photos)
        {
            if (photos != "")
            {
                var list = photos.Split(',');
                return list[0];
            }
            return "";
        }
    }

    #region 网页参数格式化帮助类
    /// <summary>
    /// 网页参数格式化帮助类
    /// </summary>
    public class QueryParameter
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public string Value { get; set; }
        public QueryParameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
        /// <summary>
        /// 格式化参数列表
        /// </summary>
        /// <param name="parameters">参数列表</param>
        /// <returns>格式化后的参数列表</returns>
        public static string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            var list = parameters.OrderBy(m => m.Name).ToList();
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var item in list)
            {
                sb.AppendFormat("{0}={1}", item.Name, item.Value);
                if (i < list.Count - 1)
                {
                    sb.Append("&");
                }
                i++;
            }
            return sb.ToString();
        }
        public static string NormalizeKDTParameters(IList<QueryParameter> parameters)
        {
            var list = parameters.OrderBy(m => m.Name).ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.AppendFormat("{0}{1}", item.Name, item.Value);
            }
            return sb.ToString();
        }
    }
    public class QueryParameterComparer : IComparer<QueryParameter>
    {
        public int Compare(QueryParameter x, QueryParameter y)
        {
            if (x.Name == y.Name)
            {
                return string.Compare(x.Value, y.Value);
            }
            else
            {
                return string.Compare(x.Name, y.Name);
            }
        }
    }
    #endregion



    /// <summary>
    /// FileEncoding 的摘要说明

    //public class EncodingType
    //{
    //    /// <summary>
    //    /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型
    //    /// </summary>
    //    /// <param name="FILE_NAME">文件路径</param>
    //    /// <returns>文件的编码类型</returns>
    //    public static Encoding GetType(string FILE_NAME)
    //    {
    //        FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
    //        Encoding r = GetType(fs);
    //        fs.Close();
    //        return r;
    //    }

    //    /// <summary>
    //    /// 通过给定的文件流，判断文件的编码类型
    //    /// </summary>
    //    /// <param name="fs">文件流</param>
    //    /// <returns>文件的编码类型</returns>
    //    //public static Encoding GetType(BinaryReader r)
    //    //{
    //    //    byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
    //    //    byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
    //    //    byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
    //    //    Encoding reVal = Encoding.Default;

            
    //    //    int i;
    //    //    int.TryParse(fs.Length.ToString(), out i);
    //    //    byte[] ss = r.ReadBytes(i);
    //    //    if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
    //    //    {
    //    //        reVal = Encoding.UTF8;
    //    //    }
    //    //    else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
    //    //    {
    //    //        reVal = Encoding.BigEndianUnicode;
    //    //    }
    //    //    else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
    //    //    {
    //    //        reVal = Encoding.Unicode;
    //    //    }
    //    //    r.Close();
    //    //    return reVal;

    //    //}

    //    ///// <summary>
    //    ///// 判断是否是不带 BOM 的 UTF8 格式
    //    ///// </summary>
    //    ///// <param name="data"></param>
    //    ///// <returns></returns>
    //    //private static bool IsUTF8Bytes(byte[] data)
    //    //{
    //    //    int charByteCounter = 1; //计算当前正分析的字符应还有的字节数
    //    //    byte curByte; //当前分析的字节.
    //    //    for (int i = 0; i < data.Length; i++)
    //    //    {
    //    //        curByte = data[i];
    //    //        if (charByteCounter == 1)
    //    //        {
    //    //            if (curByte >= 0x80)
    //    //            {
    //    //                //判断当前
    //    //                while (((curByte <<= 1) & 0x80) != 0)
    //    //                {
    //    //                    charByteCounter++;
    //    //                }
    //    //                //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
    //    //                if (charByteCounter == 1 || charByteCounter > 6)
    //    //                {
    //    //                    return false;
    //    //                }
    //    //            }
    //    //        }
    //    //        else
    //    //        {
    //    //            //若是UTF-8 此时第一位必须为1
    //    //            if ((curByte & 0xC0) != 0x80)
    //    //            {
    //    //                return false;
    //    //            }
    //    //            charByteCounter--;
    //    //        }
    //    //    }
    //    //    if (charByteCounter > 1)
    //    //    {
    //    //        throw new Exception("非预期的byte格式");
    //    //    }
    //    //    return true;
    //    //}
    //}
}