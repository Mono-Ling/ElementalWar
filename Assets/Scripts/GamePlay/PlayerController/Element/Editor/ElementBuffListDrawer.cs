using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 通用的 BaseElementBuff 列表编辑器绘制工具。
/// 用于 Editor 中对序列化的 List&lt;BaseElementBuff&gt; 进行多态 Buff 的增删和属性展示。
/// </summary>
public static class ElementBuffListDrawer
{
    private static List<Type> _cachedBuffTypes;

    /// <summary>
    /// 绘制完整的 Buff 列表 UI（折叠 Buff、删除按钮、添加按钮）。
    /// 调用方需要在 OnInspectorGUI 中自行调用 serializedObject.Update / ApplyModifiedProperties。
    /// </summary>
    public static void DrawBuffList(SerializedProperty listProp, SerializedObject serializedObject)
    {
        if (listProp == null)
        {
            Debug.LogError("【元素Buff注册表绘制器】序列化字段获取失败");
            return;
        }
        EditorGUILayout.LabelField("Buffs", EditorStyles.boldLabel);

        for (int i = 0; i < listProp.arraySize; i++)
        {
            var buffProp = listProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var buffType = buffProp.managedReferenceValue?.GetType();
            string typeName = buffType != null ? ObjectNames.NicifyVariableName(buffType.Name) : "<Null>";

            EditorGUILayout.BeginHorizontal();

            // 折叠标题
            buffProp.isExpanded = EditorGUILayout.Foldout(buffProp.isExpanded,
                $"[{i}] {typeName}", true);

            // 删除按钮
            if (GUILayout.Button("×", GUILayout.Width(24)))
            {
                listProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndHorizontal();

            // 展开时绘制多态属性
            if (buffProp.isExpanded && buffProp.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                // 复用 Reaction 列表绘制工具中的通用 SerializeReference 属性绘制
                ElementReactionListDrawer.DrawSerializedReference(buffProp);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // 添加按钮 —— 弹出类型选择菜单
        if (GUILayout.Button("+ Add Buff", GUILayout.Width(120)))
        {
            ShowAddBuffMenu(listProp, serializedObject);
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 向列表添加一个指定类型的 Buff 实例。
    /// </summary>
    public static void AddBuff(SerializedProperty listProp, Type buffType)
    {
        int index = listProp.arraySize;
        listProp.arraySize++;

        var buffProp = listProp.GetArrayElementAtIndex(index);
        buffProp.managedReferenceValue = Activator.CreateInstance(buffType);
    }

    /// <summary>
    /// 获取所有非抽象、非泛型的 BaseElementBuff 派生类型（带缓存）。
    /// </summary>
    public static List<Type> GetBuffTypes()
    {
        if (_cachedBuffTypes == null)
        {
            _cachedBuffTypes = TypeCache.GetTypesDerivedFrom<BaseElementBuff>()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .ToList();

            if (_cachedBuffTypes.Count == 0)
            {
                // 后备方案：扫描当前已加载的程序集
                _cachedBuffTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Type.EmptyTypes; }
                    })
                    .Where(t => t.IsSubclassOf(typeof(BaseElementBuff)) && !t.IsAbstract)
                    .ToList();
            }
        }

        return _cachedBuffTypes;
    }

    /// <summary>
    /// 获取列表中已存在的 Buff 类型（用于添加菜单中置灰重复项）。
    /// </summary>
    private static HashSet<Type> GetExistingTypes(SerializedProperty listProp)
    {
        var existing = new HashSet<Type>();
        for (int i = 0; i < listProp.arraySize; i++)
        {
            var type = listProp.GetArrayElementAtIndex(i).managedReferenceValue?.GetType();
            if (type != null)
                existing.Add(type);
        }
        return existing;
    }

    private static void ShowAddBuffMenu(SerializedProperty listProp, SerializedObject serializedObject)
    {
        var types = GetBuffTypes();
        var existing = GetExistingTypes(listProp);
        var menu = new GenericMenu();

        foreach (var type in types)
        {
            var capturedType = type;
            // 列表中已存在的类型置灰，避免重复注册（Buff 注册表以类型为 Key）
            menu.AddItem(
                new GUIContent(ObjectNames.NicifyVariableName(capturedType.Name)),
                !existing.Contains(capturedType),
                () =>
                {
                    AddBuff(listProp, capturedType);
                    serializedObject.ApplyModifiedProperties();
                }
            );
        }

        menu.ShowAsContext();
    }
}
