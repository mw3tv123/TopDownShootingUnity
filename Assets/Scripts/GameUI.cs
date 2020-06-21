using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
    public Image gameOverScreen;
    public GameObject gameOverUI;

    // Start is called before the first frame update
    void Start () {
        FindObjectOfType<Player> ().OnDeath += OnGameOver;
    }

    private void OnGameOver () {
        StartCoroutine (Fade (Color.clear, Color.black, 1));
        gameOverUI.SetActive (true);
    }

    private IEnumerator Fade (Color from, Color to, float time) {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime * speed;
            gameOverScreen.color = Color.Lerp (from, to, percent);
            yield return null;
        }
    }

    public void StartNewGame () {
        SceneManager.LoadScene ("ShootingScene");
    }
}
