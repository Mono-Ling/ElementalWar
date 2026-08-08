using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 通用的 BaseElementReaction 列表编辑器绘制工具。
/// 用于 Editor 中对序列化的 List&lt;ElementReactionMap.ElementReactionEntry&gt; 进行 Key + 多态 Reaction 的增删和属性展示。
/// </summary>
public static class ElementReactionListDrawer
{
    private static List<Type> _cachedReactionTypes;

    /// <summary>
    /// 绘制完整的反应列表 UI（含 Key（EnumFlagsField）、折叠 Reaction、删除按钮、添加按钮）。
    /// 调用方需要在 OnInspectorGUI 中自行调用 serializedObject.Update / ApplyModifiedProperties。
    /// </summary>
    public static void DrawReactionList(SerializedProperty listProp, SerializedObject serializedObject)
    {
        EditorGUILayout.LabelField("Reactions", EditorStyles.boldLabel);

        for (int i = 0; i < listProp.arraySize; i++)
        {
            var entry = listProp.GetArrayElementAtIndex(i);
            var keyProp = entry.FindPropertyRelative("key");
            var reactionProp = entry.FindPropertyRelative("reaction");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Key 行：枚举 Flags 字段
            if (keyProp != null)
            {
                EditorGUILayout.PropertyField(keyProp, new GUIContent($"Key [{i}]"));
            }

            // Reaction 行：序列化引用 + 删除按钮
            if (reactionProp != null)
            {
                var reactionType = reactionProp.managedReferenceValue?.GetType();
                string typeName = reactionType != null ? ObjectNames.NicifyVariableName(reactionType.Name) : "<Null>";

                EditorGUILayout.BeginHorizontal();

                // 折叠标题
                reactionProp.isExpanded = EditorGUILayout.Foldout(reactionProp.isExpanded,
                    $"Reaction: {typeName}", true);

                // 删除按钮
                if (GUILayout.Button("×", GUILayout.Width(24)))
                {
                    listProp.DeleteArrayElementAtIndex(i);
                    // DeleteArrayElementAtIndex 会留下 null 引用，需要再次删除
                    if (listProp.arraySize > i &&
                        listProp.GetArrayElementAtIndex(i).FindPropertyRelative("reaction")?.managedReferenceValue == null)
                    {
                        listProp.DeleteArrayElementAtIndex(i);
                    }
                    break;
                }

                EditorGUILayout.EndHorizontal();

                // 展开时绘制多态属性
                if (reactionProp.isExpanded && reactionProp.managedReferenceValue != null)
                {
                    EditorGUI.indentLevel++;
                    DrawSerializedReference(reactionProp);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // 添加按钮 —— 弹出类型选择菜单
        if (GUILayout.Button("+ Add Reaction", GUILayout.Width(120)))
        {
            ShowAddReactionMenu(listProp, serializedObject);
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 向列表添加一个带有默认 Key 和指定反应类型的条目。
    /// </summary>
    public static void AddReaction(SerializedProperty listProp, Type reactionType)
    {
        int index = listProp.arraySize;
        listProp.arraySize++;

        var entry = listProp.GetArrayElementAtIndex(index);
        var reactionProp = entry.FindPropertyRelative("reaction");

        if (reactionProp != null)
        {
            var instance = Activator.CreateInstance(reactionType);
            reactionProp.managedReferenceValue = instance;
        }
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
    /// 获取所有非抽象、非泛型的 BaseElementReaction 派生类型（带缓存）。
    /// </summary>
    public static List<Type> GetReactionTypes()
    {
        if (_cachedReactionTypes == null)
        {
            _cachedReactionTypes = TypeCache.GetTypesDerivedFrom<BaseElementReaction>()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .ToList();

            if (_cachedReactionTypes.Count == 0)
            {
                // 后备方案：扫描当前已加载的程序集
                _cachedReactionTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Type.EmptyTypes; }
                    })
                    .Where(t => t.IsSubclassOf(typeof(BaseElementReaction)) && !t.IsAbstract)
                    .ToList();
            }
        }

        return _cachedReactionTypes;
    }

    private static void ShowAddReactionMenu(SerializedProperty listProp, SerializedObject serializedObject)
    {
        var types = GetReactionTypes();
        var menu = new GenericMenu();

        foreach (var type in types)
        {
            var capturedType = type;
            menu.AddItem(
                new GUIContent(ObjectNames.NicifyVariableName(capturedType.Name)),
                false,
                () =>
                {
                    AddReaction(listProp, capturedType);
                    serializedObject.ApplyModifiedProperties();
                }
            );
        }

        menu.ShowAsContext();
    }
}
