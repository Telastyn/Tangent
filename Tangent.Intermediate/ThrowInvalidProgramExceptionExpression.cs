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
    }
}
