using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiCore.QLine
{
    public class QL_Quest : Def
    {
        public Type questAction;
        public bool repeatable;
        public string texPath;
        public Texture2D texture;

        public QuestWorker QuestWorker => (QuestWorker)Activator.CreateInstance(questAction, this);

        public override void PostLoad()
        {
            if(texPath != null)
                texture = QuestBackgroundHandler.GetTexture(texPath);
            Log.Message($"PostLoading quest with defName: {defName}");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (questAction != null && !questAction.IsSubclassOf(typeof(QuestWorker)))
            {
                yield return $"{QuestWorker} does not inherit from QuestWorker!";
            }

            if (questAction == null)
            {
                yield return $"Parameter \"questAction\" is null! defName: {defName}";
            }

            foreach (string str in base.ConfigErrors())
            {
                yield return str;
            }
        }
    }

    public abstract class QuestWorker : IExposable
    {
        private HashSet<int> completedStages = new HashSet<int>();
        private List<QuestStage> stages;

        private int curStage;
        private QL_Quest def;

        public int CurrentStage => curStage;

        public QuestWorker(QL_Quest def) 
        { 
            this.def = def;
        }

        /// <summary>
        ///     Creates a list of <see cref="QuestStage"/>s to be used and saved in this worker
        /// </summary>
        /// <returns>a list of <see cref="QuestStage"/>s</returns>
        protected abstract List<QuestStage> CreateStages();

        public List<QuestStage> Stages => stages ?? (stages = CreateStages());

        public void ChangeStage(int amount)
        {
            CompleteStage(curStage);

            int value = amount + curStage;
            Mathf.Clamp(value, 0, Stages.Count - 1);
            curStage = value;
        }

        /// <summary>
        ///     Sets the given <paramref name="stage"/> as completed
        /// </summary>
        /// <param name="stage">The <see cref="QuestStage"/> to be set as completed</param>
        /// <returns>true if the stage wasn't set as completed before, false otherwise</returns>
        public bool CompleteStage(QuestStage stage) => CompleteStage(IndexOfStage(stage));


        /// <summary>
        ///     Sets the <see cref="QuestStage"/> at the given <paramref name="stageIndex"/> as completed
        /// </summary>
        /// <param name="stageIndex">The index of the <see cref="QuestStage"/> to be set as completed</param>
        /// <returns>true if the stage wasn't set as completed before, false otherwise</returns>
        public bool CompleteStage(int stageIndex) => completedStages.Add(stageIndex);

        /// <summary>
        ///     Checks if the <see cref="QuestStage"/> is set as completed
        /// </summary>
        /// <param name="stage">the given <see cref="QuestStage"/></param>
        /// <returns>true, if the <see cref="QuestStage"/> has been set as completed, false otherwise</returns>
        public bool IsStageCompleted(QuestStage stage) => IsStageCompleted(IndexOfStage(stage));

        /// <summary>
        ///     Checks if the given <see cref="QuestStage"/> is <see cref="IsStageCompleted(QuestStage)"/> or if it's <see cref="IndexOfStage(QuestStage)"/> == <see cref="curStage"/>
        /// </summary>
        /// <param name="stage">the <see cref="QuestStage"/> to be checked with</param>
        /// <returns>true if the <see cref="QuestStage"/> is completed or the current stage, false otherwise</returns>
        public bool IsStageCompletedOrCurrent(QuestStage stage) => IsStageCompleted(stage) || curStage == IndexOfStage(stage);

        /// <summary>
        ///     Checks if the <see cref="QuestStage"/> at the given <paramref name="stageIndex"/> is set as completed
        /// </summary>
        /// <param name="stageIndex">the given index as <see cref="int"/></param>
        /// <returns>true, if the <see cref="QuestStage"/> has been set as completed, false otherwise</returns>
        public bool IsStageCompleted(int stageIndex) => completedStages.Contains(stageIndex);

        /// <summary>
        ///     Shortcut of <see cref="List{T}.IndexOf"/> for <see cref="Stages"/>
        /// </summary>
        /// <param name="stage">the <see cref="QuestStage"/> the index is to be found of</param>
        /// <returns>-1 if the <see cref="QuestStage"/> is not part of this worker, it's index otherwise</returns>
        public int IndexOfStage(QuestStage stage) => Stages.IndexOf(stage);

        /// <summary>
        ///     Increments the <see cref="curStage"/> by 1
        /// </summary>
        public void IncrementStage() => ChangeStage(1);

        /// <summary>
        ///     Marks this quest as finished and the current stage as completed
        /// </summary>
        public void FinishQuest()
        {
            Quest_Tracker tracker = Find.World.GetComponent<Quest_Tracker>();

            CompleteStage(curStage);
            try
            {
                if (tracker.Quests.First(quest => quest.Worker == this) is QLine qline)
                {
                    tracker.FinishQuest(qline);
                    Messages.Message($"##Quest has been finished: {qline.Quest.LabelCap}!", MessageTypeDefOf.TaskCompletion);
                }
            }
            catch
            {
                Log.Error("Quest not tracked?");
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref completedStages, nameof(completedStages), LookMode.Value);
            Scribe_Values.Look(ref curStage, nameof(curStage));
            Scribe_Defs.Look(ref def, nameof(def));
        }
    }
}
