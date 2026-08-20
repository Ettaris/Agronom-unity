using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(NarrativeSequence))]
public class NarrativeSequenceEditor : Editor
{
    private SerializedProperty _idProp;
    private SerializedProperty _stepsProp;

    private Type[] _stepTypes;
    private string[] _stepTypeNames;
    private int _selectedTypeIndex;

    private void OnEnable()
    {
        _idProp = serializedObject.FindProperty("sequenceId");
        _stepsProp = serializedObject.FindProperty("steps");

        _stepTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(NarrativeStep).IsAssignableFrom(t))
            .ToArray();

        _stepTypeNames = _stepTypes.Select(t => t.Name).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_idProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Steps", EditorStyles.boldLabel);

        // Отображение существующих шагов
        for (int i = 0; i < _stepsProp.arraySize; i++)
        {
            var stepProp = _stepsProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();

            // Стандартное отображение с выбором типа
            EditorGUILayout.PropertyField(stepProp, new GUIContent($"Step {i}"), true);

            // Кнопки перемещения
            if (i > 0)
            {
                if (GUILayout.Button("▲", GUILayout.Width(20)))
                {
                    MoveStep(i, i - 1);
                }
            }
            else
            {
                GUILayout.Space(22);
            }

            if (i < _stepsProp.arraySize - 1)
            {
                if (GUILayout.Button("▼", GUILayout.Width(20)))
                {
                    MoveStep(i, i + 1);
                }
            }
            else
            {
                GUILayout.Space(22);
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                _stepsProp.DeleteArrayElementAtIndex(i);
                // После удаления перерисовываем
                serializedObject.ApplyModifiedProperties();
                Repaint();
                return;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (_stepTypeNames.Length > 0)
        {
            _selectedTypeIndex = EditorGUILayout.Popup("Add Step", _selectedTypeIndex, _stepTypeNames);
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                var newStep = Activator.CreateInstance(_stepTypes[_selectedTypeIndex]) as NarrativeStep;
                if (newStep != null)
                {
                    int newIndex = _stepsProp.arraySize;
                    _stepsProp.InsertArrayElementAtIndex(newIndex);
                    var newProp = _stepsProp.GetArrayElementAtIndex(newIndex);
                    newProp.managedReferenceValue = newStep;
                    serializedObject.ApplyModifiedProperties();
                    Repaint();
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("No step types found.");
        }

        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void MoveStep(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || toIndex < 0 || fromIndex >= _stepsProp.arraySize || toIndex >= _stepsProp.arraySize)
            return;

        // Создаём временный список для перестановки
        var stepList = new List<NarrativeStep>();
        for (int i = 0; i < _stepsProp.arraySize; i++)
        {
            var prop = _stepsProp.GetArrayElementAtIndex(i);
            stepList.Add(prop.managedReferenceValue as NarrativeStep);
        }

        // Меняем местами
        var temp = stepList[fromIndex];
        stepList[fromIndex] = stepList[toIndex];
        stepList[toIndex] = temp;

        // Очищаем массив и перезаполняем
        _stepsProp.ClearArray();
        for (int i = 0; i < stepList.Count; i++)
        {
            _stepsProp.InsertArrayElementAtIndex(i);
            var newProp = _stepsProp.GetArrayElementAtIndex(i);
            newProp.managedReferenceValue = stepList[i];
        }

        serializedObject.ApplyModifiedProperties();
        Repaint();
    }
}