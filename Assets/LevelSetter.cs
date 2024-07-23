using UnityEngine;

public class LevelSetter : MonoBehaviour
{
	public AllLevelsData allLevelsData; // Référence au Scriptable Object contenant tous les niveaux
	public GameObject background; // Le GameObject pour le background
	private SpriteRenderer backgroundSpriteRenderer;

	void Start()
	{
		int selectedLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
		LevelData selectedLevel = allLevelsData.levels[selectedLevelIndex];

		if (selectedLevel != null)
		{
			// Changer le sprite du background
			backgroundSpriteRenderer = background.GetComponent<SpriteRenderer>();
			backgroundSpriteRenderer.sprite = selectedLevel.gameSkin;

			// Changer le sprite des trous de taupes
			for (int i = 1; i <= 10; i++)
			{
				GameObject hole = GameObject.Find($"Hole{i}");
				if (hole != null)
				{
					Transform holeSkinTransform = hole.transform.Find("HoleSkin");
					if (holeSkinTransform != null)
					{
						SpriteRenderer holeSkinSpriteRenderer = holeSkinTransform.GetComponent<SpriteRenderer>();
						if (holeSkinSpriteRenderer != null)
						{
							holeSkinSpriteRenderer.sprite = selectedLevel.holeSkin;
						}
					}
				}
			}
		}
		else
		{
			Debug.LogError("Selected level data not found!");
		}
	}
}
