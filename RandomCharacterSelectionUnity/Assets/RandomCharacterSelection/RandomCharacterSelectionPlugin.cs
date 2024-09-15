using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace RandomCharacterSelection
{
    [BepInDependency("com.KingEnderBrine.ScrollableLobbyUI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.KingEnderBrine.InLobbyConfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.KingEnderBrine.RandomCharacterSelection", "Random Character Selection", "1.5.0")]
    public class RandomCharacterSelectionPlugin : BaseUnityPlugin
    {
        internal static RandomCharacterSelectionPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger { get => Instance?.Logger; }
        internal static bool InLobbyConfigLoaded { get; private set; } 

        private void Start()
        {
            Instance = this;

            InLobbyConfigLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.InLobbyConfig");
            
            AssetBundleHelper.LoadAssetBundle();

            ConfigHelper.InitConfig(Config);

            On.RoR2.UI.CharacterSelectController.Awake += RandomizePanelController.CharacterSelectControllerAwake;
        }
    }
}