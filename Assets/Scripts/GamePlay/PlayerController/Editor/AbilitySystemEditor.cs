using UnityEditor;

[CustomEditor(typeof(AbilitySystem))]
public class AbilitySystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制 abilities 列表（SerializeReference 多态支持）
        var abilitiesProp = serializedObject.FindProperty("_abilitiesSerialized");
        AbilityListDrawer.DrawAbilityList(abilitiesProp, serializedObject);

        serializedObject.ApplyModifiedProperties();
    }
}
