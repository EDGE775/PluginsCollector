using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using KPLN_Loader.Common;
using PluginsCollector.Commands.ExternalCommands;
using PluginsCollector.Forms;
using PluginsCollector.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KPLN_Loader.Output.Output;

namespace PluginsCollector.Commands.ExecutableCommands
{
    public class OverrideGraphicsByIdCommand : IExecutableCommand
    {
        private AskingForm form { get; set; }

        public OverrideGraphicsByIdCommand(AskingForm form)
        {
            this.form = form;
        }
        public Result Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            int firstId = form.firstId;
            int lastId = 0;
            Color color = new Color(255, 1, 1);

            if (form.lastId == 0)
            {
                lastId = ViewUtils.getLastIdNumber(doc);
            }
            else
            {
                lastId = form.lastId;
            }

            ParamAction action;
            if (form.clear)
            {
                action = new StandartOverrideSingleViewAction(doc, firstId, lastId);
            }
            else
            {
                action = new OverrideSingleViewAction(doc, firstId, lastId, color);
            }

            using (Transaction t = new Transaction(doc))
            {
                t.Start(action.name());
                Print(action.name(), KPLN_Loader.Preferences.MessageType.Regular);
                if (action.execute())
                {
                    Print("Завершено успешно!", KPLN_Loader.Preferences.MessageType.Success);
                }
                else
                {
                    Print("Завершено с ошибками!", KPLN_Loader.Preferences.MessageType.Error);
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }
}
