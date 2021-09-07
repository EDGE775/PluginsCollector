using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KPLN_Loader.Output.Output;


namespace PluginsCollector.Commands.ExternalCommands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class SetElementIdCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;

                string paramName = "ID элемента";
                View activeView = commandData.Application.ActiveUIDocument.ActiveView;
                List<Element> elems = new FilteredElementCollector(doc, activeView.Id)
                 .WhereElementIsNotElementType()
                 .ToElements()
                 .ToList();
                int count = 0, err = 0;

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Копирование значений Id элементов в параметр: " + paramName);
                    foreach (Element elem in elems)
                    {
                        if (elem is Group) continue;
                        try
                        {
                            double id = (double) elem.Id.IntegerValue;
                            elem.LookupParameter(paramName).Set(id);
                            count++;
                        }
                        catch { err++; }
                    }
                    t.Commit();
                }

                Print("Обработано элементов: " + count + ", не обработано элементов: " + err, KPLN_Loader.Preferences.MessageType.Success);
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                PrintError(e, "Произошла ошибка во время запуска скрипта");
                return Result.Failed;
            }
        }
    }
}
