namespace Raybod.SCM.Domain.Extention
{
    public static class DomainExtention
    {
        public static string TryTrim(this string source)
        {
            return source?.Trim();
        }
    }
}
