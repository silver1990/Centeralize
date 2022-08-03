using Raybod.SCM.Domain.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Raybod.SCM.DataAccess.Extention
{
    public static class RepositoryExtension
    {
        public static IQueryable<T> ApplayOrdering<T>(this IQueryable<T> query, IQueryObject queryObj, Dictionary<string, Expression<Func<T, object>>> columnsMap)
        {
            if (string.IsNullOrEmpty(queryObj.SortBy) || !columnsMap.ContainsKey(queryObj.SortBy))
                return query;

            if (queryObj.IsSortAscending)
                return query.OrderBy(columnsMap[queryObj.SortBy]);
            else
                return query.OrderByDescending(columnsMap[queryObj.SortBy]);

        }

        public static IQueryable<T> ApplayPageing<T>(this IQueryable<T> query, IQueryObject queryObj)
        {
            if (queryObj.PageSize <= 0)
                queryObj.PageSize = 20;

            if (queryObj.Page <= 0)
                queryObj.Page = 1;

            return query.Skip((queryObj.Page - 1) * queryObj.PageSize).Take(queryObj.PageSize);
        }
       
        public static IQueryable<T> ApplayPageing<T>(this IQueryable<T> query, int page, int pageSize)
        {
            if (pageSize <= 0)
                pageSize = 20;

            if (page <= 0)
                page = 1;

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public static IEnumerable<T> ApplayPageing<T>(this IEnumerable<T> query, IQueryObject queryObj)
        {
            if (queryObj.PageSize <= 0)
                queryObj.PageSize = 20;

            if (queryObj.Page <= 0)
                queryObj.Page = 1;

            return query.Skip((queryObj.Page - 1) * queryObj.PageSize).Take(queryObj.PageSize);
        }
    }
}
