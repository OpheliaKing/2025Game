using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class SoundManager : ManagerBase
    {
        [SerializeField]
        private string _audioBasePath = "Audio";

        private Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioSource> _playingSounds = new Dictionary<string, AudioSource>();
        private Transform _audioSourceRoot;

        public string AudioBasePath
        {
            get { return _audioBasePath; }
            set { _audioBasePath = value ?? string.Empty; }
        }

        public override void ManagerInit()
        {
            base.ManagerInit();
            EnsureAudioSourceRoot();
        }

        private void EnsureAudioSourceRoot()
        {
            if (_audioSourceRoot != null)
                return;
            var go = new GameObject("AudioSourceRoot");
            go.transform.SetParent(transform);
            _audioSourceRoot = go.transform;
        }

        /// <summary>
        /// 리소스 매니저를 통해 클립을 가져오고, 캐시에 없으면 로드 후 캐시에 저장.
        /// </summary>
        private AudioClip GetOrLoadClip(string nameOrRelativePath)
        {
            if (string.IsNullOrEmpty(nameOrRelativePath))
                return null;

            if (_clipCache.TryGetValue(nameOrRelativePath, out var cached))
                return cached;

            var resourceManager = GameManager.Instance?.ResourceManager;
            if (resourceManager == null)
            {
                Debug.LogWarning("[SoundManager] ResourceManager를 찾을 수 없습니다.");
                return null;
            }

            var clip = resourceManager.LoadAudioClip(nameOrRelativePath, _audioBasePath);
            if (clip != null)
                _clipCache[nameOrRelativePath] = clip;

            return clip;
        }

        /// <summary>
        /// 이름으로 사운드 재생. 캐싱된 클립이 있으면 캐시에서 사용.
        /// </summary>
        /// <param name="nameOrRelativePath">사운드 파일 이름 또는 Resources 하위 상대 경로</param>
        /// <param name="volume">볼륨 (0~1)</param>
        /// <param name="loop">반복 재생 여부</param>
        /// <returns>재생 ID(문자열). 중지 시 사용. 재생 실패 시 null</returns>
        public string Play(string nameOrRelativePath, float volume = 1f, bool loop = false)
        {
            EnsureAudioSourceRoot();

            var clip = GetOrLoadClip(nameOrRelativePath);
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] 사운드를 불러올 수 없습니다: {nameOrRelativePath}");
                return null;
            }

            var id = Guid.NewGuid().ToString();
            var source = CreateAudioSource();
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.loop = loop;
            source.Play();

            _playingSounds[id] = source;
            return id;
        }

        private AudioSource CreateAudioSource()
        {
            var go = new GameObject("AudioSource");
            go.transform.SetParent(_audioSourceRoot);
            var source = go.AddComponent<AudioSource>();
            return source;
        }

        private void Update()
        {
            if (_audioSourceRoot == null)
                return;

            var toRemove = new List<string>();
            foreach (var kv in _playingSounds)
            {
                if (!kv.Value.isPlaying)
                    toRemove.Add(kv.Key);
            }

            foreach (var id in toRemove)
            {
                if (_playingSounds.TryGetValue(id, out var source) && source != null)
                {
                    Destroy(source.gameObject);
                    _playingSounds.Remove(id);
                }
            }
        }

        /// <summary>
        /// 고유 ID(문자열)로 해당 사운드 중지.
        /// </summary>
        public void Stop(string soundId)
        {
            if (string.IsNullOrEmpty(soundId) || !_playingSounds.TryGetValue(soundId, out var source))
                return;

            if (source != null)
            {
                source.Stop();
                Destroy(source.gameObject);
            }

            _playingSounds.Remove(soundId);
        }

        /// <summary>
        /// 현재 재생 중인 사운드 ID(문자열) 목록 (읽기 전용).
        /// </summary>
        public IReadOnlyCollection<string> PlayingSoundIds
        {
            get { return _playingSounds.Keys; }
        }

        /// <summary>
        /// 해당 ID의 사운드가 재생 중인지 여부.
        /// </summary>
        public bool IsPlaying(string soundId)
        {
            return !string.IsNullOrEmpty(soundId)
                && _playingSounds.TryGetValue(soundId, out var source)
                && source != null
                && source.isPlaying;
        }

        /// <summary>
        /// 캐시에서 특정 클립 제거 (메모리 해제 시 사용).
        /// </summary>
        public void UnloadFromCache(string nameOrRelativePath)
        {
            _clipCache.Remove(nameOrRelativePath);
        }

        /// <summary>
        /// 사운드 캐시 전체 비우기.
        /// </summary>
        public void ClearCache()
        {
            _clipCache.Clear();
        }
    }
}
