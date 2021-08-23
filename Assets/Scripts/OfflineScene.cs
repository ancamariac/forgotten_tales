using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OfflineScene : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("CharacterSelection");
    }
}
