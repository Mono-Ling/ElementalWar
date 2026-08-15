using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DiffusionReaction : BaseElementReaction, ISerializationCallbackReceiver
{
    [Header("扩散元素量倍率")]
    public float diffuseNum = 0.5f;
    [SerializeField]
    private List<ElementType> _diffuseAbleList = new();
    private HashSet<ElementType> _diffuseAbleSet = new();

    private List<ElementType> _currDiffuseList = new();

    public void OnAfterDeserialize()
    {
        _diffuseAbleSet.Clear();
        foreach (var element in _diffuseAbleList)
            _diffuseAbleSet.Add(element);
    }
    public void OnBeforeSerialize() { }

    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        // 若为风元素附着不反应
        if (beforeElement == ElementType.Wind || afterElement != ElementType.Wind)
            return false;
        if (!_diffuseAbleSet.Contains(beforeElement))
            return false;

        var diffCContent = afterContent * diffuseNum;
        GetDiffusionElements(ref _currDiffuseList, diffCContent);

        // 网络包：范围元素伤害

        // 风元素清空
        // 附着元素不变，确保不额外衰减附着元素
        afterContent = 0;

        #region  Debug
        StringBuilder debugStr = new();
        foreach (var element in _currDiffuseList)
            debugStr.Append($"|{element}");
        Debug.Log($"【扩散反应】扩散元素{debugStr}");
        #endregion

        return true;
    }
    private void GetDiffusionElements(ref List<ElementType> elements, float diffusContent)
    {
        if (!blackboard.GetValue<ElementAttachment>("ElementAttachment", out var elementAttachment))
        {
            Debug.LogError("【扩散反应】元素附着组件为空");
            return;
        }
        _currDiffuseList.Clear();
        foreach (var element in _diffuseAbleSet)
        {
            if (elementAttachment.ElementContentDic.ContainsKey(element))
            {
                elements.Add(element);
                elementAttachment.ReduceElementContent(element, diffusContent);
            }
        }
    }
}