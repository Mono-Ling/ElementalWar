using UnityEngine;

public class VATRenderer : MonoBehaviour
{
    public VATAsset vatAsset;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private int _vatPropertyID = Shader.PropertyToID("_VAT");
    private int _frameCountPropertyID = Shader.PropertyToID("_FrameCount");
    private int _vertexCountPropertyID = Shader.PropertyToID("_VertexCount");
    private int _frameRatePropertyID = Shader.PropertyToID("_FrameRate");
    private int _minPosPropertyID = Shader.PropertyToID("_MinPos");
    private int _maxPosPropertyID = Shader.PropertyToID("_MaxPos");
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
    => SetVATAsset(vatAsset);
    public void SetVATAsset(VATAsset asset)
    {
        if (asset == null || asset.VAT == null)
        {
            Debug.LogError("【VAT渲染器】VAT资产或纹理为空", this);
            return;
        }
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();
        if (_propertyBlock == null)
            _propertyBlock = new();
        vatAsset = asset;
        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetTexture(_vatPropertyID, asset.VAT);
        _propertyBlock.SetFloat(_frameCountPropertyID, asset.frameCount);
        _propertyBlock.SetFloat(_vertexCountPropertyID, asset.vertexCount);
        _propertyBlock.SetFloat(_frameRatePropertyID, asset.frameRate);
        _propertyBlock.SetVector(_minPosPropertyID, asset.minPos);
        _propertyBlock.SetVector(_maxPosPropertyID, asset.maxPos);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
