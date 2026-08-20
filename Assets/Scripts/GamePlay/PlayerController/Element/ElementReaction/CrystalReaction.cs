using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalReaction : BaseElementReaction, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<ElementType> _enableCrystalTypes = new();
    private HashSet<ElementType> _enableCrystalSet = new();

    public void OnAfterDeserialize()
    {
        _enableCrystalSet.Clear();
        foreach (var element in _enableCrystalTypes)
            _enableCrystalSet.Add(element);
    }
    public void OnBeforeSerialize() { }
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        if (afterElement != ElementType.Rock || !_enableCrystalSet.Contains(beforeElement))
            return false;

        var arg = (beforeElement, elementReceiver?.transform.position ?? Vector3.zero);
        DynamicSceneItemMgr.Instance.CreateLocalDynamicSceneItem<(ElementType, Vector3)>
        (Message.DynamicSceneItemType.ElementCrystal, arg);
        return base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);
    }
    public override int GetDamage(ElementType attackElement, int damage)
    => 0;
}
