using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Aliyun.OSS;
using System.IO;
namespace PeriodAid.DAL
{
    public class AliOSSUtilities
    {
        private const string AccessKey = "04nGoi0uof51Tbcn";
        private const string AccessSecret = "1z4XQF95OBX5nnMfcDukoItI85tB6s";
        private const string endpoint = "http://offlinesales.oss-cn-hangzhou.aliyuncs.com";
        private OssClient client;
        public AliOSSUtilities()
        {
            client = new OssClient(endpoint, AccessKey, AccessSecret);
        }
        public void PutObject(byte[] content, string filename)
        {
            MemoryStream ms = new MemoryStream(content);
            client.PutObject("offlinesales", filename, ms);
        }
    }
}