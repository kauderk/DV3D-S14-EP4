using System;
using ToolBox.Pools;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class PathGenerator : IInitializable, ITickable
{
    private readonly IUserInput _userInput = null;
    private readonly Settings _settings = null;
    private readonly Stopwatch _stopwatch = null;
    private readonly Stopwatch _runningStopwatch = null;
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

        _userInput.OnPress += EnableCreation;
        _stopwatch = new Stopwatch(CREATION_RATE);
        _runningStopwatch = new Stopwatch(5f);
    }

    public void Initialize()
    {
        PopulatePools();
        InitializeData();
        GenerateStartPath();
    }

    public void Tick()
    {
        if (!_runningStopwatch.IsElapsed)
            if (_isEnabled && _stopwatch.IsElapsed)
            {
                Generate();
                _stopwatch.Reset();
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

    private void GenerateStartPath()
    {
        Generate(Vector3.right);

        for (int i = 0; i < PREGENERATED_TILES_COUNT; i++)
            Generate();
    }

    private void EnableCreation()
    {
        _stopwatch.Reset();
        _isEnabled = true;
        _userInput.OnPress -= EnableCreation;
    }

    private void Generate() =>
        Generate(Random.value <= 0.5f ? Vector3.forward : Vector3.right);

    private void Generate(Vector3 direction)
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
}