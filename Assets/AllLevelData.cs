using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AllLevelsData", menuName = "ScriptableObjects/AllLevelsData", order = 1)]
public class AllLevelsData : ScriptableObject
{
	public List<LevelData> levels = new List<LevelData>(); // Liste des niveaux
}

[System.Serializable]
public class LevelData
{
	public Sprite illustration; // L'image d'illustration pour la sélection du niveau
	public string levelName; // Le nom du niveau
	public Sprite gameSkin; // Le skin en jeu
	public Sprite holeSkin; // Le skin des trous de taupes
}
