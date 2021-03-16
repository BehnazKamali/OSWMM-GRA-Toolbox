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
using SWMMData.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Threading.Tasks;
using System.Reflection;

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

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "O-SWMM GRA Toolbox " + version.Substring(0, version.ToString().LastIndexOf('.'));

            lblMaxCores.Text = "(Maximum: " + Environment.ProcessorCount + ")";

            nudCoreNumber_ValueChanged(this, null);

            this.rtbTopFlooding.BackColor = System.Drawing.Color.LemonChiffon;
            this.rtbTopFlooding.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbTopFlooding.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.rtbTopFlooding.Location = new System.Drawing.Point(388, 54);
            this.rtbTopFlooding.Name = "rtbTopFlooding";
            this.rtbTopFlooding.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbTopFlooding.Size = new System.Drawing.Size(120, 23);
            this.rtbTopFlooding.TabIndex = 73;
            this.rtbTopFlooding.Text = "k TF (0<value<1)";
            this.rtbTopFlooding.SelectionStart = 2;
            this.rtbTopFlooding.SelectionLength = 2;
            this.rtbTopFlooding.SelectionCharOffset = -9;
            this.rtbTopFlooding.SelectionLength = 0;
        }

        private void btnSelectFileName_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "input files (*.inp)|*.inp";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputFileName.Text = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();
                }
            }
        }

        private void btnOutputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtOutputFolder.Text = folderDlg.SelectedPath;
            }
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


            // --- initialize values
            errorCode = toolkitSWMM.Start(saveFlag);

            // --- execute each time step until elapsed time is re-set to 0
            if (errorCode == 0)
            {

                do
                {
                    toolkitSWMM.Step(ref elapsedTime);

                } while (elapsedTime > 0.0 && errorCode == 0);

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

                do
                {
                    toolkitSWMM.Step(ref elapsedTime);
                    newHour = (long)(elapsedTime * 24.0);
                    if (newHour >= oldHour)
                    {
                        theDay = (long)elapsedTime;
                        theHour = (long)((elapsedTime - Math.Floor(elapsedTime)) * 24.0);

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
            totalInflow = routingTotals.dwInflow * 1.0e6;

            toolkitSWMM.GetNodeTotalInflow(nodeOutfallIndex, ref outfallInflow);
            outfallInflow = outfallInflow * vcf * 1.0e6;

            outfallInflowList.Add("0" + " " + outfallInflow.ToString("0.###") + " " + "0");
            totalInflowList.Add("0" + " " + outfallInflow.ToString("0.###") + " " + "0");



            if (nudAVGDWFCoef.Value != 1)
            {
                toolkitSWMM.SetAverageDWFChangingCoef((double)nudAVGDWFCoef.Value);
            }

            
            if (errorCode == 0)
            {

                do
                {
                    toolkitSWMM.Step(ref elapsedTime);
                    newHour = (long)(elapsedTime * 24.0);
                    newMinutes = (long)((elapsedTime * 24.0 * 60.0) % 60);

                    if (newHour >= oldHour)
                    {
                        int isFlooded = 0;
                        toolkitSWMM.GetOccuredNodeFlooding(ref isFlooded);



                        toolkitSWMM.GetNodeTotalInflow(nodeOutfallIndex, ref outfallInflow);
                        outfallInflow = outfallInflow * vcf * 1.0e6;

                        if (isFlooded == 1 && floodElapsedTime == 0)
                        {
                            floodElapsedTime = elapsedTime;
                            floodOutfallInflow = outfallInflow;
                        }


                    }

                    if (newHour >= oldHour)
                    {
                        theDay = (long)elapsedTime;
                        theHour = (long)((elapsedTime - Math.Floor(elapsedTime)) * 24.0);

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
                                    outfallInflow = outfallInflow * vcf * 1.0e6;

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
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtResultsDirectory.Text = folderDlg.SelectedPath;
            }
        }

        private void btnAnalysis_Click(object sender, EventArgs e)
        {
            string[] ResultsDirectories = Directory.GetDirectories(txtResultsDirectory.Text).OrderBy(x => x, new OrdinalStringComparer()).ToArray();



            using (StreamWriter summaryFile = new StreamWriter(Path.Combine(txtResultsDirectory.Text, "summary.res")))
            {
                for (int i = 1; i <= ResultsDirectories.Count(); i++)
                {
                    string[] ResultsFiles = Directory.GetFiles(Path.Combine(txtResultsDirectory.Text, i.ToString()), "*.txt").OrderBy(x => x, new OrdinalStringComparer()).ToArray();
                    List<Tuple<long, double, double, int>> Summary = new List<Tuple<long, double, double, int>>();

                    double minResilience, meanResilience, maxResilience;
                    double minResilienceWithStorage, meanResilienceWithStorage, maxResilienceWithStorage;

                    lblAnalysis.Text = "# Scenario: " + i;
                    lblAnalysis.Refresh();


                    Parallel.ForEach(ResultsFiles, (resultsFile, state, index) =>
                    {
                        string[] lines = File.ReadAllLines(resultsFile, Encoding.UTF8);

                        double resilience = 0;
                        double resilienceWithStorage = 0;
                        double sumFloodMultipleTime = 0;
                        double totalFlood = 0;
                        double totalInflow = 0;
                        double meanHoursTotalFlood = 0;
                        double storageTime = 0;
                        double storageCapacity = 0;

                        double diameter = 0;

                        List<int> failureLinksIndex = new List<int>();
                        List<string> failureLinksID = new List<string>();

                        int numNodeFlooding = 0;

                        NodeFloodingSummary nodeFloodingSummary = new NodeFloodingSummary()
                        {
                            isNoFlooding = true,
                            Diameter = diameter,
                            FailureLinksIndex = failureLinksIndex,
                            FailureLinksID = failureLinksID,
                            NodeFloodsInfo = new List<NodeFlood>(),
                        };


                        int totalFailureTime = (int)nudTotalFailureTime.Value;


                        double TotalInflow = (double)nudTotalInflow.Value;


                        for (int k = 0; k < lines.Count(); k++)
                        {
                            string[] data = lines[k].Split(',');

                            switch (data[0])
                            {
                                case "NodeFlooding":
                                    numNodeFlooding++;


                                    double HoursFlooded = Convert.ToDouble(data[3]);
                                    double TotalFloodVolume = Convert.ToDouble(data[8]);


                                    sumFloodMultipleTime += (TotalFloodVolume / TotalInflow) * (HoursFlooded / totalFailureTime);

                                    break;

                                case "Dry Weather Inflow (gal)":
                                    totalInflow = Convert.ToDouble(data[1]);
                                    break;
                                case "Flooding Loss (gal)":
                                    totalFlood = Convert.ToDouble(data[1]);
                                    break;
                                case "Mean Failure Time":
                                    meanHoursTotalFlood = Convert.ToDouble(data[1]);
                                    break;
                                case "Flood (or End of failuring) Elapsed Time":
                                    storageTime = Convert.ToDouble(data[1]);
                                    break;
                                case "Diff OutfallInflow":
                                    storageCapacity = Convert.ToDouble(data[1]);
                                    break;
                                default:
                                    break;
                            }
                        }


                        resilience = 1 - sumFloodMultipleTime;

                        lock (Summary)
                        {
                            Summary.Add(new Tuple<long, double, double, int>(index, resilience, resilienceWithStorage, numNodeFlooding));
                        }

                    });


                    minResilience = Summary.Min(x => x.Item2);
                    meanResilience = Summary.Sum(x => x.Item2) / Summary.Count();
                    maxResilience = Summary.Max(x => x.Item2);

                    minResilienceWithStorage = Summary.Min(x => x.Item3);
                    meanResilienceWithStorage = Summary.Sum(x => x.Item3) / Summary.Count();
                    maxResilienceWithStorage = Summary.Max(x => x.Item3);

                    long indexOfnumNodeFloodingOfminResilience = Summary.FirstOrDefault(x => x.Item2 == minResilience).Item1;
                    int numNodeFloodingOfminResilience = Summary.FirstOrDefault(x => x.Item2 == minResilience).Item4;


                    summaryFile.WriteLine(minResilience + " ," + meanResilience + ", " + maxResilience + ", " + minResilienceWithStorage + " ," + meanResilienceWithStorage + ", " + maxResilienceWithStorage + ", " + indexOfnumNodeFloodingOfminResilience + ", " + numNodeFloodingOfminResilience + ";");

                    chtResilience.Series["Series1"].Points.AddXY(i, minResilience);
                    chtResilience.Series["Series2"].Points.AddXY(i, meanResilience);
                    chtResilience.Series["Series3"].Points.AddXY(i, maxResilience);
                    chtResilience.Update();
                }
            }


        }



        private void btnStartSequential_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartSequential.Text = "Running";
            btnStartSequential.Refresh();
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
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("N")) ||
                                chkAllLinks.Checked)
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

            for (int i = 0; i < (int)nudSequentialFailuring.Value; i++)
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
                        lock (toolkitSWMMIndexes)  
                        {
                            toolkitSWMMIndex = toolkitSWMMIndexes.FindIndex(x => x == 0);
                            toolkitSWMMIndexes[toolkitSWMMIndex] = failureLinkCount;
                        }


                       

                        var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                        int failuringNumber = (int)nudSequentialFailuring.Value;

                        



                        for (int j = 1; j <= failuringNumber; j++)
                        {
                            List<int> randomFailureLinks = new List<int>();
                            List<int> failureLinkIndexes = new List<int>();
                            Random rand = new Random();

                            

                            for (int i_Consecutive = 0; i_Consecutive < failureLinkCount; i_Consecutive++)
                            {
                                randomFailureLinks.Add(randomFailureLinksConsecutiveList[j - 1][i_Consecutive]);
                            }

                          

                            string currentFailureLinks = "";

                            for (int fl = 0; fl < randomFailureLinks.Count(); fl++)
                            {
                                currentFailureLinks += linkList[randomFailureLinks[fl] - 1].Index + "(" + linkList[randomFailureLinks[fl] - 1].ID + ")" + ",";
                                failureLinkIndexes.Add(linkList[randomFailureLinks[fl] - 1].Index);
                            }

                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, j.ToString() + ".txt")))
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
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, j.ToString() + ".rpt"), "");

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

                                       
                                        if (numNodalFlooding != 0)
                                        {
                                            double meanNodalFlooding = sumHoursNodalFlooding / numNodalFlooding;

                                            myReportFile.WriteLine("Sum Hours Nodal Flooding," + sumHoursNodalFlooding);
                                            myReportFile.WriteLine("Mean Failure Time," + meanNodalFlooding);
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

            btnStartSequential.Text = "Start";
            btnStartSequential.Refresh();
        }



        static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetKCombs(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
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
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("C")) ||
                                    (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("B")) ||
                                    (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("H")) ||
                                    (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("N")) ||
                                    chkAllLinks.Checked)
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


            //ISWMMToolkit[] toolkitSWMMs = new ISWMMToolkit[cores];

            List<NodeFloodingSummary> FolderSummary = new List<NodeFloodingSummary>();
            //List<NodeFloodingSummary> TopFolderSummary = new List<NodeFloodingSummary>();
            List<ResilienceMetrics> MetricsSummary = new List<ResilienceMetrics>();
            List<ResilienceMetrics> TopMetricsSummary = new List<ResilienceMetrics>();
            List<ResilienceMetrics> DownMetricsSummary = new List<ResilienceMetrics>();

            double topFailuringPercentage = (double)(nudTopFailuring.Value / 100);

            ManualResetEvent waitEndLoop = new ManualResetEvent(true);

            int rouletteWheelStartFolder = (int)nudRouletteWheelStartFailureLevel.Value;

            for (int i = rouletteWheelStartFolder; i <= linksCount; i++)
            {
                int failureLinkCount = i;

                //int topCount = failureLinkCount == 2 ? FolderSummary.Count : (int)(topFailuringPercentage * FolderSummary.Count);
                //TopFolderSummary = FolderSummary.OrderByDescending(x => x.SumTotalFloodVolume).Take(topCount).ToList();

                int topCount = (failureLinkCount == 2 || failureLinkCount == linksCount || topFailuringPercentage * (int)nudTargetNumber.Value > MetricsSummary.Count) ? MetricsSummary.Count : (int)(topFailuringPercentage * (int)nudTargetNumber.Value);

                if (MetricsSummary.Count > 0 && failureLinkCount <= linksCount)
                {
                    Matrix<double> options = DenseMatrix.OfRows(MetricsSummary.Select(x => new List<double>() { x.MeanFailureTime, x.FloodingLossVolume }));

                    ShannonEntropy shannon = new ShannonEntropy(MetricsSummary.Count, 2);

                    shannon.Matrix = options;

                    shannon.Normalize();
                    shannon.CalculateEntropy();
                    shannon.CalculateDistances();
                    shannon.CalculateWeights();

                    Topsis topsis = new Topsis(MetricsSummary.Count, 2, new bool[] { true, false });

                    topsis.Matrix = options;

                    topsis.Normalize();
                    topsis.WeighingNormalizedMatrix(shannon.W);
                    topsis.FindingIdealValues();
                    topsis.CalculateDistances();
                    topsis.CalculateSimilarity();

                    Parallel.ForEach(topsis.Similarity, (similarity, pls, index) =>
                    {
                        if ((double)similarity != double.NaN)
                        {
                            MetricsSummary[(int)index].Score = similarity;
                        }
                    });


                    TopMetricsSummary = MetricsSummary.OrderBy(x => x.Score).Take(topCount).ToList();
                    DownMetricsSummary = MetricsSummary.OrderByDescending(x => x.Score).Take(topCount).ToList();


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
                                metricsSummaryFile.Write(metricsSummary.FloodingLossVolume);
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

                            foreach (var metricsSummary in DownMetricsSummary)
                            {
                                metricsSummaryFile.Write(string.Join(",", metricsSummary.FailureLinksID));
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(string.Join(",", metricsSummary.FailureLinksIndex));
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.MeanFailureTime);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.FailureTime);
                                metricsSummaryFile.Write(" ");
                                metricsSummaryFile.Write(metricsSummary.FloodingLossVolume);
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
                        var rnd = new Random();

                        RouletteWheel.CalculateCumulativePercent(TopMetricsSummary, rouletteWheel);
                        RouletteWheel.SelectFromWheel((int)(nudTargetNumber.Value / 2), TopMetricsSummary, rouletteWheel, () => rnd.NextDouble(), linkList, ref targetCombinations);

                        rouletteWheel = new List<double>();
                        rnd = new Random();

                        RouletteWheel.CalculateCumulativePercent(DownMetricsSummary, rouletteWheel, false);
                        RouletteWheel.SelectFromWheel((int)(nudTargetNumber.Value / 2), DownMetricsSummary, rouletteWheel, () => rnd.NextDouble(), linkList, ref targetCombinations);

                        if (targetCombinations.Count < (int)nudTargetNumber.Value)
                        {
                            Random rand = new Random();

                            IEnumerable<int> range = Enumerable.Range(1, linksCount);

                            int repeatSelection = 0;

                            while (targetCombinations.Count < (int)nudTargetNumber.Value && repeatSelection < 10000)
                            {
                                repeatSelection++;

                                range = range.OrderBy(m => rand.Next());

                                List<int> failureLinksIndex = range.Take(failureLinkCount).Select(x => linkList[x - 1].Index).ToList();

                                if (!targetCombinations.Any(x => x.All(failureLinksIndex.Contains)))
                                {
                                    targetCombinations.Add(failureLinksIndex);
                                    break;
                                }
                            }
                        }
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
                    rouletteWheelStartFile = (int)nudRouletteWheelStartFailureLevel.Value;

                    if (rouletteWheelStartFile != 1)
                    {
                        for (int file_index = 1; file_index < rouletteWheelStartFile; file_index++)
                        {
                            if (File.Exists(Path.Combine(dirInfo.FullName, file_index.ToString() + ".txt")))
                            {
                                ResilienceMetrics resilienceMetrics = new ResilienceMetrics();

                                string[] file_lines = File.ReadAllLines(Path.Combine(dirInfo.FullName, file_index.ToString() + ".txt"), Encoding.UTF8);

                                resilienceMetrics.FileIndex = file_index;

                                double diameter = Convert.ToDouble(file_lines[0].Split(' ')[2]);
                                string[] failureLinks = file_lines[1].Split(' ')[2].Split(',');

                                List<int> failureLinksIndex = new List<int>();
                                List<string> failureLinksID = new List<string>();


                                for (int f = 0; f < failureLinks.Count() - 1; f++)
                                {
                                    int index = Convert.ToInt32(failureLinks[f].Split('(')[0]);
                                    string id = (failureLinks[f].Split('(')[1]).Substring(0, failureLinks[f].Split('(')[1].Length - 1);

                                    failureLinksIndex.Add(index);
                                    failureLinksID.Add(id);
                                }

                                resilienceMetrics.FailureLinksIndex = failureLinksIndex;
                                resilienceMetrics.FailureLinksID = failureLinksID;

                                NodeFloodingSummary nodeFloodingSummary = new NodeFloodingSummary()
                                {
                                    isNoFlooding = true,
                                    Diameter = diameter,
                                    FailureLinksIndex = failureLinksIndex,
                                    FailureLinksID = failureLinksID,
                                    NodeFloodsInfo = new List<NodeFlood>(),
                                };

                                for (int k = 2; k < file_lines.Count(); k++)
                                {
                                    string[] data = file_lines[k].Split(',');

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

                                            break;
                                        case "Flooding Loss (gal)":
                                            resilienceMetrics.FloodingLossVolume = Convert.ToDouble(data[1]);
                                            break;
                                        case "Sum Hours Nodal Flooding":
                                            resilienceMetrics.FailureTime = Convert.ToDouble(data[1]);
                                            break;
                                        case "Mean Hours Nodal Flooding":
                                            resilienceMetrics.MeanFailureTime = Convert.ToDouble(data[1]);
                                            break;
                                        case "Mean Failure Time":
                                            resilienceMetrics.MeanFailureTime = Convert.ToDouble(data[1]);
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                resilienceMetrics.MinNodeFloodingVolume = nodeFloodingSummary.NodeFloodsInfo.Min(x => x.TotalFloodVolume);
                                resilienceMetrics.MaxNodeFloodingVolume = nodeFloodingSummary.NodeFloodsInfo.Max(x => x.TotalFloodVolume);
                                resilienceMetrics.MinNodeFloodingTime = nodeFloodingSummary.NodeFloodsInfo.Min(x => x.HoursFlooded);
                                resilienceMetrics.MaxNodeFloodingTime = nodeFloodingSummary.NodeFloodsInfo.Max(x => x.HoursFlooded);

                                MetricsSummary.Add(resilienceMetrics);

                            }
                        }
                    }
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
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter, Path.Combine(dirInfo.FullName, failuringCounter.ToString()), ref floodElapsedTime, ref floodOutfalInflow, chkCompleteFailureTime.Checked);
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

                                        double meanNodalFloodingTime = 0;

                                        if (numNodalFlooding != 0)
                                        {
                                            meanNodalFloodingTime = sumHoursNodalFlooding / numNodalFlooding;

                                            resilienceMetrics.FailureTime = sumHoursNodalFlooding;
                                            resilienceMetrics.MeanFailureTime = meanNodalFloodingTime;
                                        }

                                        myReportFile.WriteLine("Sum Hours Nodal Flooding," + sumHoursNodalFlooding);
                                        myReportFile.WriteLine("Mean Failure Time," + meanNodalFloodingTime);

                                        myReportFile.WriteLine("Flood (or End of failuring) Elapsed Time," + floodElapsedTime);
                                        myReportFile.WriteLine("Flood (or End of failuring) OutfallInflow," + floodOutfalInflow);

                                        double refOutfallInflow = 0;
                                        double diffOutfalInflow = 0;

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
                                Matrix<double> options = DenseMatrix.OfRows(MetricsSummary.Select(x => new List<double>() { x.MeanFailureTime, x.FloodingLossVolume }));

                                ShannonEntropy shannon = new ShannonEntropy(MetricsSummary.Count, 2);

                                shannon.Matrix = options;

                                shannon.Normalize();
                                shannon.CalculateEntropy();
                                shannon.CalculateDistances();
                                shannon.CalculateWeights();

                                Topsis topsis = new Topsis(MetricsSummary.Count, 2, new bool[] { true, false });

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
                                            metricsSummaryFile.Write(metricsSummary.FloodingLossVolume);
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

                                if (FileCountInFolder == failuringNumber - rouletteWheelStartFile + 1)
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


        private void nudCoreNumber_ValueChanged(object sender, EventArgs e)
        {
            for (int k = 1; k <= nudCoreNumber.Value; k++)
            {
                File.Copy("swmm5.dll", "swmm5_" + k + ".dll", true);
            }

            for (int k = 1; k <= nudCoreNumber.Value; k++)
            {
                File.Copy("EPA SWMM 5.1.013\\swmm5.exe", "EPA SWMM 5.1.013\\swmm5_" + k + ".exe", true);
            }
        }

        private void btnStartRandom_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtInputFileName.Text;
            string outputFolderPath = txtOutputFolder.Text;

            if (outputFolderPath.Contains(" "))
            {
                MessageBox.Show("The output file address should not contain space character.");
                return;
            }

            btnStartRandom.Text = "Running";
            btnStartRandom.Refresh();
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
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("N")) ||
                                chkAllLinks.Checked)
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

            


            var cores = (int)nudCoreNumber.Value;
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

                waitEndLoop.Reset();

                int FileCountInFolder = 0;


                var dirInfo = Directory.CreateDirectory(Path.Combine(outputFolderPath, failureLinkCount.ToString()));

                int failuringNumber = (int)nudTargetNumber.Value;

                if (failureLinkCount == 1)
                    failuringNumber = linksCount;
               
                else if (failureLinkCount == linksCount - 1)
                    failuringNumber = linksCount;
                else if (failureLinkCount == linksCount)
                    failuringNumber = 1;
                else
                    failuringNumber = (int)nudTargetNumber.Value;

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

                                    double floodElapsedTime = 0;
                                    double floodOutfalInflow = 0;

                                    // --- open the files & read input data
                                    ErrorCode = toolkitSWMM.Open(inputFilePath, Path.Combine(dirInfo.FullName, j.ToString() + ".rpt"), "");

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter, Path.Combine(dirInfo.FullName, failuringCounter.ToString()), ref floodElapsedTime, ref floodOutfalInflow, chkCompleteFailureTime.Checked);


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

                                        double meanNodalFloodingTime = 0;

                                        if (numNodalFlooding != 0)
                                        {
                                            meanNodalFloodingTime = sumHoursNodalFlooding / numNodalFlooding;
                                        }

                                        myReportFile.WriteLine("Sum Hours Nodal Flooding," + sumHoursNodalFlooding);
                                        myReportFile.WriteLine("Mean Failure Time," + meanNodalFloodingTime);

                                        myReportFile.WriteLine("Flood (or End of failuring) Elapsed Time," + floodElapsedTime);
                                        myReportFile.WriteLine("Flood (or End of failuring) OutfallInflow," + floodOutfalInflow);

                                        
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

            btnStartRandom.Text = "Start";
            btnStartRandom.Refresh();
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
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("N")) ||
                                chkAllLinks.Checked)
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

            int positionOfGeom1 = 0;
            int positionOfRoughness = 0;

            if (nudFailuringRoughness.Value == 0)
            {
                foreach (var line in lines)
                {
                    if (line.Contains(";;Link"))
                    {
                        break;
                    }
                    lineNumberOfLinks++;
                }

                positionOfGeom1 = lines[lineNumberOfLinks].IndexOf("Geom1");
            }
            else
            {
                foreach (var line in lines)
                {
                    if (line.Contains("[CONDUITS]"))
                    {
                        break;
                    }
                    lineNumberOfLinks++;
                }

                lineNumberOfLinks++;

                positionOfRoughness = lines[lineNumberOfLinks].IndexOf("Roughness");
            }

            lineNumberOfLinks++; 


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

                            if (nudFailuringRoughness.Value == 0)
                            {
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
                            }
                            else
                            {
                                foreach (var failureLink in targetCombination)
                                {
                                    List<string> linkInfoSplite = failureLinksInputFile[lineNumberOfLinks + failureLink].Split(' ').ToList();
                                    //linkInfoSplite = linkInfoSplite.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                                    int roughnessIndex = 5;
                                    int counterIndex = 0;
                                    int k = 0;

                                    for (k = 0; k < linkInfoSplite.Count(); k++)
                                    {
                                        if (linkInfoSplite[k] != "")
                                        {
                                            counterIndex++;

                                            if (counterIndex == roughnessIndex)
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    int oldValueLength = linkInfoSplite[k].Length;
                                    int newValueLength = nudFailuringRoughness.Value.ToString().Length;

                                    if (newValueLength > oldValueLength)
                                    {
                                        for (int l = 0; l < newValueLength - oldValueLength; l++)
                                        {
                                            linkInfoSplite.RemoveAt(k + 1);
                                        }

                                        linkInfoSplite[k] = nudFailuringRoughness.Value.ToString();
                                    }
                                    else if (newValueLength < oldValueLength)
                                    {
                                        linkInfoSplite[k] = nudFailuringRoughness.Value.ToString();

                                        for (int l = 0; l < oldValueLength - newValueLength; l++)
                                        {
                                            linkInfoSplite[k] += " ";
                                        }
                                    }
                                    else
                                    {
                                        linkInfoSplite[k] = nudFailuringRoughness.Value.ToString();
                                    }


                                    failureLinksInputFile[lineNumberOfLinks + failureLink] = String.Join(" ", linkInfoSplite);
                                }
                            }



                            File.WriteAllLines(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".inp"), failureLinksInputFile);


                            using (StreamWriter myReportFile = new StreamWriter(Path.Combine(dirInfo.FullName, failuringCounter.ToString() + ".txt")))
                            {
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

                    readLink = true;

                    for (int li = 0; li < lines.Count(); li++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[li + line + 2]))
                        {
                            break;
                        }
                        else
                        {
                            if ((chkMainLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("C")) ||
                                (chkBranchLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("B")) ||
                                (chkHighInflowLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("H")) ||
                                (chkIMPLinks.Checked && lines[li + line + 2].Split(' ')[0].EndsWith("N")) ||
                                chkAllLinks.Checked)
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

            int startFolder = (int)nudRouletteWheelStartFailureLevel.Value;

            for (int i = startFolder; i <= startFolder/*linksCount*/; i++)
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

                int startFile = 1;

                if (failureLinkCount == startFolder)
                {
                    startFile = (int)nudRouletteWheelStartFailureScenario.Value;
                }


                int FileCountInFolder = startFile - 1;

                for (int j_index = startFile; j_index <= failuringNumber; j_index++)
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

                                    // --- run the simulation if input data OK
                                    if (ErrorCode == 0)
                                    {
                                        StepRunning(toolkitSWMM, (int)nudFailureStartTime.Value, (int)nudFailureEndTime.Value, failureLinkIndexes, Diameter, Path.Combine(dirInfo.FullName, failuringCounter.ToString()), ref floodElapsedTime, ref floodOutfalInflow, chkCompleteFailureTime.Checked);
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

                                        if (floodElapsedTime != 0)
                                        {
                                            myReportFile.WriteLine("Flood (or End of failuring) Elapsed Time," + floodElapsedTime);
                                            myReportFile.WriteLine("Flood (or End of failuring) OutfallInflow," + floodOutfalInflow);

                                           
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
                                    using (StreamWriter error = new StreamWriter("log.txt"))
                                    {
                                        error.WriteLine(ex.Message);
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

            DateTime end = DateTime.Now;

            double totalTime = (end - start).TotalSeconds;
            using (StreamWriter runTime = new StreamWriter(Path.Combine(outputFolderPath, "runTime.txt")))
            {
                runTime.WriteLine(totalTime);
            }

            btnStartCombinationOSWMMDLL.Text = "Start";
            btnStartCombinationOSWMMDLL.Refresh();
        }


        private void pictureBox2_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picLinksInfo, "In this section, links that can participate in the failure scenarios generation are identified.\n" +
                                                "You can distinguish between links by ending their \"Name\" property with these letters: C, B, H, N and M.");
        }

        private void picFerdowsiUniversityLogo_Click(object sender, EventArgs e)
        {
            Process.Start("http://en.um.ac.ir/");
        }

        private void picEpaLogo_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.epa.gov/");
        }

        private void picDiameterInfo_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picDiameterInfo, "Specify the desired pipe diameter to simulate blockage. \n" +
                                              "if \"Failure Roughness\" was applied to zero, then the simulation run with desired pipe diameter.");
        }

        private void picRoughnessInfo_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picRoughnessInfo, "Specify the desired pipe roughness to simulate obstruction.");
        }

        private void picFailureStartInfor_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picFailureStartInfor, "Specify the time that you want to start the desired pipe blockage.");
        }

        private void picFailureEndInfo_MouseHover(object sender, EventArgs e)
        {

            toolTip1.SetToolTip(picFailureEndInfo, "Specify the time that you want to end the desired pipe blockage.");
        }

        private void picCoresInfo_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picCoresInfo, "Specify num of cores that is used in simulation");
        }

        private void picNumSR_Scenarios_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picNumSR_Scenarios, "Specify failure level (Number of failed links) in Roulette wheel");
        }

        private void picNumRW_Scenarios_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picNumRW_Scenarios, "Specify perecentage of selected scenarios that got Top Score");
        }

        private void picTopFailuringInfo_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picTopFailuringInfo, "Specify percentage of scenarios that participate to the next step ");
        }

        private void picRW_StartFailureLevelInfo_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picRW_StartFailureLevelInfo, "Specify start failure level for simulation start");
        }

        private void picRW_FailureScenarioInfo_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picRW_FailureScenarioInfo, "Specify start failure scenario for simulation start");
        }

        private void picInputFile_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picInputFile, "Specify input file directory with .inp format");
        }

        private void picOutputDirectory_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(picOutputDirectory, "Specify output file directory");
        }
    }
}






