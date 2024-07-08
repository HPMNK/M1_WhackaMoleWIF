using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class GameOverUI : MonoBehaviour
{

	private TextMeshProUGUI scoreText; // Variable pour stocker le composant TextMeshProUGUI


	void Start()
	{
		GameObject scoreTextObject = GameObject.Find("ScoreText");

		if (scoreTextObject != null)
		{

			scoreText = scoreTextObject.GetComponent<TextMeshProUGUI>();


			if (scoreText != null)
			{
				int finalScore = PlayerPrefs.GetInt("FinalScore", 0);

				scoreText.text = finalScore.ToString();

			}
		}
	} 

	// Update is called once per frame
	void Update()
    {
        
    }



}
