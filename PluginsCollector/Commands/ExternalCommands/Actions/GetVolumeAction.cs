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
    class GetVolumeAction : ParamAction
    {
        private string volumeParam;
        private List<Element> elems;
        private Document doc;

        public GetVolumeAction(Document doc, List<Element> elems, string volumeParam)
        {
            this.doc = doc;
            this.elems = elems;
            this.volumeParam = volumeParam;
        }

        public bool execute()
        {
            int counter = 0;
            foreach (Element elem in elems)
            {
                List<Solid> solids = GeometryUtils.GetSolidsFromElement(elem);
                if (solids == null)
                {
                    Print("Не удалось получить Solid элемента: " + elem.Name + " с id: " + elem.Id, KPLN_Loader.Preferences.MessageType.Regular);
                    continue;
                }
                double volume = solids.ConvertAll(x => x.Volume).Aggregate((x, y) => x + y);
                Parameter targetVolumeParam = elem.LookupParameter(volumeParam);
                if (targetVolumeParam != null)
                {
                    targetVolumeParam.Set(volume);
                    counter++;
                }
            }
            Print("Обработано элементов: " + counter, KPLN_Loader.Preferences.MessageType.Success);
            return true;
        }

        public string name()
        {
            return "Заполнение параметров объёма";
        }
    }
}
