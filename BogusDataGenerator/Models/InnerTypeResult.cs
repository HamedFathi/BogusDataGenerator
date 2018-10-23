using BogusDataGenerator.Enums;
using BogusDataGenerator.Extensions;
using System;

namespace BogusDataGenerator.Models
{
    public class InnerTypeResult
    {
        public string UniqueId
        {
            get { return $"{Level}-{Name}-{FriendlyTypeName}"; }
        }

        public int Level { get; set; }
        public Type Type { get; set; }
        public TypeStatus Status { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }

        public string TypeNamespace
        {
            get
            {
                return Type.Namespace;
            }
        }

        public string Location
        {
            get
            {
                return Type.Assembly.Location;
            }
        }

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
                var isNullable = Type.ToString().StartsWith("System.Nullable`1[");
                var isArray = Type.ToString().EndsWith("[]");
                if (isNullable)
                {
                    var newName = Type.ToString().Replace("System.Nullable`1[", "").Replace("]", "");
                    friendlyTypeName = newName.GetFriendlyTypeName() + "?";
                }
                else if (isArray)
                {
                    var newName = Type.ToString().Replace("[]", "");
                    var count = Type.ToString().CountOfSubstring("[]");
                    friendlyTypeName = newName.GetFriendlyTypeName() + "[]".Repeat(count);
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
