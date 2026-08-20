using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Infrastructure;

[CustomPropertyDrawer(typeof(ICommand), true)]
public class NarrativeCommandDrawer : PropertyDrawer
{
    private Type[] _commandTypes;
    private string[] _commandNames;
    private int _selectedIndex = -1;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var currentType = property.managedReferenceValue?.GetType();

        if (_commandTypes == null)
        {
            _commandTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(ICommand).IsAssignableFrom(t))
                .ToArray();
            _commandNames = _commandTypes.Select(t => t.Name).ToArray();
        }

        if (currentType != null)
            _selectedIndex = Array.IndexOf(_commandTypes, currentType);
        else
            _selectedIndex = -1;

        if (_commandNames.Length == 0)
        {
            EditorGUI.LabelField(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                label.text + " (no commands found)"
            );
        }
        else
        {
            int newIndex = EditorGUI.Popup(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                label.text,
                _selectedIndex,
                _commandNames
            );

            if (newIndex != _selectedIndex)
            {
                try
                {
                    var newCommand = Activator.CreateInstance(_commandTypes[newIndex]) as ICommand;
                    property.managedReferenceValue = newCommand;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Cannot create instance of {_commandTypes[newIndex].Name}: {ex.Message}");
                }
            }
        }

        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            SerializedObject serializedObject = property.serializedObject;
            SerializedProperty childProperty = property.Copy();
            if (childProperty.NextVisible(true))
            {
                do
                {
                    EditorGUILayout.PropertyField(childProperty, true);
                } while (childProperty.NextVisible(false));
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (property.managedReferenceValue != null)
        {
            var childProp = property.Copy();
            if (childProp.NextVisible(true))
            {
                do
                {
                    height += EditorGUIUtility.singleLineHeight;
                } while (childProp.NextVisible(false));
            }
        }
        return height;
    }
}