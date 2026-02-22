using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace volatileEmployees.Patches.Enemies
{
    [HarmonyPatch(typeof(RedLocustBees))]
    [HarmonyPatch(nameof(RedLocustBees.OnCollideWithPlayer))]
    internal class RedLocustBeesPatch
    {
        private static string name = "RedLocustBees";

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            Label noNull = il.DefineLabel();
            int brfalse = 0;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Brfalse))
                {
                    brfalse++;
                    if (brfalse == 2)
                    {
                        codes[i].operand = noNull;
                    }
                }
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Br))
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
            MethodInfo despawnEnemy = typeof(VENetworker).GetMethod(nameof(VENetworker.DespawnEnemy));

            newCodes.AddRange(beforeDamage);

            CodeInstruction afterNull = new CodeInstruction(OpCodes.Ldarg_0);                  // RedLocustBees
            afterNull.labels.Add(noNull);
            newCodes.Add(afterNull);
            newCodes.Add(OpCodes.Call, getNetObj);
            newCodes.Add(OpCodes.Callvirt, spawnExplosion);

            newCodes.Add(OpCodes.Ldarg_0);                  // RedLocustBees - set label
            newCodes.Add(OpCodes.Call, getNetObj);
            newCodes.Add(OpCodes.Call, despawnEnemy);

            newCodes.AddRange(afterDamage);

            Plugin.mls.LogDebug($"Successfully patched {name}!");
            return newCodes.AsEnumerable();
        }
    }
}
