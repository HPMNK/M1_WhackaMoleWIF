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
	public int lives = 3; // Nombre initial de vies
	public GameObject heartPrefab; // Prefab du c�ur

	private int score = 0; // Score actuel
	private GameObject[] hearts; // Tableaux des GameObjects repr�sentant les c�urs
	private TextMeshProUGUI scoreText; // R�f�rence au TextMeshPro pour le score
	private GameObject lifeContainer; // Container pour les c�urs

	private Dictionary<Animator, bool> moleHitStatus = new Dictionary<Animator, bool>(); // Pour suivre l'�tat de chaque taupe

	void Awake()
	{
		// Trouver le LifeContainer
		lifeContainer = GameObject.Find("HeartContainer");
		if (lifeContainer == null)
		{
			Debug.LogError("HeartContainer not found!");
			return;
		}

		// D�truire les enfants du container de vies
		foreach (Transform child in lifeContainer.transform)
		{
			Destroy(child.gameObject);
		}

		// Cr�er les c�urs en fonction du nombre de vies
		hearts = new GameObject[lives];
		for (int i = lives - 1; i >= 0; i--)
		{
			GameObject heart = Instantiate(heartPrefab, lifeContainer.transform);
			heart.name = "Heart" + (lives - i);
			hearts[i] = heart;
		}
	}

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

	// Start est appel�e avant la premi�re frame update
	void Start()
	{
		mainCamera = Camera.main; // Assure-toi que la cam�ra principale est bien taggu�e comme "MainCamera"

		for (int i = 0; i < holes.Length; i++)
		{
			holes[i] = GameObject.Find("Hole" + (i + 1));
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
			scoreText.text = score.ToString();
		}
	}

	public void LoseLife()
	{
		if (lives > 0)
		{
			lives--;
			Animator heartAnimator = hearts[lives].GetComponent<Animator>();
			if (heartAnimator != null)
			{
				heartAnimator.SetBool("isEmpty", true);
			}

			if (lives == 0)
			{
				Debug.Log("Game Over!");
				// Ajoutez ici la logique de fin de jeu
			}
		}
	}
}
