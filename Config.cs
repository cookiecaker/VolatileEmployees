using BepInEx.Configuration;

namespace volatileEmployees
{
    class Config 
    {
        public ConfigEntry<bool> playerImmunity;
        public ConfigEntry<bool> patchGiantKiwi;

        public Config(ConfigFile cfg)
        {
            playerImmunity = cfg.Bind("Explosion Immunity", "Is the player immune to explosion damage and death?", true);
            patchGiantKiwi = cfg.Bind("Sapsucker explodes", "Will Giant Sapsuckers explode like normal enemies? (Slightly bugged)", false);
            
            Plugin.mls.LogDebug("Configs created!");

        }

    }
}
