using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using imgurplusbot.bll.Helpers.Attributes;
using System.Reflection;

namespace imgurplusbot.bll.Helpers.Extensions
{
    public static class EnumExtensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));
            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
        public static T Prev<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));
            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) - 1;
            return (j < 0) ? Arr[^1] : Arr[j];
        }

        public static string GetFormat(this Enum src)
        {
           return src
                .GetType()
                .GetMember(src.ToString())
                .Where(member => member.MemberType == MemberTypes.Field)
                .FirstOrDefault()
                .GetCustomAttributes<Format>(false)
                .SingleOrDefault()?.Value;
        }
    }
}
