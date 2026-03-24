using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;

namespace volatileEmployees
{
    class Config : SyncedConfig2<Config>
    {
        [SyncedEntryField] public SyncedEntry<bool> playerImmunity;
        [SyncedEntryField] public SyncedEntry<bool> patchGiantKiwi;
        [SyncedEntryField] public SyncedEntry<bool> enemiesExplode;

        public Config(ConfigFile cfg) : base(Plugin.modGUID)
        {
            playerImmunity = cfg.BindSyncedEntry(new ConfigDefinition("Player", "playerImmunity"), true, new ConfigDescription("Is the player immune to explosion damage and death?"));
            enemiesExplode = cfg.BindSyncedEntry(new ConfigDefinition("Enemies", "enemiesExplode"), true, new ConfigDescription("Will most enemies explode?"));
            patchGiantKiwi = cfg.BindSyncedEntry(new ConfigDefinition("Enemies", "patchGiantKiwi"), false, new ConfigDescription("Will Giant Sapsuckers explode? (Slightly bugged)"));

            ConfigManager.Register(this);
            Plugin.mls.LogDebug("Configs created!");

        }

    }
}
