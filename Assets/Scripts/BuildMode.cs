using UnityEngine;

public class BuildMode : MonoBehaviour
{
    public static bool Active;   // true = click dùng để đặt/kéo; false = click để move player
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) Active = !Active; // B để bật/tắt
    }
}