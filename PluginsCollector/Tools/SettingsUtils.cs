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
        public static string CheckOrCreateSettings(string file_name)
        {
            //string appdataFolder =
            //    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //string bimstarterFolder =
            //    Path.Combine(appdataFolder, "bim-starter");
            //if (!Directory.Exists(bimstarterFolder))
            //{
            //    Directory.CreateDirectory(bimstarterFolder);
            //}
            //string configPath = Path.Combine(bimstarterFolder, "config.ini");
            string serverSettingsPath = @"X:\BIM\5_Scripts\bim-starter\Config\Parametrisation";
            if (!Directory.Exists(serverSettingsPath))
            {
                throw new Exception("Директории Parametrisation не существует!");
            }

            string configFile = Path.Combine(serverSettingsPath, file_name);
            if (!File.Exists(configFile))
            {
                string sourceTxtFile = getLocalConfigFile();
                try
                {
                    File.Copy(sourceTxtFile, configFile);
                }
                catch (Exception ex)
                {
                    throw new Exception("Не удалось скопировать " + sourceTxtFile + " в " + configFile + ". " + ex.Message);
                }
            }
            return configFile;
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
