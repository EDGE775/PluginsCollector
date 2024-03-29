﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ParamManager.Forms;
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
                List<ElementId> elemsIds = new List<ElementId>();
                int indexUnjoin = 0;
                foreach (FailureMessage mess in messages)
                {
                    string description = mess.GetDescriptionText();
                    if (description.Contains("Выделенные элементы объединены, но они не пересекаются"))
                    {
                        indexUnjoin++;
                        List<ElementId> failingElems = mess.GetFailingElements().ToList();
                        elemsDictToUnjoin.Add(indexUnjoin, failingElems);
                    }
                    if (description.Contains("Выделенные перекрытия пересекаются"))
                    {
                        List<ElementId> failingElems = mess.GetFailingElements().ToList();
                        elemsIds.AddRange(failingElems);
                    }
                }

                if (elemsIds.Count > 0)
                {
                    Print("Элементы с предупреждением: \"Выделенные перекрытия пересекаются\"", KPLN_Loader.Preferences.MessageType.Header);
                    foreach (ElementId elemId in elemsIds)
                    {
                        Element elem = doc.GetElement(elemId);
                        Print(string.Format("{0} : {1}", elem.Name, elem.Id), KPLN_Loader.Preferences.MessageType.Regular);
                    }
                }

                int max = elemsDictToUnjoin.Keys.Count();
                string format = "{0} из " + max.ToString() + " элементов обработано";
                using (Progress_Single pb = new Progress_Single("Отсоединение элементов", format, max))
                {
                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start("Отсоединение элементов");
                        int counterUnj = 0;
                        int faultCounterUnj = 0;
                        Print("Начинаю отсоединение элементов. Может занять продолжительное время ↑", KPLN_Loader.Preferences.MessageType.Header);
                        foreach (int key in elemsDictToUnjoin.Keys)
                        {
                            pb.Increment();
                            List<ElementId> unjElems = elemsDictToUnjoin[key];
                            Element elem1 = doc.GetElement(unjElems[0]);
                            Element elem2 = doc.GetElement(unjElems[1]);
                            if (JoinGeometryUtils.AreElementsJoined(doc, elem1, elem2))
                            {
                                try
                                {
                                    JoinGeometryUtils.UnjoinGeometry(doc, elem1, elem2);
                                    doc.Regenerate();
                                }
                                catch (Exception e)
                                {
                                    PrintError(e);
                                }

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
