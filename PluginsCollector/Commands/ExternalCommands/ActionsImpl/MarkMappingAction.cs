﻿using System;
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
    class MarkMappingAction : ParamAction
    {
        private string markParamName;
        private List<Element> elems;
        private Document doc;

        public MarkMappingAction(Document doc, List<Element> elems, string markParamName)
        {
            this.doc = doc;
            this.elems = elems;
            this.markParamName = markParamName;
        }

        public bool execute()
        {
            foreach (Element elem in elems)
            {
                Parameter markParam = elem.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                if (markParam == null) continue;
                if (markParam.HasValue)
                {
                    string mark = markParam.AsString();

                    Parameter targetMarkParam = elem.LookupParameter(markParamName);

                    try
                    {
                        targetMarkParam.Set(mark);
                    }
                    catch (Exception)
                    {
                        Print(string.Format("Не удалось скопировать Марку в параметр {0} у элемента: {1} с Id: {2}", markParamName, elem.Name, elem.Id.IntegerValue), KPLN_Loader.Preferences.MessageType.Warning);
                    }
                }
                else
                {
                    //Print(string.Format("Не заполнена Марка у элемента: {0} с Id: {1}", elem.Name, elem.Id.IntegerValue), KPLN_Loader.Preferences.MessageType.Warning);
                }
            }
            return true;
        }

        public string name()
        {
            return "Маппинг параметров марки";
        }
    }
}
