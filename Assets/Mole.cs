using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class Mole : MonoBehaviour
{
	private Animator animator;
	public Material defaultMaterial; // Default material for reset
	private Material currentMaterial; // Current material of the mole
	public bool IsHit { get; private set; }
	public bool IsBadMole { get; private set; }
	public bool IsLifeMole { get; private set; }
	public bool IsBonusMole { get; private set; }
	public static event Action OnLoseLife;
	public static event Action OnAddLife;
	public static event Action<int> OnMoleHit;

	public float spawnStartTime;
	public float maxSpawnTime = 2f; // Temps maximal pour réduire le score, basé sur la durée de vie de la taupe

	private TextMeshPro scoreText;
	private GameObject holeScoreText;
	private GameManager gameManager;

	
	public int scoreMultiplier = 1; // Base score multiplier


	public AudioClip moleHitClip;
	public AudioClip badMoleHitClip;
	public AudioClip lifeMoleHitClip;
	private AudioSource audioSource;

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

		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}
	}
	public void SetProperties(RuntimeAnimatorController controller, Material material, int scoreMultiplier)
	{
		// Récupérer le SpriteRenderer attaché à ce GameObject
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null)
		{
			Debug.LogError("No SpriteRenderer found on the GameObject");
			return;
		}

		// Appliquer le contrôleur d'animation et le matériau
		animator.runtimeAnimatorController = controller;
		spriteRenderer.material = material;
		this.scoreMultiplier = scoreMultiplier;
	}

	public void Spawn(RuntimeAnimatorController controller, bool isBadMole, bool isLifeMole, bool isBonusMole = false)
	{
		animator.runtimeAnimatorController = controller;
		IsHit = false;
		IsBadMole = isBadMole;
		IsLifeMole = isLifeMole;
		IsBonusMole = isBonusMole;

		animator.SetBool("IsBadMole", isBadMole);
		animator.SetBool("IsLifeMole", isLifeMole);
		animator.SetBool("IsBonusMole", isBonusMole);

		spawnStartTime = Time.time;
		maxSpawnTime = gameManager.MoleLifetime;

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
				audioSource.pitch = 1.0f; // Reset pitch to default for this sound

				PlaySound(badMoleHitClip);

				OnLoseLife?.Invoke();
			}
			else if (IsLifeMole)
			{
				audioSource.pitch = 1.0f; // Reset pitch to default for this sound

				PlaySound(lifeMoleHitClip);

				OnAddLife?.Invoke();
			}
			else
			{
				PlaySoundWithRandomPitch(moleHitClip, 0.95f, 1.1f);

				int scoreIncrement = Mathf.CeilToInt(gameManager.CalculateScore() * scorePercentage * scoreMultiplier);
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
		if (!IsHit)
		{
			if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				animator.SetTrigger("despawn");
			}
			else
			{
				// Forcer le despawn si la taupe est bloquée
				StartCoroutine(ForceDespawn());
			}
		}
	}

	private IEnumerator ForceDespawn()
	{
		yield return new WaitForSeconds(0.1f); // Attendre un court instant pour permettre à l'animation de se mettre à jour
		animator.SetTrigger("despawn");
		Debug.Log($"Taupe {gameObject.name} forcée de passer à l'état Despawn.");
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
		GetComponent<SpriteRenderer>().material = defaultMaterial; // Reset to default material
		this.currentMaterial = defaultMaterial;
		this.scoreMultiplier = 1; // Reset the score multiplier
		animator.ResetTrigger("spawn");
		animator.ResetTrigger("hit");
		animator.ResetTrigger("despawn");
		animator.Play("Empty");
		IsHit = false;
	}

	private void PlaySound(AudioClip clip)
	{
		if (audioSource != null && clip != null)
		{
			audioSource.PlayOneShot(clip);
		}
	}

	private void PlaySoundWithRandomPitch(AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f)
	{
		if (audioSource != null && clip != null)
		{
			audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
			audioSource.PlayOneShot(clip);
		}
	}
}
