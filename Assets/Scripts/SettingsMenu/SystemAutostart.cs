#if UNITY_STANDALONE_WIN
using Microsoft.Win32;
using System.Diagnostics; // <-- 1. ���������
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
            // ������������ ������:
            // 2. �������� UnityEngine.Application.executablePath �� ���������� �����
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            key.SetValue(AppName, "\"" + exePath + "\"");
            UnityEngine.Debug.Log("������������ ��� ������ Windows ��������.");
        }
        else
        {
            if (key.GetValue(AppName) != null)
            {
                key.DeleteValue(AppName);
                UnityEngine.Debug.Log("������������ ��� ������ Windows ���������.");
            }
        }
        key.Close();
    }
#else
    public static void SetAutostart(bool enabled)
    {
        UnityEngine.Debug.LogWarning("������� ����������� ��� ������ �� �������� ������ � Windows-������ ����.");
    }
#endif
}