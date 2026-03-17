using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public enum SOUND_TYPE
    {
        BGM,
        SE
    }

    public class SoundManager : ManagerBase
    {
        private Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioSource> _playingSoundsBgm = new Dictionary<string, AudioSource>();
        private Dictionary<string, AudioSource> _playingSoundsSe = new Dictionary<string, AudioSource>();

        [SerializeField]
        private Transform _audioSourceRoot;

        [SerializeField]
        private int _soundCount = 5; // 초기 생성할 SE 소스 개수

        private AudioSource _bgmSource;
        private AudioSource _seSource;
        private Transform _seRoot;
        private List<AudioSource> _seSources = new List<AudioSource>(); // 풀에서 관리되는 SE 소스들

        public override void ManagerInit()
        {
            base.ManagerInit();
            EnsureAudioSourceRoot();
        }

        private void EnsureAudioSourceRoot()
        {
            if (_audioSourceRoot == null)
            {
                var go = new GameObject("AudioSourceRoot");
                go.transform.SetParent(transform);
                _audioSourceRoot = go.transform;
            }

            // BGM용 AudioSource 보장 (항상 1개만 존재하도록)
            if (_bgmSource == null)
            {
                // 기존에 "BGM" 오브젝트가 있다면 재사용
                Transform bgmTransform = _audioSourceRoot.Find("BGM");
                if (bgmTransform == null)
                {
                    var bgmGo = new GameObject("BGM");
                    bgmGo.transform.SetParent(_audioSourceRoot);
                    _bgmSource = bgmGo.AddComponent<AudioSource>();
                }
                else
                {
                    _bgmSource = bgmTransform.GetComponent<AudioSource>();
                    if (_bgmSource == null)
                        _bgmSource = bgmTransform.gameObject.AddComponent<AudioSource>();
                }
            }

            // BGM 딕셔너리 등록 보정
            if (_bgmSource != null && !_playingSoundsBgm.ContainsKey("bgm"))
                _playingSoundsBgm["bgm"] = _bgmSource;

            // SE 루트 및 풀 보장
            if (_seRoot == null)
            {
                var seRootGo = new GameObject("SE_Root");
                seRootGo.transform.SetParent(_audioSourceRoot);
                _seRoot = seRootGo.transform;
            }

            if (_seSources.Count == 0)
            {
                var count = Mathf.Max(1, _soundCount);
                for (int i = 0; i < count; i++)
                {
                    var seGo = new GameObject($"SE_{i}");
                    seGo.transform.SetParent(_seRoot);
                    var seSource = seGo.AddComponent<AudioSource>();
                    seGo.SetActive(false); // 풀링: 초기는 비활성 상태
                    _seSources.Add(seSource);
                }

                // 기존 코드와의 호환성을 위해 첫 번째 SE 소스를 기본 참조로 유지
                _seSource = _seSources[0];
            }
        }

        /// <summary>
        /// 리소스 매니저를 통해 클립을 가져오고, 캐시에 없으면 로드 후 캐시에 저장.
        /// </summary>
        private AudioClip GetOrLoadClip(string nameOrRelativePath, SOUND_TYPE type)
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

            var clip = resourceManager.LoadAudioClip(nameOrRelativePath, type, resourceManager.SoundPath);
            if (clip != null)
                _clipCache[nameOrRelativePath] = clip;

            return clip;
        }

        /// <summary>
        /// 이름으로 사운드 재생. 캐싱된 클립이 있으면 캐시에서 사용.
        /// SE일 경우 전용 AudioSource 오브젝트를 생성하여 재생 (동시 다중 SE 재생 가능).
        /// </summary>
        /// <param name="type">사운드 타입 (BGM 또는 SE)</param>
        /// <param name="nameOrRelativePath">사운드 파일 이름</param>
        /// <param name="volume">볼륨 (0~1)</param>
        /// <param name="loop">반복 재생 여부</param>
        /// <returns>재생 ID(문자열). 중지 시 사용. 재생 실패 시 null</returns>
        public string Play(SOUND_TYPE type, string nameOrRelativePath, float volume = 1f, bool loop = false)
        {
            EnsureAudioSourceRoot();

            var clip = GetOrLoadClip(nameOrRelativePath, type);
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] 사운드를 불러올 수 없습니다: {nameOrRelativePath}");
                return null;
            }

            var id = Guid.NewGuid().ToString();

            if (type == SOUND_TYPE.BGM)
            {
                _bgmSource.clip = clip;
                _bgmSource.volume = Mathf.Clamp01(volume);
                _bgmSource.loop = loop;
                _bgmSource.Play();
                _playingSoundsBgm[id] = _bgmSource;
                return id;
            }

            // SE: 오브젝트 풀에서 AudioSource를 가져와 사용
            AudioSource seSource = null;
            foreach (var source in _seSources)
            {
                if (source != null && (!source.isPlaying || !source.gameObject.activeSelf))
                {
                    seSource = source;
                    break;
                }
            }

            // 모든 소스가 사용 중이면 새로 생성하여 풀에 추가 (오브젝트 풀링 방식)
            if (seSource == null)
            {
                var parent = _seRoot != null ? _seRoot : _audioSourceRoot;
                var seGo = new GameObject($"SE_{_seSources.Count}");
                if (parent != null)
                    seGo.transform.SetParent(parent);
                var newSource = seGo.AddComponent<AudioSource>();
                seGo.SetActive(false); // 사용 시점에 활성화
                _seSources.Add(newSource);
                seSource = newSource;
            }

            // 이미 다른 ID로 등록되어 있던 소스라면 기존 매핑 제거
            string previousId = null;
            foreach (var pair in _playingSoundsSe)
            {
                if (pair.Value == seSource)
                {
                    previousId = pair.Key;
                    break;
                }
            }
            if (previousId != null)
                _playingSoundsSe.Remove(previousId);

            seSource.gameObject.SetActive(true);
            seSource.clip = clip;
            seSource.volume = Mathf.Clamp01(volume);
            seSource.loop = loop;
            seSource.Play();

            _playingSoundsSe[id] = seSource;

            if (!loop)
                StartCoroutine(ClearSeWhenFinished(id, seSource, clip.length));

            return id;
        }

        /// <summary>
        /// SE 재생이 끝나면 딕셔너리에서 제거하고, 오브젝트는 풀에 되돌린다.
        /// </summary>
        private IEnumerator ClearSeWhenFinished(string soundId, AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration + 0.1f);
            if (source != null &&
                _playingSoundsSe.TryGetValue(soundId, out var current) &&
                current == source)
            {
                _playingSoundsSe.Remove(soundId);
                source.Stop();
                source.clip = null;
                if (source.gameObject != null)
                    source.gameObject.SetActive(false); // 풀로 반환
            }
        }
        /// <summary>
        /// 고유 ID(문자열)로 해당 사운드 중지.
        /// </summary>
        public void Stop(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                return;

            if (_playingSoundsBgm.TryGetValue(soundId, out var bgmSource))
            {
                if (bgmSource != null)
                {
                    bgmSource.Stop();
                    if (bgmSource != _bgmSource)
                        Destroy(bgmSource.gameObject);
                }
                _playingSoundsBgm.Remove(soundId);
                return;
            }

            if (_playingSoundsSe.TryGetValue(soundId, out var seSource))
            {
                if (seSource != null)
                    seSource.Stop();
                _playingSoundsSe.Remove(soundId);
            }
        }

        public void StopBGM()
        {
            if (_bgmSource != null)
                _bgmSource.Stop();
        }

        /// <summary>
        /// 현재 재생 중인 사운드 ID(문자열) 목록 (읽기 전용). BGM + SE 통합.
        /// </summary>
        public IReadOnlyCollection<string> PlayingSoundIds
        {
            get
            {
                var list = new List<string>(_playingSoundsBgm.Count + _playingSoundsSe.Count);
                list.AddRange(_playingSoundsBgm.Keys);
                list.AddRange(_playingSoundsSe.Keys);
                return list;
            }
        }

        /// <summary>
        /// 해당 ID의 사운드가 재생 중인지 여부.
        /// </summary>
        public bool IsPlaying(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                return false;
            if (_playingSoundsBgm.TryGetValue(soundId, out var bgmSource) && bgmSource != null)
                return bgmSource.isPlaying;
            if (_playingSoundsSe.TryGetValue(soundId, out var seSource) && seSource != null)
                return seSource.isPlaying;
            return false;
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
