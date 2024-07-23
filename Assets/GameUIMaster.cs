using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIMaster : MonoBehaviour
{
	private AudioSource audioSource;
	public AudioClip buttonClickSound;

	// Start is called before the first frame update
	void Start()
    {
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			Debug.LogError("Aucune AudioSource trouvée sur le GameObject.");
		}

	}

	// Update is called once per frame
	void Update()
    {
        
    }

	public void LaunchGame()
	{
		StartCoroutine(LaunchGameCoroutine());
	}

public void LaunchLevelSelection()
	{
		StartCoroutine(LaunchLevelSelectionCoroutine());

	}

	public void BackToMenu()
	{
		StartCoroutine(BacktoMenuCoroutine	());
	}

	public void LaunchLeaderboard()
	{
		StartCoroutine(LaunchLeaderboardCoroutine());
	}

	private IEnumerator LaunchLevelSelectionCoroutine()
	{
		audioSource.PlayOneShot(buttonClickSound);
		yield return new WaitForSeconds(0.4f);
		SceneManager.LoadScene("LevelSelection");
	}

	private IEnumerator LaunchGameCoroutine()
	{
		audioSource.PlayOneShot(buttonClickSound);
		yield return new WaitForSeconds(0.4f);
		SceneManager.LoadScene("WhackMoleGame");
	}

	private IEnumerator LaunchLeaderboardCoroutine()
	{
		audioSource.PlayOneShot(buttonClickSound);
		yield return new WaitForSeconds(buttonClickSound.length);
		SceneManager.LoadScene("Leaderboard");
	}


	private IEnumerator BacktoMenuCoroutine()
	{
		audioSource.PlayOneShot(buttonClickSound);
		yield return new WaitForSeconds(buttonClickSound.length);
		SceneManager.LoadScene("MainMenu");
	}

	
}
