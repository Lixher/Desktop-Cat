#if UNITY_STANDALONE_WIN
using Microsoft.Win32;
using System.Diagnostics; // <-- 1. ÄÎÁÀÂËÅÍÎ
#endif
using UnityEngine;

public static class SystemAutostart
{
    private const string AppName = "ScreenCatGame";

#if UNITY_STANDALONE_WIN
    private static readonly string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public static void SetAutostart(bool enabled)
    {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);

        if (enabled)
        {
            // ÈÑÏÐÀÂËÅÍÍÀß ÑÒÐÎÊÀ:
            // 2. ÇÀÌÅÍÅÍÎ UnityEngine.Application.executablePath íà ïðàâèëüíûé ìåòîä
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            key.SetValue(AppName, "\"" + exePath + "\"");
            UnityEngine.Debug.Log("Àâòîçàãðóçêà ïðè ñòàðòå Windows ÂÊËÞ×ÅÍÀ.");
        }
        else
        {
            if (key.GetValue(AppName) != null)
            {
                key.DeleteValue(AppName);
                UnityEngine.Debug.Log("Àâòîçàãðóçêà ïðè ñòàðòå Windows ÂÛÊËÞ×ÅÍÀ.");
            }
        }
        key.Close();
    }
#else
    public static void SetAutostart(bool enabled)
    {
        UnityEngine.Debug.LogWarning("Ôóíêöèÿ àâòîçàïóñêà ïðè ñòàðòå ÎÑ ðàáîòàåò òîëüêî â Windows-âåðñèè èãðû.");
    }
#endif
}