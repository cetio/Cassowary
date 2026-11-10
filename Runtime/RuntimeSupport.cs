using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Cassowary.Runtime
{
    /// <summary>
    ///     Declares and enforces the runtime/OS/architecture combinations for which the native
    ///     CoreCLR layout projections in <c>Cassowary.Runtime.VM</c> are defined.
    /// </summary>
    /// <remarks>
    ///     Supported: .NET 8 CoreCLR, release runtime, Windows x64 or Linux x64.
    ///     Rejected: Mono, .NET Framework, .NET versions other than 8, NativeAOT-compiled
    ///     applications, ARM32/ARM64/x86, and all other operating systems.
    ///     Checked/debug CoreCLR builds add additional fields whose presence cannot be
    ///     detected reliably from managed code, those builds are not claimed to be
    ///     supported, but no detection is attempted.
    ///     <para>
    ///         Any API returning a native pointer view (MethodTable*, EEClass*, etc.) requires
    ///         the caller to keep the <see cref="Type"/> / <see cref="System.Reflection.Assembly"/>
    ///         and its containing type load context alive for as long as the pointer is used.
    ///         Pointers are views into runtime-owned metadata, they must never be written
    ///         through or freed.
    ///     </para>
    /// </remarks>
    internal static class RuntimeSupport
    {
        /// <summary>
        ///     Gets a value indicating whether the process runs on an x64 processor.
        /// </summary>
        public static bool IsX64 => RuntimeInformation.ProcessArchitecture == Architecture.X64;

        /// <summary>
        ///     Gets a value indicating whether the process runs on Windows.
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        ///     Gets a value indicating whether the process runs on Linux.
        /// </summary>
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        ///     Gets a value indicating whether the current configuration is supported by the
        ///     native CoreCLR layout projections. Does not throw.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                // CoreCLR only: reject Mono and other runtimes by description and probe type.
                if (!RuntimeInformation.FrameworkDescription.StartsWith(".NET ", StringComparison.Ordinal))
                    return false;

                if (Type.GetType("Mono.Runtime") != null)
                    return false;

                // NativeAOT-compiled code has no JIT compiler.
                if (!RuntimeFeature.IsDynamicCodeCompiled)
                    return false;

                // Only the .NET 8 CoreCLR layouts are modeled.
                if (Environment.Version.Major != 8)
                    return false;

                if (!IsX64)
                    return false;

                return IsWindows || IsLinux;
            }
        }

        /// <summary>
        ///     Throws <see cref="PlatformNotSupportedException"/> when the current process does not
        ///     run on a supported .NET 8 CoreCLR x64 configuration. Called eagerly by every API
        ///     that exposes native runtime pointers.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">
        ///     The process is not .NET 8 CoreCLR on Windows x64 or Linux x64.
        /// </exception>
        public static void EnsureSupported()
        {
            if (!IsSupported)
            {
                throw new PlatformNotSupportedException(
                    $"Native runtime layout inspection requires .NET 8 CoreCLR on Windows x64 or Linux x64. " +
                    $"Detected: {RuntimeInformation.FrameworkDescription}, " +
                    $"{RuntimeInformation.OSDescription}, {RuntimeInformation.ProcessArchitecture}, " +
                    $"dynamic code compiled: {RuntimeFeature.IsDynamicCodeCompiled}.");
            }
        }
    }
}
