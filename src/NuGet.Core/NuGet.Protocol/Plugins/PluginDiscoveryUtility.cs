using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Common;

namespace NuGet.Protocol.Plugins
{
    public static class PluginDiscoveryUtility
    {
        public static string InternalPluginDiscoveryRoot { get; set; } = null;

        public static string GetInternalPlugins()
        {
            var rootDirectory = InternalPluginDiscoveryRoot ?? System.Reflection.Assembly.GetEntryAssembly()?.Location;;

            return rootDirectory ?? Path.GetDirectoryName(rootDirectory);
        }

        public static string GetNuGetHomePluginsPath()
        {
            var nuGetHome = NuGetEnvironment.GetFolderPath(NuGetFolderPath.NuGetHome);

            return Path.Combine(nuGetHome,
                "plugins",
#if IS_DESKTOP
                "netfx"
#else
                "netcore"
#endif
                );
        }

        public static IEnumerable<string> GetConventionBasedPlugins(IEnumerable<string> directories)
        {
            var paths = new List<string>();
            foreach (var directory in directories.Where(Directory.Exists))
            {
                var pluginDirectories = Directory.GetDirectories(directory);

                foreach (var pluginDirectory in pluginDirectories)
                {
#if IS_DESKTOP
                    var expectedPluginName = Path.Combine(pluginDirectory, Path.GetFileName(pluginDirectory) + ".exe");
#else
                    var expectedPluginName = Path.Combine(pluginDirectory, Path.GetFileName(pluginDirectory) + ".dll");
#endif

                    var filesInDirectory = Directory.EnumerateFiles(pluginDirectory);
                    if (filesInDirectory.Contains(expectedPluginName, PathUtility.GetStringComparerBasedOnOS()))
                    {
                        paths.Add(expectedPluginName);
                    }
                }
            }

            return paths;
        }
    }
}
