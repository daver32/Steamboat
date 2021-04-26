using System.IO;
using System.Linq;
using System.Reflection;

namespace Steamboat.Util
{
    internal static class EmbeddedResourcesUtil
    {
        public static string ReadString(string name, Assembly assembly)
        {
            string resourcePath = assembly.GetManifestResourceNames()
                                          .Single(str => str.EndsWith(name));

            using Stream stream = assembly.GetManifestResourceStream(resourcePath)!;
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}