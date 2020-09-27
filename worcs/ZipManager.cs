using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace worcs
{
    public static class ZipManager
    {
        public enum UnpackResult
        {
            Unspecified,
            Success,
            FileNotFound
        }

        private const string TempDirParentName = "Worcs";
        private const string RepoDirName = "repo";

        public static async Task<UnpackResult> Unpack(WorcsDocument document)
        {
            if (!document.HasFlag(WorcsDocumentFlags.WorkingDirExists))
            {
                var userTempDir = Path.GetTempPath();
                var tempDirName = await document.GetHashedFilename();

                document.WorkingDir = Path.Combine(userTempDir, TempDirParentName, tempDirName);
                document.RepoDir = Path.Combine(document.WorkingDir, RepoDirName);
                document.SetFlag(WorcsDocumentFlags.WorkingDirExists);
            }

            if (!File.Exists(document.WordFilepath))
                return UnpackResult.FileNotFound;

            await Console.Out.WriteLineAsync($"Unpacking word file {document.WordFilepath} to temp repo dir {document.RepoDir}.");

            ZipFile.ExtractToDirectory(document.WordFilepath, document.RepoDir, true);

            return UnpackResult.Success;
        }

        public enum PackResult
        {
            Unspecified = 0,
            Success,
            WorkingDirNotFound,
        }

        public static async Task<PackResult> Pack(WorcsDocument document)
        {
            if (!document.HasFlag(WorcsDocumentFlags.WorkingDirExists))
                return PackResult.WorkingDirNotFound;

            var workingDir = document.WorkingDir;
            var archivePath = Path.Combine(workingDir, "temp.docx");

            ZipFile.CreateFromDirectory(document.RepoDir, archivePath, CompressionLevel.Optimal, false);

            var wordFileDir = Path.GetDirectoryName(document.WordFilepath) ?? "";
            var backupName = document.WordFilename + ".old";
            var backupPath = Path.Combine(wordFileDir, backupName);

            File.Move(document.WordFilepath, backupPath, true);
            File.Copy(archivePath, document.WordFilepath);
            File.Delete(archivePath);

            return PackResult.Success;
        }
    }
}