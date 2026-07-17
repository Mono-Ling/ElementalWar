using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBlackboardTemplate", menuName = "StateMachine/BlackboardTemplate")]
public class BlackboardTemplate : ScriptableObject
{
    public List<BlackboardTemplateInfo> argList = new();
    public Dictionary<string, BaseBlackboardArg> GetArgDic()
    {
        Dictionary<string, BaseBlackboardArg> argDic = new();
        foreach (var info in argList)
        {
            if (argDic.ContainsKey(info.name))
            {
                Debug.LogError($"【状态机黑板】{info.name}参数名重复");
                continue;
            }
            var arg = GetArg(info);
            if (arg == null)
            {
                Debug.LogWarning("【状态机黑板】未知的参数类型");
                continue;
            }
            argDic.Add(info.name, arg);
        }
        return argDic;
    }
    private BaseBlackboardArg GetArg(BlackboardTemplateInfo info) => info.type switch
    {
        BlackboardArgType.Int => new BlackboardArg<int>(),
        BlackboardArgType.Uint => new BlackboardArg<uint>(),
        BlackboardArgType.Long => new BlackboardArg<long>(),
        BlackboardArgType.Float => new BlackboardArg<float>(),
        BlackboardArgType.Double => new BlackboardArg<double>(),
        BlackboardArgType.Bool => new BlackboardArg<bool>(),
        BlackboardArgType.String => new BlackboardArg<string>(),
        BlackboardArgType.Vector2 => new BlackboardArg<Vector2>(),
        BlackboardArgType.Vector3 => new BlackboardArg<Vector3>(),
        BlackboardArgType.Quaternion => new BlackboardArg<Quaternion>(),
        BlackboardArgType.GameObject => new BlackboardArg<GameObject>(),
        BlackboardArgType.Transform => new BlackboardArg<Transform>(),
        _ => null,
    };
}
[Serializable]
public class BlackboardTemplateInfo
{
    public string name;
    public BlackboardArgType type;
}
public enum BlackboardArgType
{
    Int,
    Uint,
    Long,
    Float,
    Double,
    Bool,
    String,
    Vector2,
    Vector3,
    Quaternion,
    GameObject,
    Transform,
}