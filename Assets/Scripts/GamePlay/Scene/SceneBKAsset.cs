using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class SceneBKAsset : ScriptableObject
{
    [Serializable]
    public struct SceneBKInfo
    {
        public bool IsVAT => vatAsset != null;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
        public VATAsset vatAsset;
    }
    public List<SceneBKInfo> infos = new();
    public void Set(GameObject[] objs)
    {
        if (objs.Length == 0)
            return;
        infos.Clear();
        foreach (var obj in objs)
        {
            if (obj == null)
                continue;
            SceneBKInfo info = new();
            info.pos = obj.transform.position;
            info.rot = obj.transform.rotation;
            info.scale = obj.transform.localScale;

            var vatRenderer = obj.GetComponent<VATRenderer>();
            if (vatRenderer != null)
                if (vatRenderer.vatAsset != null)
                    info.vatAsset = vatRenderer.vatAsset;
                else
                    continue;

            infos.Add(info);
        }
    }
}