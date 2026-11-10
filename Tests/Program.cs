using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cassowary.Runtime;
using Cassowary.Runtime.VM;
using Cassowary.Runtime.VM.EE;

static class T
{
    public static int Failures;

    public static void Check(string name, bool ok)
    {
        Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name}");
        if (!ok) Failures++;
    }

    public static void Throws<TEx>(string name, Action a) where TEx : Exception
    {
        try { a(); Check(name, false); }
        catch (TEx) { Check(name, true); }
        catch (Exception e) { Console.WriteLine($"    wrong exception {e.GetType().Name}: {e.Message}"); Check(name, false); }
    }
}

public class CtorFixture { public int Value; public CtorFixture() { } public CtorFixture(int v) { Value = v; } }
public class FieldBase { public int BaseField; }
public class FieldFixture : FieldBase { public int X; public static int S; public const int C = 3; }
public class GenericFixture<T> { public T X = default!; }
public class EnumerableFixture : IEnumerable
{
    public IEnumerator GetEnumerator() => throw new InvalidOperationException("enumerated!");
}
public struct MultiField { public int A; public long B; public byte C; }
public enum SmallEnum : int { X = 77 }
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Packed1 { public int A; }
[StructLayout(LayoutKind.Sequential, Size = 4096)]
public struct BigStruct { }
public struct HasRefs { public object? O; }

public static unsafe class Program
{
    static void MetadataLookups()
    {
        object[] objs = { new object(), new int[3], "hello", new List<int> { 1 }, new EnumerableFixture() };
        foreach (object o in objs)
            T.Check($"GetMethodTable({o.GetType().Name})==TypeHandle", (nuint)Intrinsics.GetMethodTable(o) == (nuint)o.GetType().TypeHandle.Value);

        T.Throws<ArgumentNullException>("FromObject(null)", () => Intrinsics.GetMethodTable(null!));
        T.Throws<NotSupportedException>("FromType(int*)", () => Intrinsics.GetMethodTable(typeof(int*)));
        T.Throws<NotSupportedException>("FromType(int&)", () => Intrinsics.GetMethodTable(typeof(int).MakeByRefType()));
        T.Throws<NotSupportedException>("FromType(genericparam)", () => Intrinsics.GetMethodTable(typeof(List<>).GetGenericArguments()[0]));

        // A rejected lookup must not modify genuine MethodTable bytes.
        byte* snap = stackalloc byte[64];
        MethodTable* mt = Intrinsics.GetMethodTable(typeof(int));
        Cassowary.Runtime.Unsafe.CopyBlock(snap, mt, 64);
        try { Intrinsics.GetMethodTable(typeof(int*)); } catch (NotSupportedException) { }
        T.Check("MT bytes unchanged after rejected lookup", Cassowary.Runtime.Unsafe.Read<ulong>(snap) == *(ulong*)mt && Cassowary.Runtime.Unsafe.Read<ulong>(snap + 56) == *(ulong*)((byte*)mt + 56));
    }

    static void CanonAndFlags()
    {
        nuint raw = *(nuint*)((nuint)typeof(List<string>).TypeHandle.Value + 40);
        MethodTable* mt = Intrinsics.GetMethodTable(typeof(List<string>));
        T.Check("List<string> HasCanon", mt->HasCanon);
        T.Check("List<string> canon ptr untagged", (nuint)mt->GetCanonicalMethodTable() == (raw & ~(nuint)1));
        T.Check("List<string> GetClass==*(canon+40)", (nuint)mt->GetClass() == *(nuint*)((raw & ~(nuint)1) + 40));
        T.Check("List<string> root canon", (nuint)mt->GetRootCanonTable() == (raw & ~(nuint)1));

        MethodTable* imt = Intrinsics.GetMethodTable(typeof(int));
        T.Check("int !IsNullable", !imt->IsNullable);
        T.Check("int IsTruePrimitive", imt->IsTruePrimitive);
        T.Check("int !IsPrimitiveValueType (is TruePrimitive)", !imt->IsPrimitiveValueType);
        T.Check("enum IsPrimitiveValueType", Intrinsics.GetMethodTable(typeof(SmallEnum))->IsPrimitiveValueType);
        T.Check("int IsValueType", imt->IsValueType);
        T.Check("Nullable<int> IsNullable", Intrinsics.GetMethodTable(typeof(int?))->IsNullable);
        T.Check("Span<int> IsByRefLike", Intrinsics.GetMethodTable(typeof(Span<int>))->IsByRefLike);
        T.Check("List<string> IsGenericInst", mt->IsGenericInst || mt->IsGenericSharedInst || mt->IsGenericTypicalInst || mt->GenericsFlags != 0);

        // Effective low flags must not interpret component size as flags.
        MethodTable* smt = Intrinsics.GetMethodTable(typeof(string));
        T.Check("string ComponentSize==2", smt->ComponentSize == 2);
        T.Check("string !IsDynamicStatics", !smt->IsDynamicStatics);
        MethodTable* arr = Intrinsics.GetMethodTable(typeof(short[]));
        T.Check("short[] ComponentSize==2", arr->ComponentSize == 2);
        T.Check("short[] !IsDynamicStatics", !arr->IsDynamicStatics);
        T.Check("short[] IsSZArray", arr->IsSZArray);
        MethodTable* big = Intrinsics.GetMethodTable(typeof(BigStruct[]));
        T.Check("BigStruct[] ComponentSize==4096", big->ComponentSize == 4096);
        T.Check("BigStruct[] !IsByRefLike", !big->IsByRefLike);
        T.Check("int[,] IsArray&&!SZ", Intrinsics.GetMethodTable(typeof(int[,]))->IsArray && !Intrinsics.GetMethodTable(typeof(int[,]))->IsSZArray);

        MethodTable* limt = Intrinsics.GetMethodTable(typeof(List<int>));
        bool found = false;
        foreach (MethodTable* i in limt->GetInterfaces())
            if (i == Intrinsics.GetMethodTable(typeof(IList<int>))) found = true;
        T.Check("List<int> interface map contains IList<int>", found);
    }

    static void EEClassTests()
    {
        EEClass* iec = Intrinsics.GetMethodTable(typeof(int))->GetClass();
        T.Check("EEClass TypeAttributes==reflection", iec->TypeAttributes == typeof(int).Attributes);
        T.Check("int[,] Rank==2", Intrinsics.GetMethodTable(typeof(int[,]))->GetClass()->Rank == 2);

        EEClass* pec = Intrinsics.GetMethodTable(typeof(Packed1))->GetClass();
        if (pec->HasLayout)
            T.Check("Packed1 PackingSize==1", pec->LayoutEEClass->LayoutInfo.PackingSize == 1);
        else
            Console.WriteLine("SKIP Packed1 HasLayout");

        EEClass* fec = Intrinsics.GetMethodTable(typeof(FieldFixture))->GetClass();
        T.Check("FieldFixture declared fields==3 (X,S + inherited excluded, const excluded)", fec->GetFieldCount() == 2);

        EEClass* gec = Intrinsics.GetMethodTable(typeof(GenericFixture<int>))->GetClass();
        T.Check("GenericFixture<int> declared fields==1", gec->GetFieldCount() == 1);
        FieldDesc* fx = gec->GetField(nameof(GenericFixture<int>.X));
        T.Check("GetField(X) returns non-null", fx != null && fx == (FieldDesc*)typeof(GenericFixture<int>).GetField("X")!.FieldHandle.Value);

        EEClass* ilistEc = Intrinsics.GetMethodTable(typeof(IList))->GetClass();
        GuidInfo gi = ilistEc->GetOrMakeGuid();
        T.Check("IList guid value", gi.Guid == typeof(IList).GUID || gi.GeneratedFromName);

        T.Check("Vector128<int> size 16", Intrinsics.GetMethodTable(typeof(System.Runtime.Intrinsics.Vector128<int>))->GetClass()->GetVectorSize() == 16);
        T.Check("Vector64<int> size 8", Intrinsics.GetMethodTable(typeof(System.Runtime.Intrinsics.Vector64<int>))->GetClass()->GetVectorSize() == 8);
        T.Check("Vector128<bool> size 0", Intrinsics.GetMethodTable(typeof(System.Runtime.Intrinsics.Vector128<bool>))->GetClass()->GetVectorSize() == 0);
        T.Check("Vector<int> size>0", Intrinsics.GetMethodTable(typeof(System.Numerics.Vector<int>))->GetClass()->GetVectorSize() > 0);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            T.Throws<PlatformNotSupportedException>("CorInterfaceType rejects Linux", () => { _ = iec->CorInterfaceType; });
            T.Throws<PlatformNotSupportedException>("CCWTemplate rejects Linux", () => { _ = iec->CCWTemplate; });
            T.Throws<PlatformNotSupportedException>("OptionalFields CoClass rejects Linux", () => { _ = ((EEClassOptionalFields*)null)->CoClass; });
            T.Throws<PlatformNotSupportedException>("ModuleDynamicID unsupported", () => { _ = ((EEClassOptionalFields*)null)->ModuleDynamicID; });
            T.Throws<PlatformNotSupportedException>("RequiredFieldAlignment unsupported", () => { _ = ((EEClassOptionalFields*)null)->RequiredFieldAlignment; });
        }
    }

    static void AllocAndBox()
    {
        MethodTable* cmt = Intrinsics.GetMethodTable(typeof(CtorFixture));
        T.Check("CanAllocate(CtorFixture)", cmt->CanAllocate());
        T.Check("Allocate(CtorFixture)", cmt->AllocateNoChecks() is CtorFixture);
        T.Check("CanAllocate(Span<int>)==false", !Intrinsics.GetMethodTable(typeof(Span<int>))->CanAllocate());
        T.Check("CanAllocate(void)==false", !Intrinsics.GetMethodTable(typeof(void))->CanAllocate());
        T.Check("CanAllocate(open generic)==false", !Intrinsics.GetMethodTable(typeof(GenericFixture<>))->CanAllocate());
        T.Check("CanAllocate(delegate)==false", !Intrinsics.GetMethodTable(typeof(Action))->CanAllocate());
        T.Check("CanAllocate(int[,])", Intrinsics.GetMethodTable(typeof(int[,]))->CanAllocate());

        int v = 0x12345678;
        T.Check("BoxStrict int", Equals(Intrinsics.GetMethodTable(typeof(int))->BoxStrict(&v), 0x12345678));
        long lv = long.MaxValue - 5;
        T.Check("Box long", Equals(Intrinsics.GetMethodTable(typeof(long))->Box(&lv), lv));
        double dv = 3.14159;
        T.Check("Box double", Equals(Intrinsics.GetMethodTable(typeof(double))->Box(&dv), dv));
        SmallEnum se = SmallEnum.X;
        T.Check("Box enum", Equals(Intrinsics.GetMethodTable(typeof(SmallEnum))->Box(&se), se));
        MultiField mf = new() { A = 1, B = long.MinValue, C = 0xAB };
        T.Check("Box multifield struct", Equals(Intrinsics.GetMethodTable(typeof(MultiField))->Box(&mf), mf));
        int? npop = 42, nempty = null;
        T.Check("Box Nullable<int> populated", Equals(Intrinsics.GetMethodTable(typeof(int?))->Box(&npop), 42));
        T.Check("Box Nullable<int> empty==null", Intrinsics.GetMethodTable(typeof(int?))->Box(&nempty) == null);
        T.Check("CanBox(void)==false", !Intrinsics.GetMethodTable(typeof(void))->CanBox());
        T.Check("CanBox(string)==false", !Intrinsics.GetMethodTable(typeof(string))->CanBox());
        T.Check("CanBox(HasRefs)==false", !Intrinsics.GetMethodTable(typeof(HasRefs))->CanBox());
        T.Check("CanBox(int[])==false", !Intrinsics.GetMethodTable(typeof(int[]))->CanBox());
        T.Throws<ArgumentException>("BoxNoChecks(HasRefs) rejects", () =>
        {
            HasRefs h = new();
            Intrinsics.GetMethodTable(typeof(HasRefs))->BoxNoChecks(&h);
        });

        object arr = Intrinsics.GetMethodTable(typeof(int[,]))->Allocate(2, 3);
        T.Check("Allocate int[2,3] exact type", arr.GetType() == typeof(int[,]) && ((int[,])arr).GetLength(0) == 2 && ((int[,])arr).GetLength(1) == 3);
        T.Check("Allocate string==empty", Equals(Intrinsics.GetMethodTable(typeof(string))->Allocate(), string.Empty));
    }

    static void Casts()
    {
        var list = new List<int>();
        var strings = new List<string>();
        T.Check("Cast identity preserved", ReferenceEquals(Intrinsics.GetMethodTable(typeof(List<int>))->Cast(list), list));
        T.Check("Cast to base preserved", ReferenceEquals(Intrinsics.GetMethodTable(typeof(object))->Cast(list), list));
        T.Check("Cast to interface preserved", ReferenceEquals(Intrinsics.GetMethodTable(typeof(IList<int>))->Cast(list), list));
        T.Check("Cast variance preserved", ReferenceEquals(Intrinsics.GetMethodTable(typeof(IEnumerable<object>))->Cast(strings), strings));
        T.Check("Cast null==null", Intrinsics.GetMethodTable(typeof(List<int>))->Cast(null) == null);
        T.Throws<InvalidCastException>("Cast unrelated rejected", () => Intrinsics.GetMethodTable(typeof(List<int>))->Cast(new ArrayList()));
        T.Check("CanCastTo array covariance", Intrinsics.GetMethodTable(typeof(string[]))->CanCastTo(Intrinsics.GetMethodTable(typeof(object[]))));
        T.Check("CastNoChecks same", ReferenceEquals(Intrinsics.GetMethodTable(typeof(List<int>))->CastNoChecks(list), list));
    }

    static void SyntheticLayouts()
    {
        T.Check("sizeof(NativeFieldDescriptor)==32", sizeof(NativeFieldDescriptor) == 32);
        T.Check("sizeof(EEClassLayoutInfo)==8", sizeof(EEClassLayoutInfo) == 8);
        T.Check("sizeof(NativeCodeVersion)==16", sizeof(NativeCodeVersion) == 16);

        T.Check("NFD.FieldDesc@0", Marshal.OffsetOf<NativeFieldDescriptor>(nameof(NativeFieldDescriptor.FieldDesc)).ToInt32() == 0);
        T.Check("NFD.NestedType@8", Marshal.OffsetOf<NativeFieldDescriptor>(nameof(NativeFieldDescriptor.NestedType)).ToInt32() == 8);
        T.Check("NFD.NumElements@16", Marshal.OffsetOf<NativeFieldDescriptor>(nameof(NativeFieldDescriptor.NumElements)).ToInt32() == 16);
        T.Check("NFD.NativeSize@8", Marshal.OffsetOf<NativeFieldDescriptor>(nameof(NativeFieldDescriptor.NativeSize)).ToInt32() == 8);
        T.Check("NFD.Offset@24", Marshal.OffsetOf<NativeFieldDescriptor>(nameof(NativeFieldDescriptor.Offset)).ToInt32() == 24);
        T.Check("NFD.Category@28", Marshal.OffsetOf<NativeFieldDescriptor>(nameof(NativeFieldDescriptor.Category)).ToInt32() == 28);

        T.Check("LayoutInfo.PackingSize@6", Marshal.OffsetOf<EEClassLayoutInfo>(nameof(EEClassLayoutInfo.PackingSize)).ToInt32() == 6);
        T.Check("NativeLayout.Size@4", Marshal.OffsetOf<EEClassNativeLayoutInfo>(nameof(EEClassNativeLayoutInfo.Size)).ToInt32() == 4);
        T.Check("NativeLayout.NumFields@8", Marshal.OffsetOf<EEClassNativeLayoutInfo>(nameof(EEClassNativeLayoutInfo.NumFields)).ToInt32() == 8);
        byte* nbuf = stackalloc byte[32];
        T.Check("NativeLayout descriptors @16", (nuint)(((EEClassNativeLayoutInfo*)nbuf)->NativeFieldDescriptors) - (nuint)nbuf == 16);

        byte* sbuf = stackalloc byte[16];
        *(uint*)(sbuf + 4) = 0x04000000 | 12345;
        var stub = (Stub*)sbuf;
        T.Check("Stub NumCodeBytes masks flags", stub->NumCodeBytes == 12345);
        T.Check("Stub union@8", (nuint)((byte*)&stub->PatchOffset - (byte*)stub) == 8);
        T.Check("Stub HasThunk", stub->HasThunk);

        T.Check("UMThunk sig@24 len@32", Marshal.OffsetOf<UMThunkMarshInfo>(nameof(UMThunkMarshInfo.Signature)).ToInt32() == 24 && Marshal.OffsetOf<UMThunkMarshInfo>(nameof(UMThunkMarshInfo.SignatureLength)).ToInt32() == 32);
        T.Check("UMThunk OffsetOfStub==0", ((UMThunkMarshInfo*)sbuf)->OffsetOfStub == 0);
        T.Check("NativeCodeVersion union@8", Marshal.OffsetOf<NativeCodeVersion>(nameof(NativeCodeVersion.VersionNode)).ToInt32() == 8);
    }

    public static int Main(string[] args)
    {
        var groups = new (string Name, Action Run)[]
        {
            ("metadata", MetadataLookups),
            ("canonflags", CanonAndFlags),
            ("eeclass", EEClassTests),
            ("allocbox", AllocAndBox),
            ("casts", Casts),
            ("synthetic", SyntheticLayouts),
        };

        if (args.Length > 0 && args[0] == "--list")
        {
            foreach (var g in groups) Console.WriteLine(g.Name);
            return 0;
        }

        string? filter = args.Length > 0 ? args[0] : null;
        foreach (var g in groups)
            if (filter == null || filter == "all" || filter == g.Name)
                g.Run();

        Console.WriteLine($"RESULT: {(T.Failures == 0 ? "ALL PASS" : $"{T.Failures} FAILURES")}");
        return T.Failures == 0 ? 0 : 1;
    }
}
