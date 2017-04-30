using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate
{
    /// <summary>
    /// Expression node that will throw an InvalidProgram exception. Used as the body of interface functions to indicate that I've screwed something up.
    /// </summary>
    public class ThrowInvalidProgramExceptionExpression : Expression 
    {
        public ThrowInvalidProgramExceptionExpression() : base(null) { }

        public override TangentType EffectiveType
        {
            get
            {
                return TangentType.Void;
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.InvalidProgramException;
            }
        }

        public override Expression ReplaceParameterAccesses(Dictionary<ParameterDeclaration, Expression> mapping)
        {
            return this;
        }

        public override bool RequiresClosureAround(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override bool AccessesAnyParameters(HashSet<ParameterDeclaration> parameters, HashSet<Expression> workset)
        {
            return false;
        }

        public override IEnumerable<ParameterDeclaration> CollectLocals(HashSet<Expression> workset)
        {
            yield break;
        }
    }
}
