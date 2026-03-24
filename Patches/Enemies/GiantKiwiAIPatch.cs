using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

// bugged (after the bird explodes, sprinting stops working until rejoining) but functional
namespace volatileEmployees.Patches.Enemies
{
    [HarmonyPatch(typeof(GiantKiwiAI))]
    [HarmonyPatch(nameof(GiantKiwiAI.AnimationEventB))]
    internal class GiantKiwiAIPatch
    {
        private static string name = "GiantKiwiAI";

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int stfld = 0;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Stfld))
                {
                    stfld++;
                    if (stfld == 3)
                    {
                        startIndex = i + 1;
                        codes[startIndex].labels.Add(falseConfig);

                        Plugin.mls.LogDebug($"{name} startIndex: {startIndex}");

                        for (int j = startIndex; j < codes.Count; j++)
                        {

                            if (codes[j].opcode.Equals(OpCodes.Callvirt))
                            {
                                endIndex = j + 1;
                                codes[endIndex + 1].labels.Add(trueConfig);

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
                MethodInfo getConfig = typeof(Plugin).GetMethod(nameof(Plugin.GetPatchGiantKiwi));
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
