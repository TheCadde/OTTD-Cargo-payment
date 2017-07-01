using System;
using System.Diagnostics;
using System.IO;

using OTTD_Cargo_Payment.CargoDefinitions;

namespace OTTD_Cargo_Payment.NMLTemplates {
    public static class NMLWriter {
        private static readonly string[] perishableRegisters = {
                                                                   "distance",
                                                                   "time",
                                                                   "MaxDistance",
                                                                   "DiminishingReturnsLinearity",
                                                                   "PrimaryDropoffCurveOffset",
                                                                   "PrimaryDropoffCurveShortDistanceAdjustment",
                                                                   "PrimaryDropoffCurveShortDistanceEnd",
                                                                   "PrimaryDropoffCurveMediumDistanceStart",
                                                                   "PrimaryDropoffCurveMediumDistanceAdjustment",
                                                                   "PrimaryDropoffCurveMediumDistanceEnd",
                                                                   "PrimaryDropoffCurveLongDistanceStart",
                                                                   "PrimaryDropoffCurveLongDistanceAdjustment",
                                                                   "PrimaryDropoffCurveLongDistanceEnd",
                                                                   "PrimaryDropoffCurveLongevity",
                                                                   "PrimaryDropoffCurveLongevityAdjustment",
                                                                   "SecondaryReturnDivisor",
                                                                   "FinalReturnDivisor",
                                                                   "SecondaryDropoffCurveOffset",
                                                                   "SecondaryDropoffCurveDistanceAdjustment",
                                                                   "SecondaryDropoffCurveLongevity",
                                                                   "SecondaryDropoffCurveLongevityAdjustment",
                                                                   "diminishingReturns",
                                                                   "primaryDropoffCurveOffset",
                                                                   "primaryDropoffCurve",
                                                                   "secondaryDropoffCurveOffset",
                                                                   "secondaryDropoffCurve",
                                                               };

        private static readonly string[] bulkRegisters = {
                                                             "distance",
                                                             "time",
                                                             "MaxDistance",
                                                             "DiminishingReturnsLinearity",
                                                             "MaxDeliveryTime",
                                                             "LateCargoDivider",
                                                             "diminishingReturns",
                                                         };

        public static string WriteNML(CargoScheme scheme) {
            var outputDir = $"{Config.NMLOutputDirectory}\\{scheme.SchemeName}";
            var outputPath = $"{outputDir}\\cargomod_{scheme.SchemeName}.nml";
            var outputFileNml = $"cargomod_{scheme.SchemeName}.nml";
            var outputFileGrf = $"cargomod_{scheme.SchemeName.Replace(" ", "_")}.grf";

            PrepareNMLFile(scheme);
            WriteNMLCalculation(scheme, new PerishableCargoDefinition());
            WriteNMLCalculation(scheme, new BulkCargoDefinition());
            foreach (var cargoDef in scheme.CargoDefinitions)
                WriteNMLUserParams(scheme, cargoDef);
            foreach (var cargoDef in scheme.CargoDefinitions)
                WriteNMLCargoItem(scheme, cargoDef);

            var res = StartProcess(outputDir, "NMLC", $"-c --verbosity=3 --grf \"{outputFileGrf}\" \"{outputFileNml}\"");

            var ottdDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\OpenTTD\\newgrf";
            File.Copy($"{outputDir}\\{outputFileGrf}", $"{ottdDocs}\\{outputFileGrf}", true);

            return res;
        }

        private static string StartProcess(string workingDir, string fileName, string arguments) {
            var res = "";
            var process = new Process {
                    StartInfo = new ProcessStartInfo(fileName, arguments) {
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                    },
                };

            process.OutputDataReceived += (sender, args) => {
                if (string.IsNullOrWhiteSpace(args.Data))
                    return;
                res += $"> {args.Data}\n";
            };

            process.Start();
            process.BeginOutputReadLine();
            //process.BeginErrorReadLine();

            process.WaitForExit();

            var lines = process.StandardError.ReadToEnd().Split(new []{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
                res += $"\n! {string.Join("\n! ", lines)}";

            return res;
        }


        public static void PrepareNMLFile(CargoScheme scheme) {
            var outputDir = $"{Config.NMLOutputDirectory}\\{scheme.SchemeName}";
            var templateDir = $"{Config.NMLTemplateDirectory}";
            var schemeTemplateDir = $"{templateDir}\\{scheme.SchemeName}";
            var templateFile = $"{templateDir}\\cargomod_{scheme.SchemeName}.nml";
            var outputPath = $"{outputDir}\\cargomod_{scheme.SchemeName}.nml";

            Directory.CreateDirectory($"{Config.NMLOutputDirectory}");
            Directory.CreateDirectory(outputDir);

            if (Directory.Exists(schemeTemplateDir)) {
                foreach (var directory in Directory.GetDirectories(schemeTemplateDir))
                    Directory.CreateDirectory(directory.Replace(schemeTemplateDir, outputDir));
                foreach (var file in Directory.GetFiles(schemeTemplateDir, "*.*", SearchOption.AllDirectories))
                    File.Copy(file, file.Replace(schemeTemplateDir, outputDir), true);
            }
            var content = "";
            if (File.Exists(templateFile)) {
                content = File.ReadAllText(templateFile);
            }
            File.WriteAllText(outputPath, content);
        }

        public static void WriteNMLCalculation(CargoScheme scheme, CargoDefinition cargoDef) {
            var outputDir = $"{Config.NMLOutputDirectory}\\{scheme.SchemeName}";
            var templateDir = $"{Config.NMLTemplateDirectory}";
            var schemeTemplateDir = $"{templateDir}\\{scheme.SchemeName}";
            var templateFile = $"{templateDir}\\perishableCargoCalculations.nml";
            var outputPath = $"{outputDir}\\cargomod_{scheme.SchemeName}.nml";

            var registers = perishableRegisters;
            if (cargoDef.GetType() == typeof(BulkCargoDefinition)) {
                registers = bulkRegisters;
                templateFile = $"{templateDir}\\bulkCargoCalculations.nml";
            }

            var contents = File.ReadAllText(templateFile);

            for (var i = 0; i < registers.Length; i++) {
                var register = registers[i];
                contents = contents.Replace($"{{{{{register}}}}}", $"LOAD_TEMP({i})");
            }
            File.AppendAllText(outputPath, contents);
        }

        public static void WriteNMLUserParams(CargoScheme scheme, CargoDefinition cargoDef) {
            var outputDir = $"{Config.NMLOutputDirectory}\\{scheme.SchemeName}";
            var templateDir = $"{Config.NMLTemplateDirectory}";
            var schemeTemplateDir = $"{templateDir}\\{scheme.SchemeName}";
            var templateFile = $"{templateDir}\\perishableCargoUserParams.nml";
            var outputPath = $"{outputDir}\\cargomod_{scheme.SchemeName}.nml";

            var registers = perishableRegisters;
            if (cargoDef.GetType() == typeof(BulkCargoDefinition)) {
                registers = bulkRegisters;
                templateFile = $"{templateDir}\\bulkCargoUserParams.nml";
            }

            var contents = File.ReadAllText(templateFile);


            foreach (var register in registers) {
                var prop = cargoDef.GetType().GetProperty(register);
                if (prop == null)
                    continue;
                var value = prop.GetValue(cargoDef).ToString();

                contents = contents.Replace($"{{{{{register}}}}}", value);
            }
            contents = contents.Replace("{{TypeName}}", cargoDef.TypeName.Replace(" ", "_"));
            File.AppendAllText(outputPath, contents);
        }

        public static void WriteNMLCargoItem(CargoScheme scheme, CargoDefinition cargoDef) {
            var outputDir = $"{Config.NMLOutputDirectory}\\{scheme.SchemeName}";
            var templateDir = $"{Config.NMLTemplateDirectory}";
            var schemeTemplateDir = $"{templateDir}\\{scheme.SchemeName}";
            var templateFile = $"{templateDir}\\cargoItem.nml";
            var outputPath = $"{outputDir}\\cargomod_{scheme.SchemeName}.nml";

            var contents = File.ReadAllText(templateFile);

            contents = contents
                .Replace("{{TypeName}}", cargoDef.TypeName.Replace(" ", "_"))
                .Replace("{{ItemID}}", cargoDef.ItemID.ToString())
                .Replace("{{PriceFactor}}", cargoDef.PriceFactor.ToString())
                .Replace("{{CargoSprite}}", cargoDef.CargoSprite)
                .Replace("{{CargoIcon}}", cargoDef.CargoSprite == "NEW_CARGO_SPRITE" ? $"{cargoDef.CargoIcon}; " : "");

            File.AppendAllText(outputPath, contents);
        }
    }
}
