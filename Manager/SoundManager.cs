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
        private AudioSource _bgmSource;
        private AudioSource _seSource;

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

            var bgmGo = new GameObject("BGM");
            bgmGo.transform.SetParent(_audioSourceRoot);
            _bgmSource = bgmGo.AddComponent<AudioSource>();
            _playingSoundsBgm["bgm"] = _bgmSource;

            var seGo = new GameObject("SE_Root");
            seGo.transform.SetParent(_audioSourceRoot);
            _seSource = seGo.AddComponent<AudioSource>(); // 참조용, SE 재생은 동적 생성 AudioSource 사용
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

            var clip = resourceManager.LoadAudioClip(nameOrRelativePath, resourceManager.SoundPath);
            if (clip != null)
                _clipCache[nameOrRelativePath] = clip;

            return clip;
        }

        /// <summary>
        /// 이름으로 사운드 재생. 캐싱된 클립이 있으면 캐시에서 사용.
        /// SE일 경우 전용 AudioSource 오브젝트를 생성하여 재생 (동시 다중 SE 재생 가능).
        /// </summary>
        /// <param name="type">사운드 타입 (BGM 또는 SE)</param>
        /// <param name="nameOrRelativePath">사운드 파일 이름 또는 Resources 하위 상대 경로</param>
        /// <param name="volume">볼륨 (0~1)</param>
        /// <param name="loop">반복 재생 여부</param>
        /// <returns>재생 ID(문자열). 중지 시 사용. 재생 실패 시 null</returns>
        public string Play(SOUND_TYPE type, string nameOrRelativePath, float volume = 1f, bool loop = false)
        {
            EnsureAudioSourceRoot();

            var clip = GetOrLoadClip(nameOrRelativePath);
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

            // SE: 전용 AudioSource 오브젝트 생성 후 재생
            var seGo = new GameObject($"SE_{id.Substring(0, 8)}");
            seGo.transform.SetParent(_audioSourceRoot);

            var seSource = seGo.AddComponent<AudioSource>();
            seSource.clip = clip;
            seSource.volume = Mathf.Clamp01(volume);
            seSource.loop = loop;
            seSource.Play();

            _playingSoundsSe[id] = seSource;

            if (!loop)
                StartCoroutine(DestroySeWhenFinished(id, seSource, clip.length));

            return id;
        }

        /// <summary>
        /// SE 재생이 끝나면 AudioSource 오브젝트를 제거하고 딕셔너리에서 제거.
        /// </summary>
        private IEnumerator DestroySeWhenFinished(string soundId, AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration + 0.1f);
            if (source != null && source.gameObject != null)
            {
                if (_playingSoundsSe.TryGetValue(soundId, out var current) && current == source)
                    _playingSoundsSe.Remove(soundId);
                Destroy(source.gameObject);
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
                {
                    seSource.Stop();
                    if (seSource != _seSource)
                        Destroy(seSource.gameObject);
                }
                _playingSoundsSe.Remove(soundId);
            }
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
