using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using PlayFab.ClientModels;

public class LeaderboardUI : MonoBehaviour
{
	public GameObject lineContainerPrefab; // Reference to the LineContainer prefab
	public GameObject leaderboardParent; // Reference to the parent GameObject with Vertical Layout Group

	private void Start()
	{
		ClearLeaderboard();
		FetchAndDisplayLeaderboard();
	}

	private void ClearLeaderboard()
	{
		// Clear existing entries
		foreach (Transform child in leaderboardParent.transform)
		{
			Destroy(child.gameObject);
		}
	}

	public void FetchAndDisplayLeaderboard()
	{
		Leaderboards.Instance.GetTopScores(10, DisplayLeaderboard);
	}

	private void DisplayLeaderboard(List<PlayerLeaderboardEntry> entries)
	{
		ClearLeaderboard();

		// Create a new entry for each player
		foreach (PlayerLeaderboardEntry entry in entries)
		{
			GameObject lineContainer = Instantiate(lineContainerPrefab, leaderboardParent.transform);

			// Find the TextContainer first
			Transform textContainerTransform = lineContainer.transform.Find("TextContainer");
			if (textContainerTransform == null)
			{
				Debug.LogError("TextContainer object not found.");
				continue;
			}

			// Find and set the Rank Text
			Transform rankTransform = textContainerTransform.Find("Rank Text");
			if (rankTransform == null)
			{
				Debug.LogError("Rank Text object not found.");
				continue;
			}

			TMP_Text rankText = rankTransform.GetComponent<TMP_Text>();
			if (rankText == null)
			{
				Debug.LogError("TMP_Text component not found on Rank Text.");
				continue;
			}

			rankText.text = (entry.Position + 1).ToString();

			// Find and set the UserName Text
			Transform userNameTransform = textContainerTransform.Find("UserName Text");
			if (userNameTransform == null)
			{
				Debug.LogError("UserName Text object not found.");
				continue;
			}

			TMP_Text userNameText = userNameTransform.GetComponent<TMP_Text>();
			if (userNameText == null)
			{
				Debug.LogError("TMP_Text component not found on UserName Text.");
				continue;
			}

			userNameText.text = string.IsNullOrEmpty(entry.DisplayName) ? "Unknown" : entry.DisplayName;

			// Find and set the Score Text
			Transform scoreTransform = textContainerTransform.Find("Score Text");
			if (scoreTransform == null)
			{
				Debug.LogError("Score Text object not found.");
				continue;
			}

			TMP_Text scoreText = scoreTransform.GetComponent<TMP_Text>();
			if (scoreText == null)
			{
				Debug.LogError("TMP_Text component not found on Score Text.");
				continue;
			}

			scoreText.text = entry.StatValue.ToString();
		}
	}
}
