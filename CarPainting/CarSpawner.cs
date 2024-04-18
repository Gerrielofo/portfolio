using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    public GameObject[] possibleCars;

    public static GameObject currentCar;

    private void Start()
    {
        currentCar = null;
    }

    public void SpawnCar()
    {
        currentCar = Instantiate(possibleCars[Random.Range(0, possibleCars.Length)], spawnPoint.position, spawnPoint.rotation);
    }
}
