using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OCSoft.Common.IO
{
    public class FileExtensions
    {
        public static FileInfo GetFile(params string[] segments)
        {
            string path = EnsureDirectoryExists(segments.Take(segments.Length - 1).ToArray());
            return new FileInfo(Path.Combine(path, segments.Last()));
        }

        public static string EnsureDirectoryExists(params string[] segments)
        {
            string path = Path.Combine(segments);
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static T UsingJsonCache<T>(string filename, Func<T> action)
        {
            var file = GetFile(AppContext.BaseDirectory, "cache", filename);
            return UsingJsonCache(file, action);
        }

        public static T UsingJsonCache<T>(FileInfo file, Func<T> action)
        {
            if (file.Exists)
            {
                return ReadJson<T>(file);
            }

            var result = action();

            CreateCache(file, result);
            return result;
        }

        public static T ReadJson<T>(FileInfo file)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(file.FullName));
        }

        private static void CreateCache<T>(FileInfo file, T result)
        {
            using (var writer = file.CreateText())
            {
                var content = JsonConvert.SerializeObject(result, Formatting.Indented);
                writer.Write(content);
                writer.Close();
            }
        }

        public static string GetVersionedName(string dir, string name, string ext)
        {
            EnsureDirectoryExists(dir);

            return new VersionedName(dir, name, ext).ToString();
        }
    }

    class VersionedName
    {
        private readonly string _dir;
        private readonly string _name;
        private readonly string _ext;
        private int _index = 0;

        public VersionedName(string dir, string name, string ext)
        {
            _dir = dir;
            _name = name;
            _ext = ext;

            var match = Regex.Match(name, @"(^.+)_(\d{4})(\.\w+)$");
            if (match.Groups.Count > 2)
            {
                _name = match.Groups[1].Value;
                _index = int.Parse(match.Groups[2].Value);
                _ext = match.Groups[3].Value;
            }
        }

        public override string ToString()
        {
            string path;
            do
            {
                path = Path.Combine(_dir, $"{_name}_{++_index}.{_ext}");
            }
            while (File.Exists(path));

            return path;
        }
    }
}
