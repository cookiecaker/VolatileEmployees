using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace volatileEmployees.Patches.Enemies
{
    [HarmonyPatch(typeof(BlobAI))]
    [HarmonyPatch(nameof(BlobAI.OnCollideWithPlayer))]
    internal class BlobAIPatch
    {
        private static string name = "BlobAI";

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Stfld))
                {
                    startIndex = i + 1;
                    codes[startIndex].labels.Add(falseConfig);

                    Plugin.mls.LogDebug($"{name} startIndex: {startIndex}");

                    for (int j = startIndex; j < codes.Count; j++)
                    {

                        if (codes[j].opcode.Equals(OpCodes.Callvirt))
                        {
                            endIndex = j;
                            codes[endIndex + 1].labels.Add(trueConfig);

                            Plugin.mls.LogDebug($"{name} endIndex: {endIndex}");
                            break;
                        }
                    }
                    break;
                }
            }
            if (startIndex != -1 && endIndex != -1)
            {
                MethodInfo getConfig = typeof(Plugin).GetMethod(nameof(Plugin.GetEnemiesExplode));
                MethodInfo getNetObj = typeof(Unity.Netcode.NetworkBehaviour).GetProperty(nameof(Unity.Netcode.NetworkBehaviour.NetworkObject)).GetMethod;
                MethodInfo spawnExplosion = typeof(VENetworker).GetMethod(nameof(VENetworker.SpawnExplosionEnemy));
                MethodInfo despawnEnemy = typeof(VENetworker).GetMethod(nameof(VENetworker.DespawnEnemy));

                codes.Insert(startIndex, OpCodes.Br, trueConfig);
                codes.Insert(startIndex, OpCodes.Call, despawnEnemy);
                codes.Insert(startIndex, OpCodes.Call, getNetObj);
                codes.Insert(startIndex, OpCodes.Ldarg_0);
                codes.Insert(startIndex, OpCodes.Call, spawnExplosion);
                codes.Insert(startIndex, OpCodes.Call, getNetObj);
                codes.Insert(startIndex, OpCodes.Ldarg_0);
                codes.Insert(startIndex, OpCodes.Brfalse, falseConfig);
                codes.Insert(startIndex, OpCodes.Call, getConfig);

                Plugin.mls.LogDebug($"Successfully patched {name}!");
            }
            return codes.AsEnumerable();
        }
    }
}


