using System.Collections.Generic;

namespace BogusDataGenerator.Models
{
    internal class SourceResult
    {
        public string Source { get; set; }
        public List<string> Namespaces { get; set; }
        public List<string> Assemblies { get; set; }

    }
}
