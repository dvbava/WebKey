using WebKey.Models;
using WebKey.Support;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.IO.IsolatedStorage;
using Android.OS;
using Android.App;
using Android.Net;
using Android.Content;
using System.Threading.Tasks;

namespace WebKey
{
    public interface IDataProvider
    {
        Task<IEnumerable<KeyEntry>> GetAll();
        Task<bool> SaveAll(IEnumerable<KeyEntry> items);
        void SaveComputedHash(string secret, bool overwrite = false);
        string GetComputedHash();
        bool IsolatedStorageFileExists(string name);
        string GetDataFilePath();
    }
    public class DataProvider : IDataProvider
    {
        IEncryptDecrypt _encryptDecrypt = null;

        public DataProvider(IEncryptDecrypt encryptDecrypt)
        {
            _encryptDecrypt = encryptDecrypt;
        }

        public async Task<IEnumerable<KeyEntry>> GetAll()
        {
            var entryLines = new List<KeyEntry>();
            await Task.Run(() => {               

                if (File.Exists(GetDataFilePath()))
                {
                    using (StreamReader stream = new StreamReader(new FileStream(GetDataFilePath(), FileMode.Open)))
                    {
                        while (!stream.EndOfStream)
                        {
                            var line = new KeyEntry { Id = entryLines.Count + 1, Header = _encryptDecrypt.Decrypt(stream.ReadLine()), Detail = _encryptDecrypt.Decrypt(stream.ReadLine()), Saved = true };
                            entryLines.Add(line);
                        }
                    }
                }
            });

            return entryLines;
        }

        public async Task<bool> SaveAll(IEnumerable<KeyEntry> items)
        {
            await Task.Run(() => {
                var dataLines = items.Select(i => new KeyEntry { Header = _encryptDecrypt.Encrypt(i.Header), Detail = _encryptDecrypt.Encrypt(i.Detail) });

                StringBuilder sb = new StringBuilder();
                foreach (var line in dataLines)
                {
                    sb.AppendLine(line.Header);
                    sb.AppendLine(line.Detail);
                }

                using (StreamWriter stream = new StreamWriter(new FileStream(GetDataFilePath(), FileMode.Create)))
                {
                    stream.Write(sb.ToString());
                }
            });

            return true;
        }

        public void SaveComputedHash(string secret, bool overwrite = false)
        {
            if (!overwrite && IsolatedStorageFileExists(Constants.LoginHashFile))
                throw new FileLoadException("User already registered and data available.");

            using (StreamWriter stream = new StreamWriter(new IsolatedStorageFileStream(Constants.LoginHashFile, FileMode.Create)))
            {
                stream.Write(secret);
            }
        }

        public string GetComputedHash()
        {
            if (!IsolatedStorageFileExists(Constants.LoginHashFile))
                throw new FileLoadException("User not registered.");

            using (StreamReader stream = new StreamReader(new IsolatedStorageFileStream(Constants.LoginHashFile, FileMode.Open)))
            {
                return stream.ReadLine();
            }
        }

        public bool IsolatedStorageFileExists(string name)
        {
            using (var folder = IsolatedStorageFile.GetUserStoreForDomain())
            {
                return folder.FileExists(name);
            }
        }

        public string GetDataFilePath()
        {
            var envPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            //var envPath = Application.Context.GetExternalFilesDir(null).AbsolutePath;
            return Path.Combine(envPath, Constants.FileName);
        }
    }
}