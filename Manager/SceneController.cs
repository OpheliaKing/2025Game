using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Shin
{
	// UnityEngine.SceneManagement.SceneManager 와의 이름 충돌을 피하기 위해 SceneController 로 명명
	public class SceneController : MonoBehaviour
	{
		[Header("Events")]
		public UnityEvent<string> onBeforeSceneLoad;
		public UnityEvent<string> onAfterSceneLoad;
		public UnityEvent<string> onBeforeSceneUnload;
		public UnityEvent<string> onAfterSceneUnload;
		public UnityEvent<float> onLoadProgress; // 0~1

		[SerializeField]
		private bool usePhotonLoadWhenInRoom = true;

		private AsyncOperation _activeLoadOperation;
		private readonly HashSet<string> _additivelyLoaded = new HashSet<string>();

		public string CurrentSceneName => SceneManager.GetActiveScene().name;

		public void LoadScene(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName)) return;
			StopActiveLoadIfAny();
			onBeforeSceneLoad?.Invoke(sceneName);

			// 방 안이고 옵션이 켜져있으면 Photon으로 로드 (AutomaticallySyncScene 필요)
			if (usePhotonLoadWhenInRoom && PhotonNetwork.InRoom)
			{
				PhotonNetwork.LoadLevel(sceneName);
				onLoadProgress?.Invoke(1f);
				onAfterSceneLoad?.Invoke(sceneName);
				return;
			}

			StartCoroutine(LoadSceneRoutine(sceneName));
		}

		public void ReloadCurrentScene()
		{
			LoadScene(CurrentSceneName);
		}

		public void LoadAdditive(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName)) return;
			StartCoroutine(LoadAdditiveRoutine(sceneName));
		}

		public void UnloadAdditive(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName)) return;
			if (!_additivelyLoaded.Contains(sceneName)) return;
			StartCoroutine(UnloadAdditiveRoutine(sceneName));
		}

		private IEnumerator LoadSceneRoutine(string sceneName)
		{
			_activeLoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
			_activeLoadOperation.allowSceneActivation = true;
			while (!_activeLoadOperation.isDone)
			{
				onLoadProgress?.Invoke(Mathf.Clamp01(_activeLoadOperation.progress / 0.9f));
				yield return null;
			}
			_activeLoadOperation = null;
			onAfterSceneLoad?.Invoke(sceneName);
		}

		private IEnumerator LoadAdditiveRoutine(string sceneName)
		{
			onBeforeSceneLoad?.Invoke(sceneName);
			var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			while (!op.isDone)
			{
				onLoadProgress?.Invoke(Mathf.Clamp01(op.progress / 0.9f));
				yield return null;
			}
			_additivelyLoaded.Add(sceneName);
			onAfterSceneLoad?.Invoke(sceneName);
		}

		private IEnumerator UnloadAdditiveRoutine(string sceneName)
		{
			onBeforeSceneUnload?.Invoke(sceneName);
			var op = SceneManager.UnloadSceneAsync(sceneName);
			while (!op.isDone)
			{
				// 언로드는 진행률 이벤트를 선택적으로 동일 이벤트로 보냄
				onLoadProgress?.Invoke(Mathf.Clamp01(op.progress / 0.9f));
				yield return null;
			}
			_additivelyLoaded.Remove(sceneName);
			onAfterSceneUnload?.Invoke(sceneName);
		}

		private void StopActiveLoadIfAny()
		{
			if (_activeLoadOperation != null)
			{
				// Unity AsyncOperation은 중단 API가 없어, 추가 로드를 시작하지 않도록 참조만 정리
				_activeLoadOperation = null;
			}
		}
	}
}


