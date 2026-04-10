using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace volatileEmployees.Patches
{
    class EnemyPatches
    {
        static MethodInfo getConfig = typeof(Plugin).GetMethod(nameof(Plugin.GetEnemiesExplode));
        static MethodInfo getNetObj = typeof(Unity.Netcode.NetworkBehaviour).GetProperty(nameof(Unity.Netcode.NetworkBehaviour.NetworkObject)).GetMethod;
        static MethodInfo spawnExplosion = typeof(VENetworker).GetMethod(nameof(VENetworker.SpawnExplosionEntity));
        static MethodInfo despawnEnemy = typeof(VENetworker).GetMethod(nameof(VENetworker.DespawnEnemy));

        /* Basic transpiler logic:
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

        [HarmonyPatch(typeof(BaboonBirdAI), nameof(BaboonBirdAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BaboonBirdAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "BaboonBirdAI";
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

        [HarmonyPatch (typeof(BlobAI), nameof(BlobAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BlobAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "BlobAI";
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

        [HarmonyPatch (typeof(BushWolfEnemy), nameof(BushWolfEnemy.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BushWolfEnemy_Kill(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "BushWolfEnemy";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int brfalse = 0;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Brfalse))
                {
                    brfalse++;
                    if (brfalse == 7)
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
            }
            if (startIndex != -1 && endIndex != -1)
            {
                MethodInfo killBushWolf = typeof(BushWolfEnemy).GetMethod(nameof(BushWolfEnemy.KillEnemyOnOwnerClient));

                codes.Insert(startIndex, OpCodes.Br, trueConfig);
                codes.Insert(startIndex, OpCodes.Call, killBushWolf);
                codes.Insert(startIndex, OpCodes.Ldc_I4_0);
                codes.Insert(startIndex, OpCodes.Ldarg_0);
                codes.Insert(startIndex, OpCodes.Call, spawnExplosion);
                codes.Insert(startIndex, OpCodes.Call, getNetObj);
                codes.Insert(startIndex, OpCodes.Ldarg_0);
                codes.Insert(startIndex, OpCodes.Brfalse, falseConfig);
                codes.Insert(startIndex, OpCodes.Call, getConfig);
            }

            Plugin.mls.LogDebug($"Successfully patched {name}!");
            return codes.AsEnumerable();
        }

        [HarmonyPatch (typeof(ButlerBeesEnemyAI), nameof (ButlerBeesEnemyAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ButlerBeesEnemyAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "ButlerBeesEnemyAI";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int brfalse = 0;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Brfalse))
                {
                    brfalse++;
                    if (brfalse == 2)
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
            }
            if (startIndex != -1 && endIndex != -1)
            {
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

        [HarmonyPatch (typeof(ButlerEnemyAI), nameof(ButlerEnemyAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "ButlerEnemyAI";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int stfld = 0;
            int beq = 0;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();
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
            }
            if (startIndex != -1 && endIndex != -1)
            {
                CodeInstruction codeGetConfig = new CodeInstruction(OpCodes.Call, getConfig);
                codeGetConfig.labels.Add(noCBSI);

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

        [HarmonyPatch (typeof(CadaverBloomAI), nameof(CadaverBloomAI.BurstForth))]
        [HarmonyPrefix]
        private static void YesBlooming()
        {
            PlayerPatches.isBlooming = true;
        }

        [HarmonyPatch(typeof(CadaverBloomAI), nameof(CadaverBloomAI.BurstForth))]
        [HarmonyPostfix]
        private static void NoBlooming()
        {
            PlayerPatches.isBlooming = false;
        }

        [HarmonyPatch(typeof(CadaverBloomAI), nameof(CadaverBloomAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CadaverGrowthAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "CadaverBloomAI";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int stfld = 0;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Call))
                {
                    stfld++;
                    if (stfld == 6)
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
                MethodInfo killCadaverBloom = typeof(CadaverBloomAI).GetMethod(nameof(CadaverBloomAI.KillEnemyOnOwnerClient));

                codes.Insert(startIndex, OpCodes.Br, trueConfig);
                codes.Insert(startIndex, OpCodes.Call, killCadaverBloom);
                codes.Insert(startIndex, OpCodes.Ldc_I4_0);
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


            [HarmonyPatch (typeof(CadaverGrowthAI), nameof(CadaverGrowthAI.OnLocalPlayerDie))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CadaverGrowthAI_DeathBloom(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "CadaverGrowthAI";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                codes[i].opcode = OpCodes.Nop;
            }
            Plugin.mls.LogDebug($"Successfully patched {name}!");
            return codes.AsEnumerable();
        }

        [HarmonyPatch (typeof(CaveDwellerAI), nameof(CaveDwellerAI.KillPlayerAnimationClientRpc))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CaveDwellerAI_Kill(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "CaveDwellerAI";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int brfalse = 0;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Brfalse))
                {
                    brfalse++;
                    if (brfalse == 5)
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
            }
            if (startIndex != -1 && endIndex != -1)
            {
                MethodInfo killCaveDweller = typeof(CaveDwellerAI).GetMethod(nameof(CaveDwellerAI.KillEnemyOnOwnerClient));

                codes.Insert(startIndex, OpCodes.Br, trueConfig);
                codes.Insert(startIndex, OpCodes.Call, killCaveDweller);
                codes.Insert(startIndex, OpCodes.Ldc_I4_0);
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

        [HarmonyPatch (typeof(CentipedeAI), nameof(CentipedeAI.DamagePlayerOnIntervals))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CentipedeAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "CentipedeAI";
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

        [HarmonyPatch (typeof(ClaySurgeonAI), nameof(ClaySurgeonAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ClaySurgeonAI_Kill(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "ClaySurgeonAI";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Brfalse))
                {
                    startIndex = i + 1;
                    codes[startIndex].labels.Add(falseConfig);

                    Plugin.mls.LogDebug($"{name} startIndex: {startIndex}");

                    for (int j = startIndex; j < codes.Count; j++)
                    {

                        if (codes[j].opcode.Equals(OpCodes.Ret))
                        {
                            endIndex = (j - 1);
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

        [HarmonyPatch (typeof(CrawlerAI), nameof(CrawlerAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CrawlerAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "CrawlerAI";
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

        // bugged - launches players and reduces their movement to less than a crawl
        [HarmonyPatch (typeof(GiantKiwiAI), nameof (GiantKiwiAI.AnimationEventB))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GiantKiwiAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "GiantKiwiAI";
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
                MethodInfo getConfigKiwi = typeof(Plugin).GetMethod(nameof(Plugin.GetPatchGiantKiwi));
                MethodInfo killGiantKiwi = typeof(GiantKiwiAI).GetMethod(nameof(GiantKiwiAI.KillEnemyOnOwnerClient));

                codes.Insert(startIndex, OpCodes.Br, trueConfig);
                codes.Insert(startIndex, OpCodes.Call, killGiantKiwi);
                codes.Insert(startIndex, OpCodes.Ldc_I4_0);
                codes.Insert(startIndex, OpCodes.Ldarg_0);
                codes.Insert(startIndex, OpCodes.Call, spawnExplosion);
                codes.Insert(startIndex, OpCodes.Call, getNetObj);
                codes.Insert(startIndex, OpCodes.Ldarg_0);
                codes.Insert(startIndex, OpCodes.Brfalse, falseConfig);
                codes.Insert(startIndex, OpCodes.Call, getConfigKiwi);

                Plugin.mls.LogDebug($"Successfully patched {name}!");
            }
            return codes.AsEnumerable();
        }

        [HarmonyPatch (typeof(HoarderBugAI), nameof(HoarderBugAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> HoarderBugAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "HoarderBugAI";
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

        [HarmonyPatch (typeof(JesterAI), nameof(JesterAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> JesterAI_Kill(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "JesterAI";
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

                        if (codes[j].opcode.Equals(OpCodes.Call))
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

        [HarmonyPatch (typeof(MouthDogAI), nameof(MouthDogAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> MouthDogAI_Kill(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "MouthDogAI";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Bne_Un))
                {
                    startIndex = i + 1;
                    codes[startIndex].labels.Add(falseConfig);

                    Plugin.mls.LogDebug($"{name} startIndex: {startIndex}");

                    for (int j = startIndex; j < codes.Count; j++)
                    {
                        if (codes[j].opcode.Equals(OpCodes.Call))
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

        [HarmonyPatch (typeof(NutcrackerEnemyAI), nameof(NutcrackerEnemyAI.LegKickPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> NutcrackerEnemyAI_Kill(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "NutcrackerEnemyAI";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Stloc_1))
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
                MethodInfo killNutcracker = typeof(NutcrackerEnemyAI).GetMethod(nameof(NutcrackerEnemyAI.KillEnemyOnOwnerClient));

                codes.Insert(startIndex, OpCodes.Br, trueConfig);
                codes.Insert(startIndex, OpCodes.Call, killNutcracker);
                codes.Insert(startIndex, OpCodes.Ldc_I4_0);
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

        [HarmonyPatch (typeof(PufferAI), nameof(PufferAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PufferAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "PufferAI";
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

        [HarmonyPatch(typeof(PumaAI), nameof(PumaAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PumaAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "PumaAI";
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
                    if (stfld == 2)
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
            }
            if (startIndex != -1 && endIndex != -1)
            {
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

        [HarmonyPatch (typeof(RadMechAI), nameof (RadMechAI.Stomp))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RadMechAI_DamageStomp(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            /* 
             * - after (if num < radius) check, add check for config
             * - if false, transfer to check for (if (double)num < (double)radius * 0.175)
             * - if true, continue with explosion spawning & deleting
                * - transfer to last codeinstruction (ret)
             */

            string name = "RadMechAI";
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

        [HarmonyPatch (typeof(RadMechAI), nameof(RadMechAI.BeginTorchPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RadMechAI_DamageTorch(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "RadMechAI";
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            int endIndex = -1;
            int ldfld = 0;
            Label falseConfig = il.DefineLabel();
            Label trueConfig = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Ldfld))
                {
                    ldfld++;
                    if (ldfld == 13)
                    {
                        startIndex = i + 2;
                        codes[startIndex].labels.Add(falseConfig);

                        Plugin.mls.LogDebug($"{name} startIndex: {startIndex}");

                        for (int j = startIndex; j < codes.Count; j++)
                        {

                            if (codes[j].opcode.Equals(OpCodes.Stfld))
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
            }
            if (startIndex != -1 && endIndex != -1)
            {
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

        [HarmonyPatch (typeof(RedLocustBees), nameof(RedLocustBees.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RedLocustBees_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "RedLocustBees";
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

        [HarmonyPatch (typeof(SandSpiderAI), nameof(SandSpiderAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SandSpiderAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "SandSpiderAI";
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

        [HarmonyPatch (typeof(SpringManAI), nameof (SpringManAI.OnCollideWithPlayer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SpringManAI_Damage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            string name = "SpringManAI";
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
