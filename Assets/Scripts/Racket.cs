using UnityEngine;

public class Racket : MonoBehaviour
{
    [SerializeField] private float zDepth = 10f;
    [SerializeField] private GameObject bulletPrefab;

    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        Vector3 mouseScreen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDepth);
        Vector3 worldPos = _cam.ScreenToWorldPoint(mouseScreen);
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        Instantiate(bulletPrefab, transform.position, Quaternion.identity);
    }
}