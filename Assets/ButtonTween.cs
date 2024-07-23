using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonTween : MonoBehaviour, IPointerClickHandler
{
	public float scaleAmount = 1.2f; // Facteur d'échelle pour agrandir le bouton
	public float tweenDuration = 0.2f; // Durée de l'animation Tween

	private RectTransform buttonTransform;
	private Vector3 originalScale;
	private Button button;

	void Start()
	{
		buttonTransform = GetComponent<RectTransform>();
		originalScale = buttonTransform.localScale;
		button = GetComponent<Button>();

		if (button == null)
		{
			Debug.LogError("Button component not found!");
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("Button clicked");

		if (button == null)
		{
			Debug.LogError("Button component not found!");
			return;
		}

		// Arrêter toutes les animations en cours pour ce transform
		buttonTransform.DOKill();

		// Créer une séquence d'animations
		Sequence clickSequence = DOTween.Sequence();
		clickSequence.Append(buttonTransform.DOScale(originalScale * scaleAmount, tweenDuration).SetEase(Ease.OutQuad))
					 .Append(buttonTransform.DOScale(originalScale, tweenDuration).SetEase(Ease.OutQuad))
					 .OnComplete(() => {
						 // Optionnel: ajouter des actions supplémentaires après l'animation
						 Debug.Log("Animation complete");
					 });
	}
}
