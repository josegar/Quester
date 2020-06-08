using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using wManager.Events;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Windows.Forms;
using robotManager.Products;
using System.Threading.Tasks;



//FlightMaster
public class FlightMaster
{

    private int priority;

    public FlightMaster(int priority)
    {
        this.priority = priority;
    }

    public static void main(String[] args)
    {
        FlightMaster temp = new FlightMaster(1);
        Logging.Write("Stuff");
    }


    public static int timer = 0;
    public static Vector3 myPositionAtStart = new Vector3(0, 0, 0);
    public static bool _deleteOldPath = false;
    public static bool _isLaunched;
    static bool longMoveState = false;
    static Vector3 checkDistanceToDestination;
    private static int travelDistance = 1250;
    private static float saveDistance;
    public static Vector3 destinationVector = new Vector3(0, 0, 0);

    //public static MovementEvents.MovementCancelableHandler MovementEventsOnOnMovementPulse { get; private set; }

    // public static List&lt;FlightMasterDB&gt; FML = fillDB();

    public void startFlightMaster()
    {
        Logging.Write("[FNV_Quester]: Flight Master initialized");
        _isLaunched = true;
        FNVQuesterFlightMaster.Load();
        //flightMasterLoop();
    }

    public void start()
    {
        //Logging.Write("Subscribing to event");
        // SubscribeToEvent();
        // watchForEvents();
    }

    public void stop()
    {
        //Logging.Write("Unsubscribing to event");
        //UnSubscribeEvents();
    }

    public static void disposeFlightMaster()
    {
        _isLaunched = false;
        FNVQuesterFlightMaster.CurrentSettings.Save();
        //clearOldPath();
        //_deleteOldPath = true;
        //UnSubscribeEvents();
        //MovementEvents.OnMovementPulse -= new MovementEvents.MovementCancelableHandler(FlightMaster.MovementEventsOnOnMovementPulse);
        //Logging.Write("[FNV_Quester]: Flight Master disposed");
    }

    public static void disposeFlightMaster(int wert)
    {
        _isLaunched = false;
        if (wert == 1)
        {
            Logging.Write("Flight Master stopped, dead");
        }
        else
        {
            Logging.Write("Flight Master stopped, combat");
        }
    }


    private void SubscribeToEvent()
    {
        MovementEvents.OnMovementPulse += MovementEventsOnOnMovementPulse;
    }

    public void UnSubscribeEvents()
    {
        MovementEvents.OnMovementPulse -= MovementEventsOnOnMovementPulse;
    }

    private void watchForEvents()
    {

        EventsLuaWithArgs.OnEventsLuaWithArgs += (LuaEventsId id, List&lt;string&gt; args) =&gt;
        {


            if (id == wManager.Wow.Enums.LuaEventsId.PLAYER_DEAD)
            {
                Logging.Write("[FNV_Quester]: Player died, stop Flight Master");
                disposeFlightMaster(1);
            }
            if (id == wManager.Wow.Enums.LuaEventsId.PLAYER_ENTER_COMBAT)
            {
                disposeFlightMaster(2);
                Logging.Write("[FNV_Quester]: Player entered combat, stop Flight Master");
            }

        };

    }


    private void MovementEventsOnOnMovementPulse(List&lt;Vector3&gt; path, CancelEventArgs cancelEventArgs)
    {
        if (timer &lt; 1)
        {
            checkDistanceToDestination = path.Last&lt;Vector3&gt;();
            longMove(path.Last&lt;Vector3&gt;());
            destinationVector = path.Last&lt;Vector3&gt;();
            if (ObjectManager.Me.Position.DistanceTo(destinationVector) &gt; 1000 &amp;&amp; !_isLaunched)
                startFlightMaster();
        }
        else
        {
            Logging.Write("[FNV_Quester]: Taxi in Pause");
        }
    }

    public bool longMove(Vector3 destination)
    {
        saveDistance = new Vector3(destination).DistanceTo(ObjectManager.Me.Position);
        if (new Vector3(destination).DistanceTo(ObjectManager.Me.Position) &lt; travelDistance)
        {
            longMoveState = false;
            return false;
        }
        else
        {
            longMoveState = true;
            return true;
        }
    }


    public static FlightMasterDB getClosestFlightMasterFrom()
    {
        List&lt;FlightMasterDB&gt; FMLnfmd = fillDB();
        float tempDistance = 99999;
        FlightMasterDB returnObject = new FlightMasterDB("null", 0, new Vector3(0, 0, 0), false);

        foreach (var a in FMLnfmd)
        {
            if (a.alreadyDiscovered &amp;&amp; a.position.DistanceTo(ObjectManager.Me.Position) &lt; tempDistance &amp;&amp; (a.continent == checkContinent()))
            {
                tempDistance = a.position.DistanceTo(ObjectManager.Me.Position);
                returnObject = a;
            }
        }
        return returnObject;
    }

    public static FlightMasterDB getClosestFlightMasterTo()
    {
        List&lt;FlightMasterDB&gt; FMLgcfmt = fillDB();
        float tempDistance = 99999;
        FlightMasterDB returnObject = new FlightMasterDB("null", 0, new Vector3(0, 0, 0), false);

        foreach (var a in FMLgcfmt)
        {
            if (a.alreadyDiscovered &amp;&amp; a.position.DistanceTo(destinationVector) &lt; tempDistance &amp;&amp; (a.continent == checkContinent()))
            {
                tempDistance = a.position.DistanceTo(destinationVector);
                returnObject = a;
            }
        }
        return returnObject;
    }

    public static bool checkContinent()
    {
        if (Usefuls.ContinentId == (int)ContinentId.Kalimdor)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public static void waitFlying()
    {

        while (ObjectManager.Me.IsOnTaxi)
        {
            Logging.Write("[FNV_Quester]: On taxi, waiting");
            Thread.Sleep(30000);
        }

        Logging.Write("[FNV_Quester]: Arrived at destination Flight Master, finished waiting");
    }

    public static List&lt;FlightMasterDB&gt; fillDB()
    {
        //True = Kalimdor ; False = Eastern Kingdoms
        List&lt;FlightMasterDB&gt; FMListe = new List&lt;FlightMasterDB&gt;();
        FlightMasterDB Stormwind = new FlightMasterDB("Stormwind", 352, new Vector3(-8835.76f, 490.084f, 109.6157f), false);
        FMListe.Add(Stormwind);
        FlightMasterDB ArathiHighlands = new FlightMasterDB("Arathi", 2835, new Vector3(-1240.03f, -2513.96f, 21.92969f), false);
        FMListe.Add(ArathiHighlands);
        FlightMasterDB Ashenvale = new FlightMasterDB("Ashenvale", 4267, new Vector3(2828.4f, -284.3f, 106.7f), true);
        FMListe.Add(Ashenvale);
        FlightMasterDB Darkshore = new FlightMasterDB("Darkshore", 3841, new Vector3(6343.2f, 561.651f, 15.79876f), true);
        FMListe.Add(Darkshore);
        FlightMasterDB Stranglethorn = new FlightMasterDB("Stranglethorn", 2859, new Vector3(-14477.9f, 464.101f, 36.38163f), false);
        FMListe.Add(Stranglethorn);
        FlightMasterDB Duskwood = new FlightMasterDB("Duskwood", 2409, new Vector3(-10513.8f, -1258.79f, 41.43174f), false);
        FMListe.Add(Duskwood);
        FlightMasterDB FeralasFeathermoon = new FlightMasterDB("Feralas, Feathermoon", 8019, new Vector3(-4370.5f, 3340f, 12f), true);
        FMListe.Add(FeralasFeathermoon);
        FlightMasterDB FeralasThalanaar = new FlightMasterDB("Feralas, Thalanaar", 4319, new Vector3(-4491f, -781f, -40f), true);
        FMListe.Add(FeralasThalanaar);
        FlightMasterDB Tanaris = new FlightMasterDB("Tanaris", 7823, new Vector3(-7224.9f, -3738.2f, 8.4f), true);
        FMListe.Add(Tanaris);
        FlightMasterDB Hinterlands = new FlightMasterDB("The Hinterlands", 8018, new Vector3(282.1f, -2001.3f, 194.1f), false);
        FMListe.Add(Hinterlands);
        FlightMasterDB Ironforge = new FlightMasterDB("Ironforge", 1573, new Vector3(-4821.13f, -1152.4f, 502.2116f), false);
        FMListe.Add(Ironforge);
        FlightMasterDB Menethil = new FlightMasterDB("Wetlands", 1571, new Vector3(-3793.2f, -782.052f, 9.014864f), false);
        FMListe.Add(Menethil);
        FlightMasterDB TheBarrens = new FlightMasterDB("The Barrens", 16227, new Vector3(-898.246f, -3769.65f, 11.71021f), true);
        FMListe.Add(TheBarrens);
        FlightMasterDB Redridge = new FlightMasterDB("Redridge Mountains", 931, new Vector3(-9435.8f, -2234.79f, 69.43174f), false);
        FMListe.Add(Redridge);
        FlightMasterDB Teldrassil = new FlightMasterDB("Teldrassil", 3838, new Vector3(8640.58f, 841.118f, 23.26363f), true);
        FMListe.Add(Teldrassil);
        FlightMasterDB Southshore = new FlightMasterDB("Hillsbrad Foothiils", 2432, new Vector3(-715.146f, -512.134f, 26.54455f), false);
        FMListe.Add(Southshore);
        FlightMasterDB Stonetalon = new FlightMasterDB("Stonetalon Mountains", 4407, new Vector3(2682.83f, 1466.45f, 233.6483f), true);
        FMListe.Add(Stonetalon);
        FlightMasterDB Thelsamar = new FlightMasterDB("Loch Modan", 1572, new Vector3(-5424.85f, -2929.87f, 347.5623f), false);
        FMListe.Add(Thelsamar);
        FlightMasterDB Theramore = new FlightMasterDB("Dustwallow Marsh", 4321, new Vector3(-3828.88f, -4517.51f, 10.66067f), true);
        FMListe.Add(Theramore);
        FlightMasterDB WesternP = new FlightMasterDB("Western Pleaguelands", 12596, new Vector3(928.3f, -1429.1f, 64.8f), false);
        FMListe.Add(WesternP);
        FlightMasterDB Westfall = new FlightMasterDB("Westfall", 523, new Vector3(-10628.8f, 1037.79f, 34.43174f), false);
        FMListe.Add(Westfall);
        FlightMasterDB EasternP = new FlightMasterDB("Eastern Pleaguelands", 12617, new Vector3(2269.9f, -5345.4f, 86.9f), false);
        FMListe.Add(EasternP);
        FlightMasterDB SearingGorge = new FlightMasterDB("Searing Gorge", 2941, new Vector3(-6559.1f, -1169.4f, 309.8f), false);
        FMListe.Add(SearingGorge);
        FlightMasterDB BurningSteppes = new FlightMasterDB("Burning Steppes", 2299, new Vector3(-8365.1f, -2758.5f, 185.6f), false);
        FMListe.Add(BurningSteppes);
        FlightMasterDB Azshara = new FlightMasterDB("Azshara", 12577, new Vector3(2718.2f, -3880.8f, 101.4f), true);
        FMListe.Add(Azshara);
        FlightMasterDB Felwood = new FlightMasterDB("Felwood", 12578, new Vector3(6204.2f, -1951.4f, 571.3f), true);
        FMListe.Add(Felwood);
        FlightMasterDB Winterspring = new FlightMasterDB("Winterspring", 11138, new Vector3(6800.5f, -4742.4f, 701.5f), true);
        FMListe.Add(Winterspring);
        FlightMasterDB UngoroCreater = new FlightMasterDB("Ungoro Crater", 10583, new Vector3(-6110.5f, -1140.4f, -186.9f), true);
        FMListe.Add(UngoroCreater);
        FlightMasterDB Silithus = new FlightMasterDB("Silithus", 15177, new Vector3(-6758.6f, 775.6f, 89f), true);
        FMListe.Add(Silithus);
        FlightMasterDB Desolace = new FlightMasterDB("Desolace", 6706, new Vector3(136f, 1326f, 193f), true);
        FMListe.Add(Desolace);
        return FMListe;
    }

    public static bool validFlight(String from, String to)
    {
        bool von = false;
        bool zu = false;
        List&lt;FlightMasterDB&gt; FMLvf = fillDB();

        for (int i = 0; i &lt; FMLvf.Count; i++)
        {
            if (FMLvf[i].name.Contains(from))
            {
                von = FMLvf[i].continent;
            }
            if (FMLvf[i].name.Contains(to))
            {
                zu = FMLvf[i].continent;
            }
        }
        return von != zu;
    }

    public static bool discoveredTaxiNodes(String from, String to)
    {

        List&lt;FlightMasterDB&gt; FMLDTN = fillDB();

        foreach (var ele in FMLDTN)
        {
            if (ele.name.Contains(from))
            {
                if (!ele.alreadyDiscovered)
                {
                    Logging.Write("[FNV_Quester]: Taxi node from " + ele.name + " has not been discovered so far. Abort taking taxi.");
                    return false;
                }
            }
        }

        foreach (var ele in FMLDTN)
        {
            if (ele.name.Contains(to))
            {
                if (!ele.alreadyDiscovered)
                {
                    Logging.Write("[FNV_Quester]: Taxi node to " + ele.name + " has not been discovered so far. Abort taking taxi.");
                    return false;
                }
            }
        }
        return true;
    }

    public static void takeTaxi(String from, String to)
    {

        List&lt;FlightMasterDB&gt; FMLtt = fillDB();
        bool canProceed = true;
        try
        {
            FMLtt = fillDB();
        }
        catch (Exception e)
        {
            Logging.Write("Error: " + e);
        }


        Vector3 myPos = ObjectManager.Me.Position;

        if (Usefuls.MapZoneName.Contains(to))
        {
            canProceed = false;
            Logging.Write("[FNV_Quester]: Already in the zone we are going to travel to. Skip flying");
        }

        if (canProceed)
        {

            if (validFlight(from, to))
            {
                Logging.Write("[FNV_Quester]: Unable to fly between Kalimdor and Eastern Kingdoms");
            }

            var position = new Vector3();
            int npcEntryId = 0;

            for (int i = 0; i &lt; FMLtt.Count; i++)
            {
                if (FMLtt[i].name.Contains(from))
                {
                    position = FMLtt[i].position;
                    npcEntryId = FMLtt[i].NPCId;
                    FMLtt[i].name.Contains(from);
                }
            }


            if (!ObjectManager.Me.IsOnTaxi)
            {
                wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();

                while (!wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(position, npcEntryId, -1, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore) &amp;&amp; !Fight.InFight)
                {
                    wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();
                    wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(position, npcEntryId, -1, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore);
                }
                if (wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(position, npcEntryId, -1, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore))
                {
                    while (!ObjectManager.Me.IsOnTaxi)
                    {
                        Usefuls.SelectGossipOption(GossipOptionsType.taxi);

                        int node = Lua.LuaDoString&lt;int&gt;("for i=0,30 do if string.find(TaxiNodeName(i),'" + to + "') then return i end end");
                        Lua.LuaDoString("TakeTaxiNode(" + node + ")");
                        Logging.Write("[FNV_Quester]: Taking Taxi from " + from + " to " + to);
                        Thread.Sleep(Usefuls.Latency + 2500);
                    }
                }
            }
            waitFlying();
            return;
        }
    }

    public static void discoverTaxi(String discoverName)
    {
        FNVQuesterFlightMaster.Load();
        List&lt;FlightMasterDB&gt; FMLdt = fillDB();

        var position = new Vector3();
        int npcEntryId = 0;
        int j = 0;
        bool alreadyDiscoveredFlightMaster = false;

        for (int i = 0; i &lt; FMLdt.Count; i++)
        {
            if (FMLdt[i].name.Contains(discoverName))
            {
                if (FMLdt[i].alreadyDiscovered)
                {
                    Logging.Write("[FNV_Quester]: Flight Master of " + FMLdt[i].name + " already discovered. Skip it.");
                    alreadyDiscoveredFlightMaster = true;
                    break;
                }

                position = FMLdt[i].position;
                npcEntryId = FMLdt[i].NPCId;
                j = i;
            }
        }

        if (!alreadyDiscoveredFlightMaster)
        {

            if (!ObjectManager.Me.IsOnTaxi)
            {
                wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();

                while (!wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(position, npcEntryId, -1, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore))
                {
                    // wManager.Wow.Helpers.Conditions.ForceIgnoreIsAttacked = true;
                    wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();
                    wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(position, npcEntryId, -1, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore);
                }
                if (wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(position, npcEntryId, -1, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore))
                {
                    Usefuls.SelectGossipOption(GossipOptionsType.taxi);
                    Thread.Sleep(Usefuls.Latency + 250);
                    //wManager.Wow.Helpers.Conditions.ForceIgnoreIsAttacked = true;
                }
            }
            Logging.Write("[FNV_Quester]: Flight Master " + FMLdt[j].name + " discovered");
            FMLdt[j].alreadyDiscovered = true;
            FNVQuesterFlightMaster.flightMasterSaveChanges(FMLdt[j]);
            //wManager.Wow.Helpers.Conditions.ForceIgnoreIsAttacked = false;

        }
        timer = 0;
        return;
    }

}

public class FlightMasterDB
{
    public FlightMasterDB(String name, int NPCId, Vector3 position, bool continent, bool alreadyDiscovered)
    {
        this.name = name;
        this.NPCId = NPCId;
        this.position = position;
        this.continent = continent;
        this.alreadyDiscovered = alreadyDiscovered;
    }

    public FlightMasterDB(String name, int NPCId, Vector3 position, bool continent)
    {
        this.name = name;
        this.NPCId = NPCId;
        this.position = position;
        this.continent = continent;

    }

    public int NPCId { get; set; }
    public Vector3 position { get; set; }
    public String name { get; set; }
    public bool continent { get; set; }
    public bool alreadyDiscovered { get; set; }

}

//Tram
public class Tram
{

    //Stormwind to Ironforge
    static Vector3 positionTramInIronforge = new Vector3(4.58065, 28.2097, 6.90526);
    static Vector3 positionTramInStormwind = new Vector3(4.581913, 2511.531, 7.091796);

    static Vector3 positionWaitTramIronforge = new Vector3(19.1, 28, -4.3);
    static Vector3 positionOnTramIronforge = new Vector3(4.8, 28.1, -4.3);

    static Vector3 positionWaitTramStormwind = new Vector3(15.3, 2510.4, -4.3);
    static Vector3 positionOnTramStormwind = new Vector3(4.3, 2510.6, -4.3);

    static Vector3 enterStormwindInside = new Vector3(67, 2490.7, -4.3);
    static Vector3 enterIronforgeInside = new Vector3(64.5, 10.2, -4.3);

    static Vector3 enterStormwindOutside = new Vector3(-8365, 536.9, 91.8);
    static Vector3 enterIronforgeOutside = new Vector3(-4836.7, -1304.6, 501.9);

    static Vector3 leavePosIronforge = new Vector3(-10.6, 30.2, -4.3);
    static Vector3 leavePosStormwind = new Vector3(-10.6, 2510.9, -4.3);

    static Vector3 playerFellDownIronforge = new Vector3(4.5, 28, -13.9);

    static int[] tramEntryArray = new int[6] { 176080, 176082, 176083, 176084, 176085, 176081 };

    //Ironforge to Stormwind
    static Vector3 waitPosPlayerInStormwindToIronforge = new Vector3(-32.8, 2512.1, -4.3);

    //Pos where upper Tram arrives and waits in Stormwind
    static Vector3 waitPosTramSwTramStormwind = new Vector3(-45.4007, 2512.15, 6.90526);
    //Pos where player waits on upper tram in Stormwind
    static Vector3 waitPosPlayerSwTramStormwind = new Vector3(-45.20206, 2512.266, -3.562748);

    static Vector3 waitPosSwTramIronforge = new Vector3(-45.399, 30.38013, 5.877773);

    static Vector3 leavePosSwTramIronforge = new Vector3(-61.8, 30.3, -4.3);
    static Vector3 waitPlayerOnTramPosSwTramIronforge = new Vector3(-34.49804, 2511.917, -4.3);
    static Vector3 posPlayerOnTramSwToIfInIronforge = new Vector3(-20.9837, 2459.93, -4.297);

    static Vector3 playerFellDownStormwind = new Vector3(-45, 2512.6, -13.9);

    static bool isRestarted = false;
    static Process[] pname = Process.GetProcessesByName(AppDomain.CurrentDomain.FriendlyName.Remove(AppDomain.CurrentDomain.FriendlyName.Length - 4));
    static Vector3 nullVector = new Vector3(0, 0, 0);


    public static void restartTram(String from)
    {
        switch (from)
        {
            case ("Stormwind"):
                takeTramStormwind();
                break;
            case ("Ironforge"):
                takeTramIronforge();
                break;
            default:
                Logging.Write("Something failed");
                break;
        }
    }

    public static void end()
    {
        Logging.Write("[FNV_Quester]: Ending Tream");
    }



    public static void takeTramStormwind()
    {
        Logging.Write("[FNV_Quester]: Taking Tram from Stormwind to Ironforge");

        if (!Usefuls.MapZoneName.Contains("Deep"))
        {
            while (!GoToTask.ToPosition(enterStormwindOutside))
            {
                GoToTask.ToPosition(enterStormwindOutside);
                Thread.Sleep(250);
            }

            wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
            Logging.Write("[FNV_Quester]: Disable teleport during Tram");
            wManager.Wow.Helpers.MovementManager.Face(new Vector3(-8353.4, 521.4, 91.8));

            while (!Usefuls.MapZoneName.Contains("Deep"))
            {
                wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 250);
                Thread.Sleep(robotManager.Helpful.Others.Random(25, 50));
            }

        }

        if (Usefuls.MapZoneName.Contains("Deep"))
        {
            if (!GoToTask.ToPosition(enterStormwindInside))
            {
                GoToTask.ToPosition(enterStormwindInside);
                Thread.Sleep(250);
            }
            while (!GoToTask.ToPosition(waitPosPlayerInStormwindToIronforge))
            {
                GoToTask.ToPosition(waitPosPlayerInStormwindToIronforge);
                Thread.Sleep(250);
            }

            wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
            var tram = (WoWGameObject)null;

            try
            {
                tram = chooseTramStormwind();
            }
            catch
            {

            }


            if (tram != null &amp;&amp; ObjectManager.Me.Position.DistanceTo(tram.Position) &lt; 200 &amp;&amp; !tram.Position.Equals(nullVector))
            {
                Logging.Write("[FNV_Quester]: Bad circle, restart... .");
                isRestarted = true;
                takeTramStormwind();
                end();
            }

            if (!isRestarted)
            {

                if (tram != null &amp;&amp; tram.Position.DistanceTo(nullVector) &gt; 1)
                {
                    while (tram.Position.DistanceTo(waitPosTramSwTramStormwind) &gt;= 5 || ObjectManager.Me.Position.DistanceTo(waitPosPlayerInStormwindToIronforge) &gt;= 5)
                    {
                        Thread.Sleep(500);
                    }

                    if (tram.Position.DistanceTo(waitPosTramSwTramStormwind) &lt; 5)
                    {
                        Lua.LuaDoString("ClearTarget()");
                        wManager.Wow.Helpers.MovementManager.Face(waitPosPlayerSwTramStormwind);

                        while (ObjectManager.Me.Position.DistanceTo(waitPosPlayerSwTramStormwind) &gt; 1)
                        {
                            MovementManager.MoveTo(waitPosPlayerSwTramStormwind);
                            Thread.Sleep(1000);

                            if (ObjectManager.Me.Position.DistanceTo(playerFellDownStormwind) &lt; 4)
                            {
                                Logging.Write("[FNV_Quester]: Fell down, while trying to take tram. Restart...");
                                restartTram("Stormwind");
                            }
                        }

                        GoToTask.ToPosition(waitPosPlayerSwTramStormwind);
                    }

                    while (tram.Position.DistanceTo(waitPosSwTramIronforge) &gt; 5)
                    {
                        Thread.Sleep(3000);
                    }

                    if (ObjectManager.Me.HaveBuff("Stealth"))
                    {
                        Lua.LuaDoString("CastSpellByName('Stealth')");
                    }

                    wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 1500);

                    if (ObjectManager.Me.Position.DistanceTo(posPlayerOnTramSwToIfInIronforge) &lt; 5)
                    {
                        MovementManager.MoveTo(leavePosSwTramIronforge);
                        Thread.Sleep(1000);

                        while (ObjectManager.Me.Position.DistanceTo(leavePosSwTramIronforge) &gt; 4)
                        {
                            MovementManager.MoveTo(leavePosSwTramIronforge);
                            Thread.Sleep(1000);
                        }
                    }

                    if (Usefuls.MapZoneName.Contains("Deep"))
                    {
                        if (!GoToTask.ToPosition(enterIronforgeInside))
                        {
                            GoToTask.ToPosition(enterIronforgeInside);
                            Thread.Sleep(250);
                        }
                    }

                    while (Usefuls.MapZoneName.Contains("Deep"))
                    {
                        wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 250);
                        Thread.Sleep(robotManager.Helpful.Others.Random(25, 50));
                    }

                    Logging.Write("[FNV_Quester]: Re enable teleport ");
                    wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;


                    while (!GoToTask.ToPosition(enterIronforgeOutside))
                    {
                        wManager.Wow.Helpers.MovementManager.Face(enterIronforgeOutside);
                        GoToTask.ToPosition(enterIronforgeOutside);
                        Thread.Sleep(250);
                    }

                }
                else if (Usefuls.MapZoneName.Contains("Tram") || Usefuls.MapZoneName.Contains("Stormwind"))
                {
                    Logging.Write("[FNV_Quester]: Unable to find tram, restart... .");
                    isRestarted = true;
                    restartTram("Stormwind");
                }

            }
        }

        isRestarted = false;
    }

    public static WoWGameObject chooseTramIronforge()
    {
        List&lt;WoWGameObject&gt; tramList = new List&lt;WoWGameObject&gt;();
        var tramTemp = (WoWGameObject)null;

        foreach (int ele in tramEntryArray)
        {
            tramTemp = null;
            try
            {
                tramTemp = ObjectManager.GetWoWGameObjectByEntry(ele).OrderBy(o =&gt; o.GetDistance).FirstOrDefault();
            }
            catch
            {
            }

            if (tramTemp != null &amp;&amp; tramTemp.IsValid &amp;&amp; !tramTemp.Equals((WoWGameObject)null) &amp;&amp; tramTemp.ToString().Contains("Subway"))
            {
                tramList.Add(tramTemp);
            }
            else
            {

            }
        }

        float distance = 0;
        float entryDistance = 0;
        foreach (var ele in tramList)
        {
            if (ObjectManager.Me.Position.DistanceTo(ele.Position) &gt;= distance)
            {
                distance = ObjectManager.Me.Position.DistanceTo(ele.Position);
                entryDistance = ele.Entry;
            }
        }

        foreach (var ele in tramList)
        {
            if (ele.Entry == entryDistance &amp;&amp; ele != null &amp;&amp; !ele.Position.Equals(nullVector))
            {
                if (ObjectManager.Me.Position.DistanceTo(ele.Position) &lt; 200)
                {
                    Logging.Write("[FNV_Quester]: Bad circle, restart... .");
                    restartTram("Ironforge");
                }

                Logging.Write("[FNV_Quester]: Choosing tram id: " + ele.Entry + " with a current position of " + ele.Position);
                return ele;
            }
        }
        return null;
    }

    public static WoWGameObject chooseTramStormwind()
    {
        List&lt;WoWGameObject&gt; tramList = new List&lt;WoWGameObject&gt;();
        var tramTemp = (WoWGameObject)null;

        foreach (int ele in tramEntryArray)
        {
            tramTemp = null;
            try
            {
                tramTemp = ObjectManager.GetWoWGameObjectByEntry(ele).OrderBy(o =&gt; o.GetDistance).FirstOrDefault();
            }
            catch
            {
            }

            if (tramTemp != null &amp;&amp; tramTemp.IsValid &amp;&amp; !tramTemp.Equals((WoWGameObject)null) &amp;&amp; tramTemp.ToString().Contains("Subway"))
            {
                tramList.Add(tramTemp);
            }
            else
            {

            }
        }

        float distance = 99999;
        float entryDistance = 0;
        foreach (var ele in tramList)
        {
            if (ObjectManager.Me.Position.DistanceTo(ele.Position) &lt;= distance)
            {
                distance = ObjectManager.Me.Position.DistanceTo(ele.Position);
                entryDistance = ele.Entry;
            }
        }

        foreach (var ele in tramList)
        {
            if (ele.Entry == entryDistance &amp;&amp; ele != null &amp;&amp; !ele.Position.Equals(nullVector))
            {
                if (ObjectManager.Me.Position.DistanceTo(ele.Position) &lt; 200)
                {
                    Logging.Write("[FNV_Quester]: Bad circle, restart... .");
                    restartTram("Stormwind");
                }

                Logging.Write("[FNV_Quester]: Choosing tram id: " + ele.Entry + " with a current position of " + ele.Position);
                return ele;
            }
        }

        return null;
    }

    public static void takeTramIronforge()
    {
        Logging.Write("[FNV_Quester]: Taking Tram from Ironforge to Stormwind");

        if (!Usefuls.MapZoneName.Contains("Deep"))
        {
            while (!GoToTask.ToPosition(enterIronforgeOutside))
            {
                GoToTask.ToPosition(enterIronforgeOutside);
                Thread.Sleep(250);
            }

            wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
            Logging.Write("[FNV_Quester]: Disable teleport during Tram");
            wManager.Wow.Helpers.MovementManager.Face(new Vector3(-4839.4, -1320.9, 501.9));

            while (!Usefuls.MapZoneName.Contains("Deep"))
            {
                wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 250);
                Thread.Sleep(robotManager.Helpful.Others.Random(25, 50));
            }
        }

        if (Usefuls.MapZoneName.Contains("Deep"))
        {
            if (!GoToTask.ToPosition(enterIronforgeInside))
            {
                GoToTask.ToPosition(enterIronforgeInside);
                Thread.Sleep(250);
            }

            var pathEins = new List&lt;Vector3&gt;() {
new Vector3(42.56478f, 10.32987f, -4.29664f, "None"),
new Vector3(36.11083f, 10.30502f, -4.29664f, "None"),
new Vector3(29.11088f, 10.27806f, -4.29664f, "None"),
new Vector3(25.49891f, 10.26415f, -4.29664f, "None"),
new Vector3(20.57794f, 10.2452f, -4.29664f, "None"),
new Vector3(16.61328f, 11.97739f, -4.29664f, "None"),
new Vector3(17.03823f, 18.79025f, -4.29664f, "None"),
new Vector3(19.41723f, 25.35655f, -4.29664f, "None"),
};


            while (ObjectManager.Me.Position.DistanceTo2D(pathEins.Last&lt;Vector3&gt;()) &gt; 5)
            {
                MovementManager.Go(pathEins); // or MovementManager.GoLoop(path);
            }

            while (!GoToTask.ToPosition(positionWaitTramIronforge))
            {
                GoToTask.ToPosition(positionWaitTramIronforge);
                Thread.Sleep(250);
            }

            wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;

            var tram = (WoWGameObject)null;

            try
            {
                tram = chooseTramIronforge();
            }
            catch
            {

            }

            if (ObjectManager.Me.Position.DistanceTo(tram.Position) &lt; 200 &amp;&amp; tram != null &amp;&amp; !tram.Position.Equals(nullVector))
            {
                Logging.Write("[FNV_Quester]: Bad circle, restart... .");
                isRestarted = true;
                restartTram("Ironforge");
                end();
            }

            if (!isRestarted)
            {

                if (tram != null &amp;&amp; tram.Position.DistanceTo(nullVector) &gt; 1)
                {


                    while (tram.Position.DistanceTo(positionTramInIronforge) &gt;= 5 &amp;&amp; Usefuls.MapZoneName.Contains("Deep") &amp;&amp; !tram.Position.Equals(nullVector))
                    {
                        Thread.Sleep(1000);
                    }


                    if (tram.Position.DistanceTo(positionTramInIronforge) &lt; 5 &amp;&amp; Usefuls.MapZoneName.Contains("Deep"))
                    {
                        Lua.LuaDoString("ClearTarget()");
                        wManager.Wow.Helpers.MovementManager.Face(positionOnTramIronforge);

                        while (ObjectManager.Me.Position.DistanceTo(positionOnTramIronforge) &gt; 2)
                        {
                            MovementManager.MoveTo(positionOnTramIronforge);
                            Thread.Sleep(1000);

                            if (ObjectManager.Me.Position.DistanceTo(playerFellDownIronforge) &lt; 4)
                            {
                                Logging.Write("[FNV_Quester]: Fell down, while trying to take tram. Restart");
                                restartTram("Ironforge");
                            }
                        }

                        GoToTask.ToPosition(positionOnTramIronforge);
                    }


                    while (tram.Position.DistanceTo(positionTramInStormwind) &gt; 5 &amp;&amp; Usefuls.MapZoneName.Contains("Deep"))
                    {
                        Thread.Sleep(3000);
                    }

                    if (ObjectManager.Me.Position.DistanceTo(positionOnTramStormwind) &lt; 5 &amp;&amp; Usefuls.MapZoneName.Contains("Deep"))
                    {

                        Vector3 leavingTramPos = new Vector3(-4.500117, 2510.398, -4.18221, "None");
                        Vector3 leavingTramPosZwei = new Vector3(-8.363175, 2510.536, -4.291304, "None");

                        MovementManager.MoveTo(leavingTramPos);
                        Thread.Sleep(1000);
                        MovementManager.MoveTo(leavingTramPosZwei);
                        Thread.Sleep(1000);

                        var pathZwei = new List&lt;Vector3&gt;() {
new Vector3(-8.722651f, 2521.781f, -4.296569f, "None"),
new Vector3(-8.406331f, 2528.227f, -4.296569f, "None"),
new Vector3(-5.509378f, 2534.194f, -4.296569f, "None"),
new Vector3(1.102043f, 2536.338f, -4.296569f, "None"),
new Vector3(8.024714f, 2535.66f, -4.296569f, "None"),
new Vector3(13.7771f, 2531.926f, -4.296569f, "None"),
new Vector3(15.53208f, 2525.218f, -4.296569f, "None"),
new Vector3(19.39666f, 2520.334f, -4.296569f, "None"),
new Vector3(26.3053f, 2519.244f, -4.296569f, "None"),
new Vector3(31.07252f, 2514.534f, -4.296569f, "None"),
new Vector3(32.08603f, 2507.629f, -4.296569f, "None"),
new Vector3(32.20792f, 2500.735f, -4.296569f, "None"),
new Vector3(32.69915f, 2493.784f, -4.296569f, "None"),
new Vector3(38.39471f, 2490.469f, -4.296569f, "None"),
new Vector3(45.39016f, 2490.699f, -4.296569f, "None"),
new Vector3(52.39009f, 2490.728f, -4.296569f, "None"),
new Vector3(55.99506f, 2490.741f, -4.296569f, "None"),
new Vector3(62.99502f, 2490.766f, -4.296569f, "None"),
};


                        while (ObjectManager.Me.Position.DistanceTo2D(pathZwei.Last&lt;Vector3&gt;()) &gt; 5)
                        {
                            MovementManager.Go(pathZwei); // or MovementManager.GoLoop(path);

                            if (ObjectManager.Me.Position.DistanceTo2D(pathZwei.Last&lt;Vector3&gt;()) &lt;= 5)
                                break;
                        }


                        //MovementManager.MoveTo(leavePosStormwind);
                        /*
                        while(ObjectManager.Me.Position.DistanceTo(leavePosStormwind) &gt; 4)
                        {
                            MovementManager.MoveTo(leavePosStormwind);
                            Thread.Sleep(1000);

                        } */

                    }

                    if (Usefuls.MapZoneName.Contains("Deep"))
                    {
                        if (!GoToTask.ToPosition(enterStormwindInside))
                        {
                            GoToTask.ToPosition(enterStormwindInside);
                            Thread.Sleep(250);
                        }
                    }

                    while (Usefuls.MapZoneName.Contains("Deep"))
                    {
                        wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 250);
                        Thread.Sleep(robotManager.Helpful.Others.Random(25, 50));
                    }

                    Logging.Write("Re enable teleport ");
                    wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;


                    while (!GoToTask.ToPosition(enterStormwindOutside))
                    {
                        wManager.Wow.Helpers.MovementManager.Face(enterStormwindOutside);
                        GoToTask.ToPosition(enterStormwindOutside);
                        Thread.Sleep(250);
                    }

                }
                else if (Usefuls.MapZoneName.Contains("Tram") || Usefuls.MapZoneName.Contains("Ironforge"))
                {
                    Logging.Write("[FNV_Quester]: Unable to find tram, restart... .");
                    isRestarted = true;
                    restartTram("Ironforge");
                }

            }
        }
        isRestarted = false;
    }

    /*
    public static void takeTramIronforge()
    {
        Logging.Write("[FNV_Quester]: Taking Tram from Ironforge to Stormwind");

        if(!Usefuls.MapZoneName.Contains("Deep"))
        {
            while(!GoToTask.ToPosition(enterIronforgeOutside))
            {
                GoToTask.ToPosition(enterIronforgeOutside);
                Thread.Sleep(250);
            }

            wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
            Logging.Write("[FNV_Quester]: Disable teleport during Tram");
            wManager.Wow.Helpers.MovementManager.Face(new Vector3(-4839.4, -1320.9, 501.9));

            while(!Usefuls.MapZoneName.Contains("Deep"))
            {
                wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 250);
                Thread.Sleep(robotManager.Helpful.Others.Random(25, 50));
            }
        }

        if(Usefuls.MapZoneName.Contains("Deep"))
        {
            if(!GoToTask.ToPosition(enterIronforgeInside))
            {
                GoToTask.ToPosition(enterIronforgeInside);
                Thread.Sleep(250);
            }

            GoToTask.ToPosition(new Vector3(26.31003, 9.936551, -4.29664));
            Thread.Sleep(250);
            GoToTask.ToPosition(new Vector3(17.09965, 12.71437, -4.29664));
            Thread.Sleep(250);
            GoToTask.ToPosition(new Vector3(18.14983, 22.89955, -4.29664));
            Thread.Sleep(250);

            while(!GoToTask.ToPosition(positionWaitTramIronforge))
            {
                GoToTask.ToPosition(positionWaitTramIronforge);
                Thread.Sleep(250);
            }

            wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;

            var tram = (WoWGameObject)null;

            try
            {
                tram = chooseTramIronforge();
            }
            catch
            {

            }

            if(ObjectManager.Me.Position.DistanceTo(tram.Position) &lt; 200 &amp;&amp; tram != null &amp;&amp; !tram.Position.Equals(nullVector))
            {
                Logging.Write("[FNV_Quester]: Bad circle, restart... .");
                isRestarted = true;
                restartTram("Ironforge");
                end();
            }

            if(!isRestarted)
            {

                if(tram != null &amp;&amp; tram.Position.DistanceTo(nullVector) &gt; 1)
                {


                    while(tram.Position.DistanceTo(positionTramInIronforge) &gt;= 5 &amp;&amp; Usefuls.MapZoneName.Contains("Deep") &amp;&amp; !tram.Position.Equals(nullVector))
                    {
                        Thread.Sleep(1000);
                    }


                    if(tram.Position.DistanceTo(positionTramInIronforge) &lt; 5 &amp;&amp; Usefuls.MapZoneName.Contains("Deep"))
                    {
                        Lua.LuaDoString("ClearTarget()");
                        wManager.Wow.Helpers.MovementManager.Face(positionOnTramIronforge);

                        while(ObjectManager.Me.Position.DistanceTo(positionOnTramIronforge) &gt; 2)
                        {
                            wManager.Wow.Helpers.MovementManager.Face(positionOnTramIronforge);
                            wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 125);
                            Thread.Sleep(robotManager.Helpful.Others.Random(10, 25));

                            if(ObjectManager.Me.Position.DistanceTo(playerFellDownIronforge) &lt; 4)
                            {
                                Logging.Write("[FNV_Quester]: Fell down, while trying to take tram. Restart");
                                restartTram("Ironforge");
                            }
                        }

                        GoToTask.ToPosition(positionOnTramIronforge);
                    }


                    while(tram.Position.DistanceTo(positionTramInStormwind) &gt; 5 &amp;&amp; Usefuls.MapZoneName.Contains("Deep"))
                    {
                        Thread.Sleep(3000);
                    }

                    if(ObjectManager.Me.Position.DistanceTo(positionOnTramStormwind) &lt; 5 &amp;&amp; Usefuls.MapZoneName.Contains("Deep"))
                    {

                        wManager.Wow.Helpers.Move.StrafeLeft(Move.MoveAction.PressKey, 350);

                        while(ObjectManager.Me.Position.DistanceTo(leavePosStormwind) &gt; 4)
                        {
                            wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 125);
                            Thread.Sleep(robotManager.Helpful.Others.Random(10, 25));

                        }

                    }

                    if(Usefuls.MapZoneName.Contains("Deep"))
                    {
                        if(!GoToTask.ToPosition(enterStormwindInside))
                        {
                            GoToTask.ToPosition(enterStormwindInside);
                            Thread.Sleep(250);
                        }
                    }

                    while(Usefuls.MapZoneName.Contains("Deep"))
                    {
                        wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 250);
                        Thread.Sleep(robotManager.Helpful.Others.Random(25, 50));
                    }

                    Logging.Write("Re enable teleport ");
                    wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;


                    while(!GoToTask.ToPosition(enterStormwindOutside))
                    {
                        wManager.Wow.Helpers.MovementManager.Face(enterStormwindOutside);
                        GoToTask.ToPosition(enterStormwindOutside);
                        Thread.Sleep(250);
                    }

                }
                else if(Usefuls.MapZoneName.Contains("Tram") || Usefuls.MapZoneName.Contains("Ironforge"))
                {
                    Logging.Write("[FNV_Quester]: Unable to find tram, restart... .");
                    isRestarted = true;
                    restartTram("Ironforge");
                }

            }
        }
        isRestarted = false;
    }
    */
}

//Additional functions for quests
public class Quests
{
    public static void abandon(string questName)
    {
        wManager.Wow.Helpers.Lua.LuaDoString("local name = '" + questName + "' for i=1,GetNumQuestLogEntries() do local questTitle, level, questTag, suggestedGroup, isHeader, isCollapsed, isComplete = GetQuestLogTitle(i) if string.find(questTitle, name) then SelectQuestLogEntry(i) SetAbandonQuest() AbandonQuest() end end");

    }

    public static bool hasFailed(string questName)
    {
        int temp = 0;
        temp = wManager.Wow.Helpers.Lua.LuaDoString&lt;int&gt;("local name = '" + questName + "'  for i=1,GetNumQuestLogEntries() do local questTitle, level, questTag, suggestedGroup, isHeader, isCollapsed, isComplete = GetQuestLogTitle(i) if string.find(questTitle, name) then SelectQuestLogEntry(i) local questTimer = GetQuestLogTimeLeft() return questTimer end end");

        if (temp &gt; 8)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool isComplete(string questName)
    {
        int temp;
        temp = wManager.Wow.Helpers.Lua.LuaDoString&lt;int&gt;("local name = '" + questName + "'  for i=1,GetNumQuestLogEntries() do local questTitle, level, questTag, isHeader, isCollapsed, isComplete = GetQuestLogTitle(i) if string.find(questTitle, name) then local questTitleZ, levelZ, questTagZ, isHeaderZ, isCollapsedZ, isCompleteZ = GetQuestLogTitle(i) return isCompleteZ end end");

        if (temp == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool isFailed(string questName)
    {
        int temp;
        temp = wManager.Wow.Helpers.Lua.LuaDoString&lt;int&gt;("local name = '" + questName + "'  for i=1,GetNumQuestLogEntries() do local questTitle, level, questTag, isHeader, isCollapsed, isComplete = GetQuestLogTitle(i) if string.find(questTitle, name) then local questTitleZ, levelZ, questTagZ, isHeaderZ, isCollapsedZ, isCompleteZ = GetQuestLogTitle(i) return isCompleteZ end end");

        if (temp == -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}


public class ProgressSettings : Settings
{
    public static bool inProgress = false;
    public static void Initialize()
    {
        ProgressSettings.Load();
    }

    public static void Dispose()
    {
        ProgressSettings.CurrentSettings.Save();
        //isLaunched = false;
    }

    public void Settings()
    {
        ProgressSettings.Load();
        ProgressSettings.CurrentSettings.ToForm();
        ProgressSettings.CurrentSettings.Save();
    }

    public ProgressSettings()
    {
        this.launcher = 2;
        this.dwarfStart = 1 + 1;
        this.humanStart = 1 + 1;
        this.nightelfStart = 1 + 1;
        this.dwarfHunter = 1 + 1;
        this.nightelfHunter = 1 + 1;
        this.nightelfDruid = 1 + 1;
        this.warlock = 1 + 1;
        this.kharanos = 1 + 1;
        this.elwynn = 1 + 1;
        this.westfallEins = 1 + 1;
        this.lochModanEins = 1 + 1;
        this.darkshoreEins = 10 + 1;
        this.westfallZwei = 1 + 1;
        this.lochModanZwei = 1 + 1;
        this.darkshoreZwei = 6 + 1;
        this.westfallDrei = 1 + 1;
        this.redridge = 1 + 1;
        this.duskwoodEins = 1 + 1;
        this.stonetalon = 14 + 1;
        this.darkshoreDrei = 1 + 1;
        this.duskwoodZwei = 1 + 1;
        this.wetlandsEins = 1 + 1;
        this.ashenvaleEins = 7 + 1;
        this.wetlandsZwei = 1 + 1;
        this.duskwoodDrei = 2 + 1;
        this.ashenvaleZwei = 7 + 1;
        this.duskwoodVier = 2 + 1;
        this.ashenvaleDrei = 6 + 1;
        this.thousandNeedles = 7 + 1;
        this.desolaceEins = 7 + 1;
        this.hillsbradEins = 2 + 1;
        this.stranglethornEins = 2 + 1;
        this.stranglethornZwei = 2 + 1;
        this.hillsbradZwei = 2 + 1;
        this.arathiEins = 2 + 1;
        this.stranglethornDrei = 2 + 1;
        this.dustwallowEins = 10 + 1;
        this.desolaceZwei = 9 + 1;
        this.swampOfSorrowsEins = 2 + 1;
        this.stranglethornVier = 2 + 1;
        this.badlandsEins = 2 + 1;
        this.alteracMountain = 2 + 1;
        this.badlandsZwei = 2 + 1;
        this.tanarisEins = 7 + 1;
        this.stranglethornFünf  = 2 + 1;
        this.hinterlandsEins = 2 + 1;
        this.feralasEins = 7 + 1;
        this.hinterlandsZwei = 2 + 1;

        //48+

        this.hinterlandsDrei = 2 + 1;
        this.blastedLandsEins = 2 + 1;
        this.blastedLandsZwei = 2 + 1;
        this.blastedLandsDrei = 2 + 1;
        this.burningSteppesEins = 2 + 1;
        this.burningSteppesZwei = 2 + 1;
        this.burningSteppesDrei = 2 + 1;
        this.easternPleaguelandsEins = 2 + 1;
        this.easternPleaguelandsZwei = 2 + 1;
        this.easternPleaguelandsDrei = 2 + 1;
        this.searingGorgeEins = 2 + 1;
        this.searingGorgeZwei = 2 + 1;
        this.searingGorgeDrei = 2 + 1;
        this.swampOfSorrowsZwei = 2 + 1;
        this.westernPleaguelandsEins = 2 + 1;
        this.westernPleaguelandsZwei = 2 + 1;
        this.westernPleaguelandsDrei = 2 + 1;

        //48+ Kalimdor

        this.azsharaEins = 7 + 1;
        this.azsharaZwei = 7 + 1;
        this.azsharaDrei = 7 + 1;
        this.felwoodEins = 7 + 1;
        this.felwoodZwei = 7 + 1;
        this.felwoodDrei = 7 + 1;
        this.silithusEins = 7 + 1;
        this.silithusZwei = 7 + 1;
        this.silithusDrei = 7 + 1;
        this.feralasZwei = 7 + 1;
        this.feralasDrei = 7 + 1;
        this.feralasVier = 7 + 1;
        this.tanarisZwei = 7 + 1;
        this.tanarisDrei = 7 + 1;
        this.ungoroEins = 7 + 1;
        this.ungoroZwei = 7 + 1;
        this.ungoroDrei = 7 + 1;
        this.winterspringEins = 7 + 1;
        this.winterspringZwei = 7 + 1;
        this.winterspringDrei = 7 + 1;
        this.winterspringVier = 7 + 1;
    }


    public static ProgressSettings CurrentSettings { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("FNV_Progress", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteDebug("FNV_Progress =&gt; Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("FNV_Progress", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSettings = Load&lt;ProgressSettings&gt;(AdviserFilePathAndName("FNV_Progress", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }

            ProgressSettings.CurrentSettings = new ProgressSettings();
        }
        catch (Exception e)
        {
            Logging.WriteDebug("FNV_Progress =&gt; Load(): " + e);
        }
        return false;
    }

    public static bool ResetCurrentCharactersProgressSaver()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("FNV_Progress", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                File.Delete(AdviserFilePathAndName("FNV_Progress", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                ProgressSettings.CurrentSettings = new ProgressSettings();
                Logging.Write("[FNV_Quester]: FNV_Progress file of " + ObjectManager.Me.Name + " on server " + Usefuls.RealmName + " successfully deleted");
                return true;
            }
            else
            {
                Logging.Write("[FNV_Quester]: No FNV_Progress file found for " + ObjectManager.Me.Name + " on server " + Usefuls.RealmName);
                return false;
            }
        }
        catch (Exception e)
        {
            Logging.WriteDebug("FNV_Progress =&gt; Load(): " + e);
        }
        return false;
    }

    public int launcher { get; set; }
    public int dwarfStart { get; set; }
    public int dwarfHunter { get; set; }
    public int nightelfHunter { get; set; }
    public int nightelfDruid { get; set; }
    public int warlock { get; set; }
    public int nightelfStart { get; set; }
    public int humanStart { get; set; }
    public int kharanos { get; set; }
    public int elwynn { get; set; }
    public int westfallEins { get; set; }
    public int lochModanEins { get; set; }
    public int darkshoreEins { get; set; }
    public int westfallZwei { get; set; }
    public int lochModanZwei { get; set; }
    public int darkshoreZwei { get; set; }
    public int westfallDrei { get; set; }
    public int redridge { get; set; }
    public int duskwoodEins { get; set; }
    public int darkshoreDrei { get; set; }
    public int stonetalon { get; set; }
    public int duskwoodZwei { get; set; }
    public int wetlandsEins { get; set; }
    public int ashenvaleEins { get; set; }
    public int wetlandsZwei { get; set; }
    public int duskwoodDrei { get; set; }
    public int ashenvaleZwei { get; set; }
    public int duskwoodVier { get; set; }
    public int ashenvaleDrei { get; set; }
    public int thousandNeedles { get; set; }
    public int desolaceEins { get; set; }
    public int hillsbradEins { get; set; }
    public int stranglethornEins { get; set; }
    public int stranglethornZwei { get; set; }
    public int hillsbradZwei { get; set; }
    public int arathiEins { get; set; }
    public int stranglethornDrei { get; set; }
    public int dustwallowEins { get; set; }
    public int desolaceZwei { get; set; }
    public int badlandsEins { get; set; }
    public int swampOfSorrowsEins { get; set; }
    public int stranglethornVier { get; set; }
    public int alteracMountain { get; set; }
    public int badlandsZwei { get; set; }
    public int tanarisEins { get; set; }
    public int stranglethornFünf { get; set; }
public int hinterlandsEins { get; set; }
public int feralasEins { get; set; }
public int hinterlandsZwei { get; set; }

//48+ Eastern Kingdoms
public int hinterlandsDrei { get; set; }
public int blastedLandsEins { get; set; }
public int blastedLandsZwei { get; set; }
public int blastedLandsDrei { get; set; }
public int burningSteppesEins { get; set; }
public int burningSteppesZwei { get; set; }
public int burningSteppesDrei { get; set; }
public int easternPleaguelandsEins { get; set; }
public int easternPleaguelandsZwei { get; set; }
public int easternPleaguelandsDrei { get; set; }
public int searingGorgeEins { get; set; }
public int searingGorgeZwei { get; set; }
public int searingGorgeDrei { get; set; }
public int swampOfSorrowsZwei { get; set; }
public int westernPleaguelandsEins { get; set; }
public int westernPleaguelandsZwei { get; set; }
public int westernPleaguelandsDrei { get; set; }

//48+ Kalimdor

public int azsharaEins { get; set; }
public int azsharaZwei { get; set; }
public int azsharaDrei { get; set; }
public int felwoodEins { get; set; }
public int felwoodZwei { get; set; }
public int felwoodDrei { get; set; }
public int silithusEins { get; set; }
public int silithusZwei { get; set; }
public int silithusDrei { get; set; }
public int feralasZwei { get; set; }
public int feralasDrei { get; set; }
public int feralasVier { get; set; }
public int tanarisZwei { get; set; }
public int tanarisDrei { get; set; }
public int ungoroEins { get; set; }
public int ungoroZwei { get; set; }
public int ungoroDrei { get; set; }
public int winterspringEins { get; set; }
public int winterspringZwei { get; set; }
public int winterspringDrei { get; set; }
public int winterspringVier { get; set; }





}


/*
    //Settings   
    public class QuesterSettings
    {
        private static bool isLaunched;

        public static void Initialize()
        {
            isLaunched = true;
            FNVSettings.Load();

            while(isLaunched &amp; Products.IsStarted)
            {
                Thread.Sleep(500);
                Dispose();
            }
        }

        public static void Dispose()
        {
            FNVSettings.CurrentSettings.Save();
            //isLaunched = false;
        }

        public void Settings()
        {
            FNVSettings.Load();
            FNVSettings.CurrentSettings.ToForm();
            FNVSettings.CurrentSettings.Save();
        }
    }

    */

    [Serializable]
public class FNVSettings : Settings
{

    public static bool inProgress = false;
    public static void Initialize()
    {
        FNVSettings.Load();
    }

    public static void Dispose()
    {
        FNVSettings.CurrentSettings.Save();
        //isLaunched = false;
    }

    public void Settings()
    {
        FNVSettings.Load();
        FNVSettings.CurrentSettings.ToForm();
        FNVSettings.CurrentSettings.Save();
    }

    public FNVSettings()
    {
        //Completed profiles -&gt; True == completed
        this.launcher = false;
        this.dwarfStart = false;
        this.humanStart = false;
        this.nightelfStart = false;
        this.dwarfHunter = false;
        this.nightelfHunter = false;
        this.nightelfDruid = false;
        this.warlock = false;
        this.kharanos = false;
        this.elwynn = false;
        this.westfallEins = false;
        this.lochModanEins = false;
        this.darkshoreEins = false;
        this.westfallZwei = false;
        this.lochModanZwei = false;
        this.darkshoreZwei = false;
        this.westfallDrei = false;
        this.redridge = false;
        this.duskwoodEins = false;
        this.darkshoreDrei = false;
        this.stonetalon = false;
        this.duskwoodZwei = false;
        this.wetlandsEins = false;
        this.ashenvaleEins = false;
        this.wetlandsZwei = false;
        this.duskwoodDrei = false;
        this.ashenvaleZwei = false;
        this.duskwoodVier = false;
        this.ashenvaleDrei = false;
        this.thousandNeedles = false;
        this.desolaceEins = false;
        this.hillsbradEins = false;
        this.stranglethornEins = false;
        this.stranglethornZwei = false;
        this.hillsbradZwei = false;
        this.arathiEins = false;
        this.stranglethornDrei = false;
        this.dustwallowEins = false;
        this.desolaceZwei = false;
        this.swampOfSorrowsEins = false;
        this.stranglethornVier = false;
        this.badlandsEins = false;
        this.alteracMountain = false;
        this.badlandsZwei = false;
        this.tanarisEins = false;
        this.stranglethornFünf = false;
        this.hinterlandsEins = false;
        this.feralasEins = false;
        this.hinterlandsZwei = false;

        //48+

        this.hinterlandsDrei = false;
        this.blastedLandsEins = false;
        this.blastedLandsZwei = false;
        this.blastedLandsDrei = false;
        this.burningSteppesEins = false;
        this.burningSteppesZwei = false;
        this.burningSteppesDrei = false;
        this.easternPleaguelandsEins = false;
        this.easternPleaguelandsZwei = false;
        this.easternPleaguelandsDrei = false;
        this.searingGorgeEins = false;
        this.searingGorgeZwei = false;
        this.searingGorgeDrei = false;
        this.swampOfSorrowsZwei = false;
        this.westernPleaguelandsEins = false;
        this.westernPleaguelandsZwei = false;
        this.westernPleaguelandsDrei = false;

        //48+ Kalimdor

        this.azsharaEins = false;
        this.azsharaZwei = false;
        this.azsharaDrei = false;
        this.felwoodEins = false;
        this.felwoodZwei = false;
        this.felwoodDrei = false;
        this.silithusEins = false;
        this.silithusZwei = false;
        this.silithusDrei = false;
        this.feralasZwei = false;
        this.feralasDrei = false;
        this.feralasVier = false;
        this.tanarisZwei = false;
        this.tanarisDrei = false;
        this.ungoroEins = false;
        this.ungoroZwei = false;
        this.ungoroDrei = false;
        this.winterspringEins = false;
        this.winterspringZwei = false;
        this.winterspringDrei = false;
        this.winterspringVier = false;

        //FlightMaster discovered
        //Eastern Kingdoms
        this.ArathiHighlands = false;
        this.Wetlands = false;
        this.WesternPlaguelands = false;
        this.EasternPlaguelands = false;
        this.HillsbradFoothills = false;
        this.TheHinterlands = false;
        this.LochModan = false;
        this.Ironforge = false;
        this.SearingGorge = false;
        this.BurningSteppes = false;
        this.RedridgeMountains = false;
        this.Stormwind = false;
        this.Westfall = false;
        this.Duskwood = false;
        this.StranglethornValley = false;

        //Kalimdor
        this.Teldrassil = false;
        this.Darkshore = false;
        this.Winterspring = false;
        this.Azshara = false;
        this.Ashenvale = false;
        this.StonetalonMountains = false;
        this.Desolace = false;
        this.TheBarrens = false;
        this.Tanaris = false;
        this.FeralasFeathermoon = false;
        this.FeralasThalanaar = false;
        this.UngoroCrater = false;
        this.DustwallowMarsh = false;
        this.Silithus = false;
        this.Moonglade = false;
        this.Felwood = false;

    }

    public static void flightMasterSaveChanges(FlightMasterDB needToChange)
    {

        if (needToChange.name.Contains("Arathi"))
            CurrentSettings.ArathiHighlands = true;

        if (needToChange.name.Contains("Wetlands"))
            CurrentSettings.Wetlands = true;

        if (needToChange.name.Contains("Western"))
            CurrentSettings.WesternPlaguelands = true;

        if (needToChange.name.Contains("Eastern"))
            CurrentSettings.EasternPlaguelands = true;

        if (needToChange.name.Contains("Hillsbrad"))
            CurrentSettings.HillsbradFoothills = true;

        if (needToChange.name.Contains("Hinterlands"))
            CurrentSettings.TheHinterlands = true;

        if (needToChange.name.Contains("Modan"))
            CurrentSettings.LochModan = true;

        if (needToChange.name.Contains("Ironforge"))
            CurrentSettings.Ironforge = true;

        if (needToChange.name.Contains("Searing"))
            CurrentSettings.SearingGorge = true;

        if (needToChange.name.Contains("Burning"))
            CurrentSettings.BurningSteppes = true;

        if (needToChange.name.Contains("Redridge"))
            CurrentSettings.RedridgeMountains = true;

        if (needToChange.name.Contains("Stormwind"))
            CurrentSettings.Stormwind = true;

        if (needToChange.name.Contains("Westfall"))
            CurrentSettings.Westfall = true;

        if (needToChange.name.Contains("Duskwood"))
            CurrentSettings.Duskwood = true;

        if (needToChange.name.Contains("Stranglethorn"))
            CurrentSettings.StranglethornValley = true;

        if (needToChange.name.Contains("Blasted"))

        if (needToChange.name.Contains("Teldrassil"))
            CurrentSettings.Teldrassil = true;

        if (needToChange.name.Contains("Darkshore"))
            CurrentSettings.Darkshore = true;

        if (needToChange.name.Contains("Winterspring"))
            CurrentSettings.Winterspring = true;

        if (needToChange.name.Contains("Azshara"))
            CurrentSettings.Azshara = true;

        if (needToChange.name.Contains("Ashenvale"))
            CurrentSettings.Ashenvale = true;

        if (needToChange.name.Contains("Stonetalon"))
            CurrentSettings.StonetalonMountains = true;

        if (needToChange.name.Contains("Desolace"))
            CurrentSettings.Desolace = true;

        if (needToChange.name.Contains("TheBarrens"))
            CurrentSettings.TheBarrens = true;

        if (needToChange.name.Contains("Tanaris"))
            CurrentSettings.Tanaris = true;

        if (needToChange.name.Contains("TheBarrens"))
            CurrentSettings.TheBarrens = true;

        if (needToChange.name.Contains("Feathermoon"))
            CurrentSettings.FeralasFeathermoon = true;

        if (needToChange.name.Contains("Thalanaar"))
            CurrentSettings.FeralasThalanaar = true;

        if (needToChange.name.Contains("ro Crater"))
            CurrentSettings.UngoroCrater = true;

        if (needToChange.name.Contains("Dustwallow"))
            CurrentSettings.DustwallowMarsh = true;

        if (needToChange.name.Contains("Silithus"))
            CurrentSettings.Silithus = true;

        if (needToChange.name.Contains("Felwood"))
            CurrentSettings.Felwood = true;

        FNVSettings.CurrentSettings.Save();
        Logging.Write("[FNV_Quester]: Settings saved of Flight Master " + needToChange.name);
        return;
    }

    public static void flightMasterSaveChanges(String needToChange)
    {

        if (needToChange.Contains("Arathi"))
            CurrentSettings.ArathiHighlands = true;

        if (needToChange.Contains("Wetlands"))
            CurrentSettings.Wetlands = true;

        if (needToChange.Contains("Western"))
            CurrentSettings.WesternPlaguelands = true;

        if (needToChange.Contains("Eastern"))
            CurrentSettings.EasternPlaguelands = true;

        if (needToChange.Contains("Hillsbrad"))
            CurrentSettings.HillsbradFoothills = true;

        if (needToChange.Contains("Hinterlands"))
            CurrentSettings.TheHinterlands = true;

        if (needToChange.Contains("Modan"))
            CurrentSettings.LochModan = true;

        if (needToChange.Contains("Ironforge"))
            CurrentSettings.Ironforge = true;

        if (needToChange.Contains("Searing"))
            CurrentSettings.SearingGorge = true;

        if (needToChange.Contains("Burning"))
            CurrentSettings.BurningSteppes = true;

        if (needToChange.Contains("Redridge"))
            CurrentSettings.RedridgeMountains = true;

        if (needToChange.Contains("Stormwind"))
            CurrentSettings.Stormwind = true;

        if (needToChange.Contains("Westfall"))
            CurrentSettings.Westfall = true;

        if (needToChange.Contains("Duskwood"))
            CurrentSettings.Duskwood = true;

        if (needToChange.Contains("Stranglethorn"))
            CurrentSettings.StranglethornValley = true;

        if (needToChange.Contains("Blasted"))

        if (needToChange.Contains("Teldrassil"))
            CurrentSettings.Teldrassil = true;

        if (needToChange.Contains("Darkshore"))
            CurrentSettings.Darkshore = true;

        if (needToChange.Contains("Winterspring"))
            CurrentSettings.Winterspring = true;

        if (needToChange.Contains("Azshara"))
            CurrentSettings.Azshara = true;

        if (needToChange.Contains("Ashenvale"))
            CurrentSettings.Ashenvale = true;

        if (needToChange.Contains("Stonetalon"))
            CurrentSettings.StonetalonMountains = true;

        if (needToChange.Contains("Desolace"))
            CurrentSettings.Desolace = true;

        if (needToChange.Contains("TheBarrens"))
            CurrentSettings.TheBarrens = true;

        if (needToChange.Contains("Tanaris"))
            CurrentSettings.Tanaris = true;

        if (needToChange.Contains("TheBarrens"))
            CurrentSettings.TheBarrens = true;

        if (needToChange.Contains("Feathermoon"))
            CurrentSettings.FeralasFeathermoon = true;

        if (needToChange.Contains("Thalanaar"))
            CurrentSettings.FeralasThalanaar = true;

        if (needToChange.Contains("ro Crater"))
            CurrentSettings.UngoroCrater = true;

        if (needToChange.Contains("Dustwallow"))
            CurrentSettings.DustwallowMarsh = true;

        if (needToChange.Contains("Silithus"))
            CurrentSettings.Silithus = true;

        if (needToChange.Contains("Felwood"))
            CurrentSettings.Felwood = true;

        FNVSettings.CurrentSettings.Save();
        Logging.Write("[FNV_Quester]: Settings saved of Flight Master " + needToChange);
        return;
    }

    public static FNVSettings CurrentSettings { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("FNV_Quester", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteDebug("FNV_Quester =&gt; Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("FNV_Quester", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSettings = Load&lt;FNVSettings&gt;(AdviserFilePathAndName("FNV_Quester", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }

            FNVSettings.CurrentSettings = new FNVSettings();
        }
        catch (Exception e)
        {
            Logging.WriteDebug("FNV_Quester =&gt; Load(): " + e);
        }
        return false;
    }

    //Finished profiles -&gt; True == completed
    public bool launcher { get; set; }
    public bool dwarfStart { get; set; }
    public bool dwarfHunter { get; set; }
    public bool nightelfHunter { get; set; }
    public bool nightelfDruid { get; set; }
    public bool warlock { get; set; }
    public bool nightelfStart { get; set; }
    public bool humanStart { get; set; }
    public bool kharanos { get; set; }
    public bool elwynn { get; set; }
    public bool westfallEins { get; set; }
    public bool lochModanEins { get; set; }
    public bool darkshoreEins { get; set; }
    public bool westfallZwei { get; set; }
    public bool lochModanZwei { get; set; }
    public bool darkshoreZwei { get; set; }
    public bool westfallDrei { get; set; }
    public bool redridge { get; set; }
    public bool duskwoodEins { get; set; }
    public bool darkshoreDrei { get; set; }
    public bool stonetalon { get; set; }
    public bool duskwoodZwei { get; set; }
    public bool wetlandsEins { get; set; }
    public bool ashenvaleEins { get; set; }
    public bool wetlandsZwei { get; set; }
    public bool duskwoodDrei { get; set; }
    public bool ashenvaleZwei { get; set; }
    public bool duskwoodVier { get; set; }
    public bool ashenvaleDrei { get; set; }
    public bool thousandNeedles { get; set; }
    public bool desolaceEins { get; set; }
    public bool hillsbradEins { get; set; }
    public bool stranglethornEins { get; set; }
    public bool stranglethornZwei { get; set; }
    public bool hillsbradZwei { get; set; }
    public bool arathiEins { get; set; }
    public bool stranglethornDrei { get; set; }
    public bool dustwallowEins { get; set; }
    public bool desolaceZwei { get; set; }
    public bool swampOfSorrowsEins { get; set; }
    public bool stranglethornVier { get; set; }
    public bool badlandsEins { get; set; }
    public bool alteracMountain { get; set; }
    public bool badlandsZwei { get; set; }
    public bool tanarisEins { get; set; }
    public bool stranglethornFünf { get; set; }
public bool hinterlandsEins { get; set; }
public bool feralasEins { get; set; }
public bool hinterlandsZwei { get; set; }

//48+ Eastern Kingdoms
public bool hinterlandsDrei { get; set; }
public bool blastedLandsEins { get; set; }
public bool blastedLandsZwei { get; set; }
public bool blastedLandsDrei { get; set; }
public bool burningSteppesEins { get; set; }
public bool burningSteppesZwei { get; set; }
public bool burningSteppesDrei { get; set; }
public bool easternPleaguelandsEins { get; set; }
public bool easternPleaguelandsZwei { get; set; }
public bool easternPleaguelandsDrei { get; set; }
public bool searingGorgeEins { get; set; }
public bool searingGorgeZwei { get; set; }
public bool searingGorgeDrei { get; set; }
public bool swampOfSorrowsZwei { get; set; }
public bool westernPleaguelandsEins { get; set; }
public bool westernPleaguelandsZwei { get; set; }
public bool westernPleaguelandsDrei { get; set; }

//48+ Kalimdor

public bool azsharaEins { get; set; }
public bool azsharaZwei { get; set; }
public bool azsharaDrei { get; set; }
public bool felwoodEins { get; set; }
public bool felwoodZwei { get; set; }
public bool felwoodDrei { get; set; }
public bool silithusEins { get; set; }
public bool silithusZwei { get; set; }
public bool silithusDrei { get; set; }
public bool feralasZwei { get; set; }
public bool feralasDrei { get; set; }
public bool feralasVier { get; set; }
public bool tanarisZwei { get; set; }
public bool tanarisDrei { get; set; }
public bool ungoroEins { get; set; }
public bool ungoroZwei { get; set; }
public bool ungoroDrei { get; set; }
public bool winterspringEins { get; set; }
public bool winterspringZwei { get; set; }
public bool winterspringDrei { get; set; }
public bool winterspringVier { get; set; }



// public bool hinterlandsZwei { get; set; }

public bool defaultSettings { get; set; }
// public bool dwarfStart { get; set; }
// public bool nightelfStart { get; set; }
//public bool humanStart { get; set; }
//  public bool westfallEins { get; set; }
public bool lochModan { get; set; }
public bool auberdine { get; set; }
// public bool westfallZwei { get; set; }
public bool lakeshire { get; set; }
public bool wetlands { get; set; }
public bool ashenvale { get; set; }
// public bool thousandNeedles { get; set; }
public bool dustwallow { get; set; }
public bool badlands { get; set; }
public bool alterac { get; set; }
public bool swampOfSorrows { get; set; }
public bool tanaris { get; set; }
public bool stranglethorn { get; set; }
//public bool hinterlandsEins { get; set; }
public bool feralas { get; set; }
// public bool hinterlandsZwei { get; set; }

//FlightMaster
//Eastern Kingdoms
public bool Stormwind { get; set; }
public bool Westfall { get; set; }
public bool RedridgeMountains { get; set; }
public bool Duskwood { get; set; }
public bool StranglethornValley { get; set; }
public bool Ironforge { get; set; }
public bool BurningSteppes { get; set; }
public bool SearingGorge { get; set; }
public bool LochModan { get; set; }
public bool Wetlands { get; set; }
public bool ArathiHighlands { get; set; }
public bool HillsbradFoothills { get; set; }
public bool WesternPlaguelands { get; set; }
public bool EasternPlaguelands { get; set; }
public bool TheHinterlands { get; set; }

//Kalimdor
public bool Ashenvale { get; set; }
public bool Azshara { get; set; }
public bool Darkshore { get; set; }
public bool Teldrassil { get; set; }
public bool Desolace { get; set; }
public bool DustwallowMarsh { get; set; }
public bool Felwood { get; set; }
public bool FeralasFeathermoon { get; set; }
public bool FeralasThalanaar { get; set; }
public bool Moonglade { get; set; }
public bool Silithus { get; set; }
public bool StonetalonMountains { get; set; }
public bool Tanaris { get; set; }
public bool TheBarrens { get; set; }
public bool UngoroCrater { get; set; }
public bool Winterspring { get; set; }

    }

    //Buy and check bags
    public class bags
{
    public static bool bagsStarted = false;

    public static int bagsEquipped()
    {
        int returnValue = 0;
        string empty = "";

        for (int i = 20; i &lt;= 23; i++)
        {
            empty = wManager.Wow.Helpers.Lua.LuaDoString&lt;string&gt;("return GetInventoryItemLink('player', " + i + ")");

            if (empty.Contains("o"))
                returnValue++;
        }

        return returnValue;
    }

    public static string checkMoney()
    {
        string returnVariableNameZ = "";

        returnVariableNameZ = wManager.Wow.Helpers.Lua.LuaDoString("returnVariableNameZ = GetMoney() return returnVariableNameZ", returnVariableNameZ);

        return returnVariableNameZ;
    }

    public static bool canEquip()
    {
        string returnVariableName = "";

        returnVariableName = wManager.Wow.Helpers.Lua.LuaDoString("returnVariableName = GetInventoryItemLink('player', 23 ) return returnVariableName", returnVariableName);


        if (returnVariableName.Equals(""))
        {
            return true;
        }
        else
        {
            return false;
        }

    }

}


//Abandon Quests
public class abandonQuest
{

    public static void abandon(string questName)
    {

        string name = questName;

        wManager.Wow.Helpers.Lua.LuaDoString("local name = '" + name + "' for i=1,GetNumQuestLogEntries() do local questTitle, level, questTag, suggestedGroup, isHeader, isCollapsed, isComplete = GetQuestLogTitle(i) if string.find(questTitle, name) then SelectQuestLogEntry(i) SetAbandonQuest() AbandonQuest() end end");


    }

}



//Throw away items - by Reapler
public class throwAway
{
    public static int GetItemQuantity(string itemName)
    {
        var execute =
            "local itemCount = 0; " +
            "for b=0,4 do " +
                "if GetBagName(b) then " +
                    "for s=1, GetContainerNumSlots(b) do " +
                        "local itemLink = GetContainerItemLink(b, s) " +
                        "if itemLink then " +
                            "local _, stackCount = GetContainerItemInfo(b, s)\t " +
                            "if string.find(itemLink, \"" + itemName + "\") then " +
                                "itemCount = itemCount + stackCount; " +
                            "end " +
                        "end " +
                    "end " +
                "end " +
            "end; " +
            "return itemCount; ";
        return Lua.LuaDoString&lt;int&gt;(execute);
    }

    /// &lt;summary&gt;
    /// Used to delete all items by name.
    /// &lt;/summary&gt;
    /// &lt;param name="itemName"&gt;The item to delete.&lt;/param&gt;
    /// &lt;param name="leaveAmount"&gt;The amount of items which remain in the bag.&lt;/param&gt;
    /// &lt;remarks&gt;Bug at links with "-"&lt;/remarks&gt;
    public static void DeleteItems(string itemName, int leaveAmount)
    {
        var itemQuantity = GetItemQuantity(itemName) - leaveAmount;
        if (string.IsNullOrWhiteSpace(itemName) || itemQuantity &lt;= 0)
            return;
        var execute =
            "local itemCount = " + itemQuantity + "; " +
            "local deleted = 0; " +
            "for b=0,4 do " +
                "if GetBagName(b) then " +
                    "for s=1, GetContainerNumSlots(b) do " +
                        "local itemLink = GetContainerItemLink(b, s) " +
                        "if itemLink then " +
                            "local _, stackCount = GetContainerItemInfo(b, s)\t " +
                            "local leftItems = itemCount - deleted; " +
                            "if string.find(itemLink, \"" + itemName + "\") and leftItems &gt; 0 then " +
                                "if stackCount &lt;= 1 then " +
                                    "PickupContainerItem(b, s); " +
                                    "DeleteCursorItem(); " +
                                    "deleted = deleted + 1; " +
                                "else " +
                                    "if (leftItems &gt; stackCount) then " +
                                        "SplitContainerItem(b, s, stackCount); " +
                                        "DeleteCursorItem(); " +
                                        "deleted = deleted + stackCount; " +
                                    "else " +
                                        "SplitContainerItem(b, s, leftItems); " +
                                        "DeleteCursorItem(); " +
                                        "deleted = deleted + leftItems; " +
                                    "end " +
                                "end " +
                            "end " +
                        "end " +
                    "end " +
                "end " +
            "end; ";
        Lua.LuaDoString(execute);
    }
}


public class Darnassus
{
    private static Vector3 darnassusEnterPos = new Vector3(8775.104, 962.8825, 30.33067);
    private static Vector3 darnassusLeavePos = new Vector3(9945.827, 2598.705, 1316.187);

    public static void enter()
    {
        Logging.Write("[FNV_Quester]: Moving to position to enter Darnassus portal...");

        GoToTask.ToPosition(darnassusEnterPos);

        Logging.Write("[FNV_Quester]: Enter Darnassus, disable teleport...");

        wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;

        Vector3 pos = new Vector3(8812.807f, 972.6884f, 32.50122f);

        while (ObjectManager.Me.Position.DistanceTo(pos) &gt; 1 &amp;&amp; Usefuls.SubMapZoneName.Contains("theran Village"))
        {
            wManager.Wow.Helpers.MovementManager.Face(pos);
            wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 125);
            Thread.Sleep(robotManager.Helpful.Others.Random(10, 25));
        }

        Logging.Write("[FNV_Quester]: Reenable teleport");

        wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;
    }

    public static void leave()
    {
        Logging.Write("[FNV_Quester]: Moving to position to leave Darnassus portal...");

        GoToTask.ToPosition(darnassusLeavePos);

        Logging.Write("[FNV_Quester]: Leave Darnassus, disable teleport...");

        wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;

        Vector3 pos = new Vector3(9946.378f, 2642.551f, 1316.749f);

        while (ObjectManager.Me.Position.DistanceTo(pos) &gt; 1 &amp;&amp; Usefuls.MapZoneName.Contains("Darnassus"))
        {
            wManager.Wow.Helpers.MovementManager.Face(pos);
            wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, 125);
            Thread.Sleep(robotManager.Helpful.Others.Random(10, 25));
        }

        Logging.Write("[FNV_Quester]: Reenable teleport");
        wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;
    }
}

[Serializable]
public class FNVQuesterFlightMaster : Settings
{
    public FNVQuesterFlightMaster()
    {
        //FlightMaster discovered
        //Eastern Kingdoms
        this.ArathiHighlands = false;
        this.Wetlands = false;
        this.WesternPlaguelands = false;
        this.EasternPlaguelands = false;
        this.HillsbradFoothills = false;
        this.TheHinterlands = false;
        this.LochModan = false;
        this.Ironforge = false;
        this.SearingGorge = false;
        this.BurningSteppes = false;
        this.RedridgeMountains = false;
        this.Stormwind = false;
        this.Westfall = false;
        this.Duskwood = false;
        this.StranglethornValley = false;

        //Kalimdor
        this.Teldrassil = false;
        this.Darkshore = false;
        this.Winterspring = false;
        this.Azshara = false;
        this.Ashenvale = false;
        this.StonetalonMountains = false;
        this.Desolace = false;
        this.TheBarrens = false;
        this.Tanaris = false;
        this.FeralasFeathermoon = false;
        this.FeralasThalanaar = false;
        this.UngoroCrater = false;
        this.DustwallowMarsh = false;
        this.Silithus = false;
        this.Moonglade = false;
        this.Felwood = false;

    }

    public static void flightMasterSaveChanges(FlightMasterDB needToChange)
    {

        if (needToChange.name.Contains("Arathi"))
            CurrentSettings.ArathiHighlands = true;

        if (needToChange.name.Contains("Wetlands"))
            CurrentSettings.Wetlands = true;

        if (needToChange.name.Contains("Western"))
            CurrentSettings.WesternPlaguelands = true;

        if (needToChange.name.Contains("Eastern"))
            CurrentSettings.EasternPlaguelands = true;

        if (needToChange.name.Contains("Hillsbrad"))
            CurrentSettings.HillsbradFoothills = true;

        if (needToChange.name.Contains("Hinterlands"))
            CurrentSettings.TheHinterlands = true;

        if (needToChange.name.Contains("Modan"))
            CurrentSettings.LochModan = true;

        if (needToChange.name.Contains("Ironforge"))
            CurrentSettings.Ironforge = true;

        if (needToChange.name.Contains("Searing"))
            CurrentSettings.SearingGorge = true;

        if (needToChange.name.Contains("Burning"))
            CurrentSettings.BurningSteppes = true;

        if (needToChange.name.Contains("Redridge"))
            CurrentSettings.RedridgeMountains = true;

        if (needToChange.name.Contains("Stormwind"))
            CurrentSettings.Stormwind = true;

        if (needToChange.name.Contains("Westfall"))
            CurrentSettings.Westfall = true;

        if (needToChange.name.Contains("Duskwood"))
            CurrentSettings.Duskwood = true;

        if (needToChange.name.Contains("Stranglethorn"))
            CurrentSettings.StranglethornValley = true;

        if (needToChange.name.Contains("Blasted"))

        if (needToChange.name.Contains("Teldrassil"))
            CurrentSettings.Teldrassil = true;

        if (needToChange.name.Contains("Darkshore"))
            CurrentSettings.Darkshore = true;

        if (needToChange.name.Contains("Winterspring"))
            CurrentSettings.Winterspring = true;

        if (needToChange.name.Contains("Azshara"))
            CurrentSettings.Azshara = true;

        if (needToChange.name.Contains("Ashenvale"))
            CurrentSettings.Ashenvale = true;

        if (needToChange.name.Contains("Stonetalon"))
            CurrentSettings.StonetalonMountains = true;

        if (needToChange.name.Contains("Desolace"))
            CurrentSettings.Desolace = true;

        if (needToChange.name.Contains("Tanaris"))
            CurrentSettings.Tanaris = true;

        if (needToChange.name.Contains("The Barrens"))
            CurrentSettings.TheBarrens = true;

        if (needToChange.name.Contains("Feathermoon"))
            CurrentSettings.FeralasFeathermoon = true;

        if (needToChange.name.Contains("Thalanaar"))
            CurrentSettings.FeralasThalanaar = true;

        if (needToChange.name.Contains("ro Crater"))
            CurrentSettings.UngoroCrater = true;

        if (needToChange.name.Contains("Dustwallow"))
            CurrentSettings.DustwallowMarsh = true;

        if (needToChange.name.Contains("Silithus"))
            CurrentSettings.Silithus = true;

        if (needToChange.name.Contains("Felwood"))
            CurrentSettings.Felwood = true;

        FNVQuesterFlightMaster.CurrentSettings.Save();
        Logging.Write("[FNV_FlightMaster]: Settings saved of Flight Master " + needToChange.name);
        return;
    }

    public static FNVQuesterFlightMaster CurrentSettings { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("FNVQuesterFlightMaster", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteDebug("FNVQuesterFlightMaster =&gt; Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("FNVQuesterFlightMaster", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSettings = Load&lt;FNVQuesterFlightMaster&gt;(AdviserFilePathAndName("FNVQuesterFlightMaster", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }

            FNVQuesterFlightMaster.CurrentSettings = new FNVQuesterFlightMaster();
        }
        catch (Exception e)
        {
            Logging.WriteDebug("FNVQuesterFlightMaster =&gt; Load(): " + e);
        }
        return false;
    }


    //FlightMaster
    //Eastern Kingdoms
    public bool Stormwind { get; set; }
    public bool Westfall { get; set; }
    public bool RedridgeMountains { get; set; }
    public bool Duskwood { get; set; }
    public bool StranglethornValley { get; set; }
    public bool Ironforge { get; set; }
    public bool BurningSteppes { get; set; }
    public bool SearingGorge { get; set; }
    public bool LochModan { get; set; }
    public bool Wetlands { get; set; }
    public bool ArathiHighlands { get; set; }
    public bool HillsbradFoothills { get; set; }
    public bool WesternPlaguelands { get; set; }
    public bool EasternPlaguelands { get; set; }
    public bool TheHinterlands { get; set; }

    //Kalimdor

    public bool Ashenvale { get; set; }
    public bool Azshara { get; set; }
    public bool Darkshore { get; set; }
    public bool Teldrassil { get; set; }
    public bool Desolace { get; set; }
    public bool DustwallowMarsh { get; set; }
    public bool Felwood { get; set; }
    public bool FeralasFeathermoon { get; set; }
    public bool FeralasThalanaar { get; set; }
    public bool Moonglade { get; set; }
    public bool Silithus { get; set; }
    public bool StonetalonMountains { get; set; }
    public bool Tanaris { get; set; }
    public bool TheBarrens { get; set; }
    public bool UngoroCrater { get; set; }
    public bool Winterspring { get; set; }

}

public class Boat
{
    private static bool _boatTaken = false;

    public static void menethilToAuberdine()
    {

        // Settings
        var zeppelinEntryId = 176310; // Zeppelin/Ship EntryId
                                      // From
        var fromZeppelinWaitPosition = new Vector3(-3709.475, -575.0988, 0); // Position where Zeppelin/Ship waits players (from)
        var fromPlayerWaitPosition = new Vector3(-3727.4, -581.3, 6.2); // Position where the player waits Zeppelin/Ship (from)
        var fromPlayerInZeppelinPosition = new Vector3(-3713.333, -571.7416, 6.098111); // Position where the player waits in the Zeppelin/Ship (from)
                                                                                        // To
        var toZeppelinWaitPosition = new Vector3(6406.216, 823.0809, 0); // Position where Zeppelin/Ship waits players (to)
        var toPlayerLeavePosition = new Vector3(6461.408, 806.1595, 6.770809); // Position to go out the Zeppelin/Ship (to)
                                                                               //Pos1
        var enterPos1 = new Vector3(-3724.827, -581.0698, 6.191196);
        //Pos2
        var enterPos2 = new Vector3(-3723.727, -580.3727, 6.184784);
        _boatTaken = false;

        if (!_boatTaken)
        {
            // Change WRobot settings:
            Logging.Write("[FNV_Quester]: Taking boat from Menethil to Auberdine");
            wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
            wManager.wManagerSetting.CurrentSetting.Repair = false;
            wManager.wManagerSetting.CurrentSetting.Selling = false;
            wManager.wManagerSetting.CurrentSetting.UsePathsFinder = false;

            // Code:
            if (!Conditions.InGameAndConnectedAndProductStartedNotInPause)
                return;

            while (Products.IsStarted &amp;&amp; !_boatTaken)
            {
                while (Usefuls.ContinentId != (int)ContinentId.Kalimdor)
                {
                    if (!ObjectManager.Me.InTransport)
                    {
                        if (GoToTask.ToPosition(fromPlayerWaitPosition))
                        {
                            var zeppelin = ObjectManager.GetWoWGameObjectByEntry(zeppelinEntryId).OrderBy(o =&gt; o.GetDistance).FirstOrDefault();
                            if (zeppelin != null &amp;&amp; zeppelin.Position.DistanceTo(fromZeppelinWaitPosition) &lt; 1)
                            {

                                MovementManager.MoveTo(enterPos1);
                                if (GoToTask.ToPosition(enterPos1))
                                    MovementManager.MoveTo(enterPos2);

                                Lua.LuaDoString("ClearTarget()");

                                //wManager.Wow.Helpers.MovementManager.Face(new Vector3(-3713.333, -571.7416, 6.098111));

                                MovementManager.MoveTo(fromPlayerInZeppelinPosition);
                            }
                        }
                    }
                }
                while (Usefuls.ContinentId == (int)ContinentId.Kalimdor)
                {
                    if (ObjectManager.Me.InTransport)
                    {
                        var zeppelin = ObjectManager.GetWoWGameObjectByEntry(zeppelinEntryId).OrderBy(o =&gt; o.GetDistance).FirstOrDefault();
                        if (zeppelin != null &amp;&amp; zeppelin.Position.DistanceTo(toZeppelinWaitPosition) &lt; 1)
                        {
                            MovementManager.MoveTo(toPlayerLeavePosition);
                            break;
                        }
                    }
                }

                Logging.Write("[FNV_Quester]: Boat taken from Menethil to Auberdine");

                wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;
                wManager.wManagerSetting.CurrentSetting.Repair = true;
                wManager.wManagerSetting.CurrentSetting.Selling = true;
                wManager.wManagerSetting.CurrentSetting.UsePathsFinder = true;
                _boatTaken = true;
            }
            return;
        }
        return;
    }
}

public class Key
{
    public static bool haveKey(int keyId)
    {
        bool haveKey = Lua.LuaDoString&lt;bool&gt;("local itemIdSearch = " + keyId + "; local bag = KEYRING_CONTAINER; for slot = 1,MAX_CONTAINER_ITEMS do local itemLink = GetContainerItemLink(bag,slot); local _, itemCount = GetContainerItemInfo(bag,slot); if itemLink and itemCount then local _,_,itemId = string.find(itemLink, '.*|Hitem:(%d+):.*'); if itemId and tonumber(itemId) == itemIdSearch then return true end end end return false");
        return haveKey;
    }
}




public class Authentication
{

    private readonly string orderId;
    private readonly string productId;
    private readonly string wRobotAuthKey;

    private readonly BackgroundWorker _validationThread = new BackgroundWorker();
    private bool _isRunning = false;
    private string authUrl;

    public Authentication(string orderId, string productId)
    {

        return;  if (orderId == null)
        {
            MessageBox.Show("You need to enter your transaction id (from your Rocketr email) into the plugin settings to use this!");
        }

        this.orderId = orderId;
        this.productId = productId;

        this.wRobotAuthKey = robotManager.Helpful.Others.StringBetween(authManager.LoginServer.GetSubcriptionInfoThread(), robotManager.Translate.Get("License Key") + ": ", "...  - " + robotManager.Translate.Get("Subscription time expire"));
        authUrl = "http://51.38.127.249:8080/authenticate?orderId=" + orderId.Trim() + "&amp;productId=" + productId + "&amp;wRobotAuthKey=" + wRobotAuthKey;



        _isRunning = true;
        _validationThread.DoWork += CheckValidiation;
        _validationThread.RunWorkerAsync();
    }

    ~Authentication()
    {
        _isRunning = false;
        _validationThread.DoWork -= CheckValidiation;
        _validationThread.Dispose();
    }

    private void CheckValidiation(object sender, DoWorkEventArgs e)
    {
        while (Products.IsStarted &amp;&amp; _isRunning)
        {
            try
            {
                bool timedOut = false;
                HttpWebResponse content = null;
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(authUrl);
                    request.Timeout = 30000;
                    request.ReadWriteTimeout = 30000;
                    content = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception ex)
                {
                    timedOut = true;
                }

                //give it another try
                if ((content != null &amp;&amp; content.StatusCode != HttpStatusCode.OK) || timedOut)
                {
                    Thread.Sleep(60 * 1000);
                    try
                    {
                        var request = (HttpWebRequest)WebRequest.Create(authUrl);
                        request.Timeout = 30000;
                        request.ReadWriteTimeout = 30000;
                        content = (HttpWebResponse)request.GetResponse();
                        timedOut = false;
                    }
                    catch (Exception ex)
                    {
                        timedOut = true;
                    }
                }

                string result = content != null ? new StreamReader(content.GetResponseStream()).ReadToEnd() : "false";

                if (!bool.Parse(result) || (content != null &amp;&amp; content.StatusCode != HttpStatusCode.OK) || timedOut)
                {
                    Products.ProductStop();
                    _isRunning = false;
                    _validationThread.DoWork -= CheckValidiation;
                    _validationThread.Dispose();
                    MessageBox.Show("You are trying to use a different wRobot key with the same order Id or your wRobot key is used by more than 10 IPs. \n\nBot name: " + ObjectManager.Me.Name + "\n\nIf your wRobot key has changed recently (f.e. after purchasing a new one), please contact FNV316");
                    return;
                }

                Thread.Sleep(60 * 1000);
            }
            catch (Exception exp)
            {
                Logging.WriteError("" + exp);
            }
        }
    }
}

/*
static ControlAuthentication()
{
    new Thread(() =&gt;
    {
        while (!Conditions.InGameAndConnectedAndAliveAndProductStarted)
        {
            Thread.Sleep(500);
        }
        Thread.Sleep(30000);
        string status = Logging.Status;
        if (_controlVariable != 5 &amp;&amp; !status.Contains("To Town") &amp;&amp; !status.Contains("Regeneration") &amp;&amp; !status.Contains("Attacked") &amp;&amp; !status.Contains("Trainers") &amp;&amp; !status.Contains("Ressurect") &amp;&amp; !status.Contains("Started"))
        {
            Products.ProductStop();
            MessageBox.Show("Please enter your ID faster, restart the Bot now.");
        }
        Logging.Write("[FNV_Quester]: Launcher authentication sucessful");
    }).Start();
}
*/

public class ControlAuthentication
{
    private static volatile int _controlVariable = 0;
    private static int _timeout = 0;
    private static bool _isRunning = true;
    private static bool disposeAuthThread = false;
    private static string profileName = "";

    public static void SetControl(int control)
    {
        _controlVariable = control;
    }

    static ControlAuthentication()
    { return;
        new Thread(() =&gt;
        {
            profileName = wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName;

            while (_isRunning &amp;&amp; !disposeAuthThread &amp;&amp; (wManager.Wow.Helpers.Conditions.ProductIsStarted || wManager.Wow.Helpers.Conditions.ProductInPause))
            {
                if (!wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(profileName))
                {
                    disposeAuthThread = true;
                    break;
                }

                if (!wManager.Wow.Helpers.Conditions.ProductIsStarted)
                {
                    disposeAuthThread = true;
                    break;
                }

                while (wManager.Wow.Helpers.Quest.QuesterCurrentContext.CurrentStep &lt; 2)
                {
                    Thread.Sleep(500);

                    if (_controlVariable == 5 || !wManager.Wow.Helpers.Conditions.ProductIsStarted)
                        break;

                    _timeout += 500;

                    if (_timeout &gt;= 910000)
                    {
                        Products.ProductStop();
                        MessageBox.Show("[FNV_Quester]: Timeout error. Unable to authenticate for more than 15 minutes. \n\nBot name: " + ObjectManager.Me.Name);
                        break;
                    }
                }

                _isRunning = false;

                if (wManager.Wow.Helpers.Conditions.ProductIsStarted || wManager.Wow.Helpers.Conditions.ProductInPause)
                {
                    Thread.Sleep(60000);

                    if (_controlVariable != 5)
                    {
                        Products.ProductStop();
                        MessageBox.Show("[FNV_Quester]: Please enter your ID faster or make sure the authentication step is enabled (#0 START_FNVLauncher_V2 -&gt; Step [1], any other profile -&gt; Step [0]). \nRestart the Bot now. \n\nBot name: " + ObjectManager.Me.Name);
                    }
                    _timeout = 0;
                    _isRunning = true;
                }
            }

        }).Start();
    }

}

[Serializable]
public class FNVQuesterAuthSettings : robotManager.Helpful.Settings
{
    [Setting]
    [Category("__IMPORTANT__")]
    [DisplayName("Rocketr Order id")]
    [Description("This is your tracking number for when you purchased this product, it is required to use this consistently. You can find it within your product delivery e-mail")]
    public string TransactionId { get { return "free"; } set { } }

    public FNVQuesterAuthSettings()
    {
        TransactionId = null;
    }

    public static FNVQuesterAuthSettings CurrentSetting { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("FNVQuester_AuthSettings", "Vanilla_Alliance"));
        }
        catch (Exception e)
        {
            robotManager.Helpful.Logging.Write("FNVQuester_AuthSettings &gt; Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("FNVQuester_AuthSettings", "Vanilla_Alliance")))
            {
                CurrentSetting =
                    Load&lt;FNVQuesterAuthSettings&gt;(AdviserFilePathAndName("FNVQuester_AuthSettings", "Vanilla_Alliance"));
                return true;
            }
            CurrentSetting = new FNVQuesterAuthSettings();
        }
        catch (Exception e)
        {
            robotManager.Helpful.Logging.Write("FNVQuester_AuthSettings &gt; Load(): " + e);
        }
        return false;
    }
}

public class SearingGorge
{
    private static bool _isStarted = false;
    private static bool _inPause = false;
    private static int gateId = 161536;
    private static Vector3 gatePosition = new Vector3(-6756, -1166, 187);

    public static void initializeGateDetection()
    {
        if (!_isStarted)
        {
            Logging.Write("[FNV_Quester]: Searing Gorge gate detection initialized");
            _isStarted = true;
            MovementEvents.OnSeemStuck += MovementEventsOnOnSeemStuck;
        }
    }

    private static void pause()
    {
        for (int i = 0; i &lt; 15000; i += 1000)
        {
            Thread.Sleep(1000);
        }
        _inPause = false;
        return;
    }

    public static void disposeGateDetection()
    {
        if (_isStarted)
        {
            Logging.Write("[FNV_Quester]: Searing Gorge gate detection disposed");
            _isStarted = false;
            MovementEvents.OnSeemStuck -= MovementEventsOnOnSeemStuck;
        }
    }

    private static void MovementEventsOnOnSeemStuck()
    {
        Logging.Write("SeemStuck detected");

        if (ObjectManager.Me.Position.DistanceTo(gatePosition) &lt;= 10 &amp;&amp; !_inPause)
        {
            Logging.Write("[FNV_Quester]: Stuck at Quarry Gate, trying to open it...");

            GoToTask.ToPositionAndIntecractWithGameObject(gatePosition, gateId, -1, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore);

            if (GoToTask.ToPositionAndIntecractWithGameObject(gatePosition, gateId, -1, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore))
            {
                // _inPause = true;
                // pause();
            }
        }
    }
}

public class SearingGorgeFixedPathfinding
{
    private static List&lt;Vector3&gt; brokenLocationNodes = new List&lt;Vector3&gt;();
    private static bool _inProcessing;
    private static Vector3 currentDestination = new Vector3(0, 0, 0);
    private static string status = "";

    private static void ApplyNodes()
    {
        Vector3 thoriumPoint = new Vector3(-6514, -1162, 308);
        brokenLocationNodes.Add(thoriumPoint);
    }

    public static void Initialize()
    {
        Logging.Write("[FNV_Quester]: Thorium Point pathfinder workaround started");
        MovementEvents.OnMovementPulse += MovementEventsOnOnMovementPulse;
        MovementEvents.OnSeemStuck += MovementEventsOnOnSeemStuck;
        Logging.OnChanged += LoggingEvents_OnChangedStatus;
        ApplyNodes();
        _inProcessing = false;
    }

    public static void Dispose()
    {
        MovementEvents.OnMovementPulse -= MovementEventsOnOnMovementPulse;
        MovementEvents.OnSeemStuck -= MovementEventsOnOnSeemStuck;
        Logging.OnChanged -= LoggingEvents_OnChangedStatus;
    }

    private static void MovementEventsOnOnSeemStuck()
    {
        _inProcessing = false;
    }

    private static void MovementEventsOnOnMovementPulse(List&lt;Vector3&gt; points, CancelEventArgs cancelable)
    {
        //status = Logging.Status;
        if (CheckDestination(points.LastOrDefault&lt;Vector3&gt;()) &amp;&amp; !_inProcessing &amp;&amp; ObjectManager.Me.Position.DistanceTo(points.Last&lt;Vector3&gt;()) &gt; 50 &amp;&amp; !status.Contains("Follow Path"))
        {
            _inProcessing = true;
            Logging.Write("[FNV_Quester]: Destination vector within broken path finder range. Using custom path for movement to enter instead");
            cancelable.Cancel = true;
            UseCustomPathIn(points.LastOrDefault&lt;Vector3&gt;());
            cancelable.Cancel = false;
            _inProcessing = false;
        }

        if (CheckDestination(ObjectManager.Me.Position) &amp;&amp; !_inProcessing &amp;&amp; ObjectManager.Me.Position.DistanceTo(points.Last&lt;Vector3&gt;()) &gt; 50 &amp;&amp; !status.Contains("Follow Path"))
        {
            _inProcessing = true;
            Logging.Write("[FNV_Quester]: Starting vector within broken path finder range. Using custom path for movement to leave instead");
            cancelable.Cancel = true;
            UseCustomPathOut(points.LastOrDefault&lt;Vector3&gt;());
            cancelable.Cancel = false;
            _inProcessing = false;
        }

        if (CheckDestination(ObjectManager.Me.Position) &amp;&amp; !_inProcessing &amp;&amp; CheckDestination(points.Last&lt;Vector3&gt;()) &amp;&amp; ObjectManager.Me.Position.DistanceTo(points.Last&lt;Vector3&gt;()) &gt; 10 &amp;&amp; !status.Contains("Follow Path"))
        {
            _inProcessing = true;
            currentDestination = points.Last&lt;Vector3&gt;();
            Logging.Write("[FNV_Quester]: Starting and destination vector within broken path finder range. Using custom path for movement instead");
            cancelable.Cancel = true;
            UseCustomPathLoop(points.LastOrDefault&lt;Vector3&gt;());
            cancelable.Cancel = false;
            _inProcessing = false;
        }
    }


    private static void LoggingEvents_OnChangedStatus(object sender, Logging.LoggingChangeEventArgs e)
    {
        status = Logging.Status;
    }


    private static bool CheckDestination(Vector3 destination)
    {
        foreach (var ele in brokenLocationNodes)
        {
            //Logging.Write("Distance is: " + destination.DistanceTo(ele)); 
            if (destination.DistanceTo(ele) &lt;= 60)
                return true;
        }
        return false;
    }

    private static void UseCustomPathIn(Vector3 destination)
    {

        Vector3 currentDestination = new Vector3(0, 0, 0);

        var path = new List&lt;Vector3&gt;() {
new Vector3(-6605.966f, -1023.291f, 244.5074f, "None"),
new Vector3(-6601.826f, -1023.744f, 244.9385f, "None"),
new Vector3(-6597.218f, -1026.855f, 248.186f, "None"),
new Vector3(-6593.908f, -1031.353f, 252.414f, "None"),
new Vector3(-6591.3f, -1036.307f, 255.8175f, "None"),
new Vector3(-6588.852f, -1041.541f, 258.5742f, "None"),
new Vector3(-6586.07f, -1046.571f, 260.5392f, "None"),
new Vector3(-6582.013f, -1050.151f, 262.3918f, "None"),
new Vector3(-6577.125f, -1052.881f, 264.4617f, "None"),
new Vector3(-6572.03f, -1055.201f, 266.5196f, "None"),
new Vector3(-6566.89f, -1057.423f, 268.521f, "None"),
new Vector3(-6561.761f, -1059.671f, 270.6708f, "None"),
new Vector3(-6556.499f, -1062.034f, 272.971f, "None"),
new Vector3(-6551.548f, -1064.268f, 275.2576f, "None"),
new Vector3(-6546.625f, -1066.932f, 277.3527f, "None"),
new Vector3(-6541.8f, -1069.774f, 279.032f, "None"),
new Vector3(-6536.636f, -1071.892f, 280.7285f, "None"),
new Vector3(-6531.107f, -1072.746f, 282.576f, "None"),
new Vector3(-6525.543f, -1073.38f, 284.4381f, "None"),
new Vector3(-6519.98f, -1074.023f, 286.3544f, "None"),
new Vector3(-6514.447f, -1074.884f, 288.3375f, "None"),
new Vector3(-6508.975f, -1076.073f, 290.369f, "None"),
new Vector3(-6503.632f, -1077.715f, 292.0928f, "None"),
new Vector3(-6498.638f, -1080.577f, 293.695f, "None"),
new Vector3(-6494.001f, -1083.713f, 295.3442f, "None"),
new Vector3(-6490.028f, -1087.634f, 296.9394f, "None"),
new Vector3(-6486.872f, -1092.256f, 298.4525f, "None"),
new Vector3(-6484.295f, -1097.215f, 300.1746f, "None"),
new Vector3(-6483.278f, -1102.701f, 301.5711f, "None"),
new Vector3(-6483.816f, -1108.252f, 302.851f, "None"),
new Vector3(-6485.005f, -1113.707f, 304.0922f, "None"),
new Vector3(-6487.139f, -1118.884f, 305.0337f, "None"),
new Vector3(-6489.344f, -1124.032f, 305.7626f, "None"),
new Vector3(-6491.585f, -1129.163f, 306.1504f, "None"),
new Vector3(-6494.281f, -1134.475f, 306.4971f, "None"),
new Vector3(-6497.214f, -1139.442f, 306.8915f, "None"),
new Vector3(-6500.032f, -1144.085f, 307.3696f, "None"),
new Vector3(-6503.025f, -1149.016f, 307.632f, "None"),
new Vector3(-6505.93f, -1153.804f, 307.8793f, "None"),
new Vector3(-6508.76f, -1158.441f, 307.9853f, "None"),
new Vector3(-6512.148f, -1162.893f, 308.3222f, "None"),
new Vector3(-6517.508f, -1163.161f, 308.5844f, "None"),
            };

        foreach (var ele in path)
        {
            if (ele.DistanceTo(destination) &lt; ele.DistanceTo(currentDestination))
                currentDestination = ele;
        }

        if (wManager.Wow.Bot.Tasks.GoToTask.ToPosition(path.First&lt;Vector3&gt;(), 3.5f, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore))
        {
            wManager.Wow.Bot.Tasks.GoToTask.ToPosition(path.First&lt;Vector3&gt;(), 3.5f, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore);
        }

        while (ObjectManager.Me.Position.DistanceTo(currentDestination) &gt; 5 &amp;&amp; !currentDestination.Equals((Vector3)null) &amp;&amp; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore)
        {
            MovementManager.Go(path);
            Thread.Sleep(100);
        }

        //Logging.Write("Finished takeCustomPath");
        //Reenable();
        _inProcessing = false;
        return;
    }

    private static void UseCustomPathOut(Vector3 destination)
    {

        Vector3 currentDestination = new Vector3(0, 0, 0);

        var path = new List&lt;Vector3&gt;() {
new Vector3(-6520.889f, -1186.334f, 309.2557f, "None"),
new Vector3(-6511.543f, -1163.732f, 308.3743f, "None"),
new Vector3(-6508.988f, -1161.179f, 308.1429f, "None"),
new Vector3(-6504.05f, -1156.218f, 308.0315f, "None"),
new Vector3(-6501.398f, -1153.294f, 308.1518f, "None"),
new Vector3(-6495.091f, -1144.908f, 307.875f, "None"),
new Vector3(-6489.66f, -1136.319f, 307.1272f, "None"),
new Vector3(-6484.931f, -1126.579f, 306.6054f, "None"),
new Vector3(-6481.782f, -1116.925f, 305.3705f, "None"),
new Vector3(-6480.768f, -1106.506f, 302.6744f, "None"),
new Vector3(-6483.211f, -1096.021f, 299.9566f, "None"),
new Vector3(-6488.722f, -1087.545f, 297.2002f, "None"),
new Vector3(-6496.905f, -1080.999f, 294.2147f, "None"),
new Vector3(-6506.868f, -1076.894f, 291.0374f, "None"),
new Vector3(-6516.972f, -1075.943f, 287.6053f, "None"),
new Vector3(-6527.435f, -1075.096f, 283.8497f, "None"),
new Vector3(-6537.802f, -1073.449f, 280.6175f, "None"),
new Vector3(-6548.082f, -1070.127f, 276.9659f, "None"),
new Vector3(-6556.992f, -1065.242f, 273.4804f, "None"),
new Vector3(-6566.444f, -1059.943f, 269.1368f, "None"),
new Vector3(-6575.575f, -1055.484f, 265.3927f, "None"),
new Vector3(-6585.064f, -1050.99f, 261.5944f, "None"),
new Vector3(-6592.596f, -1046.345f, 258.3036f, "None"),
new Vector3(-6594.945f, -1035.768f, 253.8222f, "None"),
new Vector3(-6598.657f, -1026.457f, 247.278f, "None"),
new Vector3(-6605.125f, -1027.011f, 244.9202f, "None"),
new Vector3(-6614.792f, -1030.953f, 244.2407f, "None"),
};

        foreach (var ele in path)
        {
            if (ele.DistanceTo(destination) &lt; ele.DistanceTo(currentDestination))
                currentDestination = ele;
        }

        if (wManager.Wow.Bot.Tasks.GoToTask.ToPosition(path.First&lt;Vector3&gt;(), 3.5f, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore))
        {
            wManager.Wow.Bot.Tasks.GoToTask.ToPosition(path.First&lt;Vector3&gt;(), 3.5f, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore);
        }

        while (ObjectManager.Me.Position.DistanceTo(currentDestination) &gt; 5 &amp;&amp; !currentDestination.Equals((Vector3)null) &amp;&amp; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore)
        {
            MovementManager.Go(path);
            Thread.Sleep(100);
        }

        // Logging.Write("Finished takeCustomPathOut");
        //Reenable();
        _inProcessing = false;
        return;
    }

    private static void UseCustomPathLoop(Vector3 destination)
    {

        var path = new List&lt;Vector3&gt;() {
new Vector3(-6511.968f, -1178.022f, 309.2499f, "None"),
new Vector3(-6514.226f, -1180.391f, 309.2652f, "None"),
new Vector3(-6514.92f, -1183.646f, 309.2562f, "None"),
new Vector3(-6512.791f, -1186.548f, 309.2562f, "None"),
new Vector3(-6510.783f, -1189.414f, 309.2562f, "None"),
new Vector3(-6509.006f, -1192.427f, 309.2562f, "None"),
new Vector3(-6512.155f, -1191.501f, 309.2562f, "None"),
new Vector3(-6515.301f, -1190.243f, 309.2562f, "None"),
new Vector3(-6518.614f, -1189.136f, 309.2562f, "None"),
new Vector3(-6522.209f, -1188.875f, 309.2562f, "None"),
new Vector3(-6524.712f, -1188.661f, 309.2562f, "None"),
new Vector3(-6522.414f, -1187.187f, 309.2562f, "None"),
new Vector3(-6521.051f, -1184.315f, 309.2562f, "None"),
new Vector3(-6522.533f, -1181.164f, 309.5719f, "None"),
new Vector3(-6525.171f, -1178.908f, 310.5407f, "None"),
new Vector3(-6528.434f, -1177.668f, 311.458f, "None"),
new Vector3(-6531.906f, -1176.674f, 311.5628f, "None"),
new Vector3(-6535.271f, -1175.711f, 311.0577f, "None"),
new Vector3(-6538.62f, -1174.692f, 310.2652f, "None"),
new Vector3(-6541.887f, -1173.795f, 309.7975f, "None"),
new Vector3(-6545.396f, -1172.942f, 309.3479f, "None"),
new Vector3(-6548.806f, -1172.151f, 309.2578f, "None"),
new Vector3(-6552.216f, -1171.361f, 309.2927f, "None"),
new Vector3(-6555.617f, -1170.538f, 309.4913f, "None"),
new Vector3(-6558.993f, -1169.615f, 309.787f, "None"),
new Vector3(-6559.922f, -1167.805f, 309.8352f, "None"),
new Vector3(-6556.908f, -1165.864f, 310.0252f, "None"),
new Vector3(-6553.557f, -1164.877f, 310.0215f, "None"),
new Vector3(-6550.08f, -1164.484f, 309.926f, "None"),
new Vector3(-6544.97f, -1163.94f, 309.7741f, "None"),
new Vector3(-6534.195f, -1162.793f, 309.3187f, "None"),
new Vector3(-6523.741f, -1161.811f, 309.0215f, "None"),
new Vector3(-6513.615f, -1160.933f, 308.2635f, "None"),
new Vector3(-6506.418f, -1160.309f, 308.1041f, "None"),
new Vector3(-6502.819f, -1159.997f, 308.3569f, "None"),
new Vector3(-6499.444f, -1159.705f, 308.7631f, "None"),
new Vector3(-6496.717f, -1159.744f, 309.0773f, "None"),
new Vector3(-6495.118f, -1161.894f, 309.1303f, "None"),
new Vector3(-6493f, -1162.4f, 309.2f, "None"),
new Vector3(-6495.47f, -1165.365f, 309.19f, "None"),
new Vector3(-6496.578f, -1168.801f, 309.2209f, "None"),
new Vector3(-6497.424f, -1172.189f, 309.2521f, "None"),
new Vector3(-6496.599f, -1175.496f, 311.4286f, "None"),
new Vector3(-6494.471f, -1178.274f, 314.4402f, "None"),
new Vector3(-6492.365f, -1181.069f, 316.9778f, "None"),
new Vector3(-6490.402f, -1183.967f, 319.4284f, "None"),
new Vector3(-6488.469f, -1186.885f, 322.0103f, "None"),
new Vector3(-6486.568f, -1189.69f, 324.5566f, "None"),
new Vector3(-6484.392f, -1192.128f, 325.7157f, "None"),
new Vector3(-6479.565f, -1189.753f, 325.8502f, "None"),
new Vector3(-6477.512f, -1187.101f, 325.9147f, "None"),
new Vector3(-6478.307f, -1183.857f, 325.7819f, "None"),
new Vector3(-6480.627f, -1181.089f, 325.6377f, "None"),
new Vector3(-6482.864f, -1178.397f, 325.7611f, "None"),
new Vector3(-6485f, -1175.625f, 325.7878f, "None"),
new Vector3(-6487.067f, -1172.94f, 325.5157f, "None"),
new Vector3(-6489.333f, -1170.127f, 325.4131f, "None"),
new Vector3(-6492.227f, -1171.737f, 325.9383f, "None"),
new Vector3(-6495.255f, -1173.477f, 326.5012f, "None"),
new Vector3(-6498.819f, -1173.806f, 326.2171f, "None"),
new Vector3(-6501.867f, -1173.776f, 325.811f, "None"),
new Vector3(-6504.835f, -1175.808f, 325.9436f, "None"),
new Vector3(-6507.222f, -1178.351f, 326.3931f, "None"),
new Vector3(-6507.92f, -1181.704f, 326.9203f, "None"),
new Vector3(-6506.445f, -1184.833f, 327.1803f, "None"),
new Vector3(-6504.251f, -1187.702f, 326.8068f, "None"),
new Vector3(-6502.15f, -1190.501f, 326.4582f, "None"),
new Vector3(-6500.116f, -1193.21f, 326.1217f, "None"),
new Vector3(-6498.121f, -1196.085f, 325.64f, "None"),
new Vector3(-6496.097f, -1199.072f, 325.2523f, "None"),
new Vector3(-6493.148f, -1199.734f, 325.4391f, "None"),
new Vector3(-6490.279f, -1197.56f, 325.5786f, "None"),
new Vector3(-6488.075f, -1194.888f, 325.6983f, "None"),
new Vector3(-6489.056f, -1191.798f, 324.8035f, "None"),
new Vector3(-6491.162f, -1189.002f, 322.1753f, "None"),
new Vector3(-6493.301f, -1186.232f, 319.5414f, "None"),
new Vector3(-6495.447f, -1183.467f, 316.7057f, "None"),
new Vector3(-6497.518f, -1180.777f, 313.957f, "None"),
new Vector3(-6499.785f, -1177.831f, 311.3158f, "None"),
new Vector3(-6501.965f, -1175.094f, 309.255f, "None"),
new Vector3(-6504.853f, -1173.284f, 309.1891f, "None"),
new Vector3(-6507.97f, -1174.138f, 309.1942f, "None"),
new Vector3(-6511.066f, -1175.981f, 309.2368f, "None"),
new Vector3(-6513.179f, -1177.529f, 309.2469f, "None"),
};


        foreach (var ele in path)
        {
            if (ele.DistanceTo(destination) &lt; ele.DistanceTo(currentDestination))
                currentDestination = ele;
        }

        if (wManager.Wow.Bot.Tasks.GoToTask.ToPosition(path.First&lt;Vector3&gt;(), 3.5f, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore))
        {
            wManager.Wow.Bot.Tasks.GoToTask.ToPosition(path.First&lt;Vector3&gt;(), 3.5f, false, context =&gt; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore);
        }


        while (ObjectManager.Me.Position.DistanceTo(currentDestination) &gt; 2 &amp;&amp; !currentDestination.Equals((Vector3)null) &amp;&amp; Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause &amp;&amp; !Conditions.IsAttackedAndCannotIgnore)
        {
            if (ObjectManager.Me.Position.DistanceTo(currentDestination) &lt;= 2)
            {
                break;
            }

            MovementManager.Go(path);
            Thread.Sleep(100);
        }

        MovementManager.MoveTo(currentDestination);

        //Logging.Write("Finished takeCustomPathLoop");
        currentDestination = new Vector3(0, 0, 0);
        //Reenable();
        _inProcessing = false;
        return;
    }

    //By Matenia
    private static async void Reenable()
    {
        //Logging.Write("Enter reenable");
        await Task.Run(() =&gt;
        {
            Products.InPause = true;
            if (ObjectManager.Me.WowClass == WoWClass.Hunter)
                Lua.LuaDoString("RotaOn = false");
            MovementManager.StopMove();
            MovementManager.CurrentPath.Clear();
            MovementManager.CurrentPathOrigine.Clear();
            Thread.Sleep(5000);
            Products.InPause = false;
            if (ObjectManager.Me.WowClass == WoWClass.Hunter)
                Lua.LuaDoString("RotaOn = true");
            Logging.Write("[VanillaFlightMaster]: Resetting pathing");
        });
    }

}

public class QuesterSettings
{

    public static void ClassSettings()
    {
        if (ObjectManager.Me.WowClass == WoWClass.Warlock)
        {
            switch (ObjectManager.Me.Level / 10)
            {
                case (1):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (2):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (3):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (4):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 45;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 45;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (5):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (6):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                default:
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;
            }

            if (wManager.wManagerSetting.CurrentSetting.DrinkName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.DrinkIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.DrinkName = "Refreshing Spring Water";
            }


            if (wManager.wManagerSetting.CurrentSetting.FoodName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.FoodIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.FoodName = "Tough Hunk of Bread";
            }

            Logging.Write("[FNV_Quester]: Using default food / drink settings for Warlock");
        }

        if (ObjectManager.Me.WowClass == WoWClass.Warrior)
        {
            switch (ObjectManager.Me.Level / 10)
            {

                case (1):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (2):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (3):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 60;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 70;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (4):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 80;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (5):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 100;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 80;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (6):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 120;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 80;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                default:
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;
            }

            if (wManager.wManagerSetting.CurrentSetting.FoodName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.FoodIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.FoodName = "Tough Hunk of Bread";
            }

            Logging.Write("[FNV_Quester]: Using default food / drink settings for Warrior");
        }

        if (ObjectManager.Me.WowClass == WoWClass.Rogue)
        {
            switch (ObjectManager.Me.Level / 10)
            {
                case (1):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (2):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (3):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (4):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 75;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (5):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 100;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 80;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                case (6):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 120;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 80;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;

                default:
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 0;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = false;
                    break;
            }


            if (wManager.wManagerSetting.CurrentSetting.FoodName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.FoodIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.FoodName = "Tough Hunk of Bread";
            }

            Logging.Write("[FNV_Quester]: Using default food / drink settings for Rogue");
        }

        if (ObjectManager.Me.WowClass == WoWClass.Mage)
        {
            switch (ObjectManager.Me.Level / 10)
            {
                case (1):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (2):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (3):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (4):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 60;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 70;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (5):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 60;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 75;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (6):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 60;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 75;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                default:
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;
            }

            if (wManager.wManagerSetting.CurrentSetting.DrinkName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.DrinkIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.DrinkName = "Refreshing Spring Water";
            }


            if (wManager.wManagerSetting.CurrentSetting.FoodName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.FoodIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.FoodName = "Tough Hunk of Bread";
            }

            Logging.Write("[FNV_Quester]: Using default food / drink settings for Mage");
        }

        if (ObjectManager.Me.WowClass == WoWClass.Druid)
        {
            switch (ObjectManager.Me.Level / 10)
            {
                case (1):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (2):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (3):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (4):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 45;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (5):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (6):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                default:
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;
            }

            if (wManager.wManagerSetting.CurrentSetting.DrinkName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.DrinkIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.DrinkName = "Refreshing Spring Water";
            }

            if (wManager.wManagerSetting.CurrentSetting.FoodName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.FoodName = "Healing Touch";
                wManager.wManagerSetting.CurrentSetting.FoodIsSpell = true;
            }

            Logging.Write("[FNV_Quester]: Using default food / drink settings for Druid");
        }

        if (ObjectManager.Me.WowClass == WoWClass.Paladin)
        {

            switch (ObjectManager.Me.Level / 10)
            {
                case (1):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (2):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (3):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 60;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (4):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 45;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (5):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 50;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (6):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 100;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 55;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                default:
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 0;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 15;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;
            }

            if (wManager.wManagerSetting.CurrentSetting.DrinkName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.DrinkIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.DrinkName = "Refreshing Spring Water";
            }

            if (wManager.wManagerSetting.CurrentSetting.FoodName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.FoodName = "Holy Light";
                wManager.wManagerSetting.CurrentSetting.FoodIsSpell = true;
            }


            Logging.Write("[FNV_Quester]: Using default food / drink settings for Paladin");
        }

        if (ObjectManager.Me.WowClass == WoWClass.Hunter)
        {
            switch (ObjectManager.Me.Level / 10)
            {
                case (1):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    Lua.LuaDoString("DEFAULT_CHAT_FRAME:AddMessage('[FNV_Quester]: Do not forget that wRobot counts empty bag slots of ammo bags as free bag space!')");
                    break;

                case (2):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (3):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 40;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (4):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 60;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 60;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (5):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                case (6):
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 80;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    break;

                default:
                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.FoodAmount = 20;
                    wManager.wManagerSetting.CurrentSetting.DrinkMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.FoodMaxPercent = 95;
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
                    wManager.wManagerSetting.CurrentSetting.FoodPercent = 65;
                    wManager.wManagerSetting.CurrentSetting.RestingMana = true;
                    Lua.LuaDoString("DEFAULT_CHAT_FRAME:AddMessage('[FNV_Quester]: Do not forget that wRobot counts empty bag slots of ammo bags as free bag space!')");
                    break;
            }

            if (wManager.wManagerSetting.CurrentSetting.DrinkName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.DrinkIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.DrinkName = "Refreshing Spring Water";
            }

            if (wManager.wManagerSetting.CurrentSetting.FoodName.Length &lt; 2)
            {
                wManager.wManagerSetting.CurrentSetting.FoodIsSpell = false;
                wManager.wManagerSetting.CurrentSetting.FoodName = "Tough Hunk of Bread";
            }

            Logging.Write("[FNV_Quester]: Using default food / drink settings for Hunter");

        }
    }

    public static void SettingsEasternKingdoms()
    {
        //Eastern Kingdoms only
        wManager.wManagerSetting.CurrentSetting.TrainNewSkills = true;

        wManager.wManagerSetting.CurrentSetting.DontStartFighting = false;
        wManager.wManagerSetting.CurrentSetting.DetectNodesStuck = true;
        wManager.wManagerSetting.CurrentSetting.CanAttackUnitsAlreadyInFight = false;
        wManager.wManagerSetting.CurrentSetting.AvoidBlacklistedZonesPathFinder = true;
        wManager.wManagerSetting.CurrentSetting.AttackElite = false;
        wManager.wManagerSetting.CurrentSetting.AttackBeforeBeingAttacked = true;
        wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;
        wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUse = true;
        wManager.wManagerSetting.CurrentSetting.IgnoreFightWhenInMove = false;

        wManager.wManagerSetting.CurrentSetting.Repair = true;
        wManager.wManagerSetting.CurrentSetting.SellGray = true;
        wManager.wManagerSetting.CurrentSetting.SellGreen = true;
        wManager.wManagerSetting.CurrentSetting.Selling = true;
        wManager.wManagerSetting.CurrentSetting.SellWhite = true;
        wManager.wManagerSetting.CurrentSetting.MinFreeBagSlotsToGoToTown = 4;

        wManager.wManagerSetting.CurrentSetting.SearchRadius = 100;
        wManager.wManagerSetting.CurrentSetting.MaxUnitsNear = 100;

        wManager.wManagerSetting.CurrentSetting.WallDistancePathFinder = 2;
        wManager.wManagerSetting.CurrentSetting.AddToNpcDb = false;
        wManager.wManagerSetting.CurrentSetting.BlackListIfNotCompletePath = false;
        wManager.wManagerSetting.CurrentSetting.UseCTM = true;
        wManager.wManagerSetting.CurrentSetting.UseLuaToMove = true;

        wManager.wManagerSetting.CurrentSetting.NpcScanAuctioneer = false;
        wManager.wManagerSetting.CurrentSetting.NpcScanMailboxes = false;
        wManager.wManagerSetting.CurrentSetting.NpcScanRepair = false;
        wManager.wManagerSetting.CurrentSetting.NpcScanVendor = false;

        wManager.wManagerSetting.CurrentSetting.HarvestHerbs = false;
        wManager.wManagerSetting.CurrentSetting.HarvestMinerals = false;

        wManager.wManagerSetting.CurrentSetting.AvoidBlacklistedZonesPathFinder = true;
        wManager.wManagerSetting.CurrentSetting.AvoidWallWithRays = true;
        wManager.wManagerSetting.CurrentSetting.BlackListTrainingDummy = true;
        wManager.wManagerSetting.CurrentSetting.BlackListZoneWhereDead = false;
        wManager.wManagerSetting.CurrentSetting.CalcuCombatRange = false;
        wManager.wManagerSetting.CurrentSetting.CanAttackUnitsAlreadyInFight = false;
        wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
        wManager.wManagerSetting.CurrentSetting.DetectNodesStuck = true;
        wManager.wManagerSetting.CurrentSetting.DontStartFighting = false;
        wManager.wManagerSetting.CurrentSetting.FlightMasterDiscoverRange = 50;
        wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUse = true;
        wManager.wManagerSetting.CurrentSetting.HarvestAvoidPlayersRadius = 1;
        wManager.wManagerSetting.CurrentSetting.HarvestDuringLongMove = false;
        wManager.wManagerSetting.CurrentSetting.HarvestTimber = false;
        wManager.wManagerSetting.CurrentSetting.HelpingGroupMembers = true;
        wManager.wManagerSetting.CurrentSetting.IgnoreCombatWithPet = false;
        wManager.wManagerSetting.CurrentSetting.IgnoreFightDuringFarmIfDruidForm = false;
        wManager.wManagerSetting.CurrentSetting.IgnoreFightGoundMount = true;
        wManager.wManagerSetting.CurrentSetting.LootChests = false;
        wManager.wManagerSetting.CurrentSetting.LootMobs = true;
        wManager.wManagerSetting.CurrentSetting.MountDistance = 100;
        wManager.wManagerSetting.CurrentSetting.Relogger = true;
        wManager.wManagerSetting.CurrentSetting.SecurityPauseBotIfNerbyPlayer = false;
        wManager.wManagerSetting.CurrentSetting.RecordChatInLog = true;
        wManager.wManagerSetting.CurrentSetting.SecurityShutdownComputer = false;
        wManager.wManagerSetting.CurrentSetting.UsePathsFinder = true;
        wManager.wManagerSetting.CurrentSetting.UseSpiritHealer = false;
        wManager.wManagerSetting.CurrentSetting.WaitResurrectionSickness = true;
        wManager.wManagerSetting.CurrentSetting.WallDistancePathFinder = 2;

        ClassSettings();

        if (Conditions.ForceIgnoreIsAttacked)
        {
            Logging.Write("[FNV_Quester]: Force ignore attack is enabled, going to disable it...");
            Conditions.ForceIgnoreIsAttacked = false;
        }

        wManager.wManagerSetting.CurrentSetting.Save();
        Thread.Sleep(500);
        wManager.wManagerSetting.CurrentSetting.Save();

        wManager.Wow.Forms.UserControlTabGeneralSettings.ReloadGeneralSettings();

        Logging.Write("[FNV_Quester]: Applied default settings for Eastern Kingdoms");

        return;
    }

    public static void SettingsKalimdor()
    {
        //Kalimdor only
        wManager.wManagerSetting.CurrentSetting.TrainNewSkills = false;

        wManager.wManagerSetting.CurrentSetting.DontStartFighting = false;
        wManager.wManagerSetting.CurrentSetting.DetectNodesStuck = true;
        wManager.wManagerSetting.CurrentSetting.CanAttackUnitsAlreadyInFight = false;
        wManager.wManagerSetting.CurrentSetting.AvoidBlacklistedZonesPathFinder = true;
        wManager.wManagerSetting.CurrentSetting.AttackElite = false;
        wManager.wManagerSetting.CurrentSetting.AttackBeforeBeingAttacked = true;
        wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;
        wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUse = true;
        wManager.wManagerSetting.CurrentSetting.IgnoreFightWhenInMove = false;

        wManager.wManagerSetting.CurrentSetting.Repair = true;
        wManager.wManagerSetting.CurrentSetting.SellGray = true;
        wManager.wManagerSetting.CurrentSetting.SellGreen = true;
        wManager.wManagerSetting.CurrentSetting.Selling = true;
        wManager.wManagerSetting.CurrentSetting.SellWhite = true;
        wManager.wManagerSetting.CurrentSetting.MinFreeBagSlotsToGoToTown = 4;

        wManager.wManagerSetting.CurrentSetting.SearchRadius = 100;
        wManager.wManagerSetting.CurrentSetting.MaxUnitsNear = 100;

        wManager.wManagerSetting.CurrentSetting.WallDistancePathFinder = 2;
        wManager.wManagerSetting.CurrentSetting.AddToNpcDb = false;
        wManager.wManagerSetting.CurrentSetting.BlackListIfNotCompletePath = false;
        wManager.wManagerSetting.CurrentSetting.UseCTM = true;
        wManager.wManagerSetting.CurrentSetting.UseLuaToMove = true;

        wManager.wManagerSetting.CurrentSetting.NpcScanAuctioneer = false;
        wManager.wManagerSetting.CurrentSetting.NpcScanMailboxes = false;
        wManager.wManagerSetting.CurrentSetting.NpcScanRepair = false;
        wManager.wManagerSetting.CurrentSetting.NpcScanVendor = false;

        wManager.wManagerSetting.CurrentSetting.HarvestHerbs = false;
        wManager.wManagerSetting.CurrentSetting.HarvestMinerals = false;

        wManager.wManagerSetting.CurrentSetting.AvoidBlacklistedZonesPathFinder = true;
        wManager.wManagerSetting.CurrentSetting.AvoidWallWithRays = true;
        wManager.wManagerSetting.CurrentSetting.BlackListTrainingDummy = true;
        wManager.wManagerSetting.CurrentSetting.BlackListZoneWhereDead = false;
        wManager.wManagerSetting.CurrentSetting.CalcuCombatRange = false;
        wManager.wManagerSetting.CurrentSetting.CanAttackUnitsAlreadyInFight = false;
        wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
        wManager.wManagerSetting.CurrentSetting.DetectNodesStuck = true;
        wManager.wManagerSetting.CurrentSetting.DontStartFighting = false;
        wManager.wManagerSetting.CurrentSetting.FlightMasterDiscoverRange = 50;
        wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUse = true;
        wManager.wManagerSetting.CurrentSetting.HarvestAvoidPlayersRadius = 1;
        wManager.wManagerSetting.CurrentSetting.HarvestDuringLongMove = false;
        wManager.wManagerSetting.CurrentSetting.HarvestTimber = false;
        wManager.wManagerSetting.CurrentSetting.HelpingGroupMembers = true;
        wManager.wManagerSetting.CurrentSetting.IgnoreCombatWithPet = false;
        wManager.wManagerSetting.CurrentSetting.IgnoreFightDuringFarmIfDruidForm = false;
        wManager.wManagerSetting.CurrentSetting.IgnoreFightGoundMount = true;
        wManager.wManagerSetting.CurrentSetting.LootChests = false;
        wManager.wManagerSetting.CurrentSetting.LootMobs = true;
        wManager.wManagerSetting.CurrentSetting.MountDistance = 100;
        wManager.wManagerSetting.CurrentSetting.Relogger = true;
        wManager.wManagerSetting.CurrentSetting.SecurityPauseBotIfNerbyPlayer = false;
        wManager.wManagerSetting.CurrentSetting.RecordChatInLog = true;
        wManager.wManagerSetting.CurrentSetting.SecurityShutdownComputer = false;
        wManager.wManagerSetting.CurrentSetting.UsePathsFinder = true;
        wManager.wManagerSetting.CurrentSetting.UseSpiritHealer = false;
        wManager.wManagerSetting.CurrentSetting.WaitResurrectionSickness = true;
        wManager.wManagerSetting.CurrentSetting.WallDistancePathFinder = 2;

        ClassSettings();

        if (Conditions.ForceIgnoreIsAttacked)
        {
            Logging.Write("[FNV_Quester]: Force ignore attack is enabled, going to disable it...");
            Conditions.ForceIgnoreIsAttacked = false;
        }

        wManager.wManagerSetting.CurrentSetting.Save();
        Thread.Sleep(500);
        wManager.wManagerSetting.CurrentSetting.Save();

        wManager.Wow.Forms.UserControlTabGeneralSettings.ReloadGeneralSettings();

        Logging.Write("[FNV_Quester]: Applied default settings for Kalimdor");

        return;
    }

}


public class Blackspots
{


    public static void StartThread()
    {

        Thread BlackspotValidation = new Thread(() =&gt;
        {
            Logging.Write("[FNV_Quester]: Blackspot validation started");
            Dictionary&lt;Vector3, float&gt; blackspots = new Dictionary&lt;Vector3, float&gt;();
            bool _isStarted = false;

            string profileName = "FNV_V2\\#0 START_FNVLauncher_V2.xml";
            string currentProfileName = wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName;
            int counter = 0;

            if (!_isStarted)
            {
                _isStarted = true;

                while (wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(profileName) &amp;&amp; Conditions.ProductIsStarted)
                {
                    Thread.Sleep(1000);
                }

                while (Conditions.ProductIsStarted || Conditions.ProductInPause)
                {

                    if (wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(profileName))
                        break;

                    if (counter &gt; 300)
                    {
                        Logging.Write("[FNV_Quester]: Clear blacklist of current product session");
                        wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();
                        counter = 0;
                    }

                    if (wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported)
                        wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;

                    if (!wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(currentProfileName))
                    {
                        Logging.Write("[FNV_Quester]: Profile has been changed. Reset current blackspots and save blackspots of loaded profile...");
                        currentProfileName = wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName;

                        blackspots.Clear();

                        foreach (var temp in wManager.wManagerSetting.GetListZoneBlackListed())
                        {
                            blackspots.Add(temp.GetPosition(), temp.Radius);
                        }
                    }

                    if (blackspots.Count &gt; wManager.wManagerSetting.GetListZoneBlackListed().Count)
                    {
                        Logging.Write("[FNV_Quester]: Reaply default blackspots");
                        try
                        {
                            foreach (var temp in blackspots)
                            {
                                if (!wManager.wManagerSetting.GetListZoneBlackListed().Any(b =&gt; b.GetPosition().DistanceTo(temp.Key) &lt; 0.1 &amp;&amp; System.Math.Abs(b.Radius - temp.Value) &lt; 0.1))
                                    wManager.wManagerSetting.AddBlackListZone(temp.Key, temp.Value, true);
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }

                    if (wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(profileName))
                        break;

                    counter++;
                    Thread.Sleep(1000);
                }
            }
            _isStarted = false;
            Logging.Write("[FNV_Quester]: Blackspot validation disposed");

        });

        BlackspotValidation.Start();
    }
}

public class BoatSettings
{

    private static int foodAmount = 0;
    private static int drinkAmount = 0;

    public static void StartThread()
    {

        Thread BoatSettingsThread = new Thread(() =&gt;
        {
            Logging.Write("[FNV_Quester]: Boat settings initialized");

            string profileName = "FNV_V2\\#0 START_FNVLauncher_V2.xml";
            string currentProfileName = wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName;


            while ((wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(profileName) || !ObjectManager.Me.GetMove) &amp;&amp; Conditions.ProductIsStarted)
            {
                Thread.Sleep(1000);
            }

            while (Conditions.ProductIsStarted || Conditions.ProductInPause)
            {

                if (wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(profileName))
                    break;

                try
                {

                    foreach (var ele in Logging.List)
                    {

                        string temp = ele.ToString();
                        if (temp.Contains("[Quester] New step (") &amp;&amp; temp.Contains("): BOATShip") &amp;&amp; temp.Contains("&gt;Pulse"))
                        {
                            if (wManager.wManagerSetting.CurrentSetting.Repair || wManager.wManagerSetting.CurrentSetting.Selling)
                            {
                                wManager.wManagerSetting.CurrentSetting.Selling = false;
                                wManager.wManagerSetting.CurrentSetting.Repair = false;
                                Logging.Write("[FNV_Quester]: Currently on boat step and sell / repair is enabled. Going to disable it, to prevent conflicts during boat");
                            }
                            if (wManager.wManagerSetting.CurrentSetting.FoodAmount != 0 || wManager.wManagerSetting.CurrentSetting.DrinkAmount != 0)
                            {
                                foodAmount = wManager.wManagerSetting.CurrentSetting.FoodAmount;
                                drinkAmount = wManager.wManagerSetting.CurrentSetting.DrinkAmount;
                            }
                            wManager.Wow.Forms.UserControlTabGeneralSettings.ReloadGeneralSettings();
                            Logging.List.Clear();
                            break;
                        }
                        else if (temp.Contains("[Quester] New step (") &amp;&amp; !temp.Contains("): BOATShip") &amp;&amp; (temp.Contains("&gt;Pulse") || temp.Contains("&gt;PickUp") || temp.Contains("&gt;TurnIn")))
                        {
                            if (!wManager.wManagerSetting.CurrentSetting.Selling || !wManager.wManagerSetting.CurrentSetting.Repair)
                            {
                                wManager.wManagerSetting.CurrentSetting.Selling = true;
                                wManager.wManagerSetting.CurrentSetting.Repair = true;
                                if (foodAmount != 0)
                                    wManager.wManagerSetting.CurrentSetting.FoodAmount = foodAmount;
                                if (drinkAmount != 0)
                                    wManager.wManagerSetting.CurrentSetting.DrinkAmount = drinkAmount;
                                Logging.Write("[FNV_Quester]: Currently not on boat step and sell / repair is disabled. Going to enable it");
                                wManager.Wow.Forms.UserControlTabGeneralSettings.ReloadGeneralSettings();
                                Logging.List.Clear();
                                break;
                            }
                            Logging.List.Clear();
                        }

                    }
                }
                catch (Exception e)
                {
                }

                Thread.Sleep(30000);
            }

            Logging.Write("[FNV_Quester]: Boat settings disposed");

        });


        BoatSettingsThread.Start();
    }
}

public class ProfileRestarter
{
    private static bool _IsAfk = false;
    private static Vector3 afkPosition = (Vector3)null;
    private static int timer = 0;
    private static bool _isSubscribed = false;
    private static void WatchForEvents()
    {
        if (!_isSubscribed)
        {
            _isSubscribed = true;

            EventsLuaWithArgs.OnEventsLuaWithArgs += (LuaEventsId id, List&lt;string&gt; args) =&gt;
            {
                if (id == wManager.Wow.Enums.LuaEventsId.CHAT_MSG_SYSTEM &amp;&amp; Conditions.ProductIsStartedNotInPause)
                {

                    if (args.FirstOrDefault().Contains("You are now AFK"))
                    {
                        _IsAfk = true;
                        timer = 0;
                        afkPosition = ObjectManager.Me.Position;
                    }
                }
            };
        }
    }

    public static void StartProfileRestarter()
    {

        Thread ProfileRestarterThread = new Thread(() =&gt;
        {
            Logging.Write("[FNV_Quester]: Profile Restarter initialized");

            string profileName = "FNV_V2\\#0 START_FNVLauncher_V2.xml";
            string currentProfileName = wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName;


            while ((wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(profileName) || !ObjectManager.Me.GetMove) &amp;&amp; Conditions.ProductIsStarted)
            {
                Thread.Sleep(1000);
            }

            if (!_isSubscribed)
            {
                Thread.Sleep(Usefuls.Latency + 500);
                WatchForEvents();
            }

            while (Conditions.ProductIsStarted || Conditions.ProductInPause)
            {

                if (wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(profileName))
                    break;

                while (_IsAfk &amp;&amp; timer &lt; 600 &amp;&amp; !wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName.Equals(profileName))
                {
                    Thread.Sleep(1000);
                    timer++;
                }

                if (_IsAfk &amp;&amp; timer &gt;= 600 &amp;&amp; ObjectManager.Me.Position.DistanceTo(afkPosition) &lt; 25)
                {
                    Logging.Write("[FNV_Quester]: Bot seems to stopped / went AFK for too long. Restart of profile in process...");
                    wManager.Wow.Helpers.Quest.QuesterCurrentContext.ProfileName = profileName;
                    robotManager.Products.Products.ProductRestart();
                    break;
                }

                timer = 0;
                _IsAfk = false;
                Thread.Sleep(10000);
            }

            Logging.Write("[FNV_Quester]: Profile Restarter disposed");

        });

        ProfileRestarterThread.Start();
    }
}

public static class AutoUpdater
{

    private static bool hasUpdatedRecently = false;

    private static string[] profileNameList = new string[] {
    "%23MountDwarf",
    "%23MountGnome",
    "%23MountHuman",
    "%23MountNightElf",
    "DruidBearForm",
    "DwarfHunterQuest",
    "MateniaAlliancePoisonRogue",
    "NightElfHunterQuest",
    "WarriorDeffStance",

    "%230 START_FNVLauncher_V2",
    "%231 (1-6) Dwarf%2C Gnome Start",
    "%231 (1-6) Humant Start",
    "%231 (1-6) N811 Start",
    "%232 (6-9) Dun Morogh p1",
    "%233 (9-11) Elwynn",
    "%234 (11-12) Westfall p1",
    "%235 (12-14) Dun Murogh p2%2C Loch Modan p1",
    "%236 (14-16) Auberdine p1",
    "%237 (16-18) Westfall p2",
    "%238 (18-19) Loch Modan p2",
    "%239 (19-20) Auberdine p2",
    "%2310 (20-22) Westfall p3",
    "%2311 (22-23) Lakreshire p1",
    "%2312 (23-24) Darkshire p1",
    "%2312.1 (23-24) Auberdine p3",
    "%2313 (24-26) Stonetalon%2C Ashenvale",
    "%2314 (26-27) Darkshire p2",
    "%2315 (26-28) Wetlands p1",
    "%2316 (27-28) Ashenvale p2",
    "%2317 (28-29) Wetlands p2",
    "%2318 (29-30) Lakeshire p2%2C Darkshire p3",
    "%2319 (31-32) Ashenvale%2C Stonetalon",
    "%2320 (32-33) Darkshire p4",
    "%2321 (32-33) Ashenvale p4",
    "%2322 (33-34) Thousand Needles",
    "%2323 (34-35) Desolace",
    "%2324 (35-36) Hillsbrad Foothills p1",
    "%2325 (36-37) Stranglethorn p1",
    "%2326 (34-35) Stranglethorn p2",
    "%2327 (34-35) Hillsbrad p2%2C Arathi",
    "%2328 (35-36) Stranglethorn p3",
    "%2329 (35-36) Dustwallow",
    "%2330 (36-37) Desolace",
    "%2331 (37-38) Badlands",
    "%2332 (38-39) Swamp of Sorrows",
    "%2333 (39-40) Stranglethorn p4",
    "%2334 (40-41) Alterac Mountain",
    "%2335 (41-42) Additional quests Badlands%2C Dustwallow%2C Swamp of Sorrows",
    "%2336 (42-44) Tanaris",
    "%2337 (44-45) STV",
    "%2338 (45-45) Hinterlands",
    "%2339 (45-46) Feralas_Tanaris additional quests",
    "%2340 (46-47) Hinterlands 2",
    "%2341 (47-48) Azshara p1",
    "%2342 (48-49) Searing Gorge",
    "%2343 (49-50) Blasted Lands",
    "%2344 (50-51) Ungoro Creater p1",
    "%2345 (51-51) Feralas p3",
    "%2346 (52-53) Azshara p2",
    "%2347 (53-54) Felwood p1",
    "%2348 (53-53) Ungoro Creater p2",
    "%2349 (54-55) Winterspring p1"
     };

    private static string[] profileNameListLocal = new string[] {
    "#MountDwarf",
    "#MountGnome",
    "#MountHuman",
    "#MountNightElf",
    "DruidBearForm",
    "DwarfHunterQuest",
    "MateniaAlliancePoisonRogue",
    "NightElfHunterQuest",
    "WarriorDeffStance",

    "#0 START_FNVLauncher_V2",
    "#1 (1-6) Dwarf, Gnome Start",
    "#1 (1-6) Humant Start",
    "#1 (1-6) N811 Start",
    "#2 (6-9) Dun Morogh p1",
    "#3 (9-11) Elwynn",
    "#4 (11-12) Westfall p1",
    "#5 (12-14) Dun Murogh p2, Loch Modan p1",
    "#6 (14-16) Auberdine p1",
    "#7 (16-18) Westfall p2",
    "#8 (18-19) Loch Modan p2",
    "#9 (19-20) Auberdine p2",
    "#10 (20-22) Westfall p3",
    "#11 (22-23) Lakreshire p1",
    "#12 (23-24) Darkshire p1",
    "#12.1 (23-24) Auberdine p3",
    "#13 (24-26) Stonetalon, Ashenvale",
    "#14 (26-27) Darkshire p2",
    "#15 (26-28) Wetlands p1",
    "#16 (27-28) Ashenvale p2",
    "#17 (28-29) Wetlands p2",
    "#18 (29-30) Lakeshire p2, Darkshire p3",
    "#19 (31-32) Ashenvale, Stonetalon",
    "#20 (32-33) Darkshire p4",
    "#21 (32-33) Ashenvale p4",
    "#22 (33-34) Thousand Needles",
    "#23 (34-35) Desolace",
    "#24 (35-36) Hillsbrad Foothills p1",
    "#25 (36-37) Stranglethorn p1",
    "#26 (34-35) Stranglethorn p2",
    "#27 (34-35) Hillsbrad p2, Arathi",
    "#28 (35-36) Stranglethorn p3",
    "#29 (35-36) Dustwallow",
    "#30 (36-37) Desolace",
    "#31 (37-38) Badlands",
    "#32 (38-39) Swamp of Sorrows",
    "#33 (39-40) Stranglethorn p4",
    "#34 (40-41) Alterac Mountain",
    "#35 (41-42) Additional quests Badlands, Dustwallow, Swamp of Sorrows",
    "#36 (42-44) Tanaris",
    "#37 (44-45) STV",
    "#38 (45-45) Hinterlands",
    "#39 (45-46) Feralas_Tanaris additional quests",
    "#40 (46-47) Hinterlands 2",
    "#41 (47-48) Azshara p1",
    "#42 (48-49) Searing Gorge",
    "#43 (49-50) Blasted Lands",
    "#44 (50-51) Ungoro Creater p1",
    "#45 (51-51) Feralas p3",
    "#46 (52-53) Azshara p2",
    "#47 (53-54) Felwood p1",
    "#48 (53-53) Ungoro Creater p2",
    "#49 (54-55) Winterspring p1"
     };

    public static bool Update()
    {
		return true;
        AutoUpdaterSettings.Initialize();

        //Check for updates every 12 hours
        if (AutoUpdaterSettings.CurrentSettings.updatedTime &lt; ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - (43200 * 1000)))
        {
            Logging.Write("[FNV_Quester]: Checking for updates...");

            string changelogOnline = "https://tdefaultorandomb.000webhostapp.com/msxbt/%230%20Changelog.txt";
            string changelogCurrent = System.Windows.Forms.Application.StartupPath + @"\Profiles\Quester\FNV_V2\#0 Changelog.txt";

            try
            {
                var changelogCurrentFileContent = System.IO.File.ReadAllText(changelogCurrent, System.Text.Encoding.UTF8);
                var changelogOnlineFileContent = new System.Net.WebClient { Encoding = System.Text.Encoding.Default }.DownloadString(changelogOnline);

                if (!string.IsNullOrWhiteSpace(changelogCurrentFileContent) &amp;&amp; !string.IsNullOrWhiteSpace(changelogOnlineFileContent))
                {
                    if (changelogCurrentFileContent != changelogOnlineFileContent)
                    {
                        Logging.Write("[FNV_Quester]: Updates found!");
                        System.IO.File.WriteAllText(changelogCurrent, changelogOnlineFileContent);
                    }
                    else
                    {
                        AutoUpdaterSettings.CurrentSettings.updatedTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        AutoUpdaterSettings.CurrentSettings.Save();
                        Logging.Write("[FNV_Quester]: No updates found!");
                        return true;
                    }
                }


            }
            catch (Exception e)
            {

            }


            for (int i = 0; i &lt; profileNameList.Count(); i++)
            {
                try
                {
                    string onlineFile = "https://tdefaultorandomb.000webhostapp.com/msxbt/" + profileNameList[i] + ".xml";
                    string currentFile = System.Windows.Forms.Application.StartupPath + @"\Profiles\Quester\FNV_V2\" + profileNameListLocal[i] + ".xml";

                    var currentFileContent = System.IO.File.ReadAllText(currentFile, System.Text.Encoding.UTF8);
                    var onlineFileContent = new System.Net.WebClient { Encoding = System.Text.Encoding.Default }.DownloadString(onlineFile);

                    if (!string.IsNullOrWhiteSpace(currentFileContent) &amp;&amp; !string.IsNullOrWhiteSpace(onlineFileContent))
                    {
                        if (currentFileContent.Length == 39 &amp;&amp; onlineFileContent.Length == 39) // 39 is size of encrypted files with option "Short file"
                        {
                            if (currentFileContent != onlineFileContent) // if new update
                            {
                                robotManager.Helpful.Logging.Write("[FNV_Quester]: New version found. Updating file " + profileNameListLocal[i]);
                                System.IO.File.WriteAllText(currentFile, onlineFileContent); // replace user file by online file
                                hasUpdatedRecently = true;
                            }
                        }
                    }

                }
                catch (System.Exception e)
                {
                    //robotManager.Helpful.Logging.WriteError("Auto update: " + e);
                }

                Thread.Sleep(100);

            }
        }

        if (hasUpdatedRecently)
        {
            AutoUpdaterSettings.CurrentSettings.updatedTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            AutoUpdaterSettings.CurrentSettings.Save();
            Logging.Write("[FNV_Quester]: Update found and executed. Restarting bot now");
            new System.Threading.Thread(() =&gt; robotManager.Products.Products.ProductRestart()).Start(); // reload product (profile)
        }
        else
        {
            Logging.Write("[FNV_Quester]: No updates found [F]");
        }

        AutoUpdaterSettings.Dispose();
        return true;
    }
}


public class AutoUpdaterSettings : Settings
{
    public static void Initialize()
    {
        AutoUpdaterSettings.Load();
    }

    public static void Dispose()
    {
        AutoUpdaterSettings.CurrentSettings.Save();
        //isLaunched = false;
    }

    public void Settings()
    {
        AutoUpdaterSettings.Load();
        AutoUpdaterSettings.CurrentSettings.ToForm();
        AutoUpdaterSettings.CurrentSettings.Save();
    }

    public AutoUpdaterSettings()
    {
        this.updatedTime = 0;
    }

    public static AutoUpdaterSettings CurrentSettings { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("AutoUpdaterSettings", "FNV_Quester"));
        }
        catch (Exception e)
        {
            Logging.WriteDebug("AutoUpdaterSettings =&gt; Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("AutoUpdaterSettings", "FNV_Quester")))
            {
                CurrentSettings = Load&lt;AutoUpdaterSettings&gt;(AdviserFilePathAndName("AutoUpdaterSettings", "FNV_Quester"));
                return true;
            }

            AutoUpdaterSettings.CurrentSettings = new AutoUpdaterSettings();
        }
        catch (Exception e)
        {
            Logging.WriteDebug("AutoUpdaterSettings =&gt; Load(): " + e);
        }
        return false;
    }

    public double updatedTime { get; set; }

}




class test
{

    private static int foodAmount = 0;
    private static int drinkAmount = 0;

    public static void testen()
    {

        Thread BoatSettings = new Thread(() =&gt;
        {

            foreach (var ele in Logging.List)
            {

                string temp = ele.ToString();
                if (temp.Contains("[Quester] New step (") &amp;&amp; temp.Contains("): BOATShip") &amp;&amp; temp.Contains("&gt;Pulse"))
                {
                    if (wManager.wManagerSetting.CurrentSetting.Repair || wManager.wManagerSetting.CurrentSetting.Selling)
                    {
                        wManager.wManagerSetting.CurrentSetting.Selling = false;
                        wManager.wManagerSetting.CurrentSetting.Repair = false;
                        Logging.Write("[FNV_Quester]: Currently on boat step and sell / repair is enabled. Going to disable it, to prevent conflicts during boat");
                    }
                    if (wManager.wManagerSetting.CurrentSetting.FoodAmount != 0 || wManager.wManagerSetting.CurrentSetting.DrinkAmount != 0)
                    {
                        foodAmount = wManager.wManagerSetting.CurrentSetting.FoodAmount;
                        drinkAmount = wManager.wManagerSetting.CurrentSetting.DrinkAmount;
                    }
                    Logging.List.Clear();
                    break;
                }
                else if (temp.Contains("[Quester] New step (") &amp;&amp; !temp.Contains("): BOATShip") &amp;&amp; (temp.Contains("&gt;Pulse") || temp.Contains("&gt;PickUp") || temp.Contains("&gt;TurnIn")))
                {
                    if (!wManager.wManagerSetting.CurrentSetting.Selling || !wManager.wManagerSetting.CurrentSetting.Repair)
                    {
                        wManager.wManagerSetting.CurrentSetting.Selling = true;
                        wManager.wManagerSetting.CurrentSetting.Repair = true;
                        if (foodAmount != 0)
                            wManager.wManagerSetting.CurrentSetting.FoodAmount = foodAmount;
                        if (drinkAmount != 0)
                            wManager.wManagerSetting.CurrentSetting.DrinkAmount = drinkAmount;
                        Logging.Write("[FNV_Quester]: Currently not on boat step and sell / repair is disabled. Going to enable it");
                        Logging.List.Clear();
                        break;
                    }
                    Logging.List.Clear();
                }

            }

        });





    }
}
