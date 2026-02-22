using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GameNetcodeStuff;

namespace volatileEmployees.Patches.Player
{
    class KillPlayerPatch
    {
        private static string name = "PlayerControllerB.KillPlayer";

        [HarmonyPatch(typeof(PlayerControllerB))]
        [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
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

            newCodes.Add(OpCodes.Ldarg_S, 3);                   // load given causeOfDeath
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
