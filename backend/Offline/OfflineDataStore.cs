using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace InfyPOS.Processors
{
    /// <summary>
    /// Named offline data files under App.config "Data" folder.
    /// </summary>
    public enum OfflineDataFile
    {
        Master,
        Stock,
        Bill,
        Settlement,
        Customer,
        Offline,
        Sequence,
    }

    /// <summary>
    /// Single entry point for all offline .data read/write.
    /// Thread-safe per file; all services should use this instead of File.* directly.
    /// </summary>
    public sealed class OfflineDataStore
    {
        private static readonly OfflineDataStore instance = new OfflineDataStore();
        private readonly ConcurrentDictionary<string, object> _locks =
            new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public static OfflineDataStore Instance => instance;

        private OfflineDataStore() { }

        /// <summary>Same as BillManager.datadirectory (App.config Data).</summary>
        public string DataDirectory
        {
            get
            {
                var datafolder = System.Configuration.ConfigurationManager.AppSettings["Data"];
                if (string.IsNullOrEmpty(datafolder))
                {
                    var file = new FileInfo(Assembly.GetExecutingAssembly().Location);
                    return file.DirectoryName ?? AppContext.BaseDirectory;
                }
                return datafolder;
            }
        }

        public string GetPath(OfflineDataFile file) =>
            Path.Combine(DataDirectory, ToFileName(file));

        public string GetPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.", nameof(fileName));
            // Prevent path traversal — only allow a simple file name in Data folder
            var name = Path.GetFileName(fileName);
            return Path.Combine(DataDirectory, name);
        }

        public bool Exists(OfflineDataFile file) => Exists(ToFileName(file));

        public bool Exists(string fileName)
        {
            var path = GetPath(fileName);
            lock (Gate(path))
            {
                return File.Exists(path);
            }
        }

        public byte[] ReadAllBytes(OfflineDataFile file) => ReadAllBytes(ToFileName(file));

        public byte[] ReadAllBytes(string fileName)
        {
            var path = GetPath(fileName);
            lock (Gate(path))
            {
                EnsureDirectory();
                if (!File.Exists(path))
                    return null;
                return File.ReadAllBytes(path);
            }
        }

        public string ReadAllText(OfflineDataFile file) => ReadAllText(ToFileName(file));

        public string ReadAllText(string fileName)
        {
            var path = GetPath(fileName);
            lock (Gate(path))
            {
                EnsureDirectory();
                if (!File.Exists(path))
                    return null;
                return File.ReadAllText(path);
            }
        }

        public void WriteAllBytes(OfflineDataFile file, byte[] content) =>
            WriteAllBytes(ToFileName(file), content);

        public void WriteAllBytes(string fileName, byte[] content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var path = GetPath(fileName);
            lock (Gate(path))
            {
                EnsureDirectory();
                // Atomic-ish replace: write temp then move
                var temp = path + ".tmp";
                File.WriteAllBytes(temp, content);
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(temp, path);
            }
        }

        public void WriteAllText(string fileName, string content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var path = GetPath(fileName);
            lock (Gate(path))
            {
                EnsureDirectory();
                var temp = path + ".tmp";
                File.WriteAllText(temp, content);
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(temp, path);
            }
        }

        public void Delete(OfflineDataFile file) => Delete(ToFileName(file));

        public void Delete(string fileName)
        {
            var path = GetPath(fileName);
            lock (Gate(path))
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        public static string ToFileName(OfflineDataFile file)
        {
            switch (file)
            {
                case OfflineDataFile.Master: return "master.data";
                case OfflineDataFile.Stock: return "stock.data";
                case OfflineDataFile.Bill: return "bill.data";
                case OfflineDataFile.Settlement: return "settlement.data";
                case OfflineDataFile.Customer: return "customer.data";
                case OfflineDataFile.Offline: return "offline.data";
                case OfflineDataFile.Sequence: return "sequence.data";
                default:
                    throw new ArgumentOutOfRangeException(nameof(file), file, null);
            }
        }

        private object Gate(string path) =>
            _locks.GetOrAdd(path, _ => new object());

        private void EnsureDirectory()
        {
            var dir = DataDirectory;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
