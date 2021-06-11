using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    public class NativeInterface
    {
        public static int Initialize(string argument)
        {
            GrindScript.Initialize();
            return 1;
        }
    }
}
