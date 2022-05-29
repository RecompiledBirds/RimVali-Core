using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimValiCore.QLine
{
    public static class QuestMaker
    {
        public static QLine MakeQuest(QL_Quest questDef)
        {
            return new QLine(questDef);
        }
    }
}
