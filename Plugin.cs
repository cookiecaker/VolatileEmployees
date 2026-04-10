using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using volatileEmployees.Patches;
using BepInEx.Configuration;

namespace volatileEmployees
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "cookiecaker.volatileEmployees";
        public const string modName = "Volatile_Employees";
        public const string modVersion = "1.0.0";

        private static Harmony _harmony = new Harmony(modGUID);
        internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        internal static new Config Config;

        void Awake()
        {
            Config = new Config(base.Config);

            NetcodePatcher();
            _harmony.PatchAll(typeof(NetworkPatch));
            mls.LogInfo("Network patch successful!");
            _harmony.PatchAll(typeof(PlayerPatches));
            mls.LogInfo("Player patches successful!");
            _harmony.PatchAll(typeof(EnemyPatches));
            mls.LogInfo("Enemy patches successful!");
        }

        public static bool GetPlayerImmunity()
        {
            return Config.playerImmunity.Value;
        }
        public static bool GetEnemiesExplode()
        {
            return Config.enemiesExplode.Value;
        }
        public static bool GetPatchGiantKiwi()
        {
            return Config.patchGiantKiwi.Value;
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
