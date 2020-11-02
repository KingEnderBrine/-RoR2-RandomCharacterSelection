using BepInEx;
using BepInEx.Logging;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace RandomCharacterSelection
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.KingEnderBrine.RandomCharacterSelection", "Random Character Selection", "1.0.1")]
    public class RandomCharacterSelectionPlugin : BaseUnityPlugin
    {
        internal static RandomCharacterSelectionPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger { get => Instance?.Logger; }

        private void Awake()
        {
            Instance = this;

            AssetBundleHelper.LoadAssetBundle();
            On.RoR2.UI.CharacterSelectController.Awake += RandomizePanelHelpers.CharacterSelectControllerAwake;
        }

        private void Destroy()
        {
            Instance = null;

            AssetBundleHelper.UnloadAssetBundle();
            On.RoR2.UI.CharacterSelectController.Awake -= RandomizePanelHelpers.CharacterSelectControllerAwake;
        }
    }
}