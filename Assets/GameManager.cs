using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	private GameObject[] holes = new GameObject[10]; // Crée un tableau de 10 GameObjects
	private Camera mainCamera; // Référence à la caméra principale

	public float initialMoleLifetime = 2.0f; // Durée de vie initiale des taupes en secondes
	public float initialSpawnIntervalMin = 0.5f;
	public float initialSpawnIntervalMax = 1.5f;
	public float minMoleLifetime = 0.5f; // Durée de vie minimale des taupes
	public float minSpawnInterval = 0.1f;
	public float maxAnimationSpeed = 3.0f; // Vitesse maximale des animations
	public int baseScore = 10; // Score de base pour chaque taupe frappée
	public int maxScorePerMole = 100; // Score maximal par taupe
	public int lives = 3; // Nombre initial de vies
	public GameObject heartPrefab; // Prefab du cœur
	public RectTransform uiCanvas; // Référence au canvas UI
	public float badMoleChance = 20f; // Pourcentage de chance d'avoir une "bad mole"
	public RuntimeAnimatorController normalMoleController; // Animator Controller pour les taupes normales
	public RuntimeAnimatorController badMoleController; // Animator Controller pour les "bad moles"

	private int score = 0; // Score actuel
	private int molesHit = 0; // Nombre de taupes frappées
	private float moleLifetime; // Durée de vie actuelle des taupes en secondes
	private float spawnIntervalMin;
	private float spawnIntervalMax;
	private GameObject[] hearts; // Tableaux des GameObjects représentant les cœurs
	private TextMeshProUGUI scoreText; // Référence au TextMeshPro pour le score
	private GameObject heartContainer; // Container pour les cœurs

	private int activeMoles = 0; // Nombre de taupes actives
	private int maxActiveMoles = 2; // Nombre maximal de taupes actives au début du jeu

	bool isPaused = false;

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

		// Détruire les enfants du container de vies
		foreach (Transform child in heartContainer.transform)
		{
			Destroy(child.gameObject);
		}

		// Créer les cœurs en fonction du nombre de vies
		hearts = new GameObject[lives];
		for (int i = lives - 1; i >= 0; i--)
		{
			GameObject heart = Instantiate(heartPrefab, heartContainer.transform);
			heart.name = "Heart" + (lives - i);
			hearts[i] = heart;
		}

		Mole.OnLoseLife += LoseLife;
	}

	void OnDestroy()
	{
		Mole.OnLoseLife -= LoseLife;
	}

	void Start()
	{
		PlayerPrefs.SetInt("FinalScore", 0);
		PlayerPrefs.Save();

		mainCamera = Camera.main; // Assure-toi que la caméra principale est bien tagguée comme "MainCamera"

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

	IEnumerator SpawnMoles()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));

			if (activeMoles < maxActiveMoles)
			{
				int randomHoleIndex = Random.Range(0, holes.Length);
				GameObject selectedHole = holes[randomHoleIndex];

				Mole mole = selectedHole.GetComponent<Mole>();
				if (mole != null && mole.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Empty"))
				{
					bool isBadMole = Random.Range(0f, 100f) <= badMoleChance;
					RuntimeAnimatorController controller = isBadMole ? badMoleController : normalMoleController;
					activeMoles++; // Augmente le nombre de taupes actives
					mole.Spawn(controller, isBadMole);
					StartCoroutine(HandleMoleLifeCycle(mole));
				}
			}
		}
	}

	IEnumerator HandleMoleLifeCycle(Mole mole)
	{
		yield return new WaitForSeconds(moleLifetime);

		if (!mole.IsHit) // Vérifie si la taupe n'a pas été frappée
		{
			mole.Despawn();
		}

		yield return new WaitUntil(() => mole.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Empty"));

		activeMoles--; // Diminue le nombre de taupes actives après la fin du cycle de vie
	}

	private IEnumerator HandleGameOver()
	{
		yield return new WaitForSeconds(0.8f);
		PlayerPrefs.SetInt("FinalScore", score);
		PlayerPrefs.Save();
		SceneManager.LoadScene("GameOverScene"); // Charger la scène "GameOver"
	}

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

		if (Input.GetKeyDown(KeyCode.Space))
		{
			isPaused = !isPaused; // Inverse l'état de la pause
			Time.timeScale = isPaused ? 0 : 1; // Met le timeScale à 0 quand c'est en pause
		}
	}

	void CheckHit(Vector2 screenPosition)
	{
		Vector2 worldPoint = mainCamera.ScreenToWorldPoint(screenPosition);
		Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint);

		if (hitCollider != null)
		{
			GameObject clickedGameObject = hitCollider.gameObject;

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
		Mole mole = hole.GetComponent<Mole>();
		if (mole != null && !mole.IsHit)
		{
			mole.Hit();

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

				// Ajout de l'effet de shake screen pour le cœur
				RectTransform heartRect = hearts[lives].GetComponent<RectTransform>();
				if (heartRect != null)
				{
					heartRect.DOShakeAnchorPos(0.2f, 1f, 25, 90, false, true);
				}
			}

			// Ajout de l'effet de shake screen pour la caméra et l'UI
			mainCamera.transform.DOShakePosition(0.5f, 0.5f, 25, 90, false, true);
			if (uiCanvas != null)
			{
				uiCanvas.DOShakeAnchorPos(0.2f, 1f, 10, 90, false, true);
			}

			if (lives == 0)
			{
				StartCoroutine(HandleGameOver());

			}
		}
	}

	void AdjustDifficulty()
	{
		// Réduire le temps entre les spawns
		spawnIntervalMin = Mathf.Max(minSpawnInterval, initialSpawnIntervalMin - molesHit * 0.01f);
		spawnIntervalMax = Mathf.Max(minSpawnInterval, initialSpawnIntervalMax - molesHit * 0.01f);

		// Réduire la durée de vie des taupes plus lentement
		moleLifetime = Mathf.Max(minMoleLifetime, initialMoleLifetime - molesHit * 0.01f);

		// Augmenter le nombre maximum de taupes actives au fil du temps
		maxActiveMoles = Mathf.Min(4, 2 + molesHit / 20);

		// Mettre à jour la vitesse de l'animation pour chaque taupe
		foreach (var hole in holes)
		{
			Mole mole = hole.GetComponent<Mole>();
			if (mole != null)
			{
				mole.GetComponent<Animator>().speed = Mathf.Min(1 + molesHit * 0.005f, maxAnimationSpeed);
			}
		}
	}

	int CalculateScore()
	{
		// Formule pour augmenter le score en fonction de la difficulté, avec un maximum
		return Mathf.Min(baseScore + Mathf.FloorToInt(molesHit * 0.5f), maxScorePerMole);
	}
}
