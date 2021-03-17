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

namespace PluginsCollector
{
    public class Module : IExternalModule
    {
        public Result Close()
        {
            return Result.Succeeded;
        }
        public Result Execute(UIControlledApplication application, string tabName)
        {
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
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Plugins Collector");
            PulldownButtonData pullDownData = new PulldownButtonData("Плагины", "Плагины");
            PulldownButton pullDown = panel.AddItem(pullDownData) as PulldownButton;
            pullDown.LargeImage = new BitmapImage(new Uri(new Source.Source(Common.Collections.Icon.PluginsCollector).Value));
            string assembly = Assembly.GetExecutingAssembly().Location.Split(new string[] { "\\" }, StringSplitOptions.None).Last().Split('.').First();
            AddPushButtonData(
                "Отсоединение элементов",
                "Отсоединение\nэлементов",
                "Отсоединяет некорректно объединённе элементы, которые не пересекаются",
                string.Format("{0}.{1}", assembly, "Commands.ExternalCommands.UnjoinGeometryCommand"), 
                pullDown, 
                new Source.Source(Common.Collections.Icon.UnjoinGeometry));
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
    }
}
