using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tangent.Intermediate.UnitTests
{
    [TestClass]
    public class ConversionGraphTests
    {
        [TestMethod]
        public void EmptyGraphIsNullConversion()
        {
            var graph = new ConversionGraph(Enumerable.Empty<ReductionDeclaration>());

            var conversion = graph.FindConversion(TangentType.String, TangentType.Int);

            Assert.AreEqual(null, conversion);
        }

        [TestMethod]
        public void DirectHappyPath()
        {
            var stoi = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.String)), new Function(TangentType.Int, null));
            var graph = new ConversionGraph(new[] { stoi });

            var conversion = graph.FindConversion(TangentType.String, TangentType.Int);
            var result = conversion.Convert(new ConstantExpression<string>(TangentType.String, "foo", null), null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.NodeType == ExpressionNodeType.FunctionInvocation);
        }

        [TestMethod]
        public void IndirectHappyPath()
        {
            var stoi = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.String)), new Function(TangentType.Int, null));
            var itob = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.Int)), new Function(TangentType.Bool, null));
            var graph = new ConversionGraph(new[] { stoi, itob });

            var conversion = graph.FindConversion(TangentType.String, TangentType.Bool);
            var result = conversion.Convert(new ConstantExpression<string>(TangentType.String, "foo", null), null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.NodeType == ExpressionNodeType.FunctionInvocation);
        }

        [TestMethod]
        public void FavorDirectRoutes()
        {
            var stoi = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.String)), new Function(TangentType.Int, null));
            var itob = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.Int)), new Function(TangentType.Bool, null));
            var stob = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.String)), new Function(TangentType.Bool, null));
            var graph = new ConversionGraph(new[] { stoi, itob, stob });

            var conversion = graph.FindConversion(TangentType.String, TangentType.Bool);
            var result = conversion.Convert(new ConstantExpression<string>(TangentType.String, "foo", null), null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.NodeType == ExpressionNodeType.FunctionInvocation);
            Assert.AreEqual(stob, ((FunctionInvocationExpression)result).FunctionDefinition);
        }

        [TestMethod]
        public void FavorShortRoutes()
        {
            var ntos = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.Double)), new Function(TangentType.String, null));
            var stoi = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.String)), new Function(TangentType.Int, null));
            var itob = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.Int)), new Function(TangentType.Bool, null));
            var stob = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.String)), new Function(TangentType.Bool, null));
            var graph = new ConversionGraph(new[] { stoi, itob, stob, ntos });

            var conversion = graph.FindConversion(TangentType.Double, TangentType.Bool);
            var result = conversion.Convert(new ConstantExpression<string>(TangentType.Double, "foo", null), null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.NodeType == ExpressionNodeType.FunctionInvocation);
            var fnInvoke = ((FunctionInvocationExpression)result);
            Assert.AreEqual(stob, fnInvoke.FunctionDefinition);
            Assert.IsTrue(fnInvoke.Arguments.First().NodeType == ExpressionNodeType.FunctionInvocation);
            var fn2 = (FunctionInvocationExpression)fnInvoke.Arguments.First();
            Assert.AreEqual(ntos, fn2.FunctionDefinition);
        }

        [TestMethod]
        public void AmbiguousRoutesSaySo()
        {
            var ntos = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.Double)), new Function(TangentType.String, null));
            var ntoi = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.Double)), new Function(TangentType.Int, null));
            var itob = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.Int)), new Function(TangentType.Bool, null));
            var stob = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("_", TangentType.String)), new Function(TangentType.Bool, null));

            var graph = new ConversionGraph(new[] { ntoi, itob, stob, ntos });

            var conversion = graph.FindConversion(TangentType.Double, TangentType.Bool);
            var result = conversion.Convert(new ConstantExpression<string>(TangentType.Double, "foo", null), null);

            Assert.AreEqual(ExpressionNodeType.Ambiguity, result.NodeType);
        }

        [TestMethod]
        public void GenericSimplePath()
        {
            var gd = new ParameterDeclaration("T", TangentType.Any.Kind);
            var g = GenericArgumentReferenceType.For(gd);
            var gtos = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("x", g)), new Function(TangentType.String, null));

            var graph = new ConversionGraph(new[] { gtos });

            var conversion = graph.FindConversion(TangentType.Int, TangentType.String);
            Assert.IsNotNull(conversion);
        }

        [TestMethod]
        public void FavorPartialGenerics()
        {
            var gd = new ParameterDeclaration("T", TangentType.Any.Kind);
            var g = GenericArgumentReferenceType.For(gd);
            var gtos = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("x", g)), new Function(TangentType.String, null));

            var gd2 = new ParameterDeclaration("T", TangentType.Any.Kind);
            var g2 = GenericArgumentReferenceType.For(gd2);
            var gpt = new ProductType(new[] { new PhrasePart(new ParameterDeclaration("x", g2)) }, new[] { gd2 }, Enumerable.Empty<Field>());
            var bgt = BoundGenericType.For(gpt, new[] { g2 });
            var bgttos = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("x", bgt)), new Function(TangentType.String, null));

            var graph = new ConversionGraph(new[] { gtos, bgttos });

            var bpt = BoundGenericType.For(gpt, new[] { TangentType.Int });

            var conversion = graph.FindConversion(bpt, TangentType.String);
            Assert.IsNotNull(conversion);
            var result = conversion.Convert(new ConstantExpression<string>(bpt, null, null), null) as FunctionInvocationExpression;
            Assert.IsNotNull(result);
            Assert.AreEqual(bgttos, result.FunctionDefinition);
        }

        [TestMethod]
        public void ChainedPartialGenerics()
        {
            var gd = new ParameterDeclaration("T", TangentType.Any.Kind);
            var g = GenericArgumentReferenceType.For(gd);
            var gpt = new ProductType(new[] { new PhrasePart(new ParameterDeclaration("x", g)) }, new[] { gd }, Enumerable.Empty<Field>());
            var bgt = BoundGenericType.For(gpt, new[] { g });


            var gd2 = new ParameterDeclaration("T", TangentType.Any.Kind);
            var g2 = GenericArgumentReferenceType.For(gd2);
            var gpt2 = new ProductType(new[] { new PhrasePart(new ParameterDeclaration("x", g2)) }, new[] { gd2 }, Enumerable.Empty<Field>());
            var bgt2 = BoundGenericType.For(gpt2, new[] { g2 });
            var fng = GenericArgumentReferenceType.For(new ParameterDeclaration("fn T", TangentType.Any.Kind));
            var bgttobgt2 = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("x", bgt.ResolveGenericReferences(pd => fng))), new Function(bgt2.ResolveGenericReferences(pd => fng), null));
            var bgt2tos = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("x", bgt2)), new Function(TangentType.String, null));

            var graph = new ConversionGraph(new[] { bgttobgt2, bgt2tos });

            var bpt = BoundGenericType.For(gpt, new[] { TangentType.Int });

            var conversion = graph.FindConversion(bpt, TangentType.String);
            Assert.IsNotNull(conversion);
        }

        [TestMethod]
        public void SimplePartialGenericParameterMismatchWorks()
        {
            var gd1 = new ParameterDeclaration("T", TangentType.Any.Kind);
            var gd2 = new ParameterDeclaration("X", TangentType.Any.Kind);
            var g1 = GenericArgumentReferenceType.For(gd1);
            var g2 = GenericArgumentReferenceType.For(gd2);
            var gpt1 = new ProductType(new[] { new PhrasePart(new ParameterDeclaration("x", g1)) }, new[] { gd1 }, Enumerable.Empty<Field>());
            var gpt2 = new ProductType(new[] { new PhrasePart(new ParameterDeclaration("x", g1)) }, new[] { gd1 }, Enumerable.Empty<Field>());
            var bgt11 = BoundGenericType.For(gpt1, new[] { g1 });
            var bgt21 = BoundGenericType.For(gpt2, new[] { g1 });
            var bgt22 = BoundGenericType.For(gpt2, new[] { g2 });

            var fng = GenericArgumentReferenceType.For(new ParameterDeclaration("fn T", TangentType.Any.Kind));
            var bgt11tobgt21 = new ReductionDeclaration(new PhrasePart(new ParameterDeclaration("x", bgt11.ResolveGenericReferences(pd => fng))), new Function(bgt21.ResolveGenericReferences(pd => fng), null));

            var graph = new ConversionGraph(new[] { bgt11tobgt21 });

            var conversion = graph.FindConversion(bgt11, bgt22);

            Assert.IsNotNull(conversion);
        }
    }
}
