using System;
using System.Threading;

public class Single<T> where T : class
{
    // 网络线程与主线程可能首次同时访问 Instance，用 Lazy 保证只构造一次并正确发布
    private static readonly Lazy<T> _instance = new(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);
    public static T Instance => _instance.Value;
    private static T CreateInstance()
    {
        var constructorInfo = typeof(T).GetConstructor(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public,
            null, Type.EmptyTypes, null);
        return constructorInfo?.Invoke(null) as T;
    }
}
