using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMMData.Models
{
    public class ResilienceMetrics
    {
        public bool hasFlooding;
        public List<int> FailureLinksIndex;
        public List<String> FailureLinksID;
        public double MeanFailureTime;   
        public double FailureTime;     
        public double FloodingLossVolume;
        public double MaxNodeFloodingTime;
        public double MinNodeFloodingTime;
        public double MaxNodeFloodingVolume;
        public double MinNodeFloodingVolume;
        public double Score;
        public int FileIndex;

        public double Resilience;

        public ResilienceMetrics()
        {
            FailureLinksIndex = new List<int>();
            FailureLinksID = new List<string>();
        }
    }
}
