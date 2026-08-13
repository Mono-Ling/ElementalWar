using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ElementBuffMap))]
public class ElementBuffMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制除脚本引用和 Buff 列表外的所有属性
        DrawPropertiesExcluding(serializedObject, "m_Script", "_serializeList");

        EditorGUILayout.Space();

        // 绘制 Buff 列表（多态 BaseElementBuff）
        var listProp = serializedObject.FindProperty("_serializeList");
        if (listProp == null)
        {
            Debug.LogError("【元素Buff编辑器脚本】序列化字段获取失败");
            return;
        }
        ElementBuffListDrawer.DrawBuffList(listProp, serializedObject);

        serializedObject.ApplyModifiedProperties();
    }
}
