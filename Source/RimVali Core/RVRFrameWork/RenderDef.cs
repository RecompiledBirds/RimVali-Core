using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimValiCore.RVR
{
    #region texture types

    public class BaseTex
    {
        public string tex;
        public string femaleTex;

        public List<string> alternateMaskPaths = new List<string>();
        public List<string> alternateFemaleMaskPaths = new List<string>();

        public bool ProvidesAlternateMask(Pawn pawn)
        {
            return pawn.gender == Gender.Female ? HasFemaleAlternateMasks : HasAlternateMasks;
        }

        public bool HasAlternateMasks
        {
            get
            {
                return alternateMaskPaths.Count > 0;
            }
        }

        public bool HasFemaleAlternateMasks
        {
            get
            {
                return alternateFemaleMaskPaths.Count > 0;
            }
        }

        public List<string> GetMasks(Pawn pawn)
        {
            if (pawn.gender == Gender.Female && HasFemaleAlternateMasks)
                return alternateFemaleMaskPaths;
            else if (HasAlternateMasks)
                return alternateMaskPaths;
            return new List<string>();
        }

        public virtual bool CanApply(Pawn p)
        {
            return true;
        }
    }

    public class HediffTex : BaseTex
    {
        public HediffDef hediff;
        public override bool CanApply(Pawn p)
        {
            return p.health.hediffSet.HasHediff(hediff);
        }
    }

    public class BackstoryTex : BaseTex
    {
        public string backstoryTitle;

        public override bool CanApply(Pawn p)
        {
            return p.story.adulthood.identifier == backstoryTitle || p.story.childhood.identifier == backstoryTitle;
        }
    }

    public class HediffStoryTex : BaseTex
    {
        public string backstoryTitle;
        public HediffDef hediffDef;
        public override bool CanApply(Pawn p)
        {
            return (p.story.adulthood.identifier == backstoryTitle || p.story.childhood.identifier == backstoryTitle) && p.health.hediffSet.HasHediff(hediffDef);
        }
    }

    #endregion texture types

    public class BodyPartGraphicPos
    {
        public Vector2 position = new Vector2(0f, 0f);
        public float layer = 1f;
        public Vector2 size = new Vector2(1f, 1f);
    }

    public class RenderableDef : Def
    {
        public Graphic graphic;
        public RenderableDef linkMaskIndexWith;
        #region backstory checks

        public bool StoryIsName(Backstory story, string title)
        {
            //I have to check if everything is null so we get this mess, otherwise sometimes a null reference exception occurs.
            //There probably is a cleaner way of doing this I'm not aware of.
            return ((story.untranslatedTitle != null && story.untranslatedTitle == title)
                        || ((story.untranslatedTitle != null && story.untranslatedTitle == title)
                        || (story.untranslatedTitleShort != null && story.untranslatedTitleShort == title)
                        || (story.untranslatedTitleFemale != null && story.untranslatedTitleFemale == title)
                        //This does not need to be checked, as it literally cannot ever be null.
                        || story.identifier == title
                        || (story.titleShort != null && story.titleShort == title)
                        || (story.titleFemale != null && story.titleFemale == title)
                        || (story.titleShortFemale != null && story.titleShortFemale == title
                        //Same here.
                        || story.title == title)));
            //Now we hope Tynan never changes backstories. Ever. Or else this thing breaks.
        }

        #endregion backstory checks

        #region get index

        public int GetMyIndex(Pawn pawn)
        {
            if (pawn.def is RimValiRaceDef rDef)
            {
                try
                {
                    ColorComp comp = pawn.TryGetComp<ColorComp>();
                    foreach (string str in comp.renderableDefIndexes.Keys)
                    {
                        if (str == defName || (linkIndexWithDef != null && linkIndexWithDef.defName == str))
                        {
                            return comp.renderableDefIndexes[str];
                        }
                    }
                }catch(Exception e){
                    Log.Error($"[RVR]: Error finding index of {defName} for {pawn.Name}! Regenerating graphics...");
                    Log.Error($"[RVR]: Caught error: {e}");
                    Log.Message($"[RVR]: ColorComp is on pawn: {pawn.TryGetComp<ColorComp>()!=null}");
                    rDef.GenGraphics(pawn);
                }
            }
            return 0;
        }

        public int GetMyMaskIndex(Pawn pawn)
        {
            if (pawn.def is RimValiRaceDef)
            {
                ColorComp comp = pawn.TryGetComp<ColorComp>();
                foreach (string str in comp.renderableDefMaskIndexes.Keys)
                {
                    if (str == defName || (linkMaskIndexWith != null && linkMaskIndexWith.defName == str))
                    {
                        return comp.renderableDefMaskIndexes[str];
                    }
                }
            }
            return 0;
        }

        #endregion get index

        #region get texture

        public string TexPath(Pawn pawn)
        {
            return TexPath(pawn, GetMyIndex(pawn));
        }
       
        public string MaskPath(Pawn pawn, int index)
        {
            string path = null;
            BaseTex tex = textures[index];
            if (tex.HasAlternateMasks)
            {

            }
            if (tex.HasFemaleAlternateMasks)
            {

            }
            return path;
        }

        public BaseTex GetCurrentTexture(Pawn pawn, int index)
        {
            BaseTex tex = textures[index];
            //HediffStory gets highest priority here, by being lowest on this set
            Backstory adulthood = null;
            if (pawn.story.adulthood != null)
            {
                adulthood = pawn.story.adulthood;
            }
            Backstory childhood = pawn.story.childhood;
            foreach (BackstoryTex backstoryTex in backstoryTextures)
            {
                //Log.Message(backstoryTex.backstoryTitle);
                if ((adulthood != null && StoryIsName(adulthood, backstoryTex.backstoryTitle)) || StoryIsName(childhood, backstoryTex.backstoryTitle))
                {
                    tex = backstoryTex != null && pawn.gender == Gender.Female ? backstoryTex : backstoryTex;
                }
            }
            foreach (HediffTex hediffTex in hediffTextures)
            {
                foreach (BodyPartRecord bodyPartRecord in pawn.def.race.body.AllParts)
                {
                    BodyPartDef def = bodyPartRecord.def;
                    if (def.defName.ToLower() == bodyPart.ToLower() || def.label.ToLower() == bodyPart.ToLower() && pawn.health.hediffSet.HasHediff(hediffTex.hediff, bodyPartRecord, false))
                    {
                        tex = hediffTex != null && pawn.gender == Gender.Female ? hediffTex : hediffTex;
                    }
                }
            }

            foreach (HediffStoryTex hediffStoryTex in hediffStoryTextures)
            {
                if ((adulthood != null && StoryIsName(adulthood, hediffStoryTex.backstoryTitle)) || StoryIsName(childhood, hediffStoryTex.backstoryTitle))
                {
                    foreach (BodyPartRecord bodyPartRecord in pawn.def.race.body.AllParts)
                    {
                        BodyPartDef def = bodyPartRecord.def;
                        if ((def.defName.ToLower() == bodyPart.ToLower() || def.label.ToLower() == bodyPart.ToLower())
                            && (pawn.health.hediffSet.HasHediff(hediffStoryTex.hediffDef, bodyPartRecord, false)))
                        {
                            tex = hediffStoryTex != null && pawn.gender == Gender.Female ? hediffStoryTex : hediffStoryTex;
                        }
                    }
                }
            }
            return tex;
        }

        public string TexPath(Pawn pawn, int index)
        {
            BaseTex tex = GetCurrentTexture(pawn, index);
            string path = pawn.gender == Gender.Female && tex.femaleTex!=null? tex.femaleTex : tex.tex;

            if (pawn.Dead)
            {
                if (pawn.GetRotStage().HasFlag(RotStage.Dessicated) && dessicatedTex != null)
                {
                    path = dessicatedTex;
                }
                if (pawn.GetRotStage().HasFlag(RotStage.Rotting) && rottingTex != null)
                {
                    path = rottingTex;
                }

            }
            return path;
        }

        #endregion get texture

        public List<BaseTex> GetTexList()
        {
            List<BaseTex> texture = new List<BaseTex>();
            texture.AddRange(textures);
            texture.AddRange(hediffTextures);
            texture.AddRange(backstoryTextures);
            texture.AddRange(hediffStoryTextures);
            return texture;
        }

        public List<string> FindAllTextures()
        {
            List<string> paths = new List<string>();
            foreach (BaseTex tex in GetTexList())
            {
                paths.Add(tex.tex);
                if (tex.femaleTex != null)
                {
                    paths.Add(tex.femaleTex);
                }
            }
            return paths;
        }

        public List<BaseTex> textures;
        public string rottingTex;
        public string dessicatedTex;

        public string bodyPart = null;

        public RenderableDef linkIndexWithDef;

        public bool showsInBed = true;
        public bool showsIfDessicated = true;
        public bool showsIfRotted = true;
        public string useColorSet;
        public BodyPartGraphicPos east = new BodyPartGraphicPos();
        public BodyPartGraphicPos north = new BodyPartGraphicPos();
        public BodyPartGraphicPos south = new BodyPartGraphicPos();
        public BodyPartGraphicPos west;

        public List<BackstoryTex> backstoryTextures = new List<BackstoryTex>();
        public List<HediffTex> hediffTextures = new List<HediffTex>();
        public List<HediffStoryTex> hediffStoryTextures = new List<HediffStoryTex>();

        #region portrait check

        public bool CanShowPortrait(Pawn pawn)
        {
            if (bodyPart == null)
            {
                return true;
            }
            IEnumerable<BodyPartRecord> bodyParts = pawn.health.hediffSet.GetNotMissingParts();
            //Log.Message(bodyParts.Any(x => x.def.defName.ToLower() == "left lower ear" || x.untranslatedCustomLabel.ToLower() == "left lower ear".ToLower()).ToString());
            try
            {
                if (bodyParts.Any(x => x.def.defName.ToLower() == bodyPart.ToLower() || x.Label.ToLower() == bodyPart.ToLower()))
                {
                    if (!pawn.Spawned)
                    {
                        return true;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                //Log.Message(e.ToString(), true);
                return true;
            }
        }

        #endregion portrait check

        #region general show check

        public bool CanShow(Pawn pawn, RotDrawMode mode, bool portrait = false)
        {
            if ((mode == RotDrawMode.Rotting && !showsIfRotted) || (mode == RotDrawMode.Dessicated && !showsIfDessicated))
            {
                return false;
            }
            return CanShow(pawn, portrait);
        }

        public bool CanShow(Pawn pawn, bool portrait = false)
        {
            IEnumerable<BodyPartRecord> bodyParts = pawn.health.hediffSet.GetNotMissingParts();
            bool bodyIsHiding = bodyPart == null || bodyParts.Any(x => x.def.defName.ToLower() == bodyPart.ToLower() || x.Label.ToLower() == bodyPart.ToLower());
            return !portrait ? (!pawn.InBed() || (pawn.CurrentBed().def.building.bed_showSleeperBody)  ||showsInBed) && bodyIsHiding : bodyIsHiding ;
        }

        #endregion general show check
    }
}