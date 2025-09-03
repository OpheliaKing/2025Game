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
}
