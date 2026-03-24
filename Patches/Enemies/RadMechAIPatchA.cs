using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace volatileEmployees.Patches.Enemies
{
    [HarmonyPatch(typeof(RadMechAI))]
    [HarmonyPatch(nameof(RadMechAI.Stomp))]
    class RadMechAIPatchA // stomp
    {
        private static string name = "RadMechAI.Stomp";
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            /* 
             * - after (if num < radius) check, add check for config
             * - if false, transfer to check for (if (double)num < (double)radius * 0.175)
             * - if true, continue with explosion spawning & deleting
                * - transfer to last codeinstruction (ret)
             */

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            codes[codes.Count - 1].labels.Add(trueConfig);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Conv_R8))
                {
                    startIndex = i - 1;
                    codes[startIndex].labels.Add(falseConfig);

                    Plugin.mls.LogDebug($"{name} startIndex: {startIndex}");
                    break;
                }
            }

            if (startIndex != -1)
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
