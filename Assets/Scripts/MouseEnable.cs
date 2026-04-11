using UnityEngine;
using UnityEngine.InputSystem;

public class MouseEnable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Application.Quit();
    }
}
