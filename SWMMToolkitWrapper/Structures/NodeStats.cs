using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMMToolkitWrapper.Structures
{
    public struct NodeStats
    {
        public double avgDepth;
        public double maxDepth;
        public DateTime maxDepthDate;
        public double maxRptDepth;
        public double volFlooded;
        public double timeFlooded;
        public double timeSurcharged;
        public double timeCourantCritical;
        public double totLatFlow;
        public double maxLatFlow;
        public double maxInflow;
        public double maxOverflow;
        public double maxPondedVol;
        public DateTime maxInflowDate;
        public DateTime maxOverflowDate;
    }
}
