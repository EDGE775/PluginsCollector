using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsCollector.Common
{
    public static class Collections
    {
        /// <summary>
        /// Спискок иконок для работы класса <see cref="PluginsCollector.Source.Source"/>.
        /// Сделано для ускорения процесса обращения к элементам внутри папки Source.
        /// </summary>
        public enum Icon { PluginsCollector, UnjoinGeometry, FixHoles, INGDParamCommand, ABSParamKRCommand, ABSParamARCommand, CopyProjectParamsCommand, SetElementIdCommand, OverrideGraphicsByIdOpenForm, MTSCParamKRCommand, DSTGParamKRCommand, LEVELParamKRCommand, CopyParamValuesCommand, AddLoaderCommand, DIVFixRebarImagesCommand }
    }
}
