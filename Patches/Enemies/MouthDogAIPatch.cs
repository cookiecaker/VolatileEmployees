using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace volatileEmployees.Patches.Enemies
{
    [HarmonyPatch(typeof(MouthDogAI))]
    [HarmonyPatch(nameof(MouthDogAI.OnCollideWithPlayer))]
    internal class MouthDogAIPatch
    {
        private static string name = "MouthDogAI";

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Bne_Un))
                {
                    startIndex = i + 2;
                    Plugin.mls.LogDebug($"{name} startIndex: {startIndex}");

                    for (int j = startIndex; j < codes.Count; j++)
                    {
                        if (codes[j].opcode.Equals(OpCodes.Call))
                        {
                            endIndex = j;
                            Plugin.mls.LogDebug($"{name} endIndex: {endIndex}");
                            break;
                        }
                    }
                    break;
                }
            }
            if (startIndex != -1 && endIndex != -1)
            {
                codes.RemoveRange(startIndex, endIndex - startIndex + 1);
            }

            // lists of code instructions - splice to add explosion code
            List<CodeInstruction> beforeDamage = codes.GetRange(0, startIndex - 1);
            List<CodeInstruction> afterDamage = codes.GetRange(startIndex, codes.Count - startIndex);
            List<CodeInstruction> newCodes = new List<CodeInstruction>();

            MethodInfo getNetObj = typeof(Unity.Netcode.NetworkBehaviour).GetProperty(nameof(Unity.Netcode.NetworkBehaviour.NetworkObject)).GetMethod;
            MethodInfo spawnExplosion = typeof(VENetworker).GetMethod(nameof(VENetworker.SpawnExplosionEnemy));

            newCodes.AddRange(beforeDamage);

            newCodes.Add(OpCodes.Ldarg_0);                  // MouthDog
            newCodes.Add(OpCodes.Call, getNetObj);
            newCodes.Add(OpCodes.Callvirt, spawnExplosion);

            newCodes.AddRange(afterDamage);

            Plugin.mls.LogDebug($"Successfully patched {name}!");
            return newCodes.AsEnumerable();
        }
    }
}
