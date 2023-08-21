# Cassowary

Cassowary is a low-level library designed to provide access to Clr (Common Language Runtime) structures and reflection capabilities. It serves as an alternative to other libraries like Novus or Son Of Strike, offering direct access to Clr features. While Cassowary may not be perfect, it aims to provide a way to interact with Clr structures that might not be readily available elsewhere.

## Goals

The primary goals of Cassowary are:

- To serve as an alternative to libraries like Novus or Son Of Strike for accessing internal Clr structures.
- To provide functionalities for interacting with Clr structures and enabling reflection-like operations.

## Current Clr Structures

- **FieldDesc**: Access and manipulate fields within Clr structures.
- **MethodDesc**: Explore and work with methods in Clr.
- **MethodDescChunk**: Handle method chunks in Clr structures.
- **MethodTable**: Perform operations related to method tables, including object allocation, instance construction, initialization, casts, boxing, and more.
- **NativeCodeVersion**: Work with native code versions in Clr.
- **Stub**: Access and manage stubs within Clr.
- **UMThunkMarshInfo**: Interact with UMThunkMarshInfo structures.
- **WriteableData**: Handle writable data within Clr.
- **ArrayClass**: Deal with array classes in Clr.
- **CCWTemplate**: Manipulate CCWTemplate structures.
- **DelegateEEClass**: Perform operations on DelegateEEClass.
- **EEClassLayoutInfo**: Explore layout information for EEClass.
- **EEClassNativeLayoutInfo**: Work with native layout information for EEClass.
- **EEClassOptionalFields**: Access optional fields in EEClass.
- **GuidInfo**: Manage GUID information within Clr.
- **LayoutEEClass**: Handle layout information for EEClass.
- **NativeFieldDescriptor**: Work with descriptors for native fields.
- **SparseVTableMap**: Interact with sparse v-table maps.

## License

Cassowary is released under the [DO WHATEVER THE FUCK YOU WANT] License.
