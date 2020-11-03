using BepInEx;
using BepInEx.Logging;
using R2API;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace RandomCharacterSelection
{
    [BepInDependency("com.KingEnderBrine.ScrollableLobbyUI", BepInDependency.DependencyFlags.SoftDependency)]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.KingEnderBrine.RandomCharacterSelection", "Random Character Selection", "1.1.0")]
    public class RandomCharacterSelectionPlugin : BaseUnityPlugin
    {
        internal const string RANDOMIZE_CHARACTER_BUTTON = nameof(RANDOMIZE_CHARACTER_BUTTON);

        internal static RandomCharacterSelectionPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger { get => Instance?.Logger; }
        internal static bool ScrollableLobbyUILoaded { get; private set; } 

        private void Awake()
        {
            Instance = this;

            ScrollableLobbyUILoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ScrollableLobbyUI");
            
            AddLanguageTokens();
            AssetBundleHelper.LoadAssetBundle();

            On.RoR2.UI.CharacterSelectController.Awake += RandomizePanelController.CharacterSelectControllerAwake;
            On.RoR2.UI.EclipseRunScreenController.Start += RandomizePanelController.EclipseRunScreenControllerStart;
        }

        private void Destroy()
        {
            Instance = null;

            AssetBundleHelper.UnloadAssetBundle();
            On.RoR2.UI.CharacterSelectController.Awake -= RandomizePanelController.CharacterSelectControllerAwake;
            On.RoR2.UI.EclipseRunScreenController.Start -= RandomizePanelController.EclipseRunScreenControllerStart;
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(RANDOMIZE_CHARACTER_BUTTON, "Pick random");
            LanguageAPI.Add(RANDOMIZE_CHARACTER_BUTTON, "Pick random", "en");
            LanguageAPI.Add(RANDOMIZE_CHARACTER_BUTTON, "Выбрать случайного", "RU");
        }
    }
}