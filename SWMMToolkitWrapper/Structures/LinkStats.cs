using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMMToolkitWrapper.Structures
{
    public struct LinkStats
    {
        public double maxFlow;
        public DateTime maxFlowDate;
        public double maxVeloc;
        public double maxDepth;
        public double timeNormalFlow;
        public double timeInletControl;
        public double timeSurcharged;
        public double timeFullUpstream;
        public double timeFullDnstream;
        public double timeFullFlow;
        public double timeCapacityLimited;
        public double[] timeInFlowClass;
        public double timeCourantCritical;
        public long flowTurns;
        public int flowTurnSign;
    }
}
