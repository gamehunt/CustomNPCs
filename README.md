# CustomNPCs
Plugin is currently under development, so new features will be added soon

Permission node: `npc.all`

For usage instructions see https://github.com/gamehunt/CustomNPCs/wiki/Get-started


** AI and Nav system are experimental features, they are under development and can work not as expected / dont work at all**

Im also selling `Pets` addon for this plugin, dm me in discord if u're interested (gamehunt#6523)

Known Issues:

 ~~- Navigation is really stupid. Botz can't go in servers, they dont know about any stairs~~
 ~~- Well, I've made it little better with manual mappings. Now bad room is HCZ_Armory. Next task: Learn them how to use lifts~~
 - Navigation finally fixed (Except GateA and GateB, idfk why their mappings work only in 1/3 times. Also they sometimes can stuck in the walls while using elevators...)

FAQ:

 - Q: NPC is not walking with AINavigateToRoomTarget/GoToRoomAction
 
   A: Make sure u've enabled `generate_navigation_graph` in your `<port>-config.yml` U need to do round restart after that!
   
 - Q: Console commands dont work
   
   A: Try updating all plugins to their latest versions (especially `scpswap` !!!) 
   
Todo:

- Add more events

- Better nav system

- Not retarded AI
