namespace Cassowary.Attributes
{
    /// <summary>
    /// Indicates that what this is applied to accesses the Clr in some way
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Enum, Inherited = false)]
    internal sealed class IntrinsicAttribute : Attribute { }
}
