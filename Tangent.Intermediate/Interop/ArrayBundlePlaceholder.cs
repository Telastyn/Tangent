using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tangent.Intermediate.Interop
{
    /// <summary>
    /// This class exists to provide a key so that importing array types doesn't cause duplicates during the import.
    /// </summary>
    internal class ArrayBundlePlaceholder
    {
        internal object GetElement(int index) { return null; }
        internal void SetElement(int index, object value) { }

        internal static MethodInfo GetMI
        {
            get
            {
                return typeof(ArrayBundlePlaceholder).GetMethod(nameof(GetElement), BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        internal static MethodInfo SetMI
        {
            get
            {
                return typeof(ArrayBundlePlaceholder).GetMethod(nameof(SetElement), BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
    }
}
