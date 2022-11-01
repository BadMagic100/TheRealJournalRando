# TheRealJournalRando

A Hollow Knight mod that adds Hunter's Journal Entries and Hunter's Notes to the randomization pool.
This mod adds items and locations for every journal entry that is not included in base rando/itemchanger,
with the exception of Shade, which is automatically granted as usual when gaining the Hunter's Journal.

This readme is broken into 4 major sections, ordered by importance:
* [Items, Costs, and Locations](#Items-Costs-and-Locations) explains the ItemChanger items, locations, and costs added
  by this mod. **This section is 
  highly recommended reading whether you are developer using these in your own mod or a player using the randomizer
  connection.**
* [Logic Info](#Logic-Info) explains details of how logic works in the randomizer connection, including a few quirks that
  may not be immediately obvious. **This section is highly recommended reading before playing the randomizer 
  connection.**
* [Settings](#Settings) explains the various settings available in the randomizer connection in more detail.
* [Other Mods](#Other-Mods) explains some of the unique ways this mod interacts with other rando mods.

## Items, Costs, and Locations

This mod introduces 2 new items types, 2 new location types, and 1 new payable cost type. Each killable enemy has 2
new items and locations corresponding to unlocking the Hunter's Journal entry and completing the Hunter's notes for that
enemy. Crawlids, which are normally granted automatically when gaining the Hunter's Journal, are treated the same as other
enemies if any of these items or locations is placed, and require 30 kills for the Hunter's notes. Extra (non-killable) 
entries, including Hunter's Mark, Void Idol, and Weathered Mask, each have 1 new item and location added that corresponds 
to unlocking the entire entry and Hunter's notes, similar to the journal entries in base rando.

### Items

Journal entry items and Hunter's notes items are provided for each killable enemy type. If both items are placed for
a given enemy, they act progressively (in other words, you are guaranteed the entry before the notes). If only one 
item is placed, the other behaves as it would in vanilla. If only the entry item is placed, killing the normal amount
of that enemy will unlock the hunter's notes text. If only the notes item is placed, you must kill one enemy of the
appropriate type before viewing the entry in the journal. Regardless of which items are or are not placed, you must
obtain the Hunter's journal to be able to view any entry in the journal.

### Costs

This mod adds a cost which represents killing a certain number of a given enemy. All killable enemies are counted,
including most Godhome bosses (more details on how this works in logic below). Godhome Nightmare King Grimm and
Crystal/Enraged guardian do not count, as they don't count towards the journal entry/hunter's notes in vanilla HK.
These costs can be applied on any location, though the randomizer connection only applies them to the journal entry and
Hunter's notes locations.

### Locations

Journal entry and Hunter's notes locations are very nearly indistinguishable aside from costs and previews. Both locations
work like this:
* When you kill an enemy of the appropriate type, check if there are any costs on the location.
* If the cost is payable, pay it and give the item(s). If the cost is not payable, do nothing
* If there is no cost, give the item(s).

In addition, the items and costs of the Hunter's notes location can be previewed in the notes pane of the Hunter's
journal once you've obtained both the Hunter's journal and the corresponding Journal entry (either from
obtaining the placed item, or by killing the appropriate enemy type if the entry is not placed). This preview is either
in place of the "Defeat X more to decipher the Hunter's notes" text if you have not obtained the notes item, or after
the end of the Hunter's notes if you have obtained the items.

## Logic Info

Mostly, logic works how you would expect, with a few quirks to note.
* Both Journal entry and Hunter's notes locations will be in logic once you can kill any enemy of the appropriate type,
  as the logic is primarily controlled by cost (see below).
* Because Hunter's notes location may only require 1 kill, either if the cost is 1 kill or if there is a different
  cost type on the location (perhaps placed by another connection), the Hunter's notes location will show up as
  "reachable" in the helper log even if it's not currently possible to kill multiple enemies of that type (for example,
  `Hunter's_Notes-Hornet` with no access to Kingdom's Edge). These costs are taken into account in logic though,
  so in such a case you can't have required progression locked behind an area you need that progression for. This is
  similar to how the checks at Seer are "in logic" even without having Dream Nail or enough essence to pay costs.
* Although Godhome bosses count as kills, this is not required in logic except for checks that are exclusive to Godhome.
  Counting Godhome bosses exists only to allow other mods to implement costs larger than 1 for bosses.
* Costs of higher than 1 enemy require either access to a bench or for the enemy to automatically respawn when reloading
  the room.
* Most enemies with a finite or semi-finite supply are handled appropriately in logic (in other words, it is not
  assumed that they respawn). This includes Elder Baldurs, the Vengefly King in Greenpath, the Gruz Mother in
  Crossroads, and the Kingsmoulds outside Path of Pain. Enemies that generally respawn, but have special cases
  that do not (such as Husk Miners) were deemed too difficult to handle in logic, and are therefore considered 
  sequence breaks. This means that you never have to logically kill Myla, among other things.
* Enemies in Crossroads that are replaced after infection are not considered in logic to prevent you from being locked
  out of the check. 
* Because Infected enemies require infection to kill, it's probably slightly more likely you'll see early infection so
  rando can put those checks in logic.

## Settings

When enabled with rando, this mod will provide several connection settings. More information about each
is provided below, grouped by category. This mod has a large number of settings, and provides its own
settings code. Clicking the button to copy the code will automatically copy the code to your clipboard,
and clicking the button to apply the code will read a code on your clipboard and apply the settings.

The mod comes with some default settings enabled; I recommend these on your first playthrough to get the
full experience of the mod without it being super cursed.

### Basic Settings

* **Randomization type** - one of the following:
  * None - enable the connection without randomization - this is useful for use with custom pools
    or vanilla itemsync.
  * Entries only - For killable enemies, the "location" is killing a single enemy of a given type
    (when the enemy would normally become visible in the journal). The corresponding item will unlock the
    journal entry. Killing the default number of enemies will unlock the Hunter's notes as usual.
  * Notes only - For killable enemies, the "location" is killing many enemies of a given type (cost varies
    by enemy and cost settings). The corresponding item will unlock only the Hunter's notes. Killing a 
    single enemy will unlock the entry as usual. The item at Hunter's notes locations can be previewed in
    the journal once you have the corresponding entry, and regardless of whether or not you have the notes
    item.
  * Full Journal Rando - Both of the location types described above will exist. There will be 2 corresponding
    item types, one which grants only visibility in the journal, and one which grants the Hunter's notes.
    The items are progressive, so you are guaranteed to receive them in order.
* **Starting Items** - useful items you might want to start with.
  * None - no additional starting items
  * Hunter's Journal - grants the Hunter's Journal to make it more convenient to preview note locations
    by killing enemies.
  * Tier 1 Entries - grants the first tier of journal entries for all enemies. Makes it more convenient to
    preview, but still requires you to randomly (or vanilla-ly) obtain the Hunter's Journal first.
  * Journal and Entries - grants both the Hunter's Journal and the first level of entries for all enemies.
* **Hunter's Notes Previews** - How previews will be handled at Hunter's notes locations. The options
  are the same shop preview settings in base rando.
* **Dupe Hunter's Journal** - For helping get access to the above previews.

### Pool Settings

* **Regular Entries** - a toggle to determine whether entries for normal enemies that are required for
  the Keen Hunter achievement will be randomized.
* **Boss Entries** - a toggle to determine whether entries for bosses that are required for the Keen Hunter
  achievement will be randomized.
* **Bonus Entries** - a toggle to determine whether entries for enemies that are not required for the
  Keen Hunter achievement will be randomized. Regular/Boss entries settings will be respected here as well.

### Cost Settings

* **Minimum Cost Weight** - a number between 0 and 1 representing the minimum percentage of the vanilla cost
  the Hunter's notes locations can cost.
* **Maximum Cost Weight** - a number between 0 and 1 representing the maximum percentage of the vanilla cost
  the Hunter's notes locations can cost.
* **Cost randomization** - one of the following:
  * Off - costs will not be randomized.
  * Random (fixed weight) - A single random weight will be chosen and applied to all randomized entries. 
    All journal locations will always cost at least 1 kill.
  * Random per entry - A random weight will be chosen for each randomized entry. Hunter's notes locations
    will always cost at least 1 kill.

### Long Location Settings

* **Randomize Menderbug** - whether the Menderbug entry will be randomized.
* **Randomize Hunter's Mark** - whether the Hunter's Mark should be randomized 
  (tied to the items, not the locations)
* **Randomize Pantheon Bosses** - whether the entries for the bosses at the end of the first 4 pantheons
  (Oro & Mato, Sheo, Sly, and Pure Vessel) will be randomized.
* **Randomize Weathered Mask** - whether the Weathered Mask entry will be randomized.
* **Randomize Void Idol** - the maximum level of the Void Idol entry to randomize.

### Base Rando Settings

Journal rando respects several settings from base rando:
* The deranged setting is implemented for journals.
* If using split groups, these journal entries will be placed in the same split group as other journal
entries (specifically, as Hunter's Journal).
* If White Palace as a whole is excluded, the items and locations for Royal Retainer, Wingmoulds, and 
Kingsmoulds will be treated as vanilla. If you've randomized Hunter's Mark, you may still be expected 
to enter palace to collect the required Hunter's notes to get the check at Hunter's Mark. If you exclude 
only Path of Pain, the Kingsmoulds at the end are not required to pay costs or check the corresponding 
journal locations. With Path of Pain excluded from randomization there is always enough Wingmoulds 
outside of Path of Pain so that the Wingmoulds in PoP may be ignored.

## Other Mods

This mod is the first mod that provides checks which are accessible in more than one place. As such,
it interacts in unique ways with many other mods which have traditionally expected that checks can only
appear in a single room. This mod provides full information about scenes, titled areas, and map areas in
which an enemy appears so that other mods can use that info to provide a more integrated experience.

### RandoMapMod

Enemies that appear in multiple rooms appear in the bottom left corner of the world map.
Each pin is marked with the corresponding enemy's icon. These icons will also appear around the edge
of the quick map if the enemy appears in that area. Additionally, if pin selection (Ctrl + P) in RMM is
enabled, the rooms that the enemy appears in are highlighted. You can lock the selection to scroll around
the map and get a better view of the highlighted rooms.

### RandoPlus

In Area Blitz, an enemy's location(s) is selected if the enemy is available in at least one of the selected
map areas.

### TrandoPlus

With the Remove Random Rooms setting, an enemy's location(s) is selected if at least one of its rooms is
available. With the Remove Empty Rooms setting, each enemy is randomly assigned one if its rooms.

### MajorItemsByAreaTracker/RandoStats

If the enemy is available in only a single map area, its locations are counted under that area. If the
enemy is available in multiple map areas, its locations will be counted under "Other."

### RandoSettingsManager

This connection is fully integrated with RandoSettingsManager. It does not need to be configured manually
after saving/sharing/loading settings and will disable itself if no settings are available.

