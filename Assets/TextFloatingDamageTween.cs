using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextFloatingDamageTween : MonoBehaviour
{
	private TextMeshPro scoreText;

	void Awake()
	{
		scoreText = GetComponent<TextMeshPro>();
	}

	void OnEnable()
	{
		scoreText.alpha = 1;
		scoreText.rectTransform.localPosition = Vector3.zero;

		// Animation de disparition et de montée
		scoreText.DOFade(0, 0.8f).SetEase(Ease.InOutQuad);
		scoreText.rectTransform.DOAnchorPosY(0.5f, 1f).SetEase(Ease.InOutQuad).OnComplete(() => gameObject.SetActive(false));
	}
}
