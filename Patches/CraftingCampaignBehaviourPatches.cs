﻿using HarmonyLib;
using System;
using System.Collections;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace BannerlordTweaks.Patches
{
    [HarmonyPatch(typeof(CraftingCampaignBehavior), "GetMaxHeroCraftingStamina")]
    public class GetMaxHeroCraftingStaminaPatch
    {
        static bool Prefix(CraftingCampaignBehavior __instance, ref int __result)
        {
            __result = Settings.Instance.MaxCraftingStamina;
            return false;
        }

        static bool Prepare()
        {
            return Settings.Instance.CraftingStaminaTweakEnabled;
        }
    }

    [HarmonyPatch(typeof(CraftingCampaignBehavior), "HourlyTick")]
    public class HourlyTickPatch
    {
        static bool Prefix(CraftingCampaignBehavior __instance)
        {
            try
            {
                IDictionary records = (IDictionary)(typeof(CraftingCampaignBehavior).GetField("_heroCraftingRecords", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance));
                foreach (Hero hero in records.Keys)
                {
                    int curCraftingStamina = __instance.GetHeroCraftingStamina(hero);

                    if (curCraftingStamina < Settings.Instance.MaxCraftingStamina)
                    {
                        int staminaGainAmount = Settings.Instance.CraftingStaminaGainAmount;

                        if (Settings.Instance.CraftingStaminaGainOutsideSettlementMultiplier < 1 && hero.PartyBelongedTo?.CurrentSettlement == null)
                            staminaGainAmount = (int)Math.Ceiling(staminaGainAmount * Settings.Instance.CraftingStaminaGainOutsideSettlementMultiplier);

                        int diff = Settings.Instance.MaxCraftingStamina - curCraftingStamina;
                        if (diff < staminaGainAmount)
                            staminaGainAmount = diff;

                        __instance.SetHeroCraftingStamina(hero, Math.Min(Settings.Instance.MaxCraftingStamina, curCraftingStamina + staminaGainAmount));
                        //MessageBox.Show($"Hero: {hero.Name}\n\nCrafting Stamina: {__instance.GetHeroCraftingStamina(hero)}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception happened during the HourlyTick patch:\n\n{ex.Message}\n\n{ex.InnerException?.Message}", "Exception");
            }
            return false;
        }

        static bool Prepare()
        {
            return Settings.Instance.CraftingStaminaTweakEnabled;
        }
    }
}
