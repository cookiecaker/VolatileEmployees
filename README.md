# Volatile Employees

## Basics 

The Company has upgraded their defense equipment! All employees are now equipped with state-of-the-art explosive technology to take down the pesky creatures preventing them from doing their job. Good luck, and have fun! 

This is a mod that causes many entities to explode on impact with the player, and players themselves to explode when dying from other sources. It is fully networked, so it can work with everyone as long as they all have the mod! 

## Config

* `playerImmunity` - set true by default. This will make players immune to the explosions they cause (as well as all other exploding things - landmines, missiles, etc...). Turning this off will make for a much harder experience!
* `enemiesExplode` - set true by default. Turning this off will make enemies stop exploding, so the only abnormal source of explosions will be from the player dying.
* `patchGiantKiwi` - set false by default. Prevents a bug where players are slowed to less than a crawl after the Giant Sapsucker explodes... But also prevents the Giant Sapsucker from exploding. Your call which is worse. (If you turn this on, rejoining the lobby will fix the bug)

Config settings are synced with the host!

## Features

Spoilers ahead!

<details><summary>Things that explode</summary>
These creatures won't get the chance to damage you before exploding. Your defense equipment is working perfectly!
<br><br>
Baboon Hawks, Barbers, Bunker Spiders, Butlers, Circuit Bees, Coil-Heads, Eyeless Dogs, Feiopars, Giant Sapsuckers, Hoarding Bugs, Hygroderes, Jesters, Kidnapper Foxes, Maneaters, Mask Hornets, Nutcrackers, Old Birds, Snare Fleas, Spore Lizards, and Thumpers
<br><br>
</details>

<details><summary>Things that don't</summary>
Earth Leviathans - Some creatures are simply too massive for any reasonably priced amount of equipment to be effective against. If you absolutely need to take down one of these beasts, we suggest bringing large amounts of dynamite! (Note: The Company does not sell nor endorse the selling of dynamite to employees.)
<br><br>
Nutcrackers - Their firearms are unable to set off our defensive equipment. Try approaching them directly to trigger it!
<br><br>
Gunkfish - This creature isn't dangerous, and could cause more problems to employees if your equipment were to activate against it. Keep these cleaners safe, and they won't bother you!
<br><br>
Blooming - The Company does not guarantee equipment effectiveness if a Cadaver Growth infection results in enough internal plant matter to incapacitate an employee. However, until it is fully grown, dying to other sources will fry the plants inside and ensure your crew doesn't deal with what remains.
<br><br>
Ghost Girl - The Company has found no reason to believe the supernatural exists and thus has provided no such measures to counteract them. Even if we knew how to do that. Which we do not.
<br><br>
</details>

<details><summary>Things that die with the player</summary>
Unfortunately, our system isn't perfect. These creatures get ahold of employees before the defensive equipment can be triggered - however, you can rest easy knowing the rest of your crew will be safe from whatever managed to grab you.
<br><br>
Brackens, Masked, Forest Keepers, and Tulip Snakes
</details>

## Credits
* cookiecaker (me) - Programming, thumbnail, video editing
* zeeblo lite - [Modding tutorial](https://www.youtube.com/watch?v=1c4Ut7nINkI) used
* [EvaisaDev](https://thunderstore.io/c/lethal-company/p/Evaisa/) - UnityNetcodePatcher for syncing explosions
* [ButteryStancakes](https://thunderstore.io/c/lethal-company/p/ButteryStancakes/) - `Init()`, `Create()`, and `OnNetworkSpawn()` for the networker and helping me understand networking as a whole; idea and advice for blooming implementation
* [IAmBatby](https://thunderstore.io/c/lethal-company/p/IAmBatby/) - `Add()` for ILExtensions and help with fixing bugs
* [pacoito](https://thunderstore.io/c/lethal-company/p/pacoito/), [Hamunii](https://thunderstore.io/c/lethal-company/p/Hamunii/), [mborsh](https://thunderstore.io/c/lethal-company/p/mborsh/), [XuXiaolan](https://thunderstore.io/c/lethal-company/p/XuXiaolan/) - Lots of help with fixing bugs and using IL code; being very patient with me
* flintchips - Making the song "Xarnip" in the intro video
* [debbicar](https://thunderstore.io/c/lethal-company/p/debit_card_debit/), Neve, Ame, Thia, Limbo, Tood, and Unna - Testing and filming