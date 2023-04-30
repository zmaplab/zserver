using System;
using System.Linq;

namespace ZMap;

public static class Functions
{
    public static bool GreaterThanOrEqualTo(object v1, object v2)
    {
        if (v1 is not IComparable comparable1)
        {
            return false;
        }

        var type = v1.GetType();
        var comparable2 = TryConvert(v2, type);
        return comparable2 != null && comparable1.CompareTo(comparable2) >= 0;
    }

    public static bool Between(object v1, object v2, object v3)
    {
        if (v1 is not IComparable comparable)
        {
            return false;
        }

        var type = v1.GetType();
        var comparable1 = TryConvert(v2, type);
        var comparable2 = TryConvert(v3, type);

        //  v1 > v2  && v1 < v3
        return comparable1 != null && comparable2 != null && comparable.CompareTo(comparable1) > 0 &&
               comparable.CompareTo(comparable2) < 0;
    }

    public static bool Like(object v1, object like, string wildCard, string singleChar, string escapeChar)
    {
        if (v1 == null)
        {
            return false;
        }

        if (like == null)
        {
            return false;
        }

        var sV1 = v1.ToString();
        var sLike = like.ToString();

        // TODO: contains 更快， 还是正则？
        return sV1.Contains(sLike);
    }

    public static bool EqualTo(object v1, object v2)
    {
        if (v1 is not IComparable comparable1)
        {
            return false;
        }

        var type = v1.GetType();
        var comparable2 = TryConvert(v2, type);

        return comparable2 != null && comparable1.CompareTo(comparable2) == 0;
    }

    public static bool GreaterThan(object v1, object v2)
    {
        if (v1 is not IComparable comparable1)
        {
            return false;
        }

        var type = v1.GetType();
        var comparable2 = TryConvert(v2, type);

        return comparable2 != null && comparable1.CompareTo(comparable2) > 0;
    }

    public static bool LessThan(object v1, object v2)
    {
        if (v1 is not IComparable comparable1)
        {
            return false;
        }

        var type = v1.GetType();
        var comparable2 = TryConvert(v2, type);

        return comparable2 != null && comparable1.CompareTo(comparable2) < 0;
    }

    public static bool LessThanOrEqualTo(object v1, object v2)
    {
        if (v1 is not IComparable comparable1)
        {
            return false;
        }

        var type = v1.GetType();
        var comparable2 = TryConvert(v2, type);

        return comparable2 != null && comparable1.CompareTo(comparable2) <= 0;
    }

    public static bool NotEqualTo(object v1, object v2)
    {
        if (v1 is not IComparable comparable1)
        {
            return false;
        }

        var type = v1.GetType();
        var comparable2 = TryConvert(v2, type);

        return comparable2 != null && comparable1.CompareTo(comparable2) != 0;
    }

    // private static List<IComparable> Eval(dynamic v1, dynamic v2)
    // {
    //     // 两个都为空
    //     if (v1 == null || v2 == null)
    //     {
    //         return new List<IComparable> { null, null };
    //     }
    //
    //     // TODO: 
    //     // var result = Eval2(v1, v2);
    //     // if (result != null)
    //     // {
    //     //     return result;
    //     // }
    //
    //     var type1 = (Type)v1.GetType();
    //     var type2 = (Type)v2.GetType();
    //
    //     // 若类型相同
    //     if (type1 == type2)
    //     {
    //         return v1 is IComparable
    //             ? new List<IComparable> { v1, v2 }
    //             : new List<IComparable> { v1.ToString(), v2.ToString() };
    //     }
    //
    //     if (type1.IsValueType)
    //     {
    //         if (v1 is IComparable)
    //         {
    //             if (TryConvert(v2, type1, out IComparable covert1))
    //             {
    //                 return new List<IComparable> { v1, covert1 };
    //             }
    //         }
    //     }
    //
    //     if (type2.IsValueType)
    //     {
    //         if (v2 is IComparable)
    //         {
    //             if (TryConvert(v1, type2, out IComparable covert2))
    //             {
    //                 return new List<IComparable> { covert2, v2 };
    //             }
    //         }
    //     }
    //
    //     if (v1 is IComparable)
    //     {
    //         if (TryConvert(v2, type1, out IComparable covert1))
    //         {
    //             return new List<IComparable> { v1, covert1 };
    //         }
    //     }
    //     else if (v2 is IComparable)
    //     {
    //         if (TryConvert(v1, type2, out IComparable covert2))
    //         {
    //             return new List<IComparable> { covert2, v2 };
    //         }
    //     }
    //
    //
    //     return new List<IComparable> { v1.ToString(), v2.ToString() };
    // }

    private static IComparable TryConvert(object value, Type t)
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            var result = Convert.ChangeType(value, t) as IComparable;
            return result;
        }
        catch
        {
            try
            {
                var result = Convert.ChangeType(value.ToString(), t) as IComparable;
                return result;
            }
            catch
            {
                return null;
            }
        }
    }

    public static double Categorize(double v, double[] array)
    {
        if (array.Length % 2 == 0)
        {
            throw new ArgumentException("分类函数的数组参数长度不正确");
        }

        for (var i = 0; i < array.Length; ++i)
        {
            var isEvenNumber = i % 2 == 0;
            if (isEvenNumber)
            {
                continue;
            }

            var value = array[i - 1];
            var limiter = array[i];
            if (v <= limiter)
            {
                return value;
            }
        }

        return array.Last();
    }
}