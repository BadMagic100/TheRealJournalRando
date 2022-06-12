# Contributing to Logic

Hello and thank you for considering contributing to the logic for Journal locations!

Location logic for this mod is almost exclusively handled by Defeated_Any_<Enemy_Name> waypoints in the
[waypoints.json](TheRealJournalRando/Resources/Logic/waypoints.json) file. A handful of special locations
are defined in [enemyLocations.json](TheRealJournalRando/Resources/Logic/enemyLocations.json), with combat
logic defined in [macros.json](TheRealJournalRando/Resources/Logic/macros.json).

You can submit logic changes as pull requests in this repo (this is a Github thing, google it if you're
unfamiliar). Please read the below instructions for info on logic requirements and style guidelines before
contributing.

## Help wanted

Currently, I am looking for contributions to fill in the Defeated_Any_<Enemy_Name> waypoints. These should
essentially be giant OR expressions listing out all the ways to reach and kill a given enemy type. Logic for
enemies that don't appear in the list is not needed at this time (it's already handled separately).

## Logic requirements

* With the exception of Oro and Mato, Sheo, Sly, and Pure Vessel in Pantheons, going to Godhome is not
  logically required.
* Enemies that get replaced or removed when crossroads is infected are not logically required.
* The non-respawning Husk Miners (Myla and the one outside Deep Focus room) are not logically required.

## Writing logic

If you're unfamiliar with writing logic, it's quite simple. You only need to compile a list of relevant terms
using AND (`+`) and OR (`|`) operations. Terms can be obtained items, macros, waypoints, or transitions.
Usually, the best way to write logic is starting from a transition or other waypoint and indicate the items
needed to get where you're going in that room. For example, the logic for the Sharp Shadow check is
`Deepnest_44[top1] + RIGHTSHADOWDASH + (LEFTSHADOWDASH | 4MASKS)`, indicating you enter the room from the top1
transition and need right shade cloak (to get into the shade gate) and one of either left shade cloak or enough
masks to damage tank (to get to the check).

## Style guide

When contributing logic, please follow the below style restrictions.

### Use parentheses rather than relying on operator precedence

I don't know whether ANDs or ORs take precedence, and I don't expect you or casual logic-readers to know either.
Play it safe and group relevant terms in parentheses to keep logic readable and maintainable.

### Waypoints/transitions come first

Always start your logic clauses with the transition or waypoint that lets you enter the room, where appropriate.
If there are multiple clauses OR'd together, this is true of each clause.

### Reuse existing logic when possible

* Check if logic exists already that meets your needs in Rando's [macros.json](https://github.com/homothetyhk/RandomizerMod/blob/master/RandomizerMod/Resources/Logic/macros.json),
  and [waypoints.json](https://github.com/homothetyhk/RandomizerMod/blob/master/RandomizerMod/Resources/Logic/waypoints.json).
  Use these when possible (for waypoints, you can insert the waypoint's name in your logic, for macros use the
  dictionary key e.g. `LEFTFIREBALL`). When writing logic for a room, if you can get the check from several
  transitions, check to see if there is a waypoint for that room (e.g. `Abyss_05` is shorthand for 
  `Abyss_05[left1] | Abyss_05[right1] | Warp-White_Palace_Entrance_to_Palace_Grounds | Warp-White_Palace_Atrium_to_Palace_Grounds`).
* Note that many bosses have existing `Defeated_<Enemy_Name>` and/or combat macros that you can reuse. For example,
  the logic for `Defeated_Any_Hornet` is simply `Defeated_Hornet_1 | Defeated_Hornet_2` - this is even simpler
  for enemies that only have one variant.
* If there is not an existing macro or waypoint, but the check is logically equivalent to an existing location,
  you can reference that location using `*Location`. As a toy example, defeating Crystal Guardian is the same
  as `*Boss_Geo-Crystal_Guardian`.
* When defining combat macros, consider using referencing existing combat macros like `BOSS` or `MINIBOSS` to
  encode gear requirements.

### Define new waypoints if and only if appropriate

* If you expect to use a lengthy logic string several times (for example, access to a room via any transition),
  define a new waypoint for it if that doesn't exist already.
* If defining entering an area via a warp (such as the warp from Junk Pit to Godhome), define that as a
  waypoint.

### Use `COMBAT` macros effectively

* If your enemy needs combat logic, define the combat macro in the macro file rather than hardcoding combat
  requirements in the location/waypoint logic.
* Most enemies do not need special combat logic; this is mainly reserved for bosses and minibosses. If, after
  using your best judgement you determine you don't need a Combat macro for a given enemy, please remove it from
  the macro file.
* If a given enemy may warrant a combat macro in the future, please *do* use the macro in the waypoint logic
  but leave the macro expression as `ANY` to allow future updates.
* If you're using a combat macro, place it as the last term in the corresponding waypoint.
