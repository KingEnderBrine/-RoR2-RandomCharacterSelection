using RoR2;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RandomCharacterSelection
{
    public class RandomizePanelController : MonoBehaviour
    {
        public static GameObject CachedPrefab { get; private set; }

        public GameObject randomizeCharacterButton;
        public GameObject randomizeLoadoutButton;

        private CharacterSelectController characterSelectController;
        private CharacterSelectBarController characterSelectBarController;

        private bool isEclipseRun => PreGameController.instance && PreGameController.instance.gameModeIndex == GameModeCatalog.FindGameModeIndex("EclipseRun");

        public void Start()
        {
            characterSelectController = GetComponentInParent<CharacterSelectController>();
            characterSelectBarController = characterSelectController.GetComponentInChildren<CharacterSelectBarController>();
        }

        public void RandomizeCharacter()
        {
            if (!PreGameController.instance || !PreGameController.instance.IsCharacterSwitchingCurrentlyAllowed() || !characterSelectController)
            {
                return;
            }

            var localUser = ((MPEventSystem)EventSystem.current).localUser;
            var currentSurvivor = characterSelectController.currentSurvivorDef;
            var canSelectSameCharacter = ConfigHelper.CanSelectSameCharacter.Value;
            var survivors = SurvivorCatalog.orderedSurvivorDefs.Where(survivorDef => (canSelectSameCharacter || currentSurvivor != survivorDef) && !survivorDef.hidden && SurvivorCatalog.SurvivorIsUnlockedOnThisClient(survivorDef.survivorIndex) && survivorDef.CheckRequiredExpansionEnabled() && survivorDef.CheckUserHasRequiredEntitlement(localUser));
            var randomSurvivor = survivors.ElementAt(UnityEngine.Random.Range(0, survivors.Count()));
            if (characterSelectController)
            {
                characterSelectBarController.PickIconBySurvivorDef(randomSurvivor);
                characterSelectController.SetSurvivorInfoPanelActive(true);
            }
        }

        public void RandomizeLoadout()
        {
            if (!PreGameController.instance || !characterSelectController)
            {
                return;
            }

            var bodyIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(characterSelectController.currentSurvivorDef.survivorIndex);
            var bodySkills = BodyCatalog.GetBodyPrefabSkillSlots(bodyIndex);
            var bodySkins = BodyCatalog.GetBodySkins(bodyIndex);

            var localUser = ((MPEventSystem)EventSystem.current).localUser;

            var loadout = new Loadout();
            localUser.userProfile.CopyLoadout(loadout);

            for (var i = 0; i < bodySkills.Length; i++)
            {
                var skill = bodySkills[i];
                var unlockedVariants = new List<uint>();
                for (uint j = 0; j < skill.skillFamily.variants.Length; j++)
                {
                    if (localUser.userProfile.HasUnlockable(skill.skillFamily.variants[j].unlockableDef))
                    {
                        unlockedVariants.Add(j);
                    }
                }

                loadout.bodyLoadoutManager.SetSkillVariant(bodyIndex, i, unlockedVariants[UnityEngine.Random.Range(0, unlockedVariants.Count)]);
            }

            var unlockedSkins = new List<uint>();
            for (uint j = 0; j < bodySkins.Length; j++)
            {
                if (localUser.userProfile.HasUnlockable(bodySkins[j].unlockableDef))
                {
                    unlockedSkins.Add(j);
                }
            }

            loadout.bodyLoadoutManager.SetSkinIndex(bodyIndex, unlockedSkins[UnityEngine.Random.Range(0, unlockedSkins.Count)]);

            localUser.userProfile.SetLoadout(loadout);
        }

        internal static void CharacterSelectControllerAwake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, CharacterSelectController self)
        {
            orig(self);

            var leftHandPanel = self.transform.Find("SafeArea/LeftHandPanel (Layer: Main)");

            if (!CachedPrefab)
            {
                CachePrefabFromSurvivorGrid(leftHandPanel, "SurvivorChoiceGrid, Panel");
            }

            var randomizePanel = GameObject.Instantiate(CachedPrefab, self.transform.Find("SafeArea"), false);

            HGGamepadInputEvent cscRightInputEventOne;
            HGGamepadInputEvent cscRightInputEventTwo;

            var cscInputEvents = self.GetComponents<HGGamepadInputEvent>();
            if (RandomCharacterSelectionPlugin.InLobbyConfigLoaded)
            {
                var rightInputs = cscInputEvents.Where(el => el.actionName == "UISubmenuRight");
                cscRightInputEventOne = rightInputs.ElementAt(0);
                cscRightInputEventTwo = rightInputs.ElementAt(1);
            }
            else
            {
                cscRightInputEventOne = cscInputEvents.First(el => el.actionName == "UISubmenuRight");
                cscRightInputEventOne.requiredTopLayer = leftHandPanel.GetComponent<UILayerKey>();

                cscRightInputEventTwo = self.gameObject.AddComponent<HGGamepadInputEvent>();
                cscRightInputEventTwo.actionName = cscRightInputEventOne.actionName;
                cscRightInputEventTwo.actionEvent = cscRightInputEventOne.actionEvent;
                cscRightInputEventTwo.requiredTopLayer = leftHandPanel.Find("SurvivorInfoPanel, Active (Layer: Secondary)").GetComponent<UILayerKey>();
                cscRightInputEventTwo.enabledObjectsIfActive = Array.Empty<GameObject>();
            }

            var randomizePanelRightInputEvent = randomizePanel.GetComponents<HGGamepadInputEvent>().First(input => input.actionName == "UISubmenuRight");

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

            var randomizePanelCancelInputEvent = randomizePanel.AddComponent<HGGamepadInputEvent>();
            randomizePanelCancelInputEvent.actionName = "UICancel";
            randomizePanelCancelInputEvent.actionEvent = randomizePanelRightInputEvent.actionEvent;
            randomizePanelCancelInputEvent.requiredTopLayer = randomizePanelRightInputEvent.requiredTopLayer;
            randomizePanelCancelInputEvent.enabledObjectsIfActive = randomizePanelRightInputEvent.enabledObjectsIfActive;
        }

        private static void CachePrefabFromSurvivorGrid(Transform panel, string survivorGridName)
        {
            if (CachedPrefab)
            {
                return;
            }
            var survivorGrid = panel.Find(survivorGridName);
            CachedPrefab = AssetBundleHelper.MainAssetBundle.LoadAsset<GameObject>("Assets/Resources/RandomizePanel.prefab");

            CachedPrefab.GetComponent<Image>().sprite = panel.Find("BorderImage").GetComponent<Image>().sprite;
            var baseOutlineSprite = survivorGrid.Find("SurvivorIconPrefab/BaseOutline").GetComponent<Image>().sprite;
            var hoverOutlineSprite = survivorGrid.Find("SurvivorIconPrefab/HoverOutline").GetComponent<Image>().sprite;
            var buttonSkin = GameObject.Instantiate(survivorGrid.Find("WIPClassButtonPrefab").GetComponent<ButtonSkinController>().skinData);
            buttonSkin.buttonStyle.colors.normalColor = Color.white;
            foreach (var button in CachedPrefab.GetComponentsInChildren<HGButton>())
            {
                button.GetComponent<ButtonSkinController>().skinData = buttonSkin;
                button.transform.Find("BaseOutline").GetComponent<Image>().sprite = baseOutlineSprite;
                button.transform.Find("HoverOutline").GetComponent<Image>().sprite = hoverOutlineSprite;
            }
        }
    }
}
