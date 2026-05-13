using UnityEngine;

public class ShipCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ShipCollision] Hit by {other.gameObject.name} (tag={other.tag})");

        if (other.CompareTag("Egg") || other.CompareTag("Chicken"))
        {
            Destroy(other.gameObject);

            if (ShipAgent.Instance != null)
            {
                ShipAgent.Instance.AddCustomReward(-1f);
                Debug.Log($"[ShipAgent] Episode {ShipAgent.Instance.EpisodeCount} ended with reward {ShipAgent.Instance.CurrentEpisodeReward:F2}");
                ShipAgent.Instance.EndEpisode();
            }

            // GameManager.Instance.EndGame(); // keep disabled during training
        }
    }
}