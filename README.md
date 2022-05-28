# TheRealJournalRando

A Hollow Knight mod that adds Hunter's Journal Entries and Hunter's Notes to the randomization pool.
This mod adds items and locations for every journal entry that is not included in base rando/itemchanger,
with the following exceptions of Shade, which is always granted when gaining the Hunter's journal to prevent
the UI from breaking.

## Settings

When enabled with rando, this mod will provide several connection settings. More information about each
is provided below, grouped by category.

### Basic Settings

* **Define Refs** - if entries are not randomized, tells rando that entries are vanilla in logic (rather
  than not being items). Useful for using custom pool/group injectors.
* **Randomization type** - one of the following:
  * Entries only - For killable enemies, the "location" is killing a single enemy of a given type
    (when the enemy would normally become visible in the journal). The corresponding item will unlock the
    journal entry. Killing the default number of enemies will unlock the Hunter's notes as usual.
  * Notes only - For killable enemies, the "location" is killing many enemies of a given type (when the
    Hunter's notes would normally become visible in the journal, varies by enemy and cost settings). The
    corresponding item will unlock only the Hunter's notes. Killing a single enemy will unlock the entry
    as usual.
  * Full Journal Rando - Both of the location types described above will exist. There will be 2 corresponding
    item types, one which grants only visibility in the journal, and one which grants the Hunter's notes.
    The items are progressive, so you are guaranteed to receive them in order.
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
  the Hunter's notes locations can cost (tentative - can this be above 1? would need special handling
  for enemies with hard caps on the number of killable enemies, e.g. bosses)
* **Cost randomization** - one of the following:
  * Off - costs will not be randomized.
  * Random (fixed weight) - A single weight will be chosen and applied to all randomized entries. Hunter's
    notes locations will always cost at least 1 kill.
  * Random per entry - A unique weight will be chosen for each randomized entry. Hunter's notes locations
    will always cost at least 1 kill.

### Preview settings

* **Cost Previews** - determines whether you will be able to see the cost of the Hunter's notes locations
  when placed.
* **Item Previews** - determines whether you will be able to see the item(s) placed at the Hunter's notes
  locations when placed.

### Long Location Settings

* **Randomize Void Idol** - a number between 0 and 3 representing how many levels of the Void Idol entry will
  be randomized.
* **Randomize Weathered Mask** - whether the Weathered Mask entry will be randomized.
* **Randomize Menderbug** - whether the Menderbug entry will be randomized.
* **Randomize Hunter's Mark** - whether the Hunter's Mark will be randomized.
* (tentative) **Grind Godhome Bosses** - whether godhome enemies can be used to allow bosses to have
  higher costs than default.
