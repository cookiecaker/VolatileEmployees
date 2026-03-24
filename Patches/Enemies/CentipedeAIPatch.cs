using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace volatileEmployees.Patches.Enemies
{
    [HarmonyPatch(typeof(CentipedeAI))]
    [HarmonyPatch(nameof(CentipedeAI.DamagePlayerOnIntervals))]
    internal class CentipedeAIPatch
    {
        private static string name = "CentipedeAI";

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int ldfld = 0;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();
            List<Label> storedLabels = new List<Label>();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Ldfld)) { ldfld++; }
                if (ldfld == 8)
                {
                    startIndex = i - 1;
                    storedLabels.AddRange(codes[startIndex].labels);
                    codes[startIndex].labels.Clear();
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

                CodeInstruction codeGetConfig = new CodeInstruction(OpCodes.Call, getConfig);
                codeGetConfig.labels.AddRange(storedLabels);

                codes.Insert(startIndex, OpCodes.Br, trueConfig);
                codes.Insert(startIndex, OpCodes.Call, spawnExplosion);
                codes.Insert(startIndex, OpCodes.Call, getNetObj);
                codes.Insert(startIndex, OpCodes.Ldarg_0);
                codes.Insert(startIndex, OpCodes.Brfalse, falseConfig);
                codes.Insert(startIndex, codeGetConfig);

                Plugin.mls.LogDebug($"Successfully patched {name}!");
            }
            return codes.AsEnumerable();
        }

    }
}
