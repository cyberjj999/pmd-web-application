using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PMDWebApplication.DB;
using PMDWebApplication.Models.Home;
using PMDWebApplication.Repository;

using iTextSharp.text.pdf;

using System.Windows.Forms;
using System.Text;
using System.Collections;
using System.IO;
using PMDWebApplication.Models;
using iTextSharp.text.pdf.parser;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Entity.Validation;

namespace PMDWebApplication.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        public dbVervoerEntities ctx = new dbVervoerEntities();

        public ActionResult Index()
        {
            return View();
        }


        public List<SelectListItem> GetCategory(int productID)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var category = _unitOfWork.GetRepositoryInstance<AspNetCategory>().GetAllRecords();
            foreach (var item in category)
            {
                list.Add(new SelectListItem { Value = item.CategoryId.ToString(), Text = item.CategoryName });
            }
            return list;
        }

        [Authorize]
        public ActionResult ViewInsurance(int InsuranceId)
        {
            return View(_unitOfWork.GetRepositoryInstance<AspNetInsurance>().GetFirstorDefault(InsuranceId));
        }


        [Authorize]
        public ActionResult ViewItem(int productId)
        {
            return View(_unitOfWork.GetRepositoryInstance<AspNetProduct>().GetFirstorDefault(productId));

        }



        public string message = "";

        [Authorize]
        public ActionResult AddtoCartInsurance(AspNetInsurance Insurance)
        {
            var userId = User.Identity.GetUserId();
            AspNetCartInsurance existingCart = _unitOfWork.GetRepositoryInstance<AspNetCartInsurance>().GetFirstorDefaultByParameter(x => x.InsuranceId == Insurance.InsuranceId);
            AspNetInsurance dbInsurance = _unitOfWork.GetRepositoryInstance<AspNetInsurance>().GetFirstorDefaultByParameter(x => x.InsuranceId == Insurance.InsuranceId);


                if (existingCart != null && existingCart.MemberId.Equals(userId))
                {
                    message = "Insurance existed";
                }
                else
                {
                    AspNetCartInsurance cart = new AspNetCartInsurance();
                    cart.InsuranceId = Insurance.InsuranceId;
                    cart.MemberId = userId;
                    cart.PolicyId = Guid.NewGuid();
                    if (Insurance.CategoryId == 1)
                    {
                    cart.PlanType = "Riders";
                    }
                    else if(Insurance.CategoryId == 2)
                    {
                    cart.PlanType = "Pedestrain";
                    }

                _unitOfWork.GetRepositoryInstance<AspNetCartInsurance>().Add(cart);

                }
            
            return Redirect("InsuranceEcommerce");
        }

        [Authorize]
        public ActionResult Cart()
        {
            return View();
        }

        public ActionResult CartDelete(int cartId)
        {
            int quantity = 0;
            dbVervoerEntities dbcontext = new dbVervoerEntities();
            AspNetCart Cart = dbcontext.AspNetCarts.Find(cartId);
            AspNetProduct product = _unitOfWork.GetRepositoryInstance<AspNetProduct>().GetFirstorDefaultByParameter(x => x.ProductId == Cart.ProductId);
            quantity = (int)(product.Quantity + Cart.Quantity);
            product.Quantity = quantity;
            _unitOfWork.GetRepositoryInstance<AspNetProduct>().UpdateProduct(product);
            dbcontext.AspNetCarts.Remove(Cart);
            dbcontext.SaveChanges();
            return RedirectToAction("Cart");

        }

        public ActionResult CartDeleteInsurance(int cartId)
        {
            dbVervoerEntities dbcontext = new dbVervoerEntities();
            AspNetCartInsurance Cart = dbcontext.AspNetCartInsurances.Find(cartId);
            dbcontext.AspNetCartInsurances.Remove(Cart);
            dbcontext.SaveChanges();
            return RedirectToAction("Cart");
        }

        public ActionResult Error404()
        {
            return View();
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


        public ActionResult FAQPage(string searching)
        {
            return View(ctx.FAQs.Where(x => x.Question.Contains(searching) || searching == null).ToList());
        }

        public ActionResult FAQAnswers(int Id)
        {
            using (ctx)
            {
                return View(ctx.FAQs.Where(x => x.Id == Id).FirstOrDefault());
            }
        }



        [Authorize]
        public ActionResult AddToCart(AspNetProduct product)
        {
            dbVervoerEntities dbContext = new dbVervoerEntities();
            var userId = User.Identity.GetUserId();
            AspNetUser currentUser = _unitOfWork.GetRepositoryInstance<AspNetUser>().GetFirstorDefaultByParameter(x => x.Id == userId);
            AspNetCart existingCart = _unitOfWork.GetRepositoryInstance<AspNetCart>().GetFirstorDefaultByParameter(x => x.ProductId == product.ProductId);
            AspNetProduct dbProduct = _unitOfWork.GetRepositoryInstance<AspNetProduct>().GetFirstorDefaultByParameter(x => x.ProductId == product.ProductId);
            int quantity = 0;

            if (currentUser.EmailConfirmed == true)
            {
                if (existingCart != null && existingCart.MemberId.Equals(userId))
                {
                    existingCart.Quantity +=(int)product.Quantity;
                    quantity = (int)(dbProduct.Quantity - product.Quantity);
                    dbProduct.Quantity = quantity;
                    _unitOfWork.GetRepositoryInstance<AspNetCart>().UpdateCart(existingCart);
                    _unitOfWork.GetRepositoryInstance<AspNetProduct>().UpdateProduct(dbProduct);
                }
                else
                {
                    AspNetCart cart = new AspNetCart();
                    cart.ProductId = product.ProductId;
                    cart.MemberId = userId;
                    cart.Quantity = (int)product.Quantity;
                    quantity = (int)(dbProduct.Quantity - cart.Quantity);
                    product.Quantity = quantity;
                    _unitOfWork.GetRepositoryInstance<AspNetCart>().Add(cart);
                    _unitOfWork.GetRepositoryInstance<AspNetProduct>().UpdateProduct(product);

                }
            }
            else
            {

                return RedirectToAction("Ecommerce", "Home", new { @Message = "Please confirm email" });
            }


            return Redirect("Ecommerce");
        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult InsuranceEcommerce(string search, int? page)
        {
            if (message.Equals(""))
            {
                HomeInsuranceViewModel model = new HomeInsuranceViewModel();
                return View(model.CreateModel(Server.HtmlEncode(search), 4, page));
            }
            else
            {
                ViewBag.Message = message;
                HomeInsuranceViewModel model = new HomeInsuranceViewModel();
                return View(model.CreateModel(Server.HtmlEncode(search), 4, page));
            }
        }


        [Authorize]
        [ValidateInput(false)]
        public ActionResult Ecommerce(string search, int? page, string message)
        {

            ViewBag.Message = message;
            HomeProductViewModel model = new HomeProductViewModel();
            return View(model.CreateModel(Server.HtmlEncode(search), 4, page));

        }

        public ActionResult BuyOrClaim()
        {

            return View();
        }

        [HttpPost]
        public ActionResult UploadInsuranceClaimFile(HttpPostedFileBase file)
        {
            ClaimInsuranceViewModel model = new ClaimInsuranceViewModel();

            var userId = User.Identity.GetUserId();
            AspNetUser user = _unitOfWork.GetRepositoryInstance<AspNetUser>().GetFirstorDefaultByParameter(x => x.Id == userId);
            var haveInsurance = _unitOfWork.GetRepositoryInstance<AspNetCartInsurance>().GetAllRecordsIQueryable().Any(x => x.MemberId == userId);
            if (haveInsurance)
            {
                AspNetCartInsurance insurance = _unitOfWork.GetRepositoryInstance<AspNetCartInsurance>().GetFirstorDefaultByParameter(x => x.MemberId == userId);
                model.InsuranceNo = insurance.PolicyId.ToString();
                model.PlanType = insurance.PlanType;
            }

            if (file != null && file.ContentLength > 0)
            {

                string filename = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/InsuranceClaims"), filename);
                //string dest = System.IO.Path.Combine(Server.MapPath("~/InsuranceClaims"), "myFile.pdf");

                file.SaveAs(path);

                try
                {

                    PdfReader pdfReader = new PdfReader(path);
                    var fieldList = GetFormFieldNamesWithValues(pdfReader);
                    model.Nominee = fieldList[3];
                    model.VictimNRIC = fieldList[4];
                    model.NomineeNRIC = fieldList[5];
                    model.Relationship = fieldList[6];
                    model.Address = fieldList[7];
                    model.PhoneNumber = fieldList[8];

                }
                catch (iTextSharp.text.exceptions.InvalidPdfException)
                {
                    TempData["FileErrorMessage"] = "Please select a .PDF file!";
                    return View("ClaimInsurance", model);
                }
                //TempData["Message"] = "Profile Picture is updated successfully!";
            }
            else
            {
                TempData["FileErrorMessage"] = "You have not specified a file!";
                return View("ClaimInsurance", model);

            }

            model.FullName = user.fullname;
            TempData["Message"] = "Your insurance claim form is uploaded successfully! Please refer to the fields at the bottom " +
                "to ensure everything is correctly filled up before you submit the form.";
            return View("ClaimInsurance",model);

        }

        private static string[] GetFormFieldNames(PdfReader pdfReader)
        {
            //list.Add(pdfReader.AcroFields.Fields.Select(x => x.Key));
            /*return string.Join("\r\n", pdfReader.AcroFields.Fields
                                           .Select(x => x.Key).ToArray());*/
            return pdfReader.AcroFields.Fields.Select(x => x.Key).ToArray();
        }

        private static string[] GetFormFieldNamesWithValues(PdfReader pdfReader)
        {
           /* return string.Join("\r\n", pdfReader.AcroFields.Fields
                                           .Select(x => x.Key + "=" +
                                            pdfReader.AcroFields.GetField(x.Key))
                                           .ToArray());*/

            return pdfReader.AcroFields.Fields.Select(x => pdfReader.AcroFields.GetField(x.Key)).ToArray();
        }

        public ActionResult ClaimInsurance()
        {
            var userId = User.Identity.GetUserId();
            dbVervoerEntities db = new dbVervoerEntities();
            ClaimInsuranceViewModel emptyModel = new ClaimInsuranceViewModel();
            AspNetUser user = _unitOfWork.GetRepositoryInstance<AspNetUser>().GetFirstorDefaultByParameter(x => x.Id == userId);
            var haveInsurance = _unitOfWork.GetRepositoryInstance<AspNetCartInsurance>().GetAllRecordsIQueryable().Any(x => x.MemberId == userId);
            if (haveInsurance)
            {
                AspNetCartInsurance insurance = _unitOfWork.GetRepositoryInstance<AspNetCartInsurance>().GetFirstorDefaultByParameter(x => x.MemberId == userId);
                emptyModel.InsuranceNo = insurance.PolicyId.ToString();
                emptyModel.PlanType = insurance.PlanType;
      
            }

            if (db.InsuranceClaims.Any(i => i.UserID == userId))
            {
                //MessageBox.Show("HAVE INSURANCE ALR");
                var claim = db.InsuranceClaims.Where(i => i.UserID == userId).FirstOrDefault();
                ClaimInsuranceViewModel model = new ClaimInsuranceViewModel();
                model.Address = claim.Address;
                model.ClaimNo = claim.ClaimNo;
                model.Status = claim.Status;
                model.FullName = claim.VictimName;
                model.Id = claim.Id;
                model.InsuranceNo = claim.PolicyNo;    
                model.Nominee = claim.NomineeName;
                model.VictimNRIC = claim.VictimNRIC;
                model.NomineeNRIC = claim.NomineeNRIC;
                model.PhoneNumber = claim.ContactNo;
                model.PlanType = claim.PlanType;
                model.Relationship = claim.Relationship;
                model.DenialMessage = claim.DenialMessage;
              
                //model.Signature = claim.Signature;

                return View(model);

            }

            emptyModel.FullName = user.fullname;
            return View(emptyModel);
        }

        [HttpPost]
        public ActionResult ClaimInsurance(ClaimInsuranceViewModel model)
        {
            //https://www.thewebflash.com/using-signature-pad-with-asp-net-mvc/
            //^ complete the signature portion tomorrow!
            //string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var userId = User.Identity.GetUserId();
            dbVervoerEntities db = new dbVervoerEntities();
            InsuranceClaim insuranceClaim = new InsuranceClaim();

            AspNetUser user = _unitOfWork.GetRepositoryInstance<AspNetUser>().GetFirstorDefaultByParameter(x => x.Id == userId);
            AspNetCartInsurance insurance = _unitOfWork.GetRepositoryInstance<AspNetCartInsurance>().GetFirstorDefaultByParameter(x => x.MemberId == userId);

            string policyNo = insurance.PolicyId.ToString();
            string planType = insurance.PlanType;
            string fullname = user.fullname;
            string nric = model.VictimNRIC;
            string nominee = model.Nominee;
            string NomineeNRIC = model.NomineeNRIC;
            string relationship = model.Relationship;
            string address = model.Address;
            string phoneNumber = model.PhoneNumber;
            var signature = model.Signature;
            string status = "Pending";
            DateTime date = DateTime.Now;
            Guid ClaimNo = Guid.NewGuid();
            if (signature == null)
            {
                ModelState.AddModelError("", "Please provide your signature.");
                return View("ClaimInsurance");

            }

            insuranceClaim.PolicyNo = policyNo;
            insuranceClaim.PlanType = planType;
            insuranceClaim.VictimName = fullname;
            insuranceClaim.VictimNRIC = nric;
            insuranceClaim.NomineeName = nominee;
            insuranceClaim.NomineeNRIC = NomineeNRIC;
            insuranceClaim.Relationship = relationship;
            insuranceClaim.Address = address;
            insuranceClaim.ContactNo = phoneNumber;
            insuranceClaim.Signature = signature;
            insuranceClaim.DateOfClaim = date;
            insuranceClaim.UserID = User.Identity.GetUserId();
            insuranceClaim.ClaimNo = ClaimNo;
            insuranceClaim.Status = status;

            if (db.InsuranceClaims.Any(i => i.UserID == userId))
            {
 
                var claim = db.InsuranceClaims.Where(i => i.UserID == userId).FirstOrDefault();
                claim.PolicyNo = policyNo;
                claim.PlanType = planType;
                claim.VictimName = fullname;
                claim.VictimNRIC = nric;
                claim.NomineeName = nominee;
                claim.NomineeNRIC = NomineeNRIC;
                claim.Relationship = relationship;
                claim.Address = address;
                claim.ContactNo = phoneNumber;
                claim.Signature = signature;
                claim.DateOfClaim = date;
                claim.UserID = User.Identity.GetUserId();
                claim.ClaimNo = ClaimNo;
                claim.Status = status;
                db.SaveChanges();
                //_unitOfWork.GetRepositoryInstance<InsuranceClaim>().Update(claim);

            }

            else { 

            try
            {
                _unitOfWork.GetRepositoryInstance<InsuranceClaim>().Add(insuranceClaim);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    MessageBox.Show("Entity of type " + eve.Entry.Entity.GetType().Name + " in state " + eve.Entry.State
                        + " has the following validation errors");

                    foreach (var ve in eve.ValidationErrors)
                    {
                        MessageBox.Show("- Property:+ " + ve.PropertyName + " Error: " + ve.ErrorMessage);

                    }
                }
                throw;
            }

            }
            TempData["ClaimToken"] = ClaimNo;
            return View("GenerateClaimToken");
        }
        public ActionResult GenerateClaimToken()
        {
            var currentUser = User.Identity.GetUserId();
            dbVervoerEntities db = new dbVervoerEntities();
            Guid claimtoken = db.InsuranceClaims.Where(i => i.UserID == currentUser).Select(i => i.ClaimNo).SingleOrDefault();
            if (claimtoken == Guid.Empty)
            {
                return View("ClaimInsurance");
            }
            //Guid ClaimToken = db.ClaimInsuranceViewModels.Select(user => user.use == "yourValue").Where(UserID)
            TempData["ClaimToken"] = claimtoken;

            return View();
        }

        public ActionResult OurPartners()
        {
            return View();
        }

        public ActionResult Quizbank()
        {
            Globals.count = 1;
            Globals.answerscorrect = 0;
            Globals.answerswrong = 0;

            return View();
        }

        [HttpGet]
        public ActionResult ReportPMD()
        {
            if (User.Identity.GetUserId() == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult ReportPMD(ReportPMD tbl, HttpPostedFileBase file)
        {
            var userId = User.Identity.GetUserId();
            var userName = User.Identity.GetUserName();


            string filename = System.IO.Path.GetFileName(file.FileName);
            string _filename = DateTime.Now.ToString("yymmssfff") + filename;

            string extension = System.IO.Path.GetExtension(file.FileName);
            string path = System.IO.Path.Combine(Server.MapPath("~/ReportImg/"), _filename);

            tbl.Image = "~/ReportImg/" + _filename;
            tbl.Name = userName;

            if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png")
            {
                if (file.ContentLength >0)
                {
                    ctx.ReportPMDs.Add(tbl);

                    if (ctx.SaveChanges() > 0)
                    {
                        file.SaveAs(path);
                        ViewBag.msg = "Record Added";
                        ModelState.Clear();
                    }
                    else
                    {
                        ViewBag.msg = "Size is not valid";
                        return Redirect("Index");
                    }
                }
            }

            return Redirect("Index");
        }

    }
}