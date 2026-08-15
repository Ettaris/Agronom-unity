using UnityEngine;
using System.Collections.Generic;

public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance { get; private set; }

    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _poolSize = 10;
    [SerializeField] private Transform _container;

    private Queue<FloatingText> _pool = new Queue<FloatingText>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_container == null)
            _container = transform;

        for (int i = 0; i < _poolSize; i++)
        {
            GameObject obj = Instantiate(_prefab, _container);
            obj.SetActive(false);
            _pool.Enqueue(obj.GetComponent<FloatingText>());
        }
    }

    public void ShowTextAtWorld(Vector3 worldPos, string text, Color color, float floatHeight, float duration)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        ShowTextAtScreen(screenPos, text, color, floatHeight, duration);
    }

    public void ShowTextAtScreen(Vector2 screenPos, string text, Color color, float floatHeight, float duration)
    {
        var ft = GetFromPool();
        if (ft != null)
            ft.Setup(text, color, screenPos, floatHeight, duration);
    }

    private FloatingText GetFromPool()
    {
        if (_pool.Count > 0)
        {
            var ft = _pool.Dequeue();
            ft.gameObject.SetActive(true);
            return ft;
        }
        else
        {
            GameObject obj = Instantiate(_prefab, _container);
            return obj.GetComponent<FloatingText>();
        }
    }

    public void ReturnToPool(FloatingText ft)
    {
        ft.gameObject.SetActive(false);
        _pool.Enqueue(ft);
    }
}