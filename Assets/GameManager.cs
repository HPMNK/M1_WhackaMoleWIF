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
				Debug.Log(selectedHole.name + " spawned");

				yield return new WaitForSeconds(moleLifetime);

				holeAnimator.SetTrigger("despawn");
			}
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		mainCamera = Camera.main; // Assure-toi que la caméra principale est bien tagguée comme "MainCamera"

		for (int i = 0; i < holes.Length; i++)
		{
			holes[i] = GameObject.Find("Hole" + (i + 1));
			if (holes[i] == null)
			{
				Debug.LogError("Hole" + (i + 1) + " not found!");
			}
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

	// Update is called once per frame
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

		// Vérifier les transitions des animators
		CheckAnimatorTransitions();
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
		else
		{
		}
	}

	void TriggerHitAnimation(GameObject hole)
	{
		Animator animator = hole.GetComponent<Animator>();
		if (animator != null)
		{
			Debug.Log(hole.name + " - Animator found and hit trigger set");
			animator.SetTrigger("hit");

			moleHitStatus[animator] = true; // Marque la taupe comme frappée

			score += baseScore;
			UpdateScore();
		}
		else
		{
		}
	}

	void CheckAnimatorTransitions()
	{
		foreach (var hole in holes)
		{
			Animator animator = hole.GetComponent<Animator>();
			if (animator != null)
			{
				AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
				if (currentState.IsName("Empty"))
				{
				
					AnimatorTransitionInfo transitionInfo = animator.GetAnimatorTransitionInfo(0);
					if (transitionInfo.IsName("Despawn -> Empty") && !moleHitStatus[animator])
					{
						Debug.Log("Je passe de despawn à empty " + hole);
						LoseLife();
					}
				}
			}
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
				Debug.Log("Game Over!");
				// Ajoutez ici la logique de fin de jeu
			}
		}
	}
}
