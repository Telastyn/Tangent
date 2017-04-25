using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tangent.Cli.TestSuite.RosettaCode
{
    [TestClass]
    public class TestExpectations
    {
        [TestMethod]
        public void AplusB()
        {
            var result = Test.ProgramFile(new[] { @"RosettaCode\AplusB.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan", @"lib\console-lib.tan" }, new[] { typeof(IEnumerable<>).Assembly, typeof(List<>).Assembly, typeof(Enumerable).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new string[] { }));
        }

        [TestMethod]
        public void InputLoop()
        {
            var result = Test.DebugProgramFile(new[] { @"RosettaCode\InputLoop.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan", @"lib\console-lib.tan" }, new[] { typeof(IEnumerable<>).Assembly, typeof(List<>).Assembly, typeof(Enumerable).Assembly, typeof(TextReader).Assembly },
                "42\n How now brown cow\n\n");
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new string[] { "42", "How now brown cow" }));
        }
    }
}
