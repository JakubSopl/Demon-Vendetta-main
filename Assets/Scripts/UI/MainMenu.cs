using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private CharacterController characterController;

    [SerializeField] GameObject optionsPanel;
    private bool optionsActive = false;

    [SerializeField] Image crosshairMenu;

    [SerializeField] Slider colorSlider;

    public Color crosshairColor;


    private void Start()
    {
        colorSlider.onValueChanged.AddListener(ColorSliderChanged);
    }



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

    #region - Settings -

    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    private void ColorSliderChanged(float value)
    {
        Color newColor;

        if (value <= 0.5f) // From white to full color spectrum
        {
            // Scale the value to range from 0 to 1 within this half of the slider
            float adjustedValue = value * 2;
            // Convert to color where saturation increases from 0 to 1 and value is always 1
            newColor = Color.HSVToRGB(adjustedValue, adjustedValue, 1f);
        }
        else // From full color spectrum to black
        {
            // Scale the value to range from 0 to 1 within this half of the slider
            float adjustedValue = (value - 0.5f) * 2;
            // Convert to color where hue is fixed, saturation is 1, and value decreases from 1 to 0
            newColor = Color.HSVToRGB(1f, 1f, 1 - adjustedValue);
        }

        crosshairColor = newColor;
        crosshairMenu.color = crosshairColor; // Apply the new color
    }


    #endregion
}
