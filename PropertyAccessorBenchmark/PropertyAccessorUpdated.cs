using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
/// <summary>
/// The <c>PropertyAccessor</c> class provides methods for dynamically getting and setting properties of objects.
/// </summary>
public static class PropertyAccessorUpdated
{
   
    /// <summary>
    /// Gets the value of a specified property from the given object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object from which to retrieve the property value.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The value of the specified property.</returns>
    public static object GetProperty<T>(T obj, string propertyName)
    {
        return PropertyAccessorNested<T>.Instance.GetProperty(obj, propertyName);
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
        PropertyAccessorNested<T>.Instance.SetProperty(obj, propertyName, value);
    }

    private class PropertyAccessorNested<T>
    {
        private readonly ConcurrentDictionary<string, Func<T, object>> _cachedGetMethods
            = new ConcurrentDictionary<string, Func<T, object>>();

        private readonly ConcurrentDictionary<string, Action<T, object>> _cachedSetMethods
            = new ConcurrentDictionary<string, Action<T, object>>();
        public static PropertyAccessorNested<T> Instance { get; } = new PropertyAccessorNested<T>();
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