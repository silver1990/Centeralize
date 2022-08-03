using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Raybod.SCM.Utility.Extention
{
    public static class EnumExtetions
    {
        public static string GetEnumDescription(this System.Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            if (fi == null) return string.Empty;
            var attributes =(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute),false);
            if (attributes != null &&attributes.Length > 0)
                return attributes[0].Description;
            return value.ToString();
        }

        public static string GetDisplayName(this System.Enum enumValue)
        {
            var fi =
               enumValue.GetType()
               .GetMember(enumValue.ToString())
               .First();
            if (fi == null) return string.Empty;

            var attribute = fi
                .GetCustomAttribute<DisplayAttribute>();
            if(attribute!=null ) return attribute.GetName();
            return string.Empty;

        }
    }
}
