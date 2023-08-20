namespace Cassowary.Attributes
{
    /// <summary>
    /// Indicates that the type that this is applied to should be a pointer, not an object. For example, MethodTable* rather than MethodTable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    internal sealed class ShouldUsePointerNotObjectAttribute : Attribute { }
}
