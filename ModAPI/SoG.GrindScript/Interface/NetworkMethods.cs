using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    public static partial class GrindScript
    {
        public static bool IsLocalOrServer => Game.xNetworkInfo.enCurrentRole != NetworkHelperInterface.NetworkRole.Client;

        public static bool IsClient => Game.xNetworkInfo.enCurrentRole == NetworkHelperInterface.NetworkRole.Client;
    }
}
