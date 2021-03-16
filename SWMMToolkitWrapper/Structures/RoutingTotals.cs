using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMMToolkitWrapper.Structures
{
    public struct RoutingTotals
    {
        public double dwInflow;
        public double wwInflow;
        public double gwInflow;
        public double iiInflow;
        public double exInflow;
        public double flooding;
        public double outflow;
        public double evapLoss;
        public double seepLoss;
        public double reacted;
        public double initStorage;
        public double finalStorage;
        public double pctError;
    }
}
