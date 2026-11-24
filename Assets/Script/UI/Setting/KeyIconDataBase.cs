using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KeyIconDatabase", menuName = "Input/Key Icon Database")]
public class KeyIconDatabase : ScriptableObject
{
    [Serializable]
    public struct KeyIconEntry
    {
        public string controlPath;
        public Sprite icon;
    }

    [SerializeField] private List<KeyIconEntry> entries = new List<KeyIconEntry>();

    private Dictionary<string, Sprite> iconMap;

    public Sprite GetIcon(string controlPath)
    {
        if (string.IsNullOrEmpty(controlPath)) return null;

        if (iconMap == null)
        {
            iconMap = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.controlPath) && entry.icon != null)
                {
                    iconMap[entry.controlPath] = entry.icon;
                }
            }
        }

        return iconMap.TryGetValue(controlPath, out var sprite) ? sprite : null;
    }
}