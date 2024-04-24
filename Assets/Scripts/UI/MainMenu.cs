using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject optionsPanel;
    private bool optionsActive = false;

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // All of these examples loads "Level_"
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleOptions()
    {
        optionsActive = !optionsActive;
        optionsPanel.SetActive(optionsActive);
    }

}
