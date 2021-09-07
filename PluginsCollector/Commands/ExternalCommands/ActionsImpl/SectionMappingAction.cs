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
    class SectionMappingAction : ParamAction
    {
        private string sectionName;
        private string sectionParam;
        private List<Element> elems;
        private Document doc;

        public SectionMappingAction(Document doc, List<Element> elems, string sectionParam)
        {
            this.doc = doc;
            this.elems = elems;
            this.sectionParam = sectionParam;
        }

        public bool execute()
        {
            int counter = 0;
            sectionName = doc.Title.Split('.')[0].Split('_').Skip(4).Reverse().Skip(3).Reverse().Aggregate((x, y) => x + "_" + y);
            foreach (Element elem in elems)
            {
                Parameter targetSectionParam = elem.LookupParameter(sectionParam);
                if (targetSectionParam != null)
                {
                    targetSectionParam.Set(sectionName);
                    counter++;
                }
            }
            Print("Обработано элементов: " + counter, KPLN_Loader.Preferences.MessageType.Success);
            return true;
        }

        public string name()
        {
            return "Заполнение параметра номера секции";
        }
    }
}
