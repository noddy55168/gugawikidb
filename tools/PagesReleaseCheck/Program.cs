using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;

// 只讀驗證已建置的 Vue 公開成品；不連資料庫、不重新匯出、不修改網站。
try
{
    if (args.Length != 1) throw new InvalidDataException("Usage: PagesReleaseCheck <site-directory>");
    ReleaseCheck.Verify(Path.GetFullPath(args[0]));
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine($"Release rejected: {error.Message}");
    return 1;
}

/// <summary>發布閘門：任何清單、版本、路徑、檔案內容或連結異常都拒絕發布。</summary>
internal static class ReleaseCheck
{
    private static readonly Regex Allowed = new(@"\A(?:index\.html|\.nojekyll|assets/[A-Za-z0-9_-]+\.(?:js|css)|static-data/manifest\.json|static-data/data/[a-f0-9]{24}/[a-f0-9]{64}\.json|media/[A-Za-z0-9_./-]+\.png)\z");

    /// <summary>依 release.json 核對完整檔案集合與 SHA256，並比對公開資料版本。</summary>
    public static void Verify(string root)
    {
        var actual = new Dictionary<string, string>(StringComparer.Ordinal);
        Visit(new DirectoryInfo(root));
        void Visit(DirectoryInfo directory)
        {
            if (!directory.Exists || directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
                throw new InvalidDataException("Missing directory or linked directory");
            // 先檢查連結，再遞迴；不能讓檔案遍歷越過發布根目錄。
            foreach (var entry in directory.EnumerateFileSystemInfos())
            {
                if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new InvalidDataException("Links are not allowed");
                if (entry is DirectoryInfo child) Visit(child);
                else actual.Add(Path.GetRelativePath(root, entry.FullName).Replace('\\', '/'), entry.FullName);
            }
        }
        using var releaseDoc = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "release.json")));
        var release = releaseDoc.RootElement;
        if (release.GetProperty("schemaVersion").GetString() != "endfield.pages.v1" || release.GetProperty("base").GetString() != "/gugawikidb/")
            throw new InvalidDataException("Wrong release schema or repository base");
        var files = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var file in release.GetProperty("files").EnumerateObject()) files.Add(file.Name, file.Value);
        var expected = files.Keys.Append("release.json").ToHashSet(StringComparer.Ordinal);
        if (!expected.SetEquals(actual.Keys)) throw new InvalidDataException("Unexpected or missing files");
        LinkCheck.Verify(actual.Values.ToArray());
        long total = new FileInfo(actual["release.json"]).Length;
        foreach (var (name, item) in files)
        {
            if (!Allowed.IsMatch(name) || name.Split('/').Any(part => part is "" or "." or ".."))
                throw new InvalidDataException($"Disallowed file: {name}");
            using var stream = File.OpenRead(actual[name]);
            var length = stream.Length;
            var hash = Convert.ToHexStringLower(SHA256.HashData(stream));
            if (length != item.GetProperty("bytes").GetInt64() || hash != item.GetProperty("sha256").GetString())
                throw new InvalidDataException($"Hash or size mismatch: {name}");
            total = checked(total + length);
        }
        if (total >= 1_000_000_000) throw new InvalidDataException("Website must remain below the local 1 GB guardrail");
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(actual["static-data/manifest.json"]));
        var releaseId = release.GetProperty("dataReleaseId").GetString();
        if (manifest.RootElement.GetProperty("releaseId").GetString() != releaseId)
            throw new InvalidDataException("Data release mismatch");
        Console.WriteLine($"Verified {actual.Count} files, {total} bytes, data {releaseId}");
    }
}

/// <summary>保留舊發布閘門的硬連結防護；Windows 用檔案 handle，Linux 用系統 stat。</summary>
internal static class LinkCheck
{
    /// <summary>所有公開檔案只能有一個硬連結；不支援的平台明確失敗。</summary>
    public static void Verify(string[] paths)
    {
        if (OperatingSystem.IsWindows())
        {
            foreach (var path in paths)
            {
                using var handle = File.OpenHandle(path);
                if (!GetFileInformationByHandle(handle, out var info) || info.NumberOfLinks != 1)
                    throw new InvalidDataException($"Cannot verify single hard link: {path}");
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            // 分批避免命令列長度限制；ArgumentList 不經 shell，不插值執行檔案名稱。
            foreach (var batch in paths.Chunk(100))
            {
                var start = new ProcessStartInfo("/usr/bin/stat") { RedirectStandardOutput = true, UseShellExecute = false };
                start.ArgumentList.Add("--format=%h");
                start.ArgumentList.Add("--");
                foreach (var path in batch) start.ArgumentList.Add(path);
                using var process = Process.Start(start) ?? throw new IOException("Cannot start stat");
                var counts = process.StandardOutput.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                process.WaitForExit();
                if (process.ExitCode != 0 || counts.Length != batch.Length || counts.Any(count => count.Trim() != "1"))
                    throw new InvalidDataException("Hard link check failed");
            }
        }
        else throw new PlatformNotSupportedException("Use Windows or Linux for release verification");
    }

    // 對應 Win32 BY_HANDLE_FILE_INFORMATION；時間以 FILETIME 的兩個 uint 保留原始布局。
    [StructLayout(LayoutKind.Sequential)]
    private struct FileInformation
    {
        public uint Attributes, CreationLow, CreationHigh, AccessLow, AccessHigh, WriteLow, WriteHigh;
        public uint VolumeSerial, SizeHigh, SizeLow, NumberOfLinks, IndexHigh, IndexLow;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out FileInformation information);
}
