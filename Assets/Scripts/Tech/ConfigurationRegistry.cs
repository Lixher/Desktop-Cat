using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ConfigurationRegistry", menuName = "ScreenCat/Configuration Registry", order = 1)]
public class ConfigurationRegistry : ScriptableObject
{
    public List<MonoBehaviour> configurableComponents = new List<MonoBehaviour>();
}