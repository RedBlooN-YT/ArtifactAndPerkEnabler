global using BTD_Mod_Helper.Extensions;
using MelonLoader;
using BTD_Mod_Helper;
using ArtifactAndPerkEnabler;
using BTD_Mod_Helper.Api.ModOptions;
using System.Collections.Generic;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Models.Artifacts;
using Il2CppInterop.Runtime;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Artifacts;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Data.Knowledge;
using System;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Rounds;
using Il2CppAssets.Scripts.Data.Legends;
using Il2CppAssets.Scripts.Data.SocialSeasons;
using Il2CppAssets.Scripts.Models.Profile;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Models.CorvusSpells.Instant;
using UnityEngine;
using Il2CppAssets.Scripts.Unity;

[assembly: MelonInfo(typeof(ArtifactAndPerkEnabler.ArtifactAndPerkEnabler), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6-Epic")]

namespace ArtifactAndPerkEnabler;

public class ArtifactAndPerkEnabler : BloonsTD6Mod
{
    public static List<string> artifacts = new();
    public static HashSet<string> ForcedPerks = new HashSet<string>();

    public override void OnProfileLoaded(ProfileModel result)
    {
        var mod = ModContent.GetInstance<ArtifactAndPerkEnabler>();
        foreach (var perk in GameData.Instance.perkData.perkDatas)
        {
            mod.ModSettings[perk.key] = new ModSettingBool(false)
            {
                displayName = perk.key,
                description = $"Enable {perk.key}",
                category = Perks,
                icon = perk.Value.model.icon.AssetGUID
            };
        }

        foreach (var artifact in GameData.Instance.artifactsData.artifactModelsByType[Il2CppType.Of<ItemArtifactModel>()])
        {
            mod.ModSettings[artifact.ArtifactName] = new ModSettingBool(false)
            {
                displayName = artifact.ArtifactName,
                description = $"Enable {artifact.ArtifactName}",
                category = Artifacts,
                icon = artifact.icon.AssetGUID
            };
        }
        foreach (var artifact in GameData.Instance.artifactsData.artifactModelsByType[Il2CppType.Of<BoostArtifactModel>()])
        {
            mod.ModSettings[artifact.ArtifactName] = new ModSettingBool(false)
            {
                displayName = artifact.ArtifactName,
                description = $"Enable {artifact.ArtifactName}",
                category = Artifacts,
                icon = artifact.icon.AssetGUID
            };
        }
        foreach (var artifact in GameData.Instance.artifactsData.artifactModelsByType[Il2CppType.Of<MapArtifactModel>()])
        {
            mod.ModSettings[artifact.ArtifactName] = new ModSettingBool(false)
            {
                displayName = artifact.ArtifactName,
                description = $"Enable {artifact.ArtifactName}",
                category = Artifacts,
                icon = artifact.icon.AssetGUID
            };
        }
    }
    public static ModSettingCategory Artifacts = new ModSettingCategory("Artifacts")
    {
        icon = VanillaSprites.ArtifactPowerIcon
    };
    public static ModSettingCategory Perks = new ModSettingCategory("Perks")
    {
        icon = VanillaSprites.PerksIconSmall
    };
    [HarmonyPatch(typeof(InGame), nameof(InGame.Initialise))]
    public static class AfterInGameStartPatch
    {
        static void Postfix(InGame __instance)
        {
            if (Game.Player.IsFlagged == true)
            {
                if (Game.Player.Data.purchase.HasMadeOneTimePurchase("btd6_legendsrogue"))
                {
                    var sim = __instance.bridge.Simulation;
                    var am = sim.artifactManager;
                    artifacts.Clear();

                    var mod = ModContent.GetInstance<ArtifactAndPerkEnabler>();

                    foreach (var artifact in GameData.Instance.artifactsData.artifactModelsByType[Il2CppType.Of<ItemArtifactModel>()])
                    {
                        var setting = (ModSettingBool)mod.ModSettings[artifact.ArtifactName];

                        if ((bool)setting.GetValue())
                        {
                            artifacts.Add(artifact.ArtifactName);
                        }
                    }
                    foreach (var art in artifacts)
                    {
                        am.Activate(art);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(GameModel), nameof(GameModel.CreateModded))]
    [HarmonyPatch(new Type[] {
    typeof(Il2CppSystem.Collections.Generic.List<string>),
    typeof(ModModel),
    typeof(ActiveRelicKnowledge),
    typeof(MapModel),
    typeof(RoundSetModel),
    typeof(Il2CppSystem.Collections.Generic.List<ArtifactLoot>),
    typeof(Il2CppSystem.Collections.Generic.HashSet<SeasonPerkItem>)
})]
    public static class ApplyForcedPerkPatch
    {
        static void Prefix(
            Il2CppSystem.Collections.Generic.List<string> activeMods,
            ModModel dcmModModel,
            ActiveRelicKnowledge activeRelicKnowledge,
            MapModel map,
            RoundSetModel roundSet,
            Il2CppSystem.Collections.Generic.List<ArtifactLoot> artifactsInventory,
            Il2CppSystem.Collections.Generic.HashSet<SeasonPerkItem> activePerks
        )
        {
            if (Game.Player.IsFlagged == true)
            {
                ForcedPerks.Clear();

                var mod = ModContent.GetInstance<ArtifactAndPerkEnabler>();

                foreach (var perk in GameData.Instance.perkData.perkDatas)
                {
                    var setting = (ModSettingBool)mod.ModSettings[perk.key];

                    if ((bool)setting.GetValue())
                    {
                        ForcedPerks.Add(perk.key);
                    }
                }
                var perkDefs = GameData.Instance.perkData.perkDatas;

                var forced = new Il2CppSystem.Collections.Generic.HashSet<SeasonPerkItem>();

                foreach (var key in ForcedPerks)
                {
                    if (perkDefs.TryGetValue(key, out var perk))
                    {
                        forced.Add(perk);
                    }
                }
                activePerks.Clear();
                foreach (var perk in forced)
                {
                    activePerks.Add(perk);
                }
            }
        }
    }
}