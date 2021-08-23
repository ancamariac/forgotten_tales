using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSettings : MonoBehaviour
{
    private GameObject settings;

    private void Start()
    {
        settings = GameObject.Find("Canvas").transform
            .Find("Settings").gameObject;
    }

    public void Exit()
    {
        settings.SetActive(false);
    }
}
