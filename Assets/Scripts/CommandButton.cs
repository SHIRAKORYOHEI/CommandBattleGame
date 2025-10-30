using UnityEngine;
using UnityEngine.UI;

public class CommandButton : MonoBehaviour
{
    public CommandType commandType;
    public UIManager uiManager;

    public void OnClick()
    {
        uiManager.OnCommandButtonPressed((int)commandType);
    }
}