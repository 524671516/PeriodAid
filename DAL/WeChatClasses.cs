using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace PeriodAid.DAL
{
    #region 微信获取ACCESS_TOKEN的类
    /// <summary>
    /// 微信获取ACCESS_TOKEN的类
    /// </summary>
    public class Wx_AccessToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }
    #endregion


    #region 微信获取JS接口的类
    /// <summary>
    /// 微信获取JS接口的类
    /// </summary>
    public class Wx_JsApiTicket
    {
        public int errcode { get; set; }
        public string errmsg { get; set; }
        public string ticket { get; set; }
        public int expires_in { get; set; }
    }
    #endregion


    #region 微信获取网页授权的ACCESS_TOKEN的类
    /// <summary>
    /// 微信获取网页授权的ACCESS_TOKEN的类
    /// </summary>
    public class Wx_WebOauthAccessToken
    {
        public string access_token { get; set; }
        public Int64 expires_in { get; set; }
        public string refresh_token { get; set; }
        public string openid { get; set; }
        public string scope { get; set; }
        public string unionid { get; set; }
    }
    #endregion


    #region 微信获取用户基本资料的类
    /// <summary>
    /// 微信获取用户基本资料的类
    /// </summary>
    public class Wx_WebUserInfo
    {
        public string openid { get; set; }
        public string nickname { get; set; }
        public string sex { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string contry { get; set; }
        public string headimgurl { get; set; }
        public List<string> privilege { get; set; }
        public string unionid { get; set; }
    }
    #endregion


    #region 微信自动回复基类
    /// <summary>
    /// 微信自动回复基类
    /// </summary>
    public abstract class WeChat_Message
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public long CreateTime { get; set; }
        public string MsgType { get; set; }
        public WeChat_Message(string toUserName, string fromUserName, string msgType)
        {
            this.ToUserName = toUserName;
            this.FromUserName = fromUserName;
            this.MsgType = msgType;
            this.CreateTime = CommonUtilities.generateTimeStamp();
        }
        /// <summary>
        /// 返回回复的XML
        /// </summary>
        /// <returns>自动回复的XML</returns>
        public abstract string writeXml();
    }
    #endregion


    #region 微信回复文本消息
    /// <summary>
    /// 微信回复文本消息
    /// </summary>
    public class WeChat_Message_Text : WeChat_Message
    {
        private string Content { get; set; }
        public WeChat_Message_Text(string toUserName, string fromUserName, string content)
            : base(toUserName, fromUserName, "text")
        {
            this.Content = content;
        }
        public override string writeXml()
        {
            /*
            <xml>
            <ToUserName><![CDATA[toUser]]></ToUserName>
            <FromUserName><![CDATA[fromUser]]></FromUserName> 
            <CreateTime>1348831860</CreateTime>
            <MsgType><![CDATA[text]]></MsgType>
            <Content><![CDATA[this is a test]]></Content>
            <MsgId>1234567890123456</MsgId>
            </xml>
             * */
            StringBuilder sb = new StringBuilder();
            sb.Append("<xml>");
            sb.Append("<ToUserName><![CDATA[" + this.ToUserName + "]]></ToUserName>");
            sb.Append("<FromUserName><![CDATA[" + this.FromUserName + "]]></FromUserName>");
            sb.Append("<CreateTime>" + this.CreateTime + "</CreateTime>");
            sb.Append("<MsgType><![CDATA[" + this.MsgType + "]]></MsgType>");
            sb.Append("<Content><![CDATA[" + this.Content + "]]></Content>");
            sb.Append("</xml>");
            return sb.ToString();
            throw new NotImplementedException();
        }
    }
    #endregion


    public class WeChat_Message_News : WeChat_Message
    {
        private List<WeChat_Article> Articles;
        public WeChat_Message_News(string toUserName, string fromUserName, List<WeChat_Article> Articles):
            base(toUserName, fromUserName, "news")
        {
            this.Articles = Articles;
        }
        public override string writeXml()
        {
            /*
             *  <xml>
                <ToUserName><![CDATA[toUser]]></ToUserName>
                <FromUserName><![CDATA[fromUser]]></FromUserName>
                <CreateTime>12345678</CreateTime>
                <MsgType><![CDATA[news]]></MsgType>
                <ArticleCount>2</ArticleCount>
                <Articles>
                <item>
                <Title><![CDATA[title1]]></Title> 
                <Description><![CDATA[description1]]></Description>
                <PicUrl><![CDATA[picurl]]></PicUrl>
                <Url><![CDATA[url]]></Url>
                </item>
                <item>
                <Title><![CDATA[title]]></Title>
                <Description><![CDATA[description]]></Description>
                <PicUrl><![CDATA[picurl]]></PicUrl>
                <Url><![CDATA[url]]></Url>
                </item>
                </Articles>
                </xml> 
             */
            StringBuilder sb = new StringBuilder();
            sb.Append("<xml>");
            sb.Append("<ToUserName><![CDATA[" + this.ToUserName + "]]></ToUserName>");
            sb.Append("<FromUserName><![CDATA[" + this.FromUserName + "]]></FromUserName>");
            sb.Append("<CreateTime>" + this.CreateTime + "</CreateTime>");
            sb.Append("<MsgType><![CDATA[" + this.MsgType + "]]></MsgType>");
            sb.Append("<ArticleCount>" + Articles.Count + "</ArticleCount>");
            sb.Append("<Articles>");
            foreach (var item in Articles)
            {
                sb.Append("<item>");
                sb.Append("<Title><![CDATA[" + item.Title + "]]></Title>");
                sb.Append("<Description><![CDATA[" + item.Description + "]]></Description>");
                sb.Append("<PicUrl><![CDATA[" + item.PicUrl + "]]></PicUrl>");
                sb.Append("<Url><![CDATA[" + item.Url + "]]></Url>");
                sb.Append("</item>");
            }
            sb.Append("</Articles>");
            sb.Append("</xml>");
            return sb.ToString();
            throw new NotImplementedException();
        }
    }
    public class WeChat_Article
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PicUrl { get; set; }
        public string Url { get; set; }
        public WeChat_Article()
        {

        }
        public WeChat_Article(string Title, string Description, string PicUrl, string Url)
        {
            this.Title = Title;
            this.Url = Url;
            this.PicUrl = PicUrl;
            this.Description = Description;
        }
    }
    public class Wx_OrderResult
    {
        public string Result { get; set; }
        public string PrepayId { get; set; }
        public string Message { get; set; }
        public Wx_OrderResult()
        {

        }
        public Wx_OrderResult(string Result, string PrepayId, string Message)
        {
            this.Result = Result;
            this.PrepayId = PrepayId;
            this.Message = Message;
        }
    }
}