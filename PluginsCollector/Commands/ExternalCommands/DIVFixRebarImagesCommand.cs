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
    class DIVFixRebarImagesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            if (sel.GetElementIds().Count == 0)
            {
                Print(string.Format("Не выбраны элементы! Необходимо выбрать хотя бы один элемент в ведомости деталей."), KPLN_Loader.Preferences.MessageType.Error);
                return Result.Failed;
            }

            List<Family> families = sel.GetElementIds()
                        .Select(elem => doc.GetElement(elem))
                        .Cast<FamilyInstance>()
                        .Select(x => x.Symbol)
                         .Where(fs => fs.LookupParameter("Арм.ЭскизФормы") != null && fs.LookupParameter("Арм.ЭскизФормы").AsValueString().ToLower().Contains("нет"))
                        .Select(x => x.Family)
                        .ToList();
            List<Family> fams = new List<Family>();

            foreach (Family element in families)
            {
                if (fams.Select(x => x.Id.IntegerValue).Contains(element.Id.IntegerValue))
                {
                    continue;
                }
                fams.Add(element);
            }

            int familyCounter = 0;
            int typeCounter = 0;
            foreach (Family fam in fams)
            {
                if (fam == null || !fam.IsValidObject)
                {
                    continue;
                }
                string familyName = fam.Name;
                Document famDoc = doc.EditFamily(fam);
                FamilyManager familyManager = famDoc.FamilyManager;
                FamilyTypeSet familyTypeSet = familyManager.Types;
                List<Element> imagesInFamily = new FilteredElementCollector(famDoc)
                    .OfClass(typeof(ImageType))
                    .OfCategory(BuiltInCategory.OST_RasterImages)
                    .Where(x => x.IsValidObject)
                    .ToList();

                if (imagesInFamily.Count == 0)
                {
                    Print(string.Format("В семействе {0} отсутствуют изображения арматуры!", familyName), KPLN_Loader.Preferences.MessageType.Error);
                    continue;
                }

                ElementId imageOfRebar = imagesInFamily.First().Id;
                FamilyParameter famParam = familyManager.get_Parameter("Арм.ЭскизФормы");
                if (famParam != null && famParam.StorageType == StorageType.ElementId)
                {
                    using (Transaction t = new Transaction(famDoc))
                    {
                        t.Start("Чиню картинки в семействе: " + familyName);
                        try
                        {
                            FamilyType standartType = familyManager.CurrentType;
                            foreach (FamilyType type in familyTypeSet)
                            {
                                familyManager.CurrentType = type;
                                familyManager.Set(famParam, imageOfRebar);
                                typeCounter++;
                            }
                            familyManager.CurrentType = standartType;
                        }
                        catch (Exception e)
                        {
                            Print(string.Format("В семействе {0} не удалось назначить картинку в параметр Арм.ЭскизФормы!", familyName), KPLN_Loader.Preferences.MessageType.Error);
                            PrintError(e);
                            continue;
                        }
                        t.Commit();
                    }
                    familyCounter++;
                }
                else
                {
                    Print(string.Format("В семействе {0} отсутствует параметр Арм.ЭскизФормы!", familyName), KPLN_Loader.Preferences.MessageType.Error);
                    continue;
                }
                FamilyLoadWithOverwriteParameters loadOption = new FamilyLoadWithOverwriteParameters();
                famDoc.LoadFamily(doc, loadOption);
                if (famDoc.Close(false))
                {
                    Print(string.Format("Семейство {0} успешно загружено в проект и закрыто!", familyName), KPLN_Loader.Preferences.MessageType.Success);
                }
                else
                {
                    Print(string.Format("Семейство {0} загружено в проект, но во время закрытыя произошла ошибка!", familyName), KPLN_Loader.Preferences.MessageType.Warning);
                }
            }
            Print(string.Format("Обработано семейств: {0}, в т.ч. типоразмеров семейств: {1}", familyCounter, typeCounter), KPLN_Loader.Preferences.MessageType.Success);
            return Result.Succeeded;
        }
    }

    public class FamilyLoadWithOverwriteParameters : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            if (!familyInUse)
            {
                TaskDialog.Show("LoadOptions", "Семейство не использовалось, загружено.");
                overwriteParameterValues = true;
                return true;
            }
            else
            {
                TaskDialog.Show("LoadOptions", "Семейство использовалось, было загружено с перезаписью значений параметров.");
                overwriteParameterValues = true;
                return true;
            }
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            if (!familyInUse)
            {
                TaskDialog.Show("LoadOptions", "Общее семейство не использовалось, загружено.");
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
            else
            {
                TaskDialog.Show("LoadOptions", " Общее семейство использовалось, было загружено с перезаписью значений параметров.");
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }
    }
}
