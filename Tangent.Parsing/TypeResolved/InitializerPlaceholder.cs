using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;
using Tangent.Parsing.Partial;

namespace Tangent.Parsing
{
    public class InitializerPlaceholder : Expression
    {
        public readonly PartialStatement UnresolvedInitializer;

        public InitializerPlaceholder(PartialStatement unresolvedInitializer):base(LineColumnRange.CombineAll(unresolvedInitializer.FlatTokens.Select(ft => ft.SourceInfo)))
        {
            UnresolvedInitializer = unresolvedInitializer;
        }

        public override TangentType EffectiveType
        {
            get
            {
                throw new NotImplementedException("Should not call EffectiveType on placeholders.");
            }
        }

        public override ExpressionNodeType NodeType
        {
            get
            {
                return ExpressionNodeType.InitializerPlaceholder;
            }
        }
    }
}
