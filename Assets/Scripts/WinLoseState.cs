using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseState : MonoBehaviour
{
    public GameObject WinPanel;
    public GameObject LosePanel;

    public void WinPanelShow()
    {
        WinPanel.SetActive(true);
    }

    public void LosePanelShow()
    {
        LosePanel.SetActive(true);
    }


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
