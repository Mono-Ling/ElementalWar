using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VATAsset))]
public class VATAssetEditor : Editor
{
    private SerializedProperty _VAT;
    private SerializedProperty _frameCount;
    private SerializedProperty _frameRate;
    private SerializedProperty _vertexCount;
    private SerializedProperty _minPos;
    private SerializedProperty _maxPos;
    private SerializedProperty _VATMaterial;
    void OnEnable()
    {
        _VAT = serializedObject.FindProperty("<VAT>k__BackingField");
        _frameCount = serializedObject.FindProperty("<frameCount>k__BackingField");
        _frameRate = serializedObject.FindProperty("<frameRate>k__BackingField");
        _vertexCount = serializedObject.FindProperty("<vertexCount>k__BackingField");
        _minPos = serializedObject.FindProperty("<minPos>k__BackingField");
        _maxPos = serializedObject.FindProperty("<maxPos>k__BackingField");
        _VATMaterial = serializedObject.FindProperty("<VATMaterial>k__BackingField");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_VAT);
        EditorGUILayout.PropertyField(_frameCount);
        EditorGUILayout.PropertyField(_frameRate);
        EditorGUILayout.PropertyField(_vertexCount);
        EditorGUILayout.PropertyField(_minPos);
        EditorGUILayout.PropertyField(_maxPos);
        EditorGUILayout.PropertyField(_VATMaterial);
        EditorGUI.EndDisabledGroup();
        serializedObject.ApplyModifiedProperties();
    }
}
