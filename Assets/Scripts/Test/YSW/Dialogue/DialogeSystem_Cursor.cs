using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogeSystem_Cursor : MonoBehaviour
{
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
