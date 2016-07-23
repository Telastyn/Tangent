using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Cli
{
    [DataContract]
    public sealed class CompilerInputs1
    {
        [DataMember]
        public List<string> SourceFiles = new List<string>();

        [DataMember]
        public List<string> DllImports = new List<string>();

        [DataMember]
        public string DestinationFile = "out.exe";
    }
}
