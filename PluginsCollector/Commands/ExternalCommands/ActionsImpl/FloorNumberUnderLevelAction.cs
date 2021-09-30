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
    class FloorNumberUnderLevelAction : ParamAction
    {
        private string floorNumberParamName;
        private List<Element> elems;
        private Document doc;
        private int floorTextPosition;
        private char splitChar;

        public FloorNumberUnderLevelAction(Document doc, List<Element> elems, string floorNumberParamName, int floorTextPosition, char splitChar)
        {
            this.doc = doc;
            this.elems = elems;
            this.floorNumberParamName = floorNumberParamName;
            this.floorTextPosition = floorTextPosition;
            this.splitChar = splitChar;
        }

        public bool execute()
        {
            int counter = 0;
            foreach (Element elem in elems)
            {
                //заполняю номер этажа для элементов, находящихся НАД уровне
                Level baseLevel = LevelUtils.GetLevelOfElement(elem, doc);
                if (baseLevel != null)
                {
                    string floorNumber = LevelUtils.GetFloorNumberByUnderLevel(baseLevel, floorTextPosition, doc, splitChar);
                    if (floorNumber == null)
                    {
                        Print(string.Format("Не найден уровень с уровнем выше, равным: {0} при обработке элемента: {1} c id: {2}. Для уровней необходимо заполнить параметр: На уровень выше", baseLevel.Name, elem.Name, elem.Id.IntegerValue), KPLN_Loader.Preferences.MessageType.Regular);
                        continue;
                    }
                    Parameter floor = elem.LookupParameter(floorNumberParamName);
                    if (floor == null) continue;
                    floor.Set(floorNumber);
                    counter++;
                }
            }
            Print("Обработано элементов: " + counter, KPLN_Loader.Preferences.MessageType.Success);
            return true;
        }

        public string name()
        {
            return "Заполнение параметра номер этажа для элементов НАД уровнем";
        }
    }
}
