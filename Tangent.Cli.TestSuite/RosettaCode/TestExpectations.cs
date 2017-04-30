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
            var result = Test.DebugProgramFile(new[] { @"RosettaCode\AplusB.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan", @"lib\console-lib.tan", @"lib\string-lib.tan", @"lib\enumerable-lib.tan" }, new[] { typeof(IEnumerable<>).Assembly, typeof(List<>).Assembly, typeof(Enumerable).Assembly },
@"1 3
6 -4
42 0

");
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new string[] { "4", "2", "42"}));
        }

        [TestMethod]
        public void InputLoop()
        {
            var result = Test.DebugProgramFile(new[] { @"RosettaCode\InputLoop.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan", @"lib\console-lib.tan" }, new[] { typeof(IEnumerable<>).Assembly, typeof(List<>).Assembly, typeof(Enumerable).Assembly, typeof(TextReader).Assembly },
                "42\n How now brown cow\n\n");
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new string[] { "42", "How now brown cow" }));
        }

        [TestMethod]
        public void Factorial()
        {
            var result = Test.DebugProgramFile(new[] { @"RosettaCode\factorial.tan", @"lib\conditional-lib.tan", @"lib\console-lib.tan" }, new[] { typeof(Console).Assembly, typeof(TextReader).Assembly },
                "9\n");
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(new string[] { "362880" }));
        }

        //[Ignore] // RMS: compilation takes minutes... TODO.
        [TestMethod]
        public void FizzBuzz()
        {
            var result = Test.DebugProgramFile(new[] { @"RosettaCode\fizzbuzz.tan", @"lib\conditional-lib.tan", @"lib\looping-lib.tan", @"lib\console-lib.tan", @"lib\string-lib.tan" }, new[] { typeof(string).Assembly });
            var results = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            Assert.IsTrue(results.SequenceEqual(FizzBuzzResult));
        }

        private IEnumerable<string> FizzBuzzResult
        {
            get
            {
                for (int x = 1; x <= 100; ++x) {
                    string s = "";
                    if (x % 3 == 0) { s += "Fizz"; }
                    if (x % 5 == 0) { s += "Buzz"; }
                    if (string.IsNullOrWhiteSpace(s)) { yield return x.ToString(); } else { yield return s; }
                }
            }
        }
    }
}
