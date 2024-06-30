using UnityEditor;
using UnityEngine;

public class EditorPause : EditorWindow
{
	private static bool isPaused = false;

	[MenuItem("Tools/Toggle Editor Pause %#SPACE")] // Keyboard shortcut: Ctrl + Space (Cmd + Space on Mac)
	private static void TogglePause()
	{
		isPaused = !isPaused;

		if (isPaused)
		{
			Pause();
		}
		else
		{
			Resume();
		}
	}

	private static void Pause()
	{
		EditorApplication.isPaused = true;
		Debug.Log("Editor paused");
	}

	private static void Resume()
	{
		EditorApplication.isPaused = false;
		Debug.Log("Editor resumed");
	}
}
