
using IniParser;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using IniData = IniParser.Model.IniData;
using Debug = UnityEngine.Debug;
using Application = UnityEngine.Application;

public class INIManager : MonoBehaviour
{
    public string tagToScan = "Configurable";

    void Awake()
    {
        if (GameObject.FindObjectsOfType<INIManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        ProcessTaggedObjects();
    }

    public void ProcessTaggedObjects()
    {
        string settingsFilePath = Path.Combine(Application.dataPath, "..", "settings.ini");
        FileIniDataParser parser = new FileIniDataParser();
        IniData data = File.Exists(settingsFilePath) ? parser.ReadFile(settingsFilePath) : new IniData();
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tagToScan);
        if (taggedObjects.Length == 0)
        {
            return;
        }
        foreach (var obj in taggedObjects)
        {
            MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                ProcessComponent(component, data);
            }
        }
        parser.WriteFile(settingsFilePath, data);
    }

    private void ProcessComponent(MonoBehaviour component, IniData data)
    {
        if (component == null) return;

        var componentType = component.GetType();
        string section = componentType.Name;

        if (!data.Sections.ContainsSection(section))
        {
            data.Sections.AddSection(section);
        }

        var fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null) continue;
            if (!IsSupportedType(field.FieldType)) continue;

            string key = field.Name;

            if (data[section][key] != null)
            {
                string stringValue = data[section][key];
                try
                {
                    var convertedValue = Convert.ChangeType(stringValue, field.FieldType);
                    field.SetValue(component, convertedValue);
                }
                catch { }
            }
            else
            {
                data[section][key] = field.GetValue(component).ToString();
            }
        }
    }

    private bool IsSupportedType(Type type) => type.IsPrimitive || type == typeof(string) || type.IsEnum;
}