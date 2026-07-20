using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Data;
using GenomeEffects;

[CustomEditor(typeof(GenomePropertyData))]
public class GenomePropertyDataEditor : Editor
{
    private string[] _effectNames;
    private int _selectedIndex;

    private void OnEnable()
    {
        // Получаем все типы, наследующие GenomeEffectBase
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(GenomeEffectBase).IsAssignableFrom(t))
            .ToList();

        _effectNames = types.Select(t => t.Name).ToArray();
        // Если есть пустая строка, добавляем её для "None"
        var list = new List<string> { "None" };
        list.AddRange(_effectNames);
        _effectNames = list.ToArray();

        // Определяем текущий выбранный индекс
        var prop = (GenomePropertyData)target;
        int idx = Array.IndexOf(_effectNames, prop.EffectClassName);
        _selectedIndex = idx < 0 ? 0 : idx;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var prop = (GenomePropertyData)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effect Selection", EditorStyles.boldLabel);

        int newIndex = EditorGUILayout.Popup("Effect Class", _selectedIndex, _effectNames);
        if (newIndex != _selectedIndex)
        {
            _selectedIndex = newIndex;
            string selectedName = _effectNames[newIndex];
            if (selectedName == "None") selectedName = "";
            prop.SetEffectClassName(selectedName);
            EditorUtility.SetDirty(prop);
            AssetDatabase.SaveAssets();
        }
    }
}