using System.Runtime.InteropServices;
using UnityEngine;

public class ClipboardManager : MonoBehaviour
{
    public static void CopyToClipboard(string text)
    {
        #if UNITY_EDITOR
        
        #else
        CopyToClipboardJS(text);
        #endif
    }

    [DllImport("__Internal")]
    private static extern void CopyToClipboardJS(string text);
}