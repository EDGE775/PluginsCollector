using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KPLN_Loader.Output.Output;

namespace PluginsCollector.Commands.ExternalCommands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class DSTGParamKRCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            char splitLevelChar = '_';

            List<Element> elemsOnLevel = new List<Element>();
            List<Element> elemsUnderLevel = new List<Element>();
            List<Element> allElems = new List<Element>();

            List<Wall> wallsOnLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .Cast<Wall>()
                .Where(x => x.Name.StartsWith("00_"))
                .Where(x => !x.Name.ToLower().Contains("перепад") || !x.Name.ToLower().Contains("перекрытие"))
                .ToList();
            elemsOnLevel.AddRange(wallsOnLevel);

            List<Wall> wallsUnderLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .Cast<Wall>()
                .Where(x => x.Name.StartsWith("00_"))
                .Where(x => x.Name.ToLower().Contains("перепад") || x.Name.ToLower().Contains("перекрытие"))
                .ToList();
            elemsUnderLevel.AddRange(wallsUnderLevel);

            List<Floor> floorsUnderLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Floor))
                .Cast<Floor>()
                .Where(x => x.Name.StartsWith("00_"))
                .Where(x => !x.Name.ToLower().Contains("площадка") && !x.Name.ToLower().Contains("фундамент") && !x.Name.ToLower().Contains("пандус"))
                .ToList();
            elemsUnderLevel.AddRange(floorsUnderLevel);

            List<Floor> floorsOnLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Floor))
                .Cast<Floor>()
                .Where(x => x.Name.StartsWith("00_"))
                .Where(x => x.Name.ToLower().Contains("площадка") || x.Name.ToLower().Contains("фундамент") || x.Name.ToLower().Contains("пандус"))
                .ToList();
            elemsOnLevel.AddRange(floorsOnLevel);

            List<FamilyInstance> genericModels = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .Cast<FamilyInstance>()
                .Where(i => !i.Symbol.FamilyName.StartsWith("22") && i.Symbol.FamilyName.StartsWith("2"))
                .ToList();
            elemsOnLevel.AddRange(genericModels);

            List<FamilyInstance> windows = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(i => i.Symbol.FamilyName.StartsWith("23"))
                .ToList();
            elemsOnLevel.AddRange(windows);

            List<StairsRun> stairsRun = new FilteredElementCollector(doc)
                .OfClass(typeof(StairsRun))
                .Cast<StairsRun>()
                .ToList();
            elemsOnLevel.AddRange(stairsRun);

            List<StairsLanding> stairsLanding = new FilteredElementCollector(doc)
                .OfClass(typeof(StairsLanding))
                .Cast<StairsLanding>()
                .ToList();
            elemsOnLevel.AddRange(stairsLanding);

            List<FamilyInstance> columns = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .Cast<FamilyInstance>()
                .ToList();
            elemsOnLevel.AddRange(columns);

            List<FamilyInstance> framings = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralFraming)
                .Cast<FamilyInstance>()
                .ToList();
            elemsUnderLevel.AddRange(framings);

            List<FamilyInstance> foundations = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .Cast<FamilyInstance>()
                .ToList();
            elemsOnLevel.AddRange(foundations);

            List<FamilyInstance> floorInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Floors)
                .Cast<FamilyInstance>()
                .ToList();
            elemsUnderLevel.AddRange(floorInstances);

            List<FamilyInstance> wallInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Walls)
                .Cast<FamilyInstance>()
                .ToList();
            elemsOnLevel.AddRange(wallInstances);

            allElems.AddRange(elemsOnLevel);
            allElems.AddRange(elemsUnderLevel);

            List<ParamAction> actions = new List<ParamAction>();
            actions.Add(new FloorNumberOnLevelAction(doc, elemsOnLevel, "КП_О_Этаж", 1, splitLevelChar));
            actions.Add(new FloorNumberUnderLevelAction(doc, elemsUnderLevel, "КП_О_Этаж", 1, splitLevelChar));

            using (Transaction t = new Transaction(doc))
            {
                t.Start("ДСТЖ Параметризация");

                Print("Параметризация элементов ↑", KPLN_Loader.Preferences.MessageType.Header);

                foreach (ParamAction action in actions)
                {
                    Print(action.name(), KPLN_Loader.Preferences.MessageType.Regular);
                    if (action.execute())
                    {
                        Print("Завершено успешно!", KPLN_Loader.Preferences.MessageType.Success);
                    }
                    else
                    {
                        Print("Завершено с ошибками!", KPLN_Loader.Preferences.MessageType.Error);
                    }
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }
}
