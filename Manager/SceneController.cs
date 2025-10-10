using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
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

		public void LoadScene(string sceneName, Action loadComplete = null)
		{
			if (string.IsNullOrEmpty(sceneName)) 
			{
				Debug.LogError("씬 이름이 비어있습니다.");
				return;
			}
			
			StopActiveLoadIfAny();
			onBeforeSceneLoad?.Invoke(sceneName);

			// 포톤 퓨전 러너 상태 확인
			var runner = GameManager.Instance.NetworkManager.Runner;
			Debug.Log($"Runner 상태 - IsRunning: {runner?.IsRunning}, IsCloudReady: {runner?.IsCloudReady}");
			
			if (usePhotonLoadWhenInRoom && runner != null && runner.IsRunning && runner.IsCloudReady)
			{
				Debug.Log($"포톤 퓨전을 통한 씬 로드: {sceneName}");
				
				// NetworkSceneManager를 통한 씬 전환
				var sceneManager = runner.GetComponent<INetworkSceneManager>();
				if (sceneManager != null)
				{
					Debug.Log("NetworkSceneManager를 통한 씬 전환 시작");
					var loadOperation = sceneManager.LoadScene(SceneRef.FromIndex(GetSceneIndex(sceneName)), new NetworkLoadSceneParameters());
					
					// NetworkSceneAsyncOp를 사용한 씬 로드 완료 대기
					StartCoroutine(WaitForSceneLoadCompleteWithAsyncOp(loadOperation, sceneName, loadComplete));
				}
				else
				{
					Debug.LogWarning("NetworkSceneManager를 찾을 수 없습니다. 로컬 씬 로드로 전환");
					StartCoroutine(LoadSceneRoutine(sceneName, loadComplete));
				}
				return;
			}

			Debug.Log($"로컬 씬 로드: {sceneName}");
			StartCoroutine(LoadSceneRoutine(sceneName, loadComplete));
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

		private IEnumerator LoadSceneRoutine(string sceneName, Action loadComplete = null)
		{
			Debug.Log($"씬 로드 시작: {sceneName}");
			_activeLoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
			_activeLoadOperation.allowSceneActivation = true;
			
			while (!_activeLoadOperation.isDone)
			{
				onLoadProgress?.Invoke(Mathf.Clamp01(_activeLoadOperation.progress / 0.9f));
				yield return null;
			}
			
			_activeLoadOperation = null;
			Debug.Log($"씬 로드 완료: {sceneName}");
			onAfterSceneLoad?.Invoke(sceneName);
			loadComplete?.Invoke();
		}

		private IEnumerator WaitForSceneLoadCompleteWithAsyncOp(NetworkSceneAsyncOp loadOperation, string sceneName, Action loadComplete = null)
		{
			Debug.Log($"NetworkSceneAsyncOp를 사용한 씬 로드 완료 대기: {sceneName}");
			
			// NetworkSceneAsyncOp 완료까지 대기
			yield return new WaitUntil(() => loadOperation.IsDone);
			
			// 씬 로드 완료 처리
			Debug.Log($"NetworkSceneAsyncOp 씬 로드 완료: {sceneName}");
			onLoadProgress?.Invoke(1f);
			onAfterSceneLoad?.Invoke(sceneName);
			loadComplete?.Invoke();
		}

		private IEnumerator WaitForSceneLoadComplete(string sceneName, Action loadComplete = null)
		{
			Debug.Log($"포톤 퓨전 씬 로드 완료 대기: {sceneName}");
			
			// 씬 로드 완료까지 대기 (최대 30초)
			float timeout = 30f;
			float elapsed = 0f;
			var runner = GameManager.Instance.NetworkManager.Runner;
			var sceneManager = runner.GetComponent<INetworkSceneManager>();

			while (elapsed < timeout)
			{
				// INetworkSceneManager의 MainRunnerScene을 통해 현재 씬 확인
				string currentSceneName = sceneManager.MainRunnerScene.name;
				Debug.Log($"MainRunnerScene: {currentSceneName}, Target: {sceneName}");

				// 씬 이름 비교 (대소문자 구분 없이)
				if (string.Equals(currentSceneName, sceneName, System.StringComparison.OrdinalIgnoreCase))
				{
					Debug.Log($"포톤 퓨전 씬 로드 완료: {sceneName}");
					onLoadProgress?.Invoke(1f);
					onAfterSceneLoad?.Invoke(sceneName);
					loadComplete?.Invoke();
					yield break;
				}

				elapsed += Time.deltaTime;
				onLoadProgress?.Invoke(Mathf.Clamp01(elapsed / timeout));
				yield return null;
			}
			
			Debug.LogError($"포톤 퓨전 씬 로드 타임아웃: {sceneName}");
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

		private int GetSceneIndex(string sceneName)
		{
			for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
				string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
				if (sceneNameFromPath == sceneName)
				{
					return i;
				}
			}
			Debug.LogError($"씬을 찾을 수 없습니다: {sceneName}");
			return 0;
		}

		// NetworkManager에서 호출되는 콜백
		public void OnNetworkSceneLoadDone()
		{
			Debug.Log($"NetworkManager 콜백: 씬 로드 완료 - {CurrentSceneName}");
			// 씬 로드 완료 이벤트 발생
			onAfterSceneLoad?.Invoke(CurrentSceneName);
		}
	}
}


