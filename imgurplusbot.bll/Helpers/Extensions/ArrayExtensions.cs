using System.Linq;

namespace imgurplusbot.bll.Helpers.Extensions
{
    public static class ArrayExtensions
    {
        public static void Deconstruct<T>(this T[] array, out T first, out T[] rest)
        {
            first = array.Length > 0 ? array[0] : default(T);
            rest = array.Skip(1).ToArray();
        }
        public static void Deconstruct<T>(this T[] array, out T first, out T second, out T[] rest)
            => (first, (second, rest)) = array;
    }
}
