using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LevelSelectionUI : MonoBehaviour
{
	public AllLevelsData allLevelsData; // R�f�rence au Scriptable Object contenant tous les niveaux
	public Image levelImageDisplay;                 // L'image UI pour afficher le niveau
	public TextMeshProUGUI levelNameText;           // Le TextMeshPro pour afficher le nom du niveau
	public Button previousButton;                   // Le bouton pr�c�dent
	public Button nextButton;                       // Le bouton suivant

	private int currentLevelIndex = 0;              // L'index du niveau actuellement s�lectionn�

	void Start()
	{
		UpdateLevelDisplay();
	}

	public void NextLevel()
	{
		if (currentLevelIndex < allLevelsData.levels.Count - 1)
		{
			currentLevelIndex++;
			UpdateLevelDisplay();
		}
	}

	public void PreviousLevel()
	{
		if (currentLevelIndex > 0)
		{
			currentLevelIndex--;
			UpdateLevelDisplay();
		}
	}

	private void UpdateLevelDisplay()
	{
		LevelData currentLevel = allLevelsData.levels[currentLevelIndex];

		// Mettre � jour l'affichage de l'image et du nom
		levelImageDisplay.sprite = currentLevel.illustration;
		levelNameText.text = currentLevel.levelName;

		// Activer ou d�sactiver les boutons selon la position dans la liste
		bool isPreviousInteractable = currentLevelIndex > 0;
		bool isNextInteractable = currentLevelIndex < allLevelsData.levels.Count - 1;

		previousButton.interactable = isPreviousInteractable;
		nextButton.interactable = isNextInteractable;

		// Activer/d�sactiver les scripts ButtonTween
		ButtonTween previousButtonTween = previousButton.GetComponent<ButtonTween>();
		ButtonTween nextButtonTween = nextButton.GetComponent<ButtonTween>();

		if (previousButtonTween != null)
		{
			previousButtonTween.enabled = isPreviousInteractable;
		}

		if (nextButtonTween != null)
		{
			nextButtonTween.enabled = isNextInteractable;
		}

		// Griser les boutons d�sactiv�s
		ColorBlock colorBlock = previousButton.colors;
		colorBlock.disabledColor = new Color(0.5f, 0.5f, 0.5f); // Gris pour le bouton d�sactiv�
		previousButton.colors = colorBlock;
		nextButton.colors = colorBlock;

		// Enregistrer le niveau actuellement s�lectionn�
		PlayerPrefs.SetInt("SelectedLevel", currentLevelIndex);
		PlayerPrefs.Save();
	}
}

