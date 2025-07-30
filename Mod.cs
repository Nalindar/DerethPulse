namespace DerethPulse;

public class Mod : BasicMod
{
    public Mod() : base() => Setup(nameof(DerethPulse), new PatchClass(this));

    internal static void Log(string message, ModManager.LogLevel level = ModManager.LogLevel.Info)
    {
        ModManager.Log($"[DerethPulse] {message}", level);
    }
}
