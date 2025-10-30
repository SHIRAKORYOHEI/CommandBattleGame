using UnityEngine;

public class TargetButton : MonoBehaviour
{
    public int targetIndex;
    public UIManager uIManager;

    public void OnClick()
    {
        uIManager.OnTargetSelected(targetIndex);
    }
}