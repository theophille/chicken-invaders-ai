using UnityEngine;

public class Chicken : MonoBehaviour
{
    [SerializeField] private int maxHealth = 2;
    private int _currentHealth;

    private int _col;
    private float _gap;
    private float _rowBaseX;
    private float _direction;
    private float _moveDistance;
    private float _moveSpeed;
    private float _homeY;

    [SerializeField] private float attackerSpeed = 4f;
    [SerializeField] private float attackDuration = 3f;

    private Transform _ship;
    private bool _isAttacker = false;
    private bool _initialized = false;

    void Start()
    {
        _currentHealth = maxHealth;
    }

    void Update()
    {
        if (!_initialized) return;

        if (_isAttacker)
            UpdateAttacker();
        else
            UpdateMatrix();
    }

    public void Init(int col, float gap, float rowBaseX, float direction,
        float moveDistance, float moveSpeed, float homeY, Transform ship)
    {
        _col = col;
        _gap = gap;
        _rowBaseX = rowBaseX;
        _direction = direction;
        _moveDistance = moveDistance;
        _moveSpeed = moveSpeed;
        _homeY = homeY;
        _ship = ship;
        _initialized = true;
    }

    void UpdateMatrix()
    {
        float offsetX = _direction * _moveDistance * Mathf.Sin(Time.time * _moveSpeed);
        float targetX = _rowBaseX + _col * _gap + offsetX;

        transform.position = new Vector3(targetX, _homeY, transform.position.z);
    }

    void UpdateAttacker()
    {
        if (_ship == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            _ship.position,
            attackerSpeed * Time.deltaTime
        );
    }

    public void BecomeAttacker()
    {
        if (_isAttacker) return;
        StartCoroutine(AttackerRoutine());
    }

    private System.Collections.IEnumerator AttackerRoutine()
    {
        _isAttacker = true;

        yield return new WaitForSeconds(attackDuration);
        yield return ReturnToMatrix();

        _isAttacker = false;
    }

    private System.Collections.IEnumerator ReturnToMatrix()
    {
        float elapsed = 0f;
        float returnDuration = 1.2f;

        Vector3 startPos = transform.position;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;

            float offsetX = _direction * _moveDistance * Mathf.Sin(Time.time * _moveSpeed);
            float targetX = _rowBaseX + _col * _gap + offsetX;
            Vector3 matrixPos = new Vector3(targetX, _homeY, transform.position.z);

            transform.position = Vector3.Lerp(startPos, matrixPos, t);
            yield return null;
        }
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            if (ShipAgent.Instance != null)
            {
                ShipAgent.Instance.AddCustomReward(+1f);
                Debug.Log($"[ShipAgent] Chicken killed. EpisodeReward={ShipAgent.Instance.CurrentEpisodeReward:F2}");
            }

            Destroy(gameObject);
            MatrixSpawner.Instance?.NotifyChickenDead(); // notify spawner after destroy
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ship"))
        {
            Destroy(gameObject);
            MatrixSpawner.Instance?.NotifyChickenDead(); // also notify here
        }
    }
}