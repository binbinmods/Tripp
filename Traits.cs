﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Tripp.CustomFunctions;
using static Tripp.Plugin;
using static Tripp.DescriptionFunctions;
using static Tripp.CharacterFunctions;
using System.Text;
using TMPro;
using Obeliskial_Essentials;
using System.Data.Common;

namespace Tripp
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (!IsLivingHero(_character))
            {
                return;
            }
            string traitName = traitData.TraitName;
            string traitId = _trait;


            if (_trait == trait0)
            {
                // trait0:
                // At the start of combat, apply 1 Slow and 1 Mark to a random enemy
                LogDebug($"Handling Trait {traitId}: {traitName}");
                Character randomEnemy = GetRandomCharacter(teamNpc);
                randomEnemy?.SetAuraTrait(_character, "slow", 1);
                randomEnemy?.SetAuraTrait(_character, "mark", 1);
            }


            else if (_trait == trait2a)
            {
                // trait2a
                // When you apply Slow, gain 4 Block. (8x/turn)
                if (CanIncrementTraitActivations(traitId) && _auxString.ToLower() == "slow")// && MatchManager.Instance.energyJustWastedByHero > 0)
                {
                    LogDebug($"Handling Trait {traitId}: {traitName}");
                    _character?.SetAuraTrait(_character, "block", 4);
                    IncrementTraitActivations(traitId);
                }
            }



            else if (_trait == trait2b)
            {
                // trait2b:
                // Mark on enemies decreases speed by 1 per charge. 
                // Handled in GACM
                LogDebug($"Handling Trait {traitId}: {traitName}");

            }

            else if (_trait == trait4a)
            {
                // trait 4a;
                // When you apply Slow, deal 5 Piercing damage to a random enemy.
                if (_auxString.ToLower() == "slow") // && CanIncrementTraitActivations(traitId))
                {
                    LogDebug($"Handling Trait {traitId}: {traitName}");
                    Character randomEnemy = GetRandomCharacter(teamNpc);
                    int damage = _character.DamageWithCharacterBonus(5, Enums.DamageType.Piercing, Enums.CardClass.None);
                    randomEnemy?.IndirectDamage(Enums.DamageType.Piercing, damage);
                    // IncrementTraitActivations(traitId);
                }
            }

            else if (_trait == trait4b)
            {
                // trait 4b:
                LogDebug($"Handling Trait {traitId}: {traitName}");
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            switch (_acId)
            {
                // trait2a:

                // trait2b:
                // Mark on enemies decreases speed by 1 per charge. 

                // trait 4a;

                // trait 4b:

                case "mark":
                    traitOfInterest = trait2b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.CharacterStatModified = Enums.CharacterStat.Speed;
                        __result.CharacterStatAbsoluteValuePerStack = -1;
                    }
                    break;
                case "stealth":
                    traitOfInterest = trait2b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Heroes))
                    {
                    }
                    break;
            }
        }

        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        // public static void HealAuraCursePrefix(ref Character __instance, AuraCurseData AC, ref int __state)
        // {
        //     LogInfo($"HealAuraCursePrefix {subclassName}");
        //     string traitOfInterest = trait4b;
        //     if (IsLivingHero(__instance) && __instance.HaveTrait(traitOfInterest) && AC == GetAuraCurseData("stealth"))
        //     {
        //         __state = Mathf.FloorToInt(__instance.GetAuraCharges("stealth") * 0.25f);
        //         // __instance.SetAuraTrait(null, "stealth", 1);

        //     }

        // }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "BonusResists")]
        public static void BonusResists(ref Character __instance,
            ref int __result,
            Enums.DamageType damageType,
            string acId = "",
            bool countChargesConsumedPre = false,
            bool countChargesConsumedPost = false,
            CardData cardAux = null)
        {
            // trait4b:
            // Enemies lose 1% all resistances for every 2 speed below 0. 
            LogInfo($"HealAuraCursePrefix {subclassName}");
            string traitOfInterest = trait4b;
            if (IsLivingNPC(__instance) && AtOManager.Instance.TeamHaveTrait(traitOfInterest))
            {
                LogDebug($"Applying {traitOfInterest} BonusResists reduction to {__instance.Id} with speeds {string.Join(",", __instance.GetSpeed())}");
                int speed = __instance.GetSpeed()[2];
                int resistReduction = Mathf.FloorToInt(Mathf.Abs(Mathf.Min(speed, 0)) / 2f);
                __result -= resistReduction;
            }

        }




        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isDamagePreviewActive = false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        // public static void SetEventPostfix(
        //     Enums.EventActivation theEvent,
        //     Character target = null,
        //     int auxInt = 0,
        //     string auxString = "")
        // {
        //     if (theEvent == Enums.EventActivation.BeginTurnCardsDealt && AtOManager.Instance.TeamHaveTrait(trait2b))
        //     {
        //         string cardToPlay = "tacticianexpectedprophecy";
        //         PlayCardForFree(cardToPlay);
        //     }

        // }





        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(CardData), nameof(CardData.SetDescriptionNew))]
        // public static void SetDescriptionNewPostfix(ref CardData __instance, bool forceDescription = false, Character character = null, bool includeInSearch = true)
        // {
        //     // LogInfo("executing SetDescriptionNewPostfix");
        //     if (__instance == null)
        //     {
        //         LogDebug("Null Card");
        //         return;
        //     }
        //     if (!Globals.Instance.CardsDescriptionNormalized.ContainsKey(__instance.Id))
        //     {
        //         LogError($"missing card Id {__instance.Id}");
        //         return;
        //     }


        //     if (__instance.CardName == "Mind Maze")
        //     {
        //         StringBuilder stringBuilder1 = new StringBuilder();
        //         LogDebug($"Current description for {__instance.Id}: {stringBuilder1}");
        //         string currentDescription = Globals.Instance.CardsDescriptionNormalized[__instance.Id];
        //         stringBuilder1.Append(currentDescription);
        //         // stringBuilder1.Replace($"When you apply", $"When you play a Mind Spell\n or apply");
        //         stringBuilder1.Replace($"Lasts one turn", $"Lasts two turns");
        //         BinbinNormalizeDescription(ref __instance, stringBuilder1);
        //     }
        // }

    }
}

