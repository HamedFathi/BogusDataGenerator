using System;

namespace BogusDataGenerator
{
    public class InnerTypeResult
    {
        public int Level { get; set; }
        public Type Type { get; set; }
        public TypeStatus Status { get; set; }
        public string Name => Type.Name;
        public string FullName => Type.FullName;
        public string UnderlyingSystemTypeName => Type.UnderlyingSystemType.Name;
        public string UnderlyingSystemTypeFullName => Type.UnderlyingSystemType.FullName;

    }


}
