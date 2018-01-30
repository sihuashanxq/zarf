
namespace Zarf.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 是否null或空字符串
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string v)
            => string.IsNullOrEmpty(v);

        /// <summary>
        /// 是否null或空格
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string v)
            => string.IsNullOrWhiteSpace(v);

        /// <summary>
        ///  字符串转义 Id=>[Id]
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string Escape(this string v)
        {
            return "[" + v + "]";
        }
    }
}
