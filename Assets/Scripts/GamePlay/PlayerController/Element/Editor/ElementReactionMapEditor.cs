using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ElementReactionMap))]
public class ElementReactionMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制除脚本引用和反应列表外的所有属性
        DrawPropertiesExcluding(serializedObject, "m_Script", "_serializedReactions");

        EditorGUILayout.Space();

        // 绘制反应列表（Key 枚举 + 多态 Reaction）
        var listProp = serializedObject.FindProperty("_serializedReactions");
        ElementReactionListDrawer.DrawReactionList(listProp, serializedObject);

        serializedObject.ApplyModifiedProperties();
    }
}
