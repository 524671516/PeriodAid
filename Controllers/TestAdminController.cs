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
using PagedList;

namespace PeriodAid.Controllers
{
    [Authorize(Roles ="Admin")]
    public class TestAdminController : Controller
    {
        // GET: TestAdmin
        OfflineSales offlineDB = new OfflineSales();
        PressConference pcdb = new PressConference();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public TestAdminController()
        {

        }

        public TestAdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
            return View();
        }

        public ActionResult AddQuestion()
        {

            CreateQuestionViewModel model = new CreateQuestionViewModel();
            
            ViewBag.Store_System = new SelectList(offlineDB.TestType, "Id", "CustomName");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddQuestion(CreateQuestionViewModel model, FormCollection form)
        {
            //return Content(Convert.ToInt32(form["selectC"]).ToString());
            if (ModelState.IsValid)
            {
                try
                {
                    TestQuestion q = new TestQuestion()
                    {
                        QuestionContent = model.questionArea,
                        TestTypeID = model.TestTypeID,
                        modifyUser = User.Identity.Name,
                        modifyDateTime = DateTime.Now
                    };
                    TestAnswer a1 = new TestAnswer()
                    {
                        AnswerContent = model.answerA,
                        TestQuestion = q,
                        AnswerProperty = (model.answerRight == 1)? true :false
                    };
                    TestAnswer a2 = new TestAnswer()
                    {
                        AnswerContent = model.answerB,
                        TestQuestion = q,
                        AnswerProperty = (model.answerRight == 2) ? true : false
                    };
                    TestAnswer a3 = new TestAnswer()
                    {
                        AnswerContent = model.answerC,
                        TestQuestion = q,
                        AnswerProperty = (model.answerRight == 3) ? true : false
                    };
                    TestAnswer a4 = new TestAnswer()
                    {
                        AnswerContent = model.answerD,
                        TestQuestion = q,
                        AnswerProperty = (model.answerRight == 4) ? true : false
                    };
                    offlineDB.TestQuestion.Add(q);
                    offlineDB.TestAnswer.Add(a1);
                    offlineDB.TestAnswer.Add(a2);
                    offlineDB.TestAnswer.Add(a3);
                    offlineDB.TestAnswer.Add(a4);
                    offlineDB.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    ModelState.AddModelError("", "出现错误");
                    ViewBag.Store_System = new SelectList(offlineDB.TestType, "Id", "CustomName");
                    return View(model);
                }
                
            }
            else
            {
                ModelState.AddModelError("", "出现错误");
                ViewBag.Store_System = new SelectList(offlineDB.TestType, "Id", "CustomName");
                return View(model);
            }

            
        }
        public ActionResult QuestonList(int id)
        {
            var list = offlineDB.TestQuestion.Where(m => m.TestTypeID == id);
            return View(list);
        }

        public ActionResult EditQuestion(int id)
        {
            var item = offlineDB.TestQuestion.SingleOrDefault(m => m.ID == id);
            if (item != null)
            {
                var answerlist = (from m in offlineDB.TestAnswer
                                 where m.TestQuestionId == item.ID
                                 select m).Take(4).ToArray();
                
                string o_answerA = answerlist[0].AnswerContent;
                string o_answerB = answerlist[1].AnswerContent;
                string o_answerC = answerlist[2].AnswerContent;
                string o_answerD = answerlist[3].AnswerContent;
                int answerproperty = 1;
                for (int i = 0; i<4; i++)
                {
                    if (answerlist[i].AnswerProperty)
                    {
                        answerproperty = i + 1;
                    }
                }
                CreateQuestionViewModel model = new CreateQuestionViewModel()
                {
                    TestTypeID = item.TestTypeID,
                    questionArea = item.QuestionContent,
                    answerA = o_answerA,
                    answerB = o_answerB,
                    answerC = o_answerC,
                    answerD = o_answerD,
                    answerRight = answerproperty
                };
                ViewBag.Store_System = new SelectList(offlineDB.TestType, "Id", "CustomName");
                return View(model);
            }
            else
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditQuestion(int id, CreateQuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var question = offlineDB.TestQuestion.SingleOrDefault(m => m.ID == id);
                    if (question != null)
                    {
                        question.QuestionContent = model.questionArea;
                        question.TestTypeID = model.TestTypeID;
                        question.modifyUser = User.Identity.Name;
                        question.modifyDateTime = DateTime.Now;

                        var answerlist = (from m in offlineDB.TestAnswer
                                          where m.TestQuestionId == question.ID
                                          select m).ToArray();
                        answerlist[0].AnswerContent = model.answerA;
                        answerlist[0].AnswerProperty = (model.answerRight == 1) ? true : false;
                        answerlist[1].AnswerContent = model.answerB;
                        answerlist[1].AnswerProperty = (model.answerRight == 2) ? true : false;
                        answerlist[2].AnswerContent = model.answerC;
                        answerlist[2].AnswerProperty = (model.answerRight == 3) ? true : false;
                        answerlist[3].AnswerContent = model.answerD;
                        answerlist[3].AnswerProperty = (model.answerRight == 4) ? true : false;



                        offlineDB.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch
                {
                    var item = offlineDB.TestQuestion.SingleOrDefault(m => m.ID == id);
                    if (item != null)
                    {
                        var answerlist = (from m in offlineDB.TestAnswer
                                          where m.TestQuestionId == item.ID
                                          select m).Take(4).ToArray();

                        string o_answerA = answerlist[0].AnswerContent;
                        string o_answerB = answerlist[1].AnswerContent;
                        string o_answerC = answerlist[2].AnswerContent;
                        string o_answerD = answerlist[3].AnswerContent;
                        int answerproperty = 1;
                        for (int i = 0; i < 4; i++)
                        {
                            if (answerlist[i].AnswerProperty)
                            {
                                answerproperty = i + 1;
                            }
                        }
                        model = new CreateQuestionViewModel()
                        {
                            TestTypeID = item.TestTypeID,
                            questionArea = item.QuestionContent,
                            answerA = o_answerA,
                            answerB = o_answerB,
                            answerC = o_answerC,
                            answerD = o_answerD,
                            answerRight = answerproperty
                        };
                        ViewBag.Store_System = new SelectList(offlineDB.TestType, "Id", "CustomName");
                        return View(model);
                    }
                    else
                    {
                        return View("Error");
                    }
                }

            }
            else
            {
                var item = offlineDB.TestQuestion.SingleOrDefault(m => m.ID == id);
                if (item != null)
                {
                    var answerlist = (from m in offlineDB.TestAnswer
                                      where m.TestQuestionId == item.ID
                                      select m).Take(4).ToArray();

                    string o_answerA = answerlist[0].AnswerContent;
                    string o_answerB = answerlist[1].AnswerContent;
                    string o_answerC = answerlist[2].AnswerContent;
                    string o_answerD = answerlist[3].AnswerContent;
                    int answerproperty = 1;
                    for (int i = 0; i < 4; i++)
                    {
                        if (answerlist[i].AnswerProperty)
                        {
                            answerproperty = i + 1;
                        }
                    }
                    model = new CreateQuestionViewModel()
                    {
                        TestTypeID = item.TestTypeID,
                        questionArea = item.QuestionContent,
                        answerA = o_answerA,
                        answerB = o_answerB,
                        answerC = o_answerC,
                        answerD = o_answerD,
                        answerRight = answerproperty
                    };
                    ViewBag.Store_System = new SelectList(offlineDB.TestType, "Id", "CustomName");
                    return View(model);
                }
                else
                {
                    return View("Error");
                }
            }
        }
        
        public ActionResult ExamResultList()
        {
            var exam = from m in offlineDB.Examination
                       orderby m.ID descending
                       select m;
            return View(exam);
        }

        public ActionResult CustomOrderList()
        {
            var list = from m in offlineDB.CustomOrder
                       where m.OrderStatus !=0
                       orderby m.Id descending
                       select m;
            return View(list);
        }

        public PartialViewResult AjaxCustomOrderList(string query)
        {
            if (query.Trim() == "")
            {
                var list = from m in offlineDB.CustomOrder
                           where m.OrderStatus != 0
                           orderby m.Id descending
                           select m;
                return PartialView(list);
            }
            else
            {
                var list = from m in offlineDB.CustomOrder
                           where (m.OrderStatus != 0 && (m.OrderNumber.Contains(query) || m.NickName.Contains(query.Trim())))
                           orderby m.Id descending
                           select m;
                return PartialView(list);
            }
        }

        public JsonResult AjaxComfirmedOrder(int id)
        {
            var order = offlineDB.CustomOrder.SingleOrDefault(m => m.Id == id);
            if (order != null)
            {
                order.OrderStatus = 2;
                offlineDB.SaveChanges();
                GPRSPrint.SendGprsPrintContent(GPRSPrint.TemplateCode(order.OrderNumber));
                //GPRSPrint.SendGprsPrintContent(GPRSPrint.TemplateCode(order.OrderNumber));
                return Json(new { result = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { result = "FAIL" }, JsonRequestBehavior.AllowGet);
        }

        /*public JsonResult TestResult()
        {
            var list = from m in offlineDB.TestPossible
                       select m;
            var singleDate = new DateTime(2017, 5, 28);
            var item = list.SingleOrDefault(m => m.Date == singleDate);
            //double[] predictvalues = new double[9];

            DateTime vardate31 = singleDate.AddDays(-31);
            DateTime vardate11 = singleDate.AddDays(-11);
            DateTime vardate10 = singleDate.AddDays(-10);
            DateTime vardate9 = singleDate.AddDays(-9);
            DateTime vardate8 = singleDate.AddDays(-8);
            DateTime vardate7 = singleDate.AddDays(-7);

            DateTime vardate6 = singleDate.AddDays(-6);
            DateTime vardate5 = singleDate.AddDays(-5);
            DateTime vardate4 = singleDate.AddDays(-4);

            DateTime vardate3 = singleDate.AddDays(-3);
            DateTime vardate2 = singleDate.AddDays(-2);

            double bestabs = 1;
            int[] bestpredict = new int[5];

            int count=0;
            for (int i = 10; i < 80; i += 5)
            {
                for (int j = 10; j < 80; j += 5)
                {
                    for (int k = 10; k < 80; k += 5)
                    {
                        for (int l = 10; l < 80; l += 5)
                        {
                            for (int n = 10; n < 80; n += 5)
                            {
                                int[] predictvalues = new int[5] { i, j, k, l, n };
                                var sum = (int)((list.Where(m => m.Date >= vardate31 && m.Date < vardate11).Sum(m => m.P1) * 0.95 +
                                    list.Where(m => m.Date >= vardate11 && m.Date < vardate9).Sum(m => m.P1) * predictvalues[0] +
                                    list.Where(m => m.Date >= vardate9 && m.Date < vardate7).Sum(m => m.P1) * predictvalues[1] +
                                    list.Where(m => m.Date >= vardate7 && m.Date < vardate5).Sum(m => m.P1) * predictvalues[2] +
                                    list.Where(m => m.Date >= vardate5 && m.Date < vardate3).Sum(m => m.P1) * predictvalues[3] +
                                    list.Where(m => m.Date >= vardate3 && m.Date < vardate2).Sum(m => m.P1) * predictvalues[4])/100);
                                var abs = Math.Abs(sum - item.R1??0);
                                if (abs < bestabs)
                                {
                                    bestabs = abs;
                                    bestpredict = predictvalues;
                                }

                            }
                        }
                        //p.Add(predictvalues);
                    }
                }
            }
            /var sum = (int)(list.Where(m => m.Date >= vardate31 && m.Date < vardate11).Sum(m => m.P1) * 0.95 +
                list.Where(m => m.Date >= vardate11 && m.Date < vardate10).Sum(m => m.P1) * predictvalues[0] +
                list.Where(m => m.Date >= vardate10 && m.Date < vardate9).Sum(m => m.P1) * predictvalues[1] +
                list.Where(m => m.Date >= vardate9 && m.Date < vardate8).Sum(m => m.P1) * predictvalues[2] +
                list.Where(m => m.Date >= vardate8 && m.Date < vardate7).Sum(m => m.P1) * predictvalues[3] +
                list.Where(m => m.Date >= vardate7 && m.Date < vardate6).Sum(m => m.P1) * predictvalues[4] +
                list.Where(m => m.Date >= vardate6 && m.Date < vardate5).Sum(m => m.P1) * predictvalues[5] +
                list.Where(m => m.Date >= vardate5 && m.Date < vardate4).Sum(m => m.P1) * predictvalues[6] +=0.1
                list.Where(m => m.Date >= vardate4 && m.Date < vardate3).Sum(m => m.P1) * predictvalues[7] +
                list.Where(m => m.Date >= vardate3 && m.Date < vardate2).Sum(m => m.P1) * predictvalues[8]);
                
            return Json(new { Abs = bestabs, Predict = bestpredict }, JsonRequestBehavior.AllowGet);
        }*/

        
    }
}