using System;
using System.Linq;

namespace ZMap.Utilities;

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
            return targetType == typeof(string) ? (T)v.ToString() : (T)Convert.ChangeType(v, targetType);
        }
    }

    public static T[] ToArray<T>(string text)
    {
        var pieces = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        if (typeof(T) == typeof(string))
        {
            return pieces as T[];
        }
        else
        {
            var result = pieces.Select(x => (T)Convert.ChangeType(x, typeof(T))).ToArray();
            return result;
        }
    }

    public static dynamic ToArray(string text, Type type)
    {
        var pieces = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        if (type == typeof(string))
        {
            return pieces;
        }
        else
        {
            var result = pieces.Select(x => Convert.ChangeType(x, type)).ToArray();
            return result;
        }
    }
}