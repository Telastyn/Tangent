using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangent.Intermediate;
using Tangent.Parsing.TypeResolved;

namespace Tangent.Parsing
{
    public class HalfBoundExpression : Expression
    {
        public readonly List<Expression> Bindings;
        public readonly ReductionRule<dynamic, dynamic> Rule;
        private readonly dynamic Declaration;

        public PhrasePart NextStep
        {
            get
            {
                var takesLeft = Rule.Takes.Count - Bindings.Count;
                if (takesLeft == 0) {
                    return null;
                }

                return Fix(Rule.Takes[Rule.Takes.Count - takesLeft]);
            }
        }

        public bool IsDone
        {
            get
            {
                return Rule.Takes.Count == Bindings.Count;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.FunctionBinding;
            }
        }

        public Expression FullyBind()
        {
            if (!IsDone) {
                throw new InvalidOperationException();
            }

            if (Declaration is ParameterDeclaration) {
                return new ParameterAccessExpression(Declaration);
            }

            if (Declaration is TypeDeclaration) {
                return new TypeAccessExpression(Declaration.Returns);
            }

            if (Declaration is ReductionDeclaration) {
                return new FunctionBindingExpression(Declaration, Bindings.Where(b => b != null && !(b is IdentifierExpression)));
            }

            throw new NotImplementedException();
        }

        public HalfBoundExpression(ParameterDeclaration declaration)
        {
            Bindings = new List<Expression>();
            Rule = new ReductionRule<dynamic, dynamic>(declaration.Takes, declaration.Returns);
            Declaration = declaration;
        }

        public HalfBoundExpression(TypeDeclaration declaration)
        {
            Bindings = new List<Expression>();
            Rule = new ReductionRule<dynamic, dynamic>(declaration.Takes, declaration.Returns);
            Declaration = declaration;
        }

        public HalfBoundExpression(ReductionDeclaration declaration)
        {
            Bindings = new List<Expression>();
            Rule = new ReductionRule<dynamic, dynamic>(declaration.Takes, declaration.Returns);
            Declaration = declaration;
        }

        private PhrasePart Fix(dynamic param)
        {
            if (param is PhrasePart) {
                return param;
            }

            if (param is Identifier) {
                return new PhrasePart(param);
            }

            throw new InvalidOperationException();
        }

    }
}
