using System;
using System.Reflection;
using System.IO;

namespace Mqdf
{
    public static class MqdfParams
    {
        public static Stream GetLdaVectorStream()
        {
            return GetResource("ldaeig.bin");
        }

        public static Stream GetMqdfStream()
        {
            return GetResource("qdf.bin");
        }

        private static Stream GetResource(string resourceName)
        {
            Assembly assembly = typeof(MqdfParams).GetTypeInfo().Assembly;
            string[] names = assembly.GetManifestResourceNames();
            foreach (var name in names)
            {
                if (name.Contains(resourceName))
                    return assembly.GetManifestResourceStream(name);
            }
            return null;
        }
    }
}
