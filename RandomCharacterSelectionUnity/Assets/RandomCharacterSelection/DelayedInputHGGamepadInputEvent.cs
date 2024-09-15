using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2.UI;
using UnityEngine;

namespace RandomCharacterSelection
{
    public class DelayedInputHGGamepadInputEvent : HGGamepadInputEvent
    {
        public new void Update()
        {
            var canAcceptInput = this.CanAcceptInput();
            if (this.couldAcceptInput != canAcceptInput)
            {
                foreach (GameObject gameObject in this.enabledObjectsIfActive)
                {
                    gameObject.SetActive(canAcceptInput);
                }
            }

            if (this.couldAcceptInput && canAcceptInput && this.eventSystem.player.GetButtonDown(this.actionName))
            {
                this.actionEvent.Invoke();
            }

            this.couldAcceptInput = canAcceptInput;
        }
    }
}
