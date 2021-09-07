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
    class MTSCParamKRCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            char splitLevelChar = ' ';

            List<Element> allElems = new List<Element>();

            List<Wall> walls = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .Cast<Wall>()
                .ToList();
            allElems.AddRange(walls);

            List<Floor> floors = new FilteredElementCollector(doc)
                .OfClass(typeof(Floor))
                .Cast<Floor>()
                .ToList();
            allElems.AddRange(floors);

            List<FamilyInstance> genericModels = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .Cast<FamilyInstance>()
                .Where(i => !i.Symbol.FamilyName.StartsWith("22") && i.Symbol.FamilyName.StartsWith("2"))
                .ToList();
            allElems.AddRange(genericModels);

            List<FamilyInstance> windows = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(i => i.Symbol.FamilyName.StartsWith("23"))
                .ToList();
            allElems.AddRange(windows);

            List<FamilyInstance> columns = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(columns);

            List<FamilyInstance> framings = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralFraming)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(framings);

            List<FamilyInstance> foundations = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(foundations);

            List<FamilyInstance> floorInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Floors)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(floorInstances);

            List<FamilyInstance> wallInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Walls)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(wallInstances);

            List<ParamAction> actions = new List<ParamAction>();
            actions.Add(new FloorNumberOnLevelAction(doc, allElems, "INGD_Номер этажа", 0, splitLevelChar));
            actions.Add(new GroupOfConstrAction(doc, allElems, "INGD_Группа конструкции", "MTSC_config.txt"));
            actions.Add(new MarkMappingAction(doc, allElems, "INGD_Марка"));

            using (Transaction t = new Transaction(doc))
            {
                t.Start("MTSC Параметризация");

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
