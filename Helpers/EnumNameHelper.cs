using System;
using System.Linq;
using System.Reflection;

namespace InventoryApp.Helpers
{
    public static class EnumNameHelper
    {
        public static string GetDisplayName<T>(this T enumerationValue)
            where T : struct
        {
            //First judge whether it is enum type data
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", nameof(enumerationValue));
            }

            //Find the corresponding Display Name for the enum
            MemberInfo[] member = type.GetMember(enumerationValue.ToString());
            if (member != null && member.Length > 0)
            {
                var AttributesData = member[0].GetCustomAttributesData().FirstOrDefault();

                if (AttributesData != null)
                {
                    //Pull out the value
                    return AttributesData.NamedArguments.FirstOrDefault().TypedValue.Value.ToString();
                }
            }

            //If you have no Display Name, just return the ToString of the enum
            return enumerationValue.ToString();
        }

    }
}
