using UnityEngine;
using System.Runtime.InteropServices; // This is important

public class WebGLInputManager : MonoBehaviour
{
    // This tells Unity that the "RequestKeyboardCapture" function exists in an external JavaScript library.
    [DllImport("__Internal")]
    private static extern void RequestKeyboardCapture();

    void Start()
    {
        // This #if block ensures this code ONLY runs in a WebGL build.
        // It does nothing in the Unity Editor.
#if !UNITY_EDITOR && UNITY_WEBGL
        RequestKeyboardCapture();
#endif
    }
}
