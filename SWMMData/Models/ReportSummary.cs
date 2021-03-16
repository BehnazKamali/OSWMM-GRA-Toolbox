using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWMMToolkitWrapper.Structures;

namespace SWMMData.Models
{
    public class ReportSummary
    {
        public RoutingTotals RoutingTotals;
        public LinkSurchargeSummary LinkSurchargeSummary;
        public NodeFloodingSummary NodeFloodingSummary;
    }
}
