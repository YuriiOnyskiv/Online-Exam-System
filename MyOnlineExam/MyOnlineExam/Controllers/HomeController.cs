using MyOnlineExam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyOnlineExam.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var _ctx = new MyQuizNewEntities();
            ViewBag.Test = _ctx.Test.Where(x => x.IsActive == true).Select(x => new { x.Id, x.Name }).ToList();

            SessionModel _model = null;

            if (Session["SessionModel"] == null)
                _model = new SessionModel();
            else
                _model = (SessionModel)Session["SessionModel"];
            return View(_model);
        }


        public ActionResult Instruction(SessionModel model)
        {
            if(model != null)
            {
                var _ctx = new MyQuizNewEntities();
                var test = _ctx.Test.Where(x => x.IsActive == true && x.Id == model.TestId).FirstOrDefault();
                if(test != null)
                {
                    ViewBag.TestName = test.Name;
                    ViewBag.TestDescription = test.Description;
                    ViewBag.QuestionCount = test.TestXQuestion.Count;
                    ViewBag.TestDuration = test.DurationInMinute;
                }
            }
            return View(model);

        }


        public ActionResult Register(SessionModel model)
        {
            if(model!= null)
            Session["SessionModel"] = model;

            if(model == null || string.IsNullOrEmpty(model.UserName) || model.TestId < 1)
            {
                TempData["message"] = "Invalid Registration details. Please try again";
                return RedirectToAction("Index");
            }
            var _ctx = new MyQuizNewEntities();
            //to register the user to the system 
            // to register the user for test 
            Student _user = _ctx.Student.Where(x => x.Name.Equals(model.UserName, StringComparison.InvariantCultureIgnoreCase)
            && ((string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(x.Email)) || (x.Email == model.Email))
            && ((string.IsNullOrEmpty(model.Phone) && string.IsNullOrEmpty(x.Phone)) || (x.Phone == model.Phone))).FirstOrDefault();
          
         

            if (_user == null)
            {
                _user = new Student()
                {
                    Name = model.UserName,
                    Email = model.Email,
                    Phone = model.Phone,
                    EntryDate = DateTime.UtcNow,
                    AccessLevel = 100
                };
                _ctx.Student.Add(_user);
                _ctx.SaveChanges();
            }

            Registration registration = _ctx.Registration.Where(x => x.StudentId == _user.Id
            && x.TestId == model.TestId
            && x.TokenExpireTime > DateTime.UtcNow).FirstOrDefault();
            if(registration!= null)
            {
                this.Session["TOKEN"] = registration.Token;
                this.Session["TOKENEXPIRE"] = registration.TokenExpireTime;
            }
            else
            {
                Test test = _ctx.Test.Where(x => x.IsActive && x.Id == model.TestId).FirstOrDefault();
                if(test!= null)
                {
                    Registration newRegistration = new Registration()
                    {
                        RegistrationDate = DateTime.UtcNow,
                        TestId = model.TestId,
                        Token = Guid.NewGuid(),
                        TokenExpireTime = DateTime.UtcNow.AddMinutes(test.DurationInMinute)
                    };
                    _user.Registration.Add(newRegistration);
                    _ctx.Registration.Add(newRegistration);
                    _ctx.SaveChanges();
                    this.Session["TOKEN"] = newRegistration.Token;
                    this.Session["TOKENEXPIRE"] = newRegistration.TokenExpireTime;
                }
            }

            return View("EvalPage", new { @token = Session["TOKEN"] });
        }


        public ActionResult EvalPage(Guid token, int? qno)// qno = question number
        {
            if(token == null)
            {
                TempData["message"] = "You have an invalid token. Please re-register and try again";
                return RedirectToAction("Index");
            }
            var _ctx = new MyQuizNewEntities();

            //varify the user is registered and is allow to check the questions
            var registration = _ctx.Registration.Where(x => x.Token.Equals(token)).FirstOrDefault();
            if(registration == null)
            {
                TempData["message"] = "This token is invalid";
                return RedirectToAction("Index");
            }
            if (registration.TokenExpireTime < DateTime.UtcNow)
            {
                TempData["message"] = "The exam duretion has expired at " + registration.TokenExpireTime.ToString();
                return RedirectToAction("Index");
            }

            if (qno.GetValueOrDefault() < 1)
                qno = 1;

            var testQuestionId = _ctx.TestXQuestion.Where(x => x.TestId == registration.TestId && x.QuestionNumber == qno).Select(x => x.Id).FirstOrDefault();

            if(testQuestionId > 0)
            {
                var _model = _ctx.TestXQuestion.Where(x => x.Id == testQuestionId).Select(x => new QuestionModel()
                {
                    QuestionType = x.Question.QuestionType,
                    QuestionNumber = x.QuestionNumber,
                    Question = x.Question.Question1,
                    Point = x.Question.Points,
                    TestId = x.TestId,
                    TestName = x.Test.Name,
                    Options = x.Question.Choice.Where(y => y.IsActive == true).Select(y => new QXModel()
                    {
                        ChoiceId = y.Id,
                        Label = y.Label,
                    }).ToList()
                }).FirstOrDefault();

                return View(_model);
            }
            else
            {
                return View("Error");
            }
            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}