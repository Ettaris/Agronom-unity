using UnityEngine;
using System.Collections.Generic;
using Infrastructure;

public class UIReferenceManager : MonoBehaviour
{
    [System.Serializable]
    public class ReferenceEntry
    {
        public string id;
        public GameObject target;
    }

    [SerializeField] private List<ReferenceEntry> _references = new List<ReferenceEntry>();
    private Dictionary<string, GameObject> _dict = new Dictionary<string, GameObject>();

    public static UIReferenceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var entry in _references)
        {
            if (!string.IsNullOrEmpty(entry.id) && entry.target != null)
            {
                _dict[entry.id] = entry.target;
            }
        }
        ServiceLocator.Register(this);
    }

    public GameObject GetObject(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("UIReferenceManager: id is null or empty");
            return null;
        }
        if (_dict.TryGetValue(id, out var obj))
            return obj;
        Debug.LogWarning($"UIReferenceManager: Object with id '{id}' not found.");
        return null;
    }
}