﻿using Autodesk.Revit.Attributes;
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
    class CopyProjectParamsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document currentDoc = commandData.Application.ActiveUIDocument.Document;
                DocumentSet activeDocs = commandData.Application.Application.Documents;
                Document baseDoc = null;

                if (activeDocs.Size < 2)
                {
                    throw new Exception("Для копирования параметров необходимо открыть файл, содержащий сведения о проекте!");
                }

                foreach (Document doc in activeDocs)
                {
                    if (doc.Title.Contains("cведения") || doc.Title.Contains("Сведения"))
                    {
                        baseDoc = doc;
                        break;
                    }
                }

                if (baseDoc == null)
                {
                    throw new Exception("Файл, содержащий сведения о проекте не открыт!");
                }

                ProjectInfo baseInfo = new FilteredElementCollector(baseDoc)
                    .OfClass(typeof(ProjectInfo))
                    .OfCategory(BuiltInCategory.OST_ProjectInformation)
                    .Cast<ProjectInfo>()
                    .ToList()[0];

                ProjectInfo currentInfo = new FilteredElementCollector(currentDoc)
                    .OfClass(typeof(ProjectInfo))
                    .OfCategory(BuiltInCategory.OST_ProjectInformation)
                    .Cast<ProjectInfo>()
                    .ToList()[0];

                //List<GlobalParameter> baseGlobalParams = GlobalParametersManager
                //    .GetAllGlobalParameters(baseDoc)
                //    .Select(x => baseDoc.GetElement(x) as GlobalParameter)
                //    .Where(x => x.GetDefinition().Name.Contains("Угол поворота Севера"))
                //    .ToList();

                List<Parameter> baseParameters = new List<Parameter>();
                baseParameters.Add(baseInfo.LookupParameter("SHT_Абсолютная отметка"));
                baseParameters.Add(baseInfo.LookupParameter("SHT_Вид строительства"));
                baseParameters.Add(baseInfo.LookupParameter("Дата утверждения проекта"));
                baseParameters.Add(baseInfo.LookupParameter("Статус проекта"));
                baseParameters.Add(baseInfo.LookupParameter("Заказчик"));
                baseParameters.Add(baseInfo.LookupParameter("Адрес проекта"));
                baseParameters.Add(baseInfo.LookupParameter("Наименование проекта"));
                baseParameters.Add(baseInfo.LookupParameter("Номер проекта"));

                int counter = 0;
                int falseCounter = 0;

                using (Transaction t = new Transaction(currentDoc))
                {
                    t.Start("Копирование параметров");

                    Print(string.Format("Копирование параметров из файла: \"{0}\" в файл: \"{1}\" ↑", baseDoc.Title, currentDoc.Title), KPLN_Loader.Preferences.MessageType.Header);

                    foreach (Parameter parameter in baseParameters)
                    {
                        if (copyingParams(parameter, currentInfo))
                        {
                            counter++;
                        }
                        else
                        {
                            falseCounter++;
                        }
                    }

                    //foreach (GlobalParameter parameter in baseGlobalParams)
                    //{
                    //    if (copyingGlobalParams(parameter, currentDoc))
                    //    {
                    //        counter++;
                    //    }
                    //    else
                    //    {
                    //        falseCounter++;
                    //    }
                    //}

                    Print(string.Format("Скопировано параметров: {0}, не скопировано: {1}", counter, falseCounter), KPLN_Loader.Preferences.MessageType.Success);
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

        public bool copyingGlobalParams(GlobalParameter param, Document currentDoc)
        {
            bool check = false;
            if (param == null)
            {
                return check;
            }
            string paramName = param.GetDefinition().Name;
            try
            {
                ParameterValue paramValue = param.GetValue();
                if (paramValue == null)
                {
                    Print("Не скопирован параметр: " + paramName + ", т.к. он пуст.", KPLN_Loader.Preferences.MessageType.Code);
                    return check;
                }
                ElementId targetGlobalParamId = GlobalParametersManager.FindByName(currentDoc, paramName);
                GlobalParameter targetGlobalParam = currentDoc.GetElement(targetGlobalParamId) as GlobalParameter;
                targetGlobalParam.SetValue(paramValue);
                Print(string.Format("Параметру: \"{0}\" присвоено значение: \"{1}\"", paramName, Math.Round((paramValue as DoubleParameterValue).Value * 57.2957795D)), KPLN_Loader.Preferences.MessageType.Code);
                check = true;
            }
            catch (Exception)
            {
                Print(string.Format("Не удалось присвоить значение параметру: \"{0}\". Возможно, в файле: \"{1}\" данный параметр отсутствует.", paramName, currentDoc.Title), KPLN_Loader.Preferences.MessageType.Error);
            }
            return check;
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
