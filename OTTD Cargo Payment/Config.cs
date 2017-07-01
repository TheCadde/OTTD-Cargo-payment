using System.IO;
using System.Reflection;

namespace OTTD_Cargo_Payment {
    public static class Config {
        public static string AppDir {
            get {
                var location = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(location);
            }
        }

        public static string CargoSchemesDirectory => $"{AppDir}\\CargoSchemes";

        public static string NMLTemplateDirectory => $"{AppDir}\\NMLTemplates";

        public static string NMLOutputDirectory => $"{AppDir}\\Output";
    }
}
