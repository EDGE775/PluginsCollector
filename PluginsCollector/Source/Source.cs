using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static PluginsCollector.Common.Collections;

namespace PluginsCollector.Source
{
    public class Source
    {
        public string Value { get; }
        private static string AssemblyPath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
        public Source(Icon icon)
        {
            switch (icon)
            {
                case Icon.PluginsCollector:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.UnjoinGeometry:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.FixHoles:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.INGDParamCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.ABSParamKRCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.ABSParamARCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.CopyProjectParamsCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\CopyProjectParams.png");
                    break;
                case Icon.SetElementIdCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.OverrideGraphicsByIdOpenForm:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.MTSCParamKRCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.DSTGParamKRCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.LEVELParamKRCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.CopyParamValuesCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.AddLoaderCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                case Icon.DIVFixRebarImagesCommand:
                    Value = Path.Combine(AssemblyPath, @"Source\ImageData\PluginsCollector.png");
                    break;
                default:
                    throw new Exception("Undefined icon!");
            }
        }
    }

}
