using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using PeriodAid.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;

namespace PeriodAid.Controllers
{
    public class ExaminationController : Controller
    {
        // GET: Examination
        // GET: TestAdmin
        OfflineSales offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public ExaminationController()
        {

        }

        public ExaminationController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index()
        {
            //CreateExamintion("123", "111", 3);
            return Content("ddd");
        }

        public ActionResult ExamBody(string open_id, string nickname, int testtype)
        {
            var exam = offlineDB.Examination.SingleOrDefault(m => m.OpenId == open_id);
            //var exam = offlineDB.Examination.SingleOrDefault(m => m.ID == 7);
            if (exam!=null){
                return View(exam);
            }
            else
            {
                exam = CreateExamintion(open_id, nickname, testtype, 30);
                return RedirectToAction("ExamBody", new { open_id = open_id, nickname = nickname, testtype=testtype });
            }
            
        }

        public ActionResult getCurrentQuestion(int examId)
        {
            var exam = offlineDB.Examination.SingleOrDefault(m => m.ID == examId);
            if (exam != null)
            {
                int currentSequence = exam.CurrentSequence ?? 0;
                if (currentSequence < exam.MaxSequence)
                {
                    ExaminationDetails details = (from m in offlineDB.ExaminationDetails
                                                  where m.ExaminationID == exam.ID && m.Sequence == currentSequence
                                                  select m).FirstOrDefault();
                    TestQuestion question = details.TestQuestion;
                    return PartialView(question);
                }
                else
                {
                    return RedirectToAction("FinishExam", new { examId = exam.ID});
                }
            }
            else
            {
                return PartialView();
            }
        }
        public JsonResult SubmitAndNext(int examId, int answerId)
        {
            var exam = offlineDB.Examination.SingleOrDefault(m => m.ID == examId);
            if (exam != null)
            {
                int currentSequence = exam.CurrentSequence ?? 0;
                if (currentSequence < exam.MaxSequence)
                {
                    ExaminationDetails details = (from m in offlineDB.ExaminationDetails
                                                  where m.ExaminationID == exam.ID && m.Sequence == currentSequence
                                                  select m).FirstOrDefault();
                    TestAnswer answer = offlineDB.TestAnswer.SingleOrDefault(m => m.ID == answerId);
                    if (answer.AnswerProperty)
                    {
                        details.Result = true;
                    }
                    else
                        details.Result = false;
                    exam.CurrentSequence = currentSequence + 1;
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS", questionresult=details.Result }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { result = "FAIL" }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult FinishExam(int examId)
        {
            var exam = offlineDB.Examination.SingleOrDefault(m => m.ID == examId);
            if (exam != null)
            {
                if (exam.ExaminationDetails.Count(m => m.Result == false) == 0)
                {
                    exam.ExaminationStatus = true;
                    exam.ExaminationFinishTime = DateTime.Now;
                }
                offlineDB.SaveChanges();
                return PartialView(exam);
            }
            return PartialView("Error");
        }

        public JsonResult resetExam(int examId)
        {
            var exam = offlineDB.Examination.SingleOrDefault(m => m.ID == examId);
            if (exam != null)
            {
                exam.CurrentSequence = 0;
                exam.ExaminationStatus = false;
                exam.ExaminationFinishTime = null;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "FAIL" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckoutExam(int examId)
        {
            //var exam = offlineDB.Examination.SingleOrDefault(m => m.OpenId == open_id);
            var exam = offlineDB.Examination.SingleOrDefault(m => m.ID == examId);
            if (exam != null)
            {
                //exam.ExaminationDetails.Where(m => m.Result == false);
                return View(exam);
            }
            return View("Error");
        }

        public ActionResult getNextCheckout(int examId)
        {
            var exam = offlineDB.Examination.SingleOrDefault(m => m.ID == examId);
            if (exam != null)
            {
                var checkoutlist = exam.ExaminationDetails.Where(m => m.Result == false).ToArray();
                int count = checkoutlist.Count();
                if (count > 0)
                {
                    // 返回随机错题
                    Random r = new Random((int)DateTime.Now.Ticks);
                    ExaminationDetails details = checkoutlist[r.Next(0, count - 1)];
                    return PartialView(details);
                }
                else
                {
                    // 无错题
                    return RedirectToAction("FinishExam", new { examId = exam.ID });

                }
            }
            return View("Error");
        }

        public JsonResult SubmitCheckout(int detailsId, int answerId)
        {
            var details = offlineDB.ExaminationDetails.SingleOrDefault(m => m.ID == detailsId);
            if (details != null)
            {
                TestAnswer answer = offlineDB.TestAnswer.SingleOrDefault(m => m.ID == answerId);
                if (answer.AnswerProperty)
                {
                    details.Result = true;
                }
                else
                    details.Result = false;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS", questionresult = details.Result }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { result = "FAIL" });
        }
        private Examination CreateExamintion(string OpenId, string NickName, int TestType, int count)
        {
            var questionList = from m in offlineDB.TestQuestion
                               where m.TestTypeID == TestType
                               select m;
            if (count > questionList.Count())
                count = questionList.Count();
            Examination exam = new Examination()
            {
                OpenId = OpenId,
                NickName = NickName,
                TestTypeID = TestType,
                CurrentSequence = 0,
                ExaminationStatus = false,
                ExaminationCreateTime = DateTime.Now,
                MaxSequence = count
            };
            offlineDB.Examination.Add(exam);
            List<QuestionSequence> sequence = new List<QuestionSequence>();
            Random r = new Random((int)DateTime.Now.Ticks);
            foreach(var item in questionList)
            {
                QuestionSequence tempitem = new QuestionSequence()
                {
                    TestQuestion = item,
                    RandomCode = r.Next(0, 1000)
                };
                sequence.Add(tempitem);
            }
            int i = 0;
            foreach(var item in sequence.OrderBy(m => m.RandomCode).Take(count))
            {
                ExaminationDetails details = new ExaminationDetails()
                {
                    Examination = exam,
                    QuestionID = item.TestQuestion.ID,
                    Sequence = i
                };
                offlineDB.ExaminationDetails.Add(details);
                i++;
            }
            offlineDB.SaveChanges();
            return exam;
        }

        // 微信登陆
        public ActionResult Wx_RedirectExamBody(int testtype)
        {
            string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Examination/Wx_Authorization");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=" + testtype + "#wechat_redirect";
            return Redirect(url);
        }

        public ActionResult Wx_Authorization(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            var userinfo = wechat.getWebOauthUserInfo(jat.access_token, jat.openid);
            return RedirectToAction("ExamBody", new { open_id = userinfo.openid, nickname = userinfo.nickname, testtype = state});
        }

        private class QuestionSequence
        {
            public TestQuestion TestQuestion { get; set; }

            public int RandomCode { get; set; }
        }
    }
    
}