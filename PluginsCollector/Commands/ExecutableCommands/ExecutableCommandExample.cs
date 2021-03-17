using Autodesk.Revit.UI;
using KPLN_Loader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KPLN_Loader.Output.Output;

namespace PluginsCollector.Commands.ExecutableCommands
{
    public class ExecutableCommandExample : IExecutableCommand
    {
        /// <summary>
        /// Пример команды, которую можно запустить из любого места. Команда добавляется в коллекцию <see cref="KPLN_Loader.Preferences.CommandQueue"/>,
        /// затем вызывается в контексте текущего <see cref="Autodesk.Revit.ApplicationServices.ControlledApplication"/>.
        /// <code>
        /// <see cref="KPLN_Loader.Preferences.CommandQueue"/>.Enqueue(<see cref="IExecutableCommand"/> cmd)
        /// </code>
        /// </summary>
        /// <param name="app"></param>
        /// <returns>Если успешно - return <see cref="Result.Succeeded"/>; иначе return <see cref="Result.Failed"/>; или return <see cref="Result.Cancelled"/>;</returns>
        public Result Execute(UIApplication app)
        {
            Print("Hello Revit!", KPLN_Loader.Preferences.MessageType.Success);
            return Result.Succeeded;
        }
    }
}
