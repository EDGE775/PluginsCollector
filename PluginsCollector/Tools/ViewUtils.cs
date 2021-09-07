using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KPLN_Loader.Output.Output;

namespace PluginsCollector.Tools
{
    class ViewUtils
    {
        public static OverrideGraphicSettings getGraphicSettings(Document doc, Color color)
        {
            FillPatternElement fillPattern = null;

            List<Element> fillPatterns = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            foreach (var fp in fillPatterns)
            {
                if (fp.Name.ToString().Contains("Сплошная заливка"))
                {
                    fillPattern = (FillPatternElement)fp;
                    break;
                }
            }
            if (fillPattern == null)
            {
                Print("Заливка \"Сплошная заливка\" не найдена", KPLN_Loader.Preferences.MessageType.Error);
                return null;
            }

            OverrideGraphicSettings overrideGraphic = new OverrideGraphicSettings();
#if Revit2018
            overrideGraphic.SetProjectionFillPatternId(fillPattern.Id);
            overrideGraphic.SetProjectionFillColor(color);
#endif
#if Revit2020
            overrideGraphic.SetSurfaceForegroundPatternId(fillPattern.Id);
            overrideGraphic.SetSurfaceForegroundPatternColor(color);
#endif
            overrideGraphic.SetProjectionLineColor(color);

            return overrideGraphic;
        }

        public static OverrideGraphicSettings getStandartGraphicSettings(Document doc)
        {
            OverrideGraphicSettings overrideGraphic = new OverrideGraphicSettings();
#if Revit2018
            overrideGraphic.SetProjectionFillPatternId(ElementId.InvalidElementId);
            overrideGraphic.SetProjectionFillColor(Color.InvalidColorValue);
#endif
#if Revit2020
            overrideGraphic.SetSurfaceForegroundPatternId(ElementId.InvalidElementId);
            overrideGraphic.SetSurfaceForegroundPatternColor(Color.InvalidColorValue);
#endif
            overrideGraphic.SetProjectionLineColor(Color.InvalidColorValue);

            return overrideGraphic;
        }

        public static List<Element> getElemsForOverriding(Document doc, View view, int firstId, int lastId)
        {
            List<Element> elems = new FilteredElementCollector(doc, view.Id)
                 .WhereElementIsNotElementType()
                 .ToElements()
                 .Where(x => x.Id.IntegerValue > firstId && x.Id.IntegerValue <= lastId)
                 .ToList();

            if (elems.Count == 0)
            {
                return new List<Element> { };
            }

            return elems;
        }

        public static int getLastIdNumber(Document doc)
        {
            List<int> lastId = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList()
                .ConvertAll(x => x.Id.IntegerValue);
            lastId.Sort();
            return lastId.Last();
        }
    }
}
