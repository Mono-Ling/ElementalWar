using System;
using UnityEditor;
using UnityEngine;

public class VATCreator : EditorWindow
{
    [MenuItem("Tools/VAT")]
    private static void Creator()
    {
        var window = GetWindow<VATCreator>(title: typeof(VATCreator).ToString());
        window.Show();
    }
    private AnimationClip _clip;
    private SkinnedMeshRenderer _renderer;
    void OnGUI()
    {
        _clip = EditorGUILayout.ObjectField(_clip, typeof(AnimationClip), false) as AnimationClip;
        _renderer = EditorGUILayout.ObjectField(_renderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
        if (_clip == null || _renderer == null)
            return;

        var sampleTarget = ResolveSampleTarget(_renderer);
        EditorGUILayout.HelpBox($"动画采样目标：{sampleTarget.name}", MessageType.Info);

        if (!GUILayout.Button("渲染VAT"))
            return;
        var vatAsset = ScriptableObject.CreateInstance<VATAsset>();
        Mesh mesh = new();
        _renderer.BakeMesh(mesh); // 仅用当前姿态确定顶点数
        vatAsset.Begin(_clip, mesh.vertexCount);
        if (vatAsset.vertexCount == 0)
        {
            DestroyImmediate(mesh);
            return;
        }

        bool sampling = false;
        try
        {
            var total = vatAsset.frameCount;
            AnimationMode.StartAnimationMode();
            try
            {
                AnimationMode.BeginSampling();
                sampling = true;
                for (int i = 0; i < total; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("VAT Creator", $"烘焙VAT {i}/{total}", (float)i / total))
                        throw new OperationCanceledException();
                    AnimationMode.SampleAnimationClip(sampleTarget, _clip, i / vatAsset.frameRate);
                    mesh.Clear(false);
                    _renderer.BakeMesh(mesh);
                    vatAsset.AddFrame(i, mesh.vertices);
                }
                AnimationMode.EndSampling();
                sampling = false;
            }
            finally
            {
                if (sampling)
                    AnimationMode.EndSampling();
                // StartAnimationMode 会把编辑器置入只读的动画模式，异常路径也必须退出
                AnimationMode.StopAnimationMode();
            }
            vatAsset.Bake();
            vatAsset.Save();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("【VAT Creator】烘焙已取消，未生成资产");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            DestroyImmediate(mesh);
        }
    }

    /// <summary>
    /// 动画曲线相对动画根绑定，采样必须作用于挂 Animator/Animation 的物体
    /// </summary>
    private static GameObject ResolveSampleTarget(SkinnedMeshRenderer renderer)
    {
        var animator = renderer.GetComponentInParent<Animator>(true);
        if (animator != null)
            return animator.gameObject;
        var legacy = renderer.GetComponentInParent<Animation>(true);
        if (legacy != null)
            return legacy.gameObject;
        return renderer.transform.root.gameObject;
    }
}
