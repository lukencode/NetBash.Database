using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NetBash.Database
{
    internal static class EmbeddedResourceHelper
    {
        internal static string GetResource(string filename)
        {
            string result;

            if (!_ResourceCache.TryGetValue(filename, out result))
            {
                using (var stream = typeof(EmbeddedResourceHelper).Assembly.GetManifestResourceStream("Netbash.Database.Scripts." + filename))
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                _ResourceCache[filename] = result;
            }

            return result;
        }

        /// <summary>
        /// Embedded resource contents keyed by filename.
        /// </summary>
        private static readonly Dictionary<string, string> _ResourceCache = new Dictionary<string, string>();
    }
}
