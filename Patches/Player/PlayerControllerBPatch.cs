using GameNetcodeStuff;
using HarmonyLib;

namespace volatileEmployees.Patches.Player
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPostfix]
        private static void KillPlayerPatch(PlayerControllerB __instance, CauseOfDeath causeOfDeath)
        {
            // death results in explosion (if death wasn't caused by explosion)
            if (__instance.IsOwner && __instance.AllowPlayerDeath() && !((int)causeOfDeath == (int)(CauseOfDeath.Blast)))
            {
                VENetworker.Instance.SpawnExplosionPlayerRpc(__instance.NetworkObject);
            }

        }
    }
}
