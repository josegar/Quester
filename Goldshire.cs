using robotManager.Helpful;
using System.Threading;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Class;
using wManager.Wow.Helpers;

public sealed class AFishyPeril : QuestClass
{
    public AFishyPeril()
    {
        QuestId.Add(40);
    }
}

public sealed class GoldDustExchange : QuestGathererClass
{    // https://classic.wowhead.com/quest=47/gold-dust-exchange
    public GoldDustExchange()

    {
        QuestId.Add(47);
        Step.AddRange(new[] { 10, 0, 0, 0, });

        HotSpots.Add(new Vector3(-9803, 116, 5.5));
        HotSpots.Add(new Vector3(-9877, 220, 14));

        EntryIdObjects.Add(773);

    }
}

public sealed class KoboldCandles : QuestGathererClass
{    // https://classic.wowhead.com/quest=60/kobold-candles
    public KoboldCandles()

    {
        QuestId.Add(60);
        Step.AddRange(new[] { 8, 0, 0, 0, });

        HotSpots.Add(new Vector3(-9803, 116, 5.5));
        HotSpots.Add(new Vector3(-9877, 220, 14));
        EntryIdObjects.Add(772);

    }
}

public sealed class TheFargodeepMine : QuestClass
{	// https://classic.wowhead.com/quest=62/the-fargodeep-mine
    public TheFargodeepMine()

    {
        QuestId.Add(62);

        Step.AddRange(new[] { 1, 0, 0, 0 });
        
    }

    private bool _step1;

    public override bool Pulse() 
    {
       
            if (!_step1 && GoToTask.ToPosition(new Vector3(-9803, 116, 5.5)))
            {  
            Thread.Sleep(3000);
            _step1 = true;
            }
        return true;
    }
}

public sealed class ShipmentToStormwind : QuestClass
{
    public ShipmentToStormwind()
    {
        QuestId.Add(61);
    }
}

public sealed class WestbrookGarrisonNeedsHelp : QuestClass
{
    public WestbrookGarrisonNeedsHelp()
    {
        QuestId.Add(239);
    }
}

public sealed class ReportToGryanStoutmantle : QuestClass
{
    public ReportToGryanStoutmantle()
    {
        QuestId.Add(109);
    }
}

public sealed class RiverpawGnollBounty : QuestGrinderClass
{
    public RiverpawGnollBounty()
    {
        QuestId.Add(11);
        Step.AddRange(new[] { 8, 0, 0, 0, });

        HotSpots.Add(new Vector3(-9826, 621, 41));
        HotSpots.Add(new Vector3(-9940, 583, 38));
        HotSpots.Add(new Vector3(-9818, 564, 38));
        EntryTarget.Add(478);

    }
}