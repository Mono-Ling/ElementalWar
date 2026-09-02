using System.Collections.Generic;
using System.IO;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VATAsset : ScriptableObject
{
    public const string VAT_PATH = "Art/Texture";
    public const string ASSET_PATH = "Assets/SO/VAT";
    [field: SerializeField]
    public Texture2D VAT { get; private set; }
    [field: SerializeField]
    public int frameCount { get; private set; }
    [field: SerializeField]
    public float frameRate { get; private set; }
    [field: SerializeField]
    public int vertexCount { get; private set; }
    [field: SerializeField]
    public Vector3 maxPos { get; private set; }
    [field: SerializeField]
    public Vector3 minPos { get; private set; }

    private Texture2D _baked;
    private readonly List<Vector3[]> _frames = new();

    /// <summary>
    /// 按动画信息初始化，纹理延迟到 Bake 时创建。
    /// </summary>
    public void Begin(AnimationClip clip, int vertexCount)
    {
        if (clip == null || vertexCount <= 0)
        {
            Debug.LogError("【VAT】动画为空或顶点数非法");
            return;
        }
        frameRate = clip.frameRate;
        frameCount = Mathf.CeilToInt(clip.length * frameRate);
        this.vertexCount = vertexCount;
        _frames.Clear();
        _baked = null;
        name = clip.name;
    }

    /// <summary>
    /// 缓存一帧烘焙顶点（rootBone 局部空间），顺序必须与顶点索引一致。
    /// </summary>
    public void AddFrame(int frameIndex, Vector3[] vertices)
    {
        if (vertices == null || vertices.Length != vertexCount)
        {
            Debug.LogError("【VAT】顶点数与初始化不符，帧数据被丢弃");
            return;
        }
        if (frameIndex != _frames.Count)
        {
            Debug.LogError("【VAT】帧序号不连续，帧数据被丢弃");
            return;
        }
        _frames.Add(vertices);
    }

    /// <summary>
    /// 用全部帧顶点的包围盒并集编码，min/max 覆盖整段动画且与顶点同处 rootBone 局部空间。
    /// </summary>
    public void Bake()
    {
        if (_frames.Count == 0)
        {
            Debug.LogError("【VAT】没有帧数据，编码失败");
            return;
        }
        Vector3 min = _frames[0][0];
        Vector3 max = min;
        foreach (var frame in _frames)
        {
            foreach (var v in frame)
            {
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }
        }
        minPos = min;
        maxPos = max;

        _baked = new Texture2D(vertexCount, frameCount, TextureFormat.RGBA32, false, true);
        var pixels = new Color[vertexCount * frameCount];
        for (int f = 0; f < _frames.Count; f++)
        {
            var frame = _frames[f];
            for (int i = 0; i < frame.Length; i++)
            {
                var v = frame[i];
                pixels[f * vertexCount + i] = new Color
                (
                    Mathf.InverseLerp(min.x, max.x, v.x),
                    Mathf.InverseLerp(min.y, max.y, v.y),
                    Mathf.InverseLerp(min.z, max.z, v.z),
                    1f
                );
            }
        }
        _baked.SetPixels(pixels);
        _baked.Apply(false, false);
        _frames.Clear();
    }

#if UNITY_EDITOR
    public void Save()
    {
        if (_baked == null)
        {
            Debug.LogError("【VAT】VAT纹理为空，保存失败");
            return;
        }
        var bytes = _baked.EncodeToPNG();
        File.WriteAllBytes($"{Application.dataPath}/{VAT_PATH}/{name}.png", bytes);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        string texPath = $"Assets/{VAT_PATH}/{name}.png";
        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("【VAT】TextureImporter为空，纹理设置失败");
            return;
        }
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false;
        importer.sRGBTexture = false;
        importer.maxTextureSize = Mathf.Max(frameCount, vertexCount);
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();

        string assetPath = $"{ASSET_PATH}/{name}.asset";
        var loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        // 先清空 VAT 再入库：CreateAsset 时若持有内存纹理会被整块嵌入 .asset；
        // 已存在的资产用 CopySerialized 覆盖，保留 guid 以免场景引用断裂
        VAT = null;
        var existing = AssetDatabase.LoadAssetAtPath<VATAsset>(assetPath);
        if (existing != null)
        {
            EditorUtility.CopySerialized(this, existing);
            existing.VAT = loadedTex;
            EditorUtility.SetDirty(existing);
        }
        else
        {
            AssetDatabase.CreateAsset(this, assetPath);
            VAT = loadedTex;
            EditorUtility.SetDirty(this);
        }
        SyncReferencingMaterials(loadedTex);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 同步引用了该 VAT 纹理的材质兜底参数，保证未挂 VATRenderer 时（编辑模式）显示仍然正确。
    /// </summary>
    private void SyncReferencingMaterials(Texture2D tex)
    {
        if (tex == null)
            return;
        foreach (var matGuid in AssetDatabase.FindAssets("t:Material"))
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(matGuid));
            if (mat == null || !mat.HasProperty("_VAT") || mat.GetTexture("_VAT") != tex)
                continue;
            mat.SetFloat("_FrameCount", frameCount);
            mat.SetFloat("_FrameRate", frameRate);
            mat.SetFloat("_VertexCount", vertexCount);
            mat.SetVector("_MinPos", minPos);
            mat.SetVector("_MaxPos", maxPos);
            EditorUtility.SetDirty(mat);
        }
    }
#endif
}
