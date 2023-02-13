using System;
using System.Linq;

namespace ZMap;

public static class Methods
{
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