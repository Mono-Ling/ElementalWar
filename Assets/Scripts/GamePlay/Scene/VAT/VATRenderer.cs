using UnityEngine;

public class VATRenderer : MonoBehaviour
{
    public VATAsset vatAsset;
    public float timeOffsetRange = 3f;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private int _timeOffsetPropertyID = Shader.PropertyToID("_TimeOffset");
    void Awake()
    {
        _propertyBlock = new();
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogError("【VAT渲染器】渲染器获取失败");
            return;
        }
    }
    void OnValidate()
    {
        if (vatAsset == null || vatAsset.VAT == null || vatAsset.VATMaterial == null)
            return;
        SetVATAsset(vatAsset);
    }
    public void SetVATAsset(VATAsset asset)
    {
        if (asset == null || asset.VAT == null || asset.VATMaterial == null)
        {
            Debug.LogError("【VAT渲染器】VAT资产、材质或纹理为空", this);
            return;
        }
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();
        if (_propertyBlock == null)
            _propertyBlock = new();
        vatAsset = asset;

        var mats = _renderer.sharedMaterials;
        for (int i = 0; i < mats.Length; i++)
            mats[i] = asset.VATMaterial;
        _renderer.sharedMaterials = mats;

        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(_timeOffsetPropertyID, Random.Range(0, timeOffsetRange));
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
