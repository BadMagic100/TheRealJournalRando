using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ConstsGenerator
{
    internal static class GenTimeDependencies
    {
        private static bool set = false;

        public static void AddOnce()
        {
            if (set)
            {
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                AssemblyName name = new(args.Name);
                Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().FullName == name.FullName);
                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }

                string resourceName = $"ConstsGenerator.{name.Name}.dll";

                using Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (resourceStream == null)
                {
                    return null;
                }

                using MemoryStream memoryStream = new();
                resourceStream.CopyTo(memoryStream);

                return Assembly.Load(memoryStream.ToArray());
            };
            set = true;
        }
    }
}
