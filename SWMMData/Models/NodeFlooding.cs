using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWMMToolkitWrapper.Structures;

namespace SWMMData.Models
{
    public class NodeFloodingSummary
    {
        public double Diameter;
        public List<int> FailureLinksIndex;
        public List<String> FailureLinksID;

        public bool isNoFlooding;
        public List<NodeFlood> NodeFloodsInfo;

        public double SumTotalFloodVolume;
    }

    public class NodeFlood
    {
        public int Index;
        public string ID;
        public double HoursFlooded;
        public double MaximumRate;
        public int TimeOfMaxOccurrence_days;
        public int TimeOfMaxOccurrence_hrs;
        public int TimeOfMaxOccurrence_mins;
        public double TotalFloodVolume;
        public double MaximumPondedVolume;

        public NodeStats NodeStats;
    }
}
