using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace volatileEmployees.Patches.Enemies
{
    [HarmonyPatch(typeof(BaboonBirdAI))]
    [HarmonyPatch(nameof(BaboonBirdAI.OnCollideWithPlayer))]
    internal class BaboonBirdAIPatch
    {
        private static string name = "BaboonBirdAI";
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {

            /* 
             * - find Stfld (where DamagePlayer begins), set startIndex (+1 from i) and add falseConfig label
             * - find Callvirt (DamagePlayer is called), set endIndex (+1 from j)
             * - insert ilcode:
                 * 1. load config from plugin (GetMethod)
                 * 2. check - if false, transfer to falseConfig
                 * 3. load SpawnExplosion stuff
                 * 4. transfer to trueConfig
             * - add trueConfig label to (endIndex + 1)

             * final IL should be something like this:
             * 1. load config from plugin 
             * 2. check - if false, transfer to (falseConfig)
             * 3. load SpawnExplosion stuff
             * 4. transfer to (trueConfig)
             * 5. (falseConfig) load DamagePlayer stuff
             * 6. (trueConfig) check for isPlayerDead
             */

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

                    Plugin.mls.LogInfo($"{name} startIndex: {startIndex}");

                    for (int j = startIndex; j < codes.Count; j++)
                    {

                        if (codes[j].opcode.Equals(OpCodes.Callvirt))
                        {
                            endIndex = j;
                            codes[endIndex + 1].labels.Add(trueConfig);

                            Plugin.mls.LogInfo($"{name} endIndex: {endIndex}");
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

                codes.Insert(startIndex, OpCodes.Br, trueConfig);
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