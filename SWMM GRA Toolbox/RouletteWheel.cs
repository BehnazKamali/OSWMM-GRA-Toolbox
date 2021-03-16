using SWMMData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMM_GRA_Toolbox
{
    public class RouletteWheel
    {
        public static void CalculateCumulativePercentTotalFloodVolume(IList<NodeFloodingSummary> topFolderSummary, IList<double> rouletteWheel)
        {
            var sumFitness = topFolderSummary.Sum(c => c.SumTotalFloodVolume);

            var cumulativePercent = 0.0;

            for (int i = 0; i < topFolderSummary.Count; i++)
            {
                cumulativePercent += (topFolderSummary[i].SumTotalFloodVolume) / sumFitness;
                rouletteWheel.Add(cumulativePercent);
            }
        }

        public static List<List<int>> SelectFromWheel(int number, IList<NodeFloodingSummary> topFolderSummary, IList<double> rouletteWheel, Func<double> getPointer, IList<Link> linkList)
        {
            var selected = new List<List<int>>();
            Random rand = new Random();

            for (int i = 0; i < number; i++)
            {
                while (true)
                {
                    var pointer = getPointer();

                    var folderSummary = rouletteWheel
                                            .Select((value, index) => new { Value = value, Index = index })
                                            .FirstOrDefault(r => r.Value >= pointer);


                    if (folderSummary != null)
                    {
                        List<int> failureLinksIndex = new List<int>();
                        failureLinksIndex.AddRange(topFolderSummary[folderSummary.Index].FailureLinksIndex);

                        int randomLinkIndex = rand.Next(0, linkList.Count);

                        if (!failureLinksIndex.Contains(linkList[randomLinkIndex].Index))
                        {
                            failureLinksIndex.Add(linkList[randomLinkIndex].Index);

                            if (!selected.Any(x => x.All(failureLinksIndex.Contains)))
                            {
                                selected.Add(failureLinksIndex);
                                break;
                            }

                            failureLinksIndex.RemoveAt(failureLinksIndex.Count - 1);
                        }

                    }
                }
            }

            return selected;
        }

        public static void CalculateCumulativePercent(IList<ResilienceMetrics> items, IList<double> rouletteWheel, bool reverseScoring = true)
        {
            var sumFitness = items.Sum(c => reverseScoring ? 1 - c.Score : c.Score);

            var cumulativePercent = 0.0;

            for (int i = 0; i < items.Count; i++)
            {
                cumulativePercent += reverseScoring ? (1 - items[i].Score) / sumFitness : items[i].Score / sumFitness;
                rouletteWheel.Add(cumulativePercent);
            }
        }

        public static void SelectFromWheel(int number, IList<ResilienceMetrics> items, IList<double> rouletteWheel, Func<double> getPointer, IList<Link> linkList, ref List<List<int>> selected)
        {
            int repeatSelection = 0;
            Random rand = new Random();

            for (int i = 0; i < number; i++)
            {
                repeatSelection = 0;

                while (repeatSelection < 10000)
                {
                    repeatSelection++;

                    var pointer = getPointer();

                    var metricSummary = rouletteWheel
                                            .Select((value, index) => new { Value = value, Index = index })
                                            .FirstOrDefault(r => r.Value >= pointer);


                    if (metricSummary != null)
                    {
                        List<int> failureLinksIndex = new List<int>();
                        failureLinksIndex.AddRange(items[metricSummary.Index].FailureLinksIndex);

                        int randomLinkIndex = rand.Next(0, linkList.Count);

                        if (!failureLinksIndex.Contains(linkList[randomLinkIndex].Index))
                        {
                            failureLinksIndex.Add(linkList[randomLinkIndex].Index);

                            if (!selected.Any(x => x.All(failureLinksIndex.Contains)))
                            {
                                selected.Add(failureLinksIndex);
                                break;
                            }

                            failureLinksIndex.RemoveAt(failureLinksIndex.Count - 1);
                        }

                    }
                }
            }
        }
    }
}
