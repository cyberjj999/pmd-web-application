using NLog;
using System;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;

namespace PMDWebApplication.Controllers
{

    [HandleError]
    public class ErrorController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET: Error
        public ActionResult NotFound()
        {

            logger.Error("ERROR 404 - User navigate to invalid page!" + "|" + "" + "|" + "" + "|===");
            return View();
        }

        public ActionResult Index(HandleErrorInfo info)
        {
            MessageBox.Show(info.ActionName);
            MessageBox.Show(info.ControllerName);

            return View();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            logger.Error(filterContext.Exception.ToString() + "|" + "" + "|" + "" + "|===");
            MessageBox.Show(filterContext.Exception.ToString());
        }

    }
}