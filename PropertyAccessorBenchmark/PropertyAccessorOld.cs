using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
/// <summary>
/// The <c>PropertyAccessor</c> class provides methods for dynamically getting and setting properties of objects.
/// </summary>
public static class PropertyAccessorOld
    {
        private static readonly ConcurrentDictionary<KeyValuePair<Type, string>, Func<object, object>> _cachedGetMethods 
            = new ConcurrentDictionary<KeyValuePair<Type, string>, Func<object, object>>();

        private static readonly ConcurrentDictionary<KeyValuePair<Type, string>, Action<object, object>> _cachedSetMethods
            = new ConcurrentDictionary<KeyValuePair<Type, string>, Action<object, object>>();

        /// <summary>
        /// Gets the value of a specified property from the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object from which to retrieve the property value.</param>
        /// <param name="propertyName">The name of the property to retrieve.</param>
        /// <returns>The value of the specified property.</returns>
        public static object GetProperty<T>(T obj, string propertyName)
        {
            var typeProperty = new KeyValuePair<Type, string>(typeof(T), propertyName);
            return _cachedGetMethods.GetOrAdd(typeProperty, (tp) => CreateGetMethod<T>(tp.Value)).Invoke(obj);
        }

        /// <summary>
        /// Sets the value of a specified property on the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">The value to set for the specified property.</param>
        public static void SetProperty<T>(T obj, string propertyName, object value)
        {
            var typeProperty = new KeyValuePair<Type, string>(typeof(T), propertyName);
            _cachedSetMethods.GetOrAdd(typeProperty, (tp) => CreateSetMethod<T>(tp.Value)).Invoke(obj, value);
        }

        private static Action<object, object> CreateSetMethod<T>(string propertyName)
        {
            ParameterExpression objParamExpr = Expression.Parameter(typeof(T));
            ParameterExpression propParamExpr = Expression.Parameter(typeof(object));

            MemberExpression propertyExpr = Expression.Property(objParamExpr, propertyName);
            UnaryExpression convertedValueExpr = Expression.Convert(propParamExpr, propertyExpr.Type);
            BinaryExpression assignExpr = Expression.Assign(propertyExpr, convertedValueExpr);

            Expression<Action<T, object>> lambdaExpr = Expression.Lambda<Action<T, object>>(assignExpr, objParamExpr, propParamExpr);
            var method = lambdaExpr.Compile();
            return (obj, val) => method.Invoke((T)obj, val);
        }

        private static Func<object, object> CreateGetMethod<T>(string propertyName)
        {
            ParameterExpression objParamExpr = Expression.Parameter(typeof(T));

            MemberExpression propertyExpr = Expression.Property(objParamExpr, propertyName);
            UnaryExpression convertedValueExpr = Expression.Convert(propertyExpr, typeof(object));

            Expression<Func<T, object>> lambdaExpr = Expression.Lambda<Func<T, object>>(convertedValueExpr, objParamExpr);
            var method = lambdaExpr.Compile();
            return (obj) => method.Invoke((T)obj);
        }
    }