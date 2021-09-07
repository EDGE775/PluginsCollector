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
    class CopyParamValuesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document currentDoc = commandData.Application.ActiveUIDocument.Document;
                DocumentSet activeDocs = commandData.Application.Application.Documents;
                Document baseDoc = null;
                string paramName = "ДИВ_Секция_Целое";

                if (activeDocs.Size < 2)
                {
                    throw new Exception("Для копирования параметров необходимо открыть файл-источник!");
                }

                foreach (Document doc in activeDocs)
                {
                    if (doc.Title.Contains("фцв"))
                    {
                        baseDoc = doc;
                        break;
                    }
                }

                if (baseDoc == null)
                {
                    throw new Exception("Файл-источник не открыт!");
                }

                List<Element> allElems = new List<Element>();

                List<FamilyInstance> annotations = new FilteredElementCollector(baseDoc)
                    .OfClass(typeof(FamilyInstance))
                    .OfCategory(BuiltInCategory.OST_DetailComponents)
                    .Cast<FamilyInstance>()
                    .ToList();
                allElems.AddRange(annotations);

                Dictionary<ElementId, int> idsWithValues = new Dictionary<ElementId, int>();

                int counter = 0;
                int falseCounter = 0;

                foreach (var elem in allElems)
                {
                    try
                    {
                        Parameter parameter = elem.LookupParameter(paramName);
                        int paramValue = parameter.AsInteger();
                        if (parameter.HasValue && paramValue != 0)
                        {
                            idsWithValues.Add(elem.Id, paramValue);
                            counter++;
                        }
                        falseCounter++;
                    }
                    catch (Exception)
                    {
                        falseCounter++;
                    }
                }
                Print(string.Format("При получении значений параметров обработано элементов: {0}, не обработано: {1}", counter, falseCounter), KPLN_Loader.Preferences.MessageType.Success);

                using (Transaction t = new Transaction(currentDoc))
                {
                    t.Start("Копирование параметров");

                    Print(string.Format("Копирование параметров из файла: \"{0}\" в файл: \"{1}\" ↑", baseDoc.Title, currentDoc.Title), KPLN_Loader.Preferences.MessageType.Header);

                    counter = 0;
                    falseCounter = 0;

                    foreach (var id in idsWithValues.Keys)
                    {
                        try
                        {
                            Element element = currentDoc.GetElement(id);
                            Parameter parameter = element.LookupParameter(paramName);
                            parameter.Set(idsWithValues[id]);
                            counter++;
                        }
                        catch (Exception)
                        {
                            falseCounter++;
                        }
                    }

                    Print(string.Format("При заполнении параметров обработано элементов: {0}, не обработано: {1}", counter, falseCounter), KPLN_Loader.Preferences.MessageType.Success);
                    t.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                PrintError(e, "Произошла ошибка во время запуска скрипта!");
                return Result.Failed;
            }
        }

        public bool copyingParams(Parameter param, Element currentInfo)
        {
            bool check = false;
            if (param == null || currentInfo == null)
            {
                return check;
            }
            string paramName = param.Definition.Name;
            if (!param.HasValue)
            {
                Print("Не скопирован параметр: " + paramName + ", т.к. он пуст.", KPLN_Loader.Preferences.MessageType.Code);
                return check;
            }
            Parameter currentParam = currentInfo.LookupParameter(paramName);
            try
            {
                if (param.StorageType == StorageType.String)
                {
                    currentParam.Set(param.AsString());
                    Print(string.Format("Параметру: \"{0}\" присвоено значение: \"{1}\"", paramName, param.AsString()), KPLN_Loader.Preferences.MessageType.Code);
                    check = true;
                }
                else if (param.StorageType == StorageType.Double)
                {
                    currentParam.Set(param.AsDouble());
                    Print(string.Format("Параметру: \"{0}\" присвоено значение: \"{1}\"", paramName, param.AsDouble()), KPLN_Loader.Preferences.MessageType.Code);
                    check = true;
                }
                else if (param.StorageType == StorageType.Integer)
                {
                    currentParam.Set(param.AsInteger());
                    Print(string.Format("Параметру: \"{0}\" присвоено значение: \"{1}\"", paramName, param.AsInteger()), KPLN_Loader.Preferences.MessageType.Code);
                    check = true;
                }
                else
                {
                    Print("Не удалось определить тип параметра: " + paramName, KPLN_Loader.Preferences.MessageType.Error);
                }
            }
            catch (Exception e)
            {
                PrintError(e, "Не удалось присвоить параметр: " + paramName);
            }
            return check;
        }
    }
}
