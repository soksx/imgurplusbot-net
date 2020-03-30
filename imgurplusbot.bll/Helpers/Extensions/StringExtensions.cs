using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace imgurplusbot.bll.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveCharXCharStr(this string str, string charReplace)
        {
            Parallel.ForEach(charReplace.ToCharArray(), (anychar) => { str = str.Replace(anychar.ToString(), string.Empty); });
            return str;
        }
    }
}
