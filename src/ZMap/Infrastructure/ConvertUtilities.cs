using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZMap.Infrastructure;

public static class ConvertUtilities
{
    public static T ToObject<T>(dynamic v)
    {
        if (v == null)
        {
            return default;
        }

        var valueType = v.GetType();
        var targetType = typeof(T);
        if (valueType == targetType)
        {
            return v;
        }
        else
        {
            if (targetType.IsGenericType
                && (targetType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }

            if (targetType == null)
            {
                return default;
            }

            return targetType == typeof(string) ? (T)v.ToString() : (T)Convert.ChangeType(v, targetType);
        }
    }

    public static T[] ToArray<T>(string text)
    {
        var type = typeof(T);
        return ToArray(text, type).Select(x => x == null ? default : (T)x).ToArray();
    }

    public static IEnumerable<object> ToArray(string text, Type type)
    {
        
        var data = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        return data.Select(x => type == typeof(string) ? x : Convert.ChangeType(x, type));
    }
}