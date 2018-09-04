using BogusDataGenerator.Enums;
using System;

namespace BogusDataGenerator.Models
{
    public class InnerTypeResult
    {
        public int Level { get; set; }
        public Type Type { get; set; }
        public TypeStatus Status { get; set; }
        public string TypeName
        {
            get
            {
                if (Status == TypeStatus.Array || Status == TypeStatus.ArrayElement)
                    return Type.FullName;
                else
                    return Type.GetFullName();
            }
        }
        public string FriendlyTypeName
        {
            get
            {
                var friendlyTypeName = "";
                if (Type.ToString().StartsWith("System.Nullable`1["))
                {
                    var newName = Type.ToString().Replace("System.Nullable`1[", "").Replace("]", "");
                    friendlyTypeName = newName.GetFriendlyTypeName() + "?";
                }
                else
                {
                    friendlyTypeName = Type.GetFriendlyTypeName();
                }
                return friendlyTypeName == null ? TypeName : friendlyTypeName;
            }
        }

    }


}
