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
        private QuestWorker qWorker;
        public bool repeatable;
        public string texPath;
        public Texture2D texture;
        public QuestWorker QuestWorker
        {
            get
            {
                if (qWorker == null && questAction != null)
                {
                    qWorker = (QuestWorker)Activator.CreateInstance(questAction, this);
                }

                if (qWorker == null)
                {
                    string errorString = $"qWorker for defName: {defName} is null after request!";
                    Log.ErrorOnce(errorString, errorString.GetHashCode());
                }

                return qWorker;
            }
        }

        public override void PostLoad()
        {
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

        private int curStage;
        private QL_Quest def;

        public int CurrentStage => curStage;

        public abstract List<QuestStage> Stages();

        public QuestWorker(QL_Quest def) 
        { 
            this.def = def;
        }

        public void ChangeStage(int amount)
        {
            int value = amount + curStage;
            Mathf.Clamp(value, 0, Stages().Count - 1);
            curStage = value;
        }

        public void IncrementStage() => ChangeStage(1);

        public void ExposeData()
        {
            Scribe_Values.Look(ref curStage, nameof(curStage));
            Scribe_Defs.Look(ref def, nameof(def));
        }
    }
}
