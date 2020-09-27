using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace worcs
{
    public class WorcsDocument
    {
        public WorcsDocumentFlags Flags { get; set; }

        public string WordFilepath { get; set; }

        public string WorkingDir { get; set; }
        public string RepoDir { get; set; }

        public string WordFilename => Path.GetFileName(WordFilepath);

        private string? hashedFilename;
        public async Task<string> GetHashedFilename()
        {
            if (hashedFilename != null)
                return hashedFilename;

            var filename = WordFilename;
            var filenameBytes = Encoding.UTF8.GetBytes(filename);
            var filenameStream = new MemoryStream(filenameBytes);

            using var algo = new SHA1Managed();
            var hashedBytes = await algo.ComputeHashAsync(filenameStream);
            var hash = hashedBytes.Select(b => b.ToString("X2"));

            return hashedFilename = string.Concat(hash);
        }

        public bool HasFlag(WorcsDocumentFlags flag) => (Flags & flag) != 0;

        public void SetFlag(WorcsDocumentFlags flag) => Flags |= flag;

        public void RemoveFlag(WorcsDocumentFlags flag) => Flags &= ~flag;
    }
}