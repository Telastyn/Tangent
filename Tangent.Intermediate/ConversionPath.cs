using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ConversionPath
    {
        public readonly int Cost;
        public readonly Func<Expression, TransformationScope, Expression> Convert;
        public readonly bool IsGeneric;

        public ConversionPath(ReductionDeclaration fn)
        {
            if (fn.Takes.Count != 1 || fn.Takes.First().IsIdentifier) {
                throw new InvalidOperationException();
            }

            if (fn.Returns.EffectiveType.ImplementationType == KindOfType.TypeClass && ((TypeClass)fn.Returns.EffectiveType).Implementations.Contains(fn.Takes.First().Parameter.RequiredArgumentType)) {
                // An interface implementation.
                Cost = 0;
            } else {
                Cost = 1;
            }

            Convert = (arg, scope) => GenerateInvokeFor(fn, arg, scope);
            IsGeneric = fn.GenericParameters.Any();
        }

        public ConversionPath(params ConversionPath[] parts)
        {
            Cost = parts.Select(p => p.Cost).Sum();
            Convert = (arg, scope) => parts.Aggregate(arg, (i, p) => p.Convert(i, scope));
            IsGeneric = parts.Any(p => p.IsGeneric);
        }

        private ConversionPath(int cost, Func<Expression, TransformationScope, Expression> convert, bool isGeneric)
        {
            Cost = cost;
            Convert = convert;
            IsGeneric = isGeneric;
        }

        private Expression GenerateInvokeFor(ReductionDeclaration fn, Expression arg, TransformationScope scope)
        {
            Dictionary<ParameterDeclaration, TangentType> inferences = new Dictionary<ParameterDeclaration, TangentType>();
            if (fn.Takes.First().Parameter.RequiredArgumentType.CompatibilityMatches(arg.EffectiveType, inferences)) {
                return new FunctionInvocationExpression(fn, new[] { arg }, fn.GenericParameters.Select(gp => inferences[gp]).ToList(), arg.SourceInfo);
            } else {
                throw new ApplicationException("A conversion was called that doesn't match its type.");
            }
        }

        public static ConversionPath Ambiguity(ConversionPath a, ConversionPath b)
        {
            if(a.Cost != b.Cost || a.IsGeneric != b.IsGeneric) { throw new InvalidOperationException("Ambiguous conversion paths should match on cost and genericity."); }
            if (a.Cost == 0) { throw new InvalidOperationException("Ambiguous conversion paths should have a non-zero cost. Zero cost implies interface implementations, which do not conflict."); }

            return new ConversionPath(a.Cost, (expr, scope) => {
                var aexpr = a.Convert(expr, scope);
                var bexpr = b.Convert(expr, scope);
                List<Expression> ambiguities = new List<Expression>() { aexpr, bexpr };
                while (ambiguities.Any(e => e.NodeType == ExpressionNodeType.Ambiguity)) {
                    ambiguities = ambiguities.SelectMany(e => e.NodeType == ExpressionNodeType.Ambiguity ? ((AmbiguousExpression)e).PossibleInterpretations : new[] { e }).ToList();
                }

                return new AmbiguousExpression(ambiguities);
            }, a.IsGeneric);
        }

        public static ConversionPath Lazify(ConversionPath a)
        {
            return new ConversionPath(a.Cost + 1, (expr, scope) => {
                var partialConversion = a.Convert(expr, scope);
                return new LambdaExpression(Enumerable.Empty<ParameterDeclaration>(), partialConversion.EffectiveType, new Block(new[] { partialConversion }, Enumerable.Empty<ParameterDeclaration>()), partialConversion.SourceInfo);
            }, false);
        }

        public static ConversionPath Lazify(TangentType t)
        {
            return new ConversionPath(1, (expr, scope) => new LambdaExpression(Enumerable.Empty<ParameterDeclaration>(), t, new Block(new[] { expr }, Enumerable.Empty<ParameterDeclaration>()), expr.SourceInfo), false);
        }

        public static ConversionPath Delazy(TangentType t)
        {
            return new ConversionPath(1, (expr, scope) => new DelegateInvocationExpression(expr, Enumerable.Empty<Expression>(), expr.SourceInfo), false);
        }
    }
}
