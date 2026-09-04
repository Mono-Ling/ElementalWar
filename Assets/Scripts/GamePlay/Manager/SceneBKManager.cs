using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBKManager : SingleMono<SceneBKManager>
{
    private const int UPDATE_COUNT = 5;
    private const int MAX_BUFFER_COUNT = 100;
    private List<GameObject> _bkObjList = new();
    void Awake()
    {
        MonoObjectPool.Instance.CreatePool("QiuQiu", MAX_BUFFER_COUNT);
        MonoObjectPool.Instance.CreatePool("BKWall", MAX_BUFFER_COUNT);
    }
    public IEnumerator LoadBK(SceneBKAsset asset)
    {
        if (asset == null)
        {
            Debug.LogError("【场景背景物体管理器】场景背景资产为空，加载失败");
            yield break;
        }
        int currCount = 0;
        for (int i = 0; i < asset.infos.Count; i++)
        {
            var info = asset.infos[i];
            var obj = Create(info);
            if (obj != null)
                _bkObjList.Add(obj);

            if (++currCount >= UPDATE_COUNT)
            {
                currCount = 0;
                yield return null;
            }
        }
    }
    public void Uninstall()
    {
        foreach (var obj in _bkObjList)
            MonoObjectPool.Instance.PutObject(obj);
        _bkObjList.Clear();
    }
    private GameObject Create(SceneBKAsset.SceneBKInfo info)
    {
        GameObject obj;
        if (info.IsVAT)
            obj = MonoObjectPool.Instance.GetObject("QiuQiu");
        else
            obj = MonoObjectPool.Instance.GetObject("BKWall");

        if (obj == null)
        {
            Debug.LogError("【场景背景物体管理器】场景背景物体创建失败");
            return null;
        }
        if (info.IsVAT)
        {
            var vat = obj.GetComponent<VATRenderer>();
            if (vat == null)
            {
                Debug.LogError("【场景背景物体管理器】VAT渲染器获取失败");
                MonoObjectPool.Instance.PutObject(obj);
                return null;
            }
            vat.SetVATAsset(info.vatAsset);
        }
        obj.transform.SetParent(transform);
        obj.transform.position = info.pos;
        obj.transform.rotation = info.rot;
        obj.transform.localScale = info.scale;
        return obj;
    }
}
