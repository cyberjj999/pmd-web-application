using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Windows.Forms;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NLog;
using PMDWebApplication.Models;
using VirusTotalNet;
using VirusTotalNet.Objects;
using VirusTotalNet.ResponseCodes;
using VirusTotalNet.Results;

namespace PMDWebApplication.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
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

            var roles = UserManager.GetRoles(User.Identity.GetUserId());
            StringBuilder sb = new StringBuilder();
            foreach(string line in roles)
            {
                sb.AppendLine(line);
            }
            TempData["Roles"] = sb;

            var fileContents = System.IO.File.ReadAllText(Server.MapPath(@"~/logs/app-log.txt"));
            string[] log = fileContents.Split(new[] { "|===" }, StringSplitOptions.None);

            var listOfLogs = new List<LoggingViewModel>();
            var currentUsername = User.Identity.GetUserName();
            for (int i = 1; i < log.Length; i++)
            {
                //Base on the test string above, this logInfo will have 6 elements
                //[0] - Date Time  [1] - Type  [2] - Controller
                //[3] - Message    [4] - Roles [5] Username
                string[] logInfo = log[i-1].Split(new[] { "|" }, StringSplitOptions.None);
                if (logInfo[5] == currentUsername)
                {
                    var l = new LoggingViewModel
                    {
                        Id = i,
                        Date = DateTime.Parse(logInfo[0]).ToString(),
                        Type = logInfo[1],
                        ControllerName = logInfo[2],
                        Message = logInfo[3],
                        Roles = logInfo[4],
                        Username = logInfo[5]
                    };
                    listOfLogs.Add(l);
                }

            }

            var RecentActivities = new List<string>();

            var LogDates = new List<string>();
            var LogDetails = new List<string>();

            foreach (LoggingViewModel model in listOfLogs)
            {
                LogDates.Add(model.Date);
                LogDetails.Add(model.Message);
                RecentActivities.Add(model.Date + " | " + model.Message);
                //sb2.Append(model.Date);
                //sb2.Append(model.Message);
            }
            //MessageBox.Show(sb2.ToString());
            //ViewBag.RecentActivities = sb2;
            ViewBag.RecentLogDates = LogDates;
            ViewBag.RecentLogDetails = LogDetails;
            ViewBag.RecentActivities = RecentActivities;

            return View();

            }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
          ManageMessageId? message;
          var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
          if (result.Succeeded)
          {
              var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
              if (user != null)
              {
                  await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
              }
              message = ManageMessageId.RemoveLoginSuccess;
          }
          else
          {
              message = ManageMessageId.Error;
          }
          return RedirectToAction("ManageLogins", new { Message = message });
        }

        //Make method ASYNC
        [HttpPost]
        public ActionResult SetProfilePicture(IndexViewModel model, HttpPostedFileBase file)
        {

            //Need create a model then pass to the view, so your view can access
            if (ModelState.IsValid)
            {

                // TODO: Add code here
                //Second check //This will check if the file is empty
                if (file != null && file.ContentLength > 0)
                {
                    try
                    {
                            //https://www.virustotal.com/vtapi/v2/file/scan
                            /*VirusTotal virusTotal = new VirusTotal(ConfigurationManager.AppSettings["VIRUSTOTALAPIKEY"]);

                            //Use HTTPS instead of HTTP
                            virusTotal.UseTLS = true;
                            string filename2 = Path.GetFileName(file.FileName);
                            string path2 = Path.Combine(Server.MapPath("~/User-Images/"), filename2);

                            FileInfo fi = new FileInfo(path2);
                            ScanResult sc = await virusTotal.ScanFileAsync(fi);
                            MessageBox.Show(sc.ResponseCode.ToString());
*/                        //Create the EICAR test virus. See http://www.eicar.org/86-0-Intended-use.html
                        //byte[] eicar = Encoding.ASCII.GetBytes(@"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");

                        //Check if the file has been scanned before.
                        //FileReport report = await virusTotal.GetFileReportAsync(eicar);
                        

                        //MessageBox.Show("Seen before: " + (report.ResponseCode == FileReportResponseCode.Present ? "Yes" : "No"));

                        var allowedExtensions = new[] { ".jpeg", ".png", ".jpg" };
                        var checkextension = Path.GetExtension(file.FileName).ToLower();

                        //Check for .jpg and .png file
                        if (!allowedExtensions.Contains(checkextension))
                        {

                            TempData["ErrorCode"] = 1;
                            TempData["FileErrorMessage"] = "Please select .jpg or .png file only!";
                            return RedirectToAction("Index");

                        }

                        if (!Extension.IsImage(file.InputStream))
                        {
                            TempData["ErrorCode"] = 1;
                            TempData["FileErrorMessage"] = "Please select a VALID .jpg or .png file only!";
                            return RedirectToAction("Index");

                        }

                        else
                        {



                            string filename = Path.GetFileName(file.FileName);

                            string path = Path.Combine(Server.MapPath("~/User-Images/"), filename);
                            model.Uimg = path;
                            file.SaveAs(path);  

                            //Virus Scan
             /*               VirusTotal virusTotal = new VirusTotal(ConfigurationManager.AppSettings["VIRUSTOTALAPIKEY"]);

                            //Use HTTPS instead of HTTP
                            virusTotal.UseTLS = true;
                            FileInfo fi = new FileInfo(path);
                            ScanResult sc = await virusTotal.ScanFileAsync(fi);
                            MessageBox.Show(sc.ResponseCode.ToString());
*/
                            //Get the current user's ID
                            string uID = User.Identity.GetUserId();

                            //Parameterized Query
                            string mainconn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                            SqlConnection sqlconn = new SqlConnection(mainconn);
                            //string sqlquery = "UPDATE [dbo].[AspNetUsers] SET Uimg = '" + "/User-Images/" + file.FileName + "' WHERE Id='" + uID + "'";
                            string sqlquery = "UPDATE [dbo].[AspNetUsers] SET Uimg=@Uimg WHERE Id=@uID";
                            SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
                            sqlconn.Open();

                            sqlcomm.Parameters.AddWithValue("@Uimg", "~/User-Images/" + file.FileName);
                            sqlcomm.Parameters.AddWithValue("@uID", uID);

                            sqlcomm.ExecuteNonQuery();
                            sqlconn.Close();

                            List<string> roles = new List<string>(new string[] { });
                            roles = UserManager.GetRoles(uID).ToList();
                            var listOfRoles = string.Join(",", roles);
                            var username = User.Identity.GetUserName();
                            logger.Info("User changed profile picture" + "|" + listOfRoles + "|" + username + "|===");

                            TempData["Message"] = "Profile Picture is updated successfully!";
                        }

                    }

                    catch (Exception ex)
                    {
                        //ViewBag.Message and ViewData["Message"] do not work as you redirect, need use tempData
                        TempData["ErrorCode"] = 1;
                        TempData["ErrorMessage"] = ex.Message.ToString();
                        TempData["FileErrorMessage"] = ex.ToString();
                    }
                }

                else
                {
                    TempData["ErrorCode"] = 1;
                    TempData["FileErrorMessage"] = "You have not specified a file!";

                }

            }

            string MyUID = User.Identity.GetUserId();
            //_userManager
            ApplicationUser user = UserManager.FindById(MyUID);
            //return View(user);
            //return View("Index", model);
            //modelstate.clear()
            return RedirectToAction("Index");
            //When i redirectToAction("Index","Manage",user), alot of details are shown in the URL
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your phone verification code is: " + code + Environment.NewLine + "It will expire in 3 minutes"
                    // "Your security code is {0}" + Environment.NewLine + "It will expire in 3 minutes"
                };
                await UserManager.SmsService.SendAsync(message);
                //var sendMessage = UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                //Message to show action done
                TempData["Message"] = "Two-Factor Authentication is now enabled!";
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                //Message
                TempData["ErrorMessage"] = "Two-Factor Authentication is now disabled!";
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                TempData["Message"] = "Your phone number was added successfully!";
                //return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
                return RedirectToAction("Index");

            }
            // If we got this far, something failed, redisplay form

            //Potentially lock the user out if they try to spam their 2FA for a random phone
            /* var user2 = await UserManager.FindByIdAsync(User.Identity.GetUserId());

             //Increase AccessFailCount if he enter the code wrongly
             user2.AccessFailedCount++;
             var updateResult = await UserManager.UpdateAsync(user2);
             if (updateResult.Succeeded)
             {
                 if(user2.AccessFailedCount == 5)
                 {
                     user2.AccessFailedCount = 0;
                     await UserManager.UpdateAsync(user2);
                     return View("Lockout");
                 }
             }*/

            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        /*        [HttpPost]
                [ValidateAntiForgeryToken]
                public async Task<ActionResult> RemovePhoneNumber()
                {
                    var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
                    if (!result.Succeeded)
                    {
                        return RedirectToAction("Index", new { Message = ManageMessageId.Error });
                    }
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }

                    TempData["Message"] = "Your phone number was removed successfully!";
                    //return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
                    return RedirectToAction("Index");

                }*/

        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }

            TempData["ErrorMessage"] = "Your phone number was removed successfully!";
            //return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
            return RedirectToAction("Index");
        }

        //
        // GET: /Manage/ChangePassword
        [ExcludeFilter(typeof(PasswordAgeGlobalFilter))]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExcludeFilter(typeof(PasswordAgeGlobalFilter))]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (model.OldPassword == model.NewPassword)
            {
                //MessageBox.Show("Old and new pw cannot be the same");
                ModelState.AddModelError("", "Old password cannot be the same as the new password!");
                return View();
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
          
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }

                user.PasswordLastChangedDate = DateTime.Now;
                user.PasswordGeneratedDate = null;             
                UserManager.Update(user);

                TempData["Message"] = "Your password has been changed successfully!";
                //return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
                var currentUserID = User.Identity.GetUserId();
                var currentUserName = User.Identity.GetUserName();

                List<string> roles = UserManager.GetRoles(currentUserID).ToList();
                var listOfRoles = string.Join(",", roles);

                logger.Info("User changed his password." + "|" + listOfRoles + "|" + currentUserName + "|===");

                return RedirectToAction("Index");
                //return View("Index", model);

            }
            AddErrors(result);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExcludeFilter(typeof(PasswordAgeGlobalFilter))]
        public async Task<ActionResult> ChangePasswordPartial(ChangePasswordViewModel model)
        {
            var fileContents = System.IO.File.ReadAllText(Server.MapPath(@"~/logs/test.txt"));
            string[] log = fileContents.Split(new[] { "|===" }, StringSplitOptions.None);
            var listOfLogs = new List<LoggingViewModel>();
            for (int i = 1; i < log.Length; i++)
            {
                //Base on the test string above, this logInfo will have 6 elements
                //[0] - Date Time  [1] - Type  [2] - Controller
                //[3] - Message    [4] - Roles [5] Username
                string[] logInfo = log[i - 1].Split(new[] { "|" }, StringSplitOptions.None);
                if (logInfo[5] == User.Identity.GetUserName())
                {
                    var l = new LoggingViewModel
                    {
                        Id = i,
                        Date = DateTime.Parse(logInfo[0]).ToString(),
                        Type = logInfo[1],
                        ControllerName = logInfo[2],
                        Message = logInfo[3],
                        Roles = logInfo[4],
                        Username = logInfo[5]
                    };
                    listOfLogs.Add(l);

                }
            }

            var RecentActivities = new List<string>();

            foreach (LoggingViewModel myModel in listOfLogs)
            {
                RecentActivities.Add(myModel.Date + " | " + myModel.Message);

            }
           
            ViewBag.RecentActivities = RecentActivities;

            if (!ModelState.IsValid)
            {
                TempData["ErrorCode"] = 2;
                ModelState.AddModelError("", "Invalid Modele");
                return View("Index", model);
            }
            if (model.OldPassword == model.NewPassword)
            {
                //MessageBox.Show("Old and new pw cannot be the same");
                ModelState.AddModelError("", "Old password cannot be the same as the new password!");
                TempData["PasswordErrorMessage"] = "Old password cannot be the same as the new password!";
                TempData["ErrorCode"] = 2;

                return RedirectToAction("Index");
                //return View("Index");
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }

                user.PasswordLastChangedDate = DateTime.Now;
                user.PasswordGeneratedDate = null;
                UserManager.Update(user);

                TempData["Message"] = "Your password has been changed successfully!";
                //return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
                //return View("Index",model);
                return View("Index");

            }
            AddErrors(result);
            //Will force the users to go to change password tab
            TempData["ErrorCode"] = 2;

            StringBuilder sb = new StringBuilder();
            foreach(string line in result.Errors)
            {
                sb.Append(line);
            }
            TempData["PasswordErrorMessage"] = sb;


            //return View("Index",model);
            return RedirectToAction("Index");

        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
                //MessageBox.Show(error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            //Custom message
            //UpdateProfilePicSuccess,
            Error
        }
        #endregion
   }
}