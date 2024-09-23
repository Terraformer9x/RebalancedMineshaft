using BepInEx.Configuration;

namespace RebalancedMineshaft;

internal class RebalancedMineshaftConfig
{
    internal static ConfigEntry<bool>
        extraScrapSpawn,
        increasedManeaterChance;

    internal static void Bind(ConfigFile config)
    {
        extraScrapSpawn = config.Bind(
            "Mineshaft",
            "Spawn 6 extra scrap",
            true,
            "Should Mineshaft give 6 extra scrap?"
        );
        increasedManeaterChance = config.Bind(
            "Mineshaft",
            "Increased Maneater spawn",
            true,
            "Should Maneaters have more commonality (1.7x spawn chance) on Mineshaft?\n\n"
        );
    }
}
