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
				
				// 호스트 권한 확인 - 서버만 씬 로드를 시작할 수 있음
				if (runner.IsServer)
				{
					Debug.Log("호스트가 씬 로드를 시작합니다.");
					LoadSceneAsHost(sceneName, loadComplete);
				}
				else
				{
					Debug.Log("클라이언트는 호스트의 씬 로드를 기다립니다.");
					// 클라이언트는 포톤퓨전의 자동 씬 동기화를 기다림
					StartCoroutine(WaitForPhotonSceneSync(sceneName, loadComplete));
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

		// 클라이언트에서 호스트에게 씬 로드를 요청하는 메서드
		public void RequestSceneLoadFromClient(string sceneName)
		{
			var runner = GameManager.Instance.NetworkManager.Runner;
			if (runner != null && runner.IsRunning && runner.IsCloudReady)
			{
				if (runner.IsServer)
				{
					// 호스트인 경우 직접 로드
					LoadScene(sceneName);
				}
				else
				{
					// 클라이언트인 경우 호스트에게 요청
					Debug.Log($"클라이언트가 호스트에게 씬 로드 요청: {sceneName}");
					RequestSceneLoad(sceneName);
				}
			}
			else
			{
				Debug.LogWarning("네트워크가 연결되지 않았습니다. 로컬 씬 로드로 전환");
				LoadScene(sceneName);
			}
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
			
			// 서버인 경우 모든 클라이언트에게 씬 로드 완료 알림
			var runner = GameManager.Instance.NetworkManager.Runner;
			if (runner != null && runner.IsServer)
			{
				NotifySceneLoadCompleted(sceneName);
			}
			
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

		// 호스트가 씬을 로드하는 메서드
		private void LoadSceneAsHost(string sceneName, Action loadComplete = null)
		{
			var runner = GameManager.Instance.NetworkManager.Runner;
			var sceneManager = runner.GetComponent<INetworkSceneManager>();
			
			// 모든 클라이언트에게 씬 로드 시작 알림
			NotifySceneLoadStarted(sceneName);
			
			if (sceneManager != null)
			{
				Debug.Log("호스트가 NetworkSceneManager를 통한 씬 전환을 시작합니다.");
				
				// NetworkLoadSceneParameters 설정으로 동기화 보장
				var parameters = new NetworkLoadSceneParameters();
				
				var loadOperation = sceneManager.LoadScene(SceneRef.FromIndex(GetSceneIndex(sceneName)), parameters);
				
				// NetworkSceneAsyncOp를 사용한 씬 로드 완료 대기
				StartCoroutine(WaitForSceneLoadCompleteWithAsyncOp(loadOperation, sceneName, loadComplete));
			}
			else
			{
				Debug.LogWarning("NetworkSceneManager를 찾을 수 없습니다. 로컬 씬 로드로 전환");
				StartCoroutine(LoadSceneRoutine(sceneName, loadComplete));
			}
		}

		// 포톤퓨전의 자동 씬 동기화를 기다리는 메서드 (클라이언트용)
		private IEnumerator WaitForPhotonSceneSync(string sceneName, Action loadComplete = null)
		{
			Debug.Log($"클라이언트가 포톤퓨전 씬 동기화를 기다립니다: {sceneName}");
			
			var runner = GameManager.Instance.NetworkManager.Runner;
			var sceneManager = runner.GetComponent<INetworkSceneManager>();
			
			// 씬 동기화 완료까지 대기 (최대 60초)
			float timeout = 60f;
			float elapsed = 0f;
			bool sceneSynced = false;

			while (elapsed < timeout && !sceneSynced)
			{
				// INetworkSceneManager의 MainRunnerScene을 통해 현재 씬 확인
				if (sceneManager != null)
				{
					string currentSceneName = sceneManager.MainRunnerScene.name;
					Debug.Log($"포톤퓨전 씬 동기화 확인 - 현재: {currentSceneName}, 목표: {sceneName}");

					// 씬 이름 비교 (대소문자 구분 없이)
					if (string.Equals(currentSceneName, sceneName, System.StringComparison.OrdinalIgnoreCase))
					{
						Debug.Log($"포톤퓨전 씬 동기화 완료: {sceneName}");
						sceneSynced = true;
						onLoadProgress?.Invoke(1f);
						onAfterSceneLoad?.Invoke(sceneName);
						loadComplete?.Invoke();
						yield break;
					}
				}

				elapsed += Time.deltaTime;
				onLoadProgress?.Invoke(Mathf.Clamp01(elapsed / timeout));
				yield return null;
			}
			
			if (!sceneSynced)
			{
				Debug.LogError($"포톤퓨전 씬 동기화 타임아웃: {sceneName}");
			}
		}

		// 클라이언트가 호스트의 씬 로드를 기다리는 메서드 (레거시)
		private IEnumerator WaitForHostSceneLoad(string sceneName, Action loadComplete = null)
		{
			Debug.Log($"클라이언트가 호스트의 씬 로드 완료를 기다립니다: {sceneName}");
			
			var runner = GameManager.Instance.NetworkManager.Runner;
			var sceneManager = runner.GetComponent<INetworkSceneManager>();
			
			// 씬 로드 완료까지 대기 (최대 60초)
			float timeout = 60f;
			float elapsed = 0f;
			bool sceneLoaded = false;

			while (elapsed < timeout && !sceneLoaded)
			{
				// INetworkSceneManager의 MainRunnerScene을 통해 현재 씬 확인
				if (sceneManager != null)
				{
					string currentSceneName = sceneManager.MainRunnerScene.name;
					Debug.Log($"클라이언트 씬 확인 - 현재: {currentSceneName}, 목표: {sceneName}");

					// 씬 이름 비교 (대소문자 구분 없이)
					if (string.Equals(currentSceneName, sceneName, System.StringComparison.OrdinalIgnoreCase))
					{
						Debug.Log($"클라이언트 씬 로드 완료: {sceneName}");
						sceneLoaded = true;
						onLoadProgress?.Invoke(1f);
						onAfterSceneLoad?.Invoke(sceneName);
						loadComplete?.Invoke();
						yield break;
					}
				}

				elapsed += Time.deltaTime;
				onLoadProgress?.Invoke(Mathf.Clamp01(elapsed / timeout));
				yield return null;
			}
			
			if (!sceneLoaded)
			{
				Debug.LogError($"클라이언트 씬 로드 타임아웃: {sceneName}");
			}
		}

		// RPC를 통한 씬 로드 요청 (호스트에게)
		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RequestSceneLoad(string sceneName)
		{
			Debug.Log($"RPC로 씬 로드 요청 받음: {sceneName}");
			LoadScene(sceneName);
		}

		// RPC를 통한 씬 로드 알림 (호스트에서 모든 클라이언트에게)
		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void NotifySceneLoadStarted(string sceneName)
		{
			Debug.Log($"호스트에서 씬 로드 시작 알림: {sceneName}");
			onBeforeSceneLoad?.Invoke(sceneName);
		}

		// RPC를 통한 씬 로드 완료 알림 (호스트에서 모든 클라이언트에게)
		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void NotifySceneLoadCompleted(string sceneName)
		{
			Debug.Log($"호스트에서 씬 로드 완료 알림: {sceneName}");
			onAfterSceneLoad?.Invoke(sceneName);
		}

		// NetworkManager에서 호출되는 콜백
		public void OnNetworkSceneLoadDone()
		{
			Debug.Log($"NetworkManager 콜백: 씬 로드 완료 - {CurrentSceneName}");
			// 씬 로드 완료 이벤트 발생
			onAfterSceneLoad?.Invoke(CurrentSceneName);
		}

		// 포톤퓨전 씬 로드 시작 콜백
		public void OnSceneLoadStart(NetworkRunner runner)
		{
			Debug.Log($"포톤퓨전 씬 로드 시작: {runner}");
			// 씬 로드 시작 이벤트는 RPC로 처리됨
		}

		// 포톤퓨전 씬 로드 완료 콜백
		public void OnSceneLoadDone(NetworkRunner runner)
		{
			Debug.Log($"포톤퓨전 씬 로드 완료: {runner}");
			onAfterSceneLoad?.Invoke(CurrentSceneName);
		}
	}
}


