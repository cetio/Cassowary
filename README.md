# Cassowary

> [!NOTE]  
> This library is not particularly supported anymore.
> I have plans to do some refactoring at some point, but it will be succeeded by Godwit!

Cassowary is a low-level library designed to provide access to Clr (Common Language Runtime) structures and reflection capabilities. It serves as an alternative to other libraries like Novus or Son Of Strike, offering direct access to Clr features. While Cassowary may not be perfect, it aims to provide a way to interact with Clr structures that might not be readily available elsewhere.

## Goals

The primary goals of Cassowary are:

- To serve as an alternative to libraries like Novus or Son Of Strike for accessing internal Clr structures.
- To provide functionalities for interacting with Clr structures and enabling reflection-like operations.

## Current Clr Structures

- **FieldDesc**: Low-level structure similar to a FieldInfo, stored in EEClass.
- **MethodDesc**: Low-level structure similar to a MethodInfo, stored in EEClass.
- **MethodDescChunk**: Groups of MethodDescs.
- **MethodTable**: Base structure of a Type, with various functions, like object allocation, instance construction, initialization, casts, boxing, and more.
- **NativeCodeVersion**: This is just a small MethodDesc metadata.
- **Stub**: Thin version of a MethodDesc, used by Delegates.
- **UMThunkMarshInfo**: Marshalling info for MethodDescs.
- **WriteableData**: MethodTable extra writeable data and flags.
- **ArrayClass**: Base EEClass of array types.
- **CCWTemplate**: COM information for types that are exposed to COM.
- **DelegateEEClass**: Base EEClass of delegate types, including Stubs and UMThunkMarshInfo.
- **EEClassLayoutInfo**: Layout information for an EEClass.
- **EEClassNativeLayoutInfo**: Optional native layout information for an EEClass.
- **EEClassOptionalFields**: Optional field storage for EEClass.
- **GuidInfo**: Type GUIDs.
- **LayoutEEClass**: Contains both EEClassLayoutInfo and EEClassNativeLayoutInfo.
- **NativeFieldDescriptor**: Optional EEClass native field descriptors and iterators.
- **SparseVTableMap**: VTable map contained in EEClassOptionalFields.

## License

Cassowary is released under the [DO WHATEVER THE FUCK YOU WANT] License.
