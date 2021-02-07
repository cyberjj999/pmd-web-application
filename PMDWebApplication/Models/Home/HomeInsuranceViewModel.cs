using PagedList;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using PMDWebApplication.DB;
using PMDWebApplication.Repository;

namespace PMDWebApplication.Models.Home
{
    public class HomeInsuranceViewModel
    {
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        dbVervoerEntities context = new dbVervoerEntities();

        public IPagedList<AspNetInsurance> ListOfInsurance { get; set; }

        public HomeInsuranceViewModel CreateModel(string searchByInsurance, int pageSize, int? page)
        {
            SqlParameter[] param = new SqlParameter[]
            {
                new SqlParameter("@search", searchByInsurance??(object)DBNull.Value)
            };

            IPagedList<AspNetInsurance> data = context.Database.SqlQuery<AspNetInsurance>("GetBySearchInsurance @search", param).ToList().ToPagedList(page ?? 1, pageSize);
            return new HomeInsuranceViewModel
            {
                ListOfInsurance = data
            };
        }
    }
}