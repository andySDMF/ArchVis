using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationLoadScene : MonoBehaviour
{
    [SerializeField]
    private int sceneIndex = 0;

    public void LoadLevel()
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
