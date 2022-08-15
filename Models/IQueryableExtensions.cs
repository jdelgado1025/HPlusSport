using System.Linq.Expressions;

//Extension class by Christian Wenz
namespace HPlusSport.API.Models
{
    //Generates sort query based on parameters
    //using the appropriate LINQ command
    public static class IQueryableExtensions
    {
        //Can't use reflection with Linq to SQL; only Linq to Object
        //https://stackoverflow.com/questions/7265186/how-do-i-specify-the-linq-orderby-argument-dynamically/7265394#7265394
        public static IQueryable<TEntity> OrderByCustom<TEntity>(this IQueryable<TEntity> items, string sortBy, string sortOrder)
        {
            var type = typeof(TEntity);
            var property = type.GetProperty(sortBy);
            var parameter = Expression.Parameter(type, "t");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);
            var result = Expression.Call(
                typeof(Queryable),
                sortOrder == "desc" ? "OrderByDescending" : "OrderBy",
                new Type[] { type, property.PropertyType },
                items.Expression,
                Expression.Quote(lambda));

            return items.Provider.CreateQuery<TEntity>(result);
        }
    }
}
