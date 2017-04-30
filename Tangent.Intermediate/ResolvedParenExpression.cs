using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    public class ResolvedParenExpression : Expression
    {
        public readonly Block Contents;

        internal ResolvedParenExpression(Block contents, TangentType effectiveType, LineColumnRange sourceInfo) : base(sourceInfo)
        {
            this.Contents = contents;
            this.effectiveType = effectiveType;
        }

        private readonly TangentType effectiveType;
        public override TangentType EffectiveType
        {
            get
            {
                return effectiveType;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.ResolvedParenExpr;
            }
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Contents.Statements.Any(stmt => stmt.AccessesAnyParameters(parameters, workset));
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            List<Expression> newbs = new List<Expression>();
            bool changed = false;
            foreach (var stmt in Contents.Statements) {
                var newb = stmt.ReplaceParameterAccesses(mapping);
                if (newb != stmt) { changed = true; }
                newbs.Add(newb);
            }

            if (!changed) { return this; }
            return new ResolvedParenExpression(new Block(newbs, Enumerable.Empty<ParameterDeclaration>()), EffectiveType, SourceInfo);
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return Contents.Statements.Any(stmt => stmt.RequiresClosureAround(parameters, workset));
        }

        internal override void ReplaceTypeResolvedFunctions(Dictionary<Function, Function> replacements, HashSet<Expression> workset)
        {
            foreach (var stmt in Contents.Statements) {
                stmt.ReplaceTypeResolvedFunctions(replacements, workset);
            }
        }
    }
}
