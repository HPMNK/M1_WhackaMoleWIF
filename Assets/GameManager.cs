using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
	private GameObject[] holes = new GameObject[10]; // Crée un tableau de 10 GameObjects
	private Camera mainCamera; // Référence à la caméra principale

	public float moleLifetime = 2.0f; // Durée de vie initiale des taupes en secondes
	public float difficultyIncreaseRate = 0.1f; // Réduction de la durée de vie par cycle en secondes
	public float minMoleLifetime = 0.5f; // Durée de vie minimale des taupes
	public int baseScore = 10; // Score de base pour chaque taupe frappée

	private int score = 0; // Score actuel
	private int lives = 3; // Nombre initial de vies
	private GameObject[] hearts; // Tableaux des GameObjects représentant les coeurs
	private TextMeshProUGUI scoreText; // Référence au TextMeshPro pour le score

	private Dictionary<Animator, bool> moleHitStatus = new Dictionary<Animator, bool>(); // Pour suivre l'état de chaque taupe

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

				if (!moleHitStatus[holeAnimator]) // Vérifie si la taupe n'a pas été frappée
				{
					holeAnimator.SetTrigger("despawn");
					StartCoroutine(CheckDespawn(holeAnimator, selectedHole));
				}
			}
		}
	}

	// Coroutine pour vérifier si la taupe a atteint l'état "Empty"
	IEnumerator CheckDespawn(Animator animator, GameObject hole)
	{
		// Attendre que l'animation de despawn soit terminée
		yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));

		if (!moleHitStatus[animator]) // Vérifie si la taupe n'a toujours pas été frappée
		{
			LoseLife();
		}
	}

	// Start is called avant la première frame update
	void Start()
	{
		mainCamera = Camera.main; // Assure-toi que la caméra principale est bien tagguée comme "MainCamera"

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

	// Update est appelée une fois par frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			CheckHit(Input.mousePosition);
		}

		// Gérer les taps sur écran tactile
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

			moleHitStatus[animator] = true; // Marque la taupe comme frappée

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
