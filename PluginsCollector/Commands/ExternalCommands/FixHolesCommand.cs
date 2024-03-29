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
    class FixHolesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;
                Selection sel = commandData.Application.ActiveUIDocument.Selection;

                if (sel.GetElementIds().Count != 1) throw new Exception("Выберите одну стену!");

                Wall wall = doc.GetElement(sel.GetElementIds().First()) as Wall;
                if (wall == null) throw new Exception("Выберите стену!");

                List<FamilyInstance> holesInWall = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_GenericModel)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .Where(i => i.Symbol.FamilyName.StartsWith("231"))
                    .Where(i => JoinGeometryUtils.AreElementsJoined(doc, wall, i))
                    .ToList();

                List<FamilyInstance> doorsInWall = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .Where(i => i.Symbol.FamilyName.StartsWith("231"))
                    .Where(i => i.Host.Id == wall.Id)
                    .ToList();

                int counter = 0;

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Чиню отверстия");

                    //Отсоединяю отверстия
                    foreach (FamilyInstance hole in holesInWall)
                    {
                        JoinGeometryUtils.UnjoinGeometry(doc, wall, hole);
                        counter++;
                    }

                    doc.Regenerate();

                    if (doorsInWall.Count > 0) //есть дверь или проем в стене, меняю её размеры
                    {
                        FamilyInstance door = doorsInWall.First();
                        //меняю размер двери
                        Parameter widthParam = door.LookupParameter("Рзм.Ширина");
                        double width = widthParam.AsDouble();
                        double width2 = width + 0.01;
                        widthParam.Set(width2);

                        doc.Regenerate();
                        widthParam.Set(width);
                    }
                    else //нет двери, значит меняю высоту стены
                    {
                        Parameter offsetParam = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                        double baseOffset = offsetParam.AsDouble();
                        double baseOffset2 = baseOffset - 0.01;

                        offsetParam.Set(baseOffset2);
                        doc.Regenerate();
                        offsetParam.Set(baseOffset);
                    }
                    doc.Regenerate();

                    //присоединяю отверстия обратно
                    foreach (FamilyInstance hole in holesInWall)
                    {
                        JoinGeometryUtils.JoinGeometry(doc, hole, wall);
                    }

                    TaskDialog.Show("Результат работы", "Отработано отверстий: " + counter);
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
