using Cassowary.Attributes;

namespace Cassowary.Runtime.Cor
{
    [Intrinsic]
    public enum CorTokenType
    {
        mdtModule,
        mdtTypeRef,
        mdtTypeDef,
        mdtFieldDef,
        mdtMethodDef,
        mdtParamDef,
        mdtInterfaceImpl,
        mdtMemberRef,
        mdtCustomAttribute,
        mdtPermission,
        mdtSignature,
        mdtEvent,
        mdtProperty,
        mdtMethodImpl,
        mdtModuleRef,
        mdtTypeSpec,
        mdtAssembly,
        mdtAssemblyRef,
        mdtFile,
        mdtExportedType,
        mdtManifestResource,
        mdtGenericParam,
        mdtMethodSpec,
        mdtGenericParamConstraint,
        mdtString,
        mdtName,
        mdtBaseType
    }
}
