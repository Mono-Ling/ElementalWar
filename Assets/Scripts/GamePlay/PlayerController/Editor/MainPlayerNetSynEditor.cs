using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MainPlayerNetSyn))]
public class MainPlayerNetSynEditor : Editor
{
    private static List<Type> _cachedSynSendTypes;
    private static List<Type> _cachedFeedbackReceiveTypes;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制 stateSynSends 列表
        var synSendsProp = serializedObject.FindProperty("stateSynSends");
        DrawSynSendList(synSendsProp);

        EditorGUILayout.Space();

        // 绘制 feedbackReceives 列表
        var feedbackReceivesProp = serializedObject.FindProperty("feedbackReceives");
        DrawFeedbackReceiveList(feedbackReceivesProp);

        serializedObject.ApplyModifiedProperties();
    }

    // ==================== SynSend 列表绘制 ====================

    private void DrawSynSendList(SerializedProperty listProp)
    {
        EditorGUILayout.LabelField("State Syn Sends", EditorStyles.boldLabel);

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
        if (GUILayout.Button("+ Add SynSend", GUILayout.Width(120)))
        {
            ShowAddSynSendMenu(listProp);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ShowAddSynSendMenu(SerializedProperty listProp)
    {
        var types = GetSynSendTypes();
        var menu = new GenericMenu();

        foreach (var type in types)
        {
            var capturedType = type;
            menu.AddItem(
                new GUIContent(ObjectNames.NicifyVariableName(capturedType.Name)),
                false,
                () => AddSynSend(listProp, capturedType)
            );
        }

        menu.ShowAsContext();
    }

    private void AddSynSend(SerializedProperty listProp, Type synSendType)
    {
        var instance = Activator.CreateInstance(synSendType);
        int index = listProp.arraySize;
        listProp.arraySize++;
        var element = listProp.GetArrayElementAtIndex(index);
        element.managedReferenceValue = instance;
        serializedObject.ApplyModifiedProperties();
    }

    private static List<Type> GetSynSendTypes()
    {
        if (_cachedSynSendTypes == null)
        {
            _cachedSynSendTypes = TypeCache.GetTypesDerivedFrom<BaseSynSend>()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .ToList();

            if (_cachedSynSendTypes.Count == 0)
            {
                // 后备方案：扫描当前已加载的程序集
                _cachedSynSendTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Type.EmptyTypes; }
                    })
                    .Where(t => t.IsSubclassOf(typeof(BaseSynSend)) && !t.IsAbstract)
                    .ToList();
            }
        }

        return _cachedSynSendTypes;
    }

    private void DrawSerializedReference(SerializedProperty element)
    {
        // 遍历子属性并绘制
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

    // ==================== FeedbackReceive 列表绘制 ====================

    private void DrawFeedbackReceiveList(SerializedProperty listProp)
    {
        EditorGUILayout.LabelField("Feedback Receives", EditorStyles.boldLabel);

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
        if (GUILayout.Button("+ Add FeedbackReceive", GUILayout.Width(160)))
        {
            ShowAddFeedbackReceiveMenu(listProp);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ShowAddFeedbackReceiveMenu(SerializedProperty listProp)
    {
        var types = GetFeedbackReceiveTypes();
        var menu = new GenericMenu();

        foreach (var type in types)
        {
            var capturedType = type;
            menu.AddItem(
                new GUIContent(ObjectNames.NicifyVariableName(capturedType.Name)),
                false,
                () => AddFeedbackReceive(listProp, capturedType)
            );
        }

        menu.ShowAsContext();
    }

    private void AddFeedbackReceive(SerializedProperty listProp, Type feedbackReceiveType)
    {
        var instance = Activator.CreateInstance(feedbackReceiveType);
        int index = listProp.arraySize;
        listProp.arraySize++;
        var element = listProp.GetArrayElementAtIndex(index);
        element.managedReferenceValue = instance;
        serializedObject.ApplyModifiedProperties();
    }

    private static List<Type> GetFeedbackReceiveTypes()
    {
        if (_cachedFeedbackReceiveTypes == null)
        {
            _cachedFeedbackReceiveTypes = TypeCache.GetTypesDerivedFrom<BaseFeedbackReceive>()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .ToList();

            if (_cachedFeedbackReceiveTypes.Count == 0)
            {
                // 后备方案：扫描当前已加载的程序集
                _cachedFeedbackReceiveTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Type.EmptyTypes; }
                    })
                    .Where(t => t.IsSubclassOf(typeof(BaseFeedbackReceive)) && !t.IsAbstract)
                    .ToList();
            }
        }

        return _cachedFeedbackReceiveTypes;
    }
}
