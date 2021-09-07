using Autodesk.Revit.UI;
using KPLN_Loader.Common;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using static PluginsCollector.ModuleData;
using static KPLN_Loader.Output.Output;
using PluginsCollector.Tools;
using System.Linq;
using System.Collections.Generic;

namespace PluginsCollector
{
    public class Module : IExternalModule
    {
        public static string assembly = "";
        public static UIControlledApplication uiControlledApplication;
        public Result Close()
        {
            return Result.Succeeded;
        }
        public Result Execute(UIControlledApplication application, string tabName)
        {
            uiControlledApplication = application;

            #region Get revit main window
#if Revit2020
            MainWindowHandle = application.MainWindowHandle;
            HwndSource hwndSource = HwndSource.FromHwnd(MainWindowHandle);
            RevitWindow = hwndSource.RootVisual as Window;
#endif
#if Revit2018
            try
            {
                MainWindowHandle = WindowHandleSearch.MainWindowHandle.Handle;
            }
            catch (Exception e)
            {
                PrintError(e);
            }
#endif
            #endregion
            try { application.CreateRibbonTab(tabName); } catch { }

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "PluginsCollector");
            PulldownButtonData pullDownData = new PulldownButtonData("Плагины", "Плагины");
            PulldownButton pullDown = panel.AddItem(pullDownData) as PulldownButton;
            pullDown.LargeImage = new BitmapImage(new Uri(new Source.Source(Common.Collections.Icon.PluginsCollector).Value));
            assembly = Assembly.GetExecutingAssembly().Location.Split(new string[] { "\\" }, StringSplitOptions.None).Last().Split('.').First();

            // Блок для BIM
            if (KPLN_Loader.Preferences.User.Department.Id == 4)
            {
                AddPushButtonData(
                     "Раскрасить изменения",
                     "Раскрасить\nизменения",
                     "Показывает историю изменений модели с заданного ID с помощью переопределения графики элементов",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.OverrideGraphicsByIdOpenForm"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.OverrideGraphicsByIdOpenForm));
                AddPushButtonData(
                     "Копировать значения",
                     "Копировать\nзначения",
                     "Копирует значения параметров",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.CopyParamValuesCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.CopyParamValuesCommand));
                AddPushButtonData(
                     "Добавить модуль",
                     "Добавить\nмодуль",
                     "Добавляет модуль",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.AddLoaderCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.AddLoaderCommand));
            }

            // Блок для КР и BIM
            if (KPLN_Loader.Preferences.User.Department.Id == 2 || KPLN_Loader.Preferences.User.Department.Id == 4)
            {
                AddPushButtonData(
                     "Починить отверстия КР",
                     "Починить\nотверстия КР",
                     "Производит починку отверстий без вырезания арматуры, которые начали прорезать армирование по площади",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.FixHolesCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.FixHoles));
                AddPushButtonData(
                     "Заполнить параметры INGD КР",
                     "Заполнить\nпараметры INGD КР",
                     "Производит заполнение параметров по стандарту Инграда",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.INGDParamCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.INGDParamCommand));
                AddPushButtonData(
                     "Заполнить параметры ABS КР",
                     "Заполнить\nпараметры ABS КР",
                     "Производит заполнение параметров по стандарту Абсолют",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.ABSParamKRCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.ABSParamKRCommand));
                AddPushButtonData(
                     "Заполнить параметры MTSC КР",
                     "Заполнить\nпараметры MTSC КР",
                     "Производит заполнение параметров по стандарту Ingrad",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.MTSCParamKRCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.MTSCParamKRCommand));
                AddPushButtonData(
                     "Заполнить параметры ДСТЖ КР",
                     "Заполнить\nпараметры ДСТЖ КР",
                     "Производит заполнение параметров по стандарту KPLN",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.DSTGParamKRCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.DSTGParamKRCommand));
                AddPushButtonData(
                     "Заполнить параметры LEVEL КР",
                     "Заполнить\nпараметры LEVEL КР",
                     "Производит заполнение параметров по стандарту LEVEL",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.LEVELParamKRCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.LEVELParamKRCommand));
                AddPushButtonData(
                     "Починить картинки арматуры ДИВ",
                     "Починить картинки\nарматуры ДИВ",
                     "Производит ремонт картинок гнутых стержней для ведомости деталей",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.DIVFixRebarImagesCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.DIVFixRebarImagesCommand));
            }

            // Блок для АР и BIM
            if (KPLN_Loader.Preferences.User.Department.Id == 1 || KPLN_Loader.Preferences.User.Department.Id == 4)
            {
                    AddPushButtonData(
                     "Заполнить параметры ABS АР",
                     "Заполнить\nпараметры ABS АР",
                     "Производит заполнение параметров по стандарту Абсолют",
                     string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.ABSParamARCommand"),
                     pullDown,
                     new Source.Source(Common.Collections.Icon.ABSParamARCommand));
            }

            // Общий блок
            AddPushButtonData(
                    "Отсоединение элементов",
                    "Отсоединение\nэлементов",
                    "Отсоединяет некорректно объединённе элементы, которые не пересекаются",
                    string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.UnjoinGeometryCommand"),
                    pullDown,
                    new Source.Source(Common.Collections.Icon.UnjoinGeometry));

            // Ищет RibbonPanel и именем "Параметры" или создаёт его
            string panelName = "Параметры";
            RibbonPanel newPanel = null;
            List<RibbonPanel> tryPanels = application.GetRibbonPanels(tabName).Where(i => i.Name == panelName).ToList();
            if (tryPanels.Count == 0)
            {
                newPanel = application.CreateRibbonPanel(tabName, panelName);
            }
            else
            {
                newPanel = tryPanels.First();
            }

            AddPushButtonDataForRibbonPanel(
                 "Копирование параметров проекта",
                 "Параметры\nпроекта",
                 "Производит копирование параметров проекта из файла, содержащего сведения о проекте.\n" +
                 "Для копирования параметров необходимо открыть исходный файл или подгрузить его как связь.",
                 string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.CopyProjectParamsCommand"),
                 new Source.Source(Common.Collections.Icon.CopyProjectParamsCommand),
                 newPanel);

            return Result.Succeeded;
        }
        private void AddPushButtonData(string name, string text, string description, string className, RibbonPanel panel, Source.Source imageSource, bool avclass, string url = null)
        {
            PushButtonData data = new PushButtonData(name, text, Assembly.GetExecutingAssembly().Location, className);
            PushButton button = panel.AddItem(data) as PushButton;
            button.ToolTip = description;
            if (avclass)
            {
                button.AvailabilityClassName = "ExtensibleOpeningManager.Availability.StaticAvailable";
            }
            button.LongDescription = string.Format("Версия: {0}\nСборка: {1}-{2}", ModuleData.Version, ModuleData.Build, ModuleData.Date);
            button.ItemText = text;
            if (url == null)
            {
                button.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, ModuleData.ManualPage));
            }
            else
            {
                button.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, url));
            }
            button.LargeImage = new BitmapImage(new Uri(imageSource.Value));
        }
        private void AddPushButtonData(string name, string text, string description, string className, PulldownButton pullDown, Source.Source imageSource)
        {
            PushButtonData data = new PushButtonData(name, text, Assembly.GetExecutingAssembly().Location, className);
            PushButton button = pullDown.AddPushButton(data) as PushButton;
            button.ToolTip = description;
            button.LongDescription = string.Format("Версия: {0}\nСборка: {1}-{2}", ModuleData.Version, ModuleData.Build, ModuleData.Date);
            button.ItemText = text;
            button.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, ModuleData.ManualPage));
            button.LargeImage = new BitmapImage(new Uri(imageSource.Value));
        }
        private void AddPushButtonDataForRibbonPanel(string name, string text, string description, string className, Source.Source imageSource, RibbonPanel panel)
        {
            PushButtonData data = new PushButtonData(name, text, Assembly.GetExecutingAssembly().Location, className);
            PushButton button = panel.AddItem(data) as PushButton;
            button.ToolTip = description;
            button.LongDescription = string.Format("Версия: {0}\nСборка: {1}-{2}", ModuleData.Version, ModuleData.Build, ModuleData.Date);
            button.ItemText = text;
            button.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, ModuleData.ManualPage));
            button.LargeImage = new BitmapImage(new Uri(imageSource.Value));
        }
    }
}
