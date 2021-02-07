using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PMDWebApplication.DB;
//using PMDWebApplication.Models;
using PMDWebApplication.Repository;

namespace PMDWebApplication.Controllers
{
    [Authorize(Roles = "System Administrator")]
    public class DualDatabaseTestSchemeTestsController : Controller
    {
        //private DualDatabaseTestSchemeTestsDbContext db = new DualDatabaseTestSchemeTestsDbContext();

        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        public dbVervoerEntities db = new dbVervoerEntities();

        // GET: DualDatabaseTestSchemeTests

        [AllowAnonymous]
        public ActionResult Index()
        {
            
            return View(db.DualDatabaseTestSchemeTests.ToList());
        }

        // GET: DualDatabaseTestSchemeTests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DB.DualDatabaseTestSchemeTest dualDatabaseTestSchemeTest = db.DualDatabaseTestSchemeTests.Find(id);
            if (dualDatabaseTestSchemeTest == null)
            {
                return HttpNotFound();
            }

            //count questions
            ViewBag.countquestions = null;
            if(TempData["countquestions"] != null)
            {
                ViewBag.countquestions = (int)TempData["countquestions"];
            }




            return View(dualDatabaseTestSchemeTest);
        }

        // GET: DualDatabaseTestSchemeTests/Edit/5
        [Authorize(Roles = "System Administrator")]

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DB.DualDatabaseTestSchemeTest dualDatabaseTestSchemeTest = db.DualDatabaseTestSchemeTests.Find(id);
            if (dualDatabaseTestSchemeTest == null)
            {
                return HttpNotFound();
            }
            return View(dualDatabaseTestSchemeTest);
        }

        // POST: DualDatabaseTestSchemeTests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(bool Public, [Bind(Include = "Name,Description,Genre")] DB.DualDatabaseTestSchemeTest dualDatabaseTestSchemeTest)
        {
            if (ModelState.IsValid)
            {
                dualDatabaseTestSchemeTest.Status = Public;
                dualDatabaseTestSchemeTest.DateModified = DateTime.Now.ToString();
                db.Entry(dualDatabaseTestSchemeTest).State = EntityState.Modified;
                db.SaveChanges();
                //_unitOfWork.GetRepositoryInstance<DualDatabaseTestSchemeTest>().Update(dualDatabaseTestSchemeTest);
                return RedirectToAction("PersonalProfile");
            }
            return View(dualDatabaseTestSchemeTest);
        }

        // GET: DualDatabaseTestSchemeTests/Delete/5
        [Authorize(Roles = "System Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DB.DualDatabaseTestSchemeTest dualDatabaseTestSchemeTest = db.DualDatabaseTestSchemeTests.Find(id);
            if (dualDatabaseTestSchemeTest == null)
            {
                return HttpNotFound();
            }
            return View(dualDatabaseTestSchemeTest);
        }

        // POST: DualDatabaseTestSchemeTests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "System Administrator")]
        public ActionResult DeleteConfirmed(int id)
        {
            DB.DualDatabaseTestSchemeTest dualDatabaseTestSchemeTest = db.DualDatabaseTestSchemeTests.Find(id);
            db.DualDatabaseTestSchemeTests.Remove(dualDatabaseTestSchemeTest);
            db.SaveChanges();
            return RedirectToAction("PersonalProfile");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //new additions
        [Authorize(Roles = "System Administrator")]
        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "System Administrator")]
        public ActionResult Create(bool Public, [Bind(Include = "Name,Description,Genre")] DB.DualDatabaseTestSchemeTest newTest)
        {

            //Random is no longer necessary
            //newTest.ID = random.Next(1, 50000);
            newTest.Owner = System.Web.HttpContext.Current.User.Identity.Name;
            newTest.DateModified = DateTime.Now.ToString();
            newTest.Status = Public;



            db.DualDatabaseTestSchemeTests.Add(newTest);
            db.SaveChanges();
            /*
            db.DualDatabaseTestSchemeTestDataBase.Attach(newTest);
            db.Entry(newTest).State = EntityState.Modified;
            db.SaveChanges();*/

            return RedirectToAction("PersonalProfile");
        }
        [Authorize(Roles = "System Administrator")]
        public ActionResult CreateAdvanced()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdvanced(bool Public, [Bind(Include = "Name,Description,Genre")] DB.DualDatabaseTestSchemeTest newTest)
        {

            //Random is no longer necessary
            //newTest.ID = random.Next(1, 50000);
            newTest.Owner = System.Web.HttpContext.Current.User.Identity.Name;
            newTest.DateModified = DateTime.Now.ToString();
            newTest.Status = Public;
            newTest.IsAdvanced = true;



            db.DualDatabaseTestSchemeTests.Add(newTest);
            db.SaveChanges();
            /*
            db.DualDatabaseTestSchemeTestDataBase.Attach(newTest);
            db.Entry(newTest).State = EntityState.Modified;
            db.SaveChanges();*/

            return RedirectToAction("Index");
        }

        //legacy function
        public ActionResult CreateTestObject(bool Public, [Bind(Include = "Name,Description,Genre")] DB.DualDatabaseTestSchemeTest newTest)
        {

            //Random is no longer necessary
            //newTest.ID = random.Next(1, 50000);
            newTest.Owner = System.Web.HttpContext.Current.User.Identity.Name;
            newTest.DateModified = DateTime.Now.ToString();
            newTest.Status = Public;



            db.DualDatabaseTestSchemeTests.Add(newTest);
            db.SaveChanges();
            /*
            db.DualDatabaseTestSchemeTestDataBase.Attach(newTest);
            db.Entry(newTest).State = EntityState.Modified;
            db.SaveChanges();*/

            return RedirectToAction("Index");
        }


        public ActionResult AddNewQuestion(int? id)
        {
            ViewBag.ID = id;
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult AllTests()
        {
            //var listOfExams = db.DualDatabaseTestSchemeTests.Select(e => e.Status == true);
            if (db.DualDatabaseTestSchemeTests.ToList().Where(e => e.Status == true) == null)
            {
                RedirectToAction("Index", "Quizbank");
            }

            return View(db.DualDatabaseTestSchemeTests.ToList().Where(e => e.Status == true));
        }

        public ActionResult AllTestsPublic(string searchString = "")
        {
            var TestsSelected = from m in db.DualDatabaseTestSchemeTests select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                TestsSelected = TestsSelected.Where(s => s.Name.Contains(searchString));
            }

            //return View(db.DualDatabaseTestSchemeTestDataBase.ToList());
            return View(TestsSelected);
        }

        public ActionResult PersonalProfile()
        {
            return View(db.DualDatabaseTestSchemeTests.ToList());
        }


    }
}


