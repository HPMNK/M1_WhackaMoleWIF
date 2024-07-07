using UnityEngine;
using System;

public class Mole : MonoBehaviour
{
	private Animator animator;
	public bool IsHit { get; private set; }
	public bool IsBadMole { get; private set; }
	public bool IsLifeMole { get; private set; }
	public static event Action OnLoseLife;
	public static event Action OnAddLife;
	public static event Action<float> OnMoleHit;


	public float spawnStartTime;
	public float maxSpawnTime = 2f; // Temps maximal pour r�duire le score, bas� sur la dur�e de vie de la taupe


	void Awake()
	{
		animator = GetComponent<Animator>();
		IsHit = false;
		IsBadMole = false;
		IsLifeMole = false;
	}


	
	public void Spawn(RuntimeAnimatorController controller, bool isBadMole, bool isLifeMole)
	{
		animator.runtimeAnimatorController = controller;
		IsHit = false;
		IsBadMole = isBadMole;
		IsLifeMole = isLifeMole;
		animator.SetBool("IsBadMole", isBadMole);
		spawnStartTime = Time.time;
		maxSpawnTime = GameManager.Instance.MoleLifetime;
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
				Debug.Log($"Taupe {gameObject.name} (bad mole) frapp�e, perte de vie.");
			}
			if (IsLifeMole)
			{
				OnAddLife?.Invoke();
				Debug.Log($"Taupe {gameObject.name} (life mole) frapp�e, gain de vie.");
			}
			else
			{
				OnMoleHit?.Invoke(scorePercentage);
				Debug.Log($"Taupe {gameObject.name} frapp�e avec un score de {scorePercentage * 100}%.");
			}
			animator.ResetTrigger("despawn");
			animator.SetTrigger("hit");
			Debug.Log($"Taupe {gameObject.name} re�oit le trigger hit.");
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
			Debug.Log($"Taupe {gameObject.name} n'a pas �t� frapp�e, perte de vie.");
			OnLoseLife?.Invoke();
		}
		else
		{
			Debug.Log($"Taupe {gameObject.name} a �t� frapp�e, pas de perte de vie.");
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
