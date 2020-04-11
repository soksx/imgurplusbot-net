using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace imgurplusbot.bll.Helpers.Extensions
{
    public static class AttributeExtensions
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }
        public static TValue GetAttributeValue<TAttribute, TValue>(this MethodBase methodBase, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var att = methodBase.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }
    }
}
