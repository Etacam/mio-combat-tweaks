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
    private NumberOption? spear_multiplier_option;
    private ToggleOption? strider_dmg_toggle;
    private ToggleOption? wildcat_cancel_fix;
    private ToggleOption? log_damage_info;

    public override void Initialize()
    {
        Hooks();

        ConnectMenus();
    }

    public void Hooks()
    {
        On.MioGame.GlobalFunctions.game.On_mio.compute_slash_damage.Suffix += Compute_slash_damage_suffix;
        On.MioGame.GlobalFunctions.game.On_mio.compute_hook_slash_damage.Suffix += Compute_hook_slash_damage_suffix;
        On.MioGame.GlobalFunctions.game.On_mio.mio_compute_damage.Suffix += Mio_compute_damage_suffix;
        On.MioGame.On_Game.fixed_update.Prefix += Fixed_update_Prefix;
        LogMessage("Connected hooks");
    }

    public void ConnectMenus()
    {
        modPage = ModMenu.RegisterMod(this, "Combat Tweaks", "Combat Tweaks Mod Options");

        slash_multiplier_option = modPage.RegisterNumber("slash_multiplier", "Slash Damage Multiplier", 10, 200, 10, 100, "% Multiplier to apply to slash damage");
        shairpin_multiplier_option = modPage.RegisterNumber("shairpin_multiplier", "Sharpened Hairpin Damage Multiplier", 10, 200, 10, 100, "% Multiplier to apply to sharpened Hairpin damage");
        crit_multiplier_option = modPage.RegisterNumber("crit_multiplier", "Sharpened Hairpin crit multiplier", 0, 100, 10, 50, "% Multiplier to damage of Sharpened Hairpin crits");
        spear_multiplier_option = modPage.RegisterNumber("spear_multiplier", "Strider spear multiplier", 10, 200, 10, 100, "% Multiplier to damage of Strider spear");
        strider_dmg_toggle = modPage.RegisterToggle("strider_dmg_toggle", "Strider Damage fix", false, "Makes strider spear do the correct amount of damage at 5 attack upgrades");
        wildcat_cancel_fix = modPage.RegisterToggle("wildcat_cancel_fix", "Wildcat fix", false, "Fixes the Wildcat buff being ended by non-slash sources of damage");
        log_damage_info = modPage.RegisterToggle("log_damage_info", "Damage numbers", false, "Display damage numbers");

        LogMessage("Registered options");
    }

    private void Nerf(Damage* retBuffer, float nerf_mult)
    {
        retBuffer->@base *= nerf_mult;
        retBuffer->upgrade *= nerf_mult;
    }

    public void Mio_compute_damage_suffix(Damage* retBuffer, Mio* mio, Mio.Slash.Attack_damage atk)
    {
        if (strider_dmg_toggle?.Value is true && atk == Mio.Slash.Attack_damage.Dmg_spear && retBuffer->upgrade == 100)
        {
            retBuffer->upgrade = 170;
        }
        if ((spear_multiplier_option?.Value ?? 100) != 100 && atk == Mio.Slash.Attack_damage.Dmg_spear)
        {
            Nerf(retBuffer, (float)(spear_multiplier_option?.Value ?? 100) / 100f);
        }
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

    private void Fixed_update_Prefix(MioGame.Game* __this)
    {
        bool berserker_ult = __this->mio.slash.berserker.ult_charged._value;
        bool berserker_ult_consume_next_frame = __this->mio.slash.berserker.consume_ult_next_frame;
        bool slash_is_active = __this->mio.slash.active._value;

        if (wildcat_cancel_fix?.Value is true && !slash_is_active && berserker_ult && berserker_ult_consume_next_frame) // fixes wildcat being cancelled by non-slash damage
        {
            __this->mio.slash.berserker.consume_ult_next_frame = false;
        }
        __this->mio.debug.log_damage_info = log_damage_info?.Value is true;
    }

}
