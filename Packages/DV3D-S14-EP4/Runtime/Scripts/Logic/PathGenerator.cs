using System;
using ToolBox.Pools;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class PathGenerator : IInitializable, ITickable, IDisposable
{
    private readonly IUserInput _userInput = null;
    private readonly Settings _settings = null;
    private readonly Stopwatch _stopwatch = null;
    private readonly FakeAsyncStopwatch _fakeAsyncStopwatch = null;
    private Transform _spawnTo = null;
    private Vector3 _crystalSpawnOffset = default;
    private Camera _camera = null;
    private bool _isEnabled = false;



    private const float CREATION_RATE = 0.1f;
    private const int TILES_START_COUNT = 100;
    private const int CRYSTALS_START_COUNT = 50;
    private const int PREGENERATED_TILES_COUNT = 50;
    private const float LEFT_BOUND = 0.05f;
    private const float RIGHT_BOUND = 0.95f;

    public PathGenerator(IUserInput userInput, Settings settings)
    {
        _userInput = userInput;
        _settings = settings;

        PoolsManager.OnBeforePress += OnBeforePressManagers;
        _userInput.OnBeforePress += OnBeforePress;
        _userInput.OnPress += EnableCreation;
        _stopwatch = new Stopwatch(CREATION_RATE);

        _fakeAsyncStopwatch = new FakeAsyncStopwatch(GenerateRandom, Generate, OnFinishedPathCreation, CRYSTALS_START_COUNT);
    }

    public void Initialize()
    {
        PopulatePools();
        InitializeData();
        OnBeforePressManagers(); // hardcoded for now
        //GenerateStartPath();
    }

    public void Tick()
    {

        if (!_fakeAsyncStopwatch.Finished && _isEnabled && _stopwatch.IsElapsed)
        {
            GenerateRandom();
            _stopwatch.Reset();
        }
        else if (_fakeAsyncStopwatch.Finished)
        {
            Debug.Log("Finished");
        }
    }

    private void PopulatePools()
    {
        _settings.FloorPrefab.Populate(TILES_START_COUNT);
        _settings.CrystalPrefab.Populate(CRYSTALS_START_COUNT);
    }

    private void InitializeData()
    {
        _spawnTo = _settings.Start;
        _crystalSpawnOffset.y = _settings.FloorPrefab.GetComponentInChildren<MeshRenderer>().bounds.extents.y +
                                _settings.CrystalPrefab.GetComponent<MeshRenderer>().bounds.extents.y;
        _camera = Camera.main;
    }

    private void EnableCreation()
    {
        _stopwatch.Reset();
        _isEnabled = true;
        _userInput.OnPress -= EnableCreation;
    }

    private void OnBeforePressManagers()
    {
        _spawnTo.position = Vector3.zero;
        _fakeAsyncStopwatch.HardReset();
    }

    private void OnFinishedPathCreation()
    {
        EnableCreation();
    }

    private void OnBeforePress()
    {
        Generate(Vector3.right);

        for (int i = 0; i < PREGENERATED_TILES_COUNT; i++)
            GenerateRandom();

        _userInput.OnBeforePress -= OnBeforePress;
        _userInput.OnPress -= EnableCreation;
    }

    public void Dispose()
    {
        PoolsManager.OnBeforePress -= OnBeforePressManagers;
        _userInput.OnBeforePress -= OnBeforePress;
        _userInput.OnPress -= EnableCreation;
    }

    public void GenerateRandom() =>
        Generate(Random.value <= 0.5f ? Vector3.forward : Vector3.right);

    public void Generate(Vector3 direction)
    {
        var oldPosition = _spawnTo.position;
        var position = oldPosition + direction;
        var viewportPosition = _camera.WorldToViewportPoint(position);

        if (viewportPosition.x < LEFT_BOUND ||
            viewportPosition.x > RIGHT_BOUND)
        {
            direction = direction == Vector3.right ? Vector3.forward : Vector3.right;
            position = oldPosition + direction;
        }

        _spawnTo = _settings.FloorPrefab.Get(position, Quaternion.identity).transform;

        if (Random.value <= _settings.CrystalSpawnProbability)
            _settings.CrystalPrefab.Get(_spawnTo.position + _crystalSpawnOffset, Quaternion.identity, _spawnTo);
    }

    [Serializable]
    public class Settings
    {
        [SerializeField] private GameObject _floorPrefab = null;
        [SerializeField] private GameObject _crystalPrefab = null;
        [SerializeField] private Transform _start = null;
        [SerializeField] private float _crystalSpawnProbability = 0.2f;

        public GameObject FloorPrefab => _floorPrefab;
        public GameObject CrystalPrefab => _crystalPrefab;
        public Transform Start => _start;
        public float CrystalSpawnProbability => _crystalSpawnProbability;
    }
    [Serializable]
    public class FakeAsyncStopwatch
    {
        private readonly Stopwatch stopwatch = null;
        private int index = 0;
        private bool finished = false;
        private bool allowed2Init = false;
        private float startPathCreationRate = .25f;
        private float countTarget = 50;
        // create callbacks
        public static Action<Vector3> OnCountCero;
        public static Action OnUpdateValue;
        public static Action OnFinished;

        public FakeAsyncStopwatch(
            Action _CountCero,
            Action<Vector3> _UpdateIndex,
            Action _Finished,
            int _countTarget,
            float _startPathCreationRate = .25f
        )
        {
            _startPathCreationRate = startPathCreationRate;
            stopwatch = new Stopwatch(startPathCreationRate);
            countTarget = _countTarget;
            OnUpdateValue += _CountCero;
            OnCountCero += _UpdateIndex;
        }

        public void HardReset(float duration = .25f)
        {
            index = 0;
            allowed2Init = true;
            finished = false;
            stopwatch.Reset(duration);
        }
        public bool Finished
        {
            get
            {
                if (!finished && allowed2Init && stopwatch.IsElapsed)
                {
                    if (index == 0) // not even started yet
                    {
                        OnCountCero?.Invoke(Vector3.right);
                    }
                    else if (index < countTarget)
                    {
                        OnUpdateValue?.Invoke();
                    }
                    else
                    {
                        OnFinished?.Invoke();
                        return finished = true;
                    }
                    index++;
                    stopwatch.Reset(stopwatch.Duration - .05f);
                }
                return false;
            }
        }
    }
}