using BogusDataGenerator.Enums;
using BogusDataGenerator.Extensions;
using System;

namespace BogusDataGenerator.Models
{
    public class InnerTypeResult
    {
        public string UniqueId
        {
            get { return $"{Level}-{Name}-{Type.FullName}"; }
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

    }


}
