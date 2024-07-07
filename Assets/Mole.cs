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
