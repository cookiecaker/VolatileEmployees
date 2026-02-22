using HarmonyLib;

namespace volatileEmployees.Patches
{
    class NetworkPatch
    {
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        [HarmonyPostfix]
        static void StartOfRound_Post_Awake()
        {
            VENetworker.Create();
        }

        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        [HarmonyPostfix]
        public static void GameNetworkManager_Post_Start()
        {
            VENetworker.Init();
            Plugin.mls.LogDebug("Initialized VENetworker object!");
        }

    }
}
