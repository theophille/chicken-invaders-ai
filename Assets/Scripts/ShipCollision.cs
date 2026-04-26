using UnityEngine;

public class ShipCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.CompareTag("Egg") || other.CompareTag("Chicken"))
        {
            Destroy(gameObject);
            GameManager.Instance.EndGame();
        }
    }
}