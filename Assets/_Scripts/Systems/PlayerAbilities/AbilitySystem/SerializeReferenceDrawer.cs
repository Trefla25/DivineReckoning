#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AbilityEffect), true)]
[CustomPropertyDrawer(typeof(TargetingStrategy), true)]
public class SerializeReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        var bodyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight,
                                position.width, position.height - EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginProperty(position, label, property);

        string current = property.managedReferenceFullTypename;
        string display = ShortName(current) ?? "Select Type";

        if (EditorGUI.DropdownButton(typeRect, new GUIContent(display), FocusType.Keyboard))
        {
            var menu = new GenericMenu();
            foreach (var kvp in TypeMap(BaseType(property)))
            {
                Type type = kvp.Value;
                menu.AddItem(new GUIContent(kvp.Key), type.FullName == ShortName(current), () =>
                {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(bodyRect, property, GUIContent.none, true);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight;

    static Type BaseType(SerializedProperty property)
    {
        // managedReferenceFieldTypename = "AssemblyName Namespace.TypeName"
        var parts = property.managedReferenceFieldTypename.Split(' ');
        return parts.Length == 2 ? Type.GetType($"{parts[1]}, {parts[0]}") : typeof(object);
    }

    static Dictionary<string, Type> TypeMap(Type baseType) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
            .ToDictionary(t => ObjectNames.NicifyVariableName(t.Name), t => t);

    static string ShortName(string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName)) return null;
        var parts = fullTypeName.Split(' ');
        return parts.Length > 1 ? parts[1].Split('.').Last() : fullTypeName;
    }
}
#endif