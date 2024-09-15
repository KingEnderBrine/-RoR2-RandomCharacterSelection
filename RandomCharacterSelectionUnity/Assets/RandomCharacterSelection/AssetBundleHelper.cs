using System.IO;
using UnityEngine;

namespace RandomCharacterSelection
{
    public static class AssetBundleHelper
    {
        public static AssetBundle MainAssetBundle { get; private set; }

        internal static void LoadAssetBundle()
        {
            MainAssetBundle = AssetBundle.LoadFromFile(GetBundlePath("kingenderbrine_randomcharacterselection"));
        }

        private static string GetBundlePath(string bundleName)
        {
            return Path.Combine(Path.GetDirectoryName(RandomCharacterSelectionPlugin.Instance.Info.Location), bundleName);
        }
    }
}
