using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using PMDWebApplication.DB;

namespace PMDWebApplication.Repository
{
    public class GenericRepository<Tbl_Entity> : IRepository<Tbl_Entity> where Tbl_Entity : class
    {
            DbSet<Tbl_Entity> _dbSet;

            private dbVervoerEntities _DBEntity;

            public GenericRepository(dbVervoerEntities DBEntity)
            {
                _DBEntity = DBEntity;
                _dbSet = _DBEntity.Set<Tbl_Entity>();
            }
            public IEnumerable<Tbl_Entity> GetProduct()
            {
                return _dbSet.ToList();
            }
            public void Add(Tbl_Entity entity)
            {
                _dbSet.Add(entity);
                _DBEntity.SaveChanges();
            }

            public int GetAllrecordCount()
            {
                return _dbSet.Count();
            }

            public IEnumerable<Tbl_Entity> GetAllRecords()
            {
                return _dbSet.ToList();
            }

            public IQueryable<Tbl_Entity> GetAllRecordsIQueryable()
            {
                return _dbSet;
            }

            public Tbl_Entity GetFirstorDefault(int recordId)
            {
                return _dbSet.Find(recordId);
            }

            public Tbl_Entity GetFirstorDefaultByParameter(Expression<Func<Tbl_Entity, bool>> wherePredict)
            {
                return _dbSet.Where(wherePredict).FirstOrDefault();
            }

            public IEnumerable<Tbl_Entity> GetListParameter(Expression<Func<Tbl_Entity, bool>> wherePredict)
            {
                return _dbSet.Where(wherePredict).ToList();
            }

            public void InactiveAndDeleteMarkByWhereClause(Expression<Func<Tbl_Entity, bool>> wherePredict, Action<Tbl_Entity> ForEachPredict)
            {
                _dbSet.Where(wherePredict).ToList().ForEach(ForEachPredict);
            }

            public void Remove(Tbl_Entity entity)
            {
                if (_DBEntity.Entry(entity).State == EntityState.Detached)
                    _dbSet.Attach(entity);
                _dbSet.Remove(entity);
            }

            public void RemoveRangeBywhereClause(Expression<Func<Tbl_Entity, bool>> wherePredict)
            {
                List<Tbl_Entity> entity = _dbSet.Where(wherePredict).ToList();
                foreach (var ent in entity)
                {
                    Remove(ent);
                }
            }

            public void Update(Tbl_Entity entity)
            {
                _DBEntity.Entry(entity).State = EntityState.Modified;
                _dbSet.Attach(entity);
                _DBEntity.SaveChanges();
            }

            public void UpdateCart(AspNetCart cart)
            {
            var entity = _DBEntity.AspNetCarts.Where(c => c.CartId == cart.CartId).AsQueryable().FirstOrDefault();
            if(entity == null)
            {
                _DBEntity.AspNetCarts.Add(cart);
            }
            else
            {
                _DBEntity.Entry(entity).CurrentValues.SetValues(cart);
            }
                _DBEntity.SaveChanges();
            }

            public void UpdateByWhereClause(Expression<Func<Tbl_Entity, bool>> wherePredict, Action<Tbl_Entity> ForEachPredict)
            {
                _dbSet.Where(wherePredict).ToList().ForEach(ForEachPredict);
            }

            public IEnumerable<Tbl_Entity> GetRecordsToShow(int PageNo, int PageSize, int CurrentPage, Expression<Func<Tbl_Entity, bool>> wherePredict, Expression<Func<Tbl_Entity, int>> orderByPredict)
            {
                if (wherePredict != null)
                {
                    return _dbSet.OrderBy(orderByPredict).Where(wherePredict).ToList();

                }

                else
                {
                    return _dbSet.OrderBy(orderByPredict).ToList();
                }
            }

            public void RemoveByWhereClause(Expression<Func<Tbl_Entity, bool>> wherePredict)
            {
                Tbl_Entity entity = _dbSet.Where(wherePredict).FirstOrDefault();
                Remove(entity);
            }

            public IEnumerable<Tbl_Entity> GetResultsBySqlProcedure(string query, params object[] parameters)
            {
                if (parameters != null)
                {
                    return _DBEntity.Database.SqlQuery<Tbl_Entity>(query, parameters).ToList();
                }
                else
                    return _DBEntity.Database.SqlQuery<Tbl_Entity>(query).ToList();
            }

        public void UpdateProduct(AspNetProduct Product)
        {
            var entity = _DBEntity.AspNetProducts.Where(c => c.ProductId == Product.ProductId).AsQueryable().FirstOrDefault();
            if (entity == null)
            {
                _DBEntity.AspNetProducts.Add(Product);
            }
            else
            {
                _DBEntity.Entry(entity).CurrentValues.SetValues(Product);
            }
            _DBEntity.SaveChanges();
        }

        public void UpdateInsurance(AspNetInsurance Insurance)
        {
            var entity = _DBEntity.AspNetCartInsurances.Where(c => c.InsuranceId == Insurance.InsuranceId).AsQueryable().FirstOrDefault();
            if (entity == null)
            {
                _DBEntity.AspNetInsurances.Add(Insurance);
            }
            else
            {
                _DBEntity.Entry(entity).CurrentValues.SetValues(Insurance);
            }
            _DBEntity.SaveChanges();
        }
    }
}