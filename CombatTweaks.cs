using MioGame;
using MioModdingApi;
using MioModLoader;

namespace CombatTweaks;

public unsafe class CombatTweaksMod : Mod
{

    // Mod options
    private static ModMenuPage? modPage;

    private NumberOption? slash_multiplier_option;
    private NumberOption? shairpin_multiplier_option;
    private NumberOption? crit_multiplier_option;

    public override void Initialize()
    {
        Hooks();

        ConnectMenus();
    }

    public void Hooks()
    {
        On.MioGame.GlobalFunctions.game.On_mio.compute_slash_damage.Suffix += Compute_slash_damage_suffix;
        On.MioGame.GlobalFunctions.game.On_mio.compute_hook_slash_damage.Suffix += Compute_hook_slash_damage_suffix;
        LogMessage("Connected hooks");
    }

    public void ConnectMenus()
    {
        modPage = ModMenu.RegisterMod(this, "Combat Tweaks", "Combat Tweaks Mod Options");

        slash_multiplier_option = modPage.RegisterNumber("slash_multiplier", "Slash Damage Multiplier", 10, 100, 10, 100, "% Multiplier to apply to slash damage");
        shairpin_multiplier_option = modPage.RegisterNumber("shairpin_multiplier", "Sharpened Hairpin Damage Multiplier", 10, 100, 10, 100, "% Multiplier to apply to sharpened Hairpin damage");
        crit_multiplier_option = modPage.RegisterNumber("crit_multiplier", "Sharpened Hairpin crit multiplier", 0, 50, 10, 50, "% Multiplier to damage of Sharpened Hairpin crits");
        LogMessage("Registered options");
    }

    private void Nerf(Damage* retBuffer, float nerf_mult)
    {
        retBuffer->@base *= nerf_mult;
        retBuffer->upgrade *= nerf_mult;
    }

    public void Compute_slash_damage_suffix(Damage* retBuffer, Mio* mio, Mio.Slash.Attack* atk)
    {
        if ((slash_multiplier_option?.Value ?? 100) != 100)
        {
            float slash_multi = (float)(slash_multiplier_option?.Value ?? 100) / 100f;
            Nerf(retBuffer, slash_multi);
        }
    }

    public void Compute_hook_slash_damage_suffix(Damage* retBuffer, Mio* mio, bool perfect)
    {
        if ((shairpin_multiplier_option?.Value ?? 100) != 100)
        {
            float shairpin_multi = (float)(shairpin_multiplier_option?.Value ?? 100) / 100f;
            Nerf(retBuffer, shairpin_multi);
        }
        if ((crit_multiplier_option?.Value ?? 50) != 50 && retBuffer->modifier >= 1.5)
        {
            retBuffer->modifier -= 0.5f;
            retBuffer->modifier += (float)(crit_multiplier_option?.Value ?? 50) / 100f;
        }
    }
}
