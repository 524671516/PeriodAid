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
        private const string endpoint = "http://oss-cn-hangzhou.aliyuncs.com";
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
        public void PutObject(Stream stream, string filename)
        {
            client.PutObject("offlinesales", filename, stream);
        }
        public Stream GetObject(string filename)
        {
            OssObject downfile = client.GetObject("offlinesales", filename);
            return downfile.Content;
        }
        public void PutWebObject(byte[] content, string filename)
        {
            MemoryStream ms = new MemoryStream(content);
            client.PutObject("sqzcontent", filename, ms);
        }
        public void PutWebObject(Stream stream, string filename)
        {
            client.PutObject("sqzcontent", filename, stream);
        }
        public Stream GetWebObject(string filename)
        {
            OssObject downfile = client.GetObject("sqzcontent", filename);
            return downfile.Content;
        }

    }
    

}