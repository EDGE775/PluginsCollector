using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB; //для работы с элементами модели Revit
using Autodesk.Revit.UI; //для работы с элементами интерфейса
using PluginsCollector.Tools;
using static KPLN_Loader.Output.Output;

namespace PluginsCollector.Commands.ExternalCommands
{
    class TypeOfWallAction : ParamAction
    {
        private string typeOfWallParam;
        private List<Element> elems;
        private Document doc;

        public TypeOfWallAction(Document doc, List<Element> elems, string typeOfWallParam)
        {
            this.doc = doc;
            this.elems = elems;
            this.typeOfWallParam = typeOfWallParam;
        }

        public bool execute()
        {
            int counter = 0;
            foreach (Element elem in elems)
            {
                Wall wall = elem as Wall;
                if (wall == null) continue;
                Parameter targetParam = wall.LookupParameter(typeOfWallParam);
                if (targetParam != null)
                {
                    double length = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble() * 304.8;
                    double width = wall.Width * 304.8;
                    List<Solid> solids = GeometryUtils.GetSolidsFromElement(wall);
                    XYZ[] maxminZ = GeometryUtils.GetMaxMinHeightPoints(solids);
                    double heigth = (maxminZ[0].Z - maxminZ[1].Z) * 304.8;

                    int result = (length / width < 4 || heigth / length > 4) && wall.Name.ToLower().Contains("пилон") ? 1 : 0;
                    try
                    {
                        targetParam.Set((double)result);
                        counter++;
                    }
                    catch (Exception)
                    {
                        Print(string.Format("Не удалось присвоить значение параметру: {0} у элемента: {1} с Id: {2}", typeOfWallParam, wall.Name, wall.Id.IntegerValue), KPLN_Loader.Preferences.MessageType.Warning);
                    }
                }
            }
            Print("Обработано элементов: " + counter, KPLN_Loader.Preferences.MessageType.Success);
            return true;
        }

        public string name()
        {
            return "Заполнение параметра типа стены (1 или 0)";
        }
    }
}
