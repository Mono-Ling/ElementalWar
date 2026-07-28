using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbilitySystemState))]
public class AbilitySystemStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制基类 State 的 edgeList 属性（由父类定义）
        DrawPropertiesExcluding(serializedObject, "m_Script", "abilities");

        EditorGUILayout.Space();

        // 绘制 abilities 列表（SerializeReference 多态支持）
        var abilitiesProp = serializedObject.FindProperty("abilities");
        AbilityListDrawer.DrawAbilityList(abilitiesProp, serializedObject);

        serializedObject.ApplyModifiedProperties();
    }
}
