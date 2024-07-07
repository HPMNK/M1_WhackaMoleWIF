using UnityEngine;
using System;
using TMPro;

public class Mole : MonoBehaviour
{
	private Animator animator;
	public bool IsHit { get; private set; }
	public bool IsBadMole { get; private set; }
	public bool IsLifeMole { get; private set; }
	public static event Action OnLoseLife;
	public static event Action OnAddLife;
	public static event Action<int> OnMoleHit;

	public float spawnStartTime;
	public float maxSpawnTime = 2f; // Temps maximal pour réduire le score, basé sur la durée de vie de la taupe

	private TextMeshPro scoreText;
	private GameObject holeScoreText;
	private GameManager gameManager;

	void Awake()
	{
		animator = GetComponent<Animator>();
		holeScoreText = transform.Find("HoleScoreText").gameObject;
		scoreText = holeScoreText.GetComponent<TextMeshPro>();
		holeScoreText.SetActive(false);

		IsHit = false;
		IsBadMole = false;
		IsLifeMole = false;

		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		if (gameManager == null)
		{
			Debug.LogError("GameManager not found in the scene.");
		}
	}

	public void Spawn(RuntimeAnimatorController controller, bool isBadMole, bool isLifeMole)
	{
		animator.runtimeAnimatorController = controller;
		IsHit = false;
		IsBadMole = isBadMole;
		IsLifeMole = isLifeMole;
		animator.SetBool("IsBadMole", isBadMole);
		spawnStartTime = Time.time;
		maxSpawnTime = gameManager.MoleLifetime;
		animator.SetBool("IsLifeMole", isLifeMole);
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Empty"))
		{
			ResetMole();
			animator.SetTrigger("spawn");
		}
	}

	public void Hit()
	{
		if (!IsHit)
		{
			IsHit = true;
			float elapsedTime = (Time.time - spawnStartTime) / maxSpawnTime;
			float scorePercentage = Mathf.Max(0.3f, 1 - elapsedTime); // Assure que le score ne descend pas en dessous de 30%
			if (IsBadMole)
			{
				OnLoseLife?.Invoke();
			}
			else if (IsLifeMole)
			{
				OnAddLife?.Invoke();
			}
			else
			{
				int scoreIncrement = Mathf.CeilToInt(gameManager.CalculateScore() * scorePercentage);
				OnMoleHit?.Invoke(scoreIncrement);
				scoreText.text = $"+{scoreIncrement}";
				holeScoreText.SetActive(true); // Active le texte pour lancer l'animation
			}
			animator.ResetTrigger("despawn");
			animator.SetTrigger("hit");
		}
	}

	public void Despawn()
	{
		if (!IsHit && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			animator.SetTrigger("despawn");
		}
	}

	public void OnDespawnAnimationEnd()
	{
		if (!IsHit && !IsLifeMole) // Pas de perte de vie pour les "life moles"
		{
			OnLoseLife?.Invoke();
		}
		ResetMole();
	}

	private void ResetMole()
	{
		animator.ResetTrigger("spawn");
		animator.ResetTrigger("hit");
		animator.ResetTrigger("despawn");
		animator.Play("Empty");
		IsHit = false;
	}
}
