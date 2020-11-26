using BepInEx.Configuration;
using System.Runtime.CompilerServices;

namespace RandomCharacterSelection
{
    public static class ConfigHelper
    {
        public static ConfigEntry<bool> CanSelectSameCharacter { get; private set; }

        public static void InitConfig(ConfigFile config)
        {
            CanSelectSameCharacter = config.Bind("Main", nameof(CanSelectSameCharacter), false);

            if (RandomCharacterSelectionPlugin.InLobbyConfigLoaded)
            {
                InitInLobbyConfig(config);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void InitInLobbyConfig(ConfigFile config)
        {
            InLobbyConfig.ModConfigCatalog.Add(InLobbyConfig.Fields.ConfigFieldUtilities.CreateFromBepInExConfigFile(config, "Random Character Selection"));
        }
    }
}
