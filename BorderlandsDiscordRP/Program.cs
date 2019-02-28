using System;
using System.Timers;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using BorderlandsDiscordRP.Properties;
using DiscordRPC;
using DiscordRPC.Logging;
using static BLIO;

namespace BorderlandsDiscordRP
{
    class Program
    {

        #region Variables

        #region Discord Specific Things
        // The discord client
        public static DiscordRpcClient client;

        // The pipe discord is located on. If set to -1, the client will scan for the first available pipe.
        private static int discordPipe = -1;

        // The ID of the client using Discord RP.
        private static string clientID = Settings.Default.clientID;

        // The double (in milliseconds) for how much we update
        private static double timeToUpdate = Settings.Default.timeUpdate;

        // The level of logging to use
        private static LogLevel logLevel = LogLevel.Warning;

        // The timer we use to update our discord rpc
        private static System.Timers.Timer timer = new System.Timers.Timer(15000);

        // If we're connected to discord
        private static bool connected = false;

        // If we're quitting / have quit our program
        private static bool isRunning = true;

        #endregion

        #region Game Specific Stuff
        // The boolean of the game
        private static bool bl2 = true;
        private static bool tps = false;

        private static string lastKnownMap = "Unknown";
        private static string lastKnownMission = "Unknown";
        private static string lastKnownChar = "Unknown";
        private static int lastKnownLevel = 1;
        #endregion

        #endregion

        #region Constructors
        static void Main(string[] args)
        {
            Console.Title = "Borderlands Discord Rich Presence";
            isRunning = true;
            // Reads the args for the pipe, may or may not be an actual thing thats needed.
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-pipe")
                    discordPipe = int.Parse(args[++i]);
            }
            // If we just ran the program for the first time and our client ID is nothing
            if (Settings.Default.clientID == "" || clientID == "")
                updateClientID();

            setupClient();
            while (isRunning)
            {
                Console.WriteLine(
                    "Press C to change your client ID if need be.\n" +
                    "Press T to change how much the program will update Discord\n" +
                    "Press ESC to close the program and stop rich presence.");
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.C)
                    updateClientID();

                else if (keyInfo.Key == ConsoleKey.T)
                    updateTimer();

                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    // At the very end we need to dispose of our stuffs.
                    timer.Dispose();
                    client.Dispose();
                    isRunning = false;
                }
                Console.Clear();
            }
        }
        #endregion

        #region Updaters
        private static void updateClientID()
        {
            Console.Clear();
            Console.WriteLine("Please enter your Discord Rich Presence ID!");
            Settings.Default.clientID = Console.ReadLine();
            clientID = Settings.Default.clientID;
            Settings.Default.Save();
            setupClient();
        }

        private static void updateTimer()
        {
            bool validInput = false;
            double timeSeconds = timeToUpdate;
            while (!validInput)
            {
                Console.Clear();
                Console.WriteLine("Please enter the time (in seconds (Default: 30 seconds))  Borderlands Rich Presence will update Discord:");
                string time = Console.ReadLine();
                validInput = double.TryParse(time, out timeSeconds);
            }
            timeToUpdate = TimeSpan.FromSeconds(timeSeconds).TotalMilliseconds;
            Settings.Default.timeUpdate = timeToUpdate;
            Settings.Default.Save();
            Console.Clear();
        }
        #endregion

        #region Setup 
        private static void setupClient()
        {
            connected = false;

            // Create a new client
            client = new DiscordRpcClient(clientID);

            // Create the logger
            client.Logger = new ConsoleLogger() { Level = logLevel, Coloured = true };

            client.OnReady += (sender, msg) =>
            {
                connected = true;
                Console.Clear();
                Console.WriteLine("Connected to Discord as user {0}, using API ID of: {1}\n", msg.User.Username, clientID);
            };

            timer.Elapsed += timerHandler;
            timer.AutoReset = true;
            timer.Interval = 2500;
            timer.Start();


            // Connect to discord
            client.Initialize();

            Thread childThread = new Thread(CallToChildThread);

            childThread.Start();
            while (childThread.IsAlive)
                continue;

        }

        public static void CallToChildThread()
        {
            int animationFrame = 0;
            char[] animationFrames = new[] { '|', '/', '-', '\\' };

            Console.Write("Connecting to Discord ");
            Console.CursorVisible = false;
            while (!connected)
            {
                Thread.Sleep(100);
                // Store the current position of the cursor
                var originalX = Console.CursorLeft;
                var originalY = Console.CursorTop;

                Console.Write(animationFrames[animationFrame]);
                animationFrame++;
                if (animationFrame == animationFrames.Length)
                    animationFrame = 0;
                Console.SetCursorPosition(originalX, originalY);

            }
            Console.CursorVisible = true;
            Console.Clear();
        }
        #endregion

        #region Handlers
        static void timerHandler(object sender, ElapsedEventArgs args)
        {
            // Change our timer interval just in case.
            timer.Interval = timeToUpdate;

            Process[] bl2Array = Process.GetProcesses().Where(p => p.ProcessName.Contains("Borderlands2")).ToArray();
            Process[] tpsArray = Process.GetProcesses().Where(p => p.ProcessName.Contains("BorderlandsPreSequel")).ToArray();
            bl2 = bl2Array.Length > 0;
            tps = tpsArray.Length > 0;
            var launchDate = new DateTime();
            if (bl2)
                launchDate = bl2Array.FirstOrDefault().StartTime;
            else if (tps)
                launchDate = tpsArray.FirstOrDefault().StartTime;

            client.Invoke();


            if (!bl2 && !tps)
            {
                client.ClearPresence();
                return;
            }

            Dictionary<string, string> dict = obtainKeyBasedOnGame();
            RichPresence presence = new RichPresence()
            {
                Details = getCurrentMission(),
                State = string.Format("LVL {0} {1} - ({2} of 4)", getCurrentLevel(), getCurrentClass(), getPlayersInLobby()),
                Assets = new Assets()
                {
                    LargeImageKey = dict.Keys.ElementAtOrDefault(0),
                    LargeImageText = "Map: " + getCurrentMap()
                },
                Timestamps = new Timestamps(launchDate.ToUniversalTime())
            };

            client.SetPresence(presence);
        }
        #endregion

        #region Data Fetchers
        private static Dictionary<string, string> obtainKeyBasedOnGame()
        {
            string key = "";
            string keyTooltip = "";
            if (bl2)
            {
                key = "bl2icon";
                keyTooltip = "BL2";
            }
            else if (tps)
            {
                key = "tpsicon";
                keyTooltip = "BL: TPS";
            }
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add(key, keyTooltip);
            return dict;
        }

        private static int getPlayersInLobby()
        {
            int characters = 1;
            IReadOnlyList<BLObject> players = GetAll("WillowPlayerPawn");
            players.Distinct().Where(o => o.Name.Contains("Loader."));
            int count = players.Distinct().Where(o => o.Name.Contains("Loader.")).Count();
            characters = count != 0 ? count : 1;
            return characters;
        }

        private static string getCurrentMap()
        {
            string mapName = "Unknown";
            IReadOnlyDictionary<BLObject, object> players = GetAll("WorldInfo", "NavigationPointList");
            KeyValuePair<BLObject, object>[] dict = players.Distinct()
                .Where(p => p.Key.Name.Contains("Loader.")).ToArray();

            string pylon = "";
            foreach (KeyValuePair<BLObject, object> kvp in dict)
            {
                pylon = ((BLObject)kvp.Value)?.Name;
            }
            mapName = mapFileToActualMap(pylon);
            if (mapName.Trim() == "" || mapName.Contains("Fake"))
                mapName = lastKnownMap;
            else
                lastKnownMap = mapName;

            return mapName;
        }

        private static string getCurrentMission()
        {
            string mission = "Unknown";

            IReadOnlyDictionary<BLObject, object> dict = GetAll("HUDWidget_Missions", "CachedMissionName");
            KeyValuePair<BLObject, object>[] arr = dict.Where(m => m.Key.Name.Contains("Transient")).ToArray();
            mission = dict.FirstOrDefault().Value?.ToString();

            if (mission == null || mission.Trim() == "")
                mission = lastKnownMission;
            else
                lastKnownMission = mission;

            return mission;
        }

        private static string getCurrentClass()
        {
            IReadOnlyDictionary<BLObject, object> dict = GetAll("WillowPlayerController", "CharacterClass");
            KeyValuePair<BLObject, object>[] arr = dict.Where(m => m.Key.Name.Contains("Loader")).ToArray();
            string characterClass = ((BLObject)arr.FirstOrDefault().Value)?.Name;
            if (characterClass == null || characterClass.Trim() == "")
                characterClass = lastKnownChar;
            else
                lastKnownChar = characterClass;

            return classToCharacterName(characterClass);
        }

        private static int getCurrentLevel()
        {

            IReadOnlyDictionary<BLObject, object> dict = GetAll("WillowHUDGFxMovie", "CachedLevel");
            KeyValuePair<BLObject, object>[] arr = dict.Where(m => m.Key.Name.Contains("Transient")).ToArray();
            string lev = dict.FirstOrDefault().Value?.ToString();

            if (lev == null || lev.Trim() == "")
                lev = lastKnownMission;
            else
                lastKnownMission = lev;
            int.TryParse(lev, out int level);
            return level;

        }
        #endregion

        #region Helpers
        private static string classToCharacterName(string charClass)
        {
            if (bl2)
            {
                if (charClass.Contains("Assassin"))
                    return "Zer0";
                if (charClass.Contains("Lilac_PlayerClass"))
                    return "Krieg";
                if (charClass.Contains("Mercenary"))
                    return "Salvador";
                if (charClass.Contains("Siren"))
                    return "Maya";
                if (charClass.Contains("Soldier"))
                    return "Axton";
                if (charClass.Contains("Tulip_Mechromancer"))
                    return "Gaige";
            }

            if (tps)
            {
                if (charClass.Contains("Crocus"))
                    return "Aurelia";
                if (charClass.Contains("Enforcer"))
                    return "Wilhelm";
                if (charClass.Contains("Gladiator"))
                    return "Athena";
                if (charClass.Contains("Lawbringer"))
                    return "Nisha";
                if (charClass.Contains("Prototype"))
                    return "Claptrap";
                if (charClass.Contains("Quince_Doppel"))
                    return "Jack";
            }

            return "Unknown";
        }

        private static string mapFileToActualMap(string map)
        {
            if (map == null)
                return "Unknown";

            map = map.ToLower(CultureInfo.InvariantCulture);
            if (bl2)
            {
                if (map.Contains("menumap"))
                    return "Main Menu";
                if (map.Contains("stockade"))
                    return "Arid Nexus - Badlands";
                if (map.Contains("fyrestone"))
                    return "Arid Nexus - Boneyard";
                if (map.Contains("damtop"))
                    return "Bloodshot Ramparts";
                if (map.Contains("dam") && !map.Contains("damtop"))
                    return "Bloodshot Stronghold";
                if (map.Contains("frost"))
                    return "Three Horns Valley";
                if (map.Contains("boss_cliffs"))
                    return "Bunker";
                if (map.Contains("caverns"))
                    return "Caustic Caverns";
                if (map.Contains("vogchamber"))
                    return "Control Core Angel";
                if (map.Contains("interlude"))
                    return "The Dust";
                if (map.Contains("tundratrain"))
                    return "End of the Line";
                if (map.Contains("ash"))
                    return "Eridium Blight";
                if (map.Contains("banditslaughter"))
                    return "Fink's Slaughterhouse";
                if (map.Contains("fridge"))
                    return "The Fridge";
                if (map.Contains("hypinterlude"))
                    return "Friendship Gulag";
                if (map.Contains("icecanyon"))
                    return "Frostburn Canyon";
                if (map.Contains("finalbossascent"))
                    return @"Hero's Pass";
                if (map.Contains("outwash"))
                    return "Highlands Outwash";
                if (map.Contains("grass") && !map.Contains("lynchwood"))
                    return "Highlands";
                if (map.Contains("luckys"))
                    return "Holy Spirits";
                if (map.Contains("grass"))
                    return "Lynchwood";
                if (map.Contains("creatureslaughter"))
                    return "Natural Selection Annex";
                if (map.Contains("hyperioncity"))
                    return "Opportunity";
                if (map.Contains("robotslaughter"))
                    return "Ore Chasm";
                if (map.Contains("sanctuary") && !map.Contains("hole"))
                    return "Sanctuary";
                if (map.Contains("sanctuary_hole"))
                    return "Sanctuary Hole";
                if (map.Contains("craterlake"))
                    return "Sawtooth Cauldron";
                if (map.Contains("cove"))
                    return "Southern Shelf - Bay";
                if (map.Contains("southernshelf"))
                    return "Southern Shelf";
                if (map.Contains("Southpaw Factory"))
                    return "Southpaw Steam + Power";
                if (map.Contains("thresherraid"))
                    return "Terramorphous Peak";
                if (map.Contains("ice"))
                    return "Three Horns Divide";
                if (map.Contains("tundraexpress"))
                    return "Tundra Express";
                if (map.Contains("boss_volcano"))
                    return "Vault of the Warrior";
                if (map.Contains("pandorapark"))
                    return "Wildlife Exploitation Preserve";
                if (map.Contains("glacial"))
                    return "Windshear Waste";
                #region DLC

                if (map.Contains("orchid"))
                {
                    if (map.Contains("caves"))
                        return "Hayter's Folly";
                    if (map.Contains("wormbelly"))
                        return "Leviathan's Lair";
                    if (map.Contains("spire"))
                        return "Magnys Lighthouse";
                    if (map.Contains("oasistown"))
                        return "Oasis";
                    if (map.Contains("shipgraveyard"))
                        return "The Rustyards";
                    if (map.Contains("refinery"))
                        return "Washburne Refinery";
                    if (map.Contains("saltflats"))
                        return "Wurmwater";
                }
                else if (map.Contains("iris"))
                {
                    if (map.Contains("dl1"))
                        return "Torgue Arena";
                    if (map.Contains("moxxi"))
                        return "Badass Crater Bar";
                    if (map.Contains("hub") && !map.Contains("hub2"))
                        return "Badass Crater of Badassitude";
                    if (map.Contains("dl2") && !map.Contains("interior"))
                        return "The Beatdown";
                    if (map.Contains("dl3"))
                        return "The Forge";
                    if (map.Contains("interior"))
                        return @"Pyro Pete's Bar";
                    if (map.Contains("hub2"))
                        return "Southern Raceway";
                }
                else if (map.Contains("sage"))
                {
                    if (map.Contains("powerstation"))
                        return "Ardorton Station";
                    if (map.Contains("cliffs"))
                        return "Candlerakk's Crag";
                    if (map.Contains("hyperionship"))
                        return "H.S.S Terminus";
                    if (map.Contains("underground"))
                        return "Hunter's Grotto";
                    if (map.Contains("rockforest"))
                        return @"Scylla's Grove";
                }

                if (map.Contains("dark_forest"))
                    return "Dark Forest";
                if (map.Contains("castlekeep"))
                    return "Dragon Keep";
                if (map.Contains("village"))
                    return "Flamerock Refuge";
                if (map.Contains("castleexterior"))
                    return @"Hatred's Shadow";
                if (map.Contains("dead_forest"))
                    return "Immortal Woods";
                if (map.Contains("dungeon") && !map.Contains("raid"))
                    return "Lair of Infinite Agony";
                if (map.Contains("raid"))
                    return "The Winged Storm";
                if (map.Contains("mines"))
                    return "Mines of Avarice";
                if (map.Contains("templeslaughter"))
                    return @"Murderlin's Temple";
                if (map.Contains("docks"))
                    return "Unassuming Docks";

                if (map.Contains("hunger"))
                    return "Gluttony Gulch";
                if (map.Contains("pumpkin"))
                    return "Hallowed Hallow";
                if (map.Contains("xmas"))
                    return @"Marcus's Mercenary Shop";
                if (map.Contains("testingzone"))
                    return "Raid on Digistruct Peak";
                if (map.Contains("distillery"))
                    return "Rotgut Distillery";
                if (map.Contains("easter"))
                    return "Wam Bam Island";


                #endregion
            }
            else if (tps)
            {
                if (map.Contains("moonslaughter"))
                    return "Abandoned Training Facility";
                if (map.Contains("spaceport"))
                    return "Concorida";
                if (map.Contains("comfacility"))
                    return "Crisis Scar";
                if (map.Contains("innercore"))
                    return "Eleseer";
                if (map.Contains("laserboss"))
                    return "Eye of Helios";
                if (map.Contains("moonshotintro"))
                    return "Helios Station";
                if (map.Contains("jacksoffice"))
                    return @"Jack's Office";
                if (map.Contains("laser"))
                    return "Lunar Launching Station";
                if (map.Contains("meriff"))
                    return @"Meriff's Office";
                if (map.Contains("digsite_rk5"))
                    return "Outfall Pumping Station";
                if (map.Contains("outlands_p2"))
                    return "Outlands Canyon";
                if (map.Contains("outlands"))
                    return "Outlands Spur";
                if (map.Contains("wreck"))
                    return @"Pity's Fall";
                if (map.Contains("deadsurface"))
                    return "Regolith Range";
                if (map.Contains("randdfacility"))
                    return "Research and Development";
                if (map.Contains("moonsurface"))
                    return @"Serenity's Waste";
                if (map.Contains("stantonsliver"))
                    return @"Stanton's Liver";
                if (map.Contains("sublevel13"))
                    return "Sub-Level 13";
                if (map.Contains("dahlfactory") && !map.Contains("boss"))
                    return "Titan Industrial Facility";
                if (map.Contains("dahlfactory_boss"))
                    return "Titan Robot Production Plant";
                if (map.Contains("moon"))
                    return "Triton Flats";
                if (map.Contains("access"))
                    return @"Tycho's Ribs";
                if (map.Contains("innerhull"))
                    return "Veins of Helios";
                if (map.Contains("digsite"))
                    return "Vorago Solitude";

                if (map.Contains("ma_"))
                {
                    if (map.Contains("leftcluster"))
                        return "Cluster 007733 P4ND0R4";
                    if (map.Contains("rightcluster"))
                        return "Cluster 99002 0V3RL00K";
                    if (map.Contains("subboss"))
                        return "Cortex";
                    if (map.Contains("deck13"))
                        return "Deck 13 1/2";
                    if (map.Contains("finalboss"))
                        return "Deck 13.5";
                    if (map.Contains("motherboard"))
                        return "Motherless Board";
                    if (map.Contains("subconscious"))
                        return "Subconscious";
                }

                if (map.Contains("eridian_slaughter"))
                    return "The Holodome";
            }


            return "Unknown";
        }
        #endregion
    }
}
