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
        private readonly Dictionary<LocalUser, SurvivorIndex> lastSelectedCharacterIndex = new Dictionary<LocalUser, SurvivorIndex>();
        public static GameObject CachedPrefab { get; private set; }

        public GameObject randomizeCharacterButton;
        public GameObject randomizeLoadoutButton;

        private CharacterSelectController characterSelectController;
        private EclipseRunScreenController eclipseRunScreenController;
        private Component scrollableLobbyUIController;

        private bool isEclipseRun => PreGameController.instance && PreGameController.instance.gameModeIndex == GameModeCatalog.FindGameModeIndex("EclipseRun");

        public void Start()
        {
            characterSelectController = GetComponentInParent<CharacterSelectController>();
            eclipseRunScreenController = GetComponentInParent<EclipseRunScreenController>();
            
            StartCoroutine(StartDelayedCoroutine());
        }

        private IEnumerator StartDelayedCoroutine()
        {
            yield return new WaitForSeconds(0.1F);

            if (isEclipseRun)
            {
                randomizeCharacterButton.SetActive(false);
            }
            if (eclipseRunScreenController)
            {
                randomizeLoadoutButton.SetActive(false);
            }
            if (RandomCharacterSelectionPlugin.ScrollableLobbyUILoaded)
            {
                GetScrollableLobbyUIController();
            }
        }

        public void RandomizeCharacter()
        {
            if ((!PreGameController.instance || !PreGameController.instance.IsCharacterSwitchingCurrentlyAllowed() || !characterSelectController) && !eclipseRunScreenController)
            {
                return;
            }

            var localUser = ((MPEventSystem)EventSystem.current).localUser;
            if (!lastSelectedCharacterIndex.TryGetValue(localUser, out var lastIndex))
            {
                lastIndex = SurvivorIndex.Commando;
            }
            var survivors = SurvivorCatalog.idealSurvivorOrder.Where(survivorIndex => SurvivorCatalog.SurvivorIsUnlockedOnThisClient(survivorIndex) && lastIndex != survivorIndex);
            var randomIndex = survivors.ElementAt(UnityEngine.Random.Range(0, survivors.Count()));
            
            if (characterSelectController)
            {
                characterSelectController.SelectSurvivor(randomIndex);
                characterSelectController.SetSurvivorInfoPanelActive(true);
            }
            if (eclipseRunScreenController)
            {
                eclipseRunScreenController.SelectSurvivor(randomIndex);
            }
            if (RandomCharacterSelectionPlugin.ScrollableLobbyUILoaded)
            {
                ScrollableLobbyUISelectCharacter(randomIndex);
            }
            localUser.currentNetworkUser?.CallCmdSetBodyPreference(BodyCatalog.FindBodyIndex(SurvivorCatalog.GetSurvivorDef(randomIndex).bodyPrefab));
            lastSelectedCharacterIndex[localUser] = randomIndex;
        }

        public void RandomizeLoadout()
        {
            if (!PreGameController.instance || !characterSelectController)
            {
                return;
            }

            var bodyIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(characterSelectController.selectedSurvivorIndex);
            var bodySkills = BodyCatalog.GetBodyPrefabSkillSlots(bodyIndex);
            var bodySkins = BodyCatalog.GetBodySkins(bodyIndex);

            var localUser = ((MPEventSystem)EventSystem.current).localUser;

            var loadout = new Loadout();
            for (var i = 0; i < bodySkills.Length; i++)
            {
                var skill = bodySkills[i];
                var unlockedVariants = new List<uint>();
                for (uint j = 0; j < skill.skillFamily.variants.Length; j++)
                {
                    if (localUser.userProfile.HasUnlockable(skill.skillFamily.variants[j].unlockableName))
                    {
                        unlockedVariants.Add(j);
                    }
                }

                loadout.bodyLoadoutManager.SetSkillVariant(bodyIndex, i, unlockedVariants[UnityEngine.Random.Range(0, unlockedVariants.Count)]);
            }

            var unlockedSkins = new List<uint>();
            for (uint j = 0; j < bodySkins.Length; j++)
            {
                if (localUser.userProfile.HasUnlockable(bodySkins[j].unlockableName))
                {
                    unlockedSkins.Add(j);
                }
            }

            loadout.bodyLoadoutManager.SetSkinIndex(bodyIndex, unlockedSkins[UnityEngine.Random.Range(0, unlockedSkins.Count)]);

            localUser.userProfile.SetLoadout(loadout);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void GetScrollableLobbyUIController()
        {
            if (characterSelectController)
            {
                scrollableLobbyUIController = characterSelectController.GetComponentInChildren<ScrollableLobbyUI.CharacterSelectBarControllerReplacement>();
            }
            if (eclipseRunScreenController)
            {
                scrollableLobbyUIController = eclipseRunScreenController.GetComponentInChildren<ScrollableLobbyUI.CharacterSelectBarControllerReplacement>();
            }
            Debug.Log(scrollableLobbyUIController);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void ScrollableLobbyUISelectCharacter(SurvivorIndex survivorIndex)
        {
            if (!(scrollableLobbyUIController is ScrollableLobbyUI.CharacterSelectBarControllerReplacement barReplacement))
            {
                return;
            }
            barReplacement.OpenPageWithCharacter(survivorIndex);
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

            var cscRightInputEventOne = self.GetComponents<HGGamepadInputEvent>().First(el => el.actionName == "UISubmenuRight");
            cscRightInputEventOne.requiredTopLayer = leftHandPanel.GetComponent<UILayerKey>();

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

        internal static void EclipseRunScreenControllerStart(On.RoR2.UI.EclipseRunScreenController.orig_Start orig, EclipseRunScreenController self)
        {
            orig(self);

            var rightPanel = self.transform.Find("Main Panel/RightPanel");

            if (!CachedPrefab)
            {
                CachePrefabFromSurvivorGrid(rightPanel, "HeaderContainer");
            }

            var randomizePanel = GameObject.Instantiate(CachedPrefab, self.transform.Find("Main Panel"), false);

            randomizePanel.GetComponents<HGGamepadInputEvent>().First(inputEvent => inputEvent.actionName == "UISubmitAlt").enabled = true;

            var rectTransform = randomizePanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = new Vector2(-672, -130);

            CloneGlyphInEclipse(self);
        }

        private static void CloneGlyphInEclipse(EclipseRunScreenController self)
        {
            var submenu = self.transform.Find("Main Panel/SubmenuLegend");
            var glyph = GameObject.Instantiate(submenu.Find("GenericGlyphAndDescription"), submenu);
            glyph.SetAsFirstSibling();
            glyph.transform.Find("Text").GetComponent<InputBindingDisplayController>().actionName = "UISubmitAlt";
            glyph.transform.Find("Description").GetComponent<LanguageTextMeshController>().token = RandomCharacterSelectionPlugin.RANDOMIZE_CHARACTER_BUTTON;
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
