using HelperScripts.EventSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton
    private static GameManager instance;
    public static GameManager Instance { get => instance; }

    private int score;

    [SerializeField] GameOverScreen gameOverScreen;
    [SerializeField] EventScriptable onPiecePlaced;

    [SerializeField] private UnityEvent playGameOver, StopSound;
    [SerializeField] private BoolVariable isPlayerActive;
    [SerializeField] private FloatVariable happiness;
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject ghost;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("GameScene"))
        {
            isPlayerActive.SetValue(true);
        }else
            ui.SetActive(false);
        isPlayerActive.OnValueChanged.AddListener(StartGame);

        Grid3DManager.Instance.onBalanceBroken.AddListener(GameOver);
        onPiecePlaced.AddListener(IncreaseScore);

        Cursor.lockState = CursorLockMode.None;
    }

    public void StartGame()
    {
        ui.SetActive(isPlayerActive);
    }

    public void PauseGame()
    {
        isPlayerActive.SetValue(false);
        ghost.SetActive(isPlayerActive);
        ui.SetActive(isPlayerActive);
        pauseMenu.SetActive(true);
        //Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        isPlayerActive.SetValue(true);
        ghost.SetActive(isPlayerActive);
        ui.SetActive(isPlayerActive);
    }

    void GameOver()
    {
        StartCoroutine(endgame());
    }

    IEnumerator endgame() 
    {
        float animTime = Grid3DManager.Instance.GetComponent<TowerLeaningFeedback>().ShaderAnimTime + 0.37f ;
        yield return new WaitForSeconds(animTime);
        StopSound.Invoke();
        isPlayerActive.SetValue(false);
        ghost.SetActive(isPlayerActive);
        yield return new WaitForSeconds(5 - animTime);
        playGameOver.Invoke();
        var go = Instantiate(gameOverScreen);
        go.SetScore(score);
        go.SetHappiness(happiness);
    }

    private void OnDestroy()
    {
        Grid3DManager.Instance.onBalanceBroken.RemoveListener(GameOver);
        onPiecePlaced.RemoveListener(IncreaseScore);
    }

    private void IncreaseScore()
    {
        score++;
    }
}
