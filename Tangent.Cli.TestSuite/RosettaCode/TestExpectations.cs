using System;
using System.Collections.Generic;
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
            var result = Test.ProgramFile(new[] { @"RosettaCode\AplusB.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan", @"lib\console-lib.tan" }, new[] { typeof(IEnumerable<>).Assembly, typeof(List<>).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new string[] { }));
        }
    }
}
