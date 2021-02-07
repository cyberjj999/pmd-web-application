using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PMDWebApplication.Controllers;
using PMDWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;


namespace PMDWebApplication
{
    //SUPER USEFUL LINK : https://www.leniel.net/2012/05/user-password-expired-filter-aspnet-mvc.html
    public class PasswordAgeGlobalFilter : AuthorizeAttribute
    {

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            }
            private set
            {
                _userManager = value;
            }
        }


        /*        private static readonly int PasswordExpiresInDays =
                    int.Parse(ConfigurationManager.AppSettings["PasswordExpiresInDays"]);*/

        //private static readonly int PasswordExpiresInDays = AppSettingsHelper.Get<int>("PasswordExpiry");

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var DBcontext = new ApplicationDbContext();

            //All of the configurations in the database
            int PasswordExpiresInDays = DBcontext.SecurityConfigurationsViewModels.Select(i => i.PasswordExpiry).FirstOrDefault();
            //IPrincipal user = filterContext.HttpContext.User;
            var user = UserManager.FindById(HttpContext.Current.User.Identity.GetUserId());
            bool userAuthenticated = HttpContext.Current.User.Identity.IsAuthenticated;

            if (user != null && userAuthenticated)
            //if (user != null && user.Identity.IsAuthenticated)
            {
                //MembershipUser membershipUser = Membership.GetUser();

                //TimeSpan ts = DateTime.Today - membershipUser.LastPasswordChangedDate;
                TimeSpan ts = DateTime.Today - user.PasswordLastChangedDate;
                // If true, that means the user's password expired
                // Let's force him to change his password before using the application
                if (ts.TotalDays > PasswordExpiresInDays)
                {

                    //string code = UserManager.GeneratePasswordResetToken(user.Id);
                    //string codeMessage = "code=" + code;
                    if (user.PasswordLastChangedDate == new DateTime(1999, 1, 1))
                    {
                        MessageBox.Show("You must change your password before logging on for the first time!");

                    }
                    else if (user.PasswordLastChangedDate == new DateTime(2000, 2, 2))
                    {
                        MessageBox.Show("The System Administrator has triggered a password reset, please change your password.");

                    }
                    else
                    {
                        MessageBox.Show("Your password has expired! Please change your password now!");
                    }
                    filterContext.HttpContext.Response.Redirect(
             /*       string.Format("~/{0}/{1}?{2}", "Account", "ResetPassword",
                    "reason=expired"));*/
                    
                    string.Format("~/{0}/{1}", "Manage", "ChangePassword"));
                    /*filterContext.HttpContext.Response.Redirect(
                        string.Format("~/{0}/{1}?{2}", MVC.Account.Name, MVC.Account.ActionNames.ChangePassword,
                        "reason=expired"));
*/
                }
            }

            base.OnAuthorization(filterContext);
        }
    }
}