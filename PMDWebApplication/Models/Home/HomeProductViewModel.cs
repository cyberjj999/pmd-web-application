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
        public class HomeProductViewModel
        {
            public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
            dbVervoerEntities context = new dbVervoerEntities();

            public IPagedList<AspNetProduct> ListOfProducts { get; set; }

        public HomeProductViewModel CreateModel(string search,int pageSize, int? page)
        {
            SqlParameter[] param = new SqlParameter[]
            {
                new SqlParameter("@search", search??(object)DBNull.Value)
            };

            IPagedList<AspNetProduct> data = context.Database.SqlQuery<AspNetProduct>("GetBySearch @search", param).ToList().ToPagedList(page ?? 1, pageSize);
            return new HomeProductViewModel
            {
                ListOfProducts = data
                };
            }
        }

}