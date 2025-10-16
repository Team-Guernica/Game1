using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public AudioSource bgmAudioSource; // 배경음악 오디오 소스 참조

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        if (bgmAudioSource != null)
            bgmAudioSource.UnPause();
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        if (bgmAudioSource != null)
            bgmAudioSource.Pause();
    }

    public void GameSave()
    {
        Debug.Log("세이브 화면을 띄웁니다. (미구현)");
        // SceneManager.LoadScene("save menu");
    }

    public void GameOption()
    {
        Debug.Log("볼륨, 밝기 조정 등 옵션 화면을 띄웁니다. (미구현)");
        // SceneManager.LoadScene("option menu");
    }

    public void GameExit()
    {
        Debug.Log("게임을 나갑니다. (미구현)");
        // Application.Quit();
    }
    
}
