using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonTweenHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public float scaleAmount = 1.2f; // Facteur d'échelle pour agrandir le bouton
	public float tweenDuration = 0.2f; // Durée de l'animation Tween

	private RectTransform buttonTransform;
	private Vector3 originalScale;

	void Start()
	{
		buttonTransform = GetComponent<RectTransform>();
		originalScale = buttonTransform.localScale;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		// Redimensionner le bouton lorsqu'on passe dessus
		buttonTransform.DOScale(originalScale * scaleAmount, tweenDuration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		// Rétablir la taille originale lorsque la souris quitte le bouton
		buttonTransform.DOScale(originalScale, tweenDuration);
	}
}
