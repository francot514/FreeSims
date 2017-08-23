
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.SimsAntics.Primitives;

namespace TSO.SimsAntics.Model
{
    public class VMRuntimeHeadline
    {
        public VMSetBalloonHeadlineOperand Operand;
        public VMEntity Target;
        public VMEntity IconTarget;
        public sbyte Index;
        public int Duration;
        public int Anim;

        public VMRuntimeHeadline(VMSetBalloonHeadlineOperand op, VMEntity targ, VMEntity icon, sbyte index)
        {
            Operand = op;
            Target = targ;
            IconTarget = icon;
            Index = index;
            Duration = (op.DurationInLoops && op.Duration != -1) ? op.Duration * 15 : op.Duration;
        }


    }
}
