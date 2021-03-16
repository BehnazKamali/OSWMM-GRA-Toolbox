using SWMMToolkitWrapper.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMMData.Models
{
    public class LinkSurchargeSummary
    {
        public double Diameter;
        public List<int> FailureLinksIndex;
        public List<string> FailureLinksID;

        public bool isNoSurcharge;
        public List<LinkSurcharge> LinkSurchargeInfo;
    }
    public class LinkSurcharge
    {
        public int Index;
        public string ID;
        public double BothEnds;
        public double Upstream;
        public double Dnstream;
        public double HoursAboveFullNormalFlow;
        public double HoursCapacityLimited;

        public LinkStats LinkStats;
    }
}
