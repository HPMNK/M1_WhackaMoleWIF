using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
	private GameObject[] holes = new GameObject[10]; // Cr�e un tableau de 10 GameObjects
	private Camera mainCamera; // R�f�rence � la cam�ra principale

	public float initialMoleLifetime = 2.0f; // Dur�e de vie initiale des taupes en secondes
	public float initialSpawnIntervalMin = 0.5f;
	public float initialSpawnIntervalMax = 1.5f;
	public float minMoleLifetime = 0.5f; // Dur�e de vie minimale des taupes
	public float minSpawnInterval = 0.1f;
	public float maxAnimationSpeed = 3.0f; // Vitesse maximale des animations
	public int baseScore = 10; // Score de base pour chaque taupe frapp�e
	public int maxScorePerMole = 100; // Score maximal par taupe
	public int lives = 3; // Nombre initial de vies
	public GameObject heartPrefab; // Prefab du c�ur
	public RectTransform uiCanvas; // R�f�rence au canvas UI

	private int score = 0; // Score actuel
	private int molesHit = 0; // Nombre de taupes frapp�es
	private float moleLifetime; // Dur�e de vie actuelle des taupes en secondes
	private float spawnIntervalMin;
	private float spawnIntervalMax;
	private GameObject[] hearts; // Tableaux des GameObjects repr�sentant les c�urs
	private TextMeshProUGUI scoreText; // R�f�rence au TextMeshPro pour le score
	private GameObject heartContainer; // Container pour les c�urs

	private int activeMoles = 0; // Nombre de taupes actives
	private int maxActiveMoles = 1; // Nombre maximal de taupes actives au d�but du jeu

	private Dictionary<Animator, bool> moleHitStatus = new Dictionary<Animator, bool>(); // Pour suivre l'�tat de chaque taupe

	void Awake()
	{
		// Initialiser les valeurs dynamiques
		moleLifetime = initialMoleLifetime;
		spawnIntervalMin = initialSpawnIntervalMin;
		spawnIntervalMax = initialSpawnIntervalMax;

		// Trouver le HeartContainer
		heartContainer = GameObject.Find("HeartContainer");
		if (heartContainer == null)
		{
			Debug.LogError("HeartContainer not found!");
			return;
		}

		// D�truire les enfants du container de vies
		foreach (Transform child in heartContainer.transform)
		{
			Destroy(child.gameObject);
		}

		// Cr�er les c�urs en fonction du nombre de vies
		hearts = new GameObject[lives];
		for (int i = lives - 1; i >= 0; i--)
		{
			GameObject heart = Instantiate(heartPrefab, heartContainer.transform);
			heart.name = "Heart" + (lives - i);
			hearts[i] = heart;
		}
	}

	IEnumerator SpawnMoles()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));

			if (activeMoles < maxActiveMoles)
			{
				int randomHoleIndex = Random.Range(0, holes.Length);
				GameObject selectedHole = holes[randomHoleIndex];

				Animator holeAnimator = selectedHole.GetComponent<Animator>();
				if (holeAnimator != null)
				{
					activeMoles++; // Augmente le nombre de taupes actives
					moleHitStatus[holeAnimator] = false; // Reset hit status
					holeAnimator.SetTrigger("spawn");
					holeAnimator.speed = Mathf.Min(1 + molesHit * 0.05f, maxAnimationSpeed); // Augmenter la vitesse de l'animation

					StartCoroutine(HandleMoleLifeCycle(holeAnimator, selectedHole));
				}
			}
		}
	}

	IEnumerator HandleMoleLifeCycle(Animator holeAnimator, GameObject hole)
	{
		yield return new WaitForSeconds(moleLifetime);

		if (!moleHitStatus[holeAnimator]) // V�rifie si la taupe n'a pas �t� frapp�e
		{
			holeAnimator.SetTrigger("despawn");
			StartCoroutine(CheckDespawn(holeAnimator, hole));
		}
		else
		{
			activeMoles--; // Diminue le nombre de taupes actives si la taupe est frapp�e
		}
	}

	IEnumerator CheckDespawn(Animator animator, GameObject hole)
	{
		// Attendre que l'animation de despawn soit termin�e
		yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));

		if (!moleHitStatus[animator]) // V�rifie si la taupe n'a toujours pas �t� frapp�e
		{
			LoseLife();
		}
		activeMoles--; // Diminue le nombre de taupes actives
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
			scoreText.text = "0";
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

			molesHit++;
			AdjustDifficulty();

			score += CalculateScore();
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

				// Ajout de l'effet de shake screen pour le c�ur
				RectTransform heartRect = hearts[lives].GetComponent<RectTransform>();
				if (heartRect != null)
				{
					heartRect.DOShakeAnchorPos(0.2f, 1f, 25, 90, false, true);
				}
			}

			// Ajout de l'effet de shake screen pour la cam�ra et l'UI
			mainCamera.transform.DOShakePosition(0.5f, 0.5f, 25, 90, false, true);
			if (uiCanvas != null)
			{
				uiCanvas.DOShakeAnchorPos(0.2f, 1f, 10, 90, false, true);
			}

			if (lives == 0)
			{
				Debug.Log("Game Over!");
				SceneManager.LoadScene("GameOverScene"); // Charger la sc�ne "GameOver"

				// Ajoutez ici la logique de fin de jeu
			}
		}
	}

	void AdjustDifficulty()
	{
		// R�duire le temps entre les spawns
		spawnIntervalMin = Mathf.Max(minSpawnInterval, initialSpawnIntervalMin - molesHit * 0.01f);
		spawnIntervalMax = Mathf.Max(minSpawnInterval, initialSpawnIntervalMax - molesHit * 0.01f);

		// R�duire la dur�e de vie des taupes plus lentement
		moleLifetime = Mathf.Max(minMoleLifetime, initialMoleLifetime - molesHit * 0.01f);

		// Augmenter le nombre maximum de taupes actives au fil du temps
		maxActiveMoles = Mathf.Min(4, 2 + molesHit / 20);

		// Mettre � jour la vitesse de l'animation pour chaque taupe
		foreach (var hole in holes)
		{
			Animator holeAnimator = hole.GetComponent<Animator>();
			if (holeAnimator != null)
			{
				holeAnimator.speed = Mathf.Min(1 + molesHit * 0.002f, maxAnimationSpeed);
			}
		}
	}

	int CalculateScore()
	{
		// Formule pour augmenter le score en fonction de la difficult�, avec un maximum
		return Mathf.Min(baseScore + Mathf.FloorToInt(molesHit * 0.5f), maxScorePerMole);
	}
}
