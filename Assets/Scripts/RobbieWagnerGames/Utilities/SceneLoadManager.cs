using UnityEngine.SceneManagement;
using System.Collections;
using RobbieWagnerGames.UI;
using RobbieWagnerGames.Managers;
using System;
using UnityEngine;

namespace RobbieWagnerGames.Utilities
{
	public class SceneLoadManager : MonoBehaviourSingleton<SceneLoadManager>
	{
		protected override void Awake()
		{
			base.Awake();
		}
		public void LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Action callback = null)
		{
			Scene activeScene = SceneManager.GetActiveScene();

			if (activeScene.name.Equals(sceneName))
			{
				Debug.LogWarning(sceneName + " is already the active scene");
				return;
			}
			
			StartCoroutine(LoadAsync(sceneName, loadSceneMode, callback));
		}
		public void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Action callback = null)
		{
			Scene activeScene = SceneManager.GetActiveScene();

			if (activeScene.name.Equals(sceneName))
			{
				Debug.LogWarning(sceneName + " is already the active scene");
				return;
			}
			
			SceneManager.LoadScene(sceneName, loadSceneMode);
			callback?.Invoke();
		}

		private IEnumerator LoadAsync(string sceneName, LoadSceneMode loadSceneMode, Action callback)
		{
			InputManager.Instance.DisableAllActionMaps();
			yield return ScreenCover.Instance.FadeCoverIn(1f);

			var asyncOp = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

			if (asyncOp == null)
			{
				Debug.LogError("[SceneLoadManager] Failed to load scene: " + sceneName);
				yield break;
			}
			
			callback?.Invoke();
			yield return ScreenCover.Instance.FadeCoverOut(1f);
			InputManager.Instance.EnableActionMap(ActionMapName.UI);
			yield return null;
		}
	}
}
