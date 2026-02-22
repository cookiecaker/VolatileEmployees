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
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // assign nop normally

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndexA = -1;
            int endIndexA = -1;
            int startIndexB = -1;
            int endIndexB = -1;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Mul))
                {
                    startIndexA = i + 3;
                    Plugin.mls.LogDebug($"{name}#1 startIndex: {startIndexA}");

                    for (int j = startIndexA; j < codes.Count; j++)
                    {

                        if (codes[j].opcode.Equals(OpCodes.Callvirt))
                        {
                            endIndexA = j;
                            Plugin.mls.LogDebug($"{name}#1 endIndex: {endIndexA}");
                            break;
                        }
                    }
                    break;
                }
            }

            for (int i = startIndexA; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Mul))
                {
                    startIndexB = i + 3;
                    Plugin.mls.LogDebug($"{name}#2 startIndex: {startIndexB}");

                    for (int j = startIndexB; j < codes.Count; j++)
                    {

                        if (codes[j].opcode.Equals(OpCodes.Callvirt))
                        {
                            endIndexB = j;
                            Plugin.mls.LogDebug($"{name}#2 endIndex: {endIndexB}");
                            break;
                        }
                    }
                    break;
                }
            }
            if (startIndexA != -1 && endIndexA != -1)
            {
                for (int i = startIndexA - 1; i <= endIndexA; i++)
                {
                    codes[i].opcode = OpCodes.Nop;
                }
            }

            if (endIndexB != -1 && endIndexB != -1)
            {
                for (int i = startIndexB - 1; i <= endIndexB; i++)
                {
                    codes[i].opcode = OpCodes.Nop;
                }
            }

            
            

            MethodInfo getNetObj = typeof(Unity.Netcode.NetworkBehaviour).GetProperty(nameof(Unity.Netcode.NetworkBehaviour.NetworkObject)).GetMethod;
            MethodInfo spawnExplosion = typeof(VENetworker).GetMethod(nameof(VENetworker.SpawnExplosionEnemy));
            MethodInfo despawnEnemy = typeof(VENetworker).GetMethod(nameof(VENetworker.DespawnEnemy));

            int insert = -1;

            // run through entire thing till nop
            // insert explode/kill
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Nop))
                {
                    insert = i;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Ldarg_0));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Call, getNetObj));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Call, spawnExplosion));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Ldarg_0));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Call, getNetObj));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Call, despawnEnemy));
                    insert++;
                    break;
                }
            }

            // continue, run through until !nop
            for (int i = insert; i < codes.Count; i++)
            {
                if (!codes[i].opcode.Equals(OpCodes.Nop))
                {
                    break;
                }
            }

            // run through until nop
            // insert explode/kill
            for (int i = insert; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Nop))
                {
                    insert = i;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Ldarg_0));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Call, getNetObj));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Call, spawnExplosion));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Ldarg_0));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Call, getNetObj));
                    insert++;
                    codes.Insert(insert, new CodeInstruction(OpCodes.Call, despawnEnemy));
                    insert++;
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
}
