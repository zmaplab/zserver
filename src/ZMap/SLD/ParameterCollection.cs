using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ZMap.SLD;

public abstract class ParameterCollection
{
    /// <summary>
    /// TODO: 是否需要使用并发字典， 肯定会有性能损失
    /// </summary>
    private readonly ConcurrentDictionary<string, dynamic> _cache;

    protected ParameterCollection()
    {
        _cache = new ConcurrentDictionary<string, dynamic>();
    }

    protected abstract T GetDefault<T>(string name);

    /// <summary>
    /// 
    /// </summary>

    [XmlElement("SvgParameter", typeof(SvgParameter))]
    [XmlElement("CssParameter", typeof(CssParameter))]
    public List<NamedParameter> Parameters { get; set; } = new();

    protected T[] GetArray<T>(string name)
    {
        if (_cache.ContainsKey(name))
        {
            return _cache[name];
        }

        var parameter = Parameters.FirstOrDefault(x => x.Name == name);
        if (parameter == null)
        {
            _cache.TryAdd(name, Array.Empty<T>());
            return default;
        }
        else
        {
            var text = parameter.Text;
            var pieces = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (typeof(T) == typeof(string))
            {
                _cache.TryAdd(name, pieces);
                return pieces as T[];
            }
            else
            {
                var result = pieces.Select(x => (T)Convert.ChangeType(x, typeof(T))).ToArray();
                _cache.TryAdd(name, result);
                return result;
            }
        }
    }

    protected ParameterValue GetParameterValue<T>(string name)
    {
        if (_cache.ContainsKey(name))
        {
            return _cache[name];
        }

        var parameter = Parameters.FirstOrDefault(x => x.Name == name);

        if (parameter == null)
        {
            var value = GetDefault<T>(name);

            var parameterValue = new ParameterValue
            {
                Text = value?.ToString()
            };
            _cache.TryAdd(name, parameterValue);
            return parameterValue;
        }
        else
        {
            _cache.TryAdd(name, parameter);
            return parameter;
        }
    }

    protected ZMap.Style.Expression<T> Accept<T>(ParameterValue parameterValue, string name, IStyleVisitor visitor,
        object extraData)
    {
        if (parameterValue == null || (parameterValue.Text == null && parameterValue.Expression == null))
        {
            return ZMap.Style.Expression<T>.New(GetDefault<T>(name));
        }

        if (parameterValue.Text != null)
        {
            var value = typeof(T) == typeof(string)
                ? parameterValue.Text
                : Convert.ChangeType(parameterValue.Text, typeof(T));
            return ZMap.Style.Expression<T>.New(value == null ? default : (T)value);
        }

        visitor.Visit(parameterValue, extraData);
        return visitor.Pop();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterValue"></param>
    /// <param name="name"></param>
    /// <param name="visitor"></param>
    /// <param name="extraData"></param>
    /// <returns></returns>
    protected ZMap.Style.Expression<T[]> GetArrayExpression<T>(ParameterValue parameterValue, string name,
        IStyleVisitor visitor, object extraData)
    {
        var expression = Accept<string>(parameterValue, name, visitor, extraData);
        if (expression == null)
        {
            return null;
        }

        var type = typeof(T);

        if (expression.Value != null)
        {
            return ZMap.Style.Expression<T[]>.New(expression.Value
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (T)Convert.ChangeType(x, type)).ToArray());
        }

        var typeName = type.FullName;
        var separator = "new[] { ' ' }";

        return !string.IsNullOrWhiteSpace(expression.Body)
            ? ZMap.Style.Expression<T[]>.New(null,
                $"var v = {expression.Body}?.ToString() as string; return v?.Split({separator}, System.StringSplitOptions.RemoveEmptyEntries).Select(x => ({typeName})System.Convert.ChangeType(x, typeof({typeName}))).ToArray();")
            : null;
    }

    protected T Get<T>(string name)
    {
        var type = typeof(T);
        if (type.IsGenericType)
        {
            throw new ArgumentException("不支持泛型数据转换");
        }

        if (_cache.ContainsKey(name))
        {
            return _cache[name];
        }

        var text = Parameters.FirstOrDefault(x => x.Name == name)?.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            var result = GetDefault<T>(name);
            _cache.TryAdd(name, result);
            return result;
        }
        else
        {
            var obj = typeof(T) == typeof(string)
                ? text
                : Convert.ChangeType(text, typeof(T));
            _cache.TryAdd(name, obj);
            return obj == null ? default : (T)obj;
        }
    }
}