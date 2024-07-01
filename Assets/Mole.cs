using UnityEngine;
using System;

public class Mole : MonoBehaviour
{
	private Animator animator;
	public bool IsHit { get; private set; }
	public bool IsBadMole { get; private set; }
	public static event Action OnLoseLife;

	void Awake()
	{
		animator = GetComponent<Animator>();
		IsHit = false;
		IsBadMole = false;
	}

	public void Spawn(RuntimeAnimatorController controller, bool isBadMole)
	{
		animator.runtimeAnimatorController = controller;
		IsHit = false;
		IsBadMole = isBadMole;
		animator.SetBool("IsBadMole", isBadMole);
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
				Debug.Log($"Taupe {gameObject.name} (bad mole) frappée, perte de vie.");
			}
			animator.ResetTrigger("despawn");
			animator.SetTrigger("hit");
			Debug.Log($"Taupe {gameObject.name} reçoit le trigger hit.");
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
		if (!IsHit)
		{
			Debug.Log($"Taupe {gameObject.name} n'a pas été frappée, perte de vie.");
			OnLoseLife?.Invoke();
		}
		else
		{
			Debug.Log($"Taupe {gameObject.name} a été frappée, pas de perte de vie.");
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
