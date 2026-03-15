namespace CajunLyrics.Lib.Helpers
{
    public static class ListExtensions
    {
        public static void AddIfNotNullOrEmpty(this List<string> list, string? value, string parameterName)
        {
            if (!string.IsNullOrEmpty(value))
            {
                list.Add($"{parameterName}={value}");
            }
        }
    }
}