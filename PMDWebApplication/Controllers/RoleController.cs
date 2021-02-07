using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using Microsoft.AspNet.Identity.Owin;
using PMDWebApplication.Models;

namespace PMDWebApplication.Controllers
{
    [Authorize(Roles = "System Administrator")]
    public class RoleController : Controller
    {
        private ApplicationRoleManager _roleManager;
        private ApplicationUserManager _userManager;

        public RoleController()
        {
        }

        public RoleController(ApplicationRoleManager roleManager)
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

        // GET: Role
        public ActionResult Index()
        {
            List<RoleViewModel> list = new List<RoleViewModel>();
            foreach (var role in RoleManager.Roles)
                list.Add(new RoleViewModel(role));
            return View(list);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(RoleViewModel model)
        {

            if (model.Name == null)
            {
                //ModelState.AddModelError("", "Please enter the role name");
                //TempData["Error"] = "Error! Please enter the role name";
                ModelState.AddModelError("", "The role name field is required");
                return View("Create");
            }
            if (await RoleManager.RoleExistsAsync(model.Name))
            {
                //TempData["Error"] = "Error! Role already exists!";
                //ModelState.AddModelError("", "Please complete the recaptcha!");
                ModelState.AddModelError("", "Role already exists");
                //MessageBox.Show("Error, Role Existed");

                //****Redirect to action will clear model state, so use return view
                return View("Create");
            }
            else
            {
                var role = new ApplicationRole() { Name = model.Name };
                await RoleManager.CreateAsync(role);
                TempData["GreenMessage"] = "The role " + "\"" + role.Name + "\"" + " has been created successfully!";
                return RedirectToAction("Index");
                
            }

            //var role = new ApplicationRole() { Name = model.Name };
            //await RoleManager.CreateAsync(role);
          
        }

        public async Task<ActionResult> Edit(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            return View(new RoleViewModel(role));
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RoleViewModel model)
        {
            var role = new ApplicationRole() { Id = model.Id, Name = model.Name };
            bool roleExist = await RoleManager.RoleExistsAsync(model.Name);
            if (roleExist)
            {
                TempData["Error"] = "Error! Role already exists!";
                //ModelState.AddModelError("", "Please complete the recaptcha!");
                //ModelState.AddModelError("", "Role already exists");
                //MessageBox.Show("Error, Role Existed");
                return RedirectToAction("Edit");
            }

            else
            {
                TempData["GreenMessage"] = "The role name has been updated to " + "\"" + model.Name + "\"" + " successfully!";
                await RoleManager.UpdateAsync(role);
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> Details(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            return View(new RoleViewModel(role));
        }

        public async Task<ActionResult> Delete(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            return View(new RoleViewModel(role));
        }


        [HttpPost, ActionName("Delete")]  
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            
            var role = await RoleManager.FindByIdAsync(id);
            await RoleManager.DeleteAsync(role);
            TempData["RedMessage"] = "The role " + "\"" + role.Name + "\"" + " has been deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}