using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrenadeView : MonoBehaviour, IAutoInject<Blackboard>
{
    public Image grenadeImage;
    public TextMeshProUGUI countText;
    private BlackboardArg<ElementType> _elementTypeArg;
    private BlackboardArg<int> _countArg;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    void Awake()
    {
        if (grenadeImage == null)
            Debug.LogError("【手榴弹UI】手榴弹图片控件为空");
        if (countText == null)
            Debug.LogError("【手榴弹UI】文本控件为空");
    }
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
        {
            Debug.LogError("【手榴弹UI】黑板注入为空");
            return;
        }
        if (!inject.GetBlackboardArg("GrenadeCount", out _countArg))
            Debug.LogError("【手榴弹UI】手榴弹弹药量黑板参数获取失败");
        if (!inject.GetBlackboardArg("AttackElementType", out _elementTypeArg))
            Debug.LogError("【手榴弹UI】手榴弹元素类型黑板参数获取失败");

        if (_countArg != null)
            _countArg.OnValueChange += OnGrenadeCountChange;
        if (_elementTypeArg != null)
            _elementTypeArg.OnValueChange += OnElementTypeChange;

        OnGrenadeCountChange(_countArg?.value ?? default);
        OnElementTypeChange(_elementTypeArg?.value ?? default);
    }
    void OnDestroy()
    {
        if (_countArg != null)
            _countArg.OnValueChange -= OnGrenadeCountChange;
        if (_elementTypeArg != null)
            _elementTypeArg.OnValueChange -= OnElementTypeChange;
    }
    private void OnElementTypeChange(ElementType element)
    {
        if (grenadeImage == null)
            return;
        if (ElementInfoMap.Instance.TryGetElementInfo(element, out var info))
            grenadeImage.color = info.color;
    }
    private void OnGrenadeCountChange(int count)
    {
        if (countText == null)
            return;
        count = Mathf.Abs(count);
        if (count == 0)
            countText.color = warningColor;
        else
            countText.color = normalColor;
        countText.text = count.ToString();
    }
}
