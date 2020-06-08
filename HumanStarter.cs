using robotManager.Helpful;
using wManager.Wow.Class;
using wManager.Wow.ObjectManager;

public sealed class AThreatWithin : QuestClass
{
    public AThreatWithin()
    {
        QuestId.Add(783);
    }

    public override bool CanConditions()
    {
        if (ObjectManager.Me.Level > 8)
            return false;
        return base.CanConditions();
    }
}

public sealed class KoboldCampCleanup : QuestGrinderClass
{
    public KoboldCampCleanup()
    {
        QuestId.Add(7);
        Step.AddRange(new[] { 8, 0, 0, 0, });

        HotSpots.Add(new Vector3(-8763, -128, 83));

        EntryTarget.Add(6);

    }

    public override bool CanConditions()
    {
        if (ObjectManager.Me.Level > 8)
            return false;
        return base.CanConditions();
    }
}

public sealed class InvestigateEchoRidge : QuestGrinderClass
{
    public InvestigateEchoRidge()
    {
        QuestId.Add(15);
        Step.AddRange(new[] { 8, 0, 0, 0, });

        HotSpots.Add(new Vector3(-8763, -128, 83));

        EntryTarget.Add(257);

    }

    public override bool CanConditions()
    {
        if (ObjectManager.Me.Level > 8)
            return false;
        return base.CanConditions();
    }
}

public sealed class SkirmishAtEchoRidge : QuestGrinderClass
{
    public SkirmishAtEchoRidge()
    {
        QuestId.Add(21);
        Step.AddRange(new[] { 8, 0, 0, 0, });

        HotSpots.Add(new Vector3(-8657, -124, 90.9));

        EntryTarget.Add(80);

    }

    public override bool CanConditions()
    {
        if (ObjectManager.Me.Level > 8)
            return false;
        return base.CanConditions();
    }
}

public sealed class ReportToGoldshire : QuestClass
{
    public ReportToGoldshire()
    {
        QuestId.Add(54);
    }

    public override bool CanConditions()
    {
        if (ObjectManager.Me.Level > 8)
            return false;
        return base.CanConditions();
    }
}

public sealed class RestAndRelaxion : QuestClass
{
    public RestAndRelaxion()
    {
        QuestId.Add(2158);
    }
}

public sealed class EaganPeltskinner : QuestClass
{
    public EaganPeltskinner()
    {
        QuestId.Add(5261);
    }
}

public sealed class WolvesAcrossTheBorder : QuestGrinderClass
{
    public WolvesAcrossTheBorder()
    {
        QuestId.Add(33);
        Step.AddRange(new[] { 8, 0, 0, 0, });

        HotSpots.Add(new Vector3(-8865, -87, 82));

        EntryTarget.Add(299);

    }

    public override bool CanConditions()
    {
        if (ObjectManager.Me.Level > 8)
            return false;
        return base.CanConditions();
    }
}

// public sealed class GrindTo6 : QuestGrinderClass
//{
//    public GrindTo6()
//    {
//        QuestId.Add(21);
//        Step.AddRange(new[] { 8, 0, 0, 0, });

//       HotSpots.Add(new Vector3(-8657, -124, 90.9));

//        EntryTarget.Add(80);

//    }

//    public override bool CanConditions()
//    {
//        if (ObjectManager.Me.Level > 8)
//            return false;
//        return base.CanConditions();
//    }
//}