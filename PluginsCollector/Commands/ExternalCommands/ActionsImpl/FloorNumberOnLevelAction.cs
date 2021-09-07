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
    /// <summary>
    /// Класс для заполнения параметра номера этажа элементов исходя из уровня, на котором они находятся.
    /// </summary>
    class FloorNumberOnLevelAction : ParamAction
    {
        private string floorNumberParamName;
        private List<Element> elems;
        private Document doc;
        private int floorTextPosition;
        private char splitChar;
        /// <summary>
        /// Конструктор класса FloorNumberOnLevelAction
        /// </summary>
        /// <param name="doc">Документ</param>
        /// <param name="elems">Список элементов</param>
        /// <param name="floorNumberParamName">Имя параметра номера этажа</param>
        /// <param name="floorTextPosition">Позиция номера этажа в имени уровня</param>
        /// <param name="splitChar">Символ-разделитель в имени уровня</param>
        public FloorNumberOnLevelAction(Document doc, List<Element> elems, string floorNumberParamName, int floorTextPosition, char splitChar)
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
                try
                {
                    Level baseLevel = LevelUtils.GetLevelOfElement(elem, doc);
                    if (baseLevel != null)
                    {
                        string floorNumber = LevelUtils.GetFloorNumberByLevel(baseLevel, floorTextPosition, splitChar);
                        if (floorNumber == null) continue;
                        Parameter floor = elem.LookupParameter(floorNumberParamName);
                        if (floor == null) continue;
                        floor.Set(floorNumber);
                        counter++;
                    }
                }
                catch (Exception e)
                {
                    PrintError(e, "Не удалось обработать элемент: " + elem.Id.IntegerValue + " " + elem.Name);
                }
            }
            Print("Обработано элементов: " + counter, KPLN_Loader.Preferences.MessageType.Success);
            return true;
        }

        public string name()
        {
            return "Заполнение параметра номер этажа для элементов НА уровне";
        }
    }
}
