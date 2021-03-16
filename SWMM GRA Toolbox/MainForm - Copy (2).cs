using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using SWMMToolkitWrapper;
using SWMMToolkitWrapper.Structures;
using SWMMToolkitWrapper.enums;
using Tempesta.Extensions;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Chromosomes;
using SWMMData.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Threading.Tasks;

namespace SWMM_GRA_Toolbox
{
    public partial class MainForm : Form
    {
        public static int MaxProcessorCores =
            Math.Max(Math.Min(Environment.ProcessorCount,
                (int)(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024 / 1024 / 1024 / 2) + 1), 1);


        public MainForm()
        {
            InitializeComponent();

            lblMaxCores.Text = "(Max Of Cores: " + Environment.ProcessorCount + ")";

            nudCoreNumber_ValueChanged(this, null);

            this.WindowState = FormWindowState.Maximized;
        }

        private void btnSelectFileName_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\Users\\Behnaz\\Desktop\\Inputs";
                openFileDialog.Filter = "input files (*.inp)|*.inp";
                //openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    txtInputFileName.Text = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    //using (StreamReader reader = new StreamReader(fileStream))
                    //{
                    //    fileContent = reader.ReadToEnd();
                    //}
                }
            }
        }

        private void btnOutputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            folderDlg.SelectedPath = @"E:\Test\";
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtOutputFolder.Text = folderDlg.SelectedPath;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStart.Text = "Running";
            btnStart.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                (chkMainDiameter.Checked && lines[li + line + 2].Split(' ')[0].Contains("M")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            List<ReportSummary> Summary = new List<ReportSummary>();

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);

            List<int> toolkitSWMMIndexes = new List<int>();
            for (int t = 0; t < cores; t++)
            {
                toolkitSWMMIndexes.Add(0);
            }
            //ISWMMToolkit[] toolkitSWMMs = new ISWMMToolkit[cores];

            for (int i_index = 1; i_index <= linksCount; i_index++)
            {
                int i = i_index;
                s.WaitOne();

                Thread th = new Thread(delegate ()
                {
                    int failureLinkCount = i;
                    int toolkitSWMMIndex = -1;

                    try
                    {
                        lock (toolkitSWMMIndexes)  //for lock thread
                        {
                            toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                            toolkitSWMMIndexes[toolkitSWMMIndex] = failureLinkCount;
                        }


                        //if (InvokeRequired)
                        //{
                        //    Invoke((Action)delegate
                        //    {
                        //        lblScenarioCounter.Text = "# Failure Links: " + failureLinkCount;
                        //            //lblScenarioCounter.Refresh();
                        //        });
                        //}

                        var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                        int failuringNumber = 0;

                        if (failureLinkCount == 1)
                            failuringNumber = linksCount;
                        else if (failureLinkCount == 2 || failureLinkCount == linksCount - 2)
                            failuringNumber = 100;
                        else if (failureLinkCount == linksCount - 1)
                            failuringNumber = 100;
                        else if (failureLinkCount == linksCount)
                            failuringNumber = 1;
                        else
                            failuringNumber = 100;

                        for (int j = 1; j <= failuringNumber; j++)
                        {
                            List<int> randomFailureLinks = new List<int>();
                            List<int> failureLinkIndexes = new List<int>();
                            Random rand = new Random();

                            randomFailureLinks.AddRange(Enumerable.Range(1, linksCount).OrderBy(m => rand.Next()).Take(failureLinkCount));
                            Thread.Sleep(50);
                            //randomFailureLinks.Add(j);

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < randomFailureLinks.Count(); fl++)
                            {
                                currentFailureLinks += linkList[randomFailureLinks[fl] - 1].Index + "(" + linkList[randomFailureLinks[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[randomFailureLinks[fl] - 1].Index);
                            }

                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, j.ToString() + ".txt")))
                            {
                                //File.WriteAllText(Path.Combine(dirInfo.FullName, j.ToString() + ".txt"), "Failure Diameter: " + Diameter + "\n" + "Failure Links: " + currentFailureLinks + "\n");

                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                //StreamReader oReader;
                                //if (File.Exists(inputFilePath))
                                //{
                                //    string cSearforSomething = "Link";
                                //    oReader = new StreamReader(inputFilePath);
                                //    string cColl = oReader.ReadToEnd();
                                //    string cCriteria = @"\b" + cSearforSomething + @"\b";
                                //    System.Text.RegularExpressions.Regex oRegex = new
                                //    System.Text.RegularExpressions.Regex(cCriteria, RegexOptions.IgnoreCase);

                                //    var count = oRegex.Matches(cColl).AsQueryable();
                                //    Console.WriteLine(count.ToString());
                                //}
                                //Console.ReadLine();

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10();
                                    }

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, j.ToString() + ".rpt"),
                                                            Path.Combine(dirInfo.FullName, j.ToString() + ".out"));

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter);

                                        //////////////////////

                                        RoutingTotals routingTotals = new RoutingTotals();

                                        ErrorCode = toolkitSWMM.GetSystemRoutingStats(ref routingTotals);

                                        myReportFile.WriteLine("Dry Weather Inflow (gal)," + routingTotals.dwInflow * 1.0e6); // w_MGAL to w_GAL
                                        myReportFile.WriteLine("Flooding Loss (gal)," + routingTotals.flooding * 1.0e6); // w_MGAL to w_GAL

                                        //////////////////////

                                        int nodeCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.NODE, ref nodeCount);

                                        int nodeIndex;
                                        int days = 0, hrs = 0, mins = 0, routeModel = 0;
                                        double t, vcf = 0;
                                        string nodeID = "";
                                        bool hasFlood = false;

                                        NodeFloodingSummary nodeFloodingSummary;

                                        nodeFloodingSummary = new NodeFloodingSummary()
                                        {
                                            isNoFlooding = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            NodeFloodsInfo = new List<NodeFlood>(),
                                        };

                                        double sumHoursNodalFlooding = 0;
                                        int numNodalFlooding = 0;

                                        for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                                        {
                                            int nodeType = -1;
                                            toolkitSWMM.GetNodeType(nodeIndex, ref nodeType);

                                            if ((NodeTypeEnum)nodeType == NodeTypeEnum.OUTFALL) continue;

                                            NodeStats nodeStats = new NodeStats();
                                            toolkitSWMM.GetNodeStats(nodeIndex, ref nodeStats);

                                            if (nodeStats.timeFlooded == 0.0) continue;

                                            t = Math.Max(0.01, nodeStats.timeFlooded);

                                            hasFlood = true;

                                            NodeFlood nodeFlood = new NodeFlood();

                                            nodeFlood.Index = nodeIndex;

                                            toolkitSWMM.GetObjectId((int)ObjectTypeEnum.NODE, nodeIndex, ref nodeID);
                                            nodeFlood.ID = nodeID;
                                            nodeFlood.HoursFlooded = t;

                                            nodeFlood.MaximumRate = nodeStats.maxOverflow;

                                            toolkitSWMM.GetElapsedTime(nodeStats.maxOverflowDate, ref days, ref hrs, ref mins);

                                            nodeFlood.TimeOfMaxOccurrence_days = days;
                                            nodeFlood.TimeOfMaxOccurrence_hrs = hrs;
                                            nodeFlood.TimeOfMaxOccurrence_mins = mins;

                                            toolkitSWMM.GetVcf(ref vcf);
                                            nodeFlood.TotalFloodVolume = nodeStats.volFlooded * vcf * 1.0e6; // w_MGAL to w_GAL (VolUnitsWords)

                                            toolkitSWMM.GetRouteModel(ref routeModel);

                                            if ((RouteModelTypeEnum)routeModel == RouteModelTypeEnum.DW)
                                            {
                                                double fullDepth = 0;
                                                toolkitSWMM.GetNodeParam(nodeIndex, (int)NodeProperty.FULLDEPTH, ref fullDepth);
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxDepth - fullDepth;
                                            }
                                            else
                                            {
                                                //? / 1000
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxPondedVol / 1000.0;
                                            }

                                            nodeFlood.NodeStats = nodeStats;

                                            myReportFile.WriteLine("NodeFlooding," + nodeFlood.Index + "," + nodeFlood.ID + "," +
                                                nodeFlood.HoursFlooded.ToString("0.###") + "," + nodeFlood.MaximumRate.ToString("0.###") + "," +
                                                nodeFlood.TimeOfMaxOccurrence_days + "," + nodeFlood.TimeOfMaxOccurrence_hrs + "," + nodeFlood.TimeOfMaxOccurrence_mins + "," +
                                                nodeFlood.TotalFloodVolume.ToString("0.###") + "," + nodeFlood.MaximumPondedVolume.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.avgDepth.ToString("0.###") + "," + nodeFlood.NodeStats.maxDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.maxDepthDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxRptDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.volFlooded.ToString("0.###") + "," + nodeFlood.NodeStats.timeFlooded.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.timeSurcharged.ToString("0.###") + "," + nodeFlood.NodeStats.timeCourantCritical.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.totLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflow.ToString("0.###") +
                                                nodeFlood.NodeStats.maxOverflow.ToString("0.###") + "," + nodeFlood.NodeStats.maxPondedVol.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflowDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxOverflowDate.ToLongTimeString());

                                            nodeFloodingSummary.NodeFloodsInfo.Add(nodeFlood);

                                            sumHoursNodalFlooding += nodeFlood.HoursFlooded;
                                            if (nodeFlood.HoursFlooded > 0)
                                            {
                                                numNodalFlooding++;
                                            }

                                        }

                                        if (!hasFlood)
                                        {
                                            nodeFloodingSummary.isNoFlooding = true;
                                        }

                                        //////////////////

                                        string linkID = "";

                                        LinkSurchargeSummary linkSurchargeSummary;

                                        linkSurchargeSummary = new LinkSurchargeSummary()
                                        {
                                            isNoSurcharge = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            LinkSurchargeInfo = new List<LinkSurcharge>(),
                                        };

                                        //double[] ts = new double[5];
                                        //int linkCount = 0;
                                        //toolkitSWMM.GetObjectCount(ObjectTypeEnum.LINK, ref linkCount);

                                        //int linkIndex;
                                        //bool hasSurcharge = false;

                                        //for (linkIndex = 0; linkIndex < linkCount; linkIndex++)
                                        //{
                                        //    int linkType = 0;
                                        //    int xsectType = 0;
                                        //    LinkStats linkStats = new LinkStats();

                                        //    toolkitSWMM.GetLinkStats(linkIndex, ref linkStats);
                                        //    toolkitSWMM.GetLinkType(linkIndex, ref linkType);
                                        //    toolkitSWMM.GetLinkXsectType(linkIndex, ref xsectType);

                                        //    if ((LinkTypeEnum)linkType != LinkTypeEnum.CONDUIT || (XSectionTypeEnum)xsectType == XSectionTypeEnum.DUMMY) continue;

                                        //    ts[0] = linkStats.timeSurcharged;
                                        //    ts[1] = linkStats.timeFullUpstream;
                                        //    ts[2] = linkStats.timeFullDnstream;
                                        //    ts[3] = linkStats.timeFullFlow;

                                        //    if (ts[0] + ts[1] + ts[2] + ts[3] == 0.0)
                                        //    {
                                        //        continue;
                                        //    }
                                        //    ts[4] = linkStats.timeCapacityLimited;

                                        //    for (int ts_index = 0; ts_index < 5; ts_index++)
                                        //        ts[ts_index] = Math.Max(0.01, ts[ts_index]);

                                        //    hasSurcharge = true;

                                        //    LinkSurcharge linkSurcharge = new LinkSurcharge();

                                        //    linkSurcharge.Index = linkIndex;

                                        //    toolkitSWMM.GetObjectId((int)ObjectTypeEnum.LINK, linkIndex, ref linkID);

                                        //    linkSurcharge.ID = linkID;

                                        //    linkSurcharge.BothEnds = ts[0];
                                        //    linkSurcharge.Upstream = ts[1];
                                        //    linkSurcharge.Dnstream = ts[2];
                                        //    linkSurcharge.HoursAboveFullNormalFlow = ts[3];
                                        //    linkSurcharge.HoursCapacityLimited = ts[4];

                                        //    linkSurcharge.LinkStats = linkStats;

                                        //    myReportFile.WriteLine("LinkSurcharge," + linkSurcharge.Index + "," + linkSurcharge.ID + "," + linkSurcharge.BothEnds.ToString("0.###") + "," + linkSurcharge.Upstream.ToString("0.###") + "," + linkSurcharge.Dnstream.ToString("0.###") + "," +
                                        //        linkSurcharge.HoursAboveFullNormalFlow.ToString("0.###") + "," + linkSurcharge.HoursCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlowDate.ToLongTimeString() + "," +
                                        //        linkSurcharge.LinkStats.maxVeloc.ToString("0.###") + "," + linkSurcharge.LinkStats.maxVeloc.ToString("0.###") + "," + linkSurcharge.LinkStats.maxDepth.ToString("0.###") + "," + linkSurcharge.LinkStats.timeNormalFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeInletControl.ToString("0.###") + "," + linkSurcharge.LinkStats.timeSurcharged.ToString("0.###") + "," +
                                        //        linkSurcharge.LinkStats.timeFullUpstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullDnstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCourantCritical.ToString("0.###") + "," +
                                        //        linkSurcharge.LinkStats.flowTurns + "," + linkSurcharge.LinkStats.flowTurnSign);

                                        //    linkSurchargeSummary.LinkSurchargeInfo.Add(linkSurcharge);
                                        //}

                                        //if (!hasSurcharge)
                                        //{
                                        //    linkSurchargeSummary.isNoSurcharge = true;
                                        //}


                                        ////////////////////////
                                        if (numNodalFlooding != 0)
                                        {
                                            double meanNodalFlooding = sumHoursNodalFlooding / numNodalFlooding;

                                            myReportFile.WriteLine("Sum Hours Nodal Flooding," + sumHoursNodalFlooding);
                                            myReportFile.WriteLine("Mean Hours Nodal Flooding," + meanNodalFlooding);
                                        }



                                        Summary.Add(new ReportSummary()
                                        {
                                            RoutingTotals = routingTotals,
                                            NodeFloodingSummary = nodeFloodingSummary,
                                            LinkSurchargeSummary = linkSurchargeSummary,
                                        });
                                        //////////////////////////////
                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();
                                    // Close the system
                                    toolkitSWMM.Close();
                                    //return error_getCode(ErrorCode);
                                }
                                catch (Exception ex)
                                {
                                    // Log error.
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error.
                    }
                    finally
                    {
                        lock (toolkitSWMMIndexes)
                        {
                            toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                        }

                        s.Release();
                    }
                });

                th.Name = "Failure scenario: " + i;
                th.IsBackground = true;
                th.Priority = ThreadPriority.AboveNormal;
                //th.SetApartmentState(ApartmentState.STA);
                th.Start();
            }

            btnStart.Text = "Start";
            btnStart.Refresh();
        }

        public void StepRunning(ISWMMToolkit toolkitSWMM, List<int> failureLinks, double diameter)
        {
            double elapsedTime = 0.0;
            int errorCode = 0;

            int numberOfFailureLinks = failureLinks.Count();
            TXsect[] originalTXsect = new TXsect[numberOfFailureLinks];
            double[] originalRoughness = new double[numberOfFailureLinks];

            int index = 0;
            int i = 0;
            int saveFlag = 1;

            double geom1 = diameter;

            //for (i = 0; i < numberOfFailureLinks; i++)
            //{
            //    index = failureLinks[i];

            //    if (nudFailuringRoughness.Value == 0)
            //    {
            //        toolkitSWMM.GetLinkXsectType(index, ref originalTXsect[i].type);
            //        toolkitSWMM.GetLinkGeom(index, ref originalTXsect[i].g1, ref originalTXsect[i].g2, ref originalTXsect[i].g3, ref originalTXsect[i].g4);

            //        toolkitSWMM.SetLinkGeom(index, originalTXsect[i].type, geom1, originalTXsect[i].g2, originalTXsect[i].g3, originalTXsect[i].g4);
            //    }
            //    else
            //    {
            //        toolkitSWMM.GetConduitLinkRoughness(index, ref originalRoughness[i]);
            //        toolkitSWMM.SetConduitLinkRoughness(index, (double)nudFailuringRoughness.Value);
            //    }
            //}

            // --- initialize values
            errorCode = toolkitSWMM.Start(saveFlag);

            // --- execute each time step until elapsed time is re-set to 0
            if (errorCode == 0)
            {
                //writecon("\n o  Simulating day: 0     hour:  0");
                do
                {
                    toolkitSWMM.Step(ref elapsedTime);

                } while (elapsedTime > 0.0 && errorCode == 0);
                //writecon("Simulation complete");
            }
        }

        public void StepRunning(ISWMMToolkit_Original toolkitSWMM, List<int> failureLinks, double diameter)
        {
            double elapsedTime = 0.0;
            int errorCode = 0;

            int numberOfFailureLinks = failureLinks.Count();
            TXsect[] originalTXsect = new TXsect[numberOfFailureLinks];
            double[] originalRoughness = new double[numberOfFailureLinks];

            int index = 0;
            int i = 0;
            int saveFlag = 1;

            double geom1 = diameter;

            // --- initialize values
            errorCode = toolkitSWMM.Start(saveFlag);

            // --- execute each time step until elapsed time is re-set to 0
            if (errorCode == 0)
            {
                //writecon("\n o  Simulating day: 0     hour:  0");
                do
                {
                    toolkitSWMM.Step(ref elapsedTime);

                } while (elapsedTime > 0.0 && errorCode == 0);
                //writecon("Simulation complete");
            }
        }

        public void StepRunning(ISWMMToolkit toolkitSWMM, int failureStartTime, int failureEndTime, List<int> failureLinks, double diameter)
        {
            long newHour, oldHour = 0;
            long theDay, theHour;
            double elapsedTime = 0.0;
            int errorCode = 0;

            int numberOfFailureLinks = failureLinks.Count();
            TXsect[] originalTXsect = new TXsect[numberOfFailureLinks];
            double[] originalRoughness = new double[numberOfFailureLinks];

            int isSetFailureLinkValue = 0;
            int isResetLinkValue = 0;
            int index = 0;
            int i = 0;
            int saveFlag = 1;

            double geom1 = diameter;
            // --- initialize values
            errorCode = toolkitSWMM.Start(saveFlag);

            // --- execute each time step until elapsed time is re-set to 0
            if (errorCode == 0)
            {
                //writecon("\n o  Simulating day: 0     hour:  0");
                do
                {
                    toolkitSWMM.Step(ref elapsedTime);
                    newHour = (long)(elapsedTime * 24.0);
                    if (newHour >= oldHour)
                    {
                        theDay = (long)elapsedTime;
                        theHour = (long)((elapsedTime - Math.Floor(elapsedTime)) * 24.0);
                        //writecon("\b\b\b\b\b\b\b\b\b\b\b\b\b\b");
                        //sprintf(Msg, "%-5d hour: %-2d", theDay, theHour);
                        //writecon(Msg);
                        oldHour = newHour;

                        try
                        {
                            if (isSetFailureLinkValue == 0 && newHour >= failureStartTime && newHour < failureEndTime)
                            {
                                isSetFailureLinkValue = 1;
                                for (i = 0; i < numberOfFailureLinks; i++)
                                {
                                    index = failureLinks[i];

                                    if (nudFailuringRoughness.Value == 0)
                                    {
                                        toolkitSWMM.GetLinkXsectType(index, ref originalTXsect[i].type);
                                        toolkitSWMM.GetLinkGeom(index, ref originalTXsect[i].g1, ref originalTXsect[i].g2, ref originalTXsect[i].g3, ref originalTXsect[i].g4);

                                        toolkitSWMM.SetLinkGeom(index, originalTXsect[i].type, geom1, originalTXsect[i].g2, originalTXsect[i].g3, originalTXsect[i].g4);
                                    }
                                    else
                                    {
                                        toolkitSWMM.GetConduitLinkRoughness(index, ref originalRoughness[i]);
                                        toolkitSWMM.SetConduitLinkRoughness(index, (double)nudFailuringRoughness.Value);
                                    }
                                }

                            }
                            else if (isResetLinkValue == 0 && newHour > failureEndTime)
                            {
                                isResetLinkValue = 1;
                                for (i = 0; i < numberOfFailureLinks; i++)
                                {
                                    index = failureLinks[i];

                                    if (nudFailuringRoughness.Value == 0)
                                    {
                                        toolkitSWMM.SetLinkGeom(index, originalTXsect[i].type, originalTXsect[i].g1, originalTXsect[i].g2, originalTXsect[i].g3, originalTXsect[i].g4);
                                    }
                                    else
                                    {
                                        toolkitSWMM.SetConduitLinkRoughness(index, originalRoughness[i]);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {

                            throw;
                        }

                    }
                } while (elapsedTime > 0.0 && errorCode == 0);
                //writecon("Simulation complete");
            }
        }

        public void StepRunning(ISWMMToolkit toolkitSWMM, int failureStartTime, int failureEndTime, List<int> failureLinks, double diameter, string outPath, ref double floodElapsedTime, ref double floodOutfallInflow, bool runFromInflowAnalysis = false)
        {
            long newHour, oldHour = 0;
            long newMinutes, oldMinutes = 0;
            long theDay, theHour;
            double elapsedTime = 0.0;
            int errorCode = 0;

            int numberOfFailureLinks = failureLinks.Count();
            TXsect[] originalTXsect = new TXsect[numberOfFailureLinks];
            double[] originalRoughness = new double[numberOfFailureLinks];

            int isSetFailureLinkValue = 0;
            int isResetLinkValue = 0;
            int index = 0;
            int i = 0;
            int saveFlag = 1;

            double geom1 = diameter;
            // --- initialize values
            errorCode = toolkitSWMM.Start(saveFlag);

            int nodeCount = 0;
            int nodeOutfallIndex = -1;
            toolkitSWMM.GetObjectCount(ObjectTypeEnum.NODE, ref nodeCount);

            int nodeIndex;
            double totalInflow = 0, outfallInflow = 0, vcf = 0;

            RoutingTotals routingTotals = new RoutingTotals();

            List<string> totalInflowList = new List<string>();
            List<string> outfallInflowList = new List<string>();

            for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                int nodeType = -1;
                toolkitSWMM.GetNodeType(nodeIndex, ref nodeType);

                if ((NodeTypeEnum)nodeType == NodeTypeEnum.OUTFALL)
                {
                    nodeOutfallIndex = nodeIndex;
                    break;
                }
            }

            toolkitSWMM.GetVcf(ref vcf);

            toolkitSWMM.GetSystemRoutingStats(ref routingTotals);
            totalInflow = routingTotals.dwInflow * 1.0e6; // w_MGAL to w_GAL

            toolkitSWMM.GetNodeTotalInflow(nodeOutfallIndex, ref outfallInflow);
            outfallInflow = outfallInflow * vcf * 1.0e6; // w_MGAL to w_GAL (VolUnitsWords)

            outfallInflowList.Add("0" + " " + outfallInflow.ToString("0.###") + " " + "0"); // Time, Outfall TotalInflow, isFlooded
            totalInflowList.Add("0" + " " + outfallInflow.ToString("0.###") + " " + "0"); // Time, Outfall TotalInflow, isFlooded

            if (runFromInflowAnalysis)
            {
                if (InvokeRequired)  //Onlly Original Thread
                {
                    Invoke((Action)delegate
                    {
                        chtTotalNodeInflow.Series["Series1"].Points.AddXY(0, outfallInflow);
                        chtTotalNodeInflow.Update();
                    });
                }
            }



            // --- execute each time step until elapsed time is re-set to 0
            if (errorCode == 0)
            {
                //writecon("\n o  Simulating day: 0     hour:  0");
                do
                {
                    toolkitSWMM.Step(ref elapsedTime);
                    newHour = (long)(elapsedTime * 24.0);
                    newMinutes = (long)((elapsedTime * 24.0 * 60.0) % 60);

                    if (newHour >= oldHour)
                    {
                        int isFlooded = 0;
                        toolkitSWMM.GetIsExistAnyNodeFlooding(ref isFlooded);

                        //toolkitSWMM.GetSystemRoutingStats(ref routingTotals);
                        //totalInflow = routingTotals.dwInflow * 1.0e6; // w_MGAL to w_GAL

                        toolkitSWMM.GetNodeTotalInflow(nodeOutfallIndex, ref outfallInflow);
                        outfallInflow = outfallInflow * vcf * 1.0e6; // w_MGAL to w_GAL (VolUnitsWords)

                        if (isFlooded == 1 && floodElapsedTime == 0)
                        {
                            floodElapsedTime = elapsedTime;
                            floodOutfallInflow = outfallInflow;
                        }

                        // Uncommented for Inflow Analysis
                        if (runFromInflowAnalysis && (newHour > oldHour || (newMinutes > oldMinutes && newMinutes % 1 == 0)))
                        {
                            totalInflowList.Add(elapsedTime + " " + totalInflow.ToString("0.###") + " " + isFlooded.ToString()); // Time, Outfall TotalInflow, isFlooded
                            outfallInflowList.Add(elapsedTime + " " + outfallInflow.ToString("0.###") + " " + isFlooded.ToString()); // Time, Outfall TotalInflow, isFlooded

                            if (InvokeRequired && (newHour > oldHour || (newMinutes > oldMinutes && newMinutes % 1 == 0)))
                            {
                                Invoke((Action)delegate
                                {
                                    chtTotalNodeInflow.Series["Series1"].Points.AddXY(newHour * 60 + newMinutes, outfallInflow);
                                    chtTotalNodeInflow.Update();
                                });
                            }
                        }
                    }

                    if (newHour >= oldHour)
                    {
                        theDay = (long)elapsedTime;
                        theHour = (long)((elapsedTime - Math.Floor(elapsedTime)) * 24.0);
                        //writecon("\b\b\b\b\b\b\b\b\b\b\b\b\b\b");
                        //sprintf(Msg, "%-5d hour: %-2d", theDay, theHour);
                        //writecon(Msg);
                        oldHour = newHour;
                        oldMinutes = newMinutes;

                        try
                        {
                            if (isSetFailureLinkValue == 0 && newHour >= failureStartTime && newHour < failureEndTime)
                            {
                                isSetFailureLinkValue = 1;
                                for (i = 0; i < numberOfFailureLinks; i++)
                                {
                                    index = failureLinks[i];

                                    if (nudFailuringRoughness.Value == 0)
                                    {
                                        toolkitSWMM.GetLinkXsectType(index, ref originalTXsect[i].type);
                                        toolkitSWMM.GetLinkGeom(index, ref originalTXsect[i].g1, ref originalTXsect[i].g2, ref originalTXsect[i].g3, ref originalTXsect[i].g4);

                                        toolkitSWMM.SetLinkGeom(index, originalTXsect[i].type, geom1, originalTXsect[i].g2, originalTXsect[i].g3, originalTXsect[i].g4);
                                    }
                                    else
                                    {
                                        toolkitSWMM.GetConduitLinkRoughness(index, ref originalRoughness[i]);
                                        toolkitSWMM.SetConduitLinkRoughness(index, (double)nudFailuringRoughness.Value);
                                    }
                                }

                            }
                            else if (isResetLinkValue == 0 && newHour > failureEndTime)
                            {
                                isResetLinkValue = 1;
                                for (i = 0; i < numberOfFailureLinks; i++)
                                {
                                    index = failureLinks[i];

                                    if (nudFailuringRoughness.Value == 0)
                                    {
                                        toolkitSWMM.SetLinkGeom(index, originalTXsect[i].type, originalTXsect[i].g1, originalTXsect[i].g2, originalTXsect[i].g3, originalTXsect[i].g4);
                                    }
                                    else
                                    {
                                        toolkitSWMM.SetConduitLinkRoughness(index, originalRoughness[i]);
                                    }
                                }

                                if (floodElapsedTime == 0)
                                {
                                    toolkitSWMM.GetNodeTotalInflow(nodeOutfallIndex, ref outfallInflow);
                                    outfallInflow = outfallInflow * vcf * 1.0e6; // w_MGAL to w_GAL (VolUnitsWords)

                                    floodElapsedTime = elapsedTime;
                                    floodOutfallInflow = outfallInflow;
                                }
                            }
                        }
                        catch (Exception e)
                        {

                            throw;
                        }

                    }

                } while (elapsedTime > 0.0 && errorCode == 0);
                //writecon("Simulation complete");

                if (runFromInflowAnalysis)
                {
                    try
                    {
                        File.WriteAllLines(outPath + "_outfallInflow.flw", outfallInflowList);
                        File.WriteAllLines(outPath + "_totalInflow.flw", totalInflowList);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }


        private void btnResultsDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtResultsDirectory.Text = folderDlg.SelectedPath;
            }
        }

        private void btnAnalysis_Click(object sender, EventArgs e)
        {
            string[] ResultsDirectories = Directory.GetDirectories(txtResultsDirectory.Text).OrderBy(x => x, new OrdinalStringComparer()).ToArray();

            //chtTotalFloodVolume.ChartAreas[0].AxisX.Minimum = 0;
            //chtTotalFloodVolume.ChartAreas[0].AxisX.Maximum = 100;

            for (int i = 1; i <= ResultsDirectories.Count(); i++)
            {
                string[] ResultsFiles = Directory.GetFiles(Path.Combine(txtResultsDirectory.Text, i.ToString()), "*.txt").OrderBy(x => x, new OrdinalStringComparer()).ToArray();
                List<Tuple<int, NodeFloodingSummary, LinkSurchargeSummary, double>> Summary = new List<Tuple<int, NodeFloodingSummary, LinkSurchargeSummary, double>>();

                double totalFloodVolume = 0;

                double meanResilience = 0;
                double sumResilience = 0;

                int c_count = 0, b_count = 0, n_count = 0, h_count = 0, m_count = 0;

                for (int j = 1; j <= ResultsFiles.Count(); j++)
                {
                    lblAnalysis.Text = "# Scenario: " + i + ", # Failuring: " + j;
                    lblAnalysis.Refresh();

                    string[] lines = File.ReadAllLines(Path.Combine(txtResultsDirectory.Text, i.ToString(), j.ToString() + ".txt"), Encoding.UTF8);

                    double resilience = 0;
                    double totalFlood = 0;
                    double totalInflow = 0;
                    double meanHoursTotalFlood = 0;

                    double diameter = Convert.ToDouble(lines[0].Split(' ')[2]);
                    string[] failureLinks = lines[1].Split(' ')[2].Split(',');

                    List<int> failureLinksIndex = new List<int>();
                    List<string> failureLinksID = new List<string>();


                    for (int f = 0; f < failureLinks.Count() - 1; f++)
                    {
                        int index = Convert.ToInt32(failureLinks[f].Split('(')[0]);
                        string id = (failureLinks[f].Split('(')[1]).Substring(0, failureLinks[f].Split('(')[1].Length - 1);

                        failureLinksIndex.Add(index);
                        failureLinksID.Add(id);

                        if (id.Contains("M"))
                            m_count++;
                        else if (id.Contains("C"))
                            c_count++;
                        else if (id.Contains("B"))
                            b_count++;
                        else if (id.Contains("H"))
                            h_count++;
                        else if (id.Contains("N"))
                            n_count++;

                    }

                    NodeFloodingSummary nodeFloodingSummary = new NodeFloodingSummary()
                    {
                        isNoFlooding = true,
                        Diameter = diameter,
                        FailureLinksIndex = failureLinksIndex,
                        FailureLinksID = failureLinksID,
                        NodeFloodsInfo = new List<NodeFlood>(),
                    };

                    LinkSurchargeSummary linkSurchargeSummary = new LinkSurchargeSummary()
                    {
                        isNoSurcharge = true,
                        Diameter = diameter,
                        FailureLinksIndex = failureLinksIndex,
                        FailureLinksID = failureLinksID,
                        LinkSurchargeInfo = new List<LinkSurcharge>(),
                    };

                    for (int k = 2; k < lines.Count(); k++)
                    {
                        string[] data = lines[k].Split(',');

                        switch (data[0])
                        {
                            case "NodeFlooding":
                                nodeFloodingSummary.isNoFlooding = false;

                                nodeFloodingSummary.NodeFloodsInfo.Add(new NodeFlood()
                                {
                                    Index = Convert.ToInt32(data[1]),
                                    ID = data[2],
                                    HoursFlooded = Convert.ToDouble(data[3]),
                                    MaximumRate = Convert.ToDouble(data[4]),
                                    TimeOfMaxOccurrence_days = Convert.ToInt32(data[5]),
                                    TimeOfMaxOccurrence_hrs = Convert.ToInt32(data[6]),
                                    TimeOfMaxOccurrence_mins = Convert.ToInt32(data[7]),
                                    TotalFloodVolume = Convert.ToDouble(data[8]),
                                    MaximumPondedVolume = Convert.ToDouble(data[9]),
                                });

                                nodeFloodingSummary.SumTotalFloodVolume += Convert.ToDouble(data[8]);
                                totalFloodVolume += Convert.ToDouble(data[8]);

                                break;
                            case "LinkSurcharge":
                                linkSurchargeSummary.isNoSurcharge = false;

                                linkSurchargeSummary.LinkSurchargeInfo.Add(new LinkSurcharge()
                                {
                                    Index = Convert.ToInt32(data[1]),
                                    ID = data[2],
                                    BothEnds = Convert.ToDouble(data[3]),
                                    Upstream = Convert.ToDouble(data[4]),
                                    Dnstream = Convert.ToDouble(data[5]),
                                    HoursAboveFullNormalFlow = Convert.ToDouble(data[6]),
                                    HoursCapacityLimited = Convert.ToDouble(data[7]),
                                });
                                break;
                            case "Dry Weather Inflow (gal)":
                                totalInflow = Convert.ToDouble(data[1]);
                                break;
                            case "Flooding Loss (gal)":
                                totalFlood = Convert.ToDouble(data[1]);
                                break;
                            case "Mean Hours Nodal Flooding":
                                meanHoursTotalFlood = Convert.ToDouble(data[1]);
                                break;
                            default:
                                break;
                        }
                    }

                    //resilience = 1 - ((totalFlood / (56545000)) * (meanHoursTotalFlood / (double)nudSimulationTime.Value));
                    // resilience = 1 - ((totalFlood / (totalInflow)) * (meanHoursTotalFlood / (double)nudSimulationTime.Value));
                    // resilience = 1 - totalFlood / totalInflow;
                    resilience = 1 - totalFlood / 349751.006;

                    sumResilience += resilience;

                    Summary.Add(new Tuple<int, NodeFloodingSummary, LinkSurchargeSummary, double>(i, nodeFloodingSummary, linkSurchargeSummary, resilience));
                }

                if (ResultsFiles.Count() != 0)
                {
                    meanResilience = sumResilience / ResultsFiles.Count();
                    //meanResilience = Summary.OrderBy(x => x.Item4).Take(100).Sum(x => x.Item4) / 100;
                }

                chtTotalFloodVolume.Series["Series1"].Points.AddXY(i, totalFloodVolume);
                chtNodeFloodingCount.Series["Series1"].Points.AddXY(i, Summary.Where(x => x.Item1 == i).Sum(x => x.Item2.NodeFloodsInfo.Count));

                chtFailureLinksCount.Series["Series1"].Points.AddXY(i, c_count);
                chtFailureLinksCount.Series["Series2"].Points.AddXY(i, b_count);
                chtFailureLinksCount.Series["Series3"].Points.AddXY(i, h_count);
                chtFailureLinksCount.Series["Series4"].Points.AddXY(i, n_count);
                chtFailureLinksCount.Series["Series5"].Points.AddXY(i, m_count);

                chtResilience.Series["Series1"].Points.AddXY(i, meanResilience);



                chtTotalFloodVolume.Update();
                chtNodeFloodingCount.Update();
                chtFailureLinksCount.Update();
                chtResilience.Update();
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            nudFolderName.Value++;
            nudFolderName.Refresh();
            AnalysisFolder();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            nudFolderName.Value--;
            nudFolderName.Refresh();
            AnalysisFolder();
        }

        private void AnalysisFolder()
        {
            string[] ResultsDirectories = Directory.GetDirectories(txtResultsDirectory.Text).OrderBy(x => x, new OrdinalStringComparer()).ToArray();

            List<Tuple<int, NodeFloodingSummary, LinkSurchargeSummary>> Summary = new List<Tuple<int, NodeFloodingSummary, LinkSurchargeSummary>>();

            int i = (int)nudFolderName.Value;
            string[] ResultsFiles = Directory.GetFiles(Path.Combine(txtResultsDirectory.Text, i.ToString()), "*.txt").OrderBy(x => x, new OrdinalStringComparer()).ToArray();

            int c_count = 0, b_count = 0, n_count = 0, h_count = 0, m_count = 0;

            chtFloodingPerOneSimulation.Series["Series1"].Points.Clear();
            chtFloodingPerOneSimulation.Update();



            for (int j = 1; j <= ResultsFiles.Count(); j++)
            {
                double totalFloodvolume = 0;

                lblAnalysis.Text = "# Scenario: " + i + ", # Failuring: " + j;
                lblAnalysis.Refresh();

                string[] lines = File.ReadAllLines(Path.Combine(txtResultsDirectory.Text, i.ToString(), j.ToString() + ".txt"), Encoding.UTF8);

                double diameter = Convert.ToDouble(lines[0].Split(' ')[2]);
                string[] failureLinks = lines[1].Split(' ')[2].Split(',');

                List<int> failureLinksIndex = new List<int>();
                List<string> failureLinksID = new List<string>();


                for (int f = 0; f < failureLinks.Count() - 1; f++)
                {
                    int index = Convert.ToInt32(failureLinks[f].Split('(')[0]);
                    string id = (failureLinks[f].Split('(')[1]).Substring(0, failureLinks[f].Split('(')[1].Length - 1);

                    failureLinksIndex.Add(index);
                    failureLinksID.Add(id);

                    if (id.Contains("M"))
                        m_count++;
                    else if (id.Contains("C"))
                        c_count++;
                    else if (id.Contains("B"))
                        b_count++;
                    else if (id.Contains("H"))
                        h_count++;
                    else if (id.Contains("N"))
                        n_count++;

                }

                NodeFloodingSummary nodeFloodingSummary = new NodeFloodingSummary()
                {
                    isNoFlooding = true,
                    Diameter = diameter,
                    FailureLinksIndex = failureLinksIndex,
                    FailureLinksID = failureLinksID,
                    NodeFloodsInfo = new List<NodeFlood>(),
                };

                LinkSurchargeSummary linkSurchargeSummary = new LinkSurchargeSummary()
                {
                    isNoSurcharge = true,
                    Diameter = diameter,
                    FailureLinksIndex = failureLinksIndex,
                    FailureLinksID = failureLinksID,
                    LinkSurchargeInfo = new List<LinkSurcharge>(),
                };

                for (int k = 2; k < lines.Count(); k++)
                {
                    string[] data = lines[k].Split(',');

                    switch (data[0])
                    {
                        case "NodeFlooding":
                            nodeFloodingSummary.isNoFlooding = false;

                            nodeFloodingSummary.NodeFloodsInfo.Add(new NodeFlood()
                            {
                                Index = Convert.ToInt32(data[1]),
                                ID = data[2],
                                HoursFlooded = Convert.ToDouble(data[3]),
                                MaximumRate = Convert.ToDouble(data[4]),
                                TimeOfMaxOccurrence_days = Convert.ToInt32(data[5]),
                                TimeOfMaxOccurrence_hrs = Convert.ToInt32(data[6]),
                                TimeOfMaxOccurrence_mins = Convert.ToInt32(data[7]),
                                TotalFloodVolume = Convert.ToDouble(data[8]),
                                MaximumPondedVolume = Convert.ToDouble(data[9]),
                            });

                            nodeFloodingSummary.SumTotalFloodVolume += Convert.ToDouble(data[8]);
                            totalFloodvolume += Convert.ToDouble(data[8]);

                            break;
                        case "LinkSurcharge":
                            linkSurchargeSummary.isNoSurcharge = false;

                            linkSurchargeSummary.LinkSurchargeInfo.Add(new LinkSurcharge()
                            {
                                Index = Convert.ToInt32(data[1]),
                                ID = data[2],
                                BothEnds = Convert.ToDouble(data[3]),
                                Upstream = Convert.ToDouble(data[4]),
                                Dnstream = Convert.ToDouble(data[5]),
                                HoursAboveFullNormalFlow = Convert.ToDouble(data[6]),
                                HoursCapacityLimited = Convert.ToDouble(data[7]),
                            });
                            break;
                        default:
                            break;
                    }
                }


                chtFloodingPerOneSimulation.Series["Series1"].Points.AddXY(j, totalFloodvolume);
                chtFloodingPerOneSimulation.Update();

                Summary.Add(new Tuple<int, NodeFloodingSummary, LinkSurchargeSummary>(i, nodeFloodingSummary, linkSurchargeSummary));
            }

            using (StreamWriter OrderFailureFile = new StreamWriter(Path.Combine(txtResultsDirectory.Text, i.ToString() + ".txt")))
            {
                foreach (var item in Summary.OrderByDescending(x => x.Item2.SumTotalFloodVolume))
                {
                    OrderFailureFile.WriteLine("Failure Links ID: " + item.Item2.FailureLinksID[0] + ", Sum Total Flood Volume: " + item.Item2.SumTotalFloodVolume.ToString("0.###") + ", Sum Hours Flooded: " + item.Item2.NodeFloodsInfo.Sum(x => x.HoursFlooded).ToString("0.##") + ", Flooded Nodes: " + string.Join(",", item.Item2.NodeFloodsInfo.Select(x => x.ID).ToArray()));
                }
            }
        }

        private void btnStartFixed_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartFixed.Text = "Running";
            btnStartFixed.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>> Summary = new List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>>();

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);

            List<int> toolkitSWMMIndexes = new List<int>();
            for (int t = 0; t < cores; t++)
            {
                toolkitSWMMIndexes.Add(0);
            }
            //ISWMMToolkit[] toolkitSWMMs = new ISWMMToolkit[cores];

            for (int i_index = (int)nudFixedFailuring.Value; i_index <= (int)nudFixedFailuring.Value; i_index++)
            {
                int i = i_index;
                s.WaitOne();

                Thread th = new Thread(delegate ()
                {
                    int failureLinkCount = i;
                    int toolkitSWMMIndex = -1;

                    try
                    {
                        lock (toolkitSWMMIndexes)
                        {
                            toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                            toolkitSWMMIndexes[toolkitSWMMIndex] = failureLinkCount;
                        }


                        //if (InvokeRequired)
                        //{
                        //    Invoke((Action)delegate
                        //    {
                        //        lblScenarioCounter.Text = "# Failure Links: " + failureLinkCount;
                        //            //lblScenarioCounter.Refresh();
                        //        });
                        //}

                        var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                        int failuringNumber = 5;

                        //if (failureLinkCount == 1)
                        //    failuringNumber = 0;
                        //else if (failureLinkCount == 2 || failureLinkCount == linksCount - 2)
                        //    failuringNumber = 500;
                        //else if (failureLinkCount == linksCount - 1)
                        //    failuringNumber = 500;
                        //else if (failureLinkCount == linksCount)
                        //    failuringNumber = 1;
                        //else
                        //    failuringNumber = 500;

                        for (int j = 1; j <= failuringNumber; j++)
                        {
                            List<int> randomFailureLinks = new List<int>();
                            List<int> failureLinkIndexes = new List<int>();
                            Random rand = new Random();

                            randomFailureLinks.AddRange(Enumerable.Range(1, linksCount).OrderBy(m => rand.Next()).Take((int)nudFixedFailuring.Value));
                            //Thread.Sleep(50);
                            //randomFailureLinks.Add(j);

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < randomFailureLinks.Count(); fl++)
                            {
                                currentFailureLinks += linkList[randomFailureLinks[fl] - 1].Index + "(" + linkList[randomFailureLinks[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[randomFailureLinks[fl] - 1].Index);
                            }

                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, j.ToString() + ".txt")))
                            {
                                //File.WriteAllText(Path.Combine(dirInfo.FullName, j.ToString() + ".txt"), "Failure Diameter: " + Diameter + "\n" + "Failure Links: " + currentFailureLinks + "\n");

                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                //StreamReader oReader;
                                //if (File.Exists(inputFilePath))
                                //{
                                //    string cSearforSomething = "Link";
                                //    oReader = new StreamReader(inputFilePath);
                                //    string cColl = oReader.ReadToEnd();
                                //    string cCriteria = @"\b" + cSearforSomething + @"\b";
                                //    System.Text.RegularExpressions.Regex oRegex = new
                                //    System.Text.RegularExpressions.Regex(cCriteria, RegexOptions.IgnoreCase);

                                //    var count = oRegex.Matches(cColl).AsQueryable();
                                //    Console.WriteLine(count.ToString());
                                //}
                                //Console.ReadLine();

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10();
                                    }

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, j.ToString() + ".rpt"),
                                                            Path.Combine(dirInfo.FullName, j.ToString() + ".out"));

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter);

                                        //////////////////////

                                        int nodeCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.NODE, ref nodeCount);

                                        int nodeIndex;
                                        int days = 0, hrs = 0, mins = 0, routeModel = 0;
                                        double t, vcf = 0;
                                        string nodeID = "";
                                        bool hasFlood = false;

                                        NodeFloodingSummary nodeFloodingSummary;

                                        nodeFloodingSummary = new NodeFloodingSummary()
                                        {
                                            isNoFlooding = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            NodeFloodsInfo = new List<NodeFlood>(),
                                        };

                                        for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                                        {
                                            int nodeType = -1;
                                            toolkitSWMM.GetNodeType(nodeIndex, ref nodeType);

                                            if ((NodeTypeEnum)nodeType == NodeTypeEnum.OUTFALL) continue;

                                            NodeStats nodeStats = new NodeStats();
                                            toolkitSWMM.GetNodeStats(nodeIndex, ref nodeStats);

                                            if (nodeStats.timeFlooded == 0.0) continue;

                                            t = Math.Max(0.01, nodeStats.timeFlooded);

                                            hasFlood = true;

                                            NodeFlood nodeFlood = new NodeFlood();

                                            nodeFlood.Index = nodeIndex;

                                            toolkitSWMM.GetObjectId((int)ObjectTypeEnum.NODE, nodeIndex, ref nodeID);
                                            nodeFlood.ID = nodeID;
                                            nodeFlood.HoursFlooded = t;

                                            nodeFlood.MaximumRate = nodeStats.maxOverflow;

                                            toolkitSWMM.GetElapsedTime(nodeStats.maxOverflowDate, ref days, ref hrs, ref mins);

                                            nodeFlood.TimeOfMaxOccurrence_days = days;
                                            nodeFlood.TimeOfMaxOccurrence_hrs = hrs;
                                            nodeFlood.TimeOfMaxOccurrence_mins = mins;

                                            toolkitSWMM.GetVcf(ref vcf);
                                            nodeFlood.TotalFloodVolume = nodeStats.volFlooded * vcf * 1.0e6; // w_MGAL to w_GAL

                                            toolkitSWMM.GetRouteModel(ref routeModel);

                                            if ((RouteModelTypeEnum)routeModel == RouteModelTypeEnum.DW)
                                            {
                                                double fullDepth = 0;
                                                toolkitSWMM.GetNodeParam(nodeIndex, (int)NodeProperty.FULLDEPTH, ref fullDepth);
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxDepth - fullDepth;
                                            }
                                            else
                                            {
                                                //? / 1000
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxPondedVol / 1000.0;
                                            }

                                            nodeFlood.NodeStats = nodeStats;

                                            myReportFile.WriteLine("NodeFlooding," + nodeFlood.Index + "," + nodeFlood.ID + "," +
                                                nodeFlood.HoursFlooded.ToString("0.###") + "," + nodeFlood.MaximumRate.ToString("0.###") + "," +
                                                nodeFlood.TimeOfMaxOccurrence_days + "," + nodeFlood.TimeOfMaxOccurrence_hrs + "," + nodeFlood.TimeOfMaxOccurrence_mins + "," +
                                                nodeFlood.TotalFloodVolume.ToString("0.###") + "," + nodeFlood.MaximumPondedVolume.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.avgDepth.ToString("0.###") + "," + nodeFlood.NodeStats.maxDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.maxDepthDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxRptDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.volFlooded.ToString("0.###") + "," + nodeFlood.NodeStats.timeFlooded.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.timeSurcharged.ToString("0.###") + "," + nodeFlood.NodeStats.timeCourantCritical.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.totLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflow.ToString("0.###") +
                                                nodeFlood.NodeStats.maxOverflow.ToString("0.###") + "," + nodeFlood.NodeStats.maxPondedVol.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflowDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxOverflowDate.ToLongTimeString());

                                            nodeFloodingSummary.NodeFloodsInfo.Add(nodeFlood);
                                        }

                                        if (!hasFlood)
                                        {
                                            nodeFloodingSummary.isNoFlooding = true;
                                        }

                                        //////////////////

                                        string linkID = "";

                                        LinkSurchargeSummary linkSurchargeSummary;

                                        linkSurchargeSummary = new LinkSurchargeSummary()
                                        {
                                            isNoSurcharge = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            LinkSurchargeInfo = new List<LinkSurcharge>(),
                                        };

                                        double[] ts = new double[5];
                                        int linkCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.LINK, ref linkCount);

                                        int linkIndex;
                                        bool hasSurcharge = false;

                                        //for (linkIndex = 0; linkIndex < linkCount; linkIndex++)
                                        //{
                                        //    int linkType = 0;
                                        //    int xsectType = 0;
                                        //    LinkStats linkStats = new LinkStats();

                                        //    toolkitSWMM.GetLinkStats(linkIndex, ref linkStats);
                                        //    toolkitSWMM.GetLinkType(linkIndex, ref linkType);
                                        //    toolkitSWMM.GetLinkXsectType(linkIndex, ref xsectType);

                                        //    if ((LinkTypeEnum)linkType != LinkTypeEnum.CONDUIT || (XSectionTypeEnum)xsectType == XSectionTypeEnum.DUMMY) continue;

                                        //    ts[0] = linkStats.timeSurcharged;
                                        //    ts[1] = linkStats.timeFullUpstream;
                                        //    ts[2] = linkStats.timeFullDnstream;
                                        //    ts[3] = linkStats.timeFullFlow;

                                        //    if (ts[0] + ts[1] + ts[2] + ts[3] == 0.0)
                                        //    {
                                        //        continue;
                                        //    }
                                        //    ts[4] = linkStats.timeCapacityLimited;

                                        //    for (int ts_index = 0; ts_index < 5; ts_index++)
                                        //        ts[ts_index] = Math.Max(0.01, ts[ts_index]);

                                        //    hasSurcharge = true;

                                        //    LinkSurcharge linkSurcharge = new LinkSurcharge();

                                        //    linkSurcharge.Index = linkIndex;

                                        //    toolkitSWMM.GetObjectId((int)ObjectTypeEnum.LINK, linkIndex, ref linkID);

                                        //    linkSurcharge.ID = linkID;

                                        //    linkSurcharge.BothEnds = ts[0];
                                        //    linkSurcharge.Upstream = ts[1];
                                        //    linkSurcharge.Dnstream = ts[2];
                                        //    linkSurcharge.HoursAboveFullNormalFlow = ts[3];
                                        //    linkSurcharge.HoursCapacityLimited = ts[4];

                                        //    linkSurcharge.LinkStats = linkStats;

                                        //    myReportFile.WriteLine("LinkSurcharge," + linkSurcharge.Index + "," + linkSurcharge.ID + "," + linkSurcharge.BothEnds.ToString("0.###") + "," + linkSurcharge.Upstream.ToString("0.###") + "," + linkSurcharge.Dnstream.ToString("0.###") + "," +
                                        //        linkSurcharge.HoursAboveFullNormalFlow.ToString("0.###") + "," + linkSurcharge.HoursCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlowDate.ToLongTimeString() + "," +
                                        //        linkSurcharge.LinkStats.maxVeloc.ToString("0.###") + "," + linkSurcharge.LinkStats.maxVeloc.ToString("0.###") + "," + linkSurcharge.LinkStats.maxDepth.ToString("0.###") + "," + linkSurcharge.LinkStats.timeNormalFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeInletControl.ToString("0.###") + "," + linkSurcharge.LinkStats.timeSurcharged.ToString("0.###") + "," +
                                        //        linkSurcharge.LinkStats.timeFullUpstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullDnstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCourantCritical.ToString("0.###") + "," +
                                        //        linkSurcharge.LinkStats.flowTurns + "," + linkSurcharge.LinkStats.flowTurnSign);

                                        //    linkSurchargeSummary.LinkSurchargeInfo.Add(linkSurcharge);
                                        //}

                                        //if (!hasSurcharge)
                                        //{
                                        //    linkSurchargeSummary.isNoSurcharge = true;
                                        //}

                                        //Summary.Add(new Tuple<NodeFloodingSummary, LinkSurchargeSummary>(nodeFloodingSummary, linkSurchargeSummary));
                                        //////////////////////////////
                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();
                                    // Close the system
                                    toolkitSWMM.Close();
                                    //return error_getCode(ErrorCode);
                                }
                                catch (Exception ex)
                                {
                                    // Log error.
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error.
                    }
                    finally
                    {
                        lock (toolkitSWMMIndexes)
                        {
                            toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                        }

                        s.Release();
                    }
                });

                th.Name = "Failure scenario: " + i;
                th.IsBackground = true;
                th.Priority = ThreadPriority.AboveNormal;
                //th.SetApartmentState(ApartmentState.STA);
                th.Start();
            }

            btnStartFixed.Text = "Start";
            btnStartFixed.Refresh();
        }



        private void btnStartConsecutive_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartConsecutive.Text = "Running";
            btnStartConsecutive.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                (chkMainDiameter.Checked && lines[li + line + 2].Split(' ')[0].Contains("M")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            List<ReportSummary> Summary = new List<ReportSummary>();

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);

            List<int> toolkitSWMMIndexes = new List<int>();
            for (int t = 0; t < cores; t++)
            {
                toolkitSWMMIndexes.Add(0);
            }
            //ISWMMToolkit[] toolkitSWMMs = new ISWMMToolkit[cores];

            Random randConsecutive = new Random();

            List<List<int>> randomFailureLinksConsecutiveList = new List<List<int>>();

            for (int i = 0; i < (int)nudConsecutiveFailuring.Value; i++)
            {
                List<int> randomFailureLinksConsecutive = new List<int>();
                randomFailureLinksConsecutive.AddRange(Enumerable.Range(1, linksCount).OrderBy(m => randConsecutive.Next()).Take(linksCount));

                randomFailureLinksConsecutiveList.Add(randomFailureLinksConsecutive);
            }

            for (int i_index = 1; i_index <= linksCount; i_index++)
            {
                int i = i_index;
                s.WaitOne();

                Thread th = new Thread(delegate ()
                {
                    int failureLinkCount = i;
                    int toolkitSWMMIndex = -1;

                    try
                    {
                        lock (toolkitSWMMIndexes)  //for lock thread
                        {
                            toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                            toolkitSWMMIndexes[toolkitSWMMIndex] = failureLinkCount;
                        }


                        //if (InvokeRequired)
                        //{
                        //    Invoke((Action)delegate
                        //    {
                        //        lblScenarioCounter.Text = "# Failure Links: " + failureLinkCount;
                        //            //lblScenarioCounter.Refresh();
                        //        });
                        //}

                        var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                        int failuringNumber = (int)nudConsecutiveFailuring.Value;

                        //if (failureLinkCount == 1)
                        //    failuringNumber = 10;
                        //else if (failureLinkCount == 2 || failureLinkCount == linksCount - 2)
                        //    failuringNumber = 10;
                        //else if (failureLinkCount == linksCount - 1)
                        //    failuringNumber = 10;
                        //else if (failureLinkCount == linksCount)
                        //    failuringNumber = 10;
                        //else
                        //    failuringNumber = 10;



                        for (int j = 1; j <= failuringNumber; j++)
                        {
                            List<int> randomFailureLinks = new List<int>();
                            List<int> failureLinkIndexes = new List<int>();
                            Random rand = new Random();

                            //randomFailureLinks.AddRange(Enumerable.Range(1, linksCount).OrderBy(m => rand.Next()).Take(failureLinkCount));

                            for (int i_Consecutive = 0; i_Consecutive < failureLinkCount; i_Consecutive++)
                            {
                                randomFailureLinks.Add(randomFailureLinksConsecutiveList[j - 1][i_Consecutive]);
                            }

                            //Thread.Sleep(50);
                            //randomFailureLinks.Add(j);

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < randomFailureLinks.Count(); fl++)
                            {
                                currentFailureLinks += linkList[randomFailureLinks[fl] - 1].Index + "(" + linkList[randomFailureLinks[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[randomFailureLinks[fl] - 1].Index);
                            }

                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, j.ToString() + ".txt")))
                            {
                                //File.WriteAllText(Path.Combine(dirInfo.FullName, j.ToString() + ".txt"), "Failure Diameter: " + Diameter + "\n" + "Failure Links: " + currentFailureLinks + "\n");

                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                //StreamReader oReader;
                                //if (File.Exists(inputFilePath))
                                //{
                                //    string cSearforSomething = "Link";
                                //    oReader = new StreamReader(inputFilePath);
                                //    string cColl = oReader.ReadToEnd();
                                //    string cCriteria = @"\b" + cSearforSomething + @"\b";
                                //    System.Text.RegularExpressions.Regex oRegex = new
                                //    System.Text.RegularExpressions.Regex(cCriteria, RegexOptions.IgnoreCase);

                                //    var count = oRegex.Matches(cColl).AsQueryable();
                                //    Console.WriteLine(count.ToString());
                                //}
                                //Console.ReadLine();

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10();
                                    }

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, j.ToString() + ".rpt"),
                                                            Path.Combine(dirInfo.FullName, j.ToString() + ".out"));

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter);

                                        //////////////////////

                                        RoutingTotals routingTotals = new RoutingTotals();

                                        ErrorCode = toolkitSWMM.GetSystemRoutingStats(ref routingTotals);

                                        myReportFile.WriteLine("Dry Weather Inflow (gal)," + routingTotals.dwInflow * 1.0e6); // w_MGAL to w_GAL
                                        myReportFile.WriteLine("Flooding Loss (gal)," + routingTotals.flooding * 1.0e6); // w_MGAL to w_GAL

                                        //////////////////////

                                        int nodeCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.NODE, ref nodeCount);

                                        int nodeIndex;
                                        int days = 0, hrs = 0, mins = 0, routeModel = 0;
                                        double t, vcf = 0;
                                        string nodeID = "";
                                        bool hasFlood = false;

                                        NodeFloodingSummary nodeFloodingSummary;

                                        nodeFloodingSummary = new NodeFloodingSummary()
                                        {
                                            isNoFlooding = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            NodeFloodsInfo = new List<NodeFlood>(),
                                        };

                                        double sumHoursNodalFlooding = 0;
                                        int numNodalFlooding = 0;

                                        for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                                        {
                                            int nodeType = -1;
                                            toolkitSWMM.GetNodeType(nodeIndex, ref nodeType);

                                            if ((NodeTypeEnum)nodeType == NodeTypeEnum.OUTFALL) continue;

                                            NodeStats nodeStats = new NodeStats();
                                            toolkitSWMM.GetNodeStats(nodeIndex, ref nodeStats);

                                            if (nodeStats.timeFlooded == 0.0) continue;

                                            t = Math.Max(0.01, nodeStats.timeFlooded);

                                            hasFlood = true;

                                            NodeFlood nodeFlood = new NodeFlood();

                                            nodeFlood.Index = nodeIndex;

                                            toolkitSWMM.GetObjectId((int)ObjectTypeEnum.NODE, nodeIndex, ref nodeID);
                                            nodeFlood.ID = nodeID;
                                            nodeFlood.HoursFlooded = t;

                                            nodeFlood.MaximumRate = nodeStats.maxOverflow;

                                            toolkitSWMM.GetElapsedTime(nodeStats.maxOverflowDate, ref days, ref hrs, ref mins);

                                            nodeFlood.TimeOfMaxOccurrence_days = days;
                                            nodeFlood.TimeOfMaxOccurrence_hrs = hrs;
                                            nodeFlood.TimeOfMaxOccurrence_mins = mins;

                                            toolkitSWMM.GetVcf(ref vcf);
                                            nodeFlood.TotalFloodVolume = nodeStats.volFlooded * vcf * 1.0e6; // w_MGAL to w_GAL (VolUnitsWords)

                                            toolkitSWMM.GetRouteModel(ref routeModel);

                                            if ((RouteModelTypeEnum)routeModel == RouteModelTypeEnum.DW)
                                            {
                                                double fullDepth = 0;
                                                toolkitSWMM.GetNodeParam(nodeIndex, (int)NodeProperty.FULLDEPTH, ref fullDepth);
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxDepth - fullDepth;
                                            }
                                            else
                                            {
                                                //? / 1000
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxPondedVol / 1000.0;
                                            }

                                            nodeFlood.NodeStats = nodeStats;

                                            myReportFile.WriteLine("NodeFlooding," + nodeFlood.Index + "," + nodeFlood.ID + "," +
                                                nodeFlood.HoursFlooded.ToString("0.###") + "," + nodeFlood.MaximumRate.ToString("0.###") + "," +
                                                nodeFlood.TimeOfMaxOccurrence_days + "," + nodeFlood.TimeOfMaxOccurrence_hrs + "," + nodeFlood.TimeOfMaxOccurrence_mins + "," +
                                                nodeFlood.TotalFloodVolume.ToString("0.###") + "," + nodeFlood.MaximumPondedVolume.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.avgDepth.ToString("0.###") + "," + nodeFlood.NodeStats.maxDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.maxDepthDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxRptDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.volFlooded.ToString("0.###") + "," + nodeFlood.NodeStats.timeFlooded.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.timeSurcharged.ToString("0.###") + "," + nodeFlood.NodeStats.timeCourantCritical.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.totLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflow.ToString("0.###") +
                                                nodeFlood.NodeStats.maxOverflow.ToString("0.###") + "," + nodeFlood.NodeStats.maxPondedVol.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflowDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxOverflowDate.ToLongTimeString());

                                            nodeFloodingSummary.NodeFloodsInfo.Add(nodeFlood);

                                            sumHoursNodalFlooding += nodeFlood.HoursFlooded;
                                            if (nodeFlood.HoursFlooded > 0)
                                            {
                                                numNodalFlooding++;
                                            }

                                        }

                                        if (!hasFlood)
                                        {
                                            nodeFloodingSummary.isNoFlooding = true;
                                        }

                                        //////////////////

                                        string linkID = "";

                                        LinkSurchargeSummary linkSurchargeSummary;

                                        linkSurchargeSummary = new LinkSurchargeSummary()
                                        {
                                            isNoSurcharge = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            LinkSurchargeInfo = new List<LinkSurcharge>(),
                                        };

                                        //double[] ts = new double[5];
                                        //int linkCount = 0;
                                        //toolkitSWMM.GetObjectCount(ObjectTypeEnum.LINK, ref linkCount);

                                        //int linkIndex;
                                        //bool hasSurcharge = false;

                                        //for (linkIndex = 0; linkIndex < linkCount; linkIndex++)
                                        //{
                                        //    int linkType = 0;
                                        //    int xsectType = 0;
                                        //    LinkStats linkStats = new LinkStats();

                                        //    toolkitSWMM.GetLinkStats(linkIndex, ref linkStats);
                                        //    toolkitSWMM.GetLinkType(linkIndex, ref linkType);
                                        //    toolkitSWMM.GetLinkXsectType(linkIndex, ref xsectType);

                                        //    if ((LinkTypeEnum)linkType != LinkTypeEnum.CONDUIT || (XSectionTypeEnum)xsectType == XSectionTypeEnum.DUMMY) continue;

                                        //    ts[0] = linkStats.timeSurcharged;
                                        //    ts[1] = linkStats.timeFullUpstream;
                                        //    ts[2] = linkStats.timeFullDnstream;
                                        //    ts[3] = linkStats.timeFullFlow;

                                        //    if (ts[0] + ts[1] + ts[2] + ts[3] == 0.0)
                                        //    {
                                        //        continue;
                                        //    }
                                        //    ts[4] = linkStats.timeCapacityLimited;

                                        //    for (int ts_index = 0; ts_index < 5; ts_index++)
                                        //        ts[ts_index] = Math.Max(0.01, ts[ts_index]);

                                        //    hasSurcharge = true;

                                        //    LinkSurcharge linkSurcharge = new LinkSurcharge();

                                        //    linkSurcharge.Index = linkIndex;

                                        //    toolkitSWMM.GetObjectId((int)ObjectTypeEnum.LINK, linkIndex, ref linkID);

                                        //    linkSurcharge.ID = linkID;

                                        //    linkSurcharge.BothEnds = ts[0];
                                        //    linkSurcharge.Upstream = ts[1];
                                        //    linkSurcharge.Dnstream = ts[2];
                                        //    linkSurcharge.HoursAboveFullNormalFlow = ts[3];
                                        //    linkSurcharge.HoursCapacityLimited = ts[4];

                                        //    linkSurcharge.LinkStats = linkStats;

                                        //    myReportFile.WriteLine("LinkSurcharge," + linkSurcharge.Index + "," + linkSurcharge.ID + "," + linkSurcharge.BothEnds.ToString("0.###") + "," + linkSurcharge.Upstream.ToString("0.###") + "," + linkSurcharge.Dnstream.ToString("0.###") + "," +
                                        //        linkSurcharge.HoursAboveFullNormalFlow.ToString("0.###") + "," + linkSurcharge.HoursCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlowDate.ToLongTimeString() + "," +
                                        //        linkSurcharge.LinkStats.maxVeloc.ToString("0.###") + "," + linkSurcharge.LinkStats.maxVeloc.ToString("0.###") + "," + linkSurcharge.LinkStats.maxDepth.ToString("0.###") + "," + linkSurcharge.LinkStats.timeNormalFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeInletControl.ToString("0.###") + "," + linkSurcharge.LinkStats.timeSurcharged.ToString("0.###") + "," +
                                        //        linkSurcharge.LinkStats.timeFullUpstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullDnstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCourantCritical.ToString("0.###") + "," +
                                        //        linkSurcharge.LinkStats.flowTurns + "," + linkSurcharge.LinkStats.flowTurnSign);

                                        //    linkSurchargeSummary.LinkSurchargeInfo.Add(linkSurcharge);
                                        //}

                                        //if (!hasSurcharge)
                                        //{
                                        //    linkSurchargeSummary.isNoSurcharge = true;
                                        //}


                                        ////////////////////////
                                        if (numNodalFlooding != 0)
                                        {
                                            double meanNodalFlooding = sumHoursNodalFlooding / numNodalFlooding;

                                            myReportFile.WriteLine("Sum Hours Nodal Flooding," + sumHoursNodalFlooding);
                                            myReportFile.WriteLine("Mean Hours Nodal Flooding," + meanNodalFlooding);
                                        }



                                        Summary.Add(new ReportSummary()
                                        {
                                            RoutingTotals = routingTotals,
                                            NodeFloodingSummary = nodeFloodingSummary,
                                            LinkSurchargeSummary = linkSurchargeSummary,
                                        });
                                        //////////////////////////////
                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();
                                    // Close the system
                                    toolkitSWMM.Close();
                                    //return error_getCode(ErrorCode);
                                }
                                catch (Exception ex)
                                {
                                    // Log error.
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error.
                    }
                    finally
                    {
                        lock (toolkitSWMMIndexes)
                        {
                            toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                        }

                        s.Release();
                    }
                });

                th.Name = "Failure scenario: " + i;
                th.IsBackground = true;
                th.Priority = ThreadPriority.AboveNormal;
                //th.SetApartmentState(ApartmentState.STA);
                th.Start();
            }

            btnStartConsecutive.Text = "Start";
            btnStartConsecutive.Refresh();
        }

        private void btnStartTarget_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartTarget.Text = "Running";
            btnStartTarget.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> AllLinkList = new List<Link>();
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                            AllLinkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>> Summary = new List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>>();

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);


            //ISWMMToolkit[] toolkitSWMMs = new ISWMMToolkit[cores];

            List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>> FolderSummary = new List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>>();
            List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>> TopFolderSummary = new List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>>();

            int topFailuringCount = (int)nudTopFailuring.Value;

            ManualResetEvent waitEndLoop = new ManualResetEvent(true);

            for (int i = 1; i <= linksCount; i++)
            {
                int failureLinkCount = i;

                if (failureLinkCount != 1)
                {
                    TopFolderSummary = FolderSummary.OrderByDescending(x => x.Item1.SumTotalFloodVolume).Take(topFailuringCount).ToList();
                }
                FolderSummary.Clear();

                List<int> toolkitSWMMIndexes = new List<int>();
                for (int t = 0; t < cores; t++)
                {
                    toolkitSWMMIndexes.Add(0);
                }

                //if (InvokeRequired)
                //{
                //    Invoke((Action)delegate
                //    {
                //        lblScenarioCounter.Text = "# Failure Links: " + failureLinkCount;
                //            //lblScenarioCounter.Refresh();
                //        });
                //}

                var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                int failuringNumber = 0;
                int targetNumber = (int)nudTargetNumber.Value;

                if (failureLinkCount == 1)
                    failuringNumber = linksCount;
                else
                    failuringNumber = targetNumber;

                waitEndLoop.Reset();

                int FileCountInFolder = 0;

                for (int j_index = 1; j_index <= failuringNumber; j_index++)
                {
                    int j = j_index;
                    s.WaitOne();

                    Thread th = new Thread(delegate ()
                    {
                        int failuringCounter = j;
                        int toolkitSWMMIndex = -1;

                        try
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                                toolkitSWMMIndexes[toolkitSWMMIndex] = failuringCounter;
                            }

                            List<int> randomFailureLinks = new List<int>();
                            List<int> failureLinkIndexes = new List<int>();
                            Random rand = new Random();
                            randomFailureLinks.AddRange(Enumerable.Range(1, linksCount).OrderBy(m => rand.Next()).Take(failureLinkCount));

                            Random randTop = new Random();
                            Tuple<NodeFloodingSummary, LinkSurchargeSummary> selectedTop;
                            List<int> randomFailureLinksWithTop = new List<int>();

                            if (failureLinkCount != 1)
                            {
                                selectedTop = TopFolderSummary.ElementAt(randTop.Next(TopFolderSummary.Count()));

                                foreach (var failureLinkIndex in selectedTop.Item1.FailureLinksIndex)
                                {
                                    int failureLinkIndexInLinkList = linkList.FindIndex(x => x.ID == AllLinkList[failureLinkIndex].ID);
                                    randomFailureLinksWithTop.Add(failureLinkIndexInLinkList + 1);
                                }

                                foreach (var randomFailureLink in randomFailureLinks)
                                {
                                    if (!randomFailureLinksWithTop.Contains(randomFailureLink))
                                    {
                                        randomFailureLinksWithTop.Add(randomFailureLink);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                randomFailureLinksWithTop = randomFailureLinks;
                            }

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < randomFailureLinksWithTop.Count(); fl++)
                            {
                                currentFailureLinks += linkList[randomFailureLinksWithTop[fl] - 1].Index + "(" + linkList[randomFailureLinksWithTop[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[randomFailureLinksWithTop[fl] - 1].Index);
                            }

                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".txt")))
                            {
                                //File.WriteAllText(Path.Combine(dirInfo.FullName, j.ToString() + ".txt"), "Failure Diameter: " + Diameter + "\n" + "Failure Links: " + currentFailureLinks + "\n");

                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                //StreamReader oReader;
                                //if (File.Exists(inputFilePath))
                                //{
                                //    string cSearforSomething = "Link";
                                //    oReader = new StreamReader(inputFilePath);
                                //    string cColl = oReader.ReadToEnd();
                                //    string cCriteria = @"\b" + cSearforSomething + @"\b";
                                //    System.Text.RegularExpressions.Regex oRegex = new
                                //    System.Text.RegularExpressions.Regex(cCriteria, RegexOptions.IgnoreCase);

                                //    var count = oRegex.Matches(cColl).AsQueryable();
                                //    Console.WriteLine(count.ToString());
                                //}
                                //Console.ReadLine();

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10();
                                    }

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt"),
                                                            Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".out"));

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter);

                                        //////////////////////

                                        int nodeCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.NODE, ref nodeCount);

                                        int nodeIndex;
                                        int days = 0, hrs = 0, mins = 0, routeModel = 0;
                                        double t, vcf = 0;
                                        string nodeID = "";
                                        bool hasFlood = false;

                                        NodeFloodingSummary nodeFloodingSummary;

                                        nodeFloodingSummary = new NodeFloodingSummary()
                                        {
                                            isNoFlooding = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            NodeFloodsInfo = new List<NodeFlood>(),
                                        };

                                        for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                                        {
                                            int nodeType = -1;
                                            toolkitSWMM.GetNodeType(nodeIndex, ref nodeType);

                                            if ((NodeTypeEnum)nodeType == NodeTypeEnum.OUTFALL) continue;

                                            NodeStats nodeStats = new NodeStats();
                                            toolkitSWMM.GetNodeStats(nodeIndex, ref nodeStats);

                                            if (nodeStats.timeFlooded == 0.0) continue;

                                            t = Math.Max(0.01, nodeStats.timeFlooded);

                                            hasFlood = true;

                                            NodeFlood nodeFlood = new NodeFlood();

                                            nodeFlood.Index = nodeIndex;

                                            toolkitSWMM.GetObjectId((int)ObjectTypeEnum.NODE, nodeIndex, ref nodeID);
                                            nodeFlood.ID = nodeID;
                                            nodeFlood.HoursFlooded = t;

                                            nodeFlood.MaximumRate = nodeStats.maxOverflow;

                                            toolkitSWMM.GetElapsedTime(nodeStats.maxOverflowDate, ref days, ref hrs, ref mins);

                                            nodeFlood.TimeOfMaxOccurrence_days = days;
                                            nodeFlood.TimeOfMaxOccurrence_hrs = hrs;
                                            nodeFlood.TimeOfMaxOccurrence_mins = mins;

                                            toolkitSWMM.GetVcf(ref vcf);
                                            nodeFlood.TotalFloodVolume = nodeStats.volFlooded * vcf * 1.0e6; // w_MGAL to w_GAL

                                            toolkitSWMM.GetRouteModel(ref routeModel);

                                            if ((RouteModelTypeEnum)routeModel == RouteModelTypeEnum.DW)
                                            {
                                                double fullDepth = 0;
                                                toolkitSWMM.GetNodeParam(nodeIndex, (int)NodeProperty.FULLDEPTH, ref fullDepth);
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxDepth - fullDepth;
                                            }
                                            else
                                            {
                                                //? / 1000
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxPondedVol / 1000.0;
                                            }

                                            nodeFlood.NodeStats = nodeStats;

                                            myReportFile.WriteLine("NodeFlooding," + nodeFlood.Index + "," + nodeFlood.ID + "," +
                                                    nodeFlood.HoursFlooded.ToString("0.###") + "," + nodeFlood.MaximumRate.ToString("0.###") + "," +
                                                    nodeFlood.TimeOfMaxOccurrence_days + "," + nodeFlood.TimeOfMaxOccurrence_hrs + "," + nodeFlood.TimeOfMaxOccurrence_mins + "," +
                                                    nodeFlood.TotalFloodVolume.ToString("0.###") + "," + nodeFlood.MaximumPondedVolume.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.avgDepth.ToString("0.###") + "," + nodeFlood.NodeStats.maxDepth.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.maxDepthDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxRptDepth.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.volFlooded.ToString("0.###") + "," + nodeFlood.NodeStats.timeFlooded.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.timeSurcharged.ToString("0.###") + "," + nodeFlood.NodeStats.timeCourantCritical.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.totLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflow.ToString("0.###") +
                                                    nodeFlood.NodeStats.maxOverflow.ToString("0.###") + "," + nodeFlood.NodeStats.maxPondedVol.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflowDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxOverflowDate.ToLongTimeString());

                                            nodeFloodingSummary.NodeFloodsInfo.Add(nodeFlood);
                                            nodeFloodingSummary.SumTotalFloodVolume += nodeFlood.TotalFloodVolume;

                                        }

                                        if (!hasFlood)
                                        {
                                            nodeFloodingSummary.isNoFlooding = true;
                                        }

                                        //////////////////

                                        string linkID = "";

                                        LinkSurchargeSummary linkSurchargeSummary;

                                        linkSurchargeSummary = new LinkSurchargeSummary()
                                        {
                                            isNoSurcharge = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            LinkSurchargeInfo = new List<LinkSurcharge>(),
                                        };

                                        double[] ts = new double[5];
                                        int linkCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.LINK, ref linkCount);

                                        int linkIndex;
                                        bool hasSurcharge = false;

                                        for (linkIndex = 0; linkIndex < linkCount; linkIndex++)
                                        {
                                            int linkType = 0;
                                            int xsectType = 0;
                                            LinkStats linkStats = new LinkStats();

                                            toolkitSWMM.GetLinkStats(linkIndex, ref linkStats);
                                            toolkitSWMM.GetLinkType(linkIndex, ref linkType);
                                            toolkitSWMM.GetLinkXsectType(linkIndex, ref xsectType);

                                            if ((LinkTypeEnum)linkType != LinkTypeEnum.CONDUIT || (XSectionTypeEnum)xsectType == XSectionTypeEnum.DUMMY) continue;

                                            ts[0] = linkStats.timeSurcharged;
                                            ts[1] = linkStats.timeFullUpstream;
                                            ts[2] = linkStats.timeFullDnstream;
                                            ts[3] = linkStats.timeFullFlow;

                                            if (ts[0] + ts[1] + ts[2] + ts[3] == 0.0) continue;

                                            ts[4] = linkStats.timeCapacityLimited;

                                            for (int ts_index = 0; ts_index < 5; ts_index++)
                                                ts[ts_index] = Math.Max(0.01, ts[ts_index]);

                                            hasSurcharge = true;

                                            LinkSurcharge linkSurcharge = new LinkSurcharge();

                                            linkSurcharge.Index = linkIndex;

                                            toolkitSWMM.GetObjectId((int)ObjectTypeEnum.LINK, linkIndex, ref linkID);
                                            linkSurcharge.ID = linkID;

                                            linkSurcharge.BothEnds = ts[0];
                                            linkSurcharge.Upstream = ts[1];
                                            linkSurcharge.Dnstream = ts[2];
                                            linkSurcharge.HoursAboveFullNormalFlow = ts[3];
                                            linkSurcharge.HoursCapacityLimited = ts[4];

                                            linkSurcharge.LinkStats = linkStats;

                                            myReportFile.WriteLine("LinkSurcharge," + linkSurcharge.Index + "," + linkSurcharge.ID + "," + linkSurcharge.BothEnds.ToString("0.###") + "," + linkSurcharge.Upstream.ToString("0.###") + "," + linkSurcharge.Dnstream.ToString("0.###") + "," +
                                                    linkSurcharge.HoursAboveFullNormalFlow.ToString("0.###") + "," + linkSurcharge.HoursCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlowDate.ToLongTimeString() + "," +
                                                    linkSurcharge.LinkStats.maxVeloc.ToString("0.###") + "," + linkSurcharge.LinkStats.maxDepth.ToString("0.###") + "," + linkSurcharge.LinkStats.timeNormalFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeInletControl.ToString("0.###") + "," + linkSurcharge.LinkStats.timeSurcharged.ToString("0.###") + "," +
                                                    linkSurcharge.LinkStats.timeFullUpstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullDnstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCourantCritical.ToString("0.###") + "," +
                                                    linkSurcharge.LinkStats.flowTurns + "," + linkSurcharge.LinkStats.flowTurnSign);

                                            linkSurchargeSummary.LinkSurchargeInfo.Add(linkSurcharge);
                                        }

                                        if (!hasSurcharge)
                                        {
                                            linkSurchargeSummary.isNoSurcharge = true;
                                        }

                                        Summary.Add(new Tuple<NodeFloodingSummary, LinkSurchargeSummary>(nodeFloodingSummary, linkSurchargeSummary));
                                        FolderSummary.Add(new Tuple<NodeFloodingSummary, LinkSurchargeSummary>(nodeFloodingSummary, linkSurchargeSummary));

                                        //////////////////////////////
                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();

                                    // Close the system
                                    toolkitSWMM.Close();

                                    //return error_getCode(ErrorCode);
                                }
                                catch (Exception ex)
                                {
                                    // Log error.
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            // Log error.
                        }
                        finally
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                                FileCountInFolder++;

                                if (FileCountInFolder == failuringNumber)
                                {
                                    waitEndLoop.Set();
                                }
                            }

                            s.Release();
                        }

                    });

                    th.Name = "Failure scenario: " + i;
                    th.IsBackground = true;
                    th.Priority = ThreadPriority.AboveNormal;
                    //th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                }

                waitEndLoop.WaitOne(60 * 60 * 1000, false);
            }

            btnStartTarget.Text = "Start";
            btnStartTarget.Refresh();
        }


        private void btnDeviationAnalysis_Click(object sender, EventArgs e)
        {
            // on root folder of deviation results level 
            string[] ResultsDeviationDirectories = Directory.GetDirectories(txtResultsDeviationDirectory.Text).OrderBy(x => x, new OrdinalStringComparer()).ToArray();
            List<List<double>> meanFloodingLossPerLevel = new List<List<double>>();

            for (int i = 0; i < ResultsDeviationDirectories.Length; i++)
            {
                string[] ResultsDirectories = Directory.GetDirectories(ResultsDeviationDirectories[i]).OrderBy(x => x, new OrdinalStringComparer()).ToArray();

                meanFloodingLossPerLevel.Add(new List<double>());

                for (int j = 0; j < ResultsDirectories.Length; j++)
                {
                    lblDeviationAnalysis.Text = "# Scenario: " + (i + 1) + ", # Failuring: " + (j + 1);
                    lblDeviationAnalysis.Refresh();

                    string[] ResultsFiles = Directory.GetFiles(Path.Combine(ResultsDirectories[j]), "*.txt").OrderBy(x => x, new OrdinalStringComparer()).ToArray();

                    double sumFloodingLoss = 0;
                    double meanFloodingLoss = 0;

                    foreach (var file in ResultsFiles)
                    {
                        string[] lines = File.ReadAllLines(file, Encoding.UTF8);

                        for (int k = 0; k < lines.Count(); k++)
                        {
                            string[] data = lines[k].Split(',');

                            switch (data[0])
                            {
                                case "Flooding Loss (gal)":
                                    sumFloodingLoss += Convert.ToDouble(data[1]);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    meanFloodingLoss = sumFloodingLoss / ResultsFiles.Length;

                    meanFloodingLossPerLevel[i].Add(meanFloodingLoss);
                }
            }

            for (int i = 1; i < meanFloodingLossPerLevel.Count; i++)
            {
                for (int j = 0; j < meanFloodingLossPerLevel[i].Count; j++)
                {
                    double deviation = ((meanFloodingLossPerLevel[i][j] - meanFloodingLossPerLevel[i - 1][j]) / meanFloodingLossPerLevel[i - 1][j]) * 100;
                    chtTotalFloodDeviation.Series["Series" + i].Points.AddXY(j, deviation);
                    chtTotalFloodDeviation.Update();
                }

            }

        }

        private void btnResultsDeviationDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtResultsDeviationDirectory.Text = folderDlg.SelectedPath;
            }
        }

        private static string formatFileNumberForSort(string inVal)
        {
            int o;
            if (int.TryParse(Path.GetFileName(inVal), out o))
            {
                Console.WriteLine(string.Format("{0:0000000000}", o));
                return string.Format("{0:0000000000}", o);
            }
            else
                return inVal;
        }

        private void btnStartGeneticAlgorithm_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartGeneticAlgorithm.Text = "Running";
            btnStartGeneticAlgorithm.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> AllLinkList = new List<Link>();
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                            AllLinkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();


            RandomizationProvider.Current = new BasicRandomization();

            var selection = new EliteSelection();
            var crossover = new UniformCrossover();
            var mutation = new TworsMutation();
            var chromosome = new SwmmChromosome(10);
            var fitness = new FuncFitness((c) =>
            {
                return ((SwmmChromosome)c).TotalFlood;
            });

            var population = new Population(5, 10, chromosome);

            var ga = new SwmmGeneticAlgorithm(population, fitness, selection, crossover, mutation);

            ga.cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            ga.inputFilePath = inputFilePath;
            ga.outputFolderPath = outputFolderPath;
            ga.linkList = linkList;
            ga.Diameter = Diameter;
            ga.failureStartTime = nudFailureStartTime.Value;
            ga.failureEndTime = nudFailureEndTime.Value;
            ga.failuringRoughness = nudFailuringRoughness.Value;

            ga.TaskExecutor = new ParallelTaskExecutor()
            {
                MinThreads = 10,
                MaxThreads = 20
            };

            ga.Termination = new GenerationNumberTermination(100);
            ga.Start();

            var bestC = ga.BestChromosome as SwmmChromosome;

            btnStartGeneticAlgorithm.Text = "Start";
            btnStartGeneticAlgorithm.Refresh();
        }

        static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetKCombs(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        private void btnStartCombination_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartCombination.Text = "Running";
            btnStartCombination.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> AllLinkList = new List<Link>();
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                            AllLinkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>> Summary = new List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>>();

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);


            List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>> FolderSummary = new List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>>();
            List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>> TopFolderSummary = new List<Tuple<NodeFloodingSummary, LinkSurchargeSummary>>();

            double topFailuringPercentage = (double)(nudTopFailuring.Value / 100);



            ManualResetEvent waitEndLoop = new ManualResetEvent(true);

            for (int i = 1; i <= linksCount; i++)
            {
                int failureLinkCount = i;

                if (failureLinkCount != 1)
                {
                    int topFailuringCount = (int)(topFailuringPercentage * FolderSummary.Count);
                    TopFolderSummary = FolderSummary.OrderByDescending(x => x.Item1.SumTotalFloodVolume).Take(topFailuringCount).ToList();
                }
                FolderSummary.Clear();

                List<int> toolkitSWMMIndexes = new List<int>();
                for (int t = 0; t < cores; t++)
                {
                    toolkitSWMMIndexes.Add(0);
                }

                //if (InvokeRequired)
                //{
                //    Invoke((Action)delegate
                //    {
                //        lblScenarioCounter.Text = "# Failure Links: " + failureLinkCount;
                //            //lblScenarioCounter.Refresh();
                //        });
                //}

                var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                List<int> topDistinctIndex = new List<int>();
                List<int> topDistinctIndexInLinkList = new List<int>();
                List<List<int>> targetCombinations = new List<List<int>>();

                if (failureLinkCount == 1)
                {
                    targetCombinations = Enumerable.Range(1, linksCount).Select(x => (new int[] { x }).ToList()).ToList();
                }
                else
                {
                    foreach (var folderSummary in TopFolderSummary)
                    {
                        topDistinctIndex.AddRange(folderSummary.Item1.FailureLinksIndex);
                    }
                    topDistinctIndex = topDistinctIndex.Distinct().ToList();

                    foreach (var failureLinkIndex in topDistinctIndex)
                    {
                        int failureLinkIndexInLinkList = linkList.FindIndex(x => x.ID == AllLinkList[failureLinkIndex].ID);
                        topDistinctIndexInLinkList.Add(failureLinkIndexInLinkList + 1);
                    }

                    if (failureLinkCount > topDistinctIndexInLinkList.Count)
                    {
                        break;
                    }
                    targetCombinations = GetKCombs(topDistinctIndexInLinkList, failureLinkCount).Select(x => x.ToList()).ToList();
                }


                int failuringNumber = targetCombinations.Count();

                waitEndLoop.Reset();

                int FileCountInFolder = 0;

                for (int j_index = 1; j_index <= failuringNumber; j_index++)
                {
                    int j = j_index;
                    s.WaitOne();

                    Thread th = new Thread(delegate ()
                    {
                        int failuringCounter = j;
                        int toolkitSWMMIndex = -1;

                        try
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                                toolkitSWMMIndexes[toolkitSWMMIndex] = failuringCounter;
                            }

                            List<int> failureLinkIndexes = new List<int>();
                            List<int> targetCombination = targetCombinations[failuringCounter - 1];

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < targetCombination.Count(); fl++)
                            {
                                currentFailureLinks += linkList[targetCombination[fl] - 1].Index + "(" + linkList[targetCombination[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[targetCombination[fl] - 1].Index);
                            }

                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".txt")))
                            {
                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10();
                                    }

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt"),
                                                            Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".out"));

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter);

                                        //////////////////////

                                        int nodeCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.NODE, ref nodeCount);

                                        int nodeIndex;
                                        int days = 0, hrs = 0, mins = 0, routeModel = 0;
                                        double t, vcf = 0;
                                        string nodeID = "";
                                        bool hasFlood = false;

                                        NodeFloodingSummary nodeFloodingSummary;

                                        nodeFloodingSummary = new NodeFloodingSummary()
                                        {
                                            isNoFlooding = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            NodeFloodsInfo = new List<NodeFlood>(),
                                        };

                                        for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                                        {
                                            int nodeType = -1;
                                            toolkitSWMM.GetNodeType(nodeIndex, ref nodeType);

                                            if ((NodeTypeEnum)nodeType == NodeTypeEnum.OUTFALL) continue;

                                            NodeStats nodeStats = new NodeStats();
                                            toolkitSWMM.GetNodeStats(nodeIndex, ref nodeStats);

                                            if (nodeStats.timeFlooded == 0.0) continue;

                                            t = Math.Max(0.01, nodeStats.timeFlooded);

                                            hasFlood = true;

                                            NodeFlood nodeFlood = new NodeFlood();

                                            nodeFlood.Index = nodeIndex;

                                            toolkitSWMM.GetObjectId((int)ObjectTypeEnum.NODE, nodeIndex, ref nodeID);
                                            nodeFlood.ID = nodeID;
                                            nodeFlood.HoursFlooded = t;

                                            nodeFlood.MaximumRate = nodeStats.maxOverflow;

                                            toolkitSWMM.GetElapsedTime(nodeStats.maxOverflowDate, ref days, ref hrs, ref mins);

                                            nodeFlood.TimeOfMaxOccurrence_days = days;
                                            nodeFlood.TimeOfMaxOccurrence_hrs = hrs;
                                            nodeFlood.TimeOfMaxOccurrence_mins = mins;

                                            toolkitSWMM.GetVcf(ref vcf);
                                            nodeFlood.TotalFloodVolume = nodeStats.volFlooded * vcf * 1.0e6; // w_MGAL to w_GAL

                                            toolkitSWMM.GetRouteModel(ref routeModel);

                                            if ((RouteModelTypeEnum)routeModel == RouteModelTypeEnum.DW)
                                            {
                                                double fullDepth = 0;
                                                toolkitSWMM.GetNodeParam(nodeIndex, (int)NodeProperty.FULLDEPTH, ref fullDepth);
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxDepth - fullDepth;
                                            }
                                            else
                                            {
                                                //? / 1000
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxPondedVol / 1000.0;
                                            }

                                            nodeFlood.NodeStats = nodeStats;

                                            myReportFile.WriteLine("NodeFlooding," + nodeFlood.Index + "," + nodeFlood.ID + "," +
                                                    nodeFlood.HoursFlooded.ToString("0.###") + "," + nodeFlood.MaximumRate.ToString("0.###") + "," +
                                                    nodeFlood.TimeOfMaxOccurrence_days + "," + nodeFlood.TimeOfMaxOccurrence_hrs + "," + nodeFlood.TimeOfMaxOccurrence_mins + "," +
                                                    nodeFlood.TotalFloodVolume.ToString("0.###") + "," + nodeFlood.MaximumPondedVolume.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.avgDepth.ToString("0.###") + "," + nodeFlood.NodeStats.maxDepth.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.maxDepthDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxRptDepth.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.volFlooded.ToString("0.###") + "," + nodeFlood.NodeStats.timeFlooded.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.timeSurcharged.ToString("0.###") + "," + nodeFlood.NodeStats.timeCourantCritical.ToString("0.###") + "," +
                                                    nodeFlood.NodeStats.totLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflow.ToString("0.###") +
                                                    nodeFlood.NodeStats.maxOverflow.ToString("0.###") + "," + nodeFlood.NodeStats.maxPondedVol.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflowDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxOverflowDate.ToLongTimeString());

                                            nodeFloodingSummary.NodeFloodsInfo.Add(nodeFlood);
                                            nodeFloodingSummary.SumTotalFloodVolume += nodeFlood.TotalFloodVolume;

                                        }

                                        if (!hasFlood)
                                        {
                                            nodeFloodingSummary.isNoFlooding = true;
                                        }

                                        //////////////////

                                        string linkID = "";

                                        LinkSurchargeSummary linkSurchargeSummary;

                                        linkSurchargeSummary = new LinkSurchargeSummary()
                                        {
                                            isNoSurcharge = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            LinkSurchargeInfo = new List<LinkSurcharge>(),
                                        };


                                        Summary.Add(new Tuple<NodeFloodingSummary, LinkSurchargeSummary>(nodeFloodingSummary, linkSurchargeSummary));
                                        FolderSummary.Add(new Tuple<NodeFloodingSummary, LinkSurchargeSummary>(nodeFloodingSummary, linkSurchargeSummary));

                                        //////////////////////////////
                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();

                                    // Close the system
                                    toolkitSWMM.Close();

                                    //return error_getCode(ErrorCode);
                                }
                                catch (Exception ex)
                                {
                                    // Log error.
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            // Log error.
                        }
                        finally
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                                FileCountInFolder++;

                                if (FileCountInFolder == failuringNumber)
                                {
                                    waitEndLoop.Set();
                                }
                            }

                            s.Release();
                        }

                    });

                    th.Name = "Failure scenario: " + i;
                    th.IsBackground = true;
                    th.Priority = ThreadPriority.AboveNormal;
                    //th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                }

                waitEndLoop.WaitOne(60 * 60 * 1000, false);
            }

            btnStartCombination.Text = "Start";
            btnStartCombination.Refresh();
        }

        private void btnStartRouletteWheel_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartRouletteWheel.Text = "Running";
            btnStartRouletteWheel.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> AllLinkList = new List<Link>();
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                            AllLinkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            List<OutfallInflow> flwLinesOfRefOutfallInflow = File.ReadAllLines(txtFlwRefFile.Text, Encoding.UTF8).Select(x => new OutfallInflow()
            {
                ElapsedTime = Convert.ToDouble(x.Split(' ')[0]),
                OutfallTotalInflow = Convert.ToDouble(x.Split(' ')[1]),
                HasFlooded = x.Split(' ')[2] == "1" ? true : false
            }).ToList();

            List<double> refOutfallInflowElapsedTimes = new List<double>(flwLinesOfRefOutfallInflow.Select(x => x.ElapsedTime));


            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);


            //ISWMMToolkit[] toolkitSWMMs = new ISWMMToolkit[cores];

            List<NodeFloodingSummary> FolderSummary = new List<NodeFloodingSummary>();
            //List<NodeFloodingSummary> TopFolderSummary = new List<NodeFloodingSummary>();
            List<ResilienceMetrics> MetricsSummary = new List<ResilienceMetrics>();
            List<ResilienceMetrics> TopMetricsSummary = new List<ResilienceMetrics>();

            double topFailuringPercentage = (double)(nudTopFailuring.Value / 100);

            ManualResetEvent waitEndLoop = new ManualResetEvent(true);

            int rouletteWheelStartFolder = (int)nudRouletteWheelStartFolder.Value;

            for (int i = rouletteWheelStartFolder; i <= linksCount; i++)
            {
                int failureLinkCount = i;

                //int topCount = failureLinkCount == 2 ? FolderSummary.Count : (int)(topFailuringPercentage * FolderSummary.Count);
                //TopFolderSummary = FolderSummary.OrderByDescending(x => x.SumTotalFloodVolume).Take(topCount).ToList();

                int topCount = (failureLinkCount == 2 || failureLinkCount == linksCount) ? MetricsSummary.Count : (int)(topFailuringPercentage * MetricsSummary.Count);

                if (MetricsSummary.Count > 0 && failureLinkCount <= linksCount)
                {
                    Matrix<double> options = DenseMatrix.OfRows(MetricsSummary.Select(x => new List<double>() { x.FailureTime, x.StorageTime, x.FloodingLossVolume, x.StorageCapacity }));

                    ShannonEntropy shannon = new ShannonEntropy(MetricsSummary.Count, 4);

                    shannon.Matrix = options;

                    shannon.Normalize();
                    shannon.CalculateEntropy();
                    shannon.CalculateDistances();
                    shannon.CalculateWeights();

                    Topsis topsis = new Topsis(MetricsSummary.Count, 4, new bool[] { false, true, false, true });

                    topsis.Matrix = options;

                    topsis.Normalize();
                    topsis.WeighingNormalizedMatrix(shannon.W);
                    topsis.FindingIdealValues();
                    topsis.CalculateDistances();
                    topsis.CalculateSimilarity();

                    Parallel.ForEach(topsis.Similarity, (similarity, pls, index) =>
                    {
                        MetricsSummary[(int)index].Score = similarity;
                    });

                    TopMetricsSummary = MetricsSummary.OrderBy(x => x.Score).Take(topCount).ToList();

                    var dirInfoPrevious = Directory.CreateDirectory(Path.Combine(outputFolderPath, (failureLinkCount - 1).ToString()));

                    if (!File.Exists(Path.Combine(dirInfoPrevious.FullName, "Metrics Summary.inf")))
                    {
                        using (StreamWriter metricsSummaryFile = new StreamWriter(Path.Combine(dirInfoPrevious.FullName, "Metrics Summary.inf")))
                        {
                            metricsSummaryFile.WriteLine(string.Join(",", shannon.W));

                            foreach (var metricsSummary in TopMetricsSummary)
                            {
                                metricsSummaryFile.Write(string.Join(",", metricsSummary.FailureLinksID));
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(string.Join(",", metricsSummary.FailureLinksIndex));
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.MeanFailureTime);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.FailureTime);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.StorageTime);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.FloodingLossVolume);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.StorageCapacity);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.MinNodeFloodingTime);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.MaxNodeFloodingTime);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.MinNodeFloodingVolume);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.MaxNodeFloodingVolume);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.Score);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.FileIndex);

                                metricsSummaryFile.WriteLine();
                            }
                        }
                    }
                }


                FolderSummary.Clear();
                MetricsSummary.Clear();

                List<int> toolkitSWMMIndexes = new List<int>();
                for (int t = 0; t < cores; t++)
                {
                    toolkitSWMMIndexes.Add(0);
                }

                var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                List<List<int>> targetCombinations = new List<List<int>>();

                if (failureLinkCount == 1)
                {
                    targetCombinations = Enumerable.Range(1, linksCount).Select(x => (new int[] { linkList[x - 1].Index }).ToList()).ToList();
                }
                else if (failureLinkCount == linksCount)
                {
                    targetCombinations = new List<int>[] { Enumerable.Range(1, linksCount).Select(x => linkList[x - 1].Index).ToList() }.ToList();
                }
                else if (failureLinkCount == linksCount - 1)
                {
                    for (int k = 0; k < linksCount; k++)
                    {
                        targetCombinations.Add(Enumerable.Range(1, linksCount).Select(x => linkList[x - 1].Index).ToList());
                        targetCombinations[k].RemoveAt(k);
                    }
                }
                else
                {
                    if (failureLinkCount == rouletteWheelStartFolder)
                    {
                        string[] targetCombinationsLines = File.ReadAllLines(Path.Combine(dirInfo.FullName, "Target Combinations.ini"), Encoding.UTF8);

                        foreach (var targetCombinationsLine in targetCombinationsLines)
                        {
                            List<string> targetCombinationString = targetCombinationsLine.Split(',').ToList();
                            //targetCombinationString.RemoveAt(targetCombinationString.Count - 1);

                            List<int> targetCombination = targetCombinationString.Select(x => Convert.ToInt32(x)).ToList();
                            targetCombinations.Add(targetCombination);
                        }
                    }
                    else
                    {
                        var rouletteWheel = new List<double>();
                        var rnd = RandomizationProvider.Current;

                        //RouletteWheel.CalculateCumulativePercentTotalFloodVolume(TopFolderSummary, rouletteWheel);
                        //targetCombinations = RouletteWheel.SelectFromWheel((int)nudTargetNumber.Value, TopFolderSummary, rouletteWheel, () => rnd.GetDouble(), linkList);

                        RouletteWheel.CalculateCumulativePercent(TopMetricsSummary, rouletteWheel);
                        targetCombinations = RouletteWheel.SelectFromWheel((int)nudTargetNumber.Value, TopMetricsSummary, rouletteWheel, () => rnd.GetDouble(), linkList);
                    }
                }

                if (!File.Exists(Path.Combine(dirInfo.FullName, "Target Combinations.ini")))
                {
                    using (StreamWriter targetCombinationsFile = new StreamWriter(Path.Combine(dirInfo.FullName, "Target Combinations.ini")))
                    {
                        foreach (var targetCombination in targetCombinations)
                        {
                            //foreach (var targetIndex in targetCombination)
                            //{
                            //    targetCombinationsFile.Write(targetIndex + ",");
                            //}

                            //targetCombinationsFile.WriteLine();
                            targetCombinationsFile.WriteLine(string.Join(",", targetCombination));
                        }
                    }
                }


                int failuringNumber = targetCombinations.Count();

                waitEndLoop.Reset();

                int FileCountInFolder = 0;

                int rouletteWheelStartFile = 1;

                if (failureLinkCount == rouletteWheelStartFolder)
                {
                    rouletteWheelStartFile = (int)nudRouletteWheelStartFile.Value;
                }

                for (int j_index = rouletteWheelStartFile; j_index <= failuringNumber; j_index++)
                {
                    int j = j_index;
                    s.WaitOne();

                    Thread th = new Thread(delegate ()
                    {
                        int failuringCounter = j;
                        int toolkitSWMMIndex = -1;

                        try
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                                toolkitSWMMIndexes[toolkitSWMMIndex] = failuringCounter;
                            }

                            List<int> failureLinkIndexes = new List<int>();
                            List<int> targetCombination = targetCombinations[failuringCounter - 1];

                            string currentFailureLinks = "";

                            ResilienceMetrics resilienceMetrics = new ResilienceMetrics();

                            resilienceMetrics.FileIndex = failuringCounter;

                            for (int fl = 0; fl < targetCombination.Count(); fl++)
                            {
                                currentFailureLinks += targetCombination[fl] + "(" + linkList.FirstOrDefault(x => x.Index == targetCombination[fl]).ID + ")" + ",";
                                failureLinkIndexes.Add(targetCombination[fl]);

                                resilienceMetrics.FailureLinksIndex.Add(targetCombination[fl]);
                                resilienceMetrics.FailureLinksID.Add(linkList.FirstOrDefault(x => x.Index == targetCombination[fl]).ID);
                            }


                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".txt")))
                            {
                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10();
                                    }
                                    else if (toolkitSWMMIndex == 10)
                                    {
                                        toolkitSWMM = new SWMMToolkit_11();
                                    }
                                    else if (toolkitSWMMIndex == 11)
                                    {
                                        toolkitSWMM = new SWMMToolkit_12();
                                    }
                                    else if (toolkitSWMMIndex == 12)
                                    {
                                        toolkitSWMM = new SWMMToolkit_13();
                                    }
                                    else if (toolkitSWMMIndex == 13)
                                    {
                                        toolkitSWMM = new SWMMToolkit_14();
                                    }
                                    else if (toolkitSWMMIndex == 14)
                                    {
                                        toolkitSWMM = new SWMMToolkit_15();
                                    }
                                    else if (toolkitSWMMIndex == 15)
                                    {
                                        toolkitSWMM = new SWMMToolkit_16();
                                    }
                                    else if (toolkitSWMMIndex == 16)
                                    {
                                        toolkitSWMM = new SWMMToolkit_17();
                                    }
                                    else if (toolkitSWMMIndex == 17)
                                    {
                                        toolkitSWMM = new SWMMToolkit_18();
                                    }
                                    else if (toolkitSWMMIndex == 18)
                                    {
                                        toolkitSWMM = new SWMMToolkit_19();
                                    }
                                    else if (toolkitSWMMIndex == 19)
                                    {
                                        toolkitSWMM = new SWMMToolkit_20();
                                    }

                                    double floodElapsedTime = 0;
                                    double floodOutfalInflow = 0;

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt"), "");
                                    //Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".out"));

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        //StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter);
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter, Path.Combine(dirInfo.FullName, failuringCounter.ToString()), ref floodElapsedTime, ref floodOutfalInflow);
                                        //////////////////////

                                        RoutingTotals routingTotals = new RoutingTotals();

                                        ErrorCode = toolkitSWMM.GetSystemRoutingStats(ref routingTotals);

                                        myReportFile.WriteLine("Dry Weather Inflow (gal)," + routingTotals.dwInflow * 1.0e6); // w_MGAL to w_GAL
                                        myReportFile.WriteLine("Flooding Loss (gal)," + routingTotals.flooding * 1.0e6); // w_MGAL to w_GAL

                                        resilienceMetrics.FloodingLossVolume = routingTotals.flooding * 1.0e6;
                                        //////////////////////

                                        int nodeCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.NODE, ref nodeCount);

                                        int nodeIndex;
                                        int days = 0, hrs = 0, mins = 0, routeModel = 0;
                                        double t, vcf = 0;
                                        string nodeID = "";
                                        bool hasFlood = false;

                                        NodeFloodingSummary nodeFloodingSummary;

                                        nodeFloodingSummary = new NodeFloodingSummary()
                                        {
                                            isNoFlooding = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            NodeFloodsInfo = new List<NodeFlood>(),
                                        };



                                        double sumHoursNodalFlooding = 0;
                                        int numNodalFlooding = 0;

                                        double maxNodeFloodingVolume = 0, minNodeFloodingVolume = double.MaxValue;
                                        double maxNodeFloodingTime = 0, minNodeFloodingTime = double.MaxValue;

                                        for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                                        {
                                            int nodeType = -1;
                                            toolkitSWMM.GetNodeType(nodeIndex, ref nodeType);

                                            if ((NodeTypeEnum)nodeType == NodeTypeEnum.OUTFALL) continue;

                                            NodeStats nodeStats = new NodeStats();
                                            toolkitSWMM.GetNodeStats(nodeIndex, ref nodeStats);

                                            if (nodeStats.timeFlooded == 0.0) continue;

                                            t = Math.Max(0.01, nodeStats.timeFlooded);

                                            hasFlood = true;

                                            NodeFlood nodeFlood = new NodeFlood();

                                            nodeFlood.Index = nodeIndex;

                                            toolkitSWMM.GetObjectId((int)ObjectTypeEnum.NODE, nodeIndex, ref nodeID);
                                            nodeFlood.ID = nodeID;
                                            nodeFlood.HoursFlooded = t;

                                            nodeFlood.MaximumRate = nodeStats.maxOverflow;

                                            toolkitSWMM.GetElapsedTime(nodeStats.maxOverflowDate, ref days, ref hrs, ref mins);

                                            nodeFlood.TimeOfMaxOccurrence_days = days;
                                            nodeFlood.TimeOfMaxOccurrence_hrs = hrs;
                                            nodeFlood.TimeOfMaxOccurrence_mins = mins;

                                            toolkitSWMM.GetVcf(ref vcf);
                                            nodeFlood.TotalFloodVolume = nodeStats.volFlooded * vcf * 1.0e6; // w_MGAL to w_GAL

                                            toolkitSWMM.GetRouteModel(ref routeModel);

                                            if ((RouteModelTypeEnum)routeModel == RouteModelTypeEnum.DW)
                                            {
                                                double fullDepth = 0;
                                                toolkitSWMM.GetNodeParam(nodeIndex, (int)NodeProperty.FULLDEPTH, ref fullDepth);
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxDepth - fullDepth;
                                            }
                                            else
                                            {
                                                //? / 1000
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxPondedVol / 1000.0;
                                            }

                                            nodeFlood.NodeStats = nodeStats;

                                            myReportFile.WriteLine("NodeFlooding," + nodeFlood.Index + "," + nodeFlood.ID + "," +
                                                nodeFlood.HoursFlooded.ToString("0.###") + "," + nodeFlood.MaximumRate.ToString("0.###") + "," +
                                                nodeFlood.TimeOfMaxOccurrence_days + "," + nodeFlood.TimeOfMaxOccurrence_hrs + "," + nodeFlood.TimeOfMaxOccurrence_mins + "," +
                                                nodeFlood.TotalFloodVolume.ToString("0.###") + "," + nodeFlood.MaximumPondedVolume.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.avgDepth.ToString("0.###") + "," + nodeFlood.NodeStats.maxDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.maxDepthDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxRptDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.volFlooded.ToString("0.###") + "," + nodeFlood.NodeStats.timeFlooded.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.timeSurcharged.ToString("0.###") + "," + nodeFlood.NodeStats.timeCourantCritical.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.totLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflow.ToString("0.###") +
                                                nodeFlood.NodeStats.maxOverflow.ToString("0.###") + "," + nodeFlood.NodeStats.maxPondedVol.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflowDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxOverflowDate.ToLongTimeString());

                                            nodeFloodingSummary.NodeFloodsInfo.Add(nodeFlood);
                                            nodeFloodingSummary.SumTotalFloodVolume += nodeFlood.TotalFloodVolume;

                                            sumHoursNodalFlooding += nodeFlood.HoursFlooded;
                                            if (nodeFlood.HoursFlooded > 0)
                                            {
                                                numNodalFlooding++;

                                                if (nodeFlood.TotalFloodVolume > maxNodeFloodingVolume)
                                                {
                                                    maxNodeFloodingVolume = nodeFlood.TotalFloodVolume;
                                                }
                                                if (nodeFlood.TotalFloodVolume < minNodeFloodingVolume)
                                                {
                                                    minNodeFloodingVolume = nodeFlood.TotalFloodVolume;
                                                }

                                                if (nodeFlood.HoursFlooded > maxNodeFloodingTime)
                                                {
                                                    maxNodeFloodingTime = nodeFlood.HoursFlooded;
                                                }
                                                if (nodeFlood.HoursFlooded < minNodeFloodingTime)
                                                {
                                                    minNodeFloodingTime = nodeFlood.HoursFlooded;
                                                }
                                            }
                                        }

                                        resilienceMetrics.MinNodeFloodingVolume = minNodeFloodingVolume != double.MaxValue ? minNodeFloodingVolume : 0;
                                        resilienceMetrics.MaxNodeFloodingVolume = maxNodeFloodingVolume;
                                        resilienceMetrics.MinNodeFloodingTime = minNodeFloodingTime != double.MaxValue ? minNodeFloodingTime : 0;
                                        resilienceMetrics.MaxNodeFloodingTime = maxNodeFloodingTime;

                                        if (!hasFlood)
                                        {
                                            nodeFloodingSummary.isNoFlooding = true;
                                        }

                                        if (numNodalFlooding != 0)
                                        {
                                            double meanNodalFloodingTime = sumHoursNodalFlooding / numNodalFlooding;

                                            myReportFile.WriteLine("Sum Hours Nodal Flooding," + sumHoursNodalFlooding);
                                            myReportFile.WriteLine("Mean Failure Time," + meanNodalFloodingTime);

                                            resilienceMetrics.FailureTime = sumHoursNodalFlooding;
                                            resilienceMetrics.MeanFailureTime = meanNodalFloodingTime;
                                        }

                                        if (floodElapsedTime != 0)
                                        {
                                            myReportFile.WriteLine("Flood (or End of failuring) Elapsed Time," + floodElapsedTime);
                                            myReportFile.WriteLine("Flood (or End of failuring) OutfallInflow," + floodOutfalInflow);

                                            int refIndex = refOutfallInflowElapsedTimes.BinarySearch(floodElapsedTime);

                                            double refOutfallInflow = refIndex < 0 ? flwLinesOfRefOutfallInflow[-refIndex - 2].OutfallTotalInflow : flwLinesOfRefOutfallInflow[refIndex].OutfallTotalInflow;
                                            double diffOutfalInflow = refOutfallInflow - floodOutfalInflow;

                                            myReportFile.WriteLine("Diff OutfallInflow," + diffOutfalInflow);

                                            resilienceMetrics.StorageTime = floodElapsedTime;
                                            resilienceMetrics.StorageCapacity = diffOutfalInflow;
                                        }

                                        FolderSummary.Add(nodeFloodingSummary);
                                        MetricsSummary.Add(resilienceMetrics);
                                        //////////////////////////////
                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();

                                    // Close the system
                                    toolkitSWMM.Close();

                                    //return error_getCode(ErrorCode);
                                }
                                catch (Exception ex)
                                {
                                    // Log error.
                                }
                            }

                            if (failureLinkCount == linksCount)
                            {
                                Matrix<double> options = DenseMatrix.OfRows(MetricsSummary.Select(x => new List<double>() { x.FailureTime, x.StorageTime, x.FloodingLossVolume, x.StorageCapacity }));

                                ShannonEntropy shannon = new ShannonEntropy(MetricsSummary.Count, 4);

                                shannon.Matrix = options;

                                shannon.Normalize();
                                shannon.CalculateEntropy();
                                shannon.CalculateDistances();
                                shannon.CalculateWeights();

                                Topsis topsis = new Topsis(MetricsSummary.Count, 4, new bool[] { false, true, false, true });

                                topsis.Matrix = options;

                                topsis.Normalize();
                                topsis.WeighingNormalizedMatrix(shannon.W);
                                topsis.FindingIdealValues();
                                topsis.CalculateDistances();
                                topsis.CalculateSimilarity();

                                Parallel.ForEach(topsis.Similarity, (similarity, pls, index) =>
                                {
                                    MetricsSummary[(int)index].Score = similarity;
                                });

                                TopMetricsSummary = MetricsSummary.OrderBy(x => x.Score).Take(1).ToList();

                                var dirInfoCurrent = Directory.CreateDirectory(Path.Combine(outputFolderPath, (failureLinkCount).ToString()));

                                if (!File.Exists(Path.Combine(dirInfoCurrent.FullName, "Metrics Summary.inf")))
                                {
                                    using (StreamWriter metricsSummaryFile = new StreamWriter(Path.Combine(dirInfoCurrent.FullName, "Metrics Summary.inf")))
                                    {
                                        metricsSummaryFile.WriteLine(string.Join(",", shannon.W));

                                        foreach (var metricsSummary in TopMetricsSummary)
                                        {
                                            metricsSummaryFile.Write(string.Join(",", metricsSummary.FailureLinksID));
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(string.Join(",", metricsSummary.FailureLinksIndex));
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.MeanFailureTime);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.FailureTime);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.StorageTime);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.FloodingLossVolume);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.StorageCapacity);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.MinNodeFloodingTime);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.MaxNodeFloodingTime);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.MinNodeFloodingVolume);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.MaxNodeFloodingVolume);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.Score);
                                            metricsSummaryFile.Write(" ");
                                            metricsSummaryFile.Write(metricsSummary.FileIndex);

                                            metricsSummaryFile.WriteLine();
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error.
                        }
                        finally
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                                FileCountInFolder++;

                                if (FileCountInFolder == failuringNumber)
                                {
                                    waitEndLoop.Set();
                                }
                            }

                            s.Release();
                        }


                    });

                    th.Name = "Failure scenario: " + i;
                    th.IsBackground = true;
                    th.Priority = ThreadPriority.AboveNormal;
                    //th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                }

                waitEndLoop.WaitOne(60 * 60 * 1000, false);
            }

            btnStartRouletteWheel.Text = "Start";
            btnStartRouletteWheel.Refresh();
        }

        private void btnInflowAnalysis_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnInflowAnalysis.Text = "Running";
            btnInflowAnalysis.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                (chkMainDiameter.Checked && lines[li + line + 2].Split(' ')[0].Contains("M")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            List<ReportSummary> Summary = new List<ReportSummary>();

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);

            List<int> toolkitSWMMIndexes = new List<int>();
            for (int t = 0; t < cores; t++)
            {
                toolkitSWMMIndexes.Add(0);
            }
            //ISWMMToolkit[] toolkitSWMMs = new ISWMMToolkit[cores];

            for (int i_index = 1; i_index <= 1; i_index++)
            {
                int i = i_index;
                s.WaitOne();

                Thread th = new Thread(delegate ()
                {
                    int failureLinkCount = i;
                    int toolkitSWMMIndex = -1;

                    try
                    {
                        lock (toolkitSWMMIndexes)  //for lock thread
                        {
                            toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                            toolkitSWMMIndexes[toolkitSWMMIndex] = failureLinkCount;
                        }


                        //if (InvokeRequired)
                        //{
                        //    Invoke((Action)delegate
                        //    {
                        //        lblScenarioCounter.Text = "# Failure Links: " + failureLinkCount;
                        //            //lblScenarioCounter.Refresh();
                        //        });
                        //}

                        var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                        int failuringNumber = 1;

                        //if (failureLinkCount == 1)
                        //    failuringNumber = linksCount;
                        //else if (failureLinkCount == 2 || failureLinkCount == linksCount - 2)
                        //    failuringNumber = 5;
                        //else if (failureLinkCount == linksCount - 1)
                        //    failuringNumber = 5;
                        //else if (failureLinkCount == linksCount)
                        //    failuringNumber = 10;
                        //else
                        //    failuringNumber = 10;

                        for (int j = 1; j <= failuringNumber; j++)
                        {
                            List<int> randomFailureLinks = new List<int>();
                            List<int> failureLinkIndexes = new List<int>();
                            Random rand = new Random();

                            if (!chkWithoutFailuring.Checked)
                            {
                                randomFailureLinks.AddRange(Enumerable.Range(1, linksCount).OrderBy(m => rand.Next()).Take(failureLinkCount));
                                //Thread.Sleep(50);
                                //randomFailureLinks.Add(failureLinkCount);
                            }


                            string currentFailureLinks = "";

                            for (int fl = 0; fl < randomFailureLinks.Count(); fl++)
                            {
                                currentFailureLinks += linkList[randomFailureLinks[fl] - 1].Index + "(" + linkList[randomFailureLinks[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[randomFailureLinks[fl] - 1].Index);

                                //string linkID = "1045C";
                                //int index = linkList.FirstOrDefault(x => x.ID == linkID).Index;
                                //string id = linkList.FirstOrDefault(x => x.ID == linkID).ID;
                                //currentFailureLinks += index + "(" + id + ")" + ",";
                                //failureLinkIndexes.Add(index);
                            }

                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, j.ToString() + ".txt")))
                            {
                                //File.WriteAllText(Path.Combine(dirInfo.FullName, j.ToString() + ".txt"), "Failure Diameter: " + Diameter + "\n" + "Failure Links: " + currentFailureLinks + "\n");

                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                //StreamReader oReader;
                                //if (File.Exists(inputFilePath))
                                //{
                                //    string cSearforSomething = "Link";
                                //    oReader = new StreamReader(inputFilePath);
                                //    string cColl = oReader.ReadToEnd();
                                //    string cCriteria = @"\b" + cSearforSomething + @"\b";
                                //    System.Text.RegularExpressions.Regex oRegex = new
                                //    System.Text.RegularExpressions.Regex(cCriteria, RegexOptions.IgnoreCase);

                                //    var count = oRegex.Matches(cColl).AsQueryable();
                                //    Console.WriteLine(count.ToString());
                                //}
                                //Console.ReadLine();

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10();
                                    }

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, j.ToString() + ".rpt"),
                                                            "" /*Path.Combine(dirInfo.FullName, j.ToString() + ".out")*/);

                                    double floodElapsedTime = 0;
                                    double floodOutfalInflow = 0;

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter, Path.Combine(dirInfo.FullName, j.ToString()), ref floodElapsedTime, ref floodOutfalInflow, true);

                                        //////////////////////

                                        RoutingTotals routingTotals = new RoutingTotals();

                                        ErrorCode = toolkitSWMM.GetSystemRoutingStats(ref routingTotals);

                                        myReportFile.WriteLine("Dry Weather Inflow (gal)," + routingTotals.dwInflow * 1.0e6); // w_MGAL to w_GAL
                                        myReportFile.WriteLine("Flooding Loss (gal)," + routingTotals.flooding * 1.0e6); // w_MGAL to w_GAL

                                        //////////////////////

                                        int nodeCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.NODE, ref nodeCount);

                                        int nodeIndex;
                                        int days = 0, hrs = 0, mins = 0, routeModel = 0;
                                        double t, vcf = 0;
                                        string nodeID = "";
                                        bool hasFlood = false;

                                        NodeFloodingSummary nodeFloodingSummary;

                                        nodeFloodingSummary = new NodeFloodingSummary()
                                        {
                                            isNoFlooding = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            NodeFloodsInfo = new List<NodeFlood>(),
                                        };

                                        double sumHoursNodalFlooding = 0;
                                        int numNodalFlooding = 0;

                                        for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                                        {
                                            int nodeType = -1;
                                            toolkitSWMM.GetNodeType(nodeIndex, ref nodeType);

                                            if ((NodeTypeEnum)nodeType == NodeTypeEnum.OUTFALL) continue;

                                            NodeStats nodeStats = new NodeStats();
                                            toolkitSWMM.GetNodeStats(nodeIndex, ref nodeStats);

                                            if (nodeStats.timeFlooded == 0.0) continue;

                                            t = Math.Max(0.01, nodeStats.timeFlooded);

                                            hasFlood = true;

                                            NodeFlood nodeFlood = new NodeFlood();

                                            nodeFlood.Index = nodeIndex;

                                            toolkitSWMM.GetObjectId((int)ObjectTypeEnum.NODE, nodeIndex, ref nodeID);
                                            nodeFlood.ID = nodeID;
                                            nodeFlood.HoursFlooded = t;

                                            nodeFlood.MaximumRate = nodeStats.maxOverflow;

                                            toolkitSWMM.GetElapsedTime(nodeStats.maxOverflowDate, ref days, ref hrs, ref mins);

                                            nodeFlood.TimeOfMaxOccurrence_days = days;
                                            nodeFlood.TimeOfMaxOccurrence_hrs = hrs;
                                            nodeFlood.TimeOfMaxOccurrence_mins = mins;

                                            toolkitSWMM.GetVcf(ref vcf);
                                            nodeFlood.TotalFloodVolume = nodeStats.volFlooded * vcf * 1.0e6; // w_MGAL to w_GAL (VolUnitsWords)

                                            toolkitSWMM.GetRouteModel(ref routeModel);

                                            if ((RouteModelTypeEnum)routeModel == RouteModelTypeEnum.DW)
                                            {
                                                double fullDepth = 0;
                                                toolkitSWMM.GetNodeParam(nodeIndex, (int)NodeProperty.FULLDEPTH, ref fullDepth);
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxDepth - fullDepth;
                                            }
                                            else
                                            {
                                                //? / 1000
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxPondedVol / 1000.0;
                                            }

                                            nodeFlood.NodeStats = nodeStats;

                                            myReportFile.WriteLine("NodeFlooding," + nodeFlood.Index + "," + nodeFlood.ID + "," +
                                                nodeFlood.HoursFlooded.ToString("0.###") + "," + nodeFlood.MaximumRate.ToString("0.###") + "," +
                                                nodeFlood.TimeOfMaxOccurrence_days + "," + nodeFlood.TimeOfMaxOccurrence_hrs + "," + nodeFlood.TimeOfMaxOccurrence_mins + "," +
                                                nodeFlood.TotalFloodVolume.ToString("0.###") + "," + nodeFlood.MaximumPondedVolume.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.avgDepth.ToString("0.###") + "," + nodeFlood.NodeStats.maxDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.maxDepthDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxRptDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.volFlooded.ToString("0.###") + "," + nodeFlood.NodeStats.timeFlooded.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.timeSurcharged.ToString("0.###") + "," + nodeFlood.NodeStats.timeCourantCritical.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.totLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflow.ToString("0.###") +
                                                nodeFlood.NodeStats.maxOverflow.ToString("0.###") + "," + nodeFlood.NodeStats.maxPondedVol.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflowDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxOverflowDate.ToLongTimeString());

                                            nodeFloodingSummary.NodeFloodsInfo.Add(nodeFlood);

                                            sumHoursNodalFlooding += nodeFlood.HoursFlooded;
                                            if (nodeFlood.HoursFlooded > 0)
                                            {
                                                numNodalFlooding++;
                                            }

                                        }

                                        if (!hasFlood)
                                        {
                                            nodeFloodingSummary.isNoFlooding = true;
                                        }

                                        //////////////////

                                        string linkID = "";

                                        LinkSurchargeSummary linkSurchargeSummary;

                                        linkSurchargeSummary = new LinkSurchargeSummary()
                                        {
                                            isNoSurcharge = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            LinkSurchargeInfo = new List<LinkSurcharge>(),
                                        };

                                        //double[] ts = new double[5];
                                        //int linkCount = 0;
                                        //toolkitSWMM.GetObjectCount(ObjectTypeEnum.LINK, ref linkCount);

                                        //int linkIndex;
                                        //bool hasSurcharge = false;

                                        //for (linkIndex = 0; linkIndex < linkCount; linkIndex++)
                                        //{
                                        //    int linkType = 0;
                                        //    int xsectType = 0;
                                        //    LinkStats linkStats = new LinkStats();

                                        //    toolkitSWMM.GetLinkStats(linkIndex, ref linkStats);
                                        //    toolkitSWMM.GetLinkType(linkIndex, ref linkType);
                                        //    toolkitSWMM.GetLinkXsectType(linkIndex, ref xsectType);

                                        //    if ((LinkTypeEnum)linkType != LinkTypeEnum.CONDUIT || (XSectionTypeEnum)xsectType == XSectionTypeEnum.DUMMY) continue;

                                        //    ts[0] = linkStats.timeSurcharged;
                                        //    ts[1] = linkStats.timeFullUpstream;
                                        //    ts[2] = linkStats.timeFullDnstream;
                                        //    ts[3] = linkStats.timeFullFlow;

                                        //    if (ts[0] + ts[1] + ts[2] + ts[3] == 0.0)
                                        //    {
                                        //        continue;
                                        //    }
                                        //    ts[4] = linkStats.timeCapacityLimited;

                                        //    for (int ts_index = 0; ts_index < 5; ts_index++)
                                        //        ts[ts_index] = Math.Max(0.01, ts[ts_index]);

                                        //    hasSurcharge = true;

                                        //    LinkSurcharge linkSurcharge = new LinkSurcharge();

                                        //    linkSurcharge.Index = linkIndex;

                                        //    toolkitSWMM.GetObjectId((int)ObjectTypeEnum.LINK, linkIndex, ref linkID);

                                        //    linkSurcharge.ID = linkID;

                                        //    linkSurcharge.BothEnds = ts[0];
                                        //    linkSurcharge.Upstream = ts[1];
                                        //    linkSurcharge.Dnstream = ts[2];
                                        //    linkSurcharge.HoursAboveFullNormalFlow = ts[3];
                                        //    linkSurcharge.HoursCapacityLimited = ts[4];

                                        //    linkSurcharge.LinkStats = linkStats;

                                        //    myReportFile.WriteLine("LinkSurcharge," + linkSurcharge.Index + "," + linkSurcharge.ID + "," + linkSurcharge.BothEnds.ToString("0.###") + "," + linkSurcharge.Upstream.ToString("0.###") + "," + linkSurcharge.Dnstream.ToString("0.###") + "," +
                                        //        linkSurcharge.HoursAboveFullNormalFlow.ToString("0.###") + "," + linkSurcharge.HoursCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.maxFlowDate.ToLongTimeString() + "," +
                                        //        linkSurcharge.LinkStats.maxVeloc.ToString("0.###") + "," + linkSurcharge.LinkStats.maxVeloc.ToString("0.###") + "," + linkSurcharge.LinkStats.maxDepth.ToString("0.###") + "," + linkSurcharge.LinkStats.timeNormalFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeInletControl.ToString("0.###") + "," + linkSurcharge.LinkStats.timeSurcharged.ToString("0.###") + "," +
                                        //        linkSurcharge.LinkStats.timeFullUpstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullDnstream.ToString("0.###") + "," + linkSurcharge.LinkStats.timeFullFlow.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCapacityLimited.ToString("0.###") + "," + linkSurcharge.LinkStats.timeCourantCritical.ToString("0.###") + "," +
                                        //        linkSurcharge.LinkStats.flowTurns + "," + linkSurcharge.LinkStats.flowTurnSign);

                                        //    linkSurchargeSummary.LinkSurchargeInfo.Add(linkSurcharge);
                                        //}

                                        //if (!hasSurcharge)
                                        //{
                                        //    linkSurchargeSummary.isNoSurcharge = true;
                                        //}


                                        ////////////////////////
                                        if (numNodalFlooding != 0)
                                        {
                                            double meanNodalFlooding = sumHoursNodalFlooding / numNodalFlooding;

                                            myReportFile.WriteLine("Sum Hours Nodal Flooding," + sumHoursNodalFlooding);
                                            myReportFile.WriteLine("Mean Hours Nodal Flooding," + meanNodalFlooding);
                                        }

                                        if (floodElapsedTime != 0)
                                        {
                                            myReportFile.WriteLine("Flood Elapsed Time," + floodElapsedTime);
                                            myReportFile.WriteLine("Flood OutfallInflow," + floodOutfalInflow);
                                        }

                                        Summary.Add(new ReportSummary()
                                        {
                                            RoutingTotals = routingTotals,
                                            NodeFloodingSummary = nodeFloodingSummary,
                                            LinkSurchargeSummary = linkSurchargeSummary,
                                        });
                                        //////////////////////////////
                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();
                                    // Close the system
                                    toolkitSWMM.Close();
                                    //return error_getCode(ErrorCode);
                                }
                                catch (Exception ex)
                                {
                                    // Log error.
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error.
                    }
                    finally
                    {
                        lock (toolkitSWMMIndexes)
                        {
                            toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                        }

                        s.Release();

                        if (!toolkitSWMMIndexes.Any(x => x != 0))
                        {
                            if (InvokeRequired)
                            {
                                Invoke((Action)delegate
                                {
                                    btnInflowAnalysis.Text = "Start";
                                    btnInflowAnalysis.Refresh();
                                });
                            }

                        }
                    }
                });

                th.Name = "Failure scenario: " + i;
                th.IsBackground = true;
                th.Priority = ThreadPriority.AboveNormal;
                //th.SetApartmentState(ApartmentState.STA);
                th.Start();
            }


        }

        private void btnStorageCapasity_Click(object sender, EventArgs e)
        {
            string[] ResultsDirectories = Directory.GetDirectories(txtMetricResultsDirectory.Text).OrderBy(x => x, new OrdinalStringComparer()).ToArray();


            List<double> outfallInflowList = new List<double>();
            List<double> diffOutfalInflowList = new List<double>();
            List<double> elapsedTimeList = new List<double>();

            List<OutfallInflow> flwLines = File.ReadAllLines(txtFlwRefFile.Text, Encoding.UTF8).Select(x => new OutfallInflow()
            {
                ElapsedTime = Convert.ToDouble(x.Split(' ')[0]),
                OutfallTotalInflow = Convert.ToDouble(x.Split(' ')[1]),
                HasFlooded = x.Split(' ')[2] == "1" ? true : false
            }).ToList();

            List<double> elapsedTimes = new List<double>(flwLines.Select(x => x.ElapsedTime));



            for (int i = 1; i <= ResultsDirectories.Count(); i++)
            {
                string[] ResultsFiles = Directory.GetFiles(ResultsDirectories[i - 1], "*.txt").OrderBy(x => x, new OrdinalStringComparer()).ToArray();


                for (int j = 1; j <= ResultsFiles.Count(); j++)
                {
                    btnStorageCapasity.Text = "Running";
                    btnStorageCapasity.Refresh();

                    string[] lines = File.ReadAllLines(ResultsFiles[j - 1], Encoding.UTF8);

                    double floodElapsedTime = 0;
                    double floodOutfalInflow = 0;

                    string[] failureLinks = lines[1].Split(' ')[2].Split(',');

                    List<int> failureLinksIndex = new List<int>();
                    List<string> failureLinksID = new List<string>();

                    elapsedTimeList.Add(0);
                    outfallInflowList.Add(0);
                    diffOutfalInflowList.Add(0);



                    for (int k = 2; k < lines.Count(); k++)
                    {
                        string[] data = lines[k].Split(',');

                        switch (data[0])
                        {
                            case "Flood Elapsed Time":
                                floodElapsedTime = Convert.ToDouble(data[1]);
                                elapsedTimeList[i - 1] = floodElapsedTime;
                                break;
                            case "Flood OutfallInflow":
                                floodOutfalInflow = Convert.ToDouble(data[1]);
                                outfallInflowList[i - 1] = floodOutfalInflow;

                                int index = elapsedTimes.BinarySearch(elapsedTimeList[i - 1]);

                                double outfallInflow = index < 0 ? flwLines[-index - 2].OutfallTotalInflow : flwLines[index].OutfallTotalInflow;
                                diffOutfalInflowList[i - 1] = outfallInflow - outfallInflowList[i - 1];

                                break;
                            default:
                                break;
                        }
                    }

                }
            }

            for (int i = 0; i < ResultsDirectories.Count(); i++)
            {
                chtStorageCapacity.Series["Series1"].Points.AddXY(i, diffOutfalInflowList[i]);
                chtStorageCapacity.Update();
                chtStorageTime.Series["Series1"].Points.AddXY(i, elapsedTimeList[i]);
                chtStorageTime.Update();
            }




        }

        private void btnflwFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "inflow files (*.flw)|*.flw";
                //openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;   //History 

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    txtFlwRefFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnResResultsDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtResResultsDirectory.Text = folderDlg.SelectedPath;
            }
        }

        private void btnResilienceAnalysis_Click(object sender, EventArgs e)
        {
            string[] ResultsDirectories = Directory.GetDirectories(txtResResultsDirectory.Text).OrderBy(x => x, new OrdinalStringComparer()).ToArray();
            using (StreamWriter ResilienceFile = new StreamWriter("E:\\Resilience.txt"))
            {
                for (int i = 1; i <= ResultsDirectories.Count(); i++)
                {
                    List<ResilienceMetrics> Summary = new List<ResilienceMetrics>();

                    string[] lines = File.ReadAllLines(Path.Combine(ResultsDirectories[i - 1], "Metrics Summary.inf"), Encoding.UTF8);

                    for (int j = 1; j < lines.Count(); j++)
                    {
                        string[] values = lines[j].Split(' ');

                        ResilienceMetrics resilienceMetrics = new ResilienceMetrics();

                        resilienceMetrics.FailureLinksID = values[0].Split(',').ToList();
                        resilienceMetrics.FailureLinksIndex = values[1].Split(',').Select(x => Convert.ToInt32(x)).ToList();
                        resilienceMetrics.MeanFailureTime = Convert.ToDouble(values[2]);
                        resilienceMetrics.FailureTime = Convert.ToDouble(values[3]);
                        resilienceMetrics.StorageTime = Convert.ToDouble(values[4]);
                        resilienceMetrics.FloodingLossVolume = Convert.ToDouble(values[5]);
                        resilienceMetrics.StorageCapacity = Convert.ToDouble(values[6]);
                        resilienceMetrics.MinNodeFloodingTime = Convert.ToDouble(values[7]);
                        resilienceMetrics.MaxNodeFloodingTime = Convert.ToDouble(values[8]);
                        resilienceMetrics.MinNodeFloodingVolume = Convert.ToDouble(values[9]);
                        resilienceMetrics.MaxNodeFloodingVolume = Convert.ToDouble(values[10]);
                        resilienceMetrics.Score = Convert.ToDouble(values[11]);
                        resilienceMetrics.FileIndex = Convert.ToInt32(values[12]);

                        Summary.Add(resilienceMetrics);
                    }

                    double totalFailureTime = (double)(nudFailureEndTime.Value - nudFailureStartTime.Value) + 1;

                    // Hour 8: 0.332991898148148 87547.104
                    // Hour 16 (End of 15): 0.666637731481481 437298.115
                    double totalInflow8_15 = 349751.006; // 321308.522;

                    //Summary.ForEach(x => x.Resilience = 1 - (x.FloodingLossVolume / totalInflow8_15));
                    Summary.ForEach(x => x.Resilience = 1 - (x.FloodingLossVolume / totalInflow8_15) * ((totalInflow8_15 - x.StorageCapacity) / totalInflow8_15) * ((totalFailureTime - (x.StorageTime * 24 - (double)nudFailureStartTime.Value)) / totalFailureTime));
                    //Summary.ForEach(x => x.Resilience = 1 - ((x.FloodingLossVolume / totalInflow8_15) * ((totalInflow8_15 - x.StorageCapacity) / totalInflow8_15) * ((x.MeanFailureTime) / totalFailureTime) * ((totalFailureTime - (x.StorageTime * 24 - (double)nudFailureStartTime.Value)) / totalFailureTime)));

                    //for (int k = 0; k < Summary.Count; k++)
                    //{
                    //    Summary[k].Resilience = 1 - (Summary[k].FloodingLossVolume / totalInflow8_15) * ((totalInflow8_15 - Summary[k].StorageCapacity) / totalInflow8_15) * ((totalFailureTime - (Summary[k].StorageTime * 24 - (double)nudFailureStartTime.Value)) / totalFailureTime);
                    //}

                    //Summary.ForEach(x => x.Resilience = 1 - (x.FloodingLossVolume / totalInflow8_15) * (x.MeanFailureTime / totalFailureTime));
                    double meanResilience = Summary.Sum(x => x.Resilience) / Summary.Count();
                    double maxResilience = Summary.Max(x => x.Resilience);
                    double minResilience = Summary.Min(x => x.Resilience);

                    chtGlobalResilience.Series["Series1"].Points.AddXY(i, meanResilience);
                    chtGlobalResilience.Series["Series2"].Points.AddXY(i, maxResilience);
                    chtGlobalResilience.Series["Series3"].Points.AddXY(i, minResilience);
                    chtGlobalResilience.Update();

                    ResilienceFile.WriteLine(minResilience + ", " + meanResilience + ", " + maxResilience + ";");

                }
            }
            //double maxFloodingLossVolume = 0;
            //double maxMeanFailureTime = 0;
            //double maxStorageCapacity = 0;
            //double maxStorageTime = 0;
            //double maxFailureTime = 0;

            //maxFloodingLossVolume = Summary.Max(x => x.FloodingLossVolume);
            //maxMeanFailureTime = Summary.Max(x => x.MeanFailureTime);
            //maxStorageCapacity = Summary.Max(x => x.StorageCapacity);
            //maxFailureTime = Summary.Max(x => x.FailureTime);
            //maxStorageTime = Summary.Max(x => x.StorageTime);

            //Summary.ForEach(x => x.NormalizedFloodingLossVolume = x.FloodingLossVolume / maxFloodingLossVolume);
            //Summary.ForEach(x => x.NormalizedMeanFailureTime = x.MeanFailureTime / maxMeanFailureTime);
            //Summary.ForEach(x => x.NormalizedStorageCapacity = (maxStorageCapacity - x.StorageCapacity) / maxStorageCapacity);
            //Summary.ForEach(x => x.NormalizedFailureTime = x.FailureTime / maxFailureTime);
            //Summary.ForEach(x => x.NormalizedStorageTime = (maxStorageTime - x.StorageTime) / maxStorageTime);

            //double ceofFloodingLossVolume = chkFloodingLossVolumeMetric.Checked ? (double)nudFloodingLossVolumeCeof.Value : 0;
            //double ceofMeanFailureTime = chkMeanFailureTimeMetric.Checked ? (double)nudMeanFailureTimeCeof.Value : 0;
            //double ceofStorageCapacity = chkStorageCapacityMetric.Checked ? (double)nudStorageCapacityCeof.Value : 0;
            //double ceofStorageTime = chkStorageTimeMetric.Checked ? (double)nudStorageTimeCeof.Value : 0;
            //double ceofFailureTime = chkFailureTimeMetric.Checked ? (double)nudFailureTimeCeof.Value : 0;

            //foreach (var item in Summary)
            //{
            //    item.Score = ceofFailureTime * item.NormalizedFailureTime + ceofMeanFailureTime * item.NormalizedMeanFailureTime +
            //                 ceofStorageTime * item.NormalizedStorageTime + ceofStorageCapacity * item.NormalizedStorageCapacity +
            //                 ceofFloodingLossVolume * item.NormalizedFloodingLossVolume;
            //}


        }

        private void btnAnalysisDistinctLinks_Click(object sender, EventArgs e)
        {
            string[] ResultsDirectories = Directory.GetDirectories(txtResultsDirectory.Text).OrderBy(x => x, new OrdinalStringComparer()).ToArray();

            List<List<NodeFloodingSummary>> Summary = new List<List<NodeFloodingSummary>>();
            List<List<int>> SummaryLinksIndex = new List<List<int>>();

            for (int i = 2; i <= ResultsDirectories.Count(); i++)
            {
                string[] ResultsFiles = Directory.GetFiles(Path.Combine(txtResultsDirectory.Text, i.ToString()), "*.txt").OrderBy(x => x, new OrdinalStringComparer()).ToArray();
                Summary.Add(new List<NodeFloodingSummary>());
                SummaryLinksIndex.Add(new List<int>());

                for (int j = 1; j <= ResultsFiles.Count(); j++)
                {
                    lblAnalysis.Text = "# Scenario: " + i + ", # Failuring: " + j;
                    lblAnalysis.Refresh();

                    string[] lines = File.ReadAllLines(Path.Combine(txtResultsDirectory.Text, i.ToString(), j.ToString() + ".txt"), Encoding.UTF8);

                    double totalFlood = 0;

                    double diameter = Convert.ToDouble(lines[0].Split(' ')[2]);
                    string[] failureLinks = lines[1].Split(' ')[2].Split(',');

                    List<int> failureLinksIndex = new List<int>();
                    List<string> failureLinksID = new List<string>();


                    for (int f = 0; f < failureLinks.Count() - 1; f++)
                    {
                        int index = Convert.ToInt32(failureLinks[f].Split('(')[0]);
                        string id = (failureLinks[f].Split('(')[1]).Substring(0, failureLinks[f].Split('(')[1].Length - 1);

                        failureLinksIndex.Add(index);
                        failureLinksID.Add(id);
                    }

                    for (int k = 2; k < lines.Count(); k++)
                    {
                        string[] data = lines[k].Split(',');

                        switch (data[0])
                        {
                            case "Flooding Loss (gal)":
                                totalFlood = Convert.ToDouble(data[1]);
                                break;
                            default:
                                break;
                        }
                    }

                    NodeFloodingSummary nodeFloodingSummary = new NodeFloodingSummary()
                    {
                        isNoFlooding = true,
                        Diameter = diameter,
                        FailureLinksIndex = failureLinksIndex,
                        FailureLinksID = failureLinksID,
                        NodeFloodsInfo = new List<NodeFlood>(),
                        SumTotalFloodVolume = totalFlood,
                    };

                    Summary[i - 2].Add(nodeFloodingSummary);
                }
                Summary[i - 2] = Summary[i - 2].OrderByDescending(x => x.SumTotalFloodVolume).ToArray().Take(200).ToList();

                foreach (var item in Summary[i - 2])
                {
                    SummaryLinksIndex[i - 2].AddRange(item.FailureLinksIndex);
                }
            }
        }

        private void btnResilienceRanking_Click(object sender, EventArgs e)
        {
            string[] ResultsFiles = Directory.GetFiles(txtResResultsDirectory.Text, "*.txt").OrderBy(x => x, new OrdinalStringComparer()).ToArray();

            List<ResilienceMetrics> MetricsSummary = new List<ResilienceMetrics>();

            for (int i = 1; i <= ResultsFiles.Count(); i++)
            {
                ResilienceMetrics resilienceMetricsValue = new ResilienceMetrics();

                string[] lines = File.ReadAllLines(ResultsFiles[i - 1], Encoding.UTF8);

                double diameter = Convert.ToDouble(lines[0].Split(' ')[2]);
                string[] failureLinks = lines[1].Split(' ')[2].Split(',');

                List<int> failureLinksIndex = new List<int>();
                List<string> failureLinksID = new List<string>();


                for (int f = 0; f < failureLinks.Count() - 1; f++)
                {
                    int index = Convert.ToInt32(failureLinks[f].Split('(')[0]);
                    string id = (failureLinks[f].Split('(')[1]).Substring(0, failureLinks[f].Split('(')[1].Length - 1);

                    failureLinksIndex.Add(index);
                    failureLinksID.Add(id);
                }

                resilienceMetricsValue.FailureLinksIndex = failureLinksIndex;
                resilienceMetricsValue.FailureLinksID = failureLinksID;

                for (int k = 2; k < lines.Count(); k++)
                {
                    string[] data = lines[k].Split(',');

                    switch (data[0])
                    {
                        case "Flooding Loss (gal)":
                            resilienceMetricsValue.FloodingLossVolume = Convert.ToDouble(data[1]);
                            break;
                        case "Mean Failure Time":
                            resilienceMetricsValue.MeanFailureTime = Convert.ToDouble(data[1]);
                            break;
                        case "Diff OutfallInflow":
                            resilienceMetricsValue.StorageCapacity = Convert.ToDouble(data[1]);
                            break;
                        case "Flood (or End of failuring) Elapsed Time":
                            resilienceMetricsValue.StorageTime = Convert.ToDouble(data[1]);
                            resilienceMetricsValue.hasFlooding = true;
                            break;
                        case "Sum Hours Nodal Flooding":
                            resilienceMetricsValue.FailureTime = Convert.ToDouble(data[1]);
                            break;
                        case "Failure Diameter":
                            resilienceMetricsValue.MinNodeFloodingTime = 0;
                            resilienceMetricsValue.MaxNodeFloodingTime = 0;
                            resilienceMetricsValue.MinNodeFloodingVolume = 0;
                            resilienceMetricsValue.MaxNodeFloodingVolume = 0;
                            resilienceMetricsValue.FileIndex = 0;
                            break;
                        default:
                            break;
                    }
                }

                MetricsSummary.Add(resilienceMetricsValue);
            }

            //double maxFloodingLossVolume = 0;
            //double maxMeanFailureTime = 0;
            //double maxStorageCapacity = 0;
            //double maxStorageTime = 0;
            //double maxFailureTime = 0;

            //maxFloodingLossVolume = MetricsSummary.Max(x => x.FloodingLossVolume);
            //maxMeanFailureTime = MetricsSummary.Max(x => x.MeanFailureTime);
            //maxStorageCapacity = MetricsSummary.Max(x => x.StorageCapacity);
            //maxFailureTime = MetricsSummary.Max(x => x.FailureTime);
            //maxStorageTime = MetricsSummary.Max(x => x.StorageTime);

            //MetricsSummary.ForEach(x => x.NormalizedFloodingLossVolume = x.FloodingLossVolume / maxFloodingLossVolume);
            //MetricsSummary.ForEach(x => x.NormalizedMeanFailureTime = x.MeanFailureTime / maxMeanFailureTime);
            //MetricsSummary.ForEach(x => x.NormalizedStorageCapacity = (maxStorageCapacity - x.StorageCapacity) / maxStorageCapacity);
            //MetricsSummary.ForEach(x => x.NormalizedFailureTime = x.FailureTime / maxFailureTime);
            //MetricsSummary.ForEach(x => x.NormalizedStorageTime = (maxStorageTime - x.StorageTime) / maxStorageTime);

            //double ceofFloodingLossVolume = chkFloodingLossVolumeMetric.Checked ? (double)nudFloodingLossVolumeCeof.Value : 0;
            //double ceofMeanFailureTime = chkMeanFailureTimeMetric.Checked ? (double)nudMeanFailureTimeCeof.Value : 0;
            //double ceofStorageCapacity = chkStorageCapacityMetric.Checked ? (double)nudStorageCapacityCeof.Value : 0;
            //double ceofStorageTime = chkStorageTimeMetric.Checked ? (double)nudStorageTimeCeof.Value : 0;
            //double ceofFailureTime = chkFailureTimeMetric.Checked ? (double)nudFailureTimeCeof.Value : 0;

            //foreach (var item in MetricsSummary)
            //{
            //    item.Score = ceofFailureTime * item.NormalizedFailureTime + ceofMeanFailureTime * item.NormalizedMeanFailureTime +
            //                 ceofStorageTime * item.NormalizedStorageTime + ceofStorageCapacity * item.NormalizedStorageCapacity +
            //                 ceofFloodingLossVolume * item.NormalizedFloodingLossVolume;
            //}

            Matrix<double> options = DenseMatrix.OfRows(MetricsSummary.Select(x => new List<double>() { x.FailureTime, x.StorageTime, x.FloodingLossVolume, x.StorageCapacity }));

            ShannonEntropy shannon = new ShannonEntropy(MetricsSummary.Count, 4);

            shannon.Matrix = options;

            shannon.Normalize();
            shannon.CalculateEntropy();
            shannon.CalculateDistances();
            shannon.CalculateWeights();

            Topsis topsis = new Topsis(MetricsSummary.Count, 4, new bool[] { false, true, false, true });

            topsis.Matrix = options;

            topsis.Normalize();
            topsis.WeighingNormalizedMatrix(shannon.W);
            topsis.FindingIdealValues();
            topsis.CalculateDistances();
            topsis.CalculateSimilarity();

            // Create Metrics Summary.inf file

            Parallel.ForEach(topsis.Similarity, (similarity, pls, index) =>
            {
                MetricsSummary[(int)index].Score = similarity;
            });

            double topFailuringPercentage = (double)(nudTopFailuring.Value / 100);
            // true for (failureLinkCount == 2 || failureLinkCount == linksCount) and false for other folder
            int topCount = true ? MetricsSummary.Count : (int)(topFailuringPercentage * MetricsSummary.Count);

            List<ResilienceMetrics> TopMetricsSummary = MetricsSummary.OrderBy(x => x.Score).Take(topCount).ToList();

            var dirInfo = Directory.CreateDirectory(txtResResultsDirectory.Text);

            if (!File.Exists(Path.Combine(dirInfo.FullName, "Metrics Summary.inf")))
            {
                using (StreamWriter metricsSummaryFile = new StreamWriter(Path.Combine(dirInfo.FullName, "Metrics Summary.inf")))
                {
                    metricsSummaryFile.WriteLine(string.Join(",", shannon.W));

                    foreach (var metricsSummary in TopMetricsSummary)
                    {
                        metricsSummaryFile.Write(string.Join(",", metricsSummary.FailureLinksID));
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(string.Join(",", metricsSummary.FailureLinksIndex));
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.MeanFailureTime);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.FailureTime);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.StorageTime);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.FloodingLossVolume);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.StorageCapacity);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.MinNodeFloodingTime);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.MaxNodeFloodingTime);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.MinNodeFloodingVolume);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.MaxNodeFloodingVolume);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.Score);
                        metricsSummaryFile.Write(" ");
                        metricsSummaryFile.Write(metricsSummary.FileIndex);

                        metricsSummaryFile.WriteLine();
                    }
                }
            }
        }



        private void btnCompareShannonWeights_Click(object sender, EventArgs e)
        {
            string[] ResultsDirectories = Directory.GetDirectories(txtMetricResultsDirectory.Text).OrderBy(x => x, new OrdinalStringComparer()).ToArray();

            for (int i = 1; i <= ResultsDirectories.Count(); i++)
            {
                string[] lines = File.ReadAllLines(Path.Combine(ResultsDirectories[i - 1], "Metrics Summary.inf"), Encoding.UTF8);

                string[] weightsStrArray = lines[0].Split(',');

                double floodTimeWeight = Convert.ToDouble(weightsStrArray[0]);
                double storageTimeWeight = Convert.ToDouble(weightsStrArray[1]);
                double floodVolumeWeight = Convert.ToDouble(weightsStrArray[2]);
                double storageCapacityWeight = Convert.ToDouble(weightsStrArray[3]);


                chtShannonWeights.Series["Series1"].Points.AddXY(i, floodTimeWeight);
                chtShannonWeights.Series["Series2"].Points.AddXY(i, storageTimeWeight);
                chtShannonWeights.Series["Series3"].Points.AddXY(i, floodVolumeWeight);
                chtShannonWeights.Series["Series4"].Points.AddXY(i, storageCapacityWeight);

                chtShannonWeights.Update();
            }



        }

        private void btnMetricResultsDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtMetricResultsDirectory.Text = folderDlg.SelectedPath;
            }
        }

        private void btnCompareMetrics_Click(object sender, EventArgs e)
        {
            List<double> storageCapacityList = new List<double>();
            List<double> floodVolumeList = new List<double>();


            string[] lines = File.ReadAllLines(Path.Combine(txtMetricResultsDirectory.Text, "Metrics Summary.inf"), Encoding.UTF8);

            for (int j = 1; j < lines.Length; j++)
            {
                string[] values = lines[j].Split(' ');
                floodVolumeList.Add(Convert.ToDouble(values[5]));
                storageCapacityList.Add(Convert.ToDouble(values[6]));
            }

            var a = storageCapacityList.GroupBy(x => x)
            .Select(x => new
            {
                Count = x.Count(),
                Name = x.Key,
            })
            .OrderByDescending(x => x.Count);

            for (int i = 0; i < floodVolumeList.Count(); i++)
            {
                chtMetricsValues.Series["Series1"].Points.AddXY(i, floodVolumeList[i]);
                //chtMetricsValues.Series["Series2"].Points.AddXY(i, storageCapacityList[i]);

                chtStorageTime.Update();
            }

            //chtMetricsValues.ChartAreas[0].AxisY.Minimum = 0.42;
            //chtMetricsValues.ChartAreas[0].AxisY.Maximum = 0.44;

        }

        private void nudCoreNumber_ValueChanged(object sender, EventArgs e)
        {
            for (int k = 1; k <= nudCoreNumber.Value; k++)
            {
                File.Copy("swmm5.dll", "swmm5_" + k + ".dll", true);
            }

            for (int k = 1; k <= nudCoreNumber.Value; k++)
            {
                File.Copy("swmm5_original.dll", "swmm5_original_" + k + ".dll", true);
            }

            for (int k = 1; k <= nudCoreNumber.Value; k++)
            {
                File.Copy("EPA SWMM 5.1.013\\swmm5.exe", "EPA SWMM 5.1.013\\swmm5_" + k + ".exe", true);
            }
        }

        private void btnStart2_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStart2.Text = "Running";
            btnStart2.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                (chkMainDiameter.Checked && lines[li + line + 2].Split(' ')[0].Contains("M")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            List<ReportSummary> Summary = new List<ReportSummary>();

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);


            //ISWMMToolkit[] toolkitSWMMs = new ISWMMToolkit[cores];
            ManualResetEvent waitEndLoop = new ManualResetEvent(true);

            for (int i = 40; i <= linksCount; i++)
            {
                int failureLinkCount = i;

                List<int> toolkitSWMMIndexes = new List<int>();
                for (int t = 0; t < cores; t++)
                {
                    toolkitSWMMIndexes.Add(0);
                }

                waitEndLoop.Reset();

                int FileCountInFolder = 0;

                //if (InvokeRequired)
                //{
                //    Invoke((Action)delegate
                //    {
                //        lblScenarioCounter.Text = "# Failure Links: " + failureLinkCount;
                //            //lblScenarioCounter.Refresh();
                //        });
                //}

                var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                int failuringNumber = 1000;

                //if (failureLinkCount == 1)
                //    failuringNumber = linksCount;
                //else if (failureLinkCount == 2 || failureLinkCount == linksCount - 2)
                //    failuringNumber = 100;
                //else if (failureLinkCount == linksCount - 1)
                //    failuringNumber = linksCount;
                //else if (failureLinkCount == linksCount)
                //    failuringNumber = 1;
                //else
                //    failuringNumber = 100;

                for (int j_index = 1; j_index <= failuringNumber; j_index++)
                {

                    int j = j_index;
                    s.WaitOne();

                    Thread th = new Thread(delegate ()
                    {
                        int failuringCounter = j;
                        int toolkitSWMMIndex = -1;

                        try
                        {
                            lock (toolkitSWMMIndexes)  //for lock thread
                            {
                                toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                                toolkitSWMMIndexes[toolkitSWMMIndex] = failuringCounter;
                            }
                            List<int> randomFailureLinks = new List<int>();
                            List<int> failureLinkIndexes = new List<int>();
                            Random rand = new Random();

                            //Thread.Sleep(100 * failuringCounter);
                            //randomFailureLinks.AddRange(Enumerable.Range(1, linksCount).OrderBy(m => rand.Next(failuringCounter * 10 * linksCount, (failuringCounter + 1) * 10 * linksCount)).Take(failureLinkCount));
                            IEnumerable<int> range = Enumerable.Range(1, linksCount);
                            for (int k = 0; k < toolkitSWMMIndex; k++)
                            {
                                range = range.OrderBy(m => rand.Next());
                            }
                            randomFailureLinks.AddRange(range.Take(failureLinkCount));

                            //randomFailureLinks.Add(j);

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < randomFailureLinks.Count(); fl++)
                            {
                                currentFailureLinks += linkList[randomFailureLinks[fl] - 1].Index + "(" + linkList[randomFailureLinks[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[randomFailureLinks[fl] - 1].Index);
                            }

                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, j.ToString() + ".txt")))
                            {
                                //File.WriteAllText(Path.Combine(dirInfo.FullName, j.ToString() + ".txt"), "Failure Diameter: " + Diameter + "\n" + "Failure Links: " + currentFailureLinks + "\n");

                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                //StreamReader oReader;
                                //if (File.Exists(inputFilePath))
                                //{
                                //    string cSearforSomething = "Link";
                                //    oReader = new StreamReader(inputFilePath);
                                //    string cColl = oReader.ReadToEnd();
                                //    string cCriteria = @"\b" + cSearforSomething + @"\b";
                                //    System.Text.RegularExpressions.Regex oRegex = new
                                //    System.Text.RegularExpressions.Regex(cCriteria, RegexOptions.IgnoreCase);

                                //    var count = oRegex.Matches(cColl).AsQueryable();
                                //    Console.WriteLine(count.ToString());
                                //}
                                //Console.ReadLine();

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10();
                                    }

                                    double floodElapsedTime = 0;
                                    double floodOutfalInflow = 0;

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, j.ToString() + ".rpt"), "");

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        //StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter);
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter, "", ref floodElapsedTime, ref floodOutfalInflow);
                                        //////////////////////

                                        RoutingTotals routingTotals = new RoutingTotals();

                                        ErrorCode = toolkitSWMM.GetSystemRoutingStats(ref routingTotals);

                                        myReportFile.WriteLine("Dry Weather Inflow (gal)," + routingTotals.dwInflow * 1.0e6); // w_MGAL to w_GAL
                                        myReportFile.WriteLine("Flooding Loss (gal)," + routingTotals.flooding * 1.0e6); // w_MGAL to w_GAL

                                        //////////////////////

                                        int nodeCount = 0;
                                        toolkitSWMM.GetObjectCount(ObjectTypeEnum.NODE, ref nodeCount);

                                        int nodeIndex;
                                        int days = 0, hrs = 0, mins = 0, routeModel = 0;
                                        double t, vcf = 0;
                                        string nodeID = "";
                                        bool hasFlood = false;

                                        NodeFloodingSummary nodeFloodingSummary;

                                        nodeFloodingSummary = new NodeFloodingSummary()
                                        {
                                            isNoFlooding = false,
                                            Diameter = Diameter,
                                            FailureLinksIndex = failureLinkIndexes,
                                            NodeFloodsInfo = new List<NodeFlood>(),
                                        };



                                        double sumHoursNodalFlooding = 0;
                                        int numNodalFlooding = 0;

                                        double maxNodeFloodingVolume = 0, minNodeFloodingVolume = double.MaxValue;
                                        double maxNodeFloodingTime = 0, minNodeFloodingTime = double.MaxValue;

                                        for (nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                                        {
                                            int nodeType = -1;
                                            toolkitSWMM.GetNodeType(nodeIndex, ref nodeType);

                                            if ((NodeTypeEnum)nodeType == NodeTypeEnum.OUTFALL) continue;

                                            NodeStats nodeStats = new NodeStats();
                                            toolkitSWMM.GetNodeStats(nodeIndex, ref nodeStats);

                                            if (nodeStats.timeFlooded == 0.0) continue;

                                            t = Math.Max(0.01, nodeStats.timeFlooded);

                                            hasFlood = true;

                                            NodeFlood nodeFlood = new NodeFlood();

                                            nodeFlood.Index = nodeIndex;

                                            toolkitSWMM.GetObjectId((int)ObjectTypeEnum.NODE, nodeIndex, ref nodeID);
                                            nodeFlood.ID = nodeID;
                                            nodeFlood.HoursFlooded = t;

                                            nodeFlood.MaximumRate = nodeStats.maxOverflow;

                                            toolkitSWMM.GetElapsedTime(nodeStats.maxOverflowDate, ref days, ref hrs, ref mins);

                                            nodeFlood.TimeOfMaxOccurrence_days = days;
                                            nodeFlood.TimeOfMaxOccurrence_hrs = hrs;
                                            nodeFlood.TimeOfMaxOccurrence_mins = mins;

                                            toolkitSWMM.GetVcf(ref vcf);
                                            nodeFlood.TotalFloodVolume = nodeStats.volFlooded * vcf * 1.0e6; // w_MGAL to w_GAL

                                            toolkitSWMM.GetRouteModel(ref routeModel);

                                            if ((RouteModelTypeEnum)routeModel == RouteModelTypeEnum.DW)
                                            {
                                                double fullDepth = 0;
                                                toolkitSWMM.GetNodeParam(nodeIndex, (int)NodeProperty.FULLDEPTH, ref fullDepth);
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxDepth - fullDepth;
                                            }
                                            else
                                            {
                                                //? / 1000
                                                nodeFlood.MaximumPondedVolume = nodeStats.maxPondedVol / 1000.0;
                                            }

                                            nodeFlood.NodeStats = nodeStats;

                                            myReportFile.WriteLine("NodeFlooding," + nodeFlood.Index + "," + nodeFlood.ID + "," +
                                                nodeFlood.HoursFlooded.ToString("0.###") + "," + nodeFlood.MaximumRate.ToString("0.###") + "," +
                                                nodeFlood.TimeOfMaxOccurrence_days + "," + nodeFlood.TimeOfMaxOccurrence_hrs + "," + nodeFlood.TimeOfMaxOccurrence_mins + "," +
                                                nodeFlood.TotalFloodVolume.ToString("0.###") + "," + nodeFlood.MaximumPondedVolume.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.avgDepth.ToString("0.###") + "," + nodeFlood.NodeStats.maxDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.maxDepthDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxRptDepth.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.volFlooded.ToString("0.###") + "," + nodeFlood.NodeStats.timeFlooded.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.timeSurcharged.ToString("0.###") + "," + nodeFlood.NodeStats.timeCourantCritical.ToString("0.###") + "," +
                                                nodeFlood.NodeStats.totLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxLatFlow.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflow.ToString("0.###") +
                                                nodeFlood.NodeStats.maxOverflow.ToString("0.###") + "," + nodeFlood.NodeStats.maxPondedVol.ToString("0.###") + "," + nodeFlood.NodeStats.maxInflowDate.ToLongTimeString() + "," + nodeFlood.NodeStats.maxOverflowDate.ToLongTimeString());

                                            nodeFloodingSummary.NodeFloodsInfo.Add(nodeFlood);
                                            nodeFloodingSummary.SumTotalFloodVolume += nodeFlood.TotalFloodVolume;

                                            sumHoursNodalFlooding += nodeFlood.HoursFlooded;
                                            if (nodeFlood.HoursFlooded > 0)
                                            {
                                                numNodalFlooding++;

                                                if (nodeFlood.TotalFloodVolume > maxNodeFloodingVolume)
                                                {
                                                    maxNodeFloodingVolume = nodeFlood.TotalFloodVolume;
                                                }
                                                if (nodeFlood.TotalFloodVolume < minNodeFloodingVolume)
                                                {
                                                    minNodeFloodingVolume = nodeFlood.TotalFloodVolume;
                                                }

                                                if (nodeFlood.HoursFlooded > maxNodeFloodingTime)
                                                {
                                                    maxNodeFloodingTime = nodeFlood.HoursFlooded;
                                                }
                                                if (nodeFlood.HoursFlooded < minNodeFloodingTime)
                                                {
                                                    minNodeFloodingTime = nodeFlood.HoursFlooded;
                                                }
                                            }
                                        }

                                        if (!hasFlood)
                                        {
                                            nodeFloodingSummary.isNoFlooding = true;
                                        }

                                        if (numNodalFlooding != 0)
                                        {
                                            double meanNodalFloodingTime = sumHoursNodalFlooding / numNodalFlooding;

                                            myReportFile.WriteLine("Sum Hours Nodal Flooding," + sumHoursNodalFlooding);
                                            myReportFile.WriteLine("Mean Failure Time," + meanNodalFloodingTime);

                                        }

                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();
                                    // Close the system
                                    toolkitSWMM.Close();
                                    //return error_getCode(ErrorCode);
                                }
                                catch (Exception ex)
                                {
                                    // Log error.
                                }
                            }
                        }
                        finally
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                                FileCountInFolder++;

                                if (FileCountInFolder == failuringNumber)
                                {
                                    waitEndLoop.Set();
                                }
                            }

                            s.Release();
                        }


                    });

                    th.Name = "Failure scenario: " + j;
                    th.IsBackground = true;
                    th.Priority = ThreadPriority.AboveNormal;
                    //th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                }

                waitEndLoop.WaitOne(60 * 60 * 1000, false);
            }

            btnStart2.Text = "Start";
            btnStart2.Refresh();
        }

        private void btnStartCombinationEXE_Click(object sender, EventArgs e)
        {
            DateTime start = DateTime.Now;

            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartCombinationEXE.Text = "Running";
            btnStartCombinationEXE.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> AllLinkList = new List<Link>();
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                            AllLinkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            //
            lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            int lineNumberOfLinks = 0;

            foreach (var line in lines)
            {
                if (line.Contains(";;Link"))
                {
                    break;
                }
                lineNumberOfLinks++;
            }

            int positionOfGeom1 = lines[lineNumberOfLinks].IndexOf("Geom1");


            lineNumberOfLinks++; // ;;--------------


            //

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);

            ManualResetEvent waitEndLoop = new ManualResetEvent(true);

            for (int i = 1; i <= linksCount; i++)
            {
                int failureLinkCount = i;

                List<int> toolkitSWMMIndexes = new List<int>();
                for (int t = 0; t < cores; t++)
                {
                    toolkitSWMMIndexes.Add(0);
                }

                var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                List<List<int>> targetCombinations = new List<List<int>>();

                if (failureLinkCount == 1)
                {
                    targetCombinations = Enumerable.Range(1, linksCount).Select(x => (new int[] { x }).ToList()).ToList();
                }
                else
                {
                    targetCombinations = GetKCombs(Enumerable.Range(1, linksCount), failureLinkCount).Select(x => x.ToList()).ToList();
                }


                int failuringNumber = targetCombinations.Count();

                waitEndLoop.Reset();

                int FileCountInFolder = 0;

                for (int j_index = 1; j_index <= failuringNumber; j_index++)
                {
                    int j = j_index;
                    s.WaitOne();

                    Thread th = new Thread(delegate ()
                    {
                        int failuringCounter = j;
                        int toolkitSWMMIndex = -1;

                        try
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                                toolkitSWMMIndexes[toolkitSWMMIndex] = failuringCounter;
                            }

                            List<int> failureLinkIndexes = new List<int>();
                            List<int> targetCombination = targetCombinations[failuringCounter - 1];

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < targetCombination.Count(); fl++)
                            {
                                currentFailureLinks += linkList[targetCombination[fl] - 1].Index + "(" + linkList[targetCombination[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[targetCombination[fl] - 1].Index);
                            }

                            string[] failureLinksInputFile = new string[lines.Count()];
                            lines.CopyTo(failureLinksInputFile, 0);

                            foreach (var failureLink in targetCombination)
                            {
                                string[] linkInfoSplite = failureLinksInputFile[lineNumberOfLinks + failureLink].Split(' ');
                                linkInfoSplite = linkInfoSplite.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                                string linkName = linkInfoSplite[0];
                                string linkGeom1 = linkInfoSplite[2];

                                int ValueLength = Diameter.ToString().Length;
                                if (ValueLength > linkGeom1.Length)
                                {
                                    Regex regReplace = new Regex(linkGeom1 + new string(' ', ValueLength - linkGeom1.Length));
                                    failureLinksInputFile[lineNumberOfLinks + failureLink] =
                                        regReplace.Replace(failureLinksInputFile[lineNumberOfLinks + failureLink], Diameter.ToString(), 1, positionOfGeom1);

                                }
                                else if (ValueLength < linkGeom1.Length)
                                {
                                    Regex regReplace = new Regex(linkGeom1);
                                    failureLinksInputFile[lineNumberOfLinks + failureLink] =
                                        regReplace.Replace(failureLinksInputFile[lineNumberOfLinks + failureLink], Diameter +
                                        new string(' ', linkGeom1.Length - 1), 1, positionOfGeom1);
                                }
                                else
                                {
                                    Regex regReplace = new Regex(linkGeom1);
                                    failureLinksInputFile[lineNumberOfLinks + failureLink] =
                                        regReplace.Replace(failureLinksInputFile[lineNumberOfLinks + failureLink], Diameter.ToString(), 1, positionOfGeom1);
                                }
                            }

                            File.WriteAllLines(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".inp"), failureLinksInputFile);


                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".txt")))
                            {
                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                // Use ProcessStartInfo class
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = false;
                                startInfo.FileName = "EPA SWMM 5.1.013\\swmm5_" + (toolkitSWMMIndex + 1) + ".exe";
                                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                startInfo.Arguments = Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".inp") + " " + Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt") + " " + "";

                                try
                                {
                                    // Start the process with the info we specified.
                                    // Call WaitForExit and then the using statement will close.
                                    using (Process exeProcess = Process.Start(startInfo))
                                    {
                                        exeProcess.WaitForExit();

                                        if (File.Exists(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt")))
                                        {
                                            string[] rptLines = File.ReadAllLines(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt"), Encoding.UTF8);

                                            int lineNumberOfFlooding = 0;

                                            foreach (var line in rptLines)
                                            {
                                                if (line.Contains("Flooding Loss"))
                                                {
                                                    break;
                                                }
                                                lineNumberOfFlooding++;
                                            }

                                            string[] spliteFloodingLine = rptLines[lineNumberOfFlooding].Split(' ');

                                            myReportFile.WriteLine("Flooding Loss (gal)," + spliteFloodingLine.ElementAt(spliteFloodingLine.Count() - 1)); // w_MGAL to w_GAL
                                        }
                                    }
                                }
                                catch
                                {
                                    // Log error.
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error.
                        }
                        finally
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                                FileCountInFolder++;

                                if (FileCountInFolder == failuringNumber)
                                {
                                    waitEndLoop.Set();
                                }
                            }

                            s.Release();
                        }

                    });

                    th.Name = "Failure scenario: " + i;
                    th.IsBackground = true;
                    th.Priority = ThreadPriority.AboveNormal;
                    //th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                }

                waitEndLoop.WaitOne(60 * 60 * 1000, false);
            }

            DateTime end = DateTime.Now;

            double totalTime = (end - start).TotalSeconds;
            using (StreamWriter runTime = new StreamWriter(Path.Combine(outputFolderPath, "runTime.txt")))
            {
                runTime.WriteLine(totalTime);
            }

            btnStartCombinationEXE.Text = "Start";
            btnStartCombinationEXE.Refresh();
        }

        private void btnStartCombinationDLL_Click(object sender, EventArgs e)
        {
            DateTime start = DateTime.Now;

            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartCombinationOSWMMDLL.Text = "Running";
            btnStartCombinationOSWMMDLL.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> AllLinkList = new List<Link>();
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                            AllLinkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);

            ManualResetEvent waitEndLoop = new ManualResetEvent(true);

            for (int i = 1; i <= 3; i++)
            {
                int failureLinkCount = i;

                List<int> toolkitSWMMIndexes = new List<int>();
                for (int t = 0; t < cores; t++)
                {
                    toolkitSWMMIndexes.Add(0);
                }

                var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                List<List<int>> targetCombinations = new List<List<int>>();

                if (failureLinkCount == 1)
                {
                    targetCombinations = Enumerable.Range(1, linksCount).Select(x => (new int[] { x }).ToList()).ToList();
                }
                else
                {
                    targetCombinations = GetKCombs(Enumerable.Range(1, linksCount), failureLinkCount).Select(x => x.ToList()).ToList();
                }


                int failuringNumber = targetCombinations.Count();

                waitEndLoop.Reset();

                int FileCountInFolder = 0;

                for (int j_index = 1; j_index <= failuringNumber; j_index++)
                {
                    int j = j_index;
                    s.WaitOne();

                    Thread th = new Thread(delegate ()
                    {
                        int failuringCounter = j;
                        int toolkitSWMMIndex = -1;

                        try
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                                toolkitSWMMIndexes[toolkitSWMMIndex] = failuringCounter;
                            }

                            List<int> failureLinkIndexes = new List<int>();
                            List<int> targetCombination = targetCombinations[failuringCounter - 1];

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < targetCombination.Count(); fl++)
                            {
                                currentFailureLinks += linkList[targetCombination[fl] - 1].Index + "(" + linkList[targetCombination[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[targetCombination[fl] - 1].Index);
                            }

                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".txt")))
                            {
                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10();
                                    }

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt"), "");

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, failureLinkIndexes, Diameter);

                                        //////////////////////

                                        RoutingTotals routingTotals = new RoutingTotals();

                                        //ErrorCode = toolkitSWMM.GetSystemRoutingStats(ref routingTotals);

                                        myReportFile.WriteLine("Dry Weather Inflow (gal)," + routingTotals.dwInflow * 1.0e6); // w_MGAL to w_GAL
                                        myReportFile.WriteLine("Flooding Loss (gal)," + routingTotals.flooding * 1.0e6); // w_MGAL to w_GAL

                                        //////////////////////////////
                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();

                                    // Close the system
                                    toolkitSWMM.Close();

                                    //return error_getCode(ErrorCode);
                                }
                                catch (Exception ex)
                                {
                                    // Log error.
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            // Log error.
                        }
                        finally
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                                FileCountInFolder++;

                                if (FileCountInFolder == failuringNumber)
                                {
                                    waitEndLoop.Set();
                                }
                            }

                            s.Release();
                        }

                    });

                    th.Name = "Failure scenario: " + i;
                    th.IsBackground = true;
                    th.Priority = ThreadPriority.AboveNormal;
                    //th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                }

                waitEndLoop.WaitOne(60 * 60 * 1000, false);
            }

            DateTime end = DateTime.Now;

            double totalTime = (end - start).TotalSeconds;
            using (StreamWriter runTime = new StreamWriter(Path.Combine(outputFolderPath, "runTime.txt")))
            {
                runTime.WriteLine(totalTime);
            }

            btnStartCombinationOSWMMDLL.Text = "Start";
            btnStartCombinationOSWMMDLL.Refresh();
        }

        private void btnStartCominationOriginalDLL_Click(object sender, EventArgs e)
        {
            DateTime start = DateTime.Now;

            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartCominationOriginalDLL.Text = "Running";
            btnStartCominationOriginalDLL.Refresh();
            double Diameter = Convert.ToDouble(numFailureDiameter.Value);

            string fileContent;
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                fileContent = reader.ReadToEnd();
            }

            string[] lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            List<Link> AllLinkList = new List<Link>();
            List<Link> linkList = new List<Link>();
            bool readLink = false;

            for (int line = 0; line < lines.Count(); line++)
            {
                if (lines[line].Contains(";;Link"))
                {
                    // ;;--------------
                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].Contains("N")) ||
                                chkOtherLinks.Checked)
                            {
                                linkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                            }
                            AllLinkList.Add(new Link(li, lines[li + line + 2].Split(' ')[0]));
                        }
                    }
                }
                if (readLink)
                {
                    break;
                }
            }

            int linksCount = linkList.Count();

            //
            lines = File.ReadAllLines(inputFilePath, Encoding.UTF8);
            int lineNumberOfLinks = 0;

            foreach (var line in lines)
            {
                if (line.Contains(";;Link"))
                {
                    break;
                }
                lineNumberOfLinks++;
            }

            int positionOfGeom1 = lines[lineNumberOfLinks].IndexOf("Geom1");


            lineNumberOfLinks++; // ;;--------------


            //

            var cores = (int)nudCoreNumber.Value;// Environment.ProcessorCount;
            Semaphore s = new Semaphore(cores, cores);

            ManualResetEvent waitEndLoop = new ManualResetEvent(true);

            for (int i = 1; i <= 3; i++)
            {
                int failureLinkCount = i;

                List<int> toolkitSWMMIndexes = new List<int>();
                for (int t = 0; t < cores; t++)
                {
                    toolkitSWMMIndexes.Add(0);
                }

                var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                List<List<int>> targetCombinations = new List<List<int>>();

                if (failureLinkCount == 1)
                {
                    targetCombinations = Enumerable.Range(1, linksCount).Select(x => (new int[] { x }).ToList()).ToList();
                }
                else
                {
                    targetCombinations = GetKCombs(Enumerable.Range(1, linksCount), failureLinkCount).Select(x => x.ToList()).ToList();
                }


                int failuringNumber = targetCombinations.Count();

                waitEndLoop.Reset();

                int FileCountInFolder = 0;

                for (int j_index = 1; j_index <= failuringNumber; j_index++)
                {
                    int j = j_index;
                    s.WaitOne();

                    Thread th = new Thread(delegate ()
                    {
                        int failuringCounter = j;
                        int toolkitSWMMIndex = -1;

                        try
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                                toolkitSWMMIndexes[toolkitSWMMIndex] = failuringCounter;
                            }

                            List<int> failureLinkIndexes = new List<int>();
                            List<int> targetCombination = targetCombinations[failuringCounter - 1];

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < targetCombination.Count(); fl++)
                            {
                                currentFailureLinks += linkList[targetCombination[fl] - 1].Index + "(" + linkList[targetCombination[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[targetCombination[fl] - 1].Index);
                            }

                            string[] failureLinksInputFile = new string[lines.Count()];
                            lines.CopyTo(failureLinksInputFile, 0);

                            foreach (var failureLink in targetCombination)
                            {
                                string[] linkInfoSplite = failureLinksInputFile[lineNumberOfLinks + failureLink].Split(' ');
                                linkInfoSplite = linkInfoSplite.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                                string linkName = linkInfoSplite[0];
                                string linkGeom1 = linkInfoSplite[2];

                                int ValueLength = Diameter.ToString().Length;
                                if (ValueLength > linkGeom1.Length)
                                {
                                    Regex regReplace = new Regex(linkGeom1 + new string(' ', ValueLength - linkGeom1.Length));
                                    failureLinksInputFile[lineNumberOfLinks + failureLink] =
                                        regReplace.Replace(failureLinksInputFile[lineNumberOfLinks + failureLink], Diameter.ToString(), 1, positionOfGeom1);

                                }
                                else if (ValueLength < linkGeom1.Length)
                                {
                                    Regex regReplace = new Regex(linkGeom1);
                                    failureLinksInputFile[lineNumberOfLinks + failureLink] =
                                        regReplace.Replace(failureLinksInputFile[lineNumberOfLinks + failureLink], Diameter +
                                        new string(' ', linkGeom1.Length - 1), 1, positionOfGeom1);
                                }
                                else
                                {
                                    Regex regReplace = new Regex(linkGeom1);
                                    failureLinksInputFile[lineNumberOfLinks + failureLink] =
                                        regReplace.Replace(failureLinksInputFile[lineNumberOfLinks + failureLink], Diameter.ToString(), 1, positionOfGeom1);
                                }
                            }

                            File.WriteAllLines(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".inp"), failureLinksInputFile);


                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".txt")))
                            {
                                myReportFile.WriteLine("Failure Diameter: " + Diameter);
                                myReportFile.WriteLine("Failure Links: " + currentFailureLinks);

                                try
                                {
                                    int ErrorCode = 0;

                                    ISWMMToolkit_Original toolkitSWMM = null;

                                    if (toolkitSWMMIndex == 0)
                                    {
                                        toolkitSWMM = new SWMMToolkit_1_Original();
                                    }
                                    else if (toolkitSWMMIndex == 1)
                                    {
                                        toolkitSWMM = new SWMMToolkit_2_Original();
                                    }
                                    else if (toolkitSWMMIndex == 2)
                                    {
                                        toolkitSWMM = new SWMMToolkit_3_Original();
                                    }
                                    else if (toolkitSWMMIndex == 3)
                                    {
                                        toolkitSWMM = new SWMMToolkit_4_Original();
                                    }
                                    else if (toolkitSWMMIndex == 4)
                                    {
                                        toolkitSWMM = new SWMMToolkit_5_Original();
                                    }
                                    else if (toolkitSWMMIndex == 5)
                                    {
                                        toolkitSWMM = new SWMMToolkit_6_Original();
                                    }
                                    else if (toolkitSWMMIndex == 6)
                                    {
                                        toolkitSWMM = new SWMMToolkit_7_Original();
                                    }
                                    else if (toolkitSWMMIndex == 7)
                                    {
                                        toolkitSWMM = new SWMMToolkit_8_Original();
                                    }
                                    else if (toolkitSWMMIndex == 8)
                                    {
                                        toolkitSWMM = new SWMMToolkit_9_Original();
                                    }
                                    else if (toolkitSWMMIndex == 9)
                                    {
                                        toolkitSWMM = new SWMMToolkit_10_Original();
                                    }

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".inp"), Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt"), "");

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, failureLinkIndexes, Diameter);

                                        // --- clean up
                                        toolkitSWMM.End();
                                    }

                                    // --- report results
                                    toolkitSWMM.Report();

                                    // Close the system
                                    toolkitSWMM.Close();

                                    //return error_getCode(ErrorCode);

                                    if (File.Exists(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt")))
                                    {
                                        string[] rptLines = File.ReadAllLines(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".rpt"), Encoding.UTF8);

                                        int lineNumberOfFlooding = 0;

                                        foreach (var line in rptLines)
                                        {
                                            if (line.Contains("Flooding Loss"))
                                            {
                                                break;
                                            }
                                            lineNumberOfFlooding++;
                                        }

                                        string[] spliteFloodingLine = rptLines[lineNumberOfFlooding].Split(' ');

                                        myReportFile.WriteLine("Flooding Loss (gal)," + spliteFloodingLine.ElementAt(spliteFloodingLine.Count() - 1)); // w_MGAL to w_GAL
                                    }
                                }
                                catch
                                {
                                    // Log error.
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error.
                        }
                        finally
                        {
                            lock (toolkitSWMMIndexes)
                            {
                                toolkitSWMMIndexes[toolkitSWMMIndex] = 0;
                                FileCountInFolder++;

                                if (FileCountInFolder == failuringNumber)
                                {
                                    waitEndLoop.Set();
                                }
                            }

                            s.Release();
                        }

                    });

                    th.Name = "Failure scenario: " + i;
                    th.IsBackground = true;
                    th.Priority = ThreadPriority.AboveNormal;
                    //th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                }

                waitEndLoop.WaitOne(60 * 60 * 1000, false);
            }

            DateTime end = DateTime.Now;

            double totalTime = (end - start).TotalSeconds;
            using (StreamWriter runTime = new StreamWriter(Path.Combine(outputFolderPath, "runTime.txt")))
            {
                runTime.WriteLine(totalTime);
            }
            btnStartCominationOriginalDLL.Text = "Start";
            btnStartCominationOriginalDLL.Refresh();

        }
    }

}

