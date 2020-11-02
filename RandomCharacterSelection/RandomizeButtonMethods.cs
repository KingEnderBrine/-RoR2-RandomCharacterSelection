using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RandomCharacterSelection
{
    public class RandomizeButtonMethods : MonoBehaviour
    {
        private Dictionary<LocalUser, SurvivorIndex> lastSelectedCharacterIndex = new Dictionary<LocalUser, SurvivorIndex>();
        public void RandomizeCharacter()
        {
            if (!PreGameController.instance || !PreGameController.instance.IsCharacterSwitchingCurrentlyAllowed() || !RandomizePanelHelpers.cachedCharacterSelectController)
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
            RandomizePanelHelpers.cachedCharacterSelectController.SelectSurvivor(randomIndex);
            RandomizePanelHelpers.cachedCharacterSelectController.SetSurvivorInfoPanelActive(true);

            localUser.currentNetworkUser?.CallCmdSetBodyPreference(BodyCatalog.FindBodyIndex(SurvivorCatalog.GetSurvivorDef(randomIndex).bodyPrefab));
            lastSelectedCharacterIndex[localUser] = randomIndex;
        }

        public void RandomizeLoadout()
        {
            if (!PreGameController.instance || !RandomizePanelHelpers.cachedCharacterSelectController)
            {
                return;
            }

            var bodyIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(RandomizePanelHelpers.cachedCharacterSelectController.selectedSurvivorIndex);
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
    }
}
