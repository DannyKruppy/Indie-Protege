using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Credits()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void HowtoPlay()
    {
        SceneManager.LoadSceneAsync(3);
    }

     public void ToBeImplemented()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void BacktoMain()
    {
        SceneManager.LoadSceneAsync(0);
    }

   


    
}