using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageTween : MonoBehaviour
{
	public float floatAmount = 30f; // L'amplitude du mouvement de flottement
	public float floatDuration = 1.2f; // La durée du mouvement de flottement

	private RectTransform rectTransform;

	void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		StartFloating();
	}

	void StartFloating()
	{
		// Animer la position Y de l'image pour créer un effet de flottement
		rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + floatAmount, floatDuration)
			.SetLoops(-1, LoopType.Yoyo)
			.SetEase(Ease.InOutSine);
	}
}

