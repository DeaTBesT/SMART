using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnStartGameClick()
    {
        SceneManager.LoadScene(1);
    } 
}
