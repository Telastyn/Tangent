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
        public HashSet<string> SourceFiles = new HashSet<string>();

        [DataMember]
        public HashSet<string> DllImports = new HashSet<string>();

        [DataMember]
        public HashSet<string> Includes = new HashSet<string>();

        [DataMember]
        public string DestinationFile = "out.exe";

        public CompilerInputs1 Combine(CompilerInputs1 other)
        {
            return new CompilerInputs1() {
                DestinationFile = DestinationFile,
                SourceFiles = new HashSet<string>(SourceFiles.Concat(other.SourceFiles)),
                DllImports = new HashSet<string>(DllImports.Concat(other.DllImports))
            };
        }
    }
}
