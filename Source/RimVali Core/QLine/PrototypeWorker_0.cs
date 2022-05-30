using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiCore.QLine
{
    public class PrototypeWorker_0 : QuestWorker
    {
        public PrototypeWorker_0(QL_Quest def) : base(def) { }

        protected override List<QuestStage> CreateStages() => new List<QuestStage>
        {
            new QuestStage
            {
                label = "TestStage_0",
                description = "TestDescription_0",
                buttons = new List<QuestStageButtonDecision>
                {
                    new QuestStageButtonDecision("DoSomething_0", () => Messages.Message("PressedButton_0", MessageTypeDefOf.NeutralEvent, false), new List<DisableReason>
                    {
                        new DisableReason(() => true, () => "Does NOT have a lot of garbage lying around"),
                        new DisableReason(() => false, () => "Does NOT have Nesi killed"),
                        new DisableReason(() => false, () => "Does have a good friend"),
                        new DisableReason(() => true, () => "Does NOT pretend to be gay for financial benefits"),
                        new DisableReason(() => true, () => "Does NOT say <color=red>UwU</color> occasionally when pretending to be gay"),
                    }),

                    new QuestStageButtonDecision("GoToStage_1", () => 
                    {
                        IncrementStage();
                    }, new List<DisableReason>
                    {
                        new DisableReason(() => true, () => "Does want to see a single reason that disables this button"),
                    })
                }
            },

            new QuestStage
            {
                label = "TestStage_1",
                description = "TestDescription_1",
                buttons = new List<QuestStageButtonDecision>
                {
                    new QuestStageButtonDecision("DoSomething_0", () => Messages.Message("PressedButton_0", MessageTypeDefOf.NeutralEvent, false)),

                    new QuestStageButtonDecision("DoSomething_1",() => Messages.Message("PressedButton_1", MessageTypeDefOf.NeutralEvent, false))
                }
            }
        };
    }

    public class PrototypeWorker_1 : QuestWorker
    {
        public PrototypeWorker_1(QL_Quest def) : base(def) { }

        protected override List<QuestStage> CreateStages() => new List<QuestStage>
        {
            new QuestStage
            {
                label = "TestStage_0",
                description = "TestDescription_0",
                buttons = new List<QuestStageButtonDecision>
                {
                    new QuestStageButtonDecision ("DoSomething_0", () => Messages.Message("PressedButton_0", MessageTypeDefOf.NeutralEvent, false)),

                    new QuestStageButtonDecision("GoToStage_1", () =>
                    {
                        IncrementStage();
                    })
                }
            },

            new QuestStage
            {
                label = "TestStage_1",
                description = "TestDescription_1",
                buttons = new List<QuestStageButtonDecision>
                {
                    new QuestStageButtonDecision("DoSomething_0", () => Messages.Message("PressedButton_0", MessageTypeDefOf.NeutralEvent, false)),

                    new QuestStageButtonDecision("DoSomething_1",() => Messages.Message("PressedButton_1", MessageTypeDefOf.NeutralEvent, false)),

                    new QuestStageButtonDecision("GoToStage_2", () =>
                    {
                        IncrementStage();
                    })
                }
            },

            new QuestStage
            {
                label = "TestStage_2",
                description = "TestDescription_2",
                buttons = new List<QuestStageButtonDecision>
                {
                    new QuestStageButtonDecision("DoSomething_2", () => Messages.Message("PressedButton_0", MessageTypeDefOf.NeutralEvent, false)),

                    new QuestStageButtonDecision("DoSomething_2",() => Messages.Message("PressedButton_1", MessageTypeDefOf.NeutralEvent, false))
                }
            }
        };
    }
}
