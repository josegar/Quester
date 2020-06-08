using robotManager.Helpful;
using System.Threading;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Class;

public sealed class BlackrockBounty : QuestGrinderClass
{
    public BlackrockBounty()
    {
        QuestId.Add(128);

        Step.AddRange(new[] { 10, 0, 0, 0 });
        HotSpots.Add(new Vector3(-8685, -2345, 157));
        HotSpots.Add(new Vector3(-9401, -3025, 136));
        HotSpots.Add(new Vector3(-9389, -3025, 136));

        EntryTarget.Add(435);
    }
}

public sealed class BlackrockMenace : QuestGrinderClass
{
    public BlackrockMenace()
    {
        QuestId.Add(20);

        Step.AddRange(new[] { 10, 0, 0, 0 });
        HotSpots.Add(new Vector3(-6900, -1339, 219));
        HotSpots.Add(new Vector3(-9401, -3025, 136));
        HotSpots.Add(new Vector3(-9389, -3025, 136));

        EntryTarget.Add(437);
    }
}

public sealed class SellingFish : QuestGrinderClass
{
    public SellingFish()
    {
        QuestId.Add(127);

        Step.AddRange(new[] { 10, 0, 0, 0 });
        HotSpots.Add(new Vector3(-9213.8, -2424.1, 59.6));
        HotSpots.Add(new Vector3(-9300, -2420, 53));

        EntryTarget.AddRange(new[] { 578,  422});

    }
}

public sealed class MurlocPoachers : QuestGrinderClass
{
    public MurlocPoachers()
    {
        QuestId.Add(150);

        Step.AddRange(new[] { 8, 0, 0, 0 });
        HotSpots.Add(new Vector3(-9213.8, -2424.1, 59.6));
        HotSpots.Add(new Vector3(-9300, -2420, 53));

        EntryTarget.AddRange(new[] { 578, 422 });


    }
}

public sealed class WantedGathIlzogg : QuestGrinderClass
{
    public WantedGathIlzogg()
    {
        QuestId.Add(169);

        Step.AddRange(new[] { 1, 0, 0, 0 });
        HotSpots.Add(new Vector3(-9382, -3082, 158));
      
        EntryTarget.Add(334);
    }
}

public sealed class SolomonsLaw : QuestGrinderClass
{
    public SolomonsLaw()
    {
        QuestId.Add(91);

        Step.AddRange(new[] { 10, 0, 0, 0 });
        HotSpots.Add(new Vector3(-9165, -3179, 103));
        HotSpots.Add(new Vector3(-9107, -3241, 101));
        HotSpots.Add(new Vector3(-9389, -3025, 136));

        EntryTarget.Add(433);
    }
}

public sealed class WantedLieutenantFangore : QuestGrinderClass
{
    public WantedLieutenantFangore()
    {
        QuestId.Add(180);

        Step.AddRange(new[] { 1, 0, 0, 0 });
        HotSpots.Add(new Vector3(-9111, -3311, 102.4));


        EntryTarget.Add(703);
    }
}

public sealed class AnUnwelcomeGuest : QuestGrinderClass
{
    public AnUnwelcomeGuest()
    {
        QuestId.Add(34);

        Step.AddRange(new[] { 10, 0, 0, 0 });
        HotSpots.Add(new Vector3(-9289, -1911, 74));
        EntryTarget.Add(345);
    }
}

public sealed class HilarysNecklace : QuestGathererClass
{
    public HilarysNecklace()
    {
        QuestId.Add(3741);
        Step.AddRange(new[] { 1, 0, 0, 0 });
        HotSpots.Add(new Vector3(-9324, -1986, 43));
        HotSpots.Add(new Vector3(-9351, -2298, 71));

        EntryIdObjects.Add(154357);
    }
}