using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using PMDWebApplication.Models;


namespace PMDWebApplication
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            //return Task.FromResult(0);
            return Task.Factory.StartNew(() => {
                sendMail(message);
            });
        }

        void sendMail(IdentityMessage message)
        {
            #region formatter
            //Issue is that if you change ur password from to the same password as previously used, u cannot log in
            //Another issue is the message content, it will show this rather (which is Please confirm your ...) rather than Please reset your password
            //string text = string.Format("Please click on this link to {0}: {1}", message.Subject, message.Body);
            string text = string.Format("{0}: {1}", message.Subject, message.Body);

            //string html = "Please confirm your PMDWebApplication account by clicking this link: <a href=\"" + message.Body + "\">link</a><br/>";
            //string html = HttpUtility.HtmlEncode(@"Please confirm your PMDWebApplication account by clicking on this link : " + "<a href= " + message.Body + ">" + "Link" +"</a");

            string html = HttpUtility.HtmlEncode(@"Please confirm your Vervoer account by clicking on this link : " + message.Body);

            if (message.Subject == "Vervoer - Confirm Your Account") {

                text = HttpUtility.HtmlEncode(@"Please confirm your Vervoer account by clicking on this link : ");
                text += message.Body;
            }
                
            else if (message.Subject == "Vervoer - System Generated Password")
            {
                text = message.Body;
                
            }
            else if (message.Subject == "Vervoer - Reset Password")
            {
                //The reason why you need to text += message.Body is because htmlencode will encode the url, causing the code to be null
                //text = HttpUtility.HtmlEncode(@"Please reset your Vervoer account password by clicking on this link : " + message.Body);
                text = HttpUtility.HtmlEncode(@"Please reset your Vervoer account password by clicking on this link : ");
                text += message.Body;
            }

            //Hyperlink not working idk why
            //html += HttpUtility.HtmlEncode(@"Or click on the copy the following link on the browser:" + message.Body);
            #endregion

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(ConfigurationManager.AppSettings["Email"].ToString());
            msg.To.Add(new MailAddress(message.Destination));
            msg.Subject = message.Subject;
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32(587));
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["Email"].ToString(), ConfigurationManager.AppSettings["Password"].ToString());
            smtpClient.Credentials = credentials;
            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);
        }
    }


    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {

            // Plug in your SMS service here to send a text message. 


            /* var soapSms = new ASPSMSX2.ASPSMSX2SoapClient("ASPSMSX2Soap");
             soapSms.SendSimpleTextSMS(

                 ConfigurationManager.AppSettings["ASPSMSUSERKEY"],
                 ConfigurationManager.AppSettings["ASPSMSPASSWORD"],
                 message.Destination,
                 ConfigurationManager.AppSettings["ASPSMSORIGINATOR"],
                 message.Body);
             soapSms.Close();*/

            
            string apiKey = "TtrnfbH7I3s-NbYg4lWlDH6zLMpYOK81jwr1jCkJun";
            string sender = "Vervoer";
            String url = "https://api.txtlocal.com/send/?apikey=" + apiKey + "&numbers=" + message.Destination + "&message=" + message.Body + "&sender=" + sender;

            StreamWriter myWriter = null;
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);

            objRequest.Method = "POST";
            objRequest.ContentLength = Encoding.UTF8.GetByteCount(url);
            objRequest.ContentType = "application/x-www-form-urlencoded";
            try
            {
                myWriter = new StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(url);
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.ToString());
            }


                return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {

           // PasswordHasher = new CustomPasswordHasher();
        }

        //Self Declared to specifiy tokenlifespan
        //public IDataProtector Protector { get; set; }
        //public TimeSpan TokenLifespan { get; private set; }
        //

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var DBcontext = new ApplicationDbContext();

            //All of the configurations in the database
            int Password_Length = DBcontext.SecurityConfigurationsViewModels.Select(i => i.Password_Length).FirstOrDefault();
            bool RequireNonLetterOrDigit = DBcontext.SecurityConfigurationsViewModels.Select(i => i.RequireNonLetterOrDigit).FirstOrDefault();
            bool RequireDigit = DBcontext.SecurityConfigurationsViewModels.Select(i => i.RequireDigit).FirstOrDefault();
            bool RequireLowercase = DBcontext.SecurityConfigurationsViewModels.Select(i => i.RequireLowercase).FirstOrDefault();
            bool RequireUppercase = DBcontext.SecurityConfigurationsViewModels.Select(i => i.RequireUppercase).FirstOrDefault();
            int Password_Attempts = DBcontext.SecurityConfigurationsViewModels.Select(i => i.Password_Attempts).FirstOrDefault();
            int Lockout_Duration = DBcontext.SecurityConfigurationsViewModels.Select(i => i.LockoutDuration).FirstOrDefault();
            int Inactive_Duration = DBcontext.SecurityConfigurationsViewModels.Select(i => i.InactiveDuration).FirstOrDefault();


            /*       int Password_Length = AppSettingsHelper.Get<int>("PasswordLength");
                   bool RequireNonLetterOrDigit = AppSettingsHelper.Get<bool>("RequireNonLetterOrDigit");
                   bool RequireDigit = AppSettingsHelper.Get<bool>("RequireDigit");
                   bool RequireLowercase = AppSettingsHelper.Get<bool>("RequireLowercase");
                   bool RequireUppercase = AppSettingsHelper.Get<bool>("RequireUppercase");
                   int Password_Attempts = AppSettingsHelper.Get<int>("PasswordAttempts");
                   int Lockout_Duration = AppSettingsHelper.Get<int>("LockoutDuration");*/
            //int Inactive_Duration = AppSettingsHelper.Get<int>("Inactive_Duration");

            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {

            //SET ALL TO TRUE LATER , I ONLY DISABLED IT FOR DEMO PURPOSES
                RequiredLength = Password_Length,                       //10
                RequireNonLetterOrDigit = RequireNonLetterOrDigit,      //true
                RequireDigit = RequireDigit,                            //true
                RequireLowercase = RequireLowercase,                    //true
                RequireUppercase = RequireUppercase,                    //true
            };

            //https://stackoverflow.com/questions/45274190/how-to-change-userlockoutenabledbydefault-and-defaultaccountlockouttimespan-in-a
            //The above allow u to have a form for sysadmin to change these fields
            // Configure user lockout defaults

            //Great guide explaining these concepts - https://tech.trailmax.info/2014/06/asp-net-identity-user-lockout/
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(Lockout_Duration);  //15
            manager.MaxFailedAccessAttemptsBeforeLockout = Password_Attempts;                //5

            //It did not WORK
            //manager.TokenLifespan = TimeSpan.FromMinutes(2);
            //

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"+ Environment.NewLine + "It will expire in 3 minutes"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}" + Environment.NewLine + "It will expire in 3 minutes"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        //Declare 1 minute token lifespan (for 2FA)     
                        //TokenLifespan = TimeSpan.FromMinutes(1)
                        //TokenLifespan = TimeSpan.FromSeconds(10)
                        //Update : It doesn't work if i configure here, need to configure at startup file
                    };

            }
            return manager;
        }

    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
     
    }

    //Inherit role manager
    //Used to create a instance to role manager
    public class ApplicationRoleManager : RoleManager<ApplicationRole>
    {
        public ApplicationRoleManager(IRoleStore<ApplicationRole, string> roleStore) : base(roleStore) { }
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var applicationRoleManager = new ApplicationRoleManager(new RoleStore<ApplicationRole>(context.Get<ApplicationDbContext>()));
            return applicationRoleManager;

        }
    }
}
