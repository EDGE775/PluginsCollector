using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using static KPLN_Loader.Output.Output;
using Autodesk.Revit.DB.Architecture;

namespace PluginsCollector.Tools
{
    public static class LevelUtils
    {
        public static string GetFloorNumberByLevel(Level lev, int floorTextPosition, char splitChar)
        {
            string levname = lev.Name;
            string[] splitname = levname.Split(splitChar);
            if (splitname.Length < 2)
            {
                Print("Некорректное имя уровня: " + levname, KPLN_Loader.Preferences.MessageType.Regular);
                return null;
            }
            string floorNumber = splitname[floorTextPosition];
            return floorNumber;
        }
        public static string GetFloorNumberByUnderLevel(Level lev, int floorTextPosition, Document doc, char splitChar)
        {
            Level underLevel = null;

            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .ToList();

            foreach (Level level in levels)
            {
                if (level.get_Parameter(BuiltInParameter.LEVEL_UP_TO_LEVEL).AsValueString().Equals(lev.Name))
                {
                    underLevel = level;
                    break;
                }
            }

            if (underLevel == null) return null;

            string levname = underLevel.Name;
            string[] splitname = levname.Split(splitChar);
            if (splitname.Length < 2)
            {
                Print("Некорректное имя уровня: " + levname, KPLN_Loader.Preferences.MessageType.Regular);
                return null;
            }
            string floorNumber = splitname[floorTextPosition];
            return floorNumber;
        }
        public static Level GetLevelOfElement(Element elem, Document doc)
        {
            ElementId levId = elem.LevelId;
            if (levId == ElementId.InvalidElementId)
            {
                Parameter levParam = elem.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM);
                if (levParam != null)
                    levId = levParam.AsElementId();
            }

            if (levId == ElementId.InvalidElementId)
            {
                Parameter levParam = elem.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
                if (levParam != null)
                    levId = levParam.AsElementId();
            }

            if (levId == ElementId.InvalidElementId)
            {
                Parameter levParam = elem.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
                if (levParam != null)
                    levId = levParam.AsElementId();
            }

            if (levId == ElementId.InvalidElementId && elem is StairsRun)
            {
                StairsRun run = elem as StairsRun;
                if (run != null)
                { 
                    Stairs stair = run.GetStairs();
                    Parameter levParam = stair.get_Parameter(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM);
                    if (levParam != null)
                        levId = levParam.AsElementId();
                }
            }

            if (levId == ElementId.InvalidElementId)
            {
                List<Solid> solids = GeometryUtils.GetSolidsFromElement(elem);
                if (solids.Count == 0) return null;
                XYZ[] maxmin = GeometryUtils.GetMaxMinHeightPoints(solids);
                XYZ minPoint = maxmin[1];
                levId = GetNearestLevel(minPoint, doc);
            }

            if (levId == ElementId.InvalidElementId)
                throw new Exception("Не удалось получить уровень у элемента " + elem.Id.IntegerValue.ToString());

            Level lev = doc.GetElement(levId) as Level;
            return lev;
        }

        public static ElementId GetNearestLevel(XYZ point, Document doc)
        {
            BasePoint projectBasePoint = new FilteredElementCollector(doc)
                .OfClass(typeof(BasePoint))
                .WhereElementIsNotElementType()
                .Cast<BasePoint>()
                .Where(i => i.IsShared == false)
                .First();
            double projectPointElevation = projectBasePoint.get_BoundingBox(null).Min.Z;

            double pointZ = point.Z;
            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .ToList();

            Level finalLevel = null;

            foreach (Level lev in levels)
            {
                if (finalLevel == null)
                {
                    finalLevel = lev;
                    continue;
                }
                if (lev.Elevation < finalLevel.Elevation)
                {
                    finalLevel = lev;
                    continue;
                }
            }

            double offset = 10000;
            foreach (Level lev in levels)
            {
                double levHeigth = lev.Elevation + projectPointElevation;
                double testElev = pointZ - levHeigth;
                if (testElev < 0) continue;

                if (testElev < offset)
                {
                    finalLevel = lev;
                    offset = testElev;
                }
            }

            return finalLevel.Id;
        }
    }
}
