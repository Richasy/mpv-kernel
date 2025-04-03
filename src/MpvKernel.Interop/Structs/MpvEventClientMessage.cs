using System.Runtime.InteropServices;

namespace Richasy.MpvKernel;

/// <summary>
/// Message.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct MpvEventClientMessage
{
    /// <summary>
    /// <para>Arbitrary arguments chosen by the sender of the message. If num_args &gt; 0,</para>
    /// <para>you can access args[0] through args[num_args - 1] (inclusive). What</para>
    /// <para>these arguments mean is up to the sender and receiver.</para>
    /// <para>None of the valid items are NULL.</para>
    /// </summary>
    public int NumArgs;

    internal IntPtr _argsPtr; //Array of IntPtrs/UTF8 strings

    /// <summary>
    /// Returns an array of strings representing the arguments. The arguments are retrieved by calling the GetArgs
    /// method.
    /// </summary>
    public List<string> Args => [.. GetArgs()];

    private string[] GetArgs()
    {
        var args = new string[NumArgs];
        for (var i = 0; i < NumArgs; i++)
            args[i] = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(_argsPtr, i * IntPtr.Size)) ?? string.Empty;

        return args;
    }
}