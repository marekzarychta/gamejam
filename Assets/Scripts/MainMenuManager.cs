using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button quitButton;

    public void OnClicked_Start()
    { 
        SceneManager.LoadScene(1);
    }

    public void OnClicked_Quit()
    { 
        Application.Quit();
    }

}
