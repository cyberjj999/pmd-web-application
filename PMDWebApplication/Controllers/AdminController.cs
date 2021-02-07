using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PMDWebApplication.DB;
using PMDWebApplication.Models;
using PMDWebApplication.Repository;
using System.Data.Entity;
using System.Windows.Forms;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Drawing;

namespace PMDWebApplication.Controllers
{
    [Authorize(Roles ="Admin,System Administrator")]
    [HandleError]
    public class AdminController : Controller
    {

        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        public dbVervoerEntities db = new dbVervoerEntities();

        public List<SelectListItem> GetCategory()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var category = _unitOfWork.GetRepositoryInstance<AspNetCategory>().GetAllRecords();
            foreach (var item in category) {
                list.Add(new SelectListItem { Value = item.CategoryId.ToString(), Text = item.CategoryName });
            }
            return list;
        }       

        // GET: Admin
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult Insurance()
        {
            return View(_unitOfWork.GetRepositoryInstance<AspNetInsurance>().GetProduct());
        }

        public ActionResult Categories()
        {
            List<AspNetCategory> allcategories = _unitOfWork.GetRepositoryInstance<AspNetCategory>().GetAllRecordsIQueryable().ToList();
            return View(allcategories);
        }

        public ActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddCategory(AspNetCategory tbl)
        {
            _unitOfWork.GetRepositoryInstance<AspNetCategory>().Add(tbl);
            return RedirectToAction("AddCategory");
        }

        [HttpPost]
        public ActionResult DeleteCategory(int categoryId)
        {
            dbVervoerEntities dbcontext = new dbVervoerEntities();
            AspNetCategory category = dbcontext.AspNetCategories.Find(categoryId);
            dbcontext.AspNetCategories.Remove(category);
            dbcontext.SaveChanges();
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public ActionResult InsuranceAdd(AspNetInsurance tbl, HttpPostedFileBase file)
        {
            string pic = null;
            if (file != null)
            {
                pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/InsuranceImg/"), pic);
                // file is uploaded
                file.SaveAs(path);
            }
            tbl.InsuranceImage = pic;
            _unitOfWork.GetRepositoryInstance<AspNetInsurance>().Add(tbl);
            return RedirectToAction("Insurance");
        }

        public ActionResult InsuranceEdit(int InsuranceId)
        {
            ViewBag.CategoryList = GetCategory();
            return View(_unitOfWork.GetRepositoryInstance<AspNetInsurance>().GetFirstorDefault(InsuranceId));
        }

        [HttpPost]
        public ActionResult InsuranceEdit(AspNetInsurance tbl, HttpPostedFileBase file)
        {
            string pic = null;
            if (file != null)
            {
                pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/InsuranceImg/"), pic);
                // file is uploaded
                file.SaveAs(path);
            }
            tbl.InsuranceImage = file != null ? pic : tbl.InsuranceImage;
            var currentInsurance = db.AspNetInsurances.Find(tbl.InsuranceId);
            db.Entry(currentInsurance).CurrentValues.SetValues(tbl);
            db.SaveChanges();
            return RedirectToAction("Insurance");
        }

        [HttpPost]
        public ActionResult InsuranceDelete(int InsuranceId)
        {
            dbVervoerEntities dbcontext = new dbVervoerEntities();
            AspNetInsurance insurance = dbcontext.AspNetInsurances.Find(InsuranceId);
            dbcontext.AspNetInsurances.Remove(insurance);
            dbcontext.SaveChanges();
            return RedirectToAction("Insurance");
        }

        public ActionResult EditCategory(int categoryId)
        {
            return View(_unitOfWork.GetRepositoryInstance<AspNetCategory>().GetFirstorDefault(categoryId));
        }

        [HttpPost]
        public ActionResult EditCategory(AspNetCategory tbl)
        {
            _unitOfWork.GetRepositoryInstance<AspNetCategory>().Update(tbl);
            return RedirectToAction("Categories");
        }
        /*
        public ActionResult UpdateCategory(int categoryId = 0)
        {
            CategoryList categoryList;
            if (categoryId != null)
            {
                categoryList = JsonConvert.DeserializeObject<CategoryList>(JsonConvert.SerializeObject(_unitOfWork.GetRepositoryInstance<Tbl_Category>().GetFirstorDefault(categoryId)));
            }
            else
            {
                categoryList = new CategoryList();
            }

            return View("UpdateCategory", categoryList);

        }
        */

        public ActionResult Product()
        {
            return View(_unitOfWork.GetRepositoryInstance<AspNetProduct>().GetProduct());
        }

        public ActionResult ProductEdit(int productId)
        {
            ViewBag.CategoryList = GetCategory();
            return View(_unitOfWork.GetRepositoryInstance<AspNetProduct>().GetFirstorDefault(productId));
        }

        public ActionResult ProductAdd()
        {
            ViewBag.CategoryList = GetCategory();
            return View();
        }

        public ActionResult InsuranceAdd()
        {
            ViewBag.CategoryList = GetCategory();
            return View();
        }

        [HttpPost]
        public ActionResult ProductAdd(AspNetProduct tbl, HttpPostedFileBase file)
        {
            string pic = null;
            if (file != null)
            {
                pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/ProductImg/"), pic);
                // file is uploaded
                file.SaveAs(path);
            }
            tbl.ProductImage = pic;
            tbl.CreatedDate = DateTime.Now;
            _unitOfWork.GetRepositoryInstance<AspNetProduct>().Add(tbl);
            return RedirectToAction("Product");
        }

        [HttpPost]
        public ActionResult ProductEdit(AspNetProduct tbl, HttpPostedFileBase file)
        {
            string pic = null;
            if (file != null)
            {
                pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/ProductImg/"), pic);
                // file is uploaded
                file.SaveAs(path);
            }
            tbl.ProductImage = file != null ? pic : tbl.ProductImage;
            tbl.ModifiedDate = DateTime.Now;
            var currentProduct = db.AspNetProducts.Find(tbl.ProductId);
            db.Entry(currentProduct).CurrentValues.SetValues(tbl);
            db.SaveChanges();
            return RedirectToAction("Product");

        }

        [HttpPost]
        public ActionResult ProductDelete(int productId)
        {
            dbVervoerEntities dbcontext = new dbVervoerEntities();
            AspNetProduct product = dbcontext.AspNetProducts.Find(productId);
            dbcontext.AspNetProducts.Remove(product);
            dbcontext.SaveChanges();
            return RedirectToAction("Product");
        }

        public ActionResult Collab()
        {
            ViewBag.Token = GenerateToken();
            return View();
        }

        public string GenerateToken()
        {
            string key = "745fc6889d09430cb5d3dfc2ceeb78ca";
            string appID = "8840d1.vidyo.io";
            string userName = User.Identity.Name;
            long expiresInSecs = 1800;
            string expiresAt = null;

            long EPOCH_SECONDS = 62167219200;

            string delimStr = "=";
            char[] delimiter = delimStr.ToCharArray();

            if ((appID != null) && (key != null) && (userName != null))
            {
                string expires = "";

                // Check if using expiresInSecs or expiresAt
                if (expiresInSecs > 0)
                {
                    TimeSpan timeSinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                    expires = (Math.Floor(timeSinceEpoch.TotalSeconds) + EPOCH_SECONDS + expiresInSecs).ToString();
                }
                else if (expiresAt != null)
                {
                    try
                    {
                        TimeSpan epochToExpires = DateTime.Parse(expiresAt).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                        expires = (Math.Floor(epochToExpires.TotalSeconds) + EPOCH_SECONDS).ToString();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\nException caught in expiresAt time calculation. Time format probably invalid. Should look like so: 2055-10-27T10:54:22Z");
                        Console.WriteLine(e);
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("\nExiting! Neither expiresInSecs or expiresAt was set.");
                }

                Console.WriteLine("Setting key           : " + key);
                Console.WriteLine("Setting appId         : " + appID);
                Console.WriteLine("Setting userName      : " + userName);
                Console.WriteLine("Setting expiresInSecs : {0}", expiresInSecs);
                if (expiresAt != null)
                {
                    Console.WriteLine("Setting expiresAt     : " + expiresAt);
                }
                Console.WriteLine("Expirey time          : " + expires);
                string jid = userName + "@" + appID;
                string body = "provision" + "\0" + jid + "\0" + expires + "\0" + "";

                var encoder = new UTF8Encoding();
                var hmacsha = new HMACSHA384(encoder.GetBytes(key));
                byte[] mac = hmacsha.ComputeHash(encoder.GetBytes(body));

                // macBase64 can be used for debugging
                //string macBase64 = Convert.ToBase64String(hashmessage);

                // Get the hex version of the mac
                string macHex = BytesToHex(mac);

                string serialized = body + '\0' + macHex;

                return Convert.ToBase64String(encoder.GetBytes(serialized)).ToString();
            }
            else
            {
                return null;
            }
        }

        private static string BytesToHex(byte[] bytes)
        {
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }


        //Manage FAQ Page
        // GET: Test
        public ActionResult ManageFAQ()
        {

            {
                return View(db.FAQs.ToList());
            }

        }

        // GET: Test/Details/5
        public ActionResult ManageFAQDetails(int Id)
        {
            using (db)
            {
                return View(db.FAQs.Where(x => x.Id == Id).FirstOrDefault());
            }
        }

        // GET: Test/Create
        public ActionResult ManageFAQCreate()
        {
            return View();
        }

        // POST: Test/Create
        [HttpPost]
        public ActionResult ManageFAQCreate(FAQ faq)
        {
            try
            {
                // var isUsernameExist = UserManager.Users.Any(i => i.UserName == model.UserName);
              /*  var isIdExist = db.FAQs.Any(i => i.Id == faq.Id);
                if (isIdExist)
                {
                    ModelState.AddModelError("", "Id already exists!");
                    TempData["MyErrorMessage"] = "Id already exists bro!";
                    return View("ManageFAQ");

                }*/
                // TODO: Add insert logic here
                using (db)
                {
                    db.FAQs.Add(faq);
                    db.SaveChanges();
                }
                return RedirectToAction("ManageFAQ");
                //return View("ManageFAQ");

            }
            catch
            {
                return View();
            }
        }

        // GET: Test/Edit/5
        public ActionResult ManageFAQEdit(int Id)
        {
            using (db)
            {
                return View(db.FAQs.Where(x => x.Id == Id).FirstOrDefault());
            }
        }

        // POST: Test/Edit/5
        [HttpPost]
        public ActionResult ManageFAQEdit(int Id, FAQ faq)
        {
            try
            {
                // TODO: Add update logic here
                using (db)
                {
                    db.Entry(faq).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("ManageFAQ");
            }
            catch
            {
                return View();
            }
        }

        // GET: Test/Delete/5
        public ActionResult ManageFAQDelete(int Id)
        {
            using (db)
            {
                return View(db.FAQs.Where(x => x.Id == Id).FirstOrDefault());
            }
        }

        // POST: Test/Delete/5
        [HttpPost]
        public ActionResult ManageFAQDelete(int Id, FAQ faq)
        {
            try
            {
                // TODO: Add delete logic here
                using (db)
                {
                    faq = db.FAQs.Where(x => x.Id == Id).FirstOrDefault();
                    db.FAQs.Remove(faq);
                    db.SaveChanges();
                }
                return RedirectToAction("ManageFAQ");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult CompanyReports()
        {
            // return View(db.Reports.Where(x=> x.Month.Contains(searching) || searching == null).ToList());
            return View();
        }

        /*
        public ActionResult Report(string ReportType)
        {
            
            LocalReport localreport = new LocalReport();
            localreport.ReportPath = Server.MapPath("~/CompanyReports/CompanyReport.rdlc");
            ReportDataSource reportDataSource = new ReportDataSource();
            reportDataSource.Name = "CompanyReportDataSet1";
            reportDataSource.Value = db.Reports.ToList();
            localreport.DataSources.Add(reportDataSource);
            string reportType = ReportType;
            string mimeType;
            string encoding;
            string fileNameExtension;
            
            if (reportType == "PDF")
            {
                fileNameExtension = "pdf";
            }

            string[] streams;
          //  Warning[] warnings;
            byte[] renderedByte;
            renderedByte = localreport.Render(reportType, "", out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
            Response.AddHeader("content-disposition", "attachment:filename = company_report." + fileNameExtension);
            return File(renderedByte, fileNameExtension);
        }
        */

        public ActionResult ManageInsurance()
        {
            dbVervoerEntities db = new dbVervoerEntities();
            var myInsuranceClaim = db.InsuranceClaims.ToList();
            return View(myInsuranceClaim);
            //return View(_unitOfWork.GetRepositoryInstance<AdminExamQn>().GetAllRecords());
        }

        public ActionResult ViewClaimForm(string UID)
        {

            dbVervoerEntities db = new dbVervoerEntities();
           /* var claim = db.InsuranceClaims.Where(x => x.UserID == UID).FirstOrDefault();
            ClaimFormViewModel model = new ClaimFormViewModel();
            model.Address = claim.Address;
            model.AspNetUser = claim.AspNetUser;
            model.ClaimNo = claim.ClaimNo;
            model.ContactNo = claim.ContactNo;
            model.DateOfClaim = claim.DateOfClaim;
            model.DenialMessage = claim.DenialMessage;
            model.Id = claim.Id;
            model.NomineeName = claim.NomineeName;
            model.NomineeNRIC = claim.NomineeNRIC;
            model.PlanType = claim.PlanType;
            model.PolicyNo = claim.PolicyNo;
            model.Relationship = claim.Relationship;
            model.Signature = claim.Signature;
            model.Status = claim.Status;
            model.UserID = claim.UserID;
            model.VictimName = claim.VictimName;
            model.VictimNRIC = claim.VictimNRIC;*/

            //return View(model);
            return View(db.InsuranceClaims.Where(x => x.UserID == UID).FirstOrDefault());
        }

        [HttpPost]
        public ActionResult ApproveInsurance(InsuranceClaim claim)
        {

            dbVervoerEntities db = new dbVervoerEntities();
            string username = db.AspNetUsers.Where(i => i.Id == claim.UserID).Select(i => i.UserName).SingleOrDefault();
            InsuranceClaim userClaim = db.InsuranceClaims.Where(x => x.UserID == claim.UserID).FirstOrDefault();
            userClaim.Status = "Approved";
            userClaim.DenialMessage = null;
            db.SaveChanges();
            TempData["GreenMessage"] = "Insurance for the user : " + username + " is approved successfully!";
            return RedirectToAction("ManageInsurance");
        }

        [HttpPost]
        public ActionResult DenyInsurance(InsuranceClaim claim)
        {

            dbVervoerEntities db = new dbVervoerEntities();
            string username = db.AspNetUsers.Where(i => i.Id == claim.UserID).Select(i => i.UserName).SingleOrDefault();
            InsuranceClaim userClaim = db.InsuranceClaims.Where(x => x.UserID == claim.UserID).FirstOrDefault();
            userClaim.DenialMessage = claim.DenialMessage;
            userClaim.Status = "Denied";
            db.SaveChanges();
            TempData["RedMessage"] = "Insurance for the user : " + username + " has been denied!";
            return RedirectToAction("ManageInsurance");

        }

        public ActionResult ReportPMDAdmin()
        {
            return View(db.ReportPMDs.ToList());
        }

        public ActionResult ProductsChart()
        {
            var data = db.Reports.ToList();
            var chart = new Chart();
            var area = new ChartArea();
            chart.ChartAreas.Add(area);
            var series = new Series();
            foreach (var item in data)
            {
                series.Points.AddXY(item.ProductName, item.Quantity);
            }
            series.Label = "#PERCENT{P0}";
            series.Font = new Font("Arial", 8.0f, FontStyle.Bold);
            series.ChartType = SeriesChartType.Column;
            series["PieLabelStyle"] = "Outside";
            chart.Series.Add(series);
            var returnStream = new MemoryStream();
            chart.ImageType = ChartImageType.Png;
            chart.SaveImage(returnStream);
            returnStream.Position = 0;
            return new FileStreamResult(returnStream, "image/png");


        }



        public ActionResult LoadReports()
        {
            dbVervoerEntities db = new dbVervoerEntities();
            var listOfReports = db.Reports.ToList();
    

            return Json(new { data = listOfReports }, JsonRequestBehavior.AllowGet);
        }

    }
}