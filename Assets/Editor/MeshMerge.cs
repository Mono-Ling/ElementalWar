using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 蒙皮网格合并工具：
/// 在 Hierarchy 中选中多个 SkinnedMeshRenderer（或选中其父物体），
/// 通过 GameObject/Create/MeshMerge 将它们合并为一个 SkinnedMeshRenderer。
/// 骨骼列表会去重合并，顶点会被换算到根骨骼空间，因此角色整体被移动/旋转后依然正确。
/// </summary>
public class MeshMerge
{
    [MenuItem("GameObject/Create/MeshMerge", false, 10)]
    private static void Main()
    {
        var sourceList = Selection.gameObjects
            .SelectMany(go => go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            .Distinct()
            .ToList();

        if (sourceList.Count == 0)
        {
            Debug.LogWarning("[MeshMerge] 选中的对象里没有 SkinnedMeshRenderer。");
            return;
        }

        var first = sourceList[0];
        Transform root = first.rootBone != null ? first.rootBone : first.transform;

        // 合并结果挂在根骨骼下，根骨骼同时作为合并网格的空间参考
        var mergeGO = new GameObject(first.name + "_Merged");
        Undo.RegisterCreatedObjectUndo(mergeGO, "Merge Skinned Mesh");
        mergeGO.transform.SetParent(root, false);

        var target = mergeGO.AddComponent<SkinnedMeshRenderer>();
        target.shadowCastingMode = first.shadowCastingMode;
        target.receiveShadows = first.receiveShadows;
        target.lightProbeUsage = first.lightProbeUsage;
        target.reflectionProbeUsage = first.reflectionProbeUsage;
        target.probeAnchor = first.probeAnchor;

        MergeMesh(target, root, sourceList);

        // 合并完成后禁用源渲染器，避免重复绘制
        foreach (var smr in sourceList)
        {
            if (smr == null) continue;
            Undo.RecordObject(smr, "Merge Skinned Mesh");
            smr.enabled = false;
        }

        Selection.activeGameObject = mergeGO;

        if (target.sharedMesh != null)
            SaveMeshAsset(target.sharedMesh);

        Debug.Log($"[MeshMerge] 已合并 {sourceList.Count} 个蒙皮网格到 {mergeGO.name}，" +
                  $"共 {target.sharedMesh.vertexCount} 顶点 / {target.sharedMesh.subMeshCount} 子网格。");
    }

    [MenuItem("GameObject/Create/MeshMerge", true)]
    private static bool ValidateMain()
    {
        return Selection.gameObjects != null &&
               Selection.gameObjects.Any(go => go.GetComponentInChildren<SkinnedMeshRenderer>(true) != null);
    }

    /// <summary>
    /// 将 sourceList 中所有蒙皮网格合并到 target 上，root 为合并网格的空间参考（通常传根骨骼）。
    /// </summary>
    private static void MergeMesh(SkinnedMeshRenderer target,
                                  Transform root,
                                  List<SkinnedMeshRenderer> sourceList)
    {
        sourceList = sourceList
            .Where(smr => smr != null && smr.sharedMesh != null && smr.bones.All(b => b != null))
            .ToList();

        if (sourceList.Count == 0 || root == null)
        {
            Debug.LogWarning("[MeshMerge] 没有可合并的蒙皮网格，或空间参考为空。");
            return;
        }

        // ---------- 1. 收集统一骨骼列表（按出现顺序去重），并建立每个源的骨骼索引映射 ----------
        var boneList = new List<Transform>();
        foreach (var smr in sourceList)
        {
            foreach (var bone in smr.bones)
            {
                if (!boneList.Contains(bone))
                    boneList.Add(bone);
            }
        }

        var boneIndexMaps = new int[sourceList.Count][];
        for (int i = 0; i < sourceList.Count; i++)
        {
            var srcBones = sourceList[i].bones;
            var map = new int[srcBones.Length];
            for (int j = 0; j < srcBones.Length; j++)
                map[j] = boneList.IndexOf(srcBones[j]);
            boneIndexMaps[i] = map;
        }

        // ---------- 2. 统计总顶点数、子网格数、用到的 UV 通道 ----------
        int totalVertexCount = 0;
        int totalSubMeshCount = 0;
        var usedUvChannels = new bool[8];
        foreach (var smr in sourceList)
        {
            var mesh = smr.sharedMesh;
            totalVertexCount += mesh.vertexCount;
            totalSubMeshCount += mesh.subMeshCount;
            for (int ch = 0; ch < 8; ch++)
            {
                if (mesh.HasVertexAttribute(VertexAttribute.TexCoord0 + ch))
                    usedUvChannels[ch] = true;
            }
        }

        var vertices = new Vector3[totalVertexCount];
        var normals = new Vector3[totalVertexCount];
        var tangents = new Vector4[totalVertexCount];
        var boneWeights = new BoneWeight[totalVertexCount];
        var colors = new Color32[totalVertexCount];
        var hasAnyColor = false;
        var uvBuffers = new List<Vector4>[8];
        for (int ch = 0; ch < 8; ch++)
        {
            if (usedUvChannels[ch])
                uvBuffers[ch] = new List<Vector4>(totalVertexCount);
        }

        // ---------- 3. 合并后的 bindpose：合并网格空间（root 局部空间）到各骨骼 ----------
        var bindposes = new Matrix4x4[boneList.Count];
        for (int i = 0; i < boneList.Count; i++)
            bindposes[i] = boneList[i].worldToLocalMatrix * root.localToWorldMatrix;

        // 组合矩阵缓存：combined[i][j] = root.worldToLocal * 骨骼j当前局部到世界 * 源bindpose_j
        // 即“源网格绑定空间 → 合并网格空间”，蒙皮结果与当前骨骼姿态、角色摆放位置无关
        var combinedMatrices = new Matrix4x4[sourceList.Count][];
        for (int i = 0; i < sourceList.Count; i++)
        {
            var smr = sourceList[i];
            var srcBindposes = smr.sharedMesh.bindposes;
            var mats = new Matrix4x4[smr.bones.Length];
            for (int j = 0; j < smr.bones.Length; j++)
                mats[j] = root.worldToLocalMatrix * (smr.bones[j].localToWorldMatrix * srcBindposes[j]);
            combinedMatrices[i] = mats;
        }

        var subMeshTriangles = new List<int[]>(totalSubMeshCount);
        var materials = new List<Material>(totalSubMeshCount);
        var shapeNames = new List<string>();
        var shapeFirst = new List<int[]>(); // 每个 BlendShape 首次出现时的 [源下标, Shape下标]

        // ---------- 4. 逐源复制顶点数据 ----------
        int vertexOffset = 0;
        for (int srcIdx = 0; srcIdx < sourceList.Count; srcIdx++)
        {
            var smr = sourceList[srcIdx];
            var mesh = smr.sharedMesh;
            var srcVertices = mesh.vertices;
            var hasNormal = mesh.HasVertexAttribute(VertexAttribute.Normal);
            var hasTangent = mesh.HasVertexAttribute(VertexAttribute.Tangent);
            var hasColor = mesh.HasVertexAttribute(VertexAttribute.Color);
            var srcNormals = hasNormal ? mesh.normals : null;
            var srcTangents = hasTangent ? mesh.tangents : null;
            var srcColors = hasColor ? mesh.colors32 : null;
            var srcBoneWeights = mesh.boneWeights;
            var mats = combinedMatrices[srcIdx];
            var indexMap = boneIndexMaps[srcIdx];

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                int dst = vertexOffset + i;
                // 以主权重骨骼（boneIndex0）为基准把顶点换算到合并网格空间
                var m = mats[srcBoneWeights[i].boneIndex0];

                vertices[dst] = m.MultiplyPoint3x4(srcVertices[i]);

                if (hasNormal)
                {
                    var n = m.MultiplyVector(srcNormals[i]);
                    float len = n.magnitude;
                    normals[dst] = len > 1e-6f ? n / len : Vector3.up;
                }
                else
                {
                    normals[dst] = Vector3.up;
                }

                if (hasTangent)
                {
                    var t4 = srcTangents[i];
                    var t3 = m.MultiplyVector(new Vector3(t4.x, t4.y, t4.z));
                    float len = t3.magnitude;
                    tangents[dst] = len > 1e-6f
                        ? new Vector4(t3.x / len, t3.y / len, t3.z / len, t4.w)
                        : new Vector4(1f, 0f, 0f, t4.w);
                }
                else
                {
                    tangents[dst] = new Vector4(1f, 0f, 0f, 1f);
                }

                if (hasColor)
                {
                    colors[dst] = srcColors[i];
                    hasAnyColor = true;
                }

                boneWeights[dst] = new BoneWeight
                {
                    boneIndex0 = indexMap[srcBoneWeights[i].boneIndex0],
                    weight0 = srcBoneWeights[i].weight0,
                    boneIndex1 = indexMap[srcBoneWeights[i].boneIndex1],
                    weight1 = srcBoneWeights[i].weight1,
                    boneIndex2 = indexMap[srcBoneWeights[i].boneIndex2],
                    weight2 = srcBoneWeights[i].weight2,
                    boneIndex3 = indexMap[srcBoneWeights[i].boneIndex3],
                    weight3 = srcBoneWeights[i].weight3,
                };
            }

            for (int ch = 0; ch < 8; ch++)
            {
                if (uvBuffers[ch] == null) continue;
                var srcUv = new List<Vector4>();
                mesh.GetUVs(ch, srcUv); // 通道不存在时列表保持为空
                if (srcUv.Count == mesh.vertexCount)
                    uvBuffers[ch].AddRange(srcUv);
                else
                    uvBuffers[ch].AddRange(Enumerable.Repeat(Vector4.zero, mesh.vertexCount));
            }

            // 每个源子网格对应合并网格的一个子网格，三角形顶点号加上偏移
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                var tris = mesh.GetTriangles(s);
                for (int t = 0; t < tris.Length; t++)
                    tris[t] += vertexOffset;
                subMeshTriangles.Add(tris);

                var srcMats = smr.sharedMaterials;
                materials.Add(s < srcMats.Length ? srcMats[s] : null);
            }

            // 记录 BlendShape 名单（按名字去重）
            for (int s = 0; s < mesh.blendShapeCount; s++)
            {
                var shapeName = mesh.GetBlendShapeName(s);
                if (!shapeNames.Contains(shapeName))
                {
                    shapeNames.Add(shapeName);
                    shapeFirst.Add(new[] { srcIdx, s });
                }
            }

            vertexOffset += mesh.vertexCount;
        }

        // ---------- 5. 组装合并网格 ----------
        var newMesh = new Mesh { name = target.name };
        newMesh.indexFormat = totalVertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        newMesh.vertices = vertices;
        newMesh.normals = normals;
        newMesh.tangents = tangents;
        if (hasAnyColor)
            newMesh.colors32 = colors;
        newMesh.boneWeights = boneWeights;
        newMesh.bindposes = bindposes;

        newMesh.subMeshCount = subMeshTriangles.Count;
        for (int s = 0; s < subMeshTriangles.Count; s++)
            newMesh.SetTriangles(subMeshTriangles[s], s, false);

        for (int ch = 0; ch < 8; ch++)
        {
            if (uvBuffers[ch] != null)
                newMesh.SetUVs(ch, uvBuffers[ch]);
        }

        // ---------- 6. 合并 BlendShape：按名字对齐各源的同名形状，逐帧复制变形数据 ----------
        for (int s = 0; s < shapeNames.Count; s++)
        {
            var firstMesh = sourceList[shapeFirst[s][0]].sharedMesh;
            int firstShapeIdx = shapeFirst[s][1];
            int frameCount = firstMesh.GetBlendShapeFrameCount(firstShapeIdx);

            for (int f = 0; f < frameCount; f++)
            {
                float weight = firstMesh.GetBlendShapeFrameWeight(firstShapeIdx, f);
                var deltaV = new Vector3[totalVertexCount];
                var deltaN = new Vector3[totalVertexCount];
                var deltaT = new Vector3[totalVertexCount];

                int offset = 0;
                for (int srcIdx = 0; srcIdx < sourceList.Count; srcIdx++)
                {
                    var mesh = sourceList[srcIdx].sharedMesh;
                    int shapeIdx = mesh.GetBlendShapeIndex(shapeNames[s]);
                    if (shapeIdx >= 0)
                    {
                        // 源同名形状帧数不足时取其最后一帧
                        int frameIdx = Mathf.Min(f, mesh.GetBlendShapeFrameCount(shapeIdx) - 1);
                        var v = new Vector3[mesh.vertexCount];
                        var n = new Vector3[mesh.vertexCount];
                        var t = new Vector3[mesh.vertexCount];
                        mesh.GetBlendShapeFrameVertices(shapeIdx, frameIdx, v, n, t);

                        var mats = combinedMatrices[srcIdx];
                        var srcBoneWeights = mesh.boneWeights;
                        for (int i = 0; i < mesh.vertexCount; i++)
                        {
                            // 变形量只做方向变换（与顶点同一套线性部分），不含平移
                            var m = mats[srcBoneWeights[i].boneIndex0];
                            deltaV[offset + i] = m.MultiplyVector(v[i]);
                            deltaN[offset + i] = m.MultiplyVector(n[i]);
                            deltaT[offset + i] = m.MultiplyVector(t[i]);
                        }
                    }
                    offset += mesh.vertexCount;
                }

                newMesh.AddBlendShapeFrame(shapeNames[s], weight, deltaV, deltaN, deltaT);
            }
        }

        newMesh.RecalculateBounds();

        // ---------- 7. 应用到目标渲染器 ----------
        target.sharedMesh = newMesh;
        target.bones = boneList.ToArray();
        target.rootBone = root;
        target.sharedMaterials = materials.ToArray();
    }

    /// <summary>
    /// 把合并出的 Mesh 保存为 .asset 资产，重名时自动生成新文件名，不会覆盖旧资产。
    /// </summary>
    private static void SaveMeshAsset(Mesh mesh)
    {
        const string folder = "Assets/Art/MeshMerge";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            var parts = folder.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{mesh.name}.asset");
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(mesh);
        Debug.Log($"[MeshMerge] 合并网格已保存为资产：{path}");
    }
}
