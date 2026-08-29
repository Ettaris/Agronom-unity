using UnityEngine;
using System.Collections.Generic;
using Infrastructure;
using Infrastructure.Events;

public class HarvestBreakdownPool : MonoBehaviour
{
    public static HarvestBreakdownPool Instance { get; private set; }

    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _poolSize = 5;
    [SerializeField] private Transform _container;

    private Queue<HarvestBreakdownView> _pool = new Queue<HarvestBreakdownView>();

    //Pool is not registered in ServiceLocator.
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EventBus.Subscribe<HarvestBreakdownReadyEvent>(OnHarvestBreakdownReady);
    }

    private void Start()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject obj = Instantiate(_prefab, _container);
            obj.SetActive(false);
            _pool.Enqueue(obj.GetComponent<HarvestBreakdownView>());
        }
    }

    private void OnDestroy() => EventBus.Unsubscribe<HarvestBreakdownReadyEvent>(OnHarvestBreakdownReady);

    private void OnHarvestBreakdownReady(HarvestBreakdownReadyEvent evt)
    {
        var view = GetFromPool();
        view.Show(evt.Result, evt.Plant, evt.ScreenPos);
    }

    public HarvestBreakdownView GetFromPool()
    {
        if (_pool.Count > 0)
        {
            var view = _pool.Dequeue();
            view.gameObject.SetActive(true);
            return view;
        }
        else
        {
            GameObject obj = Instantiate(_prefab, _container);
            return obj.GetComponent<HarvestBreakdownView>();
        }
    }

    public void ReturnToPool(HarvestBreakdownView view)
    {
        view.gameObject.SetActive(false);
        _pool.Enqueue(view);
    }
}