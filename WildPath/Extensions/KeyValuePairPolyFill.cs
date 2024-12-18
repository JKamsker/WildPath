namespace WildPath.Extensions;

internal static class KeyValuePairPolyFill
{
#if NET48 || NETSTANDARD2_0
    // KeyValuePair deconstruction is not available in .NET Standard 2.0
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }
#endif
    
}