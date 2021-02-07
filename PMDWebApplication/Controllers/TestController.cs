using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using PMDWebApplication.Models;
using System.Web.Configuration;
using System.Configuration;
using System.Web.Security;

namespace PMDWebApplication.Controllers
{
    public class TestController : Controller
    {

        private ApplicationUserManager _userManager;
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
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(TestViewModel model)
        {
            MessageBox.Show(model.testInput);

            Configuration config = WebConfigurationManager.OpenWebConfiguration("/");
            try
            {
                //var myEnumValue = AppSettingsHelper.Get<string>("MyEnumValue");

                //int oldValue = AppSettingsHelper.Get<int>("Password_Attempts");
                //MessageBox.Show((oldValue + oldValue).ToString());

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            config.AppSettings.Settings["TESTKEY"].Value = model.testInput;
            //config.Save(ConfigurationSaveMode.Modified);
            config.Save(ConfigurationSaveMode.Minimal);
            return View();
        }


        public ActionResult Index2()
        {
            //MembershipUser membershipUser = Membership.GetUser();
            //TimeSpan ts = DateTime.Today - membershipUser.LastPasswordChangedDate;
            //MessageBox.Show(membershipUser.LastPasswordChangedDate.ToString());

            var user = UserManager.FindById(User.Identity.GetUserId());
            bool userAuthenticated = User.Identity.IsAuthenticated;
            TimeSpan ts = DateTime.Today - user.PasswordLastChangedDate;
            MessageBox.Show(user.PasswordLastChangedDate.ToString());
            MessageBox.Show(ts.ToString());
            return View();
        }

        [HttpPost]
        public ActionResult FormSubmit()
        {



           /* CountDownTimer timer = new CountDownTimer();


            //set to 30 mins
            timer.SetTime(30, 0);

            timer.Start();

            //update label text
            timer.TimeChanged += () => Label1.Text = timer.TimeLeftMsStr;

            // show messageBox on timer = 00:00.000
            timer.CountDownFinished += () => MessageBox.Show("Timer finished the work!");

            //timer step. By default is 1 second
            timer.StepMs = 33;*/

            /*   //Validate Google recaptcha here
               var response = Request["g-recaptcha-response"];
               string secretKey = "6LcUo8YUAAAAAK4SZ926WlfIhsuI0w4vw9f9RJid";
               var client = new WebClient();
               var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
               var obj = JObject.Parse(result);
               var status = (bool)obj.SelectToken("success");
               ViewBag.Message = status ? "Google reCaptcha validation success" : "Google reCaptcha validation failed";
               Console.WriteLine(status.ToString());
               //When you will post form for save data, you should check both the model validation and google recaptcha validation
               //EX.
               *//* if (ModelState.IsValid && status)
               {

               }*//*

               //Here I am returning to Index page for demo perpose, you can use your view here
               return View("Index");*/
                return View("Index");
        }

        public ActionResult myTest()
        {
            return View();
        }
    }
}