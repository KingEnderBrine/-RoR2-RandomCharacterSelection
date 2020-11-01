using UnityEngine;

namespace RoR2.UI
{
	public class UIJuice : MonoBehaviour
	{
		public CanvasGroup canvasGroup;
		public RectTransform panningRect;
		public float transitionDuration;
		public float panningMagnitude;
		public bool destroyOnEndOfTransition;
		
		public void TransitionPanFromTop() { }
		public void TransitionAlphaFadeIn() { }
	}
}
