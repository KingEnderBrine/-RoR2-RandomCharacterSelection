using UnityEngine;
using UnityEngine.Events;

namespace RoR2.UI
{
	public class HGGamepadInputEvent : MonoBehaviour
	{
		public string actionName;
		public UnityEvent actionEvent;
		public UILayerKey requiredTopLayer;
		public GameObject[] enabledObjectsIfActive;

		public void Update() {}
	}
}
