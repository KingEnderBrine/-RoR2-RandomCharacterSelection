using System.Reflection;
using UnityEngine;

namespace RandomCharacterSelection
{
    public static class AssetBundleHelper
    {
        public static AssetBundle MainAssetBundle { get; private set; }

        internal static void LoadAssetBundle()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RandomCharacterSelection.kingenderbrine_randomizepanel"))
            {
                MainAssetBundle = AssetBundle.LoadFromStream(stream);
            }
        }

        internal static void UnloadAssetBundle()
        {
            if (!MainAssetBundle)
            {
                return;
            }
            MainAssetBundle.Unload(true);
        }
    }
}
