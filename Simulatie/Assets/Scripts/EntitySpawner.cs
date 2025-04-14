using UnityEngine;
using System.Collections;

public class EntitySpawner : MonoBehaviour
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created

  public GameObject carPrefab;
  public Transform spawnPoint;

  void Start()
  {
    SpawnCar();
  }

    // Update is called once per frame
  void Update()
  {

  }

  void SpawnCar()
  {
    Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
  }
}
