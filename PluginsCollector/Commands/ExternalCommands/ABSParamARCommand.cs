using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB; //для работы с элементами модели Revit
using Autodesk.Revit.UI; //для работы с элементами интерфейса
using PluginsCollector.Tools;
using static KPLN_Loader.Output.Output;
using Autodesk.Revit.DB.Architecture;

namespace PluginsCollector.Commands.ExternalCommands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ABSParamARCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            char splitLevelChar = '_';

            List<Element> allElems = new List<Element>();

            List<Wall> walls = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .Cast<Wall>()
                .Where(x => !x.Name.StartsWith("00_"))
                .ToList();
            allElems.AddRange(walls);

            List<Floor> floors = new FilteredElementCollector(doc)
                .OfClass(typeof(Floor))
                .Cast<Floor>()
                .Where(x => !x.Name.StartsWith("00_"))
                .ToList();
            allElems.AddRange(floors);

            List<FamilyInstance> genericModels = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .Cast<FamilyInstance>()
                .Where(i => i.Symbol.FamilyName.StartsWith("1"))
                .ToList();
            allElems.AddRange(genericModels);

            List<FamilyInstance> windows = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Windows)
                .Cast<FamilyInstance>()
                .Where(i => !i.Symbol.FamilyName.StartsWith("23"))
                .ToList();
            allElems.AddRange(windows);

            List<FamilyInstance> doors = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Doors)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(doors);

            List<Railing> stairsRailing = new FilteredElementCollector(doc)
                .OfClass(typeof(Railing))
                .Cast<Railing>()
                .ToList();
            allElems.AddRange(stairsRailing);

            List<SpatialElement> rooms = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<SpatialElement>()
                .ToList();
            allElems.AddRange(rooms);

            List<RoofBase> roofs = new FilteredElementCollector(doc)
                .OfClass(typeof(RoofBase))
                .Cast<RoofBase>()
                .ToList();
            allElems.AddRange(roofs);

            List<FamilyInstance> curtainWallPanels = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_CurtainWallPanels)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(curtainWallPanels);

            List<FamilyInstance> curtainWallMullions = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_CurtainWallMullions)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(curtainWallMullions);

            List<FamilyInstance> plumbingFixtures = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_PlumbingFixtures)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(plumbingFixtures);

            List<FamilyInstance> mass = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Mass)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(mass);

            List<FamilyInstance> mechanicalEquipment = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(mechanicalEquipment);

            List<FamilyInstance> specialityEquipment = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_SpecialityEquipment)
                .Cast<FamilyInstance>()
                .ToList();
            allElems.AddRange(specialityEquipment);

            using (Transaction t = new Transaction(doc))
            {
                t.Start("ABS Параметризация");

                Print("Параметризация элементов ↑", KPLN_Loader.Preferences.MessageType.Header);

                List<ParamAction> actions = new List<ParamAction>();
                actions.Add(new FloorNumberOnLevelAction(doc, allElems, "ABS_Этаж", 1, splitLevelChar));
                actions.Add(new SectionMappingAction(doc, allElems, "ABS_Участок"));
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
