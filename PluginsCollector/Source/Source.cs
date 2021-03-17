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
                default:
                    throw new Exception("Undefined icon!");
            }
        }
    }

}
