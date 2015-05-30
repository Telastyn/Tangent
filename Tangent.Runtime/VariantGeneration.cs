//// ******* Did this in a separate project. Moving it here to get it into source control.
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Tangent.CodeGen
//{
//    class Program
//    {
//        public static readonly int Count = 24;
//        public static IEnumerable<string> GenericParams
//        {
//            get
//            {
//                return Enumerable.Range((int)'A', 24).Select(x => ((char)x).ToString()).ToArray();
//            }
//        }

//        static void Main(string[] args)
//        {
//            Console.WriteLine(GenerateVariants());
//        }

//        public static string GenerateVariants()
//        {
//            var buf = new StringBuilder();
//            buf.Append(
//@"using System;
//
//namespace Tangent.Runtime {
//");
//            for (int ct = 2; ct <= Count; ++ct) {
//                var genericParams = GenericParams.Take(ct).ToArray();
//                buf.AppendFormat(
//@"
//    public class Variant<{0}> {{
//        public readonly object Value;
//        public readonly int Mode;
//
//", string.Join(", ", genericParams));

//                int mode = 1;
//                foreach (var ctor in genericParams) {
//                    buf.AppendFormat(
//@"        public Variant({0} value) {{
//            Value = value;
//            Mode = {1};
//        }}
//
//", ctor, mode);
//                    mode++;
//                }

//                mode = 1;
//                foreach (var genericParam in genericParams) {
//                    buf.AppendFormat(
//@"        private {0} Value{1} {{
//            get {{
//                return ({0})Value;
//            }}
//        }}
//
//", genericParam, mode);

//                    mode++;
//                }

//                buf.AppendFormat(
//@"        public void Process({0}) {{
//            switch(Mode) {{
//{1}                default:
//                    throw new NotImplementedException();
//            }}
//        }}
//
//", string.Join(", ", genericParams.Select(pt => string.Format("Action<{0}> {1}", pt, "for" + pt))), string.Join("", Enumerable.Range(1, genericParams.Count()).Select(md => string.Format(
//@"                case {0}:
//                    for{1}(Value{0});
//                    return;
//", md, genericParams[md - 1]))));


//                buf.AppendFormat(
//@"        public TReturn Process<TReturn>({0}) {{
//            switch(Mode) {{
//{1}                default:
//                    throw new NotImplementedException();
//            }}
//        }}
//", string.Join(", ", genericParams.Select(pt => string.Format("Func<{0}, TReturn> {1}", pt, "for" + pt))), string.Join("", Enumerable.Range(1, genericParams.Count()).Select(md => string.Format(
//@"                case {0}:
//                    return for{1}(Value{0});
//", md, genericParams[md - 1]))));

//                buf.AppendLine("    }");
//            }


//            buf.AppendLine("}");

//            return buf.ToString();
//        }
//    }
//}
