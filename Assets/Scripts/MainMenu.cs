using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private GameObject settings;

    private void Start()
    {
        settings = GameObject.Find("Canvas").transform
            .Find("Settings").gameObject;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    public void Settings()
    {
        settings.SetActive(true);
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void VisitWebsite()
    {
        Application.OpenURL("https://opreaolivia73.wixsite.com/rpgeeks3/meet-the-team");
    }
}
