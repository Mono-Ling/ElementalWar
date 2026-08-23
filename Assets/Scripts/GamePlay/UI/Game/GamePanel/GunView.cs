using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunView : MonoBehaviour, IAutoInject<Blackboard>
{
    public Image gunImage;
    public Image progressImage;
    public TextMeshProUGUI currCountText;
    public TextMeshProUGUI maxCountText;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    private BlackboardArg<ElementType> _shootElementTypeArg;
    private BlackboardArg<float> _reloadProgressArg;
    private BlackboardArg<int> _bulletCountArg;
    private BlackboardArg<int> _maxBulletCountArg;
    void Awake()
    {
        if (gunImage == null)
            Debug.LogError("【枪械UI】枪械图片控件为空");
        if (progressImage == null)
            Debug.LogError("【枪械UI】进度图片控件为空");
        if (currCountText == null || maxCountText == null)
            Debug.LogError("【枪械UI】文本控件为空");
    }
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
        {
            Debug.LogError("【枪械UI】黑板注入为空");
            return;
        }
        if (!inject.GetBlackboardArg("ShootElementType", out _shootElementTypeArg))
            Debug.LogError("【枪械UI】枪械元素类型黑板参数获取失败");
        if (!inject.GetBlackboardArg("ReloadProgress", out _reloadProgressArg))
            Debug.LogError("【枪械UI】换弹进度黑板参数获取失败");
        if (!inject.GetBlackboardArg("BulletCount", out _bulletCountArg))
            Debug.LogError("【枪械UI】弹药量黑板参数获取失败");
        if (!inject.GetBlackboardArg("MaxBulletCount", out _maxBulletCountArg))
            Debug.LogError("【枪械UI】最大弹药量黑板参数获取失败");

        if (_shootElementTypeArg != null)
            _shootElementTypeArg.OnValueChange += OnElementTypeChange;
        if (_reloadProgressArg != null)
            _reloadProgressArg.OnValueChange += OnReloadProgressChange;
        if (_bulletCountArg != null)
            _bulletCountArg.OnValueChange += OnBulletCountChange;
        if (_maxBulletCountArg != null)
            _maxBulletCountArg.OnValueChange += OnMaxBulletCountChange;

        OnElementTypeChange(_shootElementTypeArg?.value ?? default);
        OnReloadProgressChange(_reloadProgressArg?.value ?? default);
        OnBulletCountChange(_bulletCountArg?.value ?? default);
        OnMaxBulletCountChange(_maxBulletCountArg?.value ?? default);
    }
    void OnDestroy()
    {
        if (_shootElementTypeArg != null)
            _shootElementTypeArg.OnValueChange -= OnElementTypeChange;
        if (_reloadProgressArg != null)
            _reloadProgressArg.OnValueChange -= OnReloadProgressChange;
        if (_bulletCountArg != null)
            _bulletCountArg.OnValueChange -= OnBulletCountChange;
        if (_maxBulletCountArg != null)
            _maxBulletCountArg.OnValueChange -= OnMaxBulletCountChange;
    }
    private void OnElementTypeChange(ElementType element)
    {
        if (ElementInfoMap.Instance.TryGetElementInfo(element, out var info))
            if (gunImage != null)
                gunImage.color = info.color;
    }
    private void OnReloadProgressChange(float progress)
    {
        progress = Mathf.Clamp01(progress);
        if (progressImage != null)
            progressImage.fillAmount = progress;
    }
    private void OnBulletCountChange(int count)
    {
        if (currCountText == null)
            return;
        count = Mathf.Abs(count);
        if (count == 0)
            currCountText.color = warningColor;
        else
            currCountText.color = normalColor;
        currCountText.text = count.ToString();
    }
    private void OnMaxBulletCountChange(int count)
    {
        if (maxCountText == null)
            return;
        count = Mathf.Abs(count);
        maxCountText.text = $"/{count}";
    }
}
