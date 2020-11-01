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

            var loadout = new Loadout();
            for (var i = 0; i < bodySkills.Length; i++)
            {
                var skill = bodySkills[i];
                loadout.bodyLoadoutManager.SetSkillVariant(bodyIndex, i, (uint)UnityEngine.Random.Range(0, skill.skillFamily.variants.Length));
            }
            loadout.bodyLoadoutManager.SetSkinIndex(bodyIndex, (uint)UnityEngine.Random.Range(0, bodySkins.Length));

            var localUser = ((MPEventSystem)EventSystem.current).localUser;
            localUser.userProfile.SetLoadout(loadout);
        }
    }
}
