# BorderlandsDiscordRP

Description
-----------
This is a program coded in C# that uses [Discord](http://discordapp.com/)'s [Rich Presence](https://discordapp.com/rich-presence) feature to display Borderlands 2 / Borderlands: The Pre-Sequel information on your discord profile while the game is running.
<br>
Information like:
* Current mission selected
* Current character being played
* Current level of said character being played
* How many players in the lobby with you (including you)
* How long you've been playing
* What map you're in
* What game you're playing

![Borderlands 2 Discord RP](https://puu.sh/CiCrw/4cbe503c94.png)
![Borderlands TPS Discord RP](https://puu.sh/CiCvX/cd3260c23f.png)
<br>
Programs like this is possible due to [c0dycode's CommandInjector](https://github.com/c0dycode/BL-CommandInjector), [mopioid's BLIO library](https://github.com/mopioid/BLIO), Discord for making Discord, [Lachee's discord-rpc-csharp library](https://github.com/Lachee/discord-rpc-csharp), and FromDarkHell for uh making this.


Installation
-----------
1. Download [BorderlandsDiscordRP](https://github.com/FromDarkHell/BorderlandsDiscordRP/blob/master/BorderlandsDiscordRP/BorderlandsDiscordRP.rar?raw=true) here.
2. Next off you'll want to create a new [Discord Application](https://discordapp.com/developers/applications/) (Hey. Click on the link)
	  1. Click the big fat button right that says, [Create an Application](https://puu.sh/CiDba/2e3966dfdd.png)
		  * You may need to [verify your email](https://support.discordapp.com/hc/en-us/articles/213219267-Resending-Verification-Email) on Discord
	  2. Next you'll have a page that should look contain a text box that says, [Name](https://puu.sh/CiDFA/824313e5fa.png)
		  * Change the text to anything, if you want to be like *me*, change it to `Borderlands Rich Presence`
	  3. Change the app icon to the `BL2Icon.png` file you downloaded earlier.
	      1. You know that ZIP file you extracted earlier containing files like, `BL2Icon.png`? You'll want to open that folder up again.
	      2. Click and drag that `BL2Icon.png` file onto the picture that looks like ![App Icon](https://puu.sh/CiDQL/a32b8e7948.png)
	  4. Below where the `Name` box was, you'll see some text that says `Client ID` and right below it is some numbers. 
	      1. Write/Copy (Click the Copy button) that down! You'll need it later.
	  5. Click `Save Changes` otherwise Discord'll shake and you'll cause an earthquake or something exciting.
	  6. Next what you'll want to do is update all of the Rich Presence settings. 
	  	  * To go to the rich presence panel, Look ![Here](https://puu.sh/CiDV2/cacfe197fb.png).
	  	  * Click the thing that says, `Rich Presence`. Pretty subtle I know.
	  7. Do the same thing you did with the app icon thing earlier with the `Cover Image`.
	  	  * See Step #3.
	  8. Now you'll want to select **BOTH**, the `BL2Icon.png`, and `TPSIcon.png` to the `Add Image(s)` button.
	  9. Click the `Save Changes` button once more
	  	  * See Step #5.
	  10. Now you're done with Discord!
3. Installing CommandInjector
	  1. Quit the game if running.
	  2. [Download the latest version of `ddraw.dll`/PluginLoader).](https://github.com/c0dycode/BorderlandsPluginLoader/releases)
	  3. Locate the `Win32` folder within your game's `Binaries` folder. ![Win32 folder](https://i.imgur.com/t6OI06l.png)
	  4. Copy `ddraw.dll` to the `Win32` folder. ![ddrawl.dll](https://i.imgur.com/FHfiSqg.png)	  
	  5. In the `Win32` folder, create a folder called `Plugins`. ![Plugins folder](https://i.imgur.com/CDdoKDs.png)
	  6. [Download the latest version of CommandInjector.](https://github.com/c0dycode/BL-CommandInjector/blob/master/CommandInjector.zip)
	  7. Open the `CommandInjector.zip` file to view its contents. ![CommandInjector.zip](https://i.imgur.com/r1I3b26.png)
	  8. Copy `CommandInjector.dll` (If you're installing it for BL2) or `CommandInjectorTPS.dll` (If you're installing it for TPS) to the `Plugins` folder you created. ![CommandInjector.dll](https://i.imgur.com/U9OSqcV.png)
	  8. From the folder with the `BL2Icon.png` stuff, copy the `Borderlands RP.exe` program wherever you would like, then run it like you do with *programs*! (BL2 or TPS do not need to already be running.)
4. Borderlands RP Setup
      1. Run `Borderlands RP.exe` if you haven't already.
	  2. You remember that `Client ID` you should've written down (See Step #4 again) right?
	  	  1. Now is the time to paste/type the ID in.
	  	  2. If you don't know how to copy paste into consoles like `Borderlands RP.exe`
	  	     1. Right click on the top bar that that says, `Borderlands Discord Rich Presence`![Top Bar](https://puu.sh/CiEdn/5e496344c7.png)
	  	     2. Next move onto the little text thing that says, `Edit`
	  	     3. Next click the thing that says `Paste`.
	  	     4. You may or may not need to click enter for it to finish pasting
	  	  3. You did it! You copy pasted!
	  3. Discord Rich Presence will now connect to Discord.
	  4. You'll hopefully never have to enter that Client ID ever again.

	  * Now if you press `C`, you'll be able to change your Client ID for some reason.
	  * If you press `T`, You'll change the time (in seconds) that Borderlands Rich Presence will update Discord.
	  * If you press `ESC`, You'll close the program. You wouldn't want to do that right? :(