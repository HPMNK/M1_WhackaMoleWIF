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

	public void BackToMenu()
	{
		StartCoroutine(BacktoMenuCoroutine	());
	}


	private IEnumerator LaunchGameCoroutine()
	{
		audioSource.PlayOneShot(buttonClickSound);
		yield return new WaitForSeconds(buttonClickSound.length);
		SceneManager.LoadScene("WhackMoleGame");
	}

	private IEnumerator BacktoMenuCoroutine()
	{
		audioSource.PlayOneShot(buttonClickSound);
		yield return new WaitForSeconds(buttonClickSound.length);
		SceneManager.LoadScene("MainMenu");
	}

	
}
