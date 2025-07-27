namespace DerethPulse;

public class Mod : BasicMod
{
    public Mod() : base() => Setup(nameof(DerethPulse), new PatchClass(this));
} 