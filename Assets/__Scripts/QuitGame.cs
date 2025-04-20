using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Игра должна закрыться. В редакторе это не работает.");
    }
}