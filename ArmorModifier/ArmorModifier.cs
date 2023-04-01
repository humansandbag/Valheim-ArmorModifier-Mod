using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using HarmonyLib;
using BepInEx.Configuration;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace ArmorModifier
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ArmorModifier : BaseUnityPlugin
    {
        public const string PluginGUID = "MainStreetGaming.ArmorModifier";
        public const string PluginName = "ArmorModifier";
        public const string PluginVersion = "0.0.1";

        public static ConfigEntry<float> _helmetModifier;
        public static ConfigEntry<float> _chestModifier;
        public static ConfigEntry<float> _legModifier;
        public static ConfigEntry<float> _capeModifier;
        public static ConfigEntry<bool> _debugEnabled;

        private void Awake()
        {
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("ArmorModifier by MainStreet Gaming has loaded.");

            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html

            _helmetModifier = Config.Bind("ArmorModifiers", "HelmetModifier", 0f, "Percentage modifier for helmet armor (negative values decrease armor, positive values increase armor)");
            _chestModifier = Config.Bind("ArmorModifiers", "ChestModifier", 0f, "Percentage modifier for chest armor (negative values decrease armor, positive values increase armor)");
            _legModifier = Config.Bind("ArmorModifiers", "LegModifier", 0f, "Percentage modifier for leg armor (negative values decrease armor, positive values increase armor)");
            _capeModifier = Config.Bind("ArmorModifiers", "CapeModifier", 0f, "Percentage modifier for cape armor (negative values decrease armor, positive values increase armor)");
            _debugEnabled = Config.Bind("Debug", "DebugEnabled", false, "Enable debug output");

            //Harmony.CreateAndPatchAll(typeof(ArmorModifier));
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }





        [HarmonyPatch(typeof(ItemDrop.ItemData), "GetArmor", new System.Type[] { typeof(int) })]
        public class ArmorValueModifierPatch
        {
            // Prefix method runs before the original method and can modify its arguments and return value
            public static bool Prefix(ref ItemDrop.ItemData __instance, ref int quality, ref float __result)
            {

                // Calculate the base armor value
                float armor = __instance.m_shared.m_armor + (float)Mathf.Max(0, quality - 1) * __instance.m_shared.m_armorPerLevel;
                __result = armor;

                float newArmor = armor;

                // Apply the armor value modifier based on the item type
                switch (__instance.m_shared.m_itemType)
                {
                    case ItemDrop.ItemData.ItemType.Helmet:
                        newArmor = ModUtils.ApplyModifier(newArmor, _helmetModifier.Value);
                        break;
                    case ItemDrop.ItemData.ItemType.Chest:
                        newArmor = ModUtils.ApplyModifier(newArmor, _chestModifier.Value);
                        break;
                    case ItemDrop.ItemData.ItemType.Legs:
                        newArmor = ModUtils.ApplyModifier(newArmor, _legModifier.Value);
                        break;
                    case ItemDrop.ItemData.ItemType.Shoulder:
                        newArmor = ModUtils.ApplyModifier(newArmor, _capeModifier.Value);
                        break;
                }

                // Set the modified armor value as the result if it's different from the base armor value
                if (newArmor != __result)
                    __result = newArmor;

                return false;
            }
        }

        /*[HarmonyPatch(typeof(ItemDrop.ItemData), "GetTooltip")]
        public static class ArmorTooltipPatch
        {
            public static void Postfix(ItemDrop.ItemData __instance, ref int quality, ref string __result)
            {

                float armor = __instance.m_shared.m_armor + (float)Mathf.Max(0, quality - 1) * __instance.m_shared.m_armorPerLevel;
                float newArmor = armor;

                if (__instance.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet)
                {
                    newArmor += (newArmor * _helmetModifier.Value / 100f);
                    //__result = __result.Replace("Armor: " + __instance.m_shared.m_armor.ToString(), "Armor: " + newArmor.ToString());
                    __result = Regex.Replace(__result, "Armor:\\s(\\d+(\\.\\d+)?)", "Armor: " + newArmor.ToString());
                }
                else if (__instance.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest)
                {
                    newArmor += (newArmor * _chestModifier.Value / 100f);
                    //__result = Regex.Replace(__result, "Armor:\\s(\\d+(\\.\\d+)?)", "Armor: " + newArmor.ToString());
                    __result = Regex.Replace(__result, @"\n.*<color=orange></color>", newArmor.ToString());
                    __result += "<color=orange>TESTING!</color>";
                }
                else if (__instance.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs)
                {
                    newArmor += (newArmor * _legModifier.Value / 100f);
                    __result = Regex.Replace(__result, "Armor:\\s(\\d+(\\.\\d+)?)", "Armor: " + newArmor.ToString());
                }
                else if (__instance.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder)
                {
                    newArmor += (newArmor * _capeModifier.Value / 100f);
                    __result = Regex.Replace(__result, "Armor:\\s(\\d+(\\.\\d+)?)", "Armor: " + newArmor.ToString());
                }
            }
        }*/

        /*[HarmonyPatch(typeof(SE_Stats), "GetDamageModifiersTooltipString")]
        static class GetDamageModifiersTooltipString_Patch
        {
            static void Postfix(ref string __result, List<HitData.DamageModPair> mods)
            {
                {

                }
            }
        }*/

        public static class ModUtils
        {
            public static float ApplyModifier(float targetValue, float modifier)
            {
                const float minValue = -100f;

                modifier = Mathf.Max(modifier, minValue);

                float newValue = targetValue;

                if (modifier >= 0)
                {
                    newValue += targetValue * (modifier / 100f);
                }
                else
                {
                    newValue -= targetValue * (modifier / -100f);
                }

                return newValue;
            }
        }

    }
}