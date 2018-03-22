using CsvHelper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using PagedList;
using PeriodAid.DAL;
using PeriodAid.Filters;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;

namespace PeriodAid.Controllers
{
    [Authorize]
    public class SalesProcessController : Controller
    {
        // GET: SalesProcess
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SalesProcessModel _db;
        private ProjectSchemeModels e_db;
        private IKCRMDATAModel crm_db;
        private ThreeLevelAddressModel address_db;
        private ProjectSchemeModels projrct_db;
        int try_times = 0;
        public SalesProcessController()
        {
            _db = new SalesProcessModel();
            crm_db = new IKCRMDATAModel();
            address_db = new ThreeLevelAddressModel();
            projrct_db = new ProjectSchemeModels();
            e_db = new ProjectSchemeModels();
        }
        public ActionResult Index()
        {
            return View();
        }

        private byte[] SetFileToByteArray(HttpPostedFileBase File)
        {
            Stream stream = File.InputStream;
            byte[] ArrayByte = new byte[File.ContentLength];
            stream.Read(ArrayByte, 0, File.ContentLength);
            stream.Close();
            return ArrayByte;
        }

        public SP_Seller getSeller(string username)
        {
            var seller = _db.SP_Seller.SingleOrDefault(m => m.User_Name == username);
            return seller;
        }
        
        //CRM
        public static String buildQueryStr(Dictionary<String, String> dicList)
        {
            String postStr = "";

            foreach (var item in dicList)
            {
                postStr += item.Key + "=" + HttpUtility.UrlEncode(item.Value, Encoding.UTF8) + "&";
            }
            postStr = postStr.Substring(0, postStr.LastIndexOf('&'));
            return postStr;
        }
        // 获取当前TOKEN
        private string getUserToken()
        {
            // 取数据库
            var token_time = crm_db.CRM_User_Token.SingleOrDefault(m => m.Key == "CRM_UserToken");
            try
            {
                TimeSpan ts = DateTime.Now - token_time.download_at;
                int days = ts.Days;
                if (days >= 1)
                {
                    return RefreshUserToken();
                }
            }
            catch (Exception)
            {
                return RefreshUserToken();
            }
            return token_time.Value;
        }

        private string RefreshUserToken()
        {
            // 远程获取
            var token_time = crm_db.CRM_User_Token.SingleOrDefault(m => m.Key == "CRM_UserToken");
            string url = "https://api.ikcrm.com/api/v2/auth/login";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ServicePoint.ConnectionLimit = int.MaxValue;
            request.Method = "post";
            request.ContentType = "application/x-www-form-urlencoded";
            ///添加参数  
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("login", UserInfo.login);
            dicList.Add("password", UserInfo.password);
            dicList.Add("device", UserInfo.device);
            String postStr = buildQueryStr(dicList);
            byte[] data = Encoding.UTF8.GetBytes(postStr);
            request.ContentLength = data.Length;
            Stream myRequestStream = request.GetRequestStream();
            myRequestStream.Write(data, 0, data.Length);
            myRequestStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            var retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            result_Data resultdata = JsonConvert.DeserializeObject<result_Data>(retString);
            if (resultdata.code == "0")
            {
                if (token_time == null)
                {
                    token_time = new CRM_User_Token();
                    token_time.Key = "CRM_UserToken";
                    token_time.Value = resultdata.data.user_token;
                    token_time.download_at = DateTime.Now;
                    crm_db.CRM_User_Token.Add(token_time);
                }
                else
                {
                    token_time.Key = "CRM_UserToken";
                    token_time.Value = resultdata.data.user_token;
                    token_time.download_at = DateTime.Now;
                    crm_db.Entry(token_time).State = System.Data.Entity.EntityState.Modified;
                }
                crm_db.SaveChanges();
            }
            else
            {
                Thread.Sleep(1000);
                return RefreshUserToken();
            }
            return token_time.Value;
        }

        private string Get_Request(string url)
        {
            var retString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ServicePoint.ConnectionLimit = int.MaxValue;
            request.Method = "get";
            request.ContentType = "application/x-www-form-urlencoded";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                return retString;
            }
            catch (Exception)
            {
                return Get_Request(url);
            }
        }

        private int Get_Count(string url_api)
        {
            string url = "https://api.ikcrm.com" + url_api + "?per_page=" + UserInfo.Count + "&user_token=" + getUserToken() + "&device=dingtalk&version_code=9.8.0";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "get";
            request.ContentType = "application/x-www-form-urlencoded";
            string result = "";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                CRM_Customer_ReturnData r = JsonConvert.DeserializeObject<CRM_Customer_ReturnData>(result);
                if (r.code == "0")
                {
                    return r.data.total_count;
                }
                else if (r.code == "100401")
                {
                    RefreshUserToken();
                    return Get_Count(url_api);
                }
                return 100;
            }
            catch (Exception)
            {
                return Get_Count(url_api);
            }
        }
        [HttpPost]
        public JsonResult GetCustomer(string url_api)
        {
            var count = Get_Count(url_api);
            var page = count / UserInfo.Count + 1;
            List<int> customerlist = new List<int>();
            List<int> CRM_Customerlist = new List<int>();
            List<int> contactlist = new List<int>();
            List<int> CRM_Contactlist = new List<int>();
            var CRM_customer = from m in crm_db.CRM_Customer
                               where m.status == 0
                               select m;
            foreach (var customer in CRM_customer)
            {
                CRM_Customerlist.Add(customer.customer_id);
            }
            var CRM_contact = from m in crm_db.CRM_Contact
                              where m.status == 0
                              select m;
            foreach (var contact in CRM_contact)
            {
                CRM_Contactlist.Add(contact.contact_id);
            }
            for (int x = 1; x <= page; x++)
            {
                string url = "https://api.ikcrm.com/api/v2/customers/?per_page=" + UserInfo.Count + "&page=" + x + "&user_token=" + getUserToken() + "&device=dingtalk&version_code=9.8.0";
                CRM_Customer_ReturnData r = JsonConvert.DeserializeObject<CRM_Customer_ReturnData>(Get_Request(url));
                if (r.code == "0")
                {
                    for (int i = 0; i < r.data.customers.Count(); i++)
                    {
                        var Cid = r.data.customers[i].id;
                        string customersAddress = r.data.customers[i].address.region_info;
                        int indexAddress = 0;
                        int indexprovince = 0;
                        string customersAddressStr = "";
                        string customersProvinceStr = "";
                        string province = "";
                        string city = "";
                        string district = "";
                        if (customersAddress == null)
                        {
                        }
                        else if (customersAddress.Count() > 3)
                        {
                            customersAddressStr = customersAddress.Remove(0, 3);
                            indexAddress = customersAddressStr.IndexOf(" ");
                            province = customersAddressStr.Substring(0, indexAddress);
                            customersProvinceStr = customersAddressStr.Remove(0, indexAddress + 1);
                            indexprovince = customersProvinceStr.IndexOf(" ");
                            city = customersProvinceStr.Substring(0, indexprovince);
                            district = customersAddressStr.Replace(" ", "-");
                        }
                        else
                        {
                            customersAddressStr = customersAddress;
                        }
                        customerlist.Add(r.data.customers[i].id);
                        var check_customer = crm_db.CRM_Customer.SingleOrDefault(m => m.customer_id == Cid);
                        if (check_customer == null)
                        {
                            // new
                            check_customer = new CRM_Customer();
                            check_customer.customer_id = Cid;
                            check_customer.customer_name = r.data.customers[i].name;
                            check_customer.customer_address = customersAddressStr + " " + r.data.customers[i].address.detail_address;
                            check_customer.customer_tel = r.data.customers[i].address.tel;
                            check_customer.zip = "200001";
                            check_customer.province = province;
                            check_customer.city = city;
                            check_customer.district = district;
                            check_customer.status = 0;
                            check_customer.customer_abbreviation = r.data.customers[i].address.wechat;
                            crm_db.CRM_Customer.Add(check_customer);
                            crm_db.SaveChanges();
                            for (int j = 0; j < r.data.customers[i].contacts.Count(); j++)
                            {
                                var ctId = r.data.customers[i].contacts[j].address.addressable_id;
                                var check_contact = crm_db.CRM_Contact.SingleOrDefault(m => m.contact_id == ctId);
                                string ctAddress = r.data.customers[i].contacts[j].address.region_info;
                                int indexCtAddress = 0;
                                int indexCtProvince = 0;
                                string CtAddressStr = "";
                                string CtProvinceStr = "";
                                string Ctprovince = "";
                                string Ctcity = "";
                                string Ctdistrict = "";
                                if (ctAddress == null)
                                {
                                }
                                else if (ctAddress.Count() > 3)
                                {
                                    CtAddressStr = ctAddress.Remove(0, 3);
                                    indexCtAddress = CtAddressStr.IndexOf(" ");
                                    Ctprovince = CtAddressStr.Substring(0, indexCtAddress);
                                    CtProvinceStr = CtAddressStr.Remove(0, indexCtAddress + 1);
                                    indexCtProvince = CtProvinceStr.IndexOf(" ");
                                    Ctcity = CtProvinceStr.Substring(0, indexprovince);
                                    Ctdistrict = CtAddressStr.Replace(" ", "-");
                                }
                                else
                                {
                                    CtAddressStr = ctAddress;
                                }
                                if (check_contact == null)
                                {
                                    // new
                                    check_contact = new CRM_Contact();
                                    check_contact.contact_id = ctId;
                                    check_contact.contact_name = r.data.customers[i].contacts[j].name;
                                    check_contact.contact_address = CtAddressStr + " " + r.data.customers[i].contacts[j].address.detail_address;
                                    check_contact.contact_tel = r.data.customers[i].contacts[j].address.phone;
                                    check_contact.customer_id = check_customer.Id;
                                    check_contact.province = Ctprovince;
                                    check_contact.city = Ctcity;
                                    check_contact.district = Ctdistrict;
                                    check_contact.zip = "200001";
                                    check_contact.status = 0;
                                    check_customer.customer_abbreviation = r.data.customers[i].address.wechat;
                                    crm_db.CRM_Contact.Add(check_contact);
                                }
                            }
                        }
                        else
                        {
                            // update
                            check_customer.customer_id = Cid;
                            check_customer.customer_name = r.data.customers[i].name;
                            check_customer.customer_address = customersAddressStr + " " + r.data.customers[i].address.detail_address;
                            check_customer.customer_tel = r.data.customers[i].address.tel;
                            check_customer.zip = "200001";
                            check_customer.province = province;
                            check_customer.city = city;
                            check_customer.district = district;
                            check_customer.status = 0;
                            check_customer.customer_abbreviation = r.data.customers[i].address.wechat;
                            crm_db.Entry(check_customer).State = System.Data.Entity.EntityState.Modified;
                            for (int j = 0; j < r.data.customers[i].contacts.Count(); j++)
                            {
                                var ctId = r.data.customers[i].contacts[j].address.addressable_id;
                                var check_contact = crm_db.CRM_Contact.SingleOrDefault(m => m.contact_id == ctId);
                                string ctAddress = r.data.customers[i].contacts[j].address.region_info;
                                int indexCtAddress = 0;
                                int indexCtProvince = 0;
                                string CtAddressStr = "";
                                string CtProvinceStr = "";
                                string Ctprovince = "";
                                string Ctcity = "";
                                string Ctdistrict = "";
                                if (ctAddress == null)
                                {
                                }
                                else if (ctAddress.Count() > 3)
                                {
                                    CtAddressStr = ctAddress.Remove(0, 3);
                                    indexCtAddress = CtAddressStr.IndexOf(" ");
                                    Ctprovince = CtAddressStr.Substring(0, indexCtAddress);
                                    CtProvinceStr = CtAddressStr.Remove(0, indexCtAddress + 1);
                                    indexCtProvince = CtProvinceStr.IndexOf(" ");
                                    Ctcity = CtProvinceStr.Substring(0, indexprovince);
                                    Ctdistrict = CtAddressStr.Replace(" ", "-");
                                }
                                else
                                {
                                    CtAddressStr = ctAddress;
                                }
                                contactlist.Add(ctId);
                                if (check_contact == null)
                                {
                                    // new
                                    check_contact = new CRM_Contact();
                                    check_contact.contact_id = ctId;
                                    check_contact.contact_name = r.data.customers[i].contacts[j].name;
                                    check_contact.contact_address = CtAddressStr + " " + r.data.customers[i].contacts[j].address.detail_address;
                                    check_contact.contact_tel = r.data.customers[i].contacts[j].address.phone;
                                    check_contact.customer_id = check_customer.Id;
                                    check_contact.province = Ctprovince;
                                    check_contact.city = Ctcity;
                                    check_contact.district = Ctdistrict;
                                    check_contact.zip = "200001";
                                    check_contact.status = 0;
                                    crm_db.CRM_Contact.Add(check_contact);
                                }
                                else
                                {
                                    //update
                                    check_contact.contact_id = ctId;
                                    check_contact.contact_name = r.data.customers[i].contacts[j].name;
                                    check_contact.contact_address = CtAddressStr + " " + r.data.customers[i].contacts[j].address.detail_address;
                                    check_contact.contact_tel = r.data.customers[i].contacts[j].address.phone;
                                    check_contact.customer_id = check_customer.Id;
                                    check_contact.province = Ctprovince;
                                    check_contact.city = Ctcity;
                                    check_contact.district = Ctdistrict;
                                    check_contact.zip = "200001";
                                    check_contact.status = 0;
                                    crm_db.Entry(check_contact).State = System.Data.Entity.EntityState.Modified;
                                }
                            }
                        }
                    }
                    var diffArrcustomer = CRM_Customerlist.Where(m => !customerlist.Contains(m)).ToArray();
                    for (int i = 0; i < diffArrcustomer.Count(); i++)
                    {
                        var customerId = diffArrcustomer[i];
                        var check_data = crm_db.CRM_Customer.SingleOrDefault(m => m.customer_id == customerId);
                        check_data.status = -1;
                        crm_db.Entry(check_data).State = System.Data.Entity.EntityState.Modified;
                    }
                    var diffArrcontact = CRM_Contactlist.Where(m => !contactlist.Contains(m)).ToArray();
                    for (int i = 0; i < diffArrcontact.Count(); i++)
                    {
                        var contactId = diffArrcontact[i];
                        var check_data = crm_db.CRM_Contact.SingleOrDefault(m => m.contact_id == contactId);
                        check_data.status = -1;
                        crm_db.Entry(check_data).State = System.Data.Entity.EntityState.Modified;
                    }
                    
                }
                else if (r.code == "100401")
                {
                    RefreshUserToken();
                    return GetCustomer(url_api);
                }
                else
                {
                    CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                    Thread.Sleep(1000);
                    try_times++;
                    if (try_times >= 10)
                    {
                        logs.type = "customer";
                        logs.exception = "[customer]获取失败";
                        logs.exception_at = DateTime.Now;
                        crm_db.CRM_ExceptionLogs.Add(logs);
                        crm_db.SaveChanges();
                        try_times = 0;
                        return Json(new { result = "FAIL" });
                    }
                    return GetCustomer(url_api);
                }
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
        [HttpPost]
        private bool GetUserInfo()
        {
            //部门
            string get_department = "https://api.ikcrm.com/api/v2/user/department_list?per_page=" + UserInfo.Count + "&user_token=" + getUserToken() + "&device=dingtalk&version_code=9.8.0";
            CRM_ContractDetail_ReturnData department_data = JsonConvert.DeserializeObject<CRM_ContractDetail_ReturnData>(Get_Request(get_department));
            if (department_data.code == "0")
            {
                foreach (var item in department_data.data.options)
                {
                    var department = crm_db.CRM_Department.SingleOrDefault(m => m.system_code == item.Id);
                    if (department == null)
                    {
                        CRM_Department newdepartment = new CRM_Department();
                        newdepartment.system_code = item.Id;
                        newdepartment.name = item.name;
                        newdepartment.level = item.level;
                        newdepartment.parent_id = item.parent_id;
                        newdepartment.can_use = item.can_use;
                        crm_db.CRM_Department.Add(newdepartment);
                        crm_db.SaveChanges();
                    }
                    else
                    {
                        department.system_code = item.Id;
                        department.name = item.name;
                        department.level = item.level;
                        department.parent_id = item.parent_id;
                        department.can_use = item.can_use;
                        crm_db.Entry(department).State = System.Data.Entity.EntityState.Modified;
                        crm_db.SaveChanges();
                    }
                }
                crm_db.SaveChanges();
            }
            else {
                return GetUserInfo();
            }
            //角色和用户
            string get_user = "https://api.ikcrm.com/api/v2/user/list?per_page=" + UserInfo.Count + "&sort=superior_id&order=asc&user_token=" + getUserToken() + "&device=dingtalk&version_code=9.8.0";
            var res = Get_Request(get_user);
            CRM_ContractDetail_ReturnData user_data = JsonConvert.DeserializeObject<CRM_ContractDetail_ReturnData>(res);
            if (user_data.code == "0")
            {
                foreach (var item in user_data.data.users) {
                    //角色
                    var role = crm_db.CRM_Role.SingleOrDefault(m => m.system_code == item.role_json.Id);
                    if (role == null)
                    {
                        CRM_Role newrole = new CRM_Role();
                        newrole.name = item.role_json.name;
                        newrole.entity_grant_scope = item.role_json.entity_grant_scope;
                        newrole.system_code = item.role_json.Id;
                        crm_db.CRM_Role.Add(newrole);
                    }
                    else
                    {
                        role.name = item.role_json.name;
                        role.entity_grant_scope = item.role_json.entity_grant_scope;
                        role.system_code = item.role_json.Id;
                        crm_db.Entry(role).State = System.Data.Entity.EntityState.Modified;
                    }
                    crm_db.SaveChanges();
                    //用户
                    var user = crm_db.CRM_User.SingleOrDefault(m => m.email == item.email);
                    if (user == null)
                    {
                        CRM_User newuser = new CRM_User();
                        newuser.system_code = item.Id;
                        newuser.email = item.email;
                        newuser.created_at = item.created_at;
                        newuser.name = item.name;
                        newuser.phone = item.phone;
                        newuser.role_id = crm_db.CRM_Role.SingleOrDefault(m => m.system_code == item.role_id).Id;
                        var superior = crm_db.CRM_User.SingleOrDefault(m => m.system_code == item.superior_id);
                        newuser.superior_id = superior != null ? superior.Id : 0;
                        newuser.department_id = crm_db.CRM_Department.SingleOrDefault(m => m.system_code == item.department_id).Id;
                        crm_db.CRM_User.Add(newuser);
                    }
                    else
                    {
                        user.system_code = item.Id;
                        user.email = item.email;
                        user.created_at = item.created_at;
                        user.name = item.name;
                        user.phone = item.phone;
                        user.role_id = crm_db.CRM_Role.SingleOrDefault(m => m.system_code == item.role_id).Id;
                        var superior = crm_db.CRM_User.SingleOrDefault(m => m.system_code == item.superior_id);
                        user.superior_id = superior != null ? superior.Id : 0;
                        user.department_id = crm_db.CRM_Department.SingleOrDefault(m => m.system_code == item.department_id).Id;
                        crm_db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                crm_db.SaveChanges();
            }
            else {
                return GetUserInfo();
            }
            return true;

        }
        [HttpPost]
        public JsonResult GetCrmInfo(string url_api)
        {
            //刷新组织架构和使用用户
            //GetUserInfo();
            var count = Get_Count(url_api);
            var page = count / UserInfo.Count + 1;
            List<int> contractlist = new List<int>();
            List<int> CRM_Contractlist = new List<int>();
            var CRM_Contract = from m in crm_db.CRM_Contract
                               select m;
            foreach (var crm in CRM_Contract)
            {
                CRM_Contractlist.Add(crm.contract_id);
            }
            for (int x = 1; x <= page; x++)
            {
                string url = "https://api.ikcrm.com/api/v2/contracts/?per_page=" + UserInfo.Count + "&page=" + x + "&approve_status=approved&user_token=" + getUserToken() + "&device=dingtalk&version_code=9.8.0";
                CRM_Contract_ReturnData r = JsonConvert.DeserializeObject<CRM_Contract_ReturnData>(Get_Request(url));
                if (r.code == "0")
                {
                    for (int i = 0; i < r.data.contracts.Count(); i++)
                    {
                        var platform_code = "IK" + DateTime.Now.ToString("yyyyMMddHHmmss") + r.data.contracts[i].id;
                        var contractId = r.data.contracts[i].id;
                        var customerId = r.data.contracts[i].customer_id;
                        var check_customer = crm_db.CRM_Customer.SingleOrDefault(m => m.customer_id == customerId);
                        var check_data = crm_db.CRM_Contract.SingleOrDefault(m => m.contract_id == contractId);
                        var total_amount = r.data.contracts[i].total_amount;
                        if (check_data == null)
                        {
                            //new
                            check_data = new CRM_Contract();
                            check_data.contract_id = r.data.contracts[i].id;
                            check_data.user_id = r.data.contracts[i].user_id;
                            check_data.user_name = r.data.contracts[i].user_name;
                            check_data.customer_id = check_customer.Id;
                            check_data.contract_title = r.data.contracts[i].title;
                            check_data.total_amount = (double)total_amount;
                            check_data.contract_status = r.data.contracts[i].status;
                            check_data.updated_at = r.data.contracts[i].updated_at;
                            check_data.platform_code = platform_code;
                            check_data.warehouse_code = "110";
                            check_data.express_code = "STO";
                            if (check_customer.customer_abbreviation == null || check_customer.customer_abbreviation == "")
                            {
                                check_data.vip_code = check_customer.customer_name;
                            }
                            else
                            {
                                check_data.vip_code = check_customer.customer_abbreviation;
                            }
                            crm_db.CRM_Contract.Add(check_data);
                        }
                        else
                        {
                            // update
                            check_data.contract_id = r.data.contracts[i].id;
                            check_data.user_id = r.data.contracts[i].user_id;
                            check_data.user_name = r.data.contracts[i].user_name;
                            check_data.customer_id = check_customer.Id;
                            check_data.contract_title = r.data.contracts[i].title;
                            check_data.total_amount = (double)total_amount;
                            check_data.contract_status = r.data.contracts[i].status;
                            check_data.updated_at = r.data.contracts[i].updated_at;
                            check_data.warehouse_code = "110";
                            check_data.express_code = "STO";
                            if (check_customer.customer_abbreviation == null || check_customer.customer_abbreviation == "")
                            {
                                check_data.vip_code = check_customer.customer_name;
                            }
                            else
                            {
                                check_data.vip_code = check_customer.customer_abbreviation;
                            }
                            crm_db.Entry(check_data).State = System.Data.Entity.EntityState.Modified;
                        }
                        contractlist.Add(r.data.contracts[i].id);
                    }
                }
                else if (r.code == "100401")
                {
                    RefreshUserToken();
                    return GetCrmInfo(url_api);
                }
                else
                {
                    CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                    Thread.Sleep(1000);
                    try_times++;
                    if (try_times >= 10)
                    {
                        logs.type = "contracts";
                        logs.exception = "[合同]获取失败";
                        logs.exception_at = DateTime.Now;
                        crm_db.CRM_ExceptionLogs.Add(logs);
                        crm_db.SaveChanges();
                        try_times = 0;
                        return Json(new { result = "FAIL" });
                    }
                    return GetCrmInfo(url_api);
                }
            }
            var diffArr = CRM_Contractlist.Where(m => !contractlist.Contains(m)).ToArray();
            for (int i = 0; i < diffArr.Count(); i++)
            {
                var contractId = diffArr[i];
                var check_data = crm_db.CRM_Contract.SingleOrDefault(m => m.contract_id == contractId);
                check_data.contract_status = "-1";
                crm_db.Entry(check_data).State = System.Data.Entity.EntityState.Modified;
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS"});
        }
        [HttpPost]
        public JsonResult GetCrmDetailInfo()
        {
            // 只获取待提交订单详情
            var contracts = from m in crm_db.CRM_Contract
                            where m.contract_status == UserInfo.status_unsend && m.contract_status != UserInfo.delete
                            select m;
            foreach (var C_id in contracts)
            {
                var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.id == C_id.id);
                string url = "https://api.ikcrm.com/api/v2/contracts/" + C_id.contract_id + "?user_token=" + getUserToken() + "&device=dingtalk&version_code=9.8.0";
                CRM_ContractDetail_ReturnData r = JsonConvert.DeserializeObject<CRM_ContractDetail_ReturnData>(Get_Request(url));
                if (r.code == "0")
                {
                    for (int i = 0; i < r.data.product_assets_for_new_record.Count(); i++)
                    {
                        var pid = r.data.product_assets_for_new_record[i].product_id;
                        var s_price = r.data.product_assets_for_new_record[i].recommended_unit_price;
                        var quantity = r.data.product_assets_for_new_record[i].quantity;
                        var product_name = r.data.product_assets_for_new_record[i].name;
                        var product_code = r.data.product_assets_for_new_record[i].product_no;
                        var contractdetail = from m in crm_db.CRM_ContractDetail
                                             where m.contract_id == C_id.id
                                             select m;
                        if (contractdetail != null)
                        {

                            crm_db.CRM_ContractDetail.RemoveRange(contractdetail);
                        }
                        var contractDetail = new CRM_ContractDetail();
                        contractDetail.contract_id = C_id.id;
                        contractDetail.product_id = pid;
                        contractDetail.quantity = quantity;
                        contractDetail.unit_price = s_price;
                        contractDetail.product_name = product_name;
                        contractDetail.product_code = product_code;
                        crm_db.CRM_ContractDetail.Add(contractDetail);
                    }
                    contract.received_payments_status = 0;
                    if (r.data.text_asset_c33e2b == UserInfo.unreceived_payments || r.data.text_asset_c33e2b == UserInfo.nonEssential_payments)
                    {
                        contract.received_payments_status = 1;
                    }
                    else if ((double)r.data.received_payments_amount >= contract.total_amount && r.data.text_asset_c33e2b == UserInfo.received_payments)
                    {
                        contract.received_payments_status = 1;
                    }
                    else
                    {
                        contract.received_payments_status = 0;
                    }
                    var shop_code = r.data.text_asset_615f62_display;
                    if (shop_code != null || shop_code != "")
                    {
                        if (shop_code.Contains("零售/团购"))
                        {
                            contract.shop_code = "线下零售/团购";
                        }else if (shop_code.Contains("线上其他渠道"))
                        {
                            contract.shop_code = "线上其他渠道";
                        }
                        else if (shop_code.Contains("自营渠道"))
                        {
                            contract.shop_code = "自营渠道";
                        }
                        else if (shop_code.Contains("展会/促销"))
                        {
                            contract.shop_code = "线下展会/促销物料";
                        }else
                        {
                            contract.shop_code = "006";
                        }
                    }
                    contract.contract_type = r.data.category_mapped;
                    contract.receiver_name = r.data.text_asset_73f972;
                    contract.receiver_address = r.data.text_asset_eb802b;
                    contract.receiver_tel = r.data.text_asset_da4211;
                    contract.express_remark = r.data.text_asset_7fd81a;
                    contract.contract_remark = r.data.special_terms;
                    crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
                    checkAddress(r.data.text_asset_eb802b, C_id.id);
                }
                else if (r.code == "100401")
                {
                    RefreshUserToken();
                    return GetCrmDetailInfo();
                }
                else
                {
                    CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                    Thread.Sleep(1000);
                    try_times++;
                    if (try_times >= 10)
                    {
                        logs.type = "contractDetail";
                        logs.exception = "[合同详情]获取失败";
                        logs.exception_at = DateTime.Now;
                        crm_db.CRM_ExceptionLogs.Add(logs);
                        crm_db.SaveChanges();
                        try_times = 0;
                        return Json(new { result = "FAIL" });
                    }
                    return GetCrmDetailInfo();
                }
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        private int UpdateCRM(int cid, string contract_status, string express_information, string express_remark)
        {
            var contracts = crm_db.CRM_Contract.SingleOrDefault(m => m.id == cid);
            string url = "https://api.ikcrm.com/api/v2/contracts/" + contracts.contract_id + "?user_token=" + getUserToken() + "&device=dingtalk&version_code=9.8.0";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/x-www-form-urlencoded";
            var retString = "";
            request.ServicePoint.ConnectionLimit = int.MaxValue;
            // 添加参数
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            //只修改了订单状态和备注
            dicList.Add("contract[status]", contracts.contract_status);
            if (express_information != null)
            {
                dicList.Add("contract[text_area_asset_8f7067]", contracts.express_information);
                dicList.Add("contract[text_asset_7fd81a]", contracts.express_remark);
            }
            String postStr = buildQueryStr(dicList);
            byte[] data = Encoding.UTF8.GetBytes(postStr);
            request.ContentLength = data.Length;
            Stream myRequestStream = request.GetRequestStream();
            myRequestStream.Write(data, 0, data.Length);
            myRequestStream.Close();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
            }
            catch (Exception)
            {
                return UpdateCRM(cid, contract_status, express_information, express_remark);
            }
            CRM_Contract_ReturnData r = JsonConvert.DeserializeObject<CRM_Contract_ReturnData>(retString);
            if (r.code == "0")
            {
                return 1; // 1 正确
            }
            else if (r.code == "100401")
            {
                RefreshUserToken();
                return UpdateCRM(cid, contract_status, express_information, express_remark);
            }
            return 0;
        }
        [Authorize(Roles = "CRM")]
        public ActionResult CRM_show()
        {
            List<string> shoplist = new List<string>();
            shoplist.Add("线上其他渠道");
            shoplist.Add("线上自营渠道");
            shoplist.Add("线下零售/团购");
            shoplist.Add("线下展会/促销物料");
            ViewBag.shopList = shoplist;
            return View();
        }
        [Authorize(Roles = "CRM")]
        public ActionResult CRM_undeliveredPartical(string status,int? page,string shopCode)
        {
            var user = getEmployee(User.Identity.Name);
            int _page = page ?? 1;
            if (user.Type == 1) {
                if (shopCode == "0")
                {
                    var undeliveredData = (from m in crm_db.CRM_Contract
                                           where m.contract_status == status
                                           orderby m.edit_time descending
                                           select m).ToPagedList(_page, 20);
                    return PartialView(undeliveredData);
                }
                else
                {
                    var undeliveredData = (from m in crm_db.CRM_Contract
                                           where m.contract_status == status && m.shop_code == shopCode
                                           orderby m.edit_time descending
                                           select m).ToPagedList(_page, 20);
                    return PartialView(undeliveredData);
                }
            } else {
                var crm_user = crm_db.CRM_User.SingleOrDefault(m => m.email == user.UserName);
                var employee = from m in crm_db.CRM_User
                               where m.department_id == crm_user.department_id
                               select m;
                List<CRM_Contract> datalist = new List<CRM_Contract>();
                if (shopCode == "0")
                {
                    foreach (var emp in employee)
                    {
                        var undeliveredData = (from m in crm_db.CRM_Contract
                                               where m.contract_status == status && m.user_id == emp.system_code
                                               orderby m.edit_time descending
                                               select m).ToPagedList(_page, 20);
                        datalist.AddRange(undeliveredData);
                    }
                    return PartialView(datalist.ToPagedList(_page, 20));
                }
                else
                {
                    foreach (var emp in employee)
                    {
                        var undelivereddata = (from m in crm_db.CRM_Contract
                                               where m.contract_status == status && m.shop_code == shopCode && m.user_id == emp.system_code
                                               orderby m.edit_time descending
                                               select m).ToPagedList(_page, 20);
                        datalist.AddRange(undelivereddata.ToPagedList(_page, 20));
                    }
                    return PartialView(datalist);
                }
            }
            

        }
        [Authorize(Roles = "CRM")]
        public ActionResult ContractDetail_show(int c_id)
        {
            var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.id == c_id);
            var contractDetail = from m in crm_db.CRM_ContractDetail
                                 where m.contract_id == c_id
                                 select m;
            ViewBag.Detail = contractDetail;
            return PartialView(contract);
        }
        [Authorize(Roles = "CRM")]
        [HttpPost]
        public JsonResult Admin_pass(int c_id)
        {
            var seller = getSeller(User.Identity.Name);
            var employee = projrct_db.Employee.SingleOrDefault(m => m.UserName == seller.User_Name);
            var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.id == c_id);
            if (employee.Type == 1)
            {
                contract.received_payments_status = UserInfo.received_payments_status;
                contract.employee_id = employee.Id;
                contract.employee_name = employee.NickName;
                crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
        public Employee getEmployee(string username)
        {
            var user = e_db.Employee.SingleOrDefault(m => m.UserName == username);
            return user;
        }
        private static string AppId = "130412";
        private static string AppSecret = "26d2e926f42a4f2181dd7d1b7f7d55c0";
        private static string SessionKey = "8a503b3d9d0d4119be2868cc69a8ef5a";
        private static string API_Url = "http://v2.api.guanyierp.com/rest/erp_open";

        private string sign(string json, string secret)
        {
            StringBuilder enValue = new StringBuilder();
            //前后加上secret
            enValue.Append(secret);
            enValue.Append(json);
            enValue.Append(secret);
            //使用MD5加密(32位大写)
            return CommonUtilities.encrypt_MD5(enValue.ToString()).ToUpper();
        }

        public string getDeliverys(string mail_no)
        {
            string json = "{" +
                       "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.trade.deliverys.get\"," +
                        "\"mail_no\":\"" + mail_no + "\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"" +
                        "}";
            string signature = sign(json, AppSecret);
            string info = "{" +
                   "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.trade.deliverys.get\"," +
                    "\"mail_no\":\"" + mail_no + "\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"sign\":\"" + signature + "\"" +
                "}";
            var request = WebRequest.Create(API_Url) as HttpWebRequest;
            request.ContentType = "text/json";
            request.Method = "post";
            string result = "";
            StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
            try
            {
                streamWriter.Write(info);
                streamWriter.Flush();
                streamWriter.Close();
                var response = request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                    StringBuilder sb = new StringBuilder(result);
                    sb.Replace("\"refund\":\"NoRefund\"", "\"refund\":0");
                    sb.Replace("\"refund\":\"RefundSuccess\"", "\"refund\":1");
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    deliverys_Result r = JsonConvert.DeserializeObject<deliverys_Result>(sb.ToString());
                    if (r.success)
                    {
                        return r.deliverys[0].seller_memo;
                    }
                    return "FAIL";
                }
            }
            catch (Exception)
            {
                try_times++;
                if (try_times >= 5)
                {
                    try_times = 0;
                    return "FAIL";
                }
                return getDeliverys(mail_no);
            }
        }
        [HttpPost]
        public JsonResult getERPORDERS(int[] c_id)
        {
            //var platform_code = "IK20180316144326431356";
            //var mail_no = "3354788962851";
            List<string> errorList = new List<string>();
            List<string> failList = new List<string>();
            List<string> partialList = new List<string>();
            List<string> successList = new List<string>();
            foreach (var cId in c_id)
            {
                var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.id == cId);
                string json = "{" +
                       "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.trade.get\"," +
                        //"\"receiver_mobile\":\"" + platform_code + "\"," +
                        "\"platform_code\":\"" + contract.platform_code + "\"," +
                        //"\"platform_code\":\"" + platform_code + "\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"" +
                        "}";
                string signature = sign(json, AppSecret);
                string info = "{" +
                       "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.trade.get\"," +
                        //"\"receiver_mobile\":\"" + platform_code + "\"," +
                        "\"platform_code\":\"" + contract.platform_code + "\"," +
                        //"\"platform_code\":\"" + platform_code + "\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"," +
                        "\"sign\":\"" + signature + "\"" +
                    "}";
                var request = WebRequest.Create(API_Url) as HttpWebRequest;
                request.ContentType = "text/json";
                request.Method = "post";
                string result = "";
                StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
                try
                {
                    streamWriter.Write(info);
                    streamWriter.Flush();
                    streamWriter.Close();
                    var response = request.GetResponse();
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                        StringBuilder sb = new StringBuilder(result);
                        sb.Replace("\"refund\":\"NoRefund\"", "\"refund\":0");
                        sb.Replace("\"refund\":\"RefundSuccess\"", "\"refund\":1");
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        orders_Result r = JsonConvert.DeserializeObject<orders_Result>(sb.ToString());
                        List<string> contractlist = new List<string>();
                        List<string> deliveryslist = new List<string>();
                        if (r.success)
                        {
                            if (r.orders[0].delivery_state == 1)
                            {
                                contract.express_status = "部分发货";
                                contract.contract_status = UserInfo.status_part;
                                contract.edit_time = DateTime.Now;
                                for (int i = 0; i < r.orders[0].deliverys.Count(); i++)
                                {
                                    if (r.orders[0].deliverys[i].mail_no == "" || r.orders[0].deliverys[i].mail_no == null)
                                    {
                                        contractlist.Add(r.orders[0].deliverys[i].express_name + " ");
                                        deliveryslist.Add("");
                                    }
                                    else
                                    {
                                        contractlist.Add(r.orders[0].deliverys[i].express_name + r.orders[0].deliverys[i].mail_no);
                                        deliveryslist.Add(getDeliverys(r.orders[0].deliverys[i].mail_no));
                                    }
                                }
                                successList.Add(contract.platform_code + " " + contract.contract_title);
                                string express_information = string.Join(";", contractlist.ToArray());
                                string express_remark = string.Join(";", deliveryslist.ToArray());
                                contract.express_information = express_information;
                                contract.express_remark = express_remark;
                                crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
                            }
                            else if (r.orders[0].delivery_state == 2)
                            {
                                contract.express_status = "全部发货";
                                contract.contract_status = UserInfo.status_delivered;
                                contract.edit_time = DateTime.Now;
                                for (int i = 0; i < r.orders[0].deliverys.Count(); i++)
                                {
                                    if (r.orders[0].deliverys[i].mail_no == "" || r.orders[0].deliverys[i].mail_no == null)
                                    {
                                        contractlist.Add(r.orders[0].deliverys[i].express_name + " ");
                                        deliveryslist.Add("");
                                    }else
                                    {
                                        contractlist.Add(r.orders[0].deliverys[i].express_name + r.orders[0].deliverys[i].mail_no);
                                        deliveryslist.Add(getDeliverys(r.orders[0].deliverys[i].mail_no));
                                    }
                                }
                                successList.Add(contract.platform_code + " " + contract.contract_title);
                                string express_information = string.Join(";", contractlist.ToArray());
                                string express_remark = string.Join(";", deliveryslist.ToArray());
                                contract.express_information = express_information;
                                contract.express_remark = express_remark;
                                crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                errorList.Add(contract.platform_code + " " + contract.contract_title);
                            }
                            var updatcrm = UpdateCRM(cId, contract.contract_status, contract.express_information, contract.express_remark);
                            if (updatcrm != 1)
                            {
                                partialList.Add(contract.platform_code +" "+ contract.contract_title);
                            }
                        }
                        else
                        {
                            failList.Add(contract.platform_code + " " + contract.contract_title+" " + r.errorDesc);
                        }
                    }
                }
                catch (Exception)
                {
                    streamWriter.Close();
                    try_times++;
                    if (try_times >= 5)
                    {
                        try_times = 0;
                        return Json(new { result = "FAIL"});
                    }
                    return getERPORDERS(c_id);
                }
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS", successlist = successList, errorlist = errorList ,faillist = failList , partiallist = partialList });
        }
        [HttpPost]
        public JsonResult createOrder(int[] c_id, string province, string city, string district)
        {
            List<string> failList = new List<string>();
            List<string> partialList = new List<string>();
            List<string> successList = new List<string>();
            var seller = getSeller(User.Identity.Name);
            var employee = projrct_db.Employee.SingleOrDefault(m => m.UserName == seller.User_Name);
            foreach (var _Cid in c_id)
            {
                var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.id == _Cid && m.contract_status == UserInfo.status_unsend && m.received_payments_status == UserInfo.received_payments_status);
                if (province != null)
                {
                    contract.receiver_province = province;
                    contract.receiver_city = city;
                    contract.receiver_district = district;
                    contract.address_status = 1;
                    crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
                    crm_db.SaveChanges();
                }
                ERPCustomOrder order = new ERPCustomOrder()
                {
                    platform_code = contract.platform_code,
                    shop_code = contract.shop_code,
                    vip_code = contract.vip_code,
                    warehouse_code = contract.warehouse_code,
                    express_code = contract.express_code,
                    receiver_name = contract.receiver_name,
                    receiver_province = contract.receiver_province,
                    receiver_city = contract.receiver_city,
                    receiver_district = contract.receiver_district,
                    receiver_mobile = contract.receiver_tel,
                    receiver_zip = contract.CRM_Customer.zip,
                    receiver_address = contract.receiver_address,
                    buyer_memo = contract.contract_remark,
                    seller_memo_late = contract.contract_title,
                    deal_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    business_man_code = contract.user_name,
                };
                order.details = new List<ERPCustomOrder_details>();
                foreach (var item in contract.CRM_ContractDetail)
                {
                    ERPCustomOrder_details details = new ERPCustomOrder_details()
                    {
                        item_code = item.product_code,
                        price = item.unit_price,
                        qty = item.quantity
                    };
                    order.details.Add(details);
                }
                order.payments = new List<ERPCustomOrder_payments>();
                foreach (var item in contract.CRM_ContractDetail)
                {
                    ERPCustomOrder_payments payments = new ERPCustomOrder_payments()
                    {
                        pay_type_code = "zhifubao",
                        payment = item.CRM_Contract.total_amount
                    };
                    order.payments.Add(payments);
                }
                ERPOrderUtilities util = new ERPOrderUtilities();
                string result = util.createOrder(order);
                Orders_Result r = JsonConvert.DeserializeObject<Orders_Result>(result);
                if (r.success)
                {
                    contract.contract_status = UserInfo.status_undelivered;
                    contract.address_status = 1;
                    contract.employee_id = employee.Id;
                    contract.employee_name = employee.NickName;
                    contract.edit_time = DateTime.Now;
                    successList.Add(contract.platform_code + " " + contract.contract_title);
                    crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
                    var updatcrm = UpdateCRM(_Cid, contract.contract_status, contract.express_information, contract.express_remark);
                    if (updatcrm != 1)
                    {
                        partialList.Add(contract.platform_code+" "+ contract.contract_title);
                    }
                }
                else
                {
                    failList.Add(contract.platform_code + " " + contract.contract_title + " "+ r.errorDesc);
                }
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS" , successlist = successList, faillist = failList, partiallist = partialList });
        }

        public void checkAddress(string full_address, int contract_id)
        {
            var address_arry = full_address.ToCharArray();
            List<int> marks = new List<int>();
            var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.id == contract_id);
            int i;
            for (i = 0; i < address_arry.Length; i++)
            {
                if (address_arry[i].ToString() == " ")
                {
                    marks.Add(i);
                }
            }
            if (marks.Count() < 3)
            {
                contract.address_status = 0;
            }
            else
            {
                var sub_province = full_address.Substring(0, marks[0]);
                var sub_city = full_address.Substring(marks[0] + 1, marks[1] - marks[0] - 1);
                var sub_area = full_address.Substring(marks[1] + 1, marks[2] - marks[1] - 1);
                contract.receiver_province = sub_province;
                contract.receiver_city = sub_city;
                contract.receiver_district = sub_area;
                contract.address_status = 1;
                crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
            }
        }
        
    }
}