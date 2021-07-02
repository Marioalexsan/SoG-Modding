using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;

namespace SoG.Modding.Content
{
    public abstract class ModdingLogic
    {
        protected GrindScript _modAPI;

        public ModdingLogic(GrindScript modAPI)
        {
            _modAPI = modAPI;
        }
    }
}
