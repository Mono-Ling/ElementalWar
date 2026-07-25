using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OtherPlayer))]
public class OtherPlayerEditor : Editor
{
    private static List<Type> _cachedSynReceiveTypes;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制 playerController 字段
        // var playerControllerProp = serializedObject.FindProperty("playerController");
        // EditorGUILayout.PropertyField(playerControllerProp);

        // EditorGUILayout.Space();

        // 绘制 stateSynReceiveList 列表（SerializeReference 多态支持）
        var synReceiveListProp = serializedObject.FindProperty("stateSynReceiveList");
        DrawSynReceiveList(synReceiveListProp);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSynReceiveList(SerializedProperty listProp)
    {
        EditorGUILayout.LabelField("State Syn Receives", EditorStyles.boldLabel);

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
        if (GUILayout.Button("+ Add SynReceive", GUILayout.Width(140)))
        {
            ShowAddSynReceiveMenu(listProp);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ShowAddSynReceiveMenu(SerializedProperty listProp)
    {
        var types = GetSynReceiveTypes();
        var menu = new GenericMenu();

        foreach (var type in types)
        {
            var capturedType = type;
            menu.AddItem(
                new GUIContent(ObjectNames.NicifyVariableName(capturedType.Name)),
                false,
                () => AddSynReceive(listProp, capturedType)
            );
        }

        menu.ShowAsContext();
    }

    private void AddSynReceive(SerializedProperty listProp, Type synReceiveType)
    {
        var instance = Activator.CreateInstance(synReceiveType);
        int index = listProp.arraySize;
        listProp.arraySize++;
        var element = listProp.GetArrayElementAtIndex(index);
        element.managedReferenceValue = instance;
        serializedObject.ApplyModifiedProperties();
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

    private static List<Type> GetSynReceiveTypes()
    {
        if (_cachedSynReceiveTypes == null)
        {
            _cachedSynReceiveTypes = TypeCache.GetTypesDerivedFrom<BaseSynReceive>()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .ToList();

            if (_cachedSynReceiveTypes.Count == 0)
            {
                // 后备方案：扫描当前已加载的程序集
                _cachedSynReceiveTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Type.EmptyTypes; }
                    })
                    .Where(t => t.IsSubclassOf(typeof(BaseSynReceive)) && !t.IsAbstract)
                    .ToList();
            }
        }

        return _cachedSynReceiveTypes;
    }
}
