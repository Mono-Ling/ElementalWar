using System;

public class Single<T> where T : class
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var constructorInfo = typeof(T).GetConstructor(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public,
                    null, Type.EmptyTypes, null);
                _instance = constructorInfo?.Invoke(null) as T;
            }
            return _instance;
        }
    }
}
