using Cassowary.Attributes;

namespace Cassowary.Runtime.Cor
{
    [Intrinsic]
    public enum CorInfoHFAElemType
    {
        CORINFO_HFA_ELEM_NONE = 0,
        CORINFO_HFA_ELEM_FLOAT = 1,
        CORINFO_HFA_ELEM_DOUBLE = 2,
        CORINFO_HFA_ELEM_VECTOR64 = 3,
        CORINFO_HFA_ELEM_VECTOR128 = 4,
        CORINFO_HFA_ELEM_VECTOR256 = 5
    }
}
