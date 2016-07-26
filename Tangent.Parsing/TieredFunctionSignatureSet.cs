using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.Intermediate;

namespace Tangent.Parsing
{
    public class TieredFunctionSignatureSet : IFunctionSignatureSet
    {
        // returnType, generic param count, takecount
        private readonly Dictionary<TangentType, Dictionary<int, Dictionary<int, List<ReductionDeclaration>>>> store = new Dictionary<TangentType, Dictionary<int, Dictionary<int, List<ReductionDeclaration>>>>();
        private readonly List<ReductionDeclaration> functions = new List<ReductionDeclaration>();

        public List<ReductionDeclaration> Functions
        {
            get
            {
                return functions;
            }
        }

        public void Add(ReductionDeclaration fn)
        {
            if (!store.ContainsKey(fn.Returns.EffectiveType)) {
                store.Add(fn.Returns.EffectiveType, new Dictionary<int, Dictionary<int, List<ReductionDeclaration>>>());
            }

            var a = store[fn.Returns.EffectiveType];
            var genericCount = fn.GenericParameters.Count();
            if (!a.ContainsKey(genericCount)) {
                a.Add(genericCount, new Dictionary<int, List<ReductionDeclaration>>());
            }

            var b = a[genericCount];
            if (!b.ContainsKey(fn.Takes.Count)) {
                b.Add(fn.Takes.Count, new List<ReductionDeclaration>() { fn });
                functions.Add(fn);
                return;
            }

            var c = b[fn.Takes.Count];
            if (c.Any(x => fn.MatchesSignatureOf(x))) {
                return;
            }

            c.Add(fn);
            functions.Add(fn);
        }
    }
}
