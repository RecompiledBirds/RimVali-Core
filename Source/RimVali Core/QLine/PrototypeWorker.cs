using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimValiCore.QLine
{
    public class PrototypeWorker : QuestWorker
    {
        public override bool IsAvalible()
        {
            return true;
        }

        public override int QuestWeight()
        {
            return 1;
        }
    }
}
