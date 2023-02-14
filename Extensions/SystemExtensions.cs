using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class SystemExtensions
    {
        public static TResult WithTimeBreak<TResult>(this Stopwatch stopwatch, Func<TResult> action)
        {
            try
            {
                stopwatch.Stop();
                return action();
            }
            finally
            {
                stopwatch.Start();
            }
        }

        public static IEnumerable<string> GetEmbeddedResourceContent(this Assembly assembly, string resourcePostfix)
        {
            return assembly.GetManifestResourceNames()
                .Where(r => r.EndsWith(resourcePostfix))
                .Select(assembly.ReadResource);
        }

        private static string ReadResource(this Assembly assembly, string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
