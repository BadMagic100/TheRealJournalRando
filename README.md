# TheRealJournalRando

A Hollow Knight mod that adds Hunter's Journal Entries and Hunter's Notes to the randomization pool.
This mod adds items and locations for every journal entry that is not included in base rando/itemchanger,
with the following exceptions of Shade, which is always granted when gaining the Hunter's journal to prevent
the UI from breaking.

## Settings

When enabled with rando, this mod will provide several connection settings. More information about each
is provided below, grouped by category.

### Basic Settings

* **Randomization type** - one of the following:
  * None - enable the connection without randomization - this is useful for use with custom pools
    or vanilla itemsync.
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
