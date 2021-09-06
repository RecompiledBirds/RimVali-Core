using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimValiCore.RVR
{
    public class FacRelation
    {
        public FactionDef otherFaction;
        public int relation;
    }

    public class FactionStartRelationDef : Def
    {
        public FactionDef faction;
        public List<FacRelation> relations;
    }

    public class FactionResearchRestrictionDef : Def
    {
        public List<FactionResearchRestriction> factionResearchRestrictions = new List<FactionResearchRestriction>();
        public List<FactionResearchRestriction> factionResearchRestrictionBlackList = new List<FactionResearchRestriction>();
    }

    public class FactionResearchRestriction
    {
        public ResearchProjectDef researchProj;
        public FactionDef factionDef;
        public bool isHackable = false;
    }
}