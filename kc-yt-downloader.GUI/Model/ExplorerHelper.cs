using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace kc_yt_downloader.GUI.Model;

public static class ExplorerHelper
{
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

    public static void OpenFolder(string folderPath)
        => Process.Start("explorer.exe", folderPath);

    public static void OpenFolderAndSelectItem(string folderPath, string file)
    {
        SHParseDisplayName(folderPath, IntPtr.Zero, out nint nativeFolder, 0, out uint psfgaoOut);

        if (nativeFolder == IntPtr.Zero)
            return;

        SHParseDisplayName(Path.Combine(folderPath, file), IntPtr.Zero, out nint nativeFile, 0, out psfgaoOut);

        IntPtr[] fileArray;
        if (nativeFile == IntPtr.Zero)
        {
            fileArray = [];
        }
        else
        {
            fileArray = [nativeFile];
        }

        SHOpenFolderAndSelectItems(nativeFolder, (uint)fileArray.Length, fileArray, 0);

        Marshal.FreeCoTaskMem(nativeFolder);
        if (nativeFile != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(nativeFile);
        }
    }
}
