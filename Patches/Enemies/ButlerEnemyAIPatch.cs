using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace volatileEmployees.Patches.Enemies
{
    [HarmonyPatch(typeof(ButlerEnemyAI))]
    [HarmonyPatch(nameof(ButlerEnemyAI.OnCollideWithPlayer))]
    internal class ButlerEnemyAIPatch
    {
        private static string name = "ButlerEnemyAI";

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int stfld = 0;
            int beq = 0;
            Label noCBSI = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Beq))
                {
                    beq++;
                    if (beq == 2)
                    {
                        codes[i].operand = noCBSI;
                        break;
                    }
                }
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Stfld))
                {
                    stfld++;
                    if (stfld == 3)
                    {
                        startIndex = i + 2;
                        Plugin.mls.LogDebug($"{name} startIndex: {startIndex}");

                        for (int j = startIndex; j < codes.Count; j++)
                        {

                            if (codes[j].opcode.Equals(OpCodes.Callvirt))
                            {
                                endIndex = j;
                                Plugin.mls.LogDebug($"{name} endIndex: {endIndex}");
                                break;
                            }
                        }
                        break;

                    }
                }
            }
            if (startIndex != -1 && endIndex != -1)
            {
                for (int i = startIndex - 1; i <= endIndex; i++)
                {
                    codes[i].opcode = OpCodes.Nop;
                }
            }

            // lists of code instructions - splice to add explosion code
            List<CodeInstruction> beforeDamage = codes.GetRange(0, startIndex - 1);
            List<CodeInstruction> afterDamage = codes.GetRange(startIndex - 1, codes.Count - startIndex + 1);
            List<CodeInstruction> newCodes = new List<CodeInstruction>();

            MethodInfo getNetObj = typeof(Unity.Netcode.NetworkBehaviour).GetProperty(nameof(Unity.Netcode.NetworkBehaviour.NetworkObject)).GetMethod;
            MethodInfo spawnExplosion = typeof(VENetworker).GetMethod(nameof(VENetworker.SpawnExplosionEnemy));

            newCodes.AddRange(beforeDamage);

            CodeInstruction afterCBSI = new CodeInstruction(OpCodes.Ldarg_0);                  // ButlerEnemy
            afterCBSI.labels.Add(noCBSI);
            newCodes.Add(afterCBSI);
            newCodes.Add(OpCodes.Call, getNetObj);
            newCodes.Add(OpCodes.Callvirt, spawnExplosion);

            newCodes.AddRange(afterDamage);

            Plugin.mls.LogDebug($"Successfully patched {name}!");

            return newCodes.AsEnumerable();
        }
    }
}
