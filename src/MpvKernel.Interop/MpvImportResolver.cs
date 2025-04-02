// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.InteropServices;

namespace Richasy.MpvKernel;

internal static class MpvImportResolver
{
    private static readonly Dictionary<string, IntPtr> _libraryHandles = [];
    private static string _mpvDllPath;
    private static bool _isInitialized;

    public static void Initialize(string mpvDllPath)
    {
        if (_isInitialized && mpvDllPath == _mpvDllPath)
        {
            return;
        }

        if (!_isInitialized)
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), ImportResolver);
        }

        if (_libraryHandles.TryGetValue("mpv", out var value))
        {
            NativeLibrary.Free(value);
            _libraryHandles.Remove("mpv");
        }

        _mpvDllPath = mpvDllPath;
        _isInitialized = true;
    }

    private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (_libraryHandles.TryGetValue(libraryName, out var handle))
        {
            return handle;
        }

        var fileName = string.Empty;
        if (libraryName == "mpv")
        {
            fileName = string.IsNullOrEmpty(_mpvDllPath) ? "libmpv-2.dll" : _mpvDllPath;
        }

        _libraryHandles[libraryName] = NativeLibrary.Load(fileName, assembly, searchPath);
        return _libraryHandles[libraryName];
    }
}
