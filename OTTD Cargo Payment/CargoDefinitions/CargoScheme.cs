using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace OTTD_Cargo_Payment.CargoDefinitions {
    [XmlInclude(typeof(CargoDefinition))]
    [Serializable]
    public class CargoScheme : ICloneable {
        [XmlIgnore]
        public bool IsDefault;

        public string SchemeName { get; set; } = "<Unknown>";

        public List<CargoDefinition> CargoDefinitions = new List<CargoDefinition>();

        public void Save() {
            Directory.CreateDirectory(Config.CargoSchemesDirectory);
            var fileName = SchemeName;
            Path.GetInvalidFileNameChars().Aggregate(fileName, (s, c) => s.Replace(c.ToString(), ""));
            var path = $"{Config.CargoSchemesDirectory}\\{fileName}.xml";

            var serializer = new XmlSerializer(typeof(CargoScheme));
            using (var file = File.Open(path, FileMode.Create, FileAccess.Write)) {
                serializer.Serialize(file, this);
            }
        }

        public static CargoScheme Load(string path) {
            var serializer = new XmlSerializer(typeof(CargoScheme));
            using (var file = File.OpenRead(path)) {
                return (CargoScheme)serializer.Deserialize(file);
            }
        }

        public override string ToString() {
            return $"{(IsDefault ? "[DEFAULT] " : "")}{SchemeName}";
        }

        public object Clone() {
            return MemberwiseClone();
        }
    }
}
