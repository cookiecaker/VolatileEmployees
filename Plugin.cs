using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using volatileEmployees.Patches;
using volatileEmployees.Patches.Player;
using volatileEmployees.Patches.Enemies;
using BepInEx.Configuration;

namespace volatileEmployees
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "cookiecaker.volatileEmployees";
        public const string modName = "Volatile_Employees";
        public const string modVersion = "0.1.0";

        private static Harmony _harmony = new Harmony(modGUID);
        internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        internal static new Config Config;

        void Awake()
        {
            Config = new Config(base.Config);

            NetcodePatcher();
            _harmony.PatchAll(typeof(NetworkPatch));
            mls.LogInfo("Network patch successful!");

            _harmony.PatchAll(typeof(PlayerControllerBPatch));      // + SpawnExplosionPlayer

            if (Plugin.Config.playerImmunity.Value)
            {
                _harmony.PatchAll(typeof(DamagePlayerPatch));
                _harmony.PatchAll(typeof(KillPlayerPatch));
                mls.LogInfo("Player patches successful!");
            }
            else
            {
                mls.LogInfo("Player is not immune to explosions. Good luck!");
            }

            _harmony.PatchAll(typeof(BaboonBirdAIPatch));
            _harmony.PatchAll(typeof(BlobAIPatch));
            _harmony.PatchAll(typeof(BushWolfEnemyPatch));
            _harmony.PatchAll(typeof(ButlerBeesEnemyAIPatch));
            _harmony.PatchAll(typeof(ButlerEnemyAIPatch));
            _harmony.PatchAll(typeof(CaveDwellerAIPatch));
            _harmony.PatchAll(typeof(CentipedeAIPatch));
            _harmony.PatchAll(typeof(ClaySurgeonAIPatch));
            _harmony.PatchAll(typeof(CrawlerAIPatch));
            if (Plugin.Config.patchGiantKiwi.Value)
            {
                _harmony.PatchAll(typeof(GiantKiwiAIPatch));
            }
            _harmony.PatchAll(typeof(HoarderBugAIPatch));
            _harmony.PatchAll(typeof(JesterAIPatch));
            _harmony.PatchAll(typeof(MouthDogAIPatch));
            _harmony.PatchAll(typeof(NutcrackerEnemyAIPatch));
            _harmony.PatchAll(typeof(PufferAIPatch));
            _harmony.PatchAll(typeof(RadMechAIPatchA));         // stomp
            _harmony.PatchAll(typeof(RadMechAIPatchB));         // torch
            _harmony.PatchAll(typeof(RedLocustBeesPatch));
            _harmony.PatchAll(typeof(SandSpiderAIPatch));
            _harmony.PatchAll(typeof(SpringManAIPatch));

            mls.LogInfo("Enemy patches successful!");
        }

        // source: https://github.com/EvaisaDev/UnityNetcodePatcher
        void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}
