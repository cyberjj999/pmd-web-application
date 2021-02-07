using System.Web;
using System.Web.Mvc;

namespace PMDWebApplication
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {

            filters.Add(new HandleErrorAttribute());

            //This 2 is used for password expiry checks
            filters.Add(new AuthorizeAttribute());
            filters.Add(new PasswordAgeGlobalFilter());
            //The logic for the check is inside the PasswordAgeGlobalFilter file
            //Check Global.asax.cs as well, I've added Filter Exception there to make login page immune to the filter (prevent constant refrehsment)
        }
    }
}
