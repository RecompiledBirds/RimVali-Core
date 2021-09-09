using Verse;

namespace RimValiCore.Cloning
{
    public class StorageCompProps : CompProperties
    {
    }

    public class StorageComp : ThingComp
    {
        public Pawn p2;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref p2, "storedPawn");
            base.PostExposeData();
        }

        public void GetPersonalityFromPawn(Pawn pawn)
        {
            p2 = pawn;
        }

        public void ClonePersonalityToPawn(ref Pawn pawn)
        {
            pawn.story.adulthood = p2.story.adulthood;
            pawn.story.childhood = p2.story.childhood;

            pawn.story.traits = p2.story.traits;

            pawn.story.title = p2.story.title;

            pawn.relations = p2.relations;

            pawn.royalty = p2.royalty;

            pawn.skills = p2.skills;

            pawn.Name = p2.Name;
        }
    }
}