using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 通用的 BaseAbility 列表编辑器绘制工具。
/// 用于 Editor 中对 [SerializeReference] List&lt;BaseAbility&gt; 进行多态增删和属性展示。
/// </summary>
public static class AbilityListDrawer
{
    private static List<Type> _cachedAbilityTypes;

    /// <summary>
    /// 绘制完整的 Ability 列表 UI（含折叠、删除、类型选择添加、多态属性绘制）。
    /// 调用方需要在 OnInspectorGUI 中自行调用 serializedObject.Update/ApplyModifiedProperties。
    /// </summary>
    /// <param name="listProp">目标 SerializeReference 列表的 SerializedProperty</param>
    /// <param name="serializedObject">所属 Editor 的 serializedObject，用于 ApplyModifiedProperties</param>
    /// <param name="label">列表标题，默认 "Abilities"</param>
    public static void DrawAbilityList(SerializedProperty listProp, SerializedObject serializedObject, string label = "Abilities")
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        for (int i = 0; i < listProp.arraySize; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            var elementType = element.managedReferenceValue?.GetType();
            string typeName = elementType != null ? ObjectNames.NicifyVariableName(elementType.Name) : "<Null>";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            // 折叠标题
            element.isExpanded = EditorGUILayout.Foldout(element.isExpanded,
                $"[{i}] {typeName}", true);

            // 删除按钮
            if (GUILayout.Button("×", GUILayout.Width(24)))
            {
                listProp.DeleteArrayElementAtIndex(i);
                // DeleteArrayElementAtIndex 会留下 null 引用，需要再次删除
                if (listProp.arraySize > i &&
                    listProp.GetArrayElementAtIndex(i).managedReferenceValue == null)
                {
                    listProp.DeleteArrayElementAtIndex(i);
                }
                break;
            }

            EditorGUILayout.EndHorizontal();

            // 展开时绘制内部属性
            if (element.isExpanded && element.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                DrawSerializedReference(element);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // 添加按钮 —— 弹出类型选择菜单
        if (GUILayout.Button($"+ Add {label.TrimEnd('s')}", GUILayout.Width(120)))
        {
            ShowAddAbilityMenu(listProp, serializedObject);
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 向列表添加一个指定类型的 Ability 实例。
    /// 调用方需要自行调用 serializedObject.ApplyModifiedProperties()。
    /// </summary>
    public static void AddAbility(SerializedProperty listProp, Type abilityType)
    {
        var instance = Activator.CreateInstance(abilityType);
        int index = listProp.arraySize;
        listProp.arraySize++;
        var element = listProp.GetArrayElementAtIndex(index);
        element.managedReferenceValue = instance;
    }

    /// <summary>
    /// 绘制 SerializeReference 元素的所有子属性。
    /// </summary>
    public static void DrawSerializedReference(SerializedProperty element)
    {
        var current = element.Copy();
        var end = element.GetEndProperty();

        if (current.NextVisible(true))
        {
            do
            {
                if (SerializedProperty.EqualContents(current, end))
                    break;

                EditorGUILayout.PropertyField(current, true);
            }
            while (current.NextVisible(false));
        }
    }

    /// <summary>
    /// 获取所有非抽象、非泛型的 BaseAbility 派生类型（带缓存）。
    /// </summary>
    public static List<Type> GetAbilityTypes()
    {
        if (_cachedAbilityTypes == null)
        {
            _cachedAbilityTypes = TypeCache.GetTypesDerivedFrom<BaseAbility>()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .ToList();

            if (_cachedAbilityTypes.Count == 0)
            {
                // 后备方案：扫描当前已加载的程序集
                _cachedAbilityTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Type.EmptyTypes; }
                    })
                    .Where(t => t.IsSubclassOf(typeof(BaseAbility)) && !t.IsAbstract)
                    .ToList();
            }
        }

        return _cachedAbilityTypes;
    }

    private static void ShowAddAbilityMenu(SerializedProperty listProp, SerializedObject serializedObject)
    {
        var types = GetAbilityTypes();
        var menu = new GenericMenu();

        foreach (var type in types)
        {
            var capturedType = type;
            menu.AddItem(
                new GUIContent(ObjectNames.NicifyVariableName(capturedType.Name)),
                false,
                () =>
                {
                    AddAbility(listProp, capturedType);
                    serializedObject.ApplyModifiedProperties();
                }
            );
        }

        menu.ShowAsContext();
    }
}
