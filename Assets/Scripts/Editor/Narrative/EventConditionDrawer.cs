using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(EventCondition), true)]
public class EventConditionDrawer : PropertyDrawer
{
    private Type[] _conditionTypes;
    private string[] _conditionNames;
    private int _selectedIndex = -1;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var currentType = property.managedReferenceValue?.GetType();

        if (_conditionTypes == null)
        {
            _conditionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(EventCondition).IsAssignableFrom(t))
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();
            _conditionNames = _conditionTypes.Select(t => t.Name).ToArray();
        }

        if (currentType != null)
            _selectedIndex = Array.IndexOf(_conditionTypes, currentType);
        else
            _selectedIndex = -1;

        int newIndex = EditorGUI.Popup(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            label.text,
            _selectedIndex,
            _conditionNames
        );

        if (newIndex != _selectedIndex)
        {
            var newCondition = Activator.CreateInstance(_conditionTypes[newIndex]) as EventCondition;
            property.managedReferenceValue = newCondition;
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