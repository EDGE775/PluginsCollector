using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static KPLN_Loader.Output.Output;

namespace PluginsCollector.Tools
{
    public static class SettingsUtils
    {
        public static string CheckOrCreateSettings()
        {
            string appdataFolder =
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string bimstarterFolder =
                Path.Combine(appdataFolder, "bim-starter");
            if (!Directory.Exists(bimstarterFolder))
            {
                Directory.CreateDirectory(bimstarterFolder);
            }
            string configPath = Path.Combine(bimstarterFolder, "config.ini");
            string serverSettingsPath = "";
            if (File.Exists(configPath))
            {
                serverSettingsPath = File.ReadAllLines(configPath)[0];
            }
            else
            {
                string sourceTxtFile = getLocalConfigFile();
                return sourceTxtFile;
            }

            string ingdConfigFolder = Path.Combine(serverSettingsPath, "IngradParametrisation");
            if (!Directory.Exists(ingdConfigFolder))
            {
                Directory.CreateDirectory(ingdConfigFolder);
            }
            string ingdConfigFile = Path.Combine(ingdConfigFolder, "config.txt");
            if (!File.Exists(ingdConfigFile))
            {
                string sourceTxtFile = getLocalConfigFile();
                try
                {
                    File.Copy(sourceTxtFile, ingdConfigFile);
                }
                catch (Exception ex)
                {
                    throw new Exception("Не удалось скопировать " + sourceTxtFile + " в " + ingdConfigFile + ". " + ex.Message);
                }
            }
            return ingdConfigFile;
        }
        /// <summary>
        ///   
        /// </summary>
        /// <param name="fileName">Имя файла настроек с раширением .ini</param>
        /// <returns></returns>
        public static string checkParametrizationSettings(string fileName)
        {
            string bimstarterFolder = "X:\\BIM\\5_Scripts\\bim-starter\\Config\\Parametrisation\\";
            if (!Directory.Exists(bimstarterFolder))
            {
                Print("Папка настроек по адресу: \"X:\\BIM\\5_Scripts\\bim-starter\\Config\\Parametrisation\" не найдена!", KPLN_Loader.Preferences.MessageType.Error);
                return null;
            }
            string сonfigFile = Path.Combine(bimstarterFolder, fileName);

            if (!File.Exists(сonfigFile))
            {
                return null;
            }
            return сonfigFile;
        }

        private static string getLocalConfigFile()
        {
            string assemblyFolder = Path.GetDirectoryName(Module.assembly);
            string sourceTxtFile = Path.Combine(assemblyFolder, "config.txt");
            if (!System.IO.File.Exists(sourceTxtFile))
            {
                throw new Exception("Не найден файл " + sourceTxtFile);
            }
            return sourceTxtFile;
        }
    }
}
