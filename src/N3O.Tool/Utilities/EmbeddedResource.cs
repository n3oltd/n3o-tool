using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace N3O.Tool.Utilities {
    public static class EmbeddedResource {
        public static string Text(string embeddedFileName) {
            var assembly = Assembly.GetEntryAssembly();
            var embeddedResourceNames = assembly.GetManifestResourceNames();
            var resourceName = embeddedResourceNames.First(s => s.EndsWith(embeddedFileName, 
                                                                           StringComparison.CurrentCultureIgnoreCase));

            using (var stream = assembly.GetManifestResourceStream(resourceName)) {
                using (var reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}