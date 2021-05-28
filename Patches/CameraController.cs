// ReSharper disable All

#pragma warning disable CS0649, CS0414

public class patch_GameManager : GameManager {
    private static GameManager _instance;
    public static GameManager _instanceSafe => _instance;
    private static readonly long TasInfoMark = 1234567890123456789;
    public static string TasInfo;
}