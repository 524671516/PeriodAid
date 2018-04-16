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
        private IKCRMDATAModel crm_db;
        private MonthlyDeliveryModel md_db;
        int try_times = 0;
        public SalesProcessController()
        {
            crm_db = new IKCRMDATAModel();
            md_db = new MonthlyDeliveryModel();
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
        private async Task<string> getUserToken()
        {
            // 取数据库
            var token_time = crm_db.CRM_User_Token.SingleOrDefault(m => m.Key == "CRM_UserToken");
            try
            {
                TimeSpan ts = DateTime.Now - token_time.download_at;
                int days = ts.Days;
                if (days >= 1)
                {
                    return await RefreshUserToken();
                }
            }
            catch (Exception)
            {
                CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                try_times++;
                if (try_times >= 5)
                {
                    logs.type = "User_Token";
                    logs.exception = "[User_Token]获取失败";
                    logs.exception_at = DateTime.Now;
                    crm_db.CRM_ExceptionLogs.Add(logs);
                    crm_db.SaveChanges();
                    try_times = 0;
                    return "FAIL";
                }
                return await RefreshUserToken();
            }
            return token_time.Value;
        }

        private async Task<string> RefreshUserToken()
        {
            // 远程获取
            var token_time = crm_db.CRM_User_Token.SingleOrDefault(m => m.Key == "CRM_UserToken");
            string url = "https://api.ikcrm.com/api/v2/auth/login";
            var retString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ServicePoint.ConnectionLimit = int.MaxValue;
            request.Method = "post";
            request.ContentType = "application/x-www-form-urlencoded";
            try
            {
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
                retString = myStreamReader.ReadToEnd();
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
            }
            catch (Exception)
            {
                CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                try_times++;
                if (try_times >= 5)
                {
                    logs.type = "User_Token";
                    logs.exception = "[User_Token]获取失败";
                    logs.exception_at = DateTime.Now;
                    crm_db.CRM_ExceptionLogs.Add(logs);
                    crm_db.SaveChanges();
                    try_times = 0;
                    return "FAIL";
                }
                return await RefreshUserToken();
            }
            return token_time.Value;
        }

        private async Task<string> Get_Request(string url)
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
                return await Get_Request(url);
            }
        }

        private async Task<int> Get_Count(string url_api)
        {
            string url = "https://api.ikcrm.com" + url_api + "?per_page=" + UserInfo.Count + "&user_token=" + await getUserToken() + "&device=dingtalk&version_code=9.8.0";
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
                    await RefreshUserToken();
                    return await Get_Count(url_api);
                }
            }
            catch (Exception)
            {
                return await Get_Count(url_api);
            }
            return 100;
        }
        [HttpPost]
        public async Task<JsonResult> GetCustomer(string url_api)
        {
            var count = await Get_Count(url_api);
            var page = count / UserInfo.Count + 1;
            List<int> customerlist = new List<int>();
            List<int> CRM_Customerlist = new List<int>();
            List<int> contactlist = new List<int>();
            List<int> CRM_Contactlist = new List<int>();
            for (int x = 1; x <= page; x++)
            {
                string url = "https://api.ikcrm.com/api/v2/customers/?per_page=" + UserInfo.Count + "&page=" + x + "&user_token=" + await getUserToken() + "&device=dingtalk&version_code=9.8.0";
                var res = await Get_Request(url);
                CRM_Customer_ReturnData r = JsonConvert.DeserializeObject<CRM_Customer_ReturnData>(res);
                if (r.code == "0")
                {
                    foreach (var item in r.data.customers)
                    {
                        var costomerid = item.id;
                        string customersAddress = item.address.region_info;
                        customerlist.Add(item.id);
                        var check_customer = crm_db.CRM_Customer.SingleOrDefault(m => m.customer_id == costomerid);
                        if (check_customer == null)
                        {
                            // new
                            check_customer = new CRM_Customer();
                            check_customer.customer_id = costomerid;
                            check_customer.customer_name = item.name;
                            check_customer.customer_address = customersAddress;
                            check_customer.customer_tel = item.address.tel;
                            check_customer.status = 0;
                            check_customer.customer_abbreviation = item.address.wechat;
                            crm_db.CRM_Customer.Add(check_customer);
                            await crm_db.SaveChangesAsync();
                            for (int i = 0; i < item.contacts.Count(); i++)
                            {
                                var contactsId = item.contacts[i].address.addressable_id;
                                var check_contact = crm_db.CRM_Contact.SingleOrDefault(m => m.contact_id == contactsId);
                                string ctAddress = item.contacts[i].address.region_info;
                                if (check_contact == null)
                                {
                                    // new
                                    check_contact = new CRM_Contact();
                                    check_contact.contact_id = contactsId;
                                    check_contact.contact_name = item.contacts[i].name;
                                    check_contact.contact_address = ctAddress;
                                    check_contact.contact_tel = item.contacts[i].address.phone;
                                    check_contact.customer_id = check_customer.Id;
                                    check_contact.status = 0;
                                    check_customer.customer_abbreviation =item.address.wechat;
                                    crm_db.CRM_Contact.Add(check_contact);
                                    await crm_db.SaveChangesAsync();
                                }
                            }
                        }
                        else
                        {
                            // update
                            if (check_customer.customer_id != costomerid || check_customer.customer_address != customersAddress|| check_customer.customer_tel != item.address.tel|| check_customer.customer_abbreviation != item.address.wechat)
                            {
                                check_customer.customer_id = costomerid;
                                check_customer.customer_name = item.name;
                                check_customer.customer_address = customersAddress;
                                check_customer.customer_tel = item.address.tel;
                                check_customer.customer_abbreviation = item.address.wechat;
                                crm_db.Entry(check_customer).State = System.Data.Entity.EntityState.Modified;
                                for (int i = 0; i < item.contacts.Count(); i++)
                                {
                                    var contactId = item.contacts[i].address.addressable_id;
                                    var check_contact = crm_db.CRM_Contact.SingleOrDefault(m => m.contact_id == contactId);
                                    string ctAddress = item.contacts[i].address.region_info;
                                    contactlist.Add(contactId);
                                    if (check_contact == null)
                                    {
                                        // new
                                        check_contact = new CRM_Contact();
                                        check_contact.contact_id = contactId;
                                        check_contact.contact_name = item.contacts[i].name;
                                        check_contact.contact_address = ctAddress;
                                        check_contact.contact_tel = item.contacts[i].address.phone;
                                        check_contact.customer_id = check_customer.Id;
                                        check_contact.status = 0;
                                        crm_db.CRM_Contact.Add(check_contact);
                                    }
                                    else
                                    {
                                        //update
                                        if (check_contact.contact_id != contactId || check_contact.contact_address != ctAddress || check_contact.contact_tel != item.contacts[i].address.phone || check_contact.customer_id != check_customer.Id)
                                        {
                                            check_contact.contact_id = contactId;
                                            check_contact.contact_name = item.contacts[i].name;
                                            check_contact.contact_address = ctAddress;
                                            check_contact.contact_tel = item.contacts[i].address.phone;
                                            check_contact.customer_id = check_customer.Id;
                                            crm_db.Entry(check_contact).State = System.Data.Entity.EntityState.Modified;
                                        }
                                    }
                                    await crm_db.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
                else if (r.code == "100401")
                {
                    await RefreshUserToken();
                    return await GetCustomer(url_api);
                }
                else
                {
                    CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                    try_times++;
                    if (try_times >= 5)
                    {
                        logs.type = "customer";
                        logs.exception = "[customer]获取失败";
                        logs.exception_at = DateTime.Now;
                        crm_db.CRM_ExceptionLogs.Add(logs);
                        crm_db.SaveChanges();
                        try_times = 0;
                        return Json(new { result = "FAIL" });
                    }
                    return await GetCustomer(url_api);
                }
            }
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
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
        [HttpPost]
        public bool GetUserInfo()
        {
            //部门
            string get_department = "https://api.ikcrm.com/api/v2/user/department_list?per_page=" + UserInfo.Count + "&user_token=" + getUserToken().Result + "&device=dingtalk&version_code=9.8.0";
            var res = Get_Request(get_department);
            CRM_ContractDetail_ReturnData department_data = JsonConvert.DeserializeObject<CRM_ContractDetail_ReturnData>(res.Result);
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
                    }
                    else
                    {
                        if (department.system_code != item.Id || department.level != item.level|| department.name != item.name|| department.parent_id != item.parent_id|| department.can_use != item.can_use)
                        {
                            department.system_code = item.Id;
                            department.name = item.name;
                            department.level = item.level;
                            department.parent_id = item.parent_id;
                            department.can_use = item.can_use;
                            crm_db.Entry(department).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }
                crm_db.SaveChanges();
            }
            else if (department_data.code == "100401")
            {
                RefreshUserToken();
                return GetUserInfo();
            }
            else {
                return GetUserInfo();
            }
            //角色和用户
            string get_user = "https://api.ikcrm.com/api/v2/user/list?per_page=" + UserInfo.Count + "&sort=superior_id&order=asc&user_token=" + getUserToken().Result + "&device=dingtalk&version_code=9.8.0";
            var rest = Get_Request(get_user);
            CRM_ContractDetail_ReturnData user_data = JsonConvert.DeserializeObject<CRM_ContractDetail_ReturnData>(rest.Result);
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
                        if (role.name != item.role_json.name || role.entity_grant_scope != item.role_json.entity_grant_scope|| role.system_code != item.role_json.Id)
                        {
                            role.name = item.role_json.name;
                            role.entity_grant_scope = item.role_json.entity_grant_scope;
                            role.system_code = item.role_json.Id;
                            crm_db.Entry(role).State = System.Data.Entity.EntityState.Modified;
                        }
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
            else if(user_data.code == "100401") {
                RefreshUserToken();
                return GetUserInfo();
            }
            else
            {
                return GetUserInfo();
            }
            return true;
        }
        [HttpPost]
        public async Task<JsonResult> GetCrmInfo(string url_api)
        {
            var count = await Get_Count(url_api);
            var page = count / UserInfo.Count + 1;
            List<int> contractlist = new List<int>();
            List<int> CRM_Contractlist = new List<int>();
            for (int x = 1; x <= page; x++)
            {
                string url = "https://api.ikcrm.com/api/v2/contracts/?per_page=" + UserInfo.Count + "&page=" + x + "&approve_status=approved&status=3531567&user_token=" + await getUserToken() + "&device=dingtalk&version_code=9.8.0";
                var res = await Get_Request(url);
                CRM_Contract_ReturnData r = JsonConvert.DeserializeObject<CRM_Contract_ReturnData>(res);
                if (r.code == "0")
                {
                    foreach (var item in r.data.contracts)
                    {
                        var contractId = item.id;
                        var customerId = item.customer_id;
                        var check_customer = crm_db.CRM_Customer.SingleOrDefault(m => m.customer_id == customerId);
                        var check_data = crm_db.CRM_Contract.SingleOrDefault(m => m.contract_id == contractId);
                        var total_amount = item.total_amount;
                        var unreceived_amount = item.unreceived_amount;
                        var user_id = item.user_id;
                        var userId = crm_db.CRM_User.SingleOrDefault(m => m.system_code == user_id);
                        if (check_data == null)
                        {
                            //new
                            check_data = new CRM_Contract();
                            check_data.contract_id = item.id;
                            check_data.user_id = userId.Id;
                            check_data.user_name = userId.name;
                            check_data.customer_id = check_customer.Id;
                            check_data.contract_title = item.title;
                            check_data.total_amount = (double)total_amount;
                            check_data.unreceived_amount = (double)unreceived_amount;
                            check_data.contract_status = item.status;
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
                            if (check_data.user_id != userId.Id || check_data.customer_id != check_customer.Id || check_data.contract_title != item.title || check_data.total_amount != (double)total_amount || check_data.contract_status != item.status)
                            {
                                check_data.user_id = userId.Id;
                                check_data.user_name = userId.name;
                                check_data.customer_id = check_customer.Id;
                                check_data.contract_title = item.title;
                                check_data.total_amount = (double)total_amount;
                                check_data.unreceived_amount = (double)unreceived_amount;
                                check_data.contract_status = item.status;
                            }
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
                        contractlist.Add(item.id);
                        await crm_db.SaveChangesAsync();
                        await getSingleCrmDetailInfo(item.id);
                    }
                }
                else if (r.code == "100401")
                {
                    await RefreshUserToken();
                    return await GetCrmInfo(url_api);
                }
                else
                {
                    CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                    try_times++;
                    if (try_times >= 5)
                    {
                        logs.type = "contract";
                        logs.exception = "[contract]获取失败";
                        logs.exception_at = DateTime.Now;
                        crm_db.CRM_ExceptionLogs.Add(logs);
                        crm_db.SaveChanges();
                        try_times = 0;
                        return Json(new { result = "FAIL" });
                    }
                    return await GetCrmInfo(url_api);
                }
            }
            var CRM_Contract = from m in crm_db.CRM_Contract
                               where m.contract_status == UserInfo.status_unsend && m.contract_status != UserInfo.delete
                               select m;
            foreach (var crm in CRM_Contract)
            {
                CRM_Contractlist.Add(crm.contract_id);
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
            return Json(new { result = "SUCCESS" });
        }
        
        public async Task<string> getSingleCrmDetailInfo(int contract_id)
        {
            var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.contract_id == contract_id);
            string url = "https://api.ikcrm.com/api/v2/contracts/" + contract.contract_id + "?user_token=" + await getUserToken() + "&device=dingtalk&version_code=9.8.0";
            var res = await Get_Request(url);
            CRM_ContractDetail_ReturnData r = JsonConvert.DeserializeObject<CRM_ContractDetail_ReturnData>(res);
            if (r.code == "0")
            {
                foreach (var item in r.data.product_assets_for_new_record)
                {
                    var pid = item.product_id;
                    var s_price = item.recommended_unit_price;
                    var quantity = item.quantity;
                    var product_name = item.name;
                    var product_code = item.product_no;
                    var contractdetail = from m in crm_db.CRM_ContractDetail
                                         where m.contract_id == contract.id && m.CRM_Contract.contract_status != UserInfo.delete
                                         select m;
                    if (contractdetail != null)
                    {
                        crm_db.CRM_ContractDetail.RemoveRange(contractdetail);
                    }
                    var contractDetail = new CRM_ContractDetail();
                    contractDetail.contract_id = contract.id;
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
                    }
                    else if (shop_code.Contains("线上其他渠道"))
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
                    }
                    else
                    {
                        contract.shop_code = "006";
                    }
                }
                else
                {
                    contract.shop_code = "006";
                }
                contract.contract_type = r.data.category_mapped;
                contract.receiver_name = r.data.text_asset_73f972;
                contract.receiver_address = r.data.text_asset_eb802b;
                contract.receiver_tel = r.data.text_asset_da4211;
                contract.express_remark = r.data.text_asset_7fd81a;
                contract.contract_remark = r.data.special_terms;
                contract.created_at = r.data.created_at;
                contract.platform_code = "IK" + r.data.created_at.ToString("yyyyMMddHHmmss") + contract_id;
                crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
                checkAddress(r.data.text_asset_eb802b, contract.id);
            }
            else if (r.code == "100401")
            {
                await RefreshUserToken();
                return await getSingleCrmDetailInfo(contract_id);
            }
            else
            {
                CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                try_times++;
                if (try_times >= 5)
                {
                    logs.type = "contractDetial";
                    logs.exception = "[contractDetial]获取失败";
                    logs.exception_at = DateTime.Now;
                    crm_db.CRM_ExceptionLogs.Add(logs);
                    crm_db.SaveChanges();
                    try_times = 0;
                    return "FAIL";
                }
                return await getSingleCrmDetailInfo(contract_id);

            }
            await crm_db.SaveChangesAsync();
            return "SUCCESS";
        }

        private async Task<int> UpdateCRM(int cid, string contract_status, string express_information, string express_remark)
        {
            var contracts = crm_db.CRM_Contract.SingleOrDefault(m => m.id == cid);
            string url = "https://api.ikcrm.com/api/v2/contracts/" + contracts.contract_id + "?user_token=" + await  getUserToken() + "&device=dingtalk&version_code=9.8.0";
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
                return await UpdateCRM(cid, contract_status, express_information, express_remark);
            }
            CRM_Contract_ReturnData r = JsonConvert.DeserializeObject<CRM_Contract_ReturnData>(retString);
            if (r.code == "0")
            {
                return 1; // 1 正确
            }
            else if (r.code == "100401")
            {
                await RefreshUserToken();
                return await UpdateCRM(cid, contract_status, express_information, express_remark);
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
        public ActionResult CRM_undeliveredPartical(string status, int? page, string shopCode, string query)
        {
            var user = getUser(User.Identity.Name);
            int _page = page ?? 1;
            if (user.role_id == UserInfo.SuperAdmin || user.role_id == UserInfo.Finance)
            {
                if (shopCode == "0")
                {
                    var undeliveredData = (from m in crm_db.CRM_Contract
                                           where m.contract_status == status && (m.contract_title.Contains((query != null & query != "" ? query : m.contract_title))
                                           || m.user_name.Contains((query != null & query != "" ? query : m.user_name))
                                           || m.platform_code.Contains((query != null & query != "" ? query : m.platform_code)))
                                           orderby m.edit_time descending
                                           select m).ToPagedList(_page, 20);
                    return PartialView(undeliveredData);
                }
                else
                {
                    var undeliveredData = (from m in crm_db.CRM_Contract
                                           where m.contract_status == status && m.shop_code == shopCode && (m.contract_title.Contains((query != null & query != "" ? query : m.contract_title))
                                           || m.user_name.Contains((query != null & query != "" ? query : m.user_name))
                                           || m.platform_code.Contains((query != null & query != "" ? query : m.platform_code)))
                                           orderby m.edit_time descending
                                           select m).ToPagedList(_page, 20);
                    return PartialView(undeliveredData);
                }
            }
            else if (user.role_id == UserInfo.Assistant || user.role_id == UserInfo.Manager)
            {
                var crm_user = crm_db.CRM_User.SingleOrDefault(m => m.email == user.email);
                if (crm_user.CRM_Department.parent_id == null)
                {
                    var employee = from m in crm_db.CRM_User
                                   where m.department_id == crm_user.department_id
                                   select m;
                    if (shopCode == "0")
                    {
                        var undeliveredData = (from m in crm_db.CRM_Contract
                                               join c in employee on m.user_id equals c.Id
                                               where m.contract_status == status && (m.contract_title.Contains((query != null & query != "" ? query : m.contract_title))
                                               || m.user_name.Contains((query != null & query != "" ? query : m.user_name))
                                               || m.platform_code.Contains((query != null & query != "" ? query : m.platform_code)))
                                               orderby m.edit_time descending
                                               select m).ToPagedList(_page, 20);
                        return PartialView(undeliveredData);
                    }
                    else
                    {
                        var undeliveredData = (from m in crm_db.CRM_Contract
                                               join c in employee on m.user_id equals c.Id
                                               where m.contract_status == status && m.shop_code == shopCode && (m.contract_title.Contains((query != null & query != "" ? query : m.contract_title))
                                               || m.user_name.Contains((query != null & query != "" ? query : m.user_name))
                                               || m.platform_code.Contains((query != null & query != "" ? query : m.platform_code)))
                                               orderby m.edit_time descending
                                               select m).ToPagedList(_page, 20);
                        return PartialView(undeliveredData);
                    }
                }
                else
                {
                    var parent_department = crm_db.CRM_Department.SingleOrDefault(m => m.system_code == crm_user.CRM_Department.parent_id);
                    var sub_department = from m in crm_db.CRM_Department
                                         where m.parent_id == parent_department.system_code || m.system_code == parent_department.system_code
                                         select m;
                    var employee = from m in crm_db.CRM_User
                                   join c in sub_department on m.department_id equals c.Id
                                   select m;
                    if (shopCode == "0")
                    {
                        var undeliveredData = (from m in crm_db.CRM_Contract
                                               join c in employee on m.user_id equals c.Id
                                               where m.contract_status == status && (m.contract_title.Contains((query != null & query != "" ? query : m.contract_title))
                                               || m.user_name.Contains((query != null & query != "" ? query : m.user_name))
                                               || m.platform_code.Contains((query != null & query != "" ? query : m.platform_code)))
                                               orderby m.edit_time descending
                                               select m).ToPagedList(_page, 20);
                        return PartialView(undeliveredData);
                    }
                    else
                    {
                        var undelivereddata = (from m in crm_db.CRM_Contract
                                               join c in employee on m.user_id equals c.Id
                                               where m.contract_status == status && m.shop_code == shopCode && (m.contract_title.Contains((query != null & query != "" ? query : m.contract_title))
                                               || m.user_name.Contains((query != null & query != "" ? query : m.user_name))
                                               || m.platform_code.Contains((query != null & query != "" ? query : m.platform_code)))
                                               orderby m.edit_time descending
                                               select m).ToPagedList(_page, 20);
                        return PartialView(undelivereddata);
                    }
                }
            }
            else
            {
                if (shopCode == "0")
                {
                    var undeliveredData = (from m in crm_db.CRM_Contract
                                           where m.contract_status == status && m.user_id == user.Id && (m.contract_title.Contains((query != null & query != "" ? query : m.contract_title))
                                           || m.user_name.Contains((query != null & query != "" ? query : m.user_name))
                                           || m.platform_code.Contains((query != null & query != "" ? query : m.platform_code)))
                                           orderby m.edit_time descending
                                           select m).ToPagedList(_page, 20);
                    return PartialView(undeliveredData);
                }
                else
                {
                    var undeliveredData = (from m in crm_db.CRM_Contract
                                           where m.contract_status == status && m.shop_code == shopCode && m.user_id == user.Id && m.user_id == user.Id && (m.contract_title.Contains((query != null & query != "" ? query : m.contract_title))
                                           || m.user_name.Contains((query != null & query != "" ? query : m.user_name))
                                           || m.platform_code.Contains((query != null & query != "" ? query : m.platform_code)))
                                           orderby m.edit_time descending
                                           select m).ToPagedList(_page, 20);
                    return PartialView(undeliveredData);
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
            var seller = getUser(User.Identity.Name);
            var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.id == c_id);
            if (seller.role_id == UserInfo.SuperAdmin)
            {
                contract.received_payments_status = UserInfo.received_payments_status;
                contract.employee_id = seller.Id;
                contract.employee_name = seller.name;
                crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        public CRM_User getUser(string username)
        {
            var user = crm_db.CRM_User.SingleOrDefault(m => m.email == username);
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
            var strresult = "";
            var result = "";
            foreach (var cId in c_id)
            {
                result =  getSingleErpOrders(cId);
                if (result.Contains("SUCCESS"))
                {
                    strresult = result.Replace("SUCCESS", "");
                    successList.Add(strresult);
                }
                else if (result.Contains("PARTIAL"))
                {
                    strresult = result.Replace("PARTIAL", "");
                    errorList.Add(strresult);
                }
                else if (result.Contains("ERROR"))
                {
                    strresult = result.Replace("ERROR", "");
                    partialList.Add(strresult);
                }
                else
                {
                    failList.Add(result);
                }
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS", successlist = successList, errorlist = errorList ,faillist = failList , partiallist = partialList });
        }

        public string getSingleErpOrders(int contractId)
        {
            var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.id == contractId);
            var fail = "";
            var partial = "";
            var success = "";
            var error = "";
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
                            success = contract.platform_code + " " + contract.contract_title + "SUCCESS";
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
                                }
                                else
                                {
                                    contractlist.Add(r.orders[0].deliverys[i].express_name + r.orders[0].deliverys[i].mail_no);
                                    deliveryslist.Add(getDeliverys(r.orders[0].deliverys[i].mail_no));
                                }
                            }
                            success = contract.platform_code + " " + contract.contract_title + "SUCCESS";
                            string express_information = string.Join(";", contractlist.ToArray());
                            string express_remark = string.Join(";", deliveryslist.ToArray());
                            contract.express_information = express_information;
                            contract.express_remark = express_remark;
                            crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            partial = contract.platform_code + " " + contract.contract_title + "PARTIAL";
                            return partial;
                        }
                        var updatcrm = UpdateCRM(contractId, contract.contract_status, contract.express_information, contract.express_remark);
                        if (updatcrm.Result != 1)
                        {
                            error = contract.platform_code + " " + contract.contract_title + "ERROR";
                            return error;
                        }
                    }
                    else
                    {
                        fail = contract.platform_code + " " + contract.contract_title + " " + r.errorDesc;
                        return fail;
                    }
                }
            }
            catch (Exception)
            {
                streamWriter.Close();
                CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                try_times++;
                if (try_times >= 5)
                {
                    logs.type = "getErpOrder";
                    logs.exception = "[ErpOrder]获取失败";
                    logs.exception_at = DateTime.Now;
                    crm_db.CRM_ExceptionLogs.Add(logs);
                    crm_db.SaveChanges();
                    try_times = 0;
                    return  "FAIL";
                }
                return getSingleErpOrders(contractId);
            }
            return success;
        }
        [HttpPost]
        public JsonResult createOrder(int[] c_id, string province, string city, string district)
        {
            List<string> failList = new List<string>();
            List<string> partialList = new List<string>();
            List<string> successList = new List<string>();
            var result = "";
            var strresult = "";
            foreach (var _Cid in c_id)
            {
                result = creatSingleOrder(_Cid, province, city, district);
                if (result.Contains("SUCCESS"))
                {
                    strresult = result.Replace("SUCCESS", "");
                    successList.Add(strresult);
                }
                else if (result.Contains("PARTIAL"))
                {
                    strresult = result.Replace("PARTIAL", "");
                    partialList.Add(strresult);
                }
                else
                {
                    failList.Add(result);
                }
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS" , successlist = successList, faillist = failList, partiallist = partialList });
        }

        public string creatSingleOrder(int contractId, string province, string city, string district)
        {
            var fail ="" ;
            var partial = "";
            var success = "";
            var seller = getUser(User.Identity.Name);
            var contract = crm_db.CRM_Contract.SingleOrDefault(m => m.id == contractId && m.contract_status == UserInfo.status_unsend && m.received_payments_status == UserInfo.received_payments_status);
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
                receiver_zip = "200000",
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
                    payment = (decimal)item.CRM_Contract.total_amount
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
                contract.employee_id = seller.Id;
                contract.employee_name = seller.name;
                contract.edit_time = DateTime.Now;
                crm_db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
                crm_db.SaveChanges();
                success = contract.platform_code + " " + contract.contract_title + "SUCCESS";
                var updatcrm = UpdateCRM(contractId, contract.contract_status, contract.express_information, contract.express_remark);
                if (updatcrm.Result != 1)
                {
                    partial = contract.platform_code + " " + contract.contract_title + "PARTIAL";
                    return partial;
                }
            }
            else
            {
                fail = contract.platform_code + " " + contract.contract_title + " " + r.errorDesc;
                return fail;
            }
            return success;
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

        // MD
        public ActionResult MD_OrderView()
        {
            return View();
        }
        
        public ActionResult MD_OrderPartialView(int? page,string query,int create_status)
        {
            int _page = page ?? 1;
            if(create_status == -1)
            {
                if (query != "")
                {
                    var order = from m in md_db.MD_Order
                                where m.receiver_times == 1
                                select m;
                    var SearchResult = (from m in order
                                        where m.order_code.Contains(query) || m.MD_Product.product_code.Contains(query)
                                        orderby m.receiver_date descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in md_db.MD_Order
                                        where m.receiver_times == 1
                                        orderby m.receiver_date descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }else
            {
                if (query != "")
                {
                    var order = from m in md_db.MD_Order
                                where m.receiver_times == 1 && m.createSub_status == create_status
                                select m;
                    var SearchResult = (from m in order
                                        where m.order_code.Contains(query) || m.MD_Product.product_code.Contains(query)
                                        orderby m.receiver_date descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in md_db.MD_Order
                                        where m.receiver_times == 1 && m.createSub_status == create_status
                                        orderby m.receiver_date descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
            
        }

        public int ReceiverTimes(int order_id)
        {
            var count = from m in md_db.MD_Order
                        where m.upload_status == 0 && m.parentOrder_id == order_id
                        select m;
            return count.Count();
        }

        public ActionResult MD_OrderDetailView(int order_id)
        {
            var orderdetail = md_db.MD_Order.SingleOrDefault(m => m.Id == order_id && m.receiver_times == 1);
            ViewBag.orderDetail = orderdetail.parentOrder_id;
            return View();
        }

        public ActionResult MD_OrderDetailPartialView(int parentOrder_id)
        {
            var orderDetail = from m in md_db.MD_Order
                              where m.parentOrder_id == parentOrder_id
                              orderby m.receiver_date ascending
                              select m;
            return PartialView(orderDetail);
        }
        // 合并
        [HttpPost]
        public JsonResult Amalgamate_Order(int[] order_id)
        {
            var FirstOid = order_id[0];
            var LastOid = order_id[order_id.Count()-1];
            DateTime? receiverDate = null;
            var Order = md_db.MD_Order.SingleOrDefault(m => m.Id == FirstOid && m.upload_status == 0);
            var Orders = md_db.MD_Order.SingleOrDefault(m => m.Id == LastOid && m.upload_status == 0);
            var t1 = DateTime.Parse(Order.receiver_date.Value.ToString("yyyy-MM-dd"));
            var t2 = DateTime.Parse(Orders.receiver_date.Value.ToString("yyyy-MM-dd"));
            int total_quantity = 0;
            if (t1 > t2)
            {
                receiverDate = t2;
            }else
            {
                receiverDate = t1;
            }
            foreach (var oId in order_id)
            {
                var order = md_db.MD_Order.SingleOrDefault(m => m.Id == oId && m.delivery_state == 0 && m.receiver_times != 1);
                total_quantity += order.qty;
                md_db.MD_Order.Remove(order);
            }
            var count = order_id.Count();
            if (count != 0)
            {
                var OrderDetail = new MD_Order();
                if (Order.order_code.Contains("MD"))
                {
                    OrderDetail.order_code = Order.order_code;
                }
                else
                {
                    OrderDetail.order_code = "MD" + Order.order_code;
                }
                OrderDetail.qty = total_quantity;
                OrderDetail.receiver_date = receiverDate;
                OrderDetail.delivery_state = 0;
                OrderDetail.receiver_area = Order.receiver_area;
                OrderDetail.receiver_address = Order.receiver_address;
                OrderDetail.parentOrder_id = Order.parentOrder_id;
                OrderDetail.receiver_tel = Order.receiver_tel;
                OrderDetail.product_id = Order.product_id;
                OrderDetail.vip_code = Order.vip_code;
                OrderDetail.receiver_times = Order.receiver_times;
                OrderDetail.express_information = " ";
                OrderDetail.receiver_name = Order.receiver_name;
                OrderDetail.order_status = 1;
                md_db.MD_Order.Add(OrderDetail);
                md_db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        // 取消发送
        public JsonResult Cancel_Order(int order_id)
        {
            var order = from m in md_db.MD_Order
                        where m.parentOrder_id == order_id  && m.delivery_state == 0 && m.upload_status != 1 && m.receiver_times != 1
                        select m;
            var singleOrder = md_db.MD_Order.SingleOrDefault(m => m.Id == order_id);
            if(order.Count() != 0)
            {
                md_db.MD_Order.RemoveRange(order);
                MD_Record successlogs = new MD_Record();
                successlogs.record_date = DateTime.Now;
                successlogs.record_type = "Cancel";
                successlogs.record_detail = singleOrder.order_code +"已取消复购订单发送";
                md_db.MD_Record.Add(successlogs);
                md_db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }else
            {
                return Json(new { result = "FAIL" });
            }
        }
        // 更改
        public ActionResult MD_EditOrderInfo(int order_id)
        {
            var order = md_db.MD_Order.SingleOrDefault(m => m.Id == order_id);
            ViewBag.Order = order;
            return PartialView(order);
        }
        [HttpPost]
        public ActionResult MD_EditOrderInfo(MD_Order model)
        {
            if (ModelState.IsValid)
            {
                bool OrderDate = md_db.MD_Order.Any(m => m.receiver_date == model.receiver_date);
                //bool OrderAddress = md_db.MD_Order.Any(m => m.receiver_area == model.receiver_area || m.receiver_address == model.receiver_address);
                MD_Order Orders = new MD_Order();
                if (TryUpdateModel(Orders))
                {
                    md_db.Entry(Orders).State = System.Data.Entity.EntityState.Modified;
                    MD_Record Editlogs = new MD_Record();
                    Editlogs.record_date = DateTime.Now;
                    Editlogs.record_type = "Edit";
                    if (OrderDate == false)
                    {
                        Editlogs.record_detail = Orders.order_code + " 新增日期修改: " + model.receiver_date;
                    }
                    else
                    {
                        Editlogs.record_detail = Orders.order_code + " 新增修改信息";
                    }
                    md_db.MD_Record.Add(Editlogs);
                    md_db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            return Json(new { result = "FAIL" });
        }
        // 手工获取ERP订单信息
        [HttpPost]
        public JsonResult getSingleErpOrder(string platform_code)
        {
            var md_order = md_db.MD_Order.SingleOrDefault(m => m.order_code == platform_code);
            string json = "{" +
                   "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.trade.get\"," +
                    //"\"receiver_mobile\":\"" + platform_code + "\"," +
                    "\"platform_code\":\"" + platform_code + "\"," +
                    //"\"platform_code\":\"" + platform_code + "\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"" +
                    "}";
            string signature = sign(json, AppSecret);
            string info = "{" +
                   "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.trade.get\"," +
                    //"\"receiver_mobile\":\"" + platform_code + "\"," +
                    "\"platform_code\":\"" + platform_code + "\"," +
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
                    orders_Result r = JsonConvert.DeserializeObject<orders_Result>(sb.ToString());
                    if (r.success)
                    {
                        if (r.orders.Count() != 0)
                        {
                            if (md_order == null)
                            {

                                md_order = new MD_Order();
                                if (r.orders[0].details[0].note.Contains("sqz333"))
                                {
                                    md_order.product_id = 1;
                                }
                                else if (r.orders[0].details[0].note.Contains("sqz444"))
                                {
                                    md_order.product_id = 3;
                                }
                                else
                                {
                                    return Json(new { result = "FAIL" });
                                }
                                var strAddre = r.orders[0].receiver_address;
                                var indexAddre1 = strAddre.IndexOf(" ");
                                var indexAddre2 = strAddre.IndexOf(" ", indexAddre1 + 1);
                                var indexAddre3 = strAddre.IndexOf(" ", indexAddre2 + 1) + 1;
                                var strReceiver_address = strAddre.Substring(indexAddre3, strAddre.Length - indexAddre3);
                                var strArea = r.orders[0].receiver_area;
                                var strReceiver_area = strArea;
                                if (strArea == null)
                                {
                                    strReceiver_area = strAddre.Substring(0, indexAddre3 - 1);
                                    strReceiver_area = strReceiver_area.Replace(" ", "-");
                                }
                                string[] t = strReceiver_area.Split('-');
                                var findT = (t.Length - 1);
                                if (findT < 2)
                                {
                                    strReceiver_area = strReceiver_area + "-";
                                }
                                md_order.order_code = r.orders[0].platform_code;
                                md_order.receiver_date = r.orders[0].createtime.Date;
                                md_order.order_status = 0;
                                if (r.orders[0].deliverys.Count != 0)
                                {
                                    md_order.express_information = r.orders[0].deliverys[0].express_name + r.orders[0].deliverys[0].mail_no;
                                }
                                else
                                {
                                    md_order.express_information = "";
                                }
                                md_order.remark = r.orders[0].buyer_memo;
                                md_order.receiver_address = strReceiver_address;
                                md_order.receiver_area = strReceiver_area;
                                md_order.upload_status = 1;
                                md_order.receiver_tel = r.orders[0].receiver_mobile;
                                md_order.vip_code = r.orders[0].vip_code;
                                md_order.receiver_times = 1;
                                md_order.qty = (int)r.orders[0].qty / 3;
                                md_order.amount = r.orders[0].amount;
                                md_order.discount_fee = r.orders[0].discount_fee;
                                md_order.payment_amount = r.orders[0].payment_amount;
                                md_order.delivery_state = r.orders[0].delivery_state;
                                md_order.payment = r.orders[0].payment;
                                md_order.receiver_name = r.orders[0].receiver_name;
                                md_db.MD_Order.Add(md_order);
                                MD_Record successlogs = new MD_Record();
                                successlogs.record_date = DateTime.Now;
                                successlogs.record_type = "Success";
                                successlogs.record_detail = r.orders[0].platform_code;
                                md_db.MD_Record.Add(successlogs);
                                md_db.SaveChanges();
                                md_order.parentOrder_id = md_order.Id;
                                md_db.Entry(md_order).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                return Json(new { result = "ERROR" });
                            }
                        }
                        else
                        {
                            return Json(new { result = "NOTFOUND" });
                        }
                    }
                }
            }
            catch (Exception)
            {
                streamWriter.Close();
                MD_Record errorlogs = new MD_Record();
                try_times++;
                if (try_times >= 5)
                {
                    errorlogs.record_date = DateTime.Now;
                    errorlogs.record_type = "FAIL";
                    errorlogs.record_detail = "[ErpOrder]获取失败";
                    md_db.MD_Record.Add(errorlogs);
                    md_db.SaveChanges();
                    try_times = 0;
                    return Json(new { result = "SYSTEMERROR" });
                }
                return getSingleErpOrder(platform_code);
            }
            md_db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
        // 生成子订单
        public ActionResult CreateSubOrders(int order_id)
        {
            var order = md_db.MD_Order.SingleOrDefault(m => m.Id == order_id);
            return PartialView(order);
        }
        [HttpPost]
        public ActionResult CreateSubOrders(MD_Order model,int order_qty,int times,int product_id)
        {
            var order = md_db.MD_Order.SingleOrDefault(m => m.Id == model.Id);
            for(int i = 1; i< times+1; i++)
            {
                var subOrder = new MD_Order();
                subOrder.order_code = "MD" + order.order_code + "-" + i;
                subOrder.receiver_date = order.receiver_date.Value.AddDays(+i * 30);
                subOrder.order_status = 0;
                subOrder.remark = order.remark;
                subOrder.receiver_address = order.receiver_address;
                subOrder.receiver_tel = order.receiver_tel;
                subOrder.vip_code = order.vip_code;
                subOrder.receiver_area = order.receiver_area;
                subOrder.receiver_times = i+1;
                subOrder.qty = order_qty;
                subOrder.product_id = product_id;
                subOrder.parentOrder_id = order.Id;
                subOrder.receiver_name = order.receiver_name;
                subOrder.express_information = "";
                md_db.MD_Order.Add(subOrder);
            }
            order.createSub_status = 1;
            MD_Record successlogs = new MD_Record();
            successlogs.record_date = DateTime.Now;
            successlogs.record_type = "Create";
            successlogs.record_detail = order.order_code + "生成复购订单数:"+ times;
            md_db.MD_Record.Add(successlogs);
            md_db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
    }
}