using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Raybod.SCM.Services.Core.Common
{
    public class ServiceUtility
    {
        private static Expression CreatePropertyLambdaExpression<TEntity>(string property)
        {
            Type type = typeof(TEntity);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            PropertyInfo pi = type.GetProperty(property);
            expr = Expression.Property(expr, pi);
            type = pi.PropertyType;
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(TEntity), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            return lambda;
        }

        public List<Expression> CreatePropertiesLambdaExpressions<TEntity>(string[] properties)
        {
            List<Expression> propertiesLambdaExpressions = new List<Expression>();
            foreach (var property in properties)
            {
                propertiesLambdaExpressions.Add(CreatePropertyLambdaExpression<TEntity>(property));
            }

            return propertiesLambdaExpressions;
        }

        public static string[] GetEntityProperties(object entity, params string[] exceptProperties)
        {
            // Implement this method without Where for more performance.
            var propertyInfos = entity.GetType().GetProperties().Where(p => !p.GetGetMethod().IsVirtual).ToArray();
            var properties = new string[propertyInfos.Length - exceptProperties.Length];
            byte counter = 0;
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                string propertyName = propertyInfos[i].Name;
                if (!exceptProperties.Contains(propertyName)/* && propertyInfos[i].GetMethod.IsFinal*/)
                    properties[counter++] = propertyName;
            }
            return properties;
        }
        public static int GenerateRandomNo()
        {
            var _min = 1000;
            var _max = 9999;
            var _rdm = new Random();
            return _rdm.Next(_min, _max);
        }
    }
}
