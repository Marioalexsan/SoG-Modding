using SoG.Modding.Core;

namespace SoG.Modding.Tools
{
    public static class NetUtils
    {
        public static bool IsLocalOrServer => ModGlobals.Game.xNetworkInfo.enCurrentRole != NetworkHelperInterface.NetworkRole.Client;

        public static bool IsClient => ModGlobals.Game.xNetworkInfo.enCurrentRole == NetworkHelperInterface.NetworkRole.Client;
    }
}
