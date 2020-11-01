using RoR2.UI;
using RoR2.UI.SkinControllers;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RandomCharacterSelection
{
    public class RandomizePanelHelpers
    {
        internal static CharacterSelectController cachedCharacterSelectController;
        public static GameObject CachedPrefab { get; private set; }

        internal static void CharacterSelectControllerAwake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, CharacterSelectController self)
        {
            orig(self);

            cachedCharacterSelectController = self;
            
            var leftHandPanel = self.transform.Find("SafeArea/LeftHandPanel (Layer: Main)");

            if (!CachedPrefab)
            {
                CachedPrefab = AssetBundleHelper.MainAssetBundle.LoadAsset<GameObject>("Assets/Resources/RandomizePanel.prefab");

                var bodrderImage = CachedPrefab.transform.Find("BorderImage");
                bodrderImage.GetComponent<Image>().sprite = leftHandPanel.Find("BorderImage").GetComponent<Image>().sprite;
                var survivorChoiseGrid = leftHandPanel.transform.Find("SurvivorChoiceGrid, Panel");
                var baseOutlineSprite = survivorChoiseGrid.Find("SurvivorIconPrefab/BaseOutline").GetComponent<Image>().sprite;
                var hoverOutlineSprite = survivorChoiseGrid.Find("SurvivorIconPrefab/HoverOutline").GetComponent<Image>().sprite;
                var buttonSkin = GameObject.Instantiate(survivorChoiseGrid.Find("WIPClassButtonPrefab").GetComponent<ButtonSkinController>().skinData);
                buttonSkin.buttonStyle.colors.normalColor = Color.white;
                foreach (var button in CachedPrefab.GetComponentsInChildren<HGButton>())
                {
                    button.GetComponent<ButtonSkinController>().skinData = buttonSkin;
                    button.transform.Find("BaseOutline").GetComponent<Image>().sprite = baseOutlineSprite;
                    button.transform.Find("HoverOutline").GetComponent<Image>().sprite = hoverOutlineSprite;
                }
            }

            var randomizePanel = GameObject.Instantiate(CachedPrefab, self.transform.Find("SafeArea"), false);

            var cscRightInputEventOne = self.GetComponents<HGGamepadInputEvent>().First(el => el.actionName == "UISubmenuRight");
            cscRightInputEventOne.requiredTopLayer = leftHandPanel.GetComponent<UILayerKey>();

            var ruleLayoutActive = self.transform.Find("SafeArea/LeftHandPanel (Layer: Main)");
           
            var cscRightInputEventTwo = self.gameObject.AddComponent<HGGamepadInputEvent>();
            cscRightInputEventTwo.actionName = cscRightInputEventOne.actionName;
            cscRightInputEventTwo.actionEvent = cscRightInputEventOne.actionEvent;
            cscRightInputEventTwo.requiredTopLayer = leftHandPanel.Find("SurvivorInfoPanel, Active (Layer: Secondary)").GetComponent<UILayerKey>();
            cscRightInputEventTwo.enabledObjectsIfActive = Array.Empty<GameObject>();

            var cscLeftInputEventOne = self.gameObject.AddComponent<HGGamepadInputEvent>();
            cscLeftInputEventOne.actionName = "UISubmenuLeft";
            cscLeftInputEventOne.actionEvent = randomizePanel.GetComponent<EventHolder>().unityEvent;
            cscLeftInputEventOne.requiredTopLayer = cscRightInputEventOne.requiredTopLayer;
            cscLeftInputEventOne.enabledObjectsIfActive = Array.Empty<GameObject>();
            
            var cscLeftInputEventTwo = self.gameObject.AddComponent<HGGamepadInputEvent>();
            cscLeftInputEventTwo.actionName = cscLeftInputEventOne.actionName;
            cscLeftInputEventTwo.actionEvent = cscLeftInputEventOne.actionEvent;
            cscLeftInputEventTwo.requiredTopLayer = cscRightInputEventTwo.requiredTopLayer;
            cscLeftInputEventTwo.enabledObjectsIfActive = Array.Empty<GameObject>();
        }
    }
}
