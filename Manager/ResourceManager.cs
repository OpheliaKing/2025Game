using System.Collections;
using System.Collections.Generic;
using Shin;
using UnityEngine;

public class ResourceManager : ManagerBase
{
    [SerializeField]
    private string _prefabBasePath = "Prefab"; // Resources 하위 기본 프리팹 경로

    [SerializeField]
    private string _uiPrefabPath = "Prefab/UI"; // Resources 하위 UI 프리팹 경로

    [SerializeField]
    private string _characterPrefabPath = "Prefab/Player/Character";

    [SerializeField]
    private string _soPath = "SO";

    [SerializeField]
    private string _soundPath = "Sound";

    [SerializeField]
    private string _spritePath = "Sprite";

    private Dictionary<string, ScriptableObject> _cashedSO = new Dictionary<string, ScriptableObject>();

    // Sprite Mode: Multiple로 잘린 경우, 동일 Texture 리소스 경로에서 Sprite 서브애셋들을 로드/캐시합니다.
    private readonly Dictionary<string, Sprite> _cashedSprites = new Dictionary<string, Sprite>();

    public string PrefabBasePath
    {
        get { return _prefabBasePath; }
        set { _prefabBasePath = value ?? string.Empty; }
    }

    public string UIPrefabPath
    {
        get { return _uiPrefabPath; }
        set { _uiPrefabPath = value ?? string.Empty; }
    }


    public string CharacterPrefabPath
    {
        get { return _characterPrefabPath; }
        set { _characterPrefabPath = value ?? string.Empty; }
    }

    public string SOPath
    {
        get { return _soPath; }
        set { _soPath = value ?? string.Empty; }
    }

    public string SoundPath
    {
        get { return _soundPath; }
        set { _soundPath = value ?? string.Empty; }
    }

    public string SpritePath
    {
        get { return _spritePath; }
        set { _spritePath = value ?? string.Empty; }
    }

    public AudioClip LoadAudioClip(string nameOrRelativePath, SOUND_TYPE type, string basePath = null)
    {
        string path = CombineResourcePath(basePath ?? _soundPath, type == SOUND_TYPE.BGM ? "BGM" : "SE");
        path += "/" + nameOrRelativePath;
        return Resources.Load<AudioClip>(path);
    }

    private static string CombineResourcePath(string basePath, string subPath)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            return string.IsNullOrEmpty(subPath) ? string.Empty : subPath.TrimStart('/', '\\');
        }

        if (string.IsNullOrEmpty(subPath))
        {
            return basePath.Trim('/', '\\');
        }

        return basePath.Trim('/', '\\') + "/" + subPath.TrimStart('/', '\\');
    }

    public GameObject LoadPrefab(string nameOrRelativePath, string basePath = null)
    {
        string path = CombineResourcePath(basePath ?? _prefabBasePath, nameOrRelativePath);
        return Resources.Load<GameObject>(path);
    }

    public T LoadPrefab<T>(string nameOrRelativePath, string basePath = null) where T : Object
    {
        string path = CombineResourcePath(basePath ?? _prefabBasePath, nameOrRelativePath);
        return Resources.Load<T>(path);
    }

    public GameObject InstantiatePrefab(string nameOrRelativePath, Transform parent = null, string basePath = null)
    {
        GameObject prefab = LoadPrefab(nameOrRelativePath, basePath);
        if (prefab == null)
        {
            return null;
        }
        return parent == null ? Object.Instantiate(prefab) : Object.Instantiate(prefab, parent);
    }

    public T InstantiatePrefab<T>(string nameOrRelativePath, Transform parent = null, string basePath = null) where T : Object
    {
        T prefab = LoadPrefab<T>(nameOrRelativePath, basePath);
        if (prefab == null)
        {
            return null;
        }
        return parent == null ? Object.Instantiate(prefab) : Object.Instantiate(prefab, parent);
    }

    public T LoadSO<T>(string nameOrRelativePath, string basePath = null) where T : ScriptableObject
    {
        string path = CombineResourcePath(basePath ?? _prefabBasePath, nameOrRelativePath);

        if (_cashedSO.ContainsKey(nameOrRelativePath))
        {
            return (T)_cashedSO[nameOrRelativePath];
        }
        else
        {
            return Resources.Load<T>(path);
        }

    }

    /// <summary>
    /// Sprite Mode: Multiple로 자른 Sprite를 지정한 이름으로 로드합니다.
    /// 예) Resources에 "UI/Icons.png"(Sprite Multiple) 이 있고, spriteName이 "idle"일 때 사용.
    /// </summary>
    public Sprite LoadSprite(string spriteName, string textureNameOrRelativePath, string basePath = null)
    {
        if (string.IsNullOrEmpty(spriteName) || string.IsNullOrEmpty(textureNameOrRelativePath))
        {
            return null;
        }

        string texturePath = CombineResourcePath(basePath ?? string.Empty, textureNameOrRelativePath);
        string cacheKey = $"{texturePath}|{spriteName}";
        if (_cashedSprites.TryGetValue(cacheKey, out var cached) && cached != null)
        {
            return cached;
        }

        // Sprite Mode: Multiple이면, Texture 리소스 경로에서 잘린 Sprite 서브애셋들이 함께 로드됩니다.
        Sprite[] sprites = Resources.LoadAll<Sprite>(texturePath);
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite s = sprites[i];
            if (s != null && s.name == spriteName)
            {
                _cashedSprites[cacheKey] = s;
                return s;
            }
        }

        return null;
    }

    /// <summary>
    /// Sprite Mode: Multiple로 잘린 Sprite들을 전부 로드합니다.
    /// </summary>
    public Sprite[] LoadSprites(string textureNameOrRelativePath, string basePath = null)
    {
        if (string.IsNullOrEmpty(textureNameOrRelativePath))
        {
            return null;
        }

        string texturePath = CombineResourcePath(basePath ?? string.Empty, textureNameOrRelativePath);
        return Resources.LoadAll<Sprite>(texturePath);
    }
}
