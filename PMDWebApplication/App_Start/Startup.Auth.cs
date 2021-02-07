using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;
using Owin;
using PMDWebApplication.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace PMDWebApplication
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            var DBcontext = new ApplicationDbContext();


            //All of the configurations in the database
            int Inactive_Duration = DBcontext.SecurityConfigurationsViewModels.Select(i => i.InactiveDuration).FirstOrDefault();
            //int Inactive_Duration = AppSettingsHelper.Get<int>("InactiveDuration");

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                //Kick user out after being inactive for 15 minutes
                //Inactive means no interaction with any buttons, even if u hover its considered as inactive
                ExpireTimeSpan = TimeSpan.FromMinutes(Inactive_Duration),           //15
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))


                }
            }); 
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            //app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
            //The 2FA will only be valid for 3 minutes, after it has passed, the code becomes invalid
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(3));

        

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");


            /*  app.UseTwitterAuthentication(new TwitterAuthenticationOptions
              {
                  ConsumerKey = "6Synsfnoi8K7V0dx6w4o1etiW",
                  ConsumerSecret = "HATrSz62yn7saFlde2Ni6mpkD2LXgt7ZA6Zy4rbDywCZASJWWS",
                  BackchannelCertificateValidator = new CertificateSubjectKeyIdentifierValidator(new[]
     {
          "A5EF0B11CEC04103A34A659048B21CE0572D7D47", // VeriSign Class 3 Secure Server CA - G2
          "0D445C165344C1827E1D20AB25F40163D8BE79A5", // VeriSign Class 3 Secure Server CA - G3
          "7FD365A7C2DDECBBF03009F34339FA02AF333133", // VeriSign Class 3 Public Primary Certification Authority - G5
          "39A55D933676616E73A761DFA16A7E59CDE66FAD", // Symantec Class 3 Secure Server CA - G4
          "5168FF90AF0207753CCCD9656462A212B859723B", //DigiCert SHA2 High Assurance Server C‎A 
          "B13EC36903F8BF4701D498261A0802EF63642BC3" //DigiCert High Assurance EV Root CA
      })
              });*/


            app.UseFacebookAuthentication(
               appId: "525127721383100",
               appSecret: "32fd549b3ec87da952ef63d857c6d2ca");

            /* app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
             {
                 ClientId = "1086675589154-bv4i2363d4vbskilgmaigbi0oe9qhni7.apps.googleusercontent.com",
                 ClientSecret = "vPvvYav7oa9xYDItd9LlR_-G-"
             });
 */
            //
            //
}

    }

}