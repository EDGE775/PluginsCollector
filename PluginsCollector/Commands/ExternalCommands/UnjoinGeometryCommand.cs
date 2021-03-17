using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
    public class UnjoinGeometryCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;
                List<FailureMessage> messages = doc.GetWarnings().ToList();
                Dictionary<int, List<ElementId>> elemsDictToUnjoin = new Dictionary<int, List<ElementId>>();
                int indexUnjoin = 0;
                foreach (FailureMessage mess in messages)
                {
                    String description = mess.GetDescriptionText();
                    if (description.Contains("Выделенные элементы объединены, но они не пересекаются")
                       || description.Contains("Выделенные стены прикреплены к выделенным целевым элементам, но не находят их"))
                    {
                        indexUnjoin++;
                        List<ElementId> failingElems = mess.GetFailingElements().ToList();
                        elemsDictToUnjoin.Add(indexUnjoin, failingElems);
                    }
                }

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Отсоединение элементов");
                    int counterUnj = 0;
                    int faultCounterUnj = 0;
                    Print("Начинаю отсоединение элементов ↑", KPLN_Loader.Preferences.MessageType.Header);
                    foreach (int key in elemsDictToUnjoin.Keys)
                    {
                        List<ElementId> unjElems = elemsDictToUnjoin[key];
                        Element elem1 = doc.GetElement(unjElems[0]);
                        Element elem2 = doc.GetElement(unjElems[1]);
                        if (JoinGeometryUtils.AreElementsJoined(doc, elem1, elem2))
                        {
                            JoinGeometryUtils.UnjoinGeometry(doc, elem1, elem2);
                            doc.Regenerate();
                            if (JoinGeometryUtils.AreElementsJoined(doc, elem1, elem2))
                            {
                                Print(string.Format("Не удалось отсоединить элементы: {0} id:{1} и {2} id:{3}", 
                                    elem1.Name, 
                                    elem1.Id.IntegerValue,
                                    elem2.Name, 
                                    elem2.Id.IntegerValue
                                    ), 
                                    KPLN_Loader.Preferences.MessageType.System_Regular);
                                faultCounterUnj++;
                            }
                            else
                            {
                                counterUnj++;
                            }
                        }
                    }

                    Print(string.Format("Обработано конфликтов: {0}", counterUnj),
                        KPLN_Loader.Preferences.MessageType.System_OK);
                    Print(string.Format("Не удалось обработать конфликтов: {0}", faultCounterUnj),
                        KPLN_Loader.Preferences.MessageType.System_OK);

                    t.Commit();
                }
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
