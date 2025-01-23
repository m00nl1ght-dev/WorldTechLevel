
# World Tech Level

A mod for the game Rimworld.

Limits the level of technology available in the world. Filters out all game content above the desired tech level, which you can select on the world creation page.


# Tech Levels

- **Neolithic**: Clubs and knives, bows, basic clothing, wood structures, simple farming, herbal medicine
- **Medieval**: Swords and plate armor, smithing, advanced farming, complex ovens, simple chemistry
- **Industrial**: Electricity, guns, IED traps, fossil fuels, chemical drugs, prosthetics, transport pods
- **Spacer**: Power armor, pulse-charged projectile weapons, complex body implants, cryptosleep
- **Ultra**: Mechanoids, nanotechnology, glitterworld medicine, advanced energy weapons, brain implants
- **Archotech**: AI personas, psychic effectors, vanometric energy, acausal and atemporal devices

See [RimWorld Wiki](https://rimworldwiki.com/wiki/Lore#Tech_levels) for more information.


# Recommendations

The base game contains very little content for the Medieval tech level.
So if you want to choose this tech level, I recommend adding some mods that add medieval content.
The most popular option is [Medieval Overhaul](https://steamcommunity.com/sharedfiles/filedetails/?id=3219596926), which also provides a well-designed medieval research progression.
Additionally, there are also some great mods that convert various vanilla content into medieval variants.

If you are looking for a curated list of mods for a pure medieval world:
https://steamcommunity.com/sharedfiles/filedetails/?id=3045497390


# FAQ

Can I safely add this mod to an existing save?
- Yes.

Can I safely remove this mod from an existing save?
- Yes.

What is the difference between this mod and similar ones such as "Rimedieval" and "Remove Industrial Stuff"?
- World Tech Level does not modify or remove any defs. This means it is fully save-compatible, less likely to conflict with other mods, and allows for more flexibility because the tech level can be changed at any time.

Does this mod mess with the pawn generation algorithm?
- No, this mod leaves the vanilla algorithm untouched, filters run only after it has completed. No action is taken unless the generated creature has weapons/apparel that exceed the world's tech level.

Can I change the tech level for an existing world?
- Yes, it can be changed in the "Planet" tab on the world map. Only things generated after that point will be affected by the new setting.


# Filters

All filters are optional and can be toggled on/off in the mod settings. If all filters are turned off, the mod does nothing.

- **Research**: Filters research projects available to the player. Also filters the corresponding techprints.
- **Items**: Filters quest rewards, trader stock and other randomly generated sources of items in the world.
- **Quests**: Filters out quests and quest sub nodes that contain content exceeding the world's tech level. Only filters vanilla and DLC quests. Does not reduce the overall amount of quests given.
- **Incidents**: Filters out storyteller-triggered incidents that contain content exceeding the world's tech level, for example mech clusters or toxic fallout in a medieval world. Only filters vanilla and DLC incidents.
- **Factions**: Filters factions based on the tech level defined by their author. Also applies to temporary factions generated for quests.
- **Creatures**: Replaces creatures that exceed the world's tech level with appropriate alternatives, for example in quest sites and trade caravans. Only affects vanilla and DLC creatures.
- **Apparel**: After a new creature is generated, if any piece of apparel exceeds the world's tech level, that specific piece is replaced with the equivalent of lower tech level, for example Flak Jacket -> Plate Armor.
- **Weapons**: After a new creature is generated, if its weapon exceeds the world's tech level, it is replaced with the lower tech level equivalent of the same weapon class, for example Assault Rifle -> Recurve Bow.
- **Possessions**: Filters initial inventory items that colonists and enemies can generate with, such as food, medicine and drugs.
- **Prosthetics**: Prevents creatures from generating with artificial body parts or prosthetics that exceed the world's tech level.
- **Backstories**: Filters backstories based on assumed tech level. Note that the classification is keyword based and not perfect. Please report any issues in the Bug Reports section on the Steam workshop page.
- **Traits**: Prevents creatures from generating with traits that conflict with the word's tech level, such as the Transhumanist trait in a medieval world.
- **Diseases**: Filters technology related diseases such as Sensory Mechanites if the world's tech level is below Ultra.
- **Addictions**: Prevents addictions from randomly being applied to new creatures if the corresponding drug would be unobtainable because it exceeds the world's tech level.
- **Damage types**: If the world's tech level is below Industrial, prevents creatures from generating with gunshot wounds/scars.
- **Ideoligions**: Filters possible memes and precepts of ideoligions. Affects both randomly generated faction ideoligions and the player ideoligion editor.
- **Xenotypes**: Filters possible options when a xenotype is randomly assigned to a creature. Does not affect creatures that are generated with a fixed xenotype, for example as part of some quests and incidents.
- **Building materials**: Filters the materials of ruins and other structures on the map. Only affects map generation.
- **Mineable resources**: Filters mineable ores generated on the map. For example, Compacted Components will not appear if the world's tech level is below Industrial. Also affects meteors.
- **Ancient debris**: Filters ancient structures and debris on the map, such as derelict vehicles, mechanoid parts and other junk.
- **Asphalt roads**: Prevents asphalt roads from being generated into the world if tech level is below Industrial.


# Customization

The tech level of individual things in the game can be customized in the "Overrides" tab of the mod settings.

List of def types that can be customized:
- Resources, items, food
- Research projects
- Weapons, apparel
- Buildings, floors
- Backstories, traits
- Incidents, quests
- Map generation steps
- World generation steps
- Ideology memes, precepts
- Biotech xenotypes


# Compatibility

No incompatibilities have been reported so far.

If you find any issues, please let me know.

When reporting bugs or mod conflicts, always upload your log file via HugsLib (press Ctrl + F12 directly after the problem happened) and include the link in your message. Without a log file there is no way for me to figure out what exactly is wrong or how to fix it.


# Credit

The preview image contains graphics from [vecteezy.com](https://www.vecteezy.com/)


# Links

Steam Workshop:
TBD


# License

Copyright (c) 2025 m00nl1ght <<https://github.com/m00nl1ght-dev>>

<a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/"><img alt="Creative Commons Licence" style="border-width:0" src="https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png" /></a><br />All contents of this work (including source code, assemblies and other files), except where a different license is specified within the file itself, are licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/">Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License</a>.

In addition to this license, content creators on YouTube and Twitch are expressly permitted to incorporate this work within their (monetized) videos and livestreams.
