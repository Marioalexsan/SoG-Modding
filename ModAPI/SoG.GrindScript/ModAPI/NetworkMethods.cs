namespace SoG.Modding
{
    public static class ModNetworking
    {
        public static bool IsLocalOrServer => GrindScript.Game.xNetworkInfo.enCurrentRole != NetworkHelperInterface.NetworkRole.Client;

        public static bool IsClient => GrindScript.Game.xNetworkInfo.enCurrentRole == NetworkHelperInterface.NetworkRole.Client;
    }
}
