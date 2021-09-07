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
    class OverrideSingleViewAction : ParamAction
    {
        private int firstId;
        private int lastId;
        private Document doc;
        private Color color;

        public OverrideSingleViewAction(Document doc, int firstId, int lastId, Color color)
        {
            this.doc = doc;
            this.firstId = firstId;
            this.lastId = lastId;
            this.color = color;
        }

        public bool execute()
        {
            View activeView = doc.ActiveView;

            OverrideGraphicSettings overrideGraphic = ViewUtils.getGraphicSettings(doc, color);
            List<Element> elems = ViewUtils.getElemsForOverriding(doc, activeView, firstId, lastId);

            if (elems.Count == 0)
            {
                Print("Не найдены элементы для раскрашивания!", KPLN_Loader.Preferences.MessageType.Error);
                return false;
            }

            int count = 0, err = 0;

            foreach (Element elem in elems)
            {
                if (elem is Group) continue;
                try
                {
                    activeView.SetElementOverrides(elem.Id, overrideGraphic);
                    count++;
                }
                catch
                {
                    err++;
                }
            }

            Print(string.Format( "Обработано элементов: {0}, не обработано элементов: {1}", count, err), KPLN_Loader.Preferences.MessageType.Success);
            return true;
        }

        public string name()
        {
            return "Колоризация текущего вида по заданному диапазону ID";
        }
    }
}
