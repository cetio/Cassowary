﻿//              FUCK OOP & FUCK INTERLACED LIBRARIES
//
//            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                   Version 2, December 2004
// 
// Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
// Everyone is permitted to copy and distribute verbatim or modified
// copies of this license document, and changing it is allowed as long
// as the name is changed.
//
//           DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//  TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//  0. You just DO WHAT THE FUCK YOU WANT TO.

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
