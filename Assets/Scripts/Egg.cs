using UnityEngine;

public class Egg : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        Vector3 screenPos = _cam.WorldToViewportPoint(transform.position);
        if (screenPos.y < 0f)
        {
            Destroy(gameObject);
        }
    }
}