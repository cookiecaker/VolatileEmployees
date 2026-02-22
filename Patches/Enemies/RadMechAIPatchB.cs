using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace volatileEmployees.Patches.Enemies
{
    [HarmonyPatch(typeof(RadMechAI))]
    [HarmonyPatch(nameof(RadMechAI.BeginTorchPlayer))]
    class RadMechAIPatchB
    {
        private static string name = "RadMechAI.BeginTorchPlayer";
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int ldfld = 0;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Ldfld))
                {
                    ldfld++;
                    if (ldfld == 13)
                    {
                        startIndex = i + 3;
                        Plugin.mls.LogDebug($"{name} startIndex: {startIndex}");

                        for (int j = startIndex; j < codes.Count; j++)
                        {

                            if (codes[j].opcode.Equals(OpCodes.Stfld))
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
            List<CodeInstruction> beforeDamage = codes.GetRange(0, endIndex - 1); 
            List<CodeInstruction> afterDamage = codes.GetRange(endIndex, codes.Count - endIndex);
            List<CodeInstruction> newCodes = new List<CodeInstruction>();

            MethodInfo getNetObj = typeof(Unity.Netcode.NetworkBehaviour).GetProperty(nameof(Unity.Netcode.NetworkBehaviour.NetworkObject)).GetMethod;
            MethodInfo spawnExplosion = typeof(VENetworker).GetMethod(nameof(VENetworker.SpawnExplosionEnemy));
            MethodInfo despawnEnemy = typeof(VENetworker).GetMethod(nameof(VENetworker.DespawnEnemy));

            newCodes.AddRange(beforeDamage);

            newCodes.Add(OpCodes.Ldarg_0);                  // RadMech
            newCodes.Add(OpCodes.Call, getNetObj);
            newCodes.Add(OpCodes.Callvirt, spawnExplosion);

            newCodes.Add(OpCodes.Ldarg_0);                  // RadMech
            newCodes.Add(OpCodes.Call, getNetObj);
            newCodes.Add(OpCodes.Call, despawnEnemy);

            newCodes.AddRange(afterDamage);

            Plugin.mls.LogDebug($"Successfully patched {name}!");
            return newCodes.AsEnumerable();
        }
    }
}
