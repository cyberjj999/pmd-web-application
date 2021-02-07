using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using PMDWebApplication.Models;
using System.Data;
using System.Net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NLog;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using System.Windows.Forms;
using PMDWebApplication.Repository;
using PMDWebApplication.DB;

namespace PMDWebApplication.Controllers
{
    [Authorize(Roles = "System Administrator")]
    public class SystemAdminController : Controller
    {
        private ApplicationRoleManager _roleManager;
        private ApplicationUserManager _userManager;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        public dbVervoerEntities db = new dbVervoerEntities();
        public SystemAdminController()
        {
        }

        public SystemAdminController(ApplicationRoleManager roleManager)
        {
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

        // Role Management
        public ActionResult RoleIndex()
        {
            List<RoleViewModel> list = new List<RoleViewModel>();
            foreach (var role in RoleManager.Roles)
                list.Add(new RoleViewModel(role));
            return View(list);
        }

        public ActionResult RoleCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> RoleCreate(RoleViewModel model)
        {

            if (model.Name == null)
            {
                //ModelState.AddModelError("", "Please enter the role name");
                //TempData["Error"] = "Error! Please enter the role name";
                ModelState.AddModelError("", "The role name field is required");
                return View("RoleCreate");
            }
            if (await RoleManager.RoleExistsAsync(model.Name))
            {
                //TempData["Error"] = "Error! Role already exists!";
                //ModelState.AddModelError("", "Please complete the recaptcha!");
                ModelState.AddModelError("", "Role already exists");
                //MessageBox.Show("Error, Role Existed");

                //****Redirect to action will clear model state, so use return view
                return View("RoleCreate");
            }
            else
            {
                var role = new ApplicationRole() { Name = model.Name };
                await RoleManager.CreateAsync(role);
                TempData["GreenMessage"] = "The role " + "\"" + role.Name + "\"" + " has been created successfully!";
                return RedirectToAction("RoleIndex");

            }

            //var role = new ApplicationRole() { Name = model.Name };
            //await RoleManager.CreateAsync(role);


        }

        public async Task<ActionResult> RoleEdit(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            return View(new RoleViewModel(role));
        }

        [HttpPost]
        public async Task<ActionResult> RoleEdit(RoleViewModel model)
        {
            var role = new ApplicationRole() { Id = model.Id, Name = model.Name };
            bool roleExist = await RoleManager.RoleExistsAsync(model.Name);
            if (roleExist)
            {
                TempData["Error"] = "Error! Role already exists!";
                //ModelState.AddModelError("", "Please complete the recaptcha!");
                //ModelState.AddModelError("", "Role already exists");
                //MessageBox.Show("Error, Role Existed");
                return RedirectToAction("RoleEdit");
            }

            else
            {
                TempData["GreenMessage"] = "The role name has been updated to " + "\"" + model.Name + "\"" + " successfully!";
                await RoleManager.UpdateAsync(role);
                return RedirectToAction("RoleIndex");
            }
        }

        public async Task<ActionResult> RoleDetails(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            return View(new RoleViewModel(role));
        }

        public async Task<ActionResult> RoleDelete(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            return View(new RoleViewModel(role));
        }


        [HttpPost, ActionName("RoleDelete")]
        public async Task<ActionResult> RoleDeleteConfirmed(string id)
        {

            var role = await RoleManager.FindByIdAsync(id);
            await RoleManager.DeleteAsync(role);
            TempData["RedMessage"] = "The role " + "\"" + role.Name + "\"" + " has been deleted successfully!";
            return RedirectToAction("RoleIndex");
        }

        //User Management
        public ActionResult GetUsers()
        {
            /*        var userRoles = new List<RoleViewModel>();
                    var context = new ApplicationDbContext();
                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);

                    //Get all the usernames
                    foreach (var user in userStore.Users)
                    {
                        var r = new RoleViewModel
                        {
                            UserName = user.UserName
                        };
                        userRoles.Add(r);
                    }
                    //Get all the Roles for our users
                    foreach (var user in userRoles)
                    {
                        user.RoleNames = userManager.GetRoles(userStore.Users.First(s => s.UserName == user.UserName).Id);
                    }*/

            //userRoles
            return View();



        }

        public ActionResult LoadUsers()
        {

            var userRoles = new List<RoleViewModel>();
            var context = new ApplicationDbContext();
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            //            MessageBox.Show(userManager.IsLockedOut(strUserId).ToString());

            //ApplicationUser test = new ApplicationUser();
            //Get all the usernames
            foreach (var user in userStore.Users)
            {
                if (user.UserName == "sysadmin")
                {

                }
                else
                {
                    var r = new RoleViewModel
                    {
                        //You want add data at the table just declare here, then go to view, add the table row and data
                        UserName = user.UserName,
                        Email = user.Email,


                    };
                    userRoles.Add(r);
                }
            }
            //Get all the Roles for our users
            foreach (var user in userRoles)
            {
                user.RoleNames = userManager.GetRoles(userStore.Users.First(s => s.UserName == user.UserName).Id);
            }


            return Json(new { data = userRoles }, JsonRequestBehavior.AllowGet);

            /* using (context)
             {

                 context.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                 //var data = context.Users.OrderBy(m => m.UserName).ToList();
                 var data3 = UserManager.Users.ToList();
                 //var data2 = context.Users.Select(m => m.Roles).ToList();
                 return Json(new { data = data3 }, JsonRequestBehavior.AllowGet);
             }*/

            /*          using (DefaultConnection dc = new PMDWebApplication())
                      {
                          // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                          var data = dc.Customers.OrderBy(a => a.ContactName).ToList();
                          return Json(new { data = data }, JsonRequestBehavior.AllowGet);
                      }*/

        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
                //MessageBox.Show(error);
            }
        }

        public JsonResult isUserAlreadyExist(string Username)
        {

            string message = "";
            var db = new ApplicationDbContext();
            //var isUserAlreadyExist = db.Users.Any(x => x.UserName == model.UserName);
            var isUserAlreadyExist = db.Users.Any(x => x.UserName == Username);
            if (isUserAlreadyExist)
            {
                message = "Username " + "'" + Username + "'" + " is already taken!";
                return Json(message, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult isEmailAlreadyExist(string Email)
        {

            string message = "";
            var db = new ApplicationDbContext();
            //var isUserAlreadyExist = db.Users.Any(x => x.UserName == model.UserName);
            var isEmailAlreadyExist = db.Users.Any(x => x.Email == Email);
            if (isEmailAlreadyExist)
            {
                message = "Email " + "'" + Email + "'" + " is already taken!";
                return Json(message, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet]
        public ActionResult RegisterNewUser()
        {
            //Get roles from role table, then add to list of selectListItem
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (var role in RoleManager.Roles)
            {
                list.Add(new SelectListItem() { Value = role.Name, Text = role.Name });

            }
            //Use viewbag to store list of SelectListItem
            //This allows you to dynamically share value from the controller to the view
            //In thew view, you retrieve those values by using name name for the property
            ViewBag.Roles = list;

            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[CustomAuthorize(Roles = "System Administrator")]
        public async Task<ActionResult> RegisterNewUser(SysAdminRegisterViewModel model)
        {

            //THE REASON WHY THE VALIDATION DID NOT SHOW AND GOT ERROR IS CAUSE IDH THIS, THE VIEWBAG.ROLES IS THE ISSUE.
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var role in RoleManager.Roles)
                list.Add(new SelectListItem() { Value = role.Name, Text = role.Name });
            //Use viewbag to store list of SelectListItem
            //This allows you to dynamically share value from the controller to the view
            //In thew view, you retrieve those values by using name name for the property
            ViewBag.Roles = list;

            /*        var db = new ApplicationDbContext();
                    var isUserAlreadyExist = db.Users.Any(x => x.UserName == model.UserName);
                    var isEmailAlreadyExist = db.Users.Any(x => x.Email == model.Email);*/

            if (ModelState.IsValid)
            {
                //Generate a temp password
                string SystemGeneratedPassword = System.Web.Security.Membership.GeneratePassword(12, 3);

                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FullName = model.FullName, Uimg = "/User-Images/default.png", PasswordLastChangedDate = new DateTime(1999, 1, 1) };
                var result = await UserManager.CreateAsync(user, SystemGeneratedPassword);
                //AddErrors(result);

                if (result.Succeeded)
                {
                    result = await UserManager.AddToRoleAsync(user.Id, model.RoleName);
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Vervoer - System Generated Account", "Dear User," +
                        Environment.NewLine +
                        "Please login to your Vervoer account using the system generated credentials" +
                        Environment.NewLine +
                        "Role : " + model.RoleName +
                        Environment.NewLine +
                        "Username : " + model.UserName +
                        Environment.NewLine +
                        "Password : " + SystemGeneratedPassword +
                        Environment.NewLine +
                        "Please note that you will be asked to change your password upon first login." +
                        Environment.NewLine +
                        "This is a System Generated Mail. Please do not reply.");

                    //Update Password Created Date
                    user.PasswordGeneratedDate = DateTime.Now;
                    UserManager.Update(user);

                    //Logging
                    var currentUserID = User.Identity.GetUserId();
                    var currentUserName = User.Identity.GetUserName();
                    List<string> roles = UserManager.GetRoles(currentUserID).ToList();
                    var listOfRoles = string.Join(",", roles);
                    logger.Info("Sysadmin registered a new user : " + model.UserName + " with the role " + model.RoleName + "|" + listOfRoles + "|" + currentUserName + "|===");


                    TempData["GreenMessage"] = "User " + "\"" + model.UserName + "\"" + " has been registered with the " + "\"" + model.RoleName + "\"" + " role successfully!";
                    return RedirectToAction("GetUsers", "SystemAdmin");
                }

                //AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            //TempData["MyRole"] = model.RoleName;

            return RedirectToAction("GetUsers", "SystemAdmin");
            //return PartialView();
        }

        [HttpGet]
        public ActionResult UpdateUserDetails(string UserName)
        {

            var userRoles = new List<RoleViewModel>();
            var context = new ApplicationDbContext();
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            RoleViewModel model = new RoleViewModel();

            var userId = UserManager.Users.Where(i => i.UserName == UserName).Select(i => i.Id).SingleOrDefault();

            //model.Name = UserName;
            //var UserRoleList = userManager.GetRoles(userStore.Users.First(s => s.UserName == UserName).Id);

            //List of all roles in this application
            List<SelectListItem> roleList = new List<SelectListItem>();
            foreach (var role in RoleManager.Roles)
                roleList.Add(new SelectListItem() { Value = role.Name, Text = role.Name });

            //List of all users
            List<SelectListItem> userList = new List<SelectListItem>();
            foreach (var user in UserManager.Users)
                userList.Add(new SelectListItem() { Value = user.UserName, Text = user.UserName });

            //List of userRole
            //List<SelectListItem> UserRoleList = new List<SelectListItem>();
            //IEnumerable<string> stringUserRoleList;
            /*  foreach (var user in userRoles)
              {

                  user.RoleNames = userManager.GetRoles(userStore.Users.First(s => s.UserName == user.UserName).Id);
              }*/

            List<string> roles = userManager.GetRoles(userId).ToList();

            //Use stringbuilder to display the content of a list, because MessageBox.Show only allow string value
            /*      StringBuilder sb = new StringBuilder();
                  foreach (string cell in roles)
                  {
                      sb.AppendLine(cell.ToString());
                  }

                  MessageBox.Show(sb.ToString());*/
            //MessageBox.Show(roles.ToString());

            //List of roles of the particular selected user
            var UserRoleList = roles.Select(s => new SelectListItem { Value = s, Text = s })
                .ToList();


            //To get the filtered list, meaning if the user have the role admin, the option admin will not appear for him
            var FilteredRoleList = roleList.Concat(UserRoleList)
             .GroupBy(x => x.Value)
             .Where(x => x.Count() == 1)
             .Select(x => x.FirstOrDefault())
             .ToList();

            ViewBag.UserRoles = UserRoleList;
            ViewBag.RemainingRoles = FilteredRoleList;
            ViewBag.AllRoles = roleList;

            ApplicationUser myUser = new ApplicationUser();
            myUser = UserManager.FindById(userId);
            ViewBag.PasswordLastChanged = myUser.PasswordLastChangedDate;
            ViewBag.Username = model.UserName;
            //ViewBag.Users = userList;

            //Pass the value if whether the user is locked out or not (true or false) to the viewbag
            //So the UpdateUserDetails view can see
            if (userManager.IsLockedOut(userId))
            {
                ViewBag.isLockedOut = "true";
                ViewBag.LockedOutEndDate = userManager.GetLockoutEndDate(userId).ToLocalTime().ToString();
            }

            return PartialView();

        }

        [HttpPost]      
        public async Task<ActionResult> AssignRole(RoleViewModel model)
        {

            var userId = UserManager.Users.Where(i => i.UserName == model.UserName).Select(i => i.Id).SingleOrDefault();

            string strUserId = userId.ToString();


            await UserManager.AddToRoleAsync(strUserId, model.Name);

            //Logging
            var currentUserID = User.Identity.GetUserId();
            var currentUserName = User.Identity.GetUserName();
            List<string> roles = UserManager.GetRoles(currentUserID).ToList();
            var listOfRoles = string.Join(",", roles);
            logger.Info("Sysadmin assigned role of " + model.Name + " to " + model.UserName + "|" + listOfRoles + "|" + currentUserName + "|===");

            TempData["GreenMessage"] = "User " + "\"" + model.UserName + "\"" + " has been assigned to the role " + "\"" + model.Name + "\"" + " successfully!";
            return RedirectToAction("GetUsers", "SystemAdmin", model);

        }

        [HttpPost]
        public async Task<ActionResult> RevokeRole(RoleViewModel model)
        {

            var userId = UserManager.Users.Where(i => i.UserName == model.UserName).Select(i => i.Id).SingleOrDefault();

            string strUserId = userId.ToString();

            await UserManager.RemoveFromRoleAsync(userId, model.Name);

            //Logging
            var currentUserID = User.Identity.GetUserId();
            var currentUserName = User.Identity.GetUserName();
            List<string> roles = UserManager.GetRoles(currentUserID).ToList();
            var listOfRoles = string.Join(",", roles);
            logger.Info("Sysadmin revoked role of " + model.Name + " from " + model.UserName + "|" + listOfRoles + "|" + currentUserName + "|===");


            TempData["RedMessage"] = "User " + "\"" + model.UserName + "\"" + " has been revoked from the role " + "\"" + model.Name + "\"" + " successfully!";
            return RedirectToAction("GetUsers", "SystemAdmin", model);

        }


        [HttpPost]
        public async Task<ActionResult> LockUserAccount(RoleViewModel model)
        {
            var context = new ApplicationDbContext();
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var userId = UserManager.Users.Where(i => i.UserName == model.UserName).Select(i => i.Id).SingleOrDefault();
            ApplicationUser user = userManager.FindByName(model.UserName);

            //userManager.AccessFailedAsync(strUserId);

            //To lock a user's account
            //await UserManager.AccessFailedAsync(userId);

            //This unlocks the user account
            //await UserManager.SetLockoutEndDateAsync(userId, new DateTimeOffset(DateTime.Now));
            //MessageBox.Show(DateTime.Now.ToString());

            //DateTime of 15 minutes later
            DateTime EndOfLockOut = DateTime.Now.AddMinutes(15);
            /*MessageBox.Show(model.LockoutEndDate.ToString());
            if(model.LockoutEndDate < DateTime.Now)
            {
                ModelState.AddModelError("DateTime", "Please enter a valid datetime");
                RedirectToAction("UpdateUserDetails");
            }
            if (model.LockoutEndDate != null)
            {
                EndOfLockOut = model.LockoutEndDate;
            }*/

            EndOfLockOut = model.LockoutEndDate;
            //This locks the user's account for 15minutes
            await UserManager.SetLockoutEndDateAsync(userId, new DateTimeOffset(EndOfLockOut));

            //MessageBox.Show("Before :" + userManager.GetLockoutEndDate(strUserId).ToString());
            //user.LockoutEndDateUtc = new DateTime(9999, 12, 30);

            //user.FullName = "test";
            //user.LockoutEndDateUtc = DateTime.Now.AddMinutes(42);
            //userManager.UpdateAsync(user);
            //context.SaveChanges();
            //MessageBox.Show("After :" + userManager.GetLockoutEndDate(strUserId).ToString());

            //Logging
            var currentUserID = User.Identity.GetUserId();
            var currentUserName = User.Identity.GetUserName();
            List<string> roles = UserManager.GetRoles(currentUserID).ToList();
            var listOfRoles = string.Join(",", roles);
            logger.Info("Sysadmin locked user : " + model.UserName + " with the role " + model.RoleNames + "|" + listOfRoles + "|" + currentUserName + "|===");


            TempData["RedMessage"] = "The user " + "\"" + model.UserName + "\"" + " account has been locked successfully!";

            return RedirectToAction("GetUsers", "SystemAdmin", model);

        }

        [HttpPost]
        public async Task<ActionResult> UnlockUserAccount(RoleViewModel model)
        {

            var userId = UserManager.Users.Where(i => i.UserName == model.UserName).Select(i => i.Id).SingleOrDefault();

            //This unlocks the user account
            await UserManager.SetLockoutEndDateAsync(userId, new DateTimeOffset(DateTime.Now));

            //Logging
            var currentUserID = User.Identity.GetUserId();
            var currentUserName = User.Identity.GetUserName();
            List<string> roles = UserManager.GetRoles(currentUserID).ToList();
            var listOfRoles = string.Join(",", roles);
            logger.Info("Sysadmin unlocked user : " + model.UserName + " locked account " + "|" + listOfRoles + "|" + currentUserName + "|===");

            TempData["GreenMessage"] = "The user " + "\"" + model.UserName + "\"" + " account has been unlocked successfully!";

            return RedirectToAction("GetUsers", "SystemAdmin", model);

        }

        [HttpPost]
        public async Task<ActionResult> ResetUserPassword(RoleViewModel model)
        {
            string SystemGeneratedPassword = System.Web.Security.Membership.GeneratePassword(10, 3);
            var userId = UserManager.Users.Where(i => i.UserName == model.UserName).Select(i => i.Id).SingleOrDefault();
            ApplicationUser user = new ApplicationUser();
            user = UserManager.FindById(userId);

            var token = await UserManager.GeneratePasswordResetTokenAsync(userId);
            var result = await UserManager.ResetPasswordAsync(userId, token, SystemGeneratedPassword);

            if (result.Succeeded)
            {

                await UserManager.SendEmailAsync(user.Id, "Vervoer - System Generated Account", "Dear User," +
                          Environment.NewLine +
                          "Your Vervoer's account password has been resetted successfully. Please login to the system using the new password" +
                          Environment.NewLine +
                          "Username : " + model.UserName +
                          Environment.NewLine +
                          "Password : " + SystemGeneratedPassword +
                          Environment.NewLine +
                          "Please note that you will be asked to change your password upon first login." +
                          Environment.NewLine +
                          "This is a System Generated Mail. Please do not reply.");

                //Change it to this date so user needs to reset the password themselves upon login
                user.PasswordLastChangedDate = new DateTime(1999, 1, 1);

                //This field ensure that if the password expires, they cannot login
                user.PasswordGeneratedDate = DateTime.Now;
                UserManager.Update(user);

                TempData["GreenMessage"] = "The user " + "\"" + model.UserName + "\"" + "' password has been resetted successfully! " +
                    "The system generated password is sent to the user's email.";
                //return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
                return RedirectToAction("GetUsers", "SystemAdmin");

            }
            AddErrors(result);
            return RedirectToAction("GetUsers", "SystemAdmin");

        }

        [HttpGet]
        public ActionResult DeleteUser(string UserName)
        {
            TempData["Username"] = UserName;
            return PartialView();

        }

        [HttpPost]
        public async Task<ActionResult> DeleteUser(RoleViewModel model)
        {

            var userRoles = new List<RoleViewModel>();
            var context = new ApplicationDbContext();
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var userId = UserManager.Users.Where(i => i.UserName == model.UserName).Select(i => i.Id).SingleOrDefault();

            if (ModelState.IsValid)
            {
                if (userId == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
             /*   using (var ctx = new dbVervoerEntities())
                {
                    AspNetUser user = ctx.AspNetUsers.Where(x => x.Id == userId).FirstOrDefault();
                    ctx.AspNetUsers.Remove(user);
                    ctx.SaveChanges();
                }
*/
                //Delete here
                var user = await UserManager.FindByIdAsync(userId);
                var logins = user.Logins;
                var rolesForUser = await UserManager.GetRolesAsync(userId);

                using (var transaction = context.Database.BeginTransaction())
                {
                    foreach (var login in logins.ToList())
                    {
                        await UserManager.RemoveLoginAsync(login.UserId, new UserLoginInfo(login.LoginProvider, login.ProviderKey));
                    }

                    if (rolesForUser.Count() > 0)
                    {
                        foreach (var item in rolesForUser.ToList())
                        {
                            // item should be the name of the role
                            var result = await UserManager.RemoveFromRoleAsync(user.Id, item);
                        }
                    }

                    await UserManager.DeleteAsync(user);
                    transaction.Commit();
                }

                //Logging
                var currentUserID = User.Identity.GetUserId();
                var currentUserName = User.Identity.GetUserName();
                List<string> roles = UserManager.GetRoles(currentUserID).ToList();
                var listOfRoles = string.Join(",", roles);
                logger.Info("Sysadmin deleted user " + model.UserName + "|" + listOfRoles + "|" + currentUserName + "|===");


                TempData["RedMessage"] = "The user " + "\"" + model.UserName + "\"" + " has been deleted successfully!";
                return RedirectToAction("GetUsers", "SystemAdmin", model);

            }

            else
            {
                return View("Error");
            }
        }


        //Logs Management
        public ActionResult GetLogs()
        {
            return View();
        }


        public ActionResult LoadLogs()
        {
            //change to test for testing purposes
            var fileContents = System.IO.File.ReadAllText(Server.MapPath(@"~/logs/app-log.txt"));
            //var fileContents = System.IO.File.ReadAllText(Server.MapPath(@"~/logs/app-log.txt"));
            //Test string
            /*
             * 2019-12-31 17:34:57.8547|WARN|PMDWebApplication.Controllers.AccountController|Invalid Login Attempt for |Clerk
                Admin
                |fasfsafsa

             */
            var listOfLogs = new List<LoggingViewModel>();
            

            string[] log = fileContents.Split(new[] { "|===" }, StringSplitOptions.None);
            //MessageBox.Show(log.Length.ToString()); For some reason the length is +1, meaning if i have 3 records, there is 4 (I think its bc of how .split works)
            //So when i loop through logInfo, i have to i-1, my i also start with 1 as a result
            /* try
             {

                 //DateTime test = DateTime.ParseExact("2019-12-31 17:34:57.8547", "yyyy-MM-dd HH:mm tt", null);
                 DateTime test = DateTime.Parse("2019-12-31 17:34:57.8547");
                 MessageBox.Show(test.ToString());
             }
             catch(Exception e)
             {
                 MessageBox.Show(e.ToString());

             }*/
            for (int i = 1; i < log.Length; i++)
            {
                //Base on the test string above, this logInfo will have 6 elements
                //[0] - Date Time  [1] - Type  [2] - Controller
                //[3] - Message    [4] - Roles [5] Username
                string[] logInfo = log[i - 1].Split(new[] { "|" }, StringSplitOptions.None);

                var l = new LoggingViewModel
                {
                    Id = i,
                    Date = DateTime.Parse(logInfo[0]).ToString(),
                    Type = logInfo[1],
                    ControllerName = logInfo[2].Replace("PMDWebApplication.Controllers.", ""),
                    Message = logInfo[3],
                    Roles = logInfo[4],
                    Username = logInfo[5]
                };

                listOfLogs.Add(l);
            }

            /*   StringBuilder sb = new StringBuilder();
               foreach(var model in listOfLogs)
               {
                   sb.Append(model);
               }
               MessageBox.Show("This fella has " + sb);*/

            //Get all the usernames
            /*    foreach (string line in log)
                {
                    var l = new LoggingViewModel
                    {
                        //You want add data at the table just declare here, then go to view, add the table row and data
                        Message = line,
                        TimeAgo = new DateTime(2000, 2, 2),
                        Date = new DateTime(1000, 1, 1),
                        Type = "Warn"


                    };
                    listOfLogs.Add(l);
                }*/

            return Json(new { data = listOfLogs }, JsonRequestBehavior.AllowGet);

        }

        //Security Configurations

        [HttpGet]
        public ActionResult ConfigureSettings()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            //SecurityConfigurationsViewModel currentModel = new SecurityConfigurationsViewModel();
            int Current_Password_Length = db.SecurityConfigurationsViewModels.Select(p => p.Password_Length).FirstOrDefault();
            int Current_Password_Expiry = db.SecurityConfigurationsViewModels.Select(p => p.PasswordExpiry).FirstOrDefault();
            bool Current_RequireNonLetterOrDigit = db.SecurityConfigurationsViewModels.Select(p => p.RequireNonLetterOrDigit).FirstOrDefault();
            bool Current_RequireDigit = db.SecurityConfigurationsViewModels.Select(p => p.RequireDigit).FirstOrDefault();
            bool Current_RequireLowercase = db.SecurityConfigurationsViewModels.Select(p => p.RequireLowercase).FirstOrDefault();
            bool Current_RequireUppercase = db.SecurityConfigurationsViewModels.Select(p => p.RequireUppercase).FirstOrDefault();
            int Current_Password_Attempts = db.SecurityConfigurationsViewModels.Select(p => p.Password_Attempts).FirstOrDefault();
            int Current_Lockout_Duration = db.SecurityConfigurationsViewModels.Select(p => p.LockoutDuration).FirstOrDefault();
            int Current_Inactive_Duration = db.SecurityConfigurationsViewModels.Select(p => p.InactiveDuration).FirstOrDefault();

            //int Current_Password_Expiry = AppSettingsHelper.Get<int>("PasswordExpiry");


            TempData["Current_Password_Length"] = Current_Password_Length;
            TempData["Current_Password_Expiry"] = Current_Password_Expiry;
            TempData["Current_RequireNonLetterOrDigit"] = Current_RequireNonLetterOrDigit;
            TempData["Current_RequireDigit"] = Current_RequireDigit;
            TempData["Current_RequireLowercase"] = Current_RequireLowercase;
            TempData["Current_RequireUppercase"] = Current_RequireUppercase;
            TempData["Current_Password_Attempts"] = Current_Password_Attempts;
            TempData["Current_Lockout_Duration"] = Current_Lockout_Duration;
            TempData["Current_Inactive_Duration"] = Current_Inactive_Duration;

            return View();
        }

        [HttpPost]
        public ActionResult ConfigureSettings(SecurityConfigurationsViewModel model)
        {
            //if (user.AccessFailedCount == ConfigurationManagerHelper.GetAppSettingsToInt(Constant.UserMaxFailureLoginAttempts, 6) - 1)
            ApplicationDbContext db = new ApplicationDbContext();

            TempData["Current_Password_Length"] = db.SecurityConfigurationsViewModels.Select(p => p.Password_Length).FirstOrDefault();
            TempData["Current_Password_Expiry"] = db.SecurityConfigurationsViewModels.Select(p => p.PasswordExpiry).FirstOrDefault();
            TempData["Current_RequireNonLetterOrDigit"] = db.SecurityConfigurationsViewModels.Select(p => p.RequireNonLetterOrDigit).FirstOrDefault();
            TempData["Current_RequireDigit"] = db.SecurityConfigurationsViewModels.Select(p => p.RequireDigit).FirstOrDefault();
            TempData["Current_RequireLowercase"] = db.SecurityConfigurationsViewModels.Select(p => p.RequireLowercase).FirstOrDefault();
            TempData["Current_RequireUppercase"] = db.SecurityConfigurationsViewModels.Select(p => p.RequireUppercase).FirstOrDefault();
            TempData["Current_Password_Attempts"] = db.SecurityConfigurationsViewModels.Select(p => p.Password_Attempts).FirstOrDefault();
            TempData["Current_Lockout_Duration"] = db.SecurityConfigurationsViewModels.Select(p => p.LockoutDuration).FirstOrDefault();
            TempData["Current_Inactive_Duration"] = db.SecurityConfigurationsViewModels.Select(p => p.InactiveDuration).FirstOrDefault();
            //To unlock locked user account
            //await UserManager.SetLockoutEndDateAsync(userId, new DateTimeOffset(DateTime.UtcNow));
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //MessageBox.Show(model.RequireNonLetterOrDigit.ToString());
            //MessageBox.Show(model.Password_Attempts.ToString());
            //ApplicationDbContext db = new ApplicationDbContext();
            //  string sqlquery = "UPDATE [dbo].[AspNetUsers] SET Uimg = '" + "/User-Images/" + file.FileName + "' WHERE Id='" + uID + "'";

            //All model's variable
            int PasswordLength = model.Password_Length;
            int PasswordExpiry = model.PasswordExpiry;
            bool RequireNonLetterOrDigit = model.RequireNonLetterOrDigit;
            bool RequireDigit = model.RequireDigit;
            bool RequireLowercase = model.RequireLowercase;
            bool RequireUppercase = model.RequireUppercase;
            int PasswordAttempts = model.Password_Attempts;
            int LockoutDuration = model.LockoutDuration;
            int InactiveDuration = model.InactiveDuration;

            //SQL Command 
            var sql = @"Update [dbo].[SecurityConfigurations] 
                SET Password_Length = @PasswordLength, 
                PasswordExpiry = @PasswordExpiry,
                RequireNonLetterOrDigit = @RequireNonLetterOrDigit,
                RequireDigit = @RequireDigit,
                RequireLowercase = @RequireLowercase,
                RequireUppercase = @RequireUppercase,
                Password_Attempts = @PasswordAttempts,
                LockoutDuration = @LockoutDuration,
                InactiveDuration = @InactiveDuration";

            //Execution of the SQL Command
            db.Database.ExecuteSqlCommand(
                sql,
                new SqlParameter("@PasswordLength", PasswordLength),
                new SqlParameter("@PasswordExpiry", PasswordExpiry),
                new SqlParameter("@RequireNonLetterOrDigit", RequireNonLetterOrDigit),
                new SqlParameter("@RequireDigit", RequireDigit),
                new SqlParameter("@RequireLowercase", RequireLowercase),
                new SqlParameter("@RequireUppercase", RequireUppercase),
                new SqlParameter("@PasswordAttempts", PasswordAttempts),
                new SqlParameter("@LockoutDuration", LockoutDuration),
                new SqlParameter("@InactiveDuration", InactiveDuration));

            //Update in web.config instead of in database
            Configuration config = WebConfigurationManager.OpenWebConfiguration("/");

            //THIS IS NOT IDEAL BECAUSE EVERY CHANGES TO THE XML FILE WILL CAUSE A RESTART IN THE APP
            //string oldValue = config.AppSettings.Settings["TESTKEY"].Value;

            /*            config.AppSettings.Settings["PasswordLength"].Value = PasswordLength.ToString();                    //8
                     
                        config.AppSettings.Settings["PasswordExpiry"].Value = PasswordExpiry.ToString();  //False
                        config.AppSettings.Settings["RequireNonLetterOrDigit"].Value = RequireNonLetterOrDigit.ToString();  //False
                        config.AppSettings.Settings["RequireDigit"].Value = RequireDigit.ToString();                        //False
                        config.AppSettings.Settings["RequireLowercase"].Value = RequireLowercase.ToString();                //False
                        config.AppSettings.Settings["RequireUppercase"].Value = RequireUppercase.ToString();                //False
                        config.AppSettings.Settings["PasswordAttempts"].Value = PasswordAttempts.ToString();                //5
                        config.AppSettings.Settings["LockoutDuration"].Value = LockoutDuration.ToString();                  //5     
                        config.AppSettings.Settings["InactiveDuration"].Value = InactiveDuration.ToString();                //15
                        config.Save(ConfigurationSaveMode.Modified);

                        TempData["Current_PasswordLength"] = AppSettingsHelper.Get<int>("PasswordLength");               
                        TempData["Current_Password_Expiry"] = AppSettingsHelper.Get<int>("PasswordExpiry");
                        TempData["Current_RequireNonLetterOrDigit"] = AppSettingsHelper.Get<bool>("RequireNonLetterOrDigit"); 
                        TempData["Current_RequireDigit"] = AppSettingsHelper.Get<bool>("RequireDigit");
                        TempData["Current_RequireLowercase"] = AppSettingsHelper.Get<bool>("RequireLowercase");
                        TempData["Current_PasswordAttempts"] = AppSettingsHelper.Get<bool>("RequireUppercase");
                        TempData["Current_LockoutDuration"] = AppSettingsHelper.Get<int>("PasswordAttempts");
                        TempData["Current_InactiveDuration"] = AppSettingsHelper.Get<int>("LockoutDuration");*/

            int Current_Password_Length = db.SecurityConfigurationsViewModels.Select(p => p.Password_Length).FirstOrDefault();
            int Current_Password_Expiry = db.SecurityConfigurationsViewModels.Select(p => p.PasswordExpiry).FirstOrDefault();
            bool Current_RequireNonLetterOrDigit = db.SecurityConfigurationsViewModels.Select(p => p.RequireNonLetterOrDigit).FirstOrDefault();
            bool Current_RequireDigit = db.SecurityConfigurationsViewModels.Select(p => p.RequireDigit).FirstOrDefault();
            bool Current_RequireLowercase = db.SecurityConfigurationsViewModels.Select(p => p.RequireLowercase).FirstOrDefault();
            bool Current_RequireUppercase = db.SecurityConfigurationsViewModels.Select(p => p.RequireUppercase).FirstOrDefault();
            int Current_Password_Attempts = db.SecurityConfigurationsViewModels.Select(p => p.Password_Attempts).FirstOrDefault();
            int Current_Lockout_Duration = db.SecurityConfigurationsViewModels.Select(p => p.LockoutDuration).FirstOrDefault();
            int Current_Inactive_Duration = db.SecurityConfigurationsViewModels.Select(p => p.InactiveDuration).FirstOrDefault();

 /*           TempData["Current_Password_Length"] = Current_Password_Length;
            TempData["Current_Password_Expiry"] = Current_Password_Expiry;
            TempData["Current_RequireNonLetterOrDigit"] = Current_RequireNonLetterOrDigit;
            TempData["Current_RequireDigit"] = Current_RequireDigit;
            TempData["Current_RequireLowercase"] = Current_RequireLowercase;
            TempData["Current_RequireUppercase"] = Current_RequireUppercase;
            TempData["Current_Password_Attempts"] = Current_Password_Attempts;
            TempData["Current_Lockout_Duration"] = Current_Lockout_Duration;
            TempData["Current_Inactive_Duration"] = Current_Inactive_Duration;*/

            //Logging
            var currentUserID = User.Identity.GetUserId();
            var currentUserName = User.Identity.GetUserName();
            List<string> roles = UserManager.GetRoles(currentUserID).ToList();
            var listOfRoles = string.Join(",", roles);
            logger.Info("Sysadmin updated security configurations" + "|" + listOfRoles + "|" + currentUserName + "|===");

            TempData["Current_Password_Length"] = db.SecurityConfigurationsViewModels.Select(p => p.Password_Length).FirstOrDefault();
            TempData["Current_Password_Expiry"] = db.SecurityConfigurationsViewModels.Select(p => p.PasswordExpiry).FirstOrDefault();
            TempData["Current_RequireNonLetterOrDigit"] = db.SecurityConfigurationsViewModels.Select(p => p.RequireNonLetterOrDigit).FirstOrDefault();
            TempData["Current_RequireDigit"] = db.SecurityConfigurationsViewModels.Select(p => p.RequireDigit).FirstOrDefault();
            TempData["Current_RequireLowercase"] = db.SecurityConfigurationsViewModels.Select(p => p.RequireLowercase).FirstOrDefault();
            TempData["Current_RequireUppercase"] = db.SecurityConfigurationsViewModels.Select(p => p.RequireUppercase).FirstOrDefault();
            TempData["Current_Password_Attempts"] = db.SecurityConfigurationsViewModels.Select(p => p.Password_Attempts).FirstOrDefault();
            TempData["Current_Lockout_Duration"] = db.SecurityConfigurationsViewModels.Select(p => p.LockoutDuration).FirstOrDefault();
            TempData["Current_Inactive_Duration"] = db.SecurityConfigurationsViewModels.Select(p => p.InactiveDuration).FirstOrDefault();
            TempData["Message"] = "Security Configurations has been updated successfully!";

            //Super important to ModelState.AddModelError to ensure that the validation messages show up!
            ModelState.AddModelError("", "");
            /*UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(DefaultLockingTime);
            UserManager.MaxFailedAccessAttemptsBeforeLockout = 10;*/

            return View("ConfigureSettings");

        }

        [HttpPost]
        public ActionResult ResetAllUsersPassword()
        {
            //MessageBox.Show("HELOOO");
            var context = new ApplicationDbContext();
            var userStore = new UserStore<ApplicationUser>(context);
            //string testId = "e6dc3869 - 6f4a - 46bf - a982 - 4da8ba38d51d";


            /*     GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
                   var users = _unitOfWork.GetRepositoryInstance<AspNetUser>().GetAllRecords();*/



            foreach (ApplicationUser user in userStore.Users)
            {
                var isSysAdmin = UserManager.GetRoles(user.Id).Contains("System Administrator");
                if (!isSysAdmin)
                {
                    ApplicationUser myUser = UserManager.FindById(user.Id);
                   
                    myUser.PasswordLastChangedDate = new DateTime(2000, 2, 2);
                    UserManager.Update(myUser);

                    //MessageBox.Show(user.PasswordLastChangedDate.ToString());
                    //_unitOfWork.GetRepositoryInstance<AspNetUser>().UpdateByWhereClause(x=>x.Id == user.Id);
                }
            }
            TempData["RedMessage"] = "All User's Password has been resetted successfully!";
            return View("ConfigureSettings");

        }



    }
}