﻿using ItemChanger;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheRealJournalRando.Data
{
    /// <summary>
    /// Broad, multipurpose definition of an enemy for a journal entry
    /// </summary>
    /// <param name="icName">The name of the enemy (IC friendly). Usually, this the English enemy name with spaces replaced by underscores.</param>
    /// <param name="pdName">The name of the enemy in playerdata.</param>
    /// <param name="convoName">The name of the enemy in language keys.</param>
    /// <param name="isBoss">Whether the enemy is a boss.</param>
    /// <param name="singleSceneName">If the enemy appears in only a single scene, the scene that the enemy appears in. 
    /// If defined, singleTitledArea and singleMapArea are not needed.</param>
    /// <param name="singleTitledArea">If the enemy appears in only a single titled area, the titled area the enemy appears in. If defined, singleMapArea is not needed.</param>
    /// <param name="singleMapArea">If the enemy appears in only a single map area, the map area the enemy appears in.</param>
    /// <param name="allScenes">A list of scenes the enemy can logically appear in (if there are more than 1)</param>
    /// <param name="allTitledAreas">A list of titled areas the enemy can logically appear in (if there are more than 1)</param>
    /// <param name="allMapAreas">A list of map areas the enemy can logically appear in (if there are more than 1)</param>
    /// <param name="ignoredForHunterMark">Whether the enemy will be ignored in counting hunter's mark (i.e. it's bonus content).</param>
    /// <param name="ignoredForJournalCount">Whether the enemy, when ignored for hunter's mark, should also be ignored when counting total number of entries.</param>
    /// <param name="respawns">Whether the enemy respawns without a bench.</param>
    /// <param name="unkillable">Whether the enemy is a "special" enemy, such as Goam, Garpede, etc that can't be killed.</param>
    /// <param name="notesCost">The number of kills needed for hunter's notes in base game.</param>
    /// <param name="icIgnore">Whether to ignore this enemy when creating items and locations in IC generically (i.e. needs special handling in IC).</param>
    /// <param name="logicItemIgnore">Whether to ignore this enemy when defining logic items generically (e.g. doesn't have the "standard" entry and note items).</param>
    /// <param name="logicLocationIgnore">Whether to ignore this enemy when defining logic locations generically (e.g. special handling for location logic).</param>
    /// <param name="requestDefineIgnore">Whether to ignore this enemy when defining item/location defs and costs (e.g. doesn't have hunter's notes).</param>
    /// <param name="requestAddIgnore">Whether to ignore this enemy when defining pool requests generically (e.g. uses long location settings).</param>
    /// <param name="index">The index of the enemy in the journal data, to provide to data consumers such as map mod to sort. Omit from json,
    /// the correct value will be inferred when deserializing.</param>
    public record EnemyDef(string icName, string pdName, string convoName, bool isBoss, 
        string? singleSceneName, string? singleTitledArea, string? singleMapArea,
        List<string>? allScenes, List<string>? allTitledAreas, List<string>? allMapAreas,
        bool ignoredForHunterMark, bool ignoredForJournalCount, bool respawns, bool unkillable, int notesCost, 
        bool icIgnore, bool logicItemIgnore, bool logicLocationIgnore, bool requestDefineIgnore, bool requestAddIgnore,
        int index);

    public static class EnemyData
    {
        public static readonly IReadOnlyDictionary<string, EnemyDef> Enemies;

        public static readonly string[] BluggsacLocations = {
            LocationNames.Rancid_Egg_Queens_Gardens, LocationNames.Rancid_Egg_Blue_Lake,
            LocationNames.Rancid_Egg_Crystal_Peak_Dive_Entrance, LocationNames.Rancid_Egg_Crystal_Peak_Tall_Room,
            LocationNames.Rancid_Egg_Beasts_Den, LocationNames.Rancid_Egg_Dark_Deepnest,
            LocationNames.Rancid_Egg_Near_Quick_Slash, LocationNames.Rancid_Egg_Waterways_East,
            LocationNames.Rancid_Egg_Waterways_Main, LocationNames.Rancid_Egg_Waterways_West_Bluggsac
        };

        static EnemyData()
        {
            Enemies = LoadJournalData("TheRealJournalRando.Resources.journalData.json");
        }

        private static IReadOnlyDictionary<string, EnemyDef> LoadJournalData(string file)
        {
            using Stream s = typeof(EnemyData).Assembly.GetManifestResourceStream(file);
            using StreamReader sr = new(s);
            List<EnemyDef>? data = JsonConvert.DeserializeObject<List<EnemyDef>>(sr.ReadToEnd());
            if (data == null)
            {
                throw new IOException("Failed to load enemy definitions");
            }

            return data.Select((e, i) => e with { index = i }).ToDictionary(e => e.icName);
        }
    }
}
