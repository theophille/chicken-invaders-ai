using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 1;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * 0.02f);

        Vector3 screenPos = cam.WorldToViewportPoint(transform.position);
        if (screenPos.y > 1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Chicken chicken = other.GetComponent<Chicken>();

        if (chicken != null)
        {
            Debug.Log("A chicken was hit!");
            chicken.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}