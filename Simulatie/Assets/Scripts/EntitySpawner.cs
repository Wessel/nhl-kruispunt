using UnityEngine;
using System.Collections;

public class EntitySpawner : MonoBehaviour
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created

  public GameObject entityPrefab;
  public Transform spawnPoint;
  public Transform parentTransform;

  void Start()
  {
    SpawnEntity();
  }

    // Update is called once per frame
  void Update()
  {

  }

  void SpawnEntity()
  {
    GameObject newEntity = Instantiate(entityPrefab, spawnPoint.position, spawnPoint.rotation, parentTransform);
  }
}
