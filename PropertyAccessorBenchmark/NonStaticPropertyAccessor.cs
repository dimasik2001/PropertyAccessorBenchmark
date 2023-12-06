using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PropertyAccessorBenchmark
{
    public class NonStaticPropertyAccessor<T>
    {
        private readonly ConcurrentDictionary<string, Func<T, object>> _cachedGetMethods
            = new ConcurrentDictionary<string, Func<T, object>>();

        private readonly ConcurrentDictionary<string, Action<T, object>> _cachedSetMethods
            = new ConcurrentDictionary<string, Action<T, object>>();


        public object GetProperty(T obj, string propertyName)
        {
            return _cachedGetMethods.GetOrAdd(propertyName, CreateGetMethod).Invoke(obj);
        }

        public void SetProperty(T obj, string propertyName, object value)
        {
            _cachedSetMethods.GetOrAdd(propertyName, CreateSetMethod).Invoke(obj, value);
        }

        private Action<T, object> CreateSetMethod(string propertyName)
        {
            ParameterExpression objParamExpr = Expression.Parameter(typeof(T));
            ParameterExpression propParamExpr = Expression.Parameter(typeof(object));

            MemberExpression propertyExpr = Expression.Property(objParamExpr, propertyName);
            UnaryExpression convertedValueExpr = Expression.Convert(propParamExpr, propertyExpr.Type);
            BinaryExpression assignExpr = Expression.Assign(propertyExpr, convertedValueExpr);

            Expression<Action<T, object>> lambdaExpr = Expression.Lambda<Action<T, object>>(assignExpr, objParamExpr, propParamExpr);
            var method = lambdaExpr.Compile();
            return method;
        }

        private Func<T, object> CreateGetMethod(string propertyName)
        {
            ParameterExpression objParamExpr = Expression.Parameter(typeof(T));

            MemberExpression propertyExpr = Expression.Property(objParamExpr, propertyName);
            UnaryExpression convertedValueExpr = Expression.Convert(propertyExpr, typeof(object));

            Expression<Func<T, object>> lambdaExpr = Expression.Lambda<Func<T, object>>(convertedValueExpr, objParamExpr);
            var method = lambdaExpr.Compile();
            return method;
        }
    }
}
