using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
	private GameObject[] holes = new GameObject[10]; // Cr�e un tableau de 10 GameObjects
	private Camera mainCamera; // R�f�rence � la cam�ra principale

	public float moleLifetime = 2.0f; // Dur�e de vie initiale des taupes en secondes
	public float difficultyIncreaseRate = 0.1f; // R�duction de la dur�e de vie par cycle en secondes
	public float minMoleLifetime = 0.5f; // Dur�e de vie minimale des taupes
	public int baseScore = 10; // Score de base pour chaque taupe frapp�e

	private int score = 0; // Score actuel
	private int lives = 3; // Nombre initial de vies
	private GameObject[] hearts; // Tableaux des GameObjects repr�sentant les coeurs
	private TextMeshProUGUI scoreText; // R�f�rence au TextMeshPro pour le score

	private Dictionary<Animator, bool> moleHitStatus = new Dictionary<Animator, bool>(); // Pour suivre l'�tat de chaque taupe

	IEnumerator SpawnMoles()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

			int randomHoleIndex = Random.Range(0, holes.Length);
			GameObject selectedHole = holes[randomHoleIndex];

			Animator holeAnimator = selectedHole.GetComponent<Animator>();
			if (holeAnimator != null)
			{
				moleHitStatus[holeAnimator] = false; // Reset hit status
				holeAnimator.SetTrigger("spawn");

				yield return new WaitForSeconds(moleLifetime);

				if (!moleHitStatus[holeAnimator]) // V�rifie si la taupe n'a pas �t� frapp�e
				{
					holeAnimator.SetTrigger("despawn");
					StartCoroutine(CheckDespawn(holeAnimator, selectedHole));
				}
			}
		}
	}

	// Coroutine pour v�rifier si la taupe a atteint l'�tat "Empty"
	IEnumerator CheckDespawn(Animator animator, GameObject hole)
	{
		// Attendre que l'animation de despawn soit termin�e
		yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));

		if (!moleHitStatus[animator]) // V�rifie si la taupe n'a toujours pas �t� frapp�e
		{
			LoseLife();
		}
	}

	// Start is called avant la premi�re frame update
	void Start()
	{
		mainCamera = Camera.main; // Assure-toi que la cam�ra principale est bien taggu�e comme "MainCamera"

		for (int i = 0; i < holes.Length; i++)
		{
			holes[i] = GameObject.Find("Hole" + (i + 1));
		}

		hearts = new GameObject[3];
		for (int i = 0; i < 3; i++)
		{
			hearts[i] = GameObject.Find("Heart" + (i + 1));
			if (hearts[i] == null)
			{
				Debug.LogError("Heart" + (i + 1) + " not found!");
			}
		}

		// Trouver le TextMeshPro pour le score
		GameObject scoreTextObject = GameObject.Find("ScoreText");
		if (scoreTextObject != null)
		{
			scoreText = scoreTextObject.GetComponent<TextMeshProUGUI>();
			scoreText.text = "Score: 0";
		}

		StartCoroutine(SpawnMoles());
	}

	// Update est appel�e une fois par frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			CheckHit(Input.mousePosition);
		}

		// G�rer les taps sur �cran tactile
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
		{
			CheckHit(Input.GetTouch(0).position);
		}
	}

	void CheckHit(Vector2 screenPosition)
	{
		RaycastHit2D hit;
		Ray ray = mainCamera.ScreenPointToRay(screenPosition);

		hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

		if (hit.collider != null)
		{
			GameObject clickedGameObject = hit.collider.gameObject;

			if (clickedGameObject != null)
			{
				for (int i = 0; i < holes.Length; i++)
				{
					if (clickedGameObject == holes[i])
					{
						TriggerHitAnimation(clickedGameObject);
						break;
					}
				}
			}
		}
	}

	void TriggerHitAnimation(GameObject hole)
	{
		Animator animator = hole.GetComponent<Animator>();
		if (animator != null)
		{
			animator.SetTrigger("hit");

			moleHitStatus[animator] = true; // Marque la taupe comme frapp�e

			score += baseScore;
			UpdateScore();
		}
	}

	void UpdateScore()
	{
		if (scoreText != null)
		{
			scoreText.text = "Score: " + score;
		}
	}

	public void LoseLife()
	{
		if (lives > 0)
		{
			lives--;
			if (hearts[lives] != null)
			{
				Destroy(hearts[lives]);
			}

			if (lives == 0)
			{
				// Ajoutez ici la logique de fin de jeu
			}
		}
	}
}
