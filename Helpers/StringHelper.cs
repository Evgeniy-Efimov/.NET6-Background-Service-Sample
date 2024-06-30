namespace BackgroundService.Helpers
{
    public static class StringHelper
    {
        public static string NormalizeString(this string str, bool toUpper = true)
        {
            var result = (str ?? string.Empty).Trim().Replace(" ", string.Empty);

            result = toUpper ? result.ToUpper() : result.ToLower();

            return result;
        }

        public static string GetSubstring(this string str, string endStr)
        {
            str = (str ?? string.Empty);
            var endIndex = str.IndexOf(endStr);

            if (endIndex >= 0)
            {
                return str.Substring(0, endIndex);
            }

            return str;
        }
    }
}
