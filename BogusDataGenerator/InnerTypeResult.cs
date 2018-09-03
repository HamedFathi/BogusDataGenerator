using System;

namespace BogusDataGenerator
{
    public class InnerTypeResult
    {
        public int Level { get; set; }
        public Type Type { get; set; }
        public TypeStatus Status { get; set; }
        public string Name => Type.ToString();
        public string FullName => Type.FullName;
        public string CorrectedFullName
        {
            get
            {
                if (Status == TypeStatus.Array || Status == TypeStatus.ArrayElement)                
                    return FullName;                
                else
                    return Type.GetFullName();
            }
        }


    }


}
