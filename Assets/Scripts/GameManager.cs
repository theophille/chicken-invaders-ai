using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool _gameOver = false;

    void Awake()
    {
        Instance = this;
    }

    public void EndGame()
    {
        if (_gameOver) return;
        _gameOver = true;

        Debug.Log("GAME OVER");

        Time.timeScale = 0f;
    }
}