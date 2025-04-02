using UnityEngine;

public class Road : MonoBehaviour
{
    void Start()
    {
        EnsureBoxColliderSize();
  }

    
    void Update()
    {
        
    }

    private void EnsureBoxColliderSize()
    {
      SpriteRenderer sr = GetComponent<SpriteRenderer>();
      BoxCollider2D col = GetComponent<BoxCollider2D>();

      if (col != null) col.size = sr.size;
    }
}
