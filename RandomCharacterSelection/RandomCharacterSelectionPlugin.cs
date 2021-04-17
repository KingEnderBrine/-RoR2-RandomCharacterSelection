using BepInEx;
using BepInEx.Logging;
using RoR2;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: R2API.Utils.ManualNetworkRegistration]
[assembly: EnigmaticThunder.Util.ManualNetworkRegistration]
namespace RandomCharacterSelection
{
    [BepInDependency("com.KingEnderBrine.ScrollableLobbyUI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.KingEnderBrine.InLobbyConfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.KingEnderBrine.RandomCharacterSelection", "Random Character Selection", "1.3.1")]
    public class RandomCharacterSelectionPlugin : BaseUnityPlugin
    {
        internal const string RANDOMIZE_CHARACTER_BUTTON = nameof(RANDOMIZE_CHARACTER_BUTTON);

        internal static RandomCharacterSelectionPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger { get => Instance?.Logger; }
        internal static bool ScrollableLobbyUILoaded { get; private set; }
        internal static bool InLobbyConfigLoaded { get; private set; } 

        private void Awake()
        {
            Instance = this;

            ScrollableLobbyUILoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ScrollableLobbyUI");
            InLobbyConfigLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.InLobbyConfig");
            
            AssetBundleHelper.LoadAssetBundle();

            ConfigHelper.InitConfig(Config);

            On.RoR2.UI.CharacterSelectController.Awake += RandomizePanelController.CharacterSelectControllerAwake;
            On.RoR2.UI.EclipseRunScreenController.Start += RandomizePanelController.EclipseRunScreenControllerStart;
            On.RoR2.Language.LoadStrings += LoadStrings;
        }

        private void Destroy()
        {
            Instance = null;

            AssetBundleHelper.UnloadAssetBundle();
            On.RoR2.UI.CharacterSelectController.Awake -= RandomizePanelController.CharacterSelectControllerAwake;
            On.RoR2.UI.EclipseRunScreenController.Start -= RandomizePanelController.EclipseRunScreenControllerStart;
            On.RoR2.Language.LoadStrings -= LoadStrings;
        }

        private static void LoadStrings(On.RoR2.Language.orig_LoadStrings orig, Language self)
        {
            orig(self);

            switch (self.name.ToLower())
            {
                case "ru":
                    self.SetStringByToken(RANDOMIZE_CHARACTER_BUTTON, "Выбрать случайного");
                    break;
                default:
                    self.SetStringByToken(RANDOMIZE_CHARACTER_BUTTON, "Pick random");
                    break;
            }
        }
    }
}