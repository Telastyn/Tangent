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
    }
}
