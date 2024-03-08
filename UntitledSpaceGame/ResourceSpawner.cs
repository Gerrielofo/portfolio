using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class ResourceSpawner : MonoBehaviour, IDataPersistence
{
    [Header("Settings")]
    [SerializeField] GameObject[] _resourcePrefabs;

    [Header("Save & Load")]
    [SerializeField] List<Vector3> _resourceRotations = new();
    [SerializeField] List<Vector3> _resourcePositions = new();
    [SerializeField] List<int> _resourceIndex = new();
    public List<GameObject> _resourceGameObjects = new();

    [Header("Settings")]
    [SerializeField] int _amountToSpawn;
    [SerializeField] float _minSpawnRange;
    [SerializeField] float _maxSpawnRange;
    [SerializeField] LayerMask _terrainLayer;

    Vector3 _randomPos;
    int _randomResourceIndex;

    RaycastHit _terrainHit;

    bool _hasLoadData;

    private void Start()
    {
        if (_hasLoadData)
        {
            Debug.Log("Found Load Data");
            for (int i = 0; i < _resourcePositions.Count; i++)
            {
                SpawnResource(_resourcePositions[i], _resourceRotations[i], _resourceIndex[i]);
            }
        }
        else
        {
            for (int i = 0; i < _amountToSpawn; i++)
            {
                CheckSpawnResource();
            }
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            CheckSpawnResource();
        }
    }

    void CheckSpawnResource()
    {
        _randomPos = GetRandomPosition();
        _randomResourceIndex = GetRandomResource();

        if (Physics.Raycast(_randomPos, Vector3.down, out _terrainHit, Mathf.Infinity, _terrainLayer))
        {
            GenerateResource(_terrainHit.point, _terrainHit.normal, _randomResourceIndex);
        }
        else
        {
            CheckSpawnResource();
        }

    }

    Vector3 GetRandomPosition()
    {
        float xPos = Random.Range(_minSpawnRange, _maxSpawnRange);
        float yPos = 1000;
        float zPos = Random.Range(_minSpawnRange, _maxSpawnRange);

        Vector3 randomPos = new Vector3(xPos, yPos, zPos);

        return randomPos;
    }

    int GetRandomResource()
    {
        int randomResourceIndex = Random.Range(0, _resourcePrefabs.Length);
        return randomResourceIndex;
    }

    void GenerateResource(Vector3 position, Vector3 rotation, int resourceIndex)
    {
        GameObject spawnedResource = Instantiate(_resourcePrefabs[resourceIndex]);
        _resourceGameObjects.Add(spawnedResource);

        _resourceIndex.Add(resourceIndex);

        spawnedResource.transform.position = position;
        _resourcePositions.Add(position);

        spawnedResource.transform.rotation = Quaternion.FromToRotation(Vector3.up, rotation);
        _resourceRotations.Add(rotation);

        spawnedResource.GetComponent<ResourceVein>().resourceIndex = _resourceGameObjects.Count - 1;
    }

    void SpawnResource(Vector3 position, Vector3 rotation, int resourceIndex)
    {
        GameObject spawnedResource = Instantiate(_resourcePrefabs[resourceIndex]);
        _resourceGameObjects.Add(spawnedResource);

        spawnedResource.transform.position = position;

        spawnedResource.transform.rotation = Quaternion.FromToRotation(Vector3.up, rotation);

        spawnedResource.GetComponent<ResourceVein>().resourceIndex = _resourceGameObjects.Count - 1;
    }

    public void LoadData(GameData data)
    {
        _resourcePositions = data.resourcePositions;
        _resourceRotations = data.resourceRotations;
        _resourceIndex = data.resourceIndex;

        if (data.resourcePositions.Count != 0)
        {
            _hasLoadData = true;
        }
    }

    public void SaveData(GameData data)
    {
        data.resourcePositions = _resourcePositions;
        data.resourceRotations = _resourceRotations;
        data.resourceIndex = _resourceIndex;
    }
}