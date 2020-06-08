using robotManager.Helpful;
using wManager.Wow.Class;

public sealed class ReportToGryanStourmantle : QuestClass
{
    public ReportToGryanStourmantle()
    {
        QuestId.Add(109);
        wManager.Wow.ObjectManager.ObjectManager.Me.PlayerRace.Equals("Human");
    }
}

public sealed class PoorOldBlanchy : QuestGathererClass
{
    public PoorOldBlanchy()
    {
        QuestId.Add(151);

        Step.AddRange(new[] { 8, 0, 0, 0 });
        EntryIdObjects.Add(2724);
        HotSpots.Add(new Vector3(-9875, 973, 31));
        HotSpots.Add(new Vector3(-9790, 990, 29));
        HotSpots.Add(new Vector3(-10128, 1055, 36));
        HotSpots.Add(new Vector3(-10123, 1058, 36));
        HotSpots.Add(new Vector3(-9940, 1230, 42));
        
    }
}

public sealed class WestfallStew : QuestClass
{
    public WestfallStew()
    {
        QuestId.Add(36);
    }
}

public sealed class GoretuskLiverPie : QuestGrinderClass
{
    public GoretuskLiverPie()
    {
        QuestId.Add(22);
        Step.AddRange(new[] { 8, 0, 0, 0, });

        HotSpots.Add(new Vector3(-10155, 910, 39));
        HotSpots.Add(new Vector3(-1099, 597, 36));
        HotSpots.Add(new Vector3(-10016, 1118, 42));
        HotSpots.Add(new Vector3(-9945, 1218, 42));
        HotSpots.Add(new Vector3(-10082, 1250, 39));
        EntryTarget.Add(454);

    }
}

public sealed class KillingFields : QuestGrinderClass
{
    public KillingFields()
    {
        QuestId.Add(9);
        Step.AddRange(new[] { 8, 0, 0, 0, });

        HotSpots.Add(new Vector3(-10128, 1055, 36));
        HotSpots.Add(new Vector3(-10123, 1058, 36));
        HotSpots.Add(new Vector3(-9940, 1230, 42));
        EntryTarget.Add(114);

    }
}

public sealed class TheForgottenHeirloom : QuestGathererClass
{
    public TheForgottenHeirloom()
    {
        QuestId.Add(64);

        Step.AddRange(new[] { 1, 0, 0, 0 });
        EntryIdObjects.Add(290);
        HotSpots.Add(new Vector3(-9849, 1285, 41));

    }
}

public sealed class ThePeoplesMilitaOne : QuestGrinderClass
{
    public ThePeoplesMilitaOne()
    {
        QuestId.Add(12);
        Step.AddRange(new[] { 15, 0, 0, 0, });

        HotSpots.Add(new Vector3(-10128, 1055, 36));
        HotSpots.Add(new Vector3(-10123, 1058, 36));
        HotSpots.Add(new Vector3(-9940, 1230, 42));
        EntryTarget.Add(114);

    }
}

public sealed class TheDefiasBrotherhoodOne : QuestClass
{
    public TheDefiasBrotherhoodOne()
    {
        QuestId.Add(65);
    }
}