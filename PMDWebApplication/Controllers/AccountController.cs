using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using PMDWebApplication.Models;
using NLog;
using System.Text;
using Microsoft.AspNet.Identity.EntityFramework;

namespace PMDWebApplication.Controllers
{

    [ExcludeFilter(typeof(PasswordAgeGlobalFilter))]
    public class AccountController : Controller
    {
        //Declaring a logging object
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
      
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost] 
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {

            //ModelState.IsValid checks whether the data you are binding to the model is valid or not
            //E.G if the model has int age in his data, if you put string age then of course will have error thus not valid
            if (!ModelState.IsValid)
            {
                //MessageBox.Show("Model is invalid!");
                return View(model);
            }

            var isUsernameExist = UserManager.Users.Any(i => i.UserName == model.UserName);

            string userId = "";
            ApplicationUser user = new ApplicationUser();
            if (isUsernameExist)
            {
                userId = UserManager.Users.Where(i => i.UserName == model.UserName).Select(i => i.Id).SingleOrDefault();
                user = UserManager.FindById(userId);

            }

            //To reset every "interval", you can have a datetime field that marks when they last failed.
            //IF the particular last failed datetime is 1 day from the current time that they are logging in, the count will be reset
            if (user.AccessFailDate != null)
            {
                //MessageBox.Show("Not Null");
                DateTime accessFailDate = user.AccessFailDate.GetValueOrDefault();
                DateTime resetAccessFailDate = accessFailDate.AddMinutes(2);
                //MessageBox.Show(resetAccessFailDate.ToString() + " is the access fail date date");

                //If you fail at 10pm, we reset in 2minutes, resetAccessFailDate will be 10.02pm
                //If 10.02pm is greater than current time, we will reset your accessFailCount
                if (DateTime.Now > resetAccessFailDate)
                {
                    UserManager.ResetAccessFailedCount(userId);
                    //MessageBox.Show("I AM RESETTING YOUR ACCESS FAIL COUNT");

                }
            }

            if (user.PasswordGeneratedDate != null)
            {
                //I.E this is 10pm
                DateTime PasswordGeneratedDatetime = user.PasswordGeneratedDate.GetValueOrDefault();
                //This will be 10.02pm
                DateTime SysGeneratedPasswordExpiredDT = PasswordGeneratedDatetime.AddMinutes(2);
                //If currently is 10.03pm, which is past 10.02pm, they should not be able to log in anymore
                if (DateTime.Now > SysGeneratedPasswordExpiredDT)
                {
                    MessageBox.Show("Your system generated password has expired. Please contact the system administrator to" +
                        " generate a new password");
                    return View("Login");

                }
            }
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, false, shouldLockout: true);
           
            List<string> roles = new List<string>(new string[] { });

            switch (result) 
            {
                //If username and password is correct
                case SignInStatus.Success:
                    //Retrieve the user's id and reset the accessFailedCount
                    //var userId = UserManager.Users.Where(i => i.UserName == model.UserName).Select(i => i.Id).SingleOrDefault();
                    //Reset accessFailedCount upon log in
                    await UserManager.ResetAccessFailedCountAsync(userId);                 
                    //logger.Info("Successful Login for " + "|" + userRole + "|" + model.UserName);
                    if(UserManager.GetRoles(userId).Contains("System Administrator") || UserManager.GetRoles(userId).Contains("Admin"))
                    {
                      
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    return RedirectToAction("Index", "Home");
                //return RedirectToLocal(returnUrl);                   
                case SignInStatus.LockedOut:
                    //Logging
                    if (isUsernameExist)
                    {

                        roles = UserManager.GetRoles(userId).ToList();
                        var listOfRoles = string.Join(",", roles);
                        logger.Warn("User's account locked out" + "|" + listOfRoles + "|" + model.UserName + "|===");

                    }
                    return View("Lockout");
                case SignInStatus.RequiresVerification:   
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false});
                case SignInStatus.Failure:
                    ViewBag.Msg = "LoginFailed";
                    ModelState.AddModelError("", "Invalid Credentials!");
                    //Logging, Logs every single invalid login attempt only if the username exists
                    if (isUsernameExist)
                    {

                        user.AccessFailDate = DateTime.Now;
                        UserManager.Update(user);

                        roles = UserManager.GetRoles(userId).ToList();
                        var listOfRoles = string.Join(",", roles);
                        logger.Warn("Invalid Login Attempt" + "|" + listOfRoles + "|" + model.UserName + "|===");

                    }
                    return View("Login");
                    //return RedirectToAction("Index","Home",model);
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            //I removed isPersistent: model.RememberMe
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                    
                    //Logging
                    //Cannot cause at this point the user havent login yet, so udk which user it is
             /*       var currentUserID = User.Identity.GetUserId();
                    var currentUserName = User.Identity.GetUserName();
                    List<string> roles = UserManager.GetRoles(currentUserID).ToList();
                    var listOfRoles = string.Join(",", roles);
                    logger.Warn("Enter Invalid 2FA Code" + "|" + listOfRoles + "|" + currentUserName + "|===");*/
                    ModelState.AddModelError("", "Sign in failure!");
                    return View(model);
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            //Get roles from role table, then add to list of selectListItem
            /*   List<SelectListItem> list = new List<SelectListItem>();
                   foreach(var role in RoleManager.Roles)
                   list.Add(new SelectListItem() { Value = role.Name, Text = role.Name });
                   //Use viewbag to store list of SelectListItem
                   //This allows you to dynamically share value from the controller to the view
                   //In thew view, you retrieve those values by using name name for the property
                   ViewBag.Roles = list;*/

            if (User.Identity.IsAuthenticated)
            {
                return View("Index", "Home");
            }

            return View();
        }

        //
        // GET: /Account/Register
     /*   [AllowAnonymous]
        public ActionResult EventRegister()
        {
            return View();
        }  */

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            //Validate Google recaptcha here
            var response = Request["g-recaptcha-response"];
            //string secretKey = ConfigurationManager.AppSettings["RECAPTCHASECRETKEY"];
            string secretKey = "6LcUo8YUAAAAAK4SZ926WlfIhsuI0w4vw9f9RJid";

            //"6LcUo8YUAAAAAK4SZ926WlfIhsuI0w4vw9f9RJid";
            var client = new WebClient();
            var ReCaptchaResult = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
            var obj = JObject.Parse(ReCaptchaResult);
            var status = (bool)obj.SelectToken("success");
            ViewBag.Message = status ? "Google reCaptcha validation success" : "Google reCaptcha validation failed";
            //MessageBox.Show(status.ToString());

            //MessageBox.Show("My message here");
            if (ModelState.IsValid && status)
            {

                /*                PasswordHasher = new CustomPasswordHasher();
                */
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FullName = model.FullName, Uimg = "/User-Images/default.png", AccessFailDate = null, Age = model.Age ,PasswordLastChangedDate = DateTime.Now };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    result = await UserManager.AddToRoleAsync(user.Id, "User");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);


                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    //var message = HttpUtility.HtmlEncode("Please confirm your account by clicking <a href=" + callbackUrl + ">here</a>");
                    await UserManager.SendEmailAsync(user.Id, "Vervoer - Confirm Your Account", callbackUrl);

                    //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=" + callbackUrl + ">here</a>");



                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            else if (status == false) 
            {
                ModelState.AddModelError("Model", "Please complete the recaptcha!");
            }

            // If we got this far, something failed, redisplay form
            
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {

                var roles = UserManager.GetRoles(userId).ToList();
                var listOfRoles = string.Join(",", roles);
                var user = UserManager.FindById(userId);
                var username = user.UserName;
                logger.Info("Email Confirmed" + "|" + listOfRoles + "|" + username + "|===");

            }
            return View(result.Succeeded ? "ConfirmEmail" : "Error");

        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                //If user don't exist and hanve't confirm email, return the view
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                //else you send the email to them
                else {
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link

                    //Logging
                    var currentUserID = user.Id;
                    var currentUserName = user.UserName;

                    List<string> roles = UserManager.GetRoles(currentUserID).ToList();
                    var listOfRoles = string.Join(",", roles);
                    logger.Info("User requested for 'forget password'." + "|" + listOfRoles + "|" + currentUserName + "|===");


                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
          
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Vervoer - Reset Password", callbackUrl);
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
              
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FullName = model.FullName, Uimg = "/User-Images/default.png", AccessFailDate = null, Age = model.Age, PasswordLastChangedDate = DateTime.Now };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "User");
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
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
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        //This is to return a view to let user know they are kicked out due to inactivity
        [AllowAnonymous]
        public ActionResult Inactive()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PrefilledRegister(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                //If user don't exist and hanve't confirm email, return the view
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                //else you send the email to them
                else
                {
                    
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Vervoer - Reset Password", callbackUrl);
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

    }
}