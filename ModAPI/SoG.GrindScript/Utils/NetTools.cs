using SoG.Modding.Core;

namespace SoG.Modding.Utils
{
    public static class NetTools
    {
        public static bool IsLocalOrServer => APIGlobals.Game.xNetworkInfo.enCurrentRole != NetworkHelperInterface.NetworkRole.Client;

        public static bool IsClient => APIGlobals.Game.xNetworkInfo.enCurrentRole == NetworkHelperInterface.NetworkRole.Client;
    }
}
