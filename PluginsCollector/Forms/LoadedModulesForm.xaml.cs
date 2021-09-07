using KPLN_Loader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static KPLN_Loader.Preferences;
using static KPLN_Loader.Output.Output;
using KPLN_Loader;
using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;
using System.Security.Policy;

namespace PluginsCollector.Forms
{
    /// <summary>
    /// Логика взаимодействия для LoadedModulesForm.xaml
    /// </summary>
    public partial class LoadedModulesForm : Window
    {
        UIControlledApplication application;
        AppDomain domain;
        public LoadedModulesForm(UIControlledApplication application)
        {
            InitializeComponent();
            this.Collection.ItemsSource = loadedModules;
            this.application = application;
        }

        private void Delete_Module_Click(object sender, RoutedEventArgs e)
        {
            // Выгрузка домена приведёт к ожидаемому результату. Ни одна сборка не зависнет.
            UnloadDomain(domain);
            domain = null;
            GC.Collect(2);
            this.Close();
        }

        private void Add_Old_Module_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Preferences.Tools.PrepareLocalDirectory())
                {
                    string modulePath = null;
                    System.Windows.Forms.OpenFileDialog storageDialog = new System.Windows.Forms.OpenFileDialog();
                    storageDialog.Multiselect = false;
                    if (storageDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        modulePath = storageDialog.FileName;
                    }
                    Print(string.Format("Инфо: Загрузка модуля [{0}]", modulePath), MessageType.System_Regular);

                    FileInfo originalfileInfo = new FileInfo(modulePath);
                    DirectoryInfo loadedModule = Preferences.Tools.CopyModuleFromPath(new DirectoryInfo(originalfileInfo.Directory.FullName), "0.0.0.0", originalfileInfo.Name);
                    Print(string.Format("Инфо: loadedModule [{0}]", loadedModule.FullName), MessageType.System_Regular);

                    foreach (FileInfo fileInfo in loadedModule.GetFiles())
                    {
                        if (fileInfo.FullName.Split('.').Last() == "dll")
                        {
                            Print(string.Format("Файл: [{0}], [{1}]", fileInfo.FullName, fileInfo.Name), MessageType.System_Regular);
                            try
                            {
                                Assembly assembly = Assembly.LoadFrom(fileInfo.FullName);
                                Type implemnentationType = assembly.GetType(fileInfo.Name.Split('.').First() + ".Module");
                                try
                                {
                                    implemnentationType.GetMember("Module");
                                }
                                catch (Exception)
                                {

                                }
                                IExternalModule moduleInstance = Activator.CreateInstance(implemnentationType) as IExternalModule;
                                Result loadingResult = moduleInstance.Execute(application, "KPLN");
                                if (loadingResult == Result.Succeeded)
                                {
                                    loadedModules.Add(moduleInstance);
                                    Print(string.Format("Модуль [{0}] успешно активирован!", fileInfo.Name), MessageType.System_OK);
                                }
                            }
                            catch (Exception exp)
                            {
                                PrintError(exp, string.Format("Ошибка при получении модуля «{0}»", fileInfo.Name));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
        }

        private void Add_New_Module_Click(object sender, RoutedEventArgs e)
        {
            string modulePath = null;
            System.Windows.Forms.OpenFileDialog storageDialog = new System.Windows.Forms.OpenFileDialog();
            storageDialog.Multiselect = false;
            if (storageDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                modulePath = storageDialog.FileName;
            }
            Print(string.Format("Инфо: Загрузка модуля [{0}]", modulePath), MessageType.System_Regular);

            FileInfo originalfileInfo = new FileInfo(modulePath);
            DirectoryInfo loadedModule = Preferences.Tools.CopyModuleFromPath(new DirectoryInfo(originalfileInfo.Directory.FullName), "0.0.0.0", originalfileInfo.Name);
            Print(string.Format("Инфо: loadedModule [{0}]", loadedModule.FullName), MessageType.System_Regular);


            // Создаём домен приложения, в котором будут выполняться расширения.
            AppDomain domain = CreateDomain(loadedModule.FullName);
            this.domain = domain;

            Print(string.Format("domain [{0}] [{1}]", domain.BaseDirectory, domain.FriendlyName), MessageType.System_Regular);
            Print(string.Format("999 loadedModule.FullName [{0}]", loadedModule.FullName), MessageType.System_Regular);

            //try
            //{
            // Получаем список экземпляров расширений.
            IEnumerable<IExternalModule> extensions = EnumerateExtensions(domain);
            Print(string.Format("Кол-во extensions [{0}]", extensions.Count()), MessageType.System_Regular);

            foreach (IExternalModule moduleInstance in extensions)
            // Выполняем метод расширения. Выполнение происходит в другом домене.
            {
                Print(string.Format("moduleInstance [{0}]", moduleInstance.ToString()), MessageType.System_Regular);
                Result loadingResult = moduleInstance.Execute(application, "KPLN");
                Print(string.Format("loadingResult [{0}]", loadingResult), MessageType.System_Regular);
                if (loadingResult == Result.Succeeded)
                {
                    loadedModules.Add(moduleInstance);
                    Print(string.Format("Модуль [{0}] успешно активирован!", moduleInstance.ToString()), MessageType.System_OK);
                }

            }

            //}
            //catch (Exception ex)
            //{
            //    PrintError(ex, string.Format("Ошибка при выполнении методов подключаемых плагинов «{0}»", domain.FriendlyName));
            //}
        }

        private static AppDomain CreateDomain(string path)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = path + "//";
            Evidence adevidence = AppDomain.CurrentDomain.Evidence;
            return AppDomain.CreateDomain("Temporary domain", adevidence, setup);
        }

        private static IEnumerable<IExternalModule> EnumerateExtensions(AppDomain domain)
        {
            Print(string.Format("Зашёл в метод EnumerateExtensions"), MessageType.System_OK);

            IEnumerable<string> fileNames = Directory.EnumerateFiles(domain.BaseDirectory, "*.dll");
            Print(string.Format("fileNames [{0}]", fileNames.ToString()), MessageType.System_OK);

            if (fileNames != null)
            {
                foreach (string assemblyFileName in fileNames)
                {
                    Print(string.Format("assemblyFileName [{0}]", assemblyFileName), MessageType.System_OK);

                    foreach (string typeName in GetTypes(assemblyFileName, typeof(IExternalModule), domain))
                    {
                        Print(string.Format("1"), MessageType.System_OK);

                        System.Runtime.Remoting.ObjectHandle handle;
                        Print(string.Format("2"), MessageType.System_OK);

                        try
                        {
                            Print(string.Format("3"), MessageType.System_OK);

                            handle = domain.CreateInstanceFrom(assemblyFileName, typeName);
                        }
                        catch (MissingMethodException e)
                        {
                            PrintError(e, string.Format("Ошибка при создании экземпляра класса «{0}»", typeName));
                            continue;
                        }
                        object obj = handle.Unwrap();
                        IExternalModule extension = (IExternalModule)obj;
                        yield return extension;
                    }
                }
            }
            yield return null;
        }

        private static IEnumerable<string> GetTypes(string assemblyFileName, Type interfaceFilter, AppDomain domain)
        {
            Print("4" + AssemblyName.GetAssemblyName(assemblyFileName).FullName, MessageType.System_OK);
            Print("4" + AssemblyName.GetAssemblyName(assemblyFileName).Name, MessageType.System_OK);
            Print("5" + assemblyFileName, MessageType.System_OK);
            Print("6" + domain.BaseDirectory + " " + domain.FriendlyName, MessageType.System_OK);
            FileInfo fileInfo = new FileInfo(assemblyFileName);

            IExternalModule instance = (IExternalModule)domain.CreateInstanceAndUnwrap(AssemblyName.GetAssemblyName(assemblyFileName).FullName, "Module");
            Print("7 " + instance, MessageType.System_OK);

            Assembly asm = domain.Load(AssemblyName.GetAssemblyName(assemblyFileName));
            Print(string.Format("asm", asm.FullName), MessageType.System_OK);

            Type[] types = asm.GetTypes();
            Print(string.Format("types", types), MessageType.System_OK);

            foreach (Type type in types)
            {
                if (type.GetInterface(interfaceFilter.Name) != null)
                {
                    yield return type.FullName.Split('.').First() + ".Module";
                }
            }
        }

        private static IEnumerable<string> GetTypesOld(string assemblyFileName, Type interfaceFilter, AppDomain domain)
        {
            Print("4" + AssemblyName.GetAssemblyName(assemblyFileName), MessageType.System_OK);
            Print("5" + assemblyFileName, MessageType.System_OK);

            Assembly asm = domain.Load(assemblyFileName);
            Print(string.Format("asm", asm.FullName), MessageType.System_OK);

            Type[] types = asm.GetTypes();
            Print(string.Format("types", types), MessageType.System_OK);

            foreach (Type type in types)
            {
                if (type.GetInterface(interfaceFilter.Name) != null)
                {
                    yield return type.FullName.Split('.').First() + ".Module";
                }
            }
        }

        private static void UnloadDomain(AppDomain domain)
        {
            AppDomain.Unload(domain);
        }
    }
}
