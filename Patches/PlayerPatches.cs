using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace volatileEmployees.Patches
{
    class PlayerPatches
    {
        static MethodInfo getConfig = typeof(Plugin).GetMethod(nameof(Plugin.GetPlayerImmunity));
        internal static bool isBlooming = false;

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PlayerControllerB_Kill(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "PlayerControllerB.KillPlayer";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            // find beginning of comparisons - add all of this (below) BEFORE comparisons
            // this is the first thing so should be OK to just add it before

            // load causeofdeath onto stack
            // load int 3 onto stack
            // load "ceq" (compare other two values for equality, returns 1 if equal)
            // load "stloc.0" (stores ceq as local variable)
            // load "brfalse.s" with created label

            // set other label to beginning of comparisons

            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            Label noBlast = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Ldarg_0))
                {
                    codes[i].labels.Add(noBlast);
                    break;
                }
            }

            newCodes.Add(OpCodes.Call, getConfig);              // load config for playerImmunity
            newCodes.Add(OpCodes.Brfalse, noBlast);             // if config is false (no immunity), transfer to noBlast
            newCodes.Add(OpCodes.Ldarg_S, 3);                   // load given causeOfDeath
            newCodes.Add(OpCodes.Ldc_I4_3);                     // load "blast" causeOfDeath
            newCodes.Add(OpCodes.Ceq);                          // compare - if equal, return 1
            newCodes.Add(OpCodes.Brfalse_S, noBlast);           // if 0, transfer to noBlast
            newCodes.Add(OpCodes.Ret);

            newCodes.AddRange(codes);

            Plugin.mls.LogDebug($"Successfully patched {name}!");

            return newCodes.AsEnumerable();
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPostfix]
        private static void KillPlayerPatch(PlayerControllerB __instance, CauseOfDeath causeOfDeath)
        {
            // death results in explosion (if death wasn't caused by explosion or blooming)
            if (__instance.IsOwner && __instance.AllowPlayerDeath() && !((int)causeOfDeath == (int)(CauseOfDeath.Blast)) && !isBlooming)
            {
                VENetworker.Instance.SpawnExplosionEntityRpc(__instance.NetworkObject);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PlayerControllerB_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "PlayerControllerB.DamagePlayer";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            Label noBlast = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Ldarg_0))
                {
                    codes[i].labels.Add(noBlast);
                    break;
                }
            }

            newCodes.Add(OpCodes.Call, getConfig);              // load config for playerImmunity
            newCodes.Add(OpCodes.Brfalse, noBlast);             // if config is false (no immunity), transfer to noBlast
            newCodes.Add(OpCodes.Ldarg_S, 4);                   // load given causeOfDeath
            newCodes.Add(OpCodes.Ldc_I4_3);                     // load "blast" causeOfDeath
            newCodes.Add(OpCodes.Ceq);                          // compare - if equal, return 1
            newCodes.Add(OpCodes.Brfalse_S, noBlast);           // if 0, transfer to noBlast
            newCodes.Add(OpCodes.Ret);

            newCodes.AddRange(codes);

            Plugin.mls.LogDebug($"Successfully patched {name}!");

            return newCodes.AsEnumerable();
        }
    }
}
