using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing
{
    public class Input
    {
        private readonly List<Expression> buffer;
        public readonly Scope Scope;
        private readonly List<ReductionDeclaration> conversionsTaken;

        public Input(IEnumerable<Identifier> identifiers, Scope scope)
        {
            buffer = new List<Expression>(identifiers.Select(id => new IdentifierExpression(id)));
            Scope = scope;
            conversionsTaken = new List<ReductionDeclaration>();
        }

        public Input(IEnumerable<Expression> exprs, Scope scope) : this(exprs, scope, new List<ReductionDeclaration>()) { }

        private Input(IEnumerable<Expression> exprs, Scope scope, IEnumerable<ReductionDeclaration> conversionsTaken)
        {
            buffer = new List<Expression>(exprs);
            Scope = scope;
            this.conversionsTaken = new List<ReductionDeclaration>(conversionsTaken);
        }

        public List<Expression> InterpretAsStatement()
        {
            return InterpretTowards(TangentType.Void);
        }

        private List<Expression> InterpretTowards(TangentType type)
        {
            if (buffer.Count == 1)
            {
                if (type == GetEffectiveTypeIfPossible(buffer[0]))
                {
                    return buffer;
                }
            }

            for (int ix = 0; ix < buffer.Count; ++ix)
            {
                foreach (var reductionCandidatePool in TryReduce(ix))
                {
                    List<Expression> successes = new List<Expression>();
                    foreach (var candidate in reductionCandidatePool)
                    {
                        successes.AddRange(new Input(candidate, Scope).InterpretAsStatement());
                    }

                    if (successes.Any())
                    {
                        return successes;
                    }
                }
            }

            // And if we've gotten here, we have some bundle of tokens that cannot be reduced further but don't result in our target.
            return new List<Expression>();
        }

        /// <summary>
        /// Checks for a match at index ix, yielding tiers of flattened expressions with the match processed.
        /// </summary>
        private IEnumerable<List<List<Expression>>> TryReduce(int ix)
        {
            // param
            foreach (IGrouping<int, ParameterDeclaration> parameterTier in Scope.Parameters.Where(pd => IsMatch(buffer.Skip(ix).ToList(), pd.Takes.Select(id => new PhrasePart(id)).ToList())).GroupBy(pd => pd.Takes.Count).OrderByDescending(grp => grp.Key))
            {
                yield return parameterTier.Select(pd => buffer.Take(ix).Concat(new[] { new ParameterAccessExpression(pd) }).Concat(buffer.Skip(ix + pd.Takes.Count)).ToList()).ToList();
            }

            // type
            foreach (IGrouping<int, TypeDeclaration> typeTier in Scope.Types.Where(td => IsMatch(buffer.Skip(ix).ToList(), td.Takes.Select(id => new PhrasePart(id)).ToList())).GroupBy(td => td.Takes.Count).OrderByDescending(grp => grp.Key))
            {
                yield return typeTier.Select(td => buffer.Take(ix).Concat(new[] { new TypeAccessExpression(td.Returns) }).Concat(buffer.Skip(ix + td.Takes.Count)).ToList()).ToList();
            }

            // fn
            var legalFunctions = Scope.Functions.Where(fn => IsMatch(buffer.Skip(ix).ToList(), fn.Takes)).ToList();
            while (legalFunctions.Any())
            {
                yield return PopBestCandidates(legalFunctions).Select(fn=>buffer.Take(ix).Concat(new[]{new FunctionBindingExpression(fn, buffer.Skip(ix).Take(fn.Takes.Count).Where(p=>p.NodeType!= ExpressionNodeType.Identifier).ToList())}).Concat(buffer.Skip(ix+fn.Takes.Count)).ToList()).ToList();
            }

            // enums
            if (buffer[ix].NodeType == ExpressionNodeType.Identifier)
            {
                var id = (buffer[ix] as IdentifierExpression).Identifier;
                var valueCandidates = Scope.Types.Where(td => td.Returns.ImplementationType == KindOfType.Enum && ((EnumType)td.Returns).Values.Select(v => v.Value).Contains(id.Value)).Select(td=>td.Returns).Cast<EnumType>().Select(enumtype=>enumtype.SingleValueTypeFor(id)).ToList();
                if (valueCandidates.Any())
                {
                    yield return valueCandidates.Select(svt => buffer.Take(ix).Concat(new[] { new EnumValueAccessExpression(svt) }.Concat(buffer.Skip(ix + 1))).ToList()).ToList();
                }
            }

            // **Coersions.**
            // Enum constant -> Enum type
            if (buffer[ix].NodeType == ExpressionNodeType.EnumValueAccess)
            {
                yield return new List<List<Expression>>() { buffer.Take(ix).Concat(new[] { new EnumWideningExpression(buffer[ix] as EnumValueAccessExpression) }).Concat(buffer.Skip(ix + 1)).ToList() };
            }

            // Bound function -> function invocation.
            if (buffer[ix].NodeType == ExpressionNodeType.FunctionBinding)
            {
                yield return new List<List<Expression>>() { buffer.Take(ix).Concat(new[] { new FunctionInvocationExpression(buffer[ix] as FunctionBindingExpression) }).Concat(buffer.Skip(ix + 1)).ToList() };
            }

            // Param accessed delegate -> delegate invocation.
            if (buffer[ix].NodeType == ExpressionNodeType.ParameterAccess)
            {
                var paramAccess = (ParameterAccessExpression)buffer[ix];
                if (paramAccess.Parameter.Returns.ImplementationType == KindOfType.Lazy)
                {
                    yield return new List<List<Expression>>() { buffer.Take(ix).Concat(new[] { new DelegateInvocationExpression(paramAccess) }).Concat(buffer.Skip(ix + 1)).ToList() };
                }
            }
        }

        public static bool IsMatch(List<Expression> input, List<PhrasePart> rule)
        {
            if (input.Count < rule.Count) { return false; }
            var inputEnum = input.GetEnumerator();
            foreach (var entry in rule)
            {
                inputEnum.MoveNext();
                if (entry.IsIdentifier)
                {
                    if ((inputEnum.Current.NodeType != ExpressionNodeType.Identifier) ||
                        ((IdentifierExpression)inputEnum.Current).Identifier.Value != entry.Identifier.Value)
                    {
                        return false;
                    }
                }
                else
                {
                    var inType = GetEffectiveTypeIfPossible(inputEnum.Current);
                    if (inType == null || inType != entry.Parameter.Returns)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        
        private static TangentType GetEffectiveTypeIfPossible(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionNodeType.FunctionInvocation:
                    var invoke = (FunctionInvocationExpression)expr;
                    return invoke.EffectiveType;

                case ExpressionNodeType.ParameterAccess:
                    var param = (ParameterAccessExpression)expr;
                    return param.Parameter.Returns;

                case ExpressionNodeType.FunctionBinding:
                    var binding = expr as FunctionBindingExpression;
                    return binding.EffectiveType;

                case ExpressionNodeType.DelegateInvocation:
                    var delegateInvoke = expr as DelegateInvocationExpression;
                    return delegateInvoke.EffectiveType;

                case ExpressionNodeType.Constant:
                    var constant = expr as ConstantExpression;
                    return constant.EffectiveType;

                case ExpressionNodeType.EnumValueAccess:
                    var valueAccess = expr as EnumValueAccessExpression;
                    return valueAccess.EnumValue;

                case ExpressionNodeType.EnumWidening:
                    var widening = expr as EnumWideningExpression;
                    return widening.EnumAccess.EnumValue.ValueType;

                case ExpressionNodeType.TypeAccess:
                case ExpressionNodeType.Identifier:
                case ExpressionNodeType.Unknown:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        private List<ReductionDeclaration> PopBestCandidates(List<ReductionDeclaration> candidates)
        {
            var best = new List<ReductionDeclaration>();
            foreach (var entry in candidates)
            {
                if (!best.Any())
                {
                    best.Add(entry);
                }
                else if (best.First().Takes.Count == entry.Takes.Count)
                {
                    var bestEnum = best.First().Takes.GetEnumerator();
                    var entryEnum = entry.Takes.GetEnumerator();
                    bool go = true;
                    while (go && bestEnum.MoveNext() && entryEnum.MoveNext())
                    {
                        int cmp = Compare(bestEnum.Current, entryEnum.Current);
                        if (cmp == -1)
                        {
                            go = false;
                        }
                        else if (cmp == 1)
                        {
                            best.Clear();
                            best.Add(entry);
                            go = false;
                        }
                    }

                    if (go)
                    {
                        best.Add(entry);
                    }
                }
                else
                {
                    candidates.RemoveAll(r => best.Contains(r));
                    return best;
                }
            }

            candidates.RemoveAll(r => best.Contains(r));
            return best;
        }

        private int Compare(dynamic a, dynamic b)
        {
            var phraseA = a is PhrasePart;
            var phraseB = b is PhrasePart;

            var idA = phraseA ? a.IsIdentifier : true;
            var idB = phraseB ? b.IsIdentifier : true;

            if (idA && !idB)
            {
                return -1;
            }

            if (idB && !idA)
            {
                return 1;
            }

            if (phraseA)
            {
                var ppa = ((PhrasePart)a);
                if (!ppa.IsIdentifier)
                {
                    if (ppa.Parameter.Returns.ImplementationType == KindOfType.SingleValue)
                    {
                        if (phraseB)
                        {
                            var ppb = ((PhrasePart)b);
                            if (!ppb.IsIdentifier && ppb.Parameter.Returns.ImplementationType == KindOfType.Enum)
                            {
                                if (((SingleValueType)a.Parameter.Returns).ValueType == b.Parameter.Returns)
                                {
                                    return -1;
                                }
                            }
                        }
                    }
                    else if (ppa.Parameter.Returns.ImplementationType == KindOfType.Enum)
                    {
                        if (phraseB)
                        {
                            var ppb = ((PhrasePart)b);
                            if (!ppb.IsIdentifier && ppb.Parameter.Returns.ImplementationType == KindOfType.SingleValue)
                            {
                                if (a.Parameter.Returns == ((SingleValueType)b.Parameter.Returns).ValueType)
                                {
                                    return 1;
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }
    }
}
