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
    class GroupOfConstrAction : ParamAction
    {
        private string paramNameGroupConstr;
        private List<Element> elems;
        private Document doc;
        private Dictionary<string, string> marksBase = new Dictionary<string, string>();
        private string fileName;

        public GroupOfConstrAction(Document doc, List<Element> elems, string paramNameGroupConstr, string fileName)
        {
            this.doc = doc;
            this.elems = elems;
            this.paramNameGroupConstr = paramNameGroupConstr;
            this.fileName = fileName;
        }

        public bool execute()
        {
            string txtFile = SettingsUtils.checkParametrizationSettings(fileName);
            if (txtFile == null)
            {
                Print(string.Format("Файл настроек \"{0}\" не найден!", txtFile), KPLN_Loader.Preferences.MessageType.Error);
                return false;
            }

            string[] data = System.IO.File.ReadAllLines(txtFile);
            foreach (string s in data)
            {
                string[] line = s.Split(';');
                marksBase.Add(line[0], line[1]);
            }

            int counter = 0;
            foreach (Element elem in elems)
            {
                Parameter markParam = elem.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                if (markParam == null) continue;
                if (markParam.HasValue)
                {
                    string mark = markParam.AsString();
                    string[] splitmark = mark.Split('-');
                    if (splitmark.Length > 1)
                    {
                        string markPrefix = splitmark[0];
                        if (!marksBase.ContainsKey(markPrefix))
                        {
                            Print("Недопустимый префикс марки " + markPrefix + " у элемента id. Параметр не заполнен!" + elem.Id.IntegerValue.ToString(), KPLN_Loader.Preferences.MessageType.Regular);
                            continue;
                        }
                        string group = marksBase[markPrefix];
                        Parameter groupParam = elem.LookupParameter(paramNameGroupConstr);
                        if (groupParam != null)
                        {
                            groupParam.Set(group);
                        }
                        counter++;
                    }
                }
            }
            Print("Обработано элементов: " + counter, KPLN_Loader.Preferences.MessageType.Success);
            return true;
        }

        public string name()
        {
            return string.Format("Заполнение параметра группа конструкции из файла: {0}", fileName);
        }
    }
}
