using System.Drawing;
using System.Windows.Forms;

namespace SWMM_GRA_Toolbox
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabGRAnalysis = new System.Windows.Forms.TabPage();
            this.lblSimulationTime = new System.Windows.Forms.Label();
            this.nudTotalFailureTime = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            this.lblAnalysis = new System.Windows.Forms.Label();
            this.btnResultsDirectory = new System.Windows.Forms.Button();
            this.txtResultsDirectory = new System.Windows.Forms.TextBox();
            this.lblResultsDirectory = new System.Windows.Forms.Label();
            this.chtResilience = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabGRASimulation = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.Panel();
            this.picFerdowsiUniversityLogo = new System.Windows.Forms.PictureBox();
            this.picEpaLogo = new System.Windows.Forms.PictureBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblFailureLevelCounter = new System.Windows.Forms.Label();
            this.lblTotalTime = new System.Windows.Forms.Label();
            this.lblScenarioCounter = new System.Windows.Forms.Label();
            this.lbliInputOutput = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.nudAVGDWFCoef = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.chkNoReportFiles = new System.Windows.Forms.CheckBox();
            this.picOutputDirectory = new System.Windows.Forms.PictureBox();
            this.picInputFile = new System.Windows.Forms.PictureBox();
            this.btnSelectFileName = new System.Windows.Forms.Button();
            this.btnOutputFolder = new System.Windows.Forms.Button();
            this.lblOutputFolder = new System.Windows.Forms.Label();
            this.lblInputFileName = new System.Windows.Forms.Label();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.txtInputFileName = new System.Windows.Forms.TextBox();
            this.lblSimulationProperties = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rtbTopFlooding = new System.Windows.Forms.RichTextBox();
            this.picRW_FailureScenarioInfo = new System.Windows.Forms.PictureBox();
            this.picRW_StartFailureLevelInfo = new System.Windows.Forms.PictureBox();
            this.picNumRW_Scenarios = new System.Windows.Forms.PictureBox();
            this.picTopFailuringInfo = new System.Windows.Forms.PictureBox();
            this.picNumSR_Scenarios = new System.Windows.Forms.PictureBox();
            this.picCoresInfo = new System.Windows.Forms.PictureBox();
            this.picFailureEndInfo = new System.Windows.Forms.PictureBox();
            this.picFailureStartInfor = new System.Windows.Forms.PictureBox();
            this.picRoughnessInfo = new System.Windows.Forms.PictureBox();
            this.picDiameterInfo = new System.Windows.Forms.PictureBox();
            this.lblFailureDiameter = new System.Windows.Forms.Label();
            this.lblStartFailureTime = new System.Windows.Forms.Label();
            this.numFailureDiameter = new System.Windows.Forms.NumericUpDown();
            this.nudFailureStartTime = new System.Windows.Forms.NumericUpDown();
            this.lblEndFailureTime = new System.Windows.Forms.Label();
            this.nudFailureEndTime = new System.Windows.Forms.NumericUpDown();
            this.nudTopFailuring = new System.Windows.Forms.NumericUpDown();
            this.lblNumOfCores = new System.Windows.Forms.Label();
            this.lblMaxCores = new System.Windows.Forms.Label();
            this.nudCoreNumber = new System.Windows.Forms.NumericUpDown();
            this.lblFailuringRoughness = new System.Windows.Forms.Label();
            this.nudFailuringRoughness = new System.Windows.Forms.NumericUpDown();
            this.nudSequentialFailuring = new System.Windows.Forms.NumericUpDown();
            this.nudRouletteWheelStartFailureScenario = new System.Windows.Forms.NumericUpDown();
            this.lblFailuringSequential = new System.Windows.Forms.Label();
            this.lblRouletteWheelStartFile = new System.Windows.Forms.Label();
            this.nudRouletteWheelStartFailureLevel = new System.Windows.Forms.NumericUpDown();
            this.lblRouletteWheelStartFolder = new System.Windows.Forms.Label();
            this.lblNumOfTarget = new System.Windows.Forms.Label();
            this.nudTargetNumber = new System.Windows.Forms.NumericUpDown();
            this.picLinksInfo = new System.Windows.Forms.PictureBox();
            this.lblLinks = new System.Windows.Forms.Label();
            this.flpLinks = new System.Windows.Forms.FlowLayoutPanel();
            this.chkAllLinks = new System.Windows.Forms.CheckBox();
            this.chkMainLinks = new System.Windows.Forms.CheckBox();
            this.chkBranchLinks = new System.Windows.Forms.CheckBox();
            this.chkHighInflowLinks = new System.Windows.Forms.CheckBox();
            this.chkIMPLinks = new System.Windows.Forms.CheckBox();
            this.btnStartCombinationOSWMMDLL = new System.Windows.Forms.Button();
            this.btnStartCombinationEXE = new System.Windows.Forms.Button();
            this.btnStartRandom = new System.Windows.Forms.Button();
            this.btnStartRouletteWheel = new System.Windows.Forms.Button();
            this.btnStartSequential = new System.Windows.Forms.Button();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.nudTotalInflow = new System.Windows.Forms.NumericUpDown();
            this.chkCompleteFailureTime = new System.Windows.Forms.CheckBox();
            this.tabGRAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalFailureTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chtResilience)).BeginInit();
            this.tabGRASimulation.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picFerdowsiUniversityLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picEpaLogo)).BeginInit();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAVGDWFCoef)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picOutputDirectory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picInputFile)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picRW_FailureScenarioInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRW_StartFailureLevelInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNumRW_Scenarios)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTopFailuringInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNumSR_Scenarios)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCoresInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFailureEndInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFailureStartInfor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRoughnessInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDiameterInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFailureDiameter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFailureStartTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFailureEndTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTopFailuring)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoreNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFailuringRoughness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSequentialFailuring)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRouletteWheelStartFailureScenario)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRouletteWheelStartFailureLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTargetNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLinksInfo)).BeginInit();
            this.flpLinks.SuspendLayout();
            this.tabMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalInflow)).BeginInit();
            this.SuspendLayout();
            // 
            // tabGRAnalysis
            // 
            this.tabGRAnalysis.Controls.Add(this.nudTotalInflow);
            this.tabGRAnalysis.Controls.Add(this.label2);
            this.tabGRAnalysis.Controls.Add(this.lblSimulationTime);
            this.tabGRAnalysis.Controls.Add(this.nudTotalFailureTime);
            this.tabGRAnalysis.Controls.Add(this.button1);
            this.tabGRAnalysis.Controls.Add(this.lblAnalysis);
            this.tabGRAnalysis.Controls.Add(this.btnResultsDirectory);
            this.tabGRAnalysis.Controls.Add(this.txtResultsDirectory);
            this.tabGRAnalysis.Controls.Add(this.lblResultsDirectory);
            this.tabGRAnalysis.Controls.Add(this.chtResilience);
            this.tabGRAnalysis.Location = new System.Drawing.Point(4, 26);
            this.tabGRAnalysis.Margin = new System.Windows.Forms.Padding(2);
            this.tabGRAnalysis.Name = "tabGRAnalysis";
            this.tabGRAnalysis.Padding = new System.Windows.Forms.Padding(2);
            this.tabGRAnalysis.Size = new System.Drawing.Size(819, 479);
            this.tabGRAnalysis.TabIndex = 1;
            this.tabGRAnalysis.Text = "GR Analysis";
            this.tabGRAnalysis.UseVisualStyleBackColor = true;
            // 
            // lblSimulationTime
            // 
            this.lblSimulationTime.AutoSize = true;
            this.lblSimulationTime.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSimulationTime.Location = new System.Drawing.Point(629, 82);
            this.lblSimulationTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSimulationTime.Name = "lblSimulationTime";
            this.lblSimulationTime.Size = new System.Drawing.Size(117, 17);
            this.lblSimulationTime.TabIndex = 22;
            this.lblSimulationTime.Text = "Total Failure Time:";
            // 
            // nudTotalFailureTime
            // 
            this.nudTotalFailureTime.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudTotalFailureTime.Location = new System.Drawing.Point(746, 80);
            this.nudTotalFailureTime.Margin = new System.Windows.Forms.Padding(2);
            this.nudTotalFailureTime.Name = "nudTotalFailureTime";
            this.nudTotalFailureTime.Size = new System.Drawing.Size(58, 25);
            this.nudTotalFailureTime.TabIndex = 21;
            this.nudTotalFailureTime.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.button1.Location = new System.Drawing.Point(659, 241);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(136, 35);
            this.button1.TabIndex = 13;
            this.button1.Text = "Analysis";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnAnalysis_Click);
            // 
            // lblAnalysis
            // 
            this.lblAnalysis.AutoSize = true;
            this.lblAnalysis.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysis.Location = new System.Drawing.Point(643, 285);
            this.lblAnalysis.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAnalysis.Name = "lblAnalysis";
            this.lblAnalysis.Size = new System.Drawing.Size(166, 17);
            this.lblAnalysis.TabIndex = 12;
            this.lblAnalysis.Text = "# Scenario:     , #Failuring: ";
            // 
            // btnResultsDirectory
            // 
            this.btnResultsDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnResultsDirectory.Location = new System.Drawing.Point(659, 30);
            this.btnResultsDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.btnResultsDirectory.Name = "btnResultsDirectory";
            this.btnResultsDirectory.Size = new System.Drawing.Size(136, 22);
            this.btnResultsDirectory.TabIndex = 3;
            this.btnResultsDirectory.Text = "Select Directory ...";
            this.btnResultsDirectory.UseVisualStyleBackColor = true;
            this.btnResultsDirectory.Click += new System.EventHandler(this.btnResultsDirectory_Click);
            // 
            // txtResultsDirectory
            // 
            this.txtResultsDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.txtResultsDirectory.Location = new System.Drawing.Point(137, 30);
            this.txtResultsDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.txtResultsDirectory.Name = "txtResultsDirectory";
            this.txtResultsDirectory.Size = new System.Drawing.Size(518, 22);
            this.txtResultsDirectory.TabIndex = 2;
            // 
            // lblResultsDirectory
            // 
            this.lblResultsDirectory.AutoSize = true;
            this.lblResultsDirectory.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResultsDirectory.Location = new System.Drawing.Point(19, 33);
            this.lblResultsDirectory.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultsDirectory.Name = "lblResultsDirectory";
            this.lblResultsDirectory.Size = new System.Drawing.Size(114, 17);
            this.lblResultsDirectory.TabIndex = 1;
            this.lblResultsDirectory.Text = "Results Directory:";
            // 
            // chtResilience
            // 
            this.chtResilience.BorderlineColor = System.Drawing.Color.DarkRed;
            this.chtResilience.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            this.chtResilience.BorderlineWidth = 3;
            chartArea2.Name = "ChartArea1";
            this.chtResilience.ChartAreas.Add(chartArea2);
            this.chtResilience.Location = new System.Drawing.Point(21, 65);
            this.chtResilience.Margin = new System.Windows.Forms.Padding(2);
            this.chtResilience.Name = "chtResilience";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series4.IsVisibleInLegend = false;
            series4.Legend = "Legend1";
            series4.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series4.Name = "Series1";
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series5.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series5.Name = "Series2";
            series6.ChartArea = "ChartArea1";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series6.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series6.Name = "Series3";
            this.chtResilience.Series.Add(series4);
            this.chtResilience.Series.Add(series5);
            this.chtResilience.Series.Add(series6);
            this.chtResilience.Size = new System.Drawing.Size(604, 407);
            this.chtResilience.TabIndex = 44;
            this.chtResilience.Text = "chart5";
            title2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title2.Name = "Title1";
            title2.Text = "Resilience";
            this.chtResilience.Titles.Add(title2);
            // 
            // tabGRASimulation
            // 
            this.tabGRASimulation.Controls.Add(this.panel4);
            this.tabGRASimulation.Controls.Add(this.panel3);
            this.tabGRASimulation.Controls.Add(this.lbliInputOutput);
            this.tabGRASimulation.Controls.Add(this.panel2);
            this.tabGRASimulation.Controls.Add(this.lblSimulationProperties);
            this.tabGRASimulation.Controls.Add(this.panel1);
            this.tabGRASimulation.Controls.Add(this.picLinksInfo);
            this.tabGRASimulation.Controls.Add(this.lblLinks);
            this.tabGRASimulation.Controls.Add(this.flpLinks);
            this.tabGRASimulation.Controls.Add(this.btnStartCombinationOSWMMDLL);
            this.tabGRASimulation.Controls.Add(this.btnStartCombinationEXE);
            this.tabGRASimulation.Controls.Add(this.btnStartRandom);
            this.tabGRASimulation.Controls.Add(this.btnStartRouletteWheel);
            this.tabGRASimulation.Controls.Add(this.btnStartSequential);
            this.tabGRASimulation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.tabGRASimulation.Location = new System.Drawing.Point(4, 26);
            this.tabGRASimulation.Margin = new System.Windows.Forms.Padding(2);
            this.tabGRASimulation.Name = "tabGRASimulation";
            this.tabGRASimulation.Padding = new System.Windows.Forms.Padding(2);
            this.tabGRASimulation.Size = new System.Drawing.Size(819, 479);
            this.tabGRASimulation.TabIndex = 0;
            this.tabGRASimulation.Text = "GRA Simulation";
            this.tabGRASimulation.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(180)))), ((int)(((byte)(239)))));
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.picFerdowsiUniversityLogo);
            this.panel4.Controls.Add(this.picEpaLogo);
            this.panel4.Location = new System.Drawing.Point(8, 379);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(475, 90);
            this.panel4.TabIndex = 78;
            // 
            // picFerdowsiUniversityLogo
            // 
            this.picFerdowsiUniversityLogo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picFerdowsiUniversityLogo.BackgroundImage")));
            this.picFerdowsiUniversityLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picFerdowsiUniversityLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picFerdowsiUniversityLogo.Location = new System.Drawing.Point(3, 3);
            this.picFerdowsiUniversityLogo.Name = "picFerdowsiUniversityLogo";
            this.picFerdowsiUniversityLogo.Size = new System.Drawing.Size(297, 82);
            this.picFerdowsiUniversityLogo.TabIndex = 2;
            this.picFerdowsiUniversityLogo.TabStop = false;
            this.picFerdowsiUniversityLogo.Click += new System.EventHandler(this.picFerdowsiUniversityLogo_Click);
            // 
            // picEpaLogo
            // 
            this.picEpaLogo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picEpaLogo.BackgroundImage")));
            this.picEpaLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picEpaLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picEpaLogo.Location = new System.Drawing.Point(388, 3);
            this.picEpaLogo.Name = "picEpaLogo";
            this.picEpaLogo.Size = new System.Drawing.Size(82, 82);
            this.picEpaLogo.TabIndex = 0;
            this.picEpaLogo.TabStop = false;
            this.picEpaLogo.Click += new System.EventHandler(this.picEpaLogo_Click);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(180)))), ((int)(((byte)(239)))));
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.lblFailureLevelCounter);
            this.panel3.Controls.Add(this.lblTotalTime);
            this.panel3.Controls.Add(this.lblScenarioCounter);
            this.panel3.Location = new System.Drawing.Point(495, 379);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(313, 90);
            this.panel3.TabIndex = 77;
            // 
            // lblFailureLevelCounter
            // 
            this.lblFailureLevelCounter.AutoSize = true;
            this.lblFailureLevelCounter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFailureLevelCounter.ForeColor = System.Drawing.Color.White;
            this.lblFailureLevelCounter.Location = new System.Drawing.Point(12, 11);
            this.lblFailureLevelCounter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFailureLevelCounter.Name = "lblFailureLevelCounter";
            this.lblFailureLevelCounter.Size = new System.Drawing.Size(114, 16);
            this.lblFailureLevelCounter.TabIndex = 11;
            this.lblFailureLevelCounter.Text = "Failure Level: 0";
            // 
            // lblTotalTime
            // 
            this.lblTotalTime.AutoSize = true;
            this.lblTotalTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalTime.ForeColor = System.Drawing.Color.White;
            this.lblTotalTime.Location = new System.Drawing.Point(12, 61);
            this.lblTotalTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTotalTime.Name = "lblTotalTime";
            this.lblTotalTime.Size = new System.Drawing.Size(147, 16);
            this.lblTotalTime.TabIndex = 76;
            this.lblTotalTime.Text = "Total Time: 00:00:00";
            // 
            // lblScenarioCounter
            // 
            this.lblScenarioCounter.AutoSize = true;
            this.lblScenarioCounter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScenarioCounter.ForeColor = System.Drawing.Color.White;
            this.lblScenarioCounter.Location = new System.Drawing.Point(12, 36);
            this.lblScenarioCounter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblScenarioCounter.Name = "lblScenarioCounter";
            this.lblScenarioCounter.Size = new System.Drawing.Size(167, 16);
            this.lblScenarioCounter.TabIndex = 75;
            this.lblScenarioCounter.Text = "Simulated Scenarios: 0";
            // 
            // lbliInputOutput
            // 
            this.lbliInputOutput.AutoSize = true;
            this.lbliInputOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbliInputOutput.Location = new System.Drawing.Point(8, 7);
            this.lbliInputOutput.Name = "lbliInputOutput";
            this.lbliInputOutput.Size = new System.Drawing.Size(42, 16);
            this.lbliInputOutput.TabIndex = 74;
            this.lbliInputOutput.Text = "Files";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LemonChiffon;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.nudAVGDWFCoef);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.chkNoReportFiles);
            this.panel2.Controls.Add(this.picOutputDirectory);
            this.panel2.Controls.Add(this.picInputFile);
            this.panel2.Controls.Add(this.btnSelectFileName);
            this.panel2.Controls.Add(this.btnOutputFolder);
            this.panel2.Controls.Add(this.lblOutputFolder);
            this.panel2.Controls.Add(this.lblInputFileName);
            this.panel2.Controls.Add(this.txtOutputFolder);
            this.panel2.Controls.Add(this.txtInputFileName);
            this.panel2.Location = new System.Drawing.Point(8, 26);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(800, 97);
            this.panel2.TabIndex = 1;
            // 
            // nudAVGDWFCoef
            // 
            this.nudAVGDWFCoef.BackColor = System.Drawing.Color.White;
            this.nudAVGDWFCoef.DecimalPlaces = 2;
            this.nudAVGDWFCoef.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudAVGDWFCoef.Location = new System.Drawing.Point(372, 70);
            this.nudAVGDWFCoef.Margin = new System.Windows.Forms.Padding(2);
            this.nudAVGDWFCoef.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudAVGDWFCoef.Name = "nudAVGDWFCoef";
            this.nudAVGDWFCoef.Size = new System.Drawing.Size(77, 22);
            this.nudAVGDWFCoef.TabIndex = 66;
            this.nudAVGDWFCoef.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.label1.Location = new System.Drawing.Point(222, 72);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(146, 16);
            this.label1.TabIndex = 65;
            this.label1.Text = "Inflow (DWF) coefficient";
            // 
            // chkNoReportFiles
            // 
            this.chkNoReportFiles.AutoSize = true;
            this.chkNoReportFiles.Checked = true;
            this.chkNoReportFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNoReportFiles.Location = new System.Drawing.Point(11, 68);
            this.chkNoReportFiles.Name = "chkNoReportFiles";
            this.chkNoReportFiles.Size = new System.Drawing.Size(139, 20);
            this.chkNoReportFiles.TabIndex = 64;
            this.chkNoReportFiles.Text = "No report files (.rpt)";
            this.chkNoReportFiles.UseVisualStyleBackColor = true;
            // 
            // picOutputDirectory
            // 
            this.picOutputDirectory.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picOutputDirectory.BackgroundImage")));
            this.picOutputDirectory.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picOutputDirectory.Location = new System.Drawing.Point(600, 40);
            this.picOutputDirectory.Name = "picOutputDirectory";
            this.picOutputDirectory.Size = new System.Drawing.Size(18, 18);
            this.picOutputDirectory.TabIndex = 63;
            this.picOutputDirectory.TabStop = false;
            this.picOutputDirectory.MouseHover += new System.EventHandler(this.picOutputDirectory_MouseHover);
            // 
            // picInputFile
            // 
            this.picInputFile.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picInputFile.BackgroundImage")));
            this.picInputFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picInputFile.Location = new System.Drawing.Point(600, 11);
            this.picInputFile.Name = "picInputFile";
            this.picInputFile.Size = new System.Drawing.Size(18, 18);
            this.picInputFile.TabIndex = 62;
            this.picInputFile.TabStop = false;
            this.picInputFile.MouseHover += new System.EventHandler(this.picInputFile_MouseHover);
            // 
            // btnSelectFileName
            // 
            this.btnSelectFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnSelectFileName.Location = new System.Drawing.Point(636, 8);
            this.btnSelectFileName.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelectFileName.Name = "btnSelectFileName";
            this.btnSelectFileName.Size = new System.Drawing.Size(153, 24);
            this.btnSelectFileName.TabIndex = 2;
            this.btnSelectFileName.Text = "Select Input File ...";
            this.btnSelectFileName.UseVisualStyleBackColor = true;
            this.btnSelectFileName.Click += new System.EventHandler(this.btnSelectFileName_Click);
            // 
            // btnOutputFolder
            // 
            this.btnOutputFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnOutputFolder.Location = new System.Drawing.Point(636, 37);
            this.btnOutputFolder.Margin = new System.Windows.Forms.Padding(2);
            this.btnOutputFolder.Name = "btnOutputFolder";
            this.btnOutputFolder.Size = new System.Drawing.Size(153, 24);
            this.btnOutputFolder.TabIndex = 4;
            this.btnOutputFolder.Text = "Select Output Directory";
            this.btnOutputFolder.UseVisualStyleBackColor = true;
            this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.lblOutputFolder.Location = new System.Drawing.Point(8, 41);
            this.lblOutputFolder.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(91, 16);
            this.lblOutputFolder.TabIndex = 7;
            this.lblOutputFolder.Text = "Output Folder:";
            // 
            // lblInputFileName
            // 
            this.lblInputFileName.AutoSize = true;
            this.lblInputFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.lblInputFileName.Location = new System.Drawing.Point(8, 12);
            this.lblInputFileName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInputFileName.Name = "lblInputFileName";
            this.lblInputFileName.Size = new System.Drawing.Size(64, 16);
            this.lblInputFileName.TabIndex = 0;
            this.lblInputFileName.Text = "Input File:";
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.txtOutputFolder.Location = new System.Drawing.Point(146, 38);
            this.txtOutputFolder.Margin = new System.Windows.Forms.Padding(2);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(449, 22);
            this.txtOutputFolder.TabIndex = 3;
            // 
            // txtInputFileName
            // 
            this.txtInputFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.txtInputFileName.Location = new System.Drawing.Point(146, 9);
            this.txtInputFileName.Margin = new System.Windows.Forms.Padding(2);
            this.txtInputFileName.Name = "txtInputFileName";
            this.txtInputFileName.Size = new System.Drawing.Size(449, 22);
            this.txtInputFileName.TabIndex = 1;
            // 
            // lblSimulationProperties
            // 
            this.lblSimulationProperties.AutoSize = true;
            this.lblSimulationProperties.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSimulationProperties.Location = new System.Drawing.Point(8, 131);
            this.lblSimulationProperties.Name = "lblSimulationProperties";
            this.lblSimulationProperties.Size = new System.Drawing.Size(80, 16);
            this.lblSimulationProperties.TabIndex = 63;
            this.lblSimulationProperties.Text = "Properties";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LemonChiffon;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.chkCompleteFailureTime);
            this.panel1.Controls.Add(this.rtbTopFlooding);
            this.panel1.Controls.Add(this.picRW_FailureScenarioInfo);
            this.panel1.Controls.Add(this.picRW_StartFailureLevelInfo);
            this.panel1.Controls.Add(this.picNumRW_Scenarios);
            this.panel1.Controls.Add(this.picTopFailuringInfo);
            this.panel1.Controls.Add(this.picNumSR_Scenarios);
            this.panel1.Controls.Add(this.picCoresInfo);
            this.panel1.Controls.Add(this.picFailureEndInfo);
            this.panel1.Controls.Add(this.picFailureStartInfor);
            this.panel1.Controls.Add(this.picRoughnessInfo);
            this.panel1.Controls.Add(this.picDiameterInfo);
            this.panel1.Controls.Add(this.lblFailureDiameter);
            this.panel1.Controls.Add(this.lblStartFailureTime);
            this.panel1.Controls.Add(this.numFailureDiameter);
            this.panel1.Controls.Add(this.nudFailureStartTime);
            this.panel1.Controls.Add(this.lblEndFailureTime);
            this.panel1.Controls.Add(this.nudFailureEndTime);
            this.panel1.Controls.Add(this.nudTopFailuring);
            this.panel1.Controls.Add(this.lblNumOfCores);
            this.panel1.Controls.Add(this.lblMaxCores);
            this.panel1.Controls.Add(this.nudCoreNumber);
            this.panel1.Controls.Add(this.lblFailuringRoughness);
            this.panel1.Controls.Add(this.nudFailuringRoughness);
            this.panel1.Controls.Add(this.nudSequentialFailuring);
            this.panel1.Controls.Add(this.nudRouletteWheelStartFailureScenario);
            this.panel1.Controls.Add(this.lblFailuringSequential);
            this.panel1.Controls.Add(this.lblRouletteWheelStartFile);
            this.panel1.Controls.Add(this.nudRouletteWheelStartFailureLevel);
            this.panel1.Controls.Add(this.lblRouletteWheelStartFolder);
            this.panel1.Controls.Add(this.lblNumOfTarget);
            this.panel1.Controls.Add(this.nudTargetNumber);
            this.panel1.Location = new System.Drawing.Point(8, 150);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(636, 132);
            this.panel1.TabIndex = 5;
            // 
            // rtbTopFlooding
            // 
            this.rtbTopFlooding.BackColor = System.Drawing.Color.LemonChiffon;
            this.rtbTopFlooding.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbTopFlooding.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.rtbTopFlooding.Location = new System.Drawing.Point(388, 54);
            this.rtbTopFlooding.Name = "rtbTopFlooding";
            this.rtbTopFlooding.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbTopFlooding.Size = new System.Drawing.Size(120, 23);
            this.rtbTopFlooding.TabIndex = 73;
            this.rtbTopFlooding.Text = "k TF (0<value<1)";
            // 
            // picRW_FailureScenarioInfo
            // 
            this.picRW_FailureScenarioInfo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picRW_FailureScenarioInfo.BackgroundImage")));
            this.picRW_FailureScenarioInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picRW_FailureScenarioInfo.Location = new System.Drawing.Point(600, 103);
            this.picRW_FailureScenarioInfo.Name = "picRW_FailureScenarioInfo";
            this.picRW_FailureScenarioInfo.Size = new System.Drawing.Size(18, 18);
            this.picRW_FailureScenarioInfo.TabIndex = 72;
            this.picRW_FailureScenarioInfo.TabStop = false;
            this.picRW_FailureScenarioInfo.MouseHover += new System.EventHandler(this.picRW_FailureScenarioInfo_MouseHover);
            // 
            // picRW_StartFailureLevelInfo
            // 
            this.picRW_StartFailureLevelInfo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picRW_StartFailureLevelInfo.BackgroundImage")));
            this.picRW_StartFailureLevelInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picRW_StartFailureLevelInfo.Location = new System.Drawing.Point(600, 79);
            this.picRW_StartFailureLevelInfo.Name = "picRW_StartFailureLevelInfo";
            this.picRW_StartFailureLevelInfo.Size = new System.Drawing.Size(18, 18);
            this.picRW_StartFailureLevelInfo.TabIndex = 71;
            this.picRW_StartFailureLevelInfo.TabStop = false;
            this.picRW_StartFailureLevelInfo.MouseHover += new System.EventHandler(this.picRW_StartFailureLevelInfo_MouseHover);
            // 
            // picNumRW_Scenarios
            // 
            this.picNumRW_Scenarios.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picNumRW_Scenarios.BackgroundImage")));
            this.picNumRW_Scenarios.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picNumRW_Scenarios.Location = new System.Drawing.Point(600, 31);
            this.picNumRW_Scenarios.Name = "picNumRW_Scenarios";
            this.picNumRW_Scenarios.Size = new System.Drawing.Size(18, 18);
            this.picNumRW_Scenarios.TabIndex = 70;
            this.picNumRW_Scenarios.TabStop = false;
            this.picNumRW_Scenarios.MouseHover += new System.EventHandler(this.picNumRW_Scenarios_MouseHover);
            // 
            // picTopFailuringInfo
            // 
            this.picTopFailuringInfo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picTopFailuringInfo.BackgroundImage")));
            this.picTopFailuringInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picTopFailuringInfo.Location = new System.Drawing.Point(600, 55);
            this.picTopFailuringInfo.Name = "picTopFailuringInfo";
            this.picTopFailuringInfo.Size = new System.Drawing.Size(18, 18);
            this.picTopFailuringInfo.TabIndex = 69;
            this.picTopFailuringInfo.TabStop = false;
            this.picTopFailuringInfo.MouseHover += new System.EventHandler(this.picTopFailuringInfo_MouseHover);
            // 
            // picNumSR_Scenarios
            // 
            this.picNumSR_Scenarios.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picNumSR_Scenarios.BackgroundImage")));
            this.picNumSR_Scenarios.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picNumSR_Scenarios.Location = new System.Drawing.Point(600, 7);
            this.picNumSR_Scenarios.Name = "picNumSR_Scenarios";
            this.picNumSR_Scenarios.Size = new System.Drawing.Size(18, 18);
            this.picNumSR_Scenarios.TabIndex = 68;
            this.picNumSR_Scenarios.TabStop = false;
            this.picNumSR_Scenarios.MouseHover += new System.EventHandler(this.picNumSR_Scenarios_MouseHover);
            // 
            // picCoresInfo
            // 
            this.picCoresInfo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picCoresInfo.BackgroundImage")));
            this.picCoresInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picCoresInfo.Location = new System.Drawing.Point(209, 103);
            this.picCoresInfo.Name = "picCoresInfo";
            this.picCoresInfo.Size = new System.Drawing.Size(18, 18);
            this.picCoresInfo.TabIndex = 66;
            this.picCoresInfo.TabStop = false;
            this.picCoresInfo.MouseHover += new System.EventHandler(this.picCoresInfo_MouseHover);
            // 
            // picFailureEndInfo
            // 
            this.picFailureEndInfo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picFailureEndInfo.BackgroundImage")));
            this.picFailureEndInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picFailureEndInfo.Location = new System.Drawing.Point(209, 79);
            this.picFailureEndInfo.Name = "picFailureEndInfo";
            this.picFailureEndInfo.Size = new System.Drawing.Size(18, 18);
            this.picFailureEndInfo.TabIndex = 65;
            this.picFailureEndInfo.TabStop = false;
            this.picFailureEndInfo.MouseHover += new System.EventHandler(this.picFailureEndInfo_MouseHover);
            // 
            // picFailureStartInfor
            // 
            this.picFailureStartInfor.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picFailureStartInfor.BackgroundImage")));
            this.picFailureStartInfor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picFailureStartInfor.Location = new System.Drawing.Point(209, 55);
            this.picFailureStartInfor.Name = "picFailureStartInfor";
            this.picFailureStartInfor.Size = new System.Drawing.Size(18, 18);
            this.picFailureStartInfor.TabIndex = 64;
            this.picFailureStartInfor.TabStop = false;
            this.picFailureStartInfor.MouseHover += new System.EventHandler(this.picFailureStartInfor_MouseHover);
            // 
            // picRoughnessInfo
            // 
            this.picRoughnessInfo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picRoughnessInfo.BackgroundImage")));
            this.picRoughnessInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picRoughnessInfo.Location = new System.Drawing.Point(209, 31);
            this.picRoughnessInfo.Name = "picRoughnessInfo";
            this.picRoughnessInfo.Size = new System.Drawing.Size(18, 18);
            this.picRoughnessInfo.TabIndex = 63;
            this.picRoughnessInfo.TabStop = false;
            this.picRoughnessInfo.MouseHover += new System.EventHandler(this.picRoughnessInfo_MouseHover);
            // 
            // picDiameterInfo
            // 
            this.picDiameterInfo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picDiameterInfo.BackgroundImage")));
            this.picDiameterInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picDiameterInfo.Location = new System.Drawing.Point(209, 7);
            this.picDiameterInfo.Name = "picDiameterInfo";
            this.picDiameterInfo.Size = new System.Drawing.Size(18, 18);
            this.picDiameterInfo.TabIndex = 62;
            this.picDiameterInfo.TabStop = false;
            this.picDiameterInfo.MouseHover += new System.EventHandler(this.picDiameterInfo_MouseHover);
            // 
            // lblFailureDiameter
            // 
            this.lblFailureDiameter.AutoSize = true;
            this.lblFailureDiameter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.lblFailureDiameter.Location = new System.Drawing.Point(8, 8);
            this.lblFailureDiameter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFailureDiameter.Name = "lblFailureDiameter";
            this.lblFailureDiameter.Size = new System.Drawing.Size(127, 16);
            this.lblFailureDiameter.TabIndex = 16;
            this.lblFailureDiameter.Text = "Failure Diameter (ft):";
            // 
            // lblStartFailureTime
            // 
            this.lblStartFailureTime.AutoSize = true;
            this.lblStartFailureTime.Location = new System.Drawing.Point(8, 56);
            this.lblStartFailureTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStartFailureTime.Name = "lblStartFailureTime";
            this.lblStartFailureTime.Size = new System.Drawing.Size(134, 16);
            this.lblStartFailureTime.TabIndex = 19;
            this.lblStartFailureTime.Text = "Failure Start Time (h):";
            // 
            // numFailureDiameter
            // 
            this.numFailureDiameter.DecimalPlaces = 3;
            this.numFailureDiameter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.numFailureDiameter.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numFailureDiameter.Location = new System.Drawing.Point(145, 6);
            this.numFailureDiameter.Margin = new System.Windows.Forms.Padding(2);
            this.numFailureDiameter.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numFailureDiameter.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numFailureDiameter.Name = "numFailureDiameter";
            this.numFailureDiameter.Size = new System.Drawing.Size(59, 22);
            this.numFailureDiameter.TabIndex = 5;
            this.numFailureDiameter.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            // 
            // nudFailureStartTime
            // 
            this.nudFailureStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudFailureStartTime.Location = new System.Drawing.Point(145, 54);
            this.nudFailureStartTime.Margin = new System.Windows.Forms.Padding(2);
            this.nudFailureStartTime.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudFailureStartTime.Name = "nudFailureStartTime";
            this.nudFailureStartTime.Size = new System.Drawing.Size(59, 22);
            this.nudFailureStartTime.TabIndex = 7;
            this.nudFailureStartTime.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lblEndFailureTime
            // 
            this.lblEndFailureTime.AutoSize = true;
            this.lblEndFailureTime.Location = new System.Drawing.Point(8, 80);
            this.lblEndFailureTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEndFailureTime.Name = "lblEndFailureTime";
            this.lblEndFailureTime.Size = new System.Drawing.Size(131, 16);
            this.lblEndFailureTime.TabIndex = 21;
            this.lblEndFailureTime.Text = "Failure End Time (h):";
            // 
            // nudFailureEndTime
            // 
            this.nudFailureEndTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudFailureEndTime.Location = new System.Drawing.Point(145, 78);
            this.nudFailureEndTime.Margin = new System.Windows.Forms.Padding(2);
            this.nudFailureEndTime.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudFailureEndTime.Name = "nudFailureEndTime";
            this.nudFailureEndTime.Size = new System.Drawing.Size(59, 22);
            this.nudFailureEndTime.TabIndex = 8;
            this.nudFailureEndTime.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // nudTopFailuring
            // 
            this.nudTopFailuring.BackColor = System.Drawing.SystemColors.Window;
            this.nudTopFailuring.DecimalPlaces = 2;
            this.nudTopFailuring.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudTopFailuring.Location = new System.Drawing.Point(536, 54);
            this.nudTopFailuring.Margin = new System.Windows.Forms.Padding(2);
            this.nudTopFailuring.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudTopFailuring.Name = "nudTopFailuring";
            this.nudTopFailuring.Size = new System.Drawing.Size(59, 22);
            this.nudTopFailuring.TabIndex = 12;
            this.nudTopFailuring.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // lblNumOfCores
            // 
            this.lblNumOfCores.AutoSize = true;
            this.lblNumOfCores.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.lblNumOfCores.Location = new System.Drawing.Point(8, 104);
            this.lblNumOfCores.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblNumOfCores.Name = "lblNumOfCores";
            this.lblNumOfCores.Size = new System.Drawing.Size(127, 16);
            this.lblNumOfCores.TabIndex = 27;
            this.lblNumOfCores.Text = "Logical Processors:";
            // 
            // lblMaxCores
            // 
            this.lblMaxCores.AutoSize = true;
            this.lblMaxCores.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.lblMaxCores.Location = new System.Drawing.Point(232, 104);
            this.lblMaxCores.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMaxCores.Name = "lblMaxCores";
            this.lblMaxCores.Size = new System.Drawing.Size(79, 16);
            this.lblMaxCores.TabIndex = 54;
            this.lblMaxCores.Text = "(Maximum: )";
            // 
            // nudCoreNumber
            // 
            this.nudCoreNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudCoreNumber.Location = new System.Drawing.Point(145, 102);
            this.nudCoreNumber.Margin = new System.Windows.Forms.Padding(2);
            this.nudCoreNumber.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudCoreNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudCoreNumber.Name = "nudCoreNumber";
            this.nudCoreNumber.Size = new System.Drawing.Size(59, 22);
            this.nudCoreNumber.TabIndex = 9;
            this.nudCoreNumber.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nudCoreNumber.ValueChanged += new System.EventHandler(this.nudCoreNumber_ValueChanged);
            // 
            // lblFailuringRoughness
            // 
            this.lblFailuringRoughness.AutoSize = true;
            this.lblFailuringRoughness.Location = new System.Drawing.Point(8, 32);
            this.lblFailuringRoughness.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFailuringRoughness.Name = "lblFailuringRoughness";
            this.lblFailuringRoughness.Size = new System.Drawing.Size(134, 16);
            this.lblFailuringRoughness.TabIndex = 33;
            this.lblFailuringRoughness.Text = "Failuring Roughness:";
            // 
            // nudFailuringRoughness
            // 
            this.nudFailuringRoughness.DecimalPlaces = 3;
            this.nudFailuringRoughness.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudFailuringRoughness.Location = new System.Drawing.Point(145, 30);
            this.nudFailuringRoughness.Margin = new System.Windows.Forms.Padding(2);
            this.nudFailuringRoughness.Name = "nudFailuringRoughness";
            this.nudFailuringRoughness.Size = new System.Drawing.Size(59, 22);
            this.nudFailuringRoughness.TabIndex = 6;
            // 
            // nudSequentialFailuring
            // 
            this.nudSequentialFailuring.BackColor = System.Drawing.SystemColors.Window;
            this.nudSequentialFailuring.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudSequentialFailuring.Location = new System.Drawing.Point(536, 6);
            this.nudSequentialFailuring.Margin = new System.Windows.Forms.Padding(2);
            this.nudSequentialFailuring.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.nudSequentialFailuring.Name = "nudSequentialFailuring";
            this.nudSequentialFailuring.Size = new System.Drawing.Size(59, 22);
            this.nudSequentialFailuring.TabIndex = 10;
            this.nudSequentialFailuring.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // nudRouletteWheelStartFailureScenario
            // 
            this.nudRouletteWheelStartFailureScenario.BackColor = System.Drawing.SystemColors.Window;
            this.nudRouletteWheelStartFailureScenario.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudRouletteWheelStartFailureScenario.Location = new System.Drawing.Point(536, 102);
            this.nudRouletteWheelStartFailureScenario.Margin = new System.Windows.Forms.Padding(2);
            this.nudRouletteWheelStartFailureScenario.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudRouletteWheelStartFailureScenario.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRouletteWheelStartFailureScenario.Name = "nudRouletteWheelStartFailureScenario";
            this.nudRouletteWheelStartFailureScenario.Size = new System.Drawing.Size(59, 22);
            this.nudRouletteWheelStartFailureScenario.TabIndex = 14;
            this.nudRouletteWheelStartFailureScenario.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblFailuringSequential
            // 
            this.lblFailuringSequential.AutoSize = true;
            this.lblFailuringSequential.Location = new System.Drawing.Point(386, 8);
            this.lblFailuringSequential.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFailuringSequential.Name = "lblFailuringSequential";
            this.lblFailuringSequential.Size = new System.Drawing.Size(150, 16);
            this.lblFailuringSequential.TabIndex = 37;
            this.lblFailuringSequential.Text = "S (Sequential Random):";
            // 
            // lblRouletteWheelStartFile
            // 
            this.lblRouletteWheelStartFile.AutoSize = true;
            this.lblRouletteWheelStartFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.lblRouletteWheelStartFile.Location = new System.Drawing.Point(386, 104);
            this.lblRouletteWheelStartFile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRouletteWheelStartFile.Name = "lblRouletteWheelStartFile";
            this.lblRouletteWheelStartFile.Size = new System.Drawing.Size(135, 16);
            this.lblRouletteWheelStartFile.TabIndex = 49;
            this.lblRouletteWheelStartFile.Text = "RW Failure Scenario:";
            // 
            // nudRouletteWheelStartFailureLevel
            // 
            this.nudRouletteWheelStartFailureLevel.BackColor = System.Drawing.SystemColors.Window;
            this.nudRouletteWheelStartFailureLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudRouletteWheelStartFailureLevel.Location = new System.Drawing.Point(536, 78);
            this.nudRouletteWheelStartFailureLevel.Margin = new System.Windows.Forms.Padding(2);
            this.nudRouletteWheelStartFailureLevel.Maximum = new decimal(new int[] {
            1005,
            0,
            0,
            0});
            this.nudRouletteWheelStartFailureLevel.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRouletteWheelStartFailureLevel.Name = "nudRouletteWheelStartFailureLevel";
            this.nudRouletteWheelStartFailureLevel.Size = new System.Drawing.Size(59, 22);
            this.nudRouletteWheelStartFailureLevel.TabIndex = 13;
            this.nudRouletteWheelStartFailureLevel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblRouletteWheelStartFolder
            // 
            this.lblRouletteWheelStartFolder.AutoSize = true;
            this.lblRouletteWheelStartFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.lblRouletteWheelStartFolder.Location = new System.Drawing.Point(386, 80);
            this.lblRouletteWheelStartFolder.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRouletteWheelStartFolder.Name = "lblRouletteWheelStartFolder";
            this.lblRouletteWheelStartFolder.Size = new System.Drawing.Size(117, 16);
            this.lblRouletteWheelStartFolder.TabIndex = 47;
            this.lblRouletteWheelStartFolder.Text = "RW Failure Level :";
            // 
            // lblNumOfTarget
            // 
            this.lblNumOfTarget.AutoSize = true;
            this.lblNumOfTarget.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.lblNumOfTarget.Location = new System.Drawing.Point(386, 32);
            this.lblNumOfTarget.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblNumOfTarget.Name = "lblNumOfTarget";
            this.lblNumOfTarget.Size = new System.Drawing.Size(120, 16);
            this.lblNumOfTarget.TabIndex = 42;
            this.lblNumOfTarget.Text = "S (Roulette Wheel)";
            // 
            // nudTargetNumber
            // 
            this.nudTargetNumber.BackColor = System.Drawing.SystemColors.Window;
            this.nudTargetNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.nudTargetNumber.Location = new System.Drawing.Point(536, 30);
            this.nudTargetNumber.Margin = new System.Windows.Forms.Padding(2);
            this.nudTargetNumber.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudTargetNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudTargetNumber.Name = "nudTargetNumber";
            this.nudTargetNumber.Size = new System.Drawing.Size(59, 22);
            this.nudTargetNumber.TabIndex = 11;
            this.nudTargetNumber.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // picLinksInfo
            // 
            this.picLinksInfo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picLinksInfo.BackgroundImage")));
            this.picLinksInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picLinksInfo.Location = new System.Drawing.Point(790, 129);
            this.picLinksInfo.Name = "picLinksInfo";
            this.picLinksInfo.Size = new System.Drawing.Size(18, 18);
            this.picLinksInfo.TabIndex = 61;
            this.picLinksInfo.TabStop = false;
            this.picLinksInfo.MouseHover += new System.EventHandler(this.pictureBox2_MouseHover);
            // 
            // lblLinks
            // 
            this.lblLinks.AutoSize = true;
            this.lblLinks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLinks.Location = new System.Drawing.Point(658, 131);
            this.lblLinks.Name = "lblLinks";
            this.lblLinks.Size = new System.Drawing.Size(44, 16);
            this.lblLinks.TabIndex = 60;
            this.lblLinks.Text = "Links";
            // 
            // flpLinks
            // 
            this.flpLinks.BackColor = System.Drawing.Color.LemonChiffon;
            this.flpLinks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flpLinks.Controls.Add(this.chkAllLinks);
            this.flpLinks.Controls.Add(this.chkMainLinks);
            this.flpLinks.Controls.Add(this.chkBranchLinks);
            this.flpLinks.Controls.Add(this.chkHighInflowLinks);
            this.flpLinks.Controls.Add(this.chkIMPLinks);
            this.flpLinks.Location = new System.Drawing.Point(659, 150);
            this.flpLinks.Name = "flpLinks";
            this.flpLinks.Padding = new System.Windows.Forms.Padding(3, 5, 5, 5);
            this.flpLinks.Size = new System.Drawing.Size(149, 132);
            this.flpLinks.TabIndex = 15;
            // 
            // chkAllLinks
            // 
            this.chkAllLinks.AutoSize = true;
            this.chkAllLinks.Checked = true;
            this.chkAllLinks.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllLinks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.chkAllLinks.Location = new System.Drawing.Point(5, 7);
            this.chkAllLinks.Margin = new System.Windows.Forms.Padding(2);
            this.chkAllLinks.Name = "chkAllLinks";
            this.chkAllLinks.Size = new System.Drawing.Size(76, 20);
            this.chkAllLinks.TabIndex = 15;
            this.chkAllLinks.Text = "All Links";
            this.chkAllLinks.UseVisualStyleBackColor = true;
            // 
            // chkMainLinks
            // 
            this.chkMainLinks.AutoSize = true;
            this.chkMainLinks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.chkMainLinks.Location = new System.Drawing.Point(5, 31);
            this.chkMainLinks.Margin = new System.Windows.Forms.Padding(2);
            this.chkMainLinks.Name = "chkMainLinks";
            this.chkMainLinks.Size = new System.Drawing.Size(110, 20);
            this.chkMainLinks.TabIndex = 16;
            this.chkMainLinks.Text = "Main Links (C)";
            this.chkMainLinks.UseVisualStyleBackColor = true;
            // 
            // chkBranchLinks
            // 
            this.chkBranchLinks.AutoSize = true;
            this.chkBranchLinks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.chkBranchLinks.Location = new System.Drawing.Point(5, 55);
            this.chkBranchLinks.Margin = new System.Windows.Forms.Padding(2);
            this.chkBranchLinks.Name = "chkBranchLinks";
            this.chkBranchLinks.Size = new System.Drawing.Size(123, 20);
            this.chkBranchLinks.TabIndex = 17;
            this.chkBranchLinks.Text = "Branch Links (B)";
            this.chkBranchLinks.UseVisualStyleBackColor = true;
            // 
            // chkHighInflowLinks
            // 
            this.chkHighInflowLinks.AutoSize = true;
            this.chkHighInflowLinks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.chkHighInflowLinks.Location = new System.Drawing.Point(5, 79);
            this.chkHighInflowLinks.Margin = new System.Windows.Forms.Padding(2);
            this.chkHighInflowLinks.Name = "chkHighInflowLinks";
            this.chkHighInflowLinks.Size = new System.Drawing.Size(146, 20);
            this.chkHighInflowLinks.TabIndex = 18;
            this.chkHighInflowLinks.Text = "High Inflow Links (H)";
            this.chkHighInflowLinks.UseVisualStyleBackColor = true;
            // 
            // chkIMPLinks
            // 
            this.chkIMPLinks.AutoSize = true;
            this.chkIMPLinks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.chkIMPLinks.Location = new System.Drawing.Point(5, 103);
            this.chkIMPLinks.Margin = new System.Windows.Forms.Padding(2);
            this.chkIMPLinks.Name = "chkIMPLinks";
            this.chkIMPLinks.Size = new System.Drawing.Size(123, 20);
            this.chkIMPLinks.TabIndex = 19;
            this.chkIMPLinks.Text = "Lateral Links (N)";
            this.chkIMPLinks.UseVisualStyleBackColor = true;
            // 
            // btnStartCombinationOSWMMDLL
            // 
            this.btnStartCombinationOSWMMDLL.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnStartCombinationOSWMMDLL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnStartCombinationOSWMMDLL.Location = new System.Drawing.Point(170, 300);
            this.btnStartCombinationOSWMMDLL.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartCombinationOSWMMDLL.Name = "btnStartCombinationOSWMMDLL";
            this.btnStartCombinationOSWMMDLL.Size = new System.Drawing.Size(150, 60);
            this.btnStartCombinationOSWMMDLL.TabIndex = 21;
            this.btnStartCombinationOSWMMDLL.Text = "Simulate All Scenarios (O-SWMM.dll)";
            this.btnStartCombinationOSWMMDLL.UseVisualStyleBackColor = false;
            this.btnStartCombinationOSWMMDLL.Click += new System.EventHandler(this.btnStartCombinationDLL_Click);
            // 
            // btnStartCombinationEXE
            // 
            this.btnStartCombinationEXE.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnStartCombinationEXE.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnStartCombinationEXE.Location = new System.Drawing.Point(8, 300);
            this.btnStartCombinationEXE.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartCombinationEXE.Name = "btnStartCombinationEXE";
            this.btnStartCombinationEXE.Size = new System.Drawing.Size(150, 60);
            this.btnStartCombinationEXE.TabIndex = 20;
            this.btnStartCombinationEXE.Text = "Simulate All Scenarios (SWMM.exe)";
            this.btnStartCombinationEXE.UseVisualStyleBackColor = false;
            this.btnStartCombinationEXE.Click += new System.EventHandler(this.btnStartCombinationEXE_Click);
            // 
            // btnStartRandom
            // 
            this.btnStartRandom.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnStartRandom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnStartRandom.Location = new System.Drawing.Point(333, 300);
            this.btnStartRandom.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartRandom.Name = "btnStartRandom";
            this.btnStartRandom.Size = new System.Drawing.Size(150, 60);
            this.btnStartRandom.TabIndex = 22;
            this.btnStartRandom.Text = "Simulate Scenarios (Random Selection)";
            this.btnStartRandom.UseVisualStyleBackColor = false;
            this.btnStartRandom.Click += new System.EventHandler(this.btnStartRandom_Click);
            // 
            // btnStartRouletteWheel
            // 
            this.btnStartRouletteWheel.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnStartRouletteWheel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnStartRouletteWheel.Location = new System.Drawing.Point(658, 300);
            this.btnStartRouletteWheel.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartRouletteWheel.Name = "btnStartRouletteWheel";
            this.btnStartRouletteWheel.Size = new System.Drawing.Size(150, 60);
            this.btnStartRouletteWheel.TabIndex = 24;
            this.btnStartRouletteWheel.Text = "Simulate Scenarios (Roulette Wheel)";
            this.btnStartRouletteWheel.UseVisualStyleBackColor = false;
            this.btnStartRouletteWheel.Click += new System.EventHandler(this.btnStartRouletteWheel_Click);
            // 
            // btnStartSequential
            // 
            this.btnStartSequential.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnStartSequential.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnStartSequential.Location = new System.Drawing.Point(495, 300);
            this.btnStartSequential.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartSequential.Name = "btnStartSequential";
            this.btnStartSequential.Size = new System.Drawing.Size(150, 60);
            this.btnStartSequential.TabIndex = 23;
            this.btnStartSequential.Text = "Simulate Scenarios (Sequential Random)";
            this.btnStartSequential.UseVisualStyleBackColor = false;
            this.btnStartSequential.Click += new System.EventHandler(this.btnStartSequential_Click);
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabGRASimulation);
            this.tabMain.Controls.Add(this.tabGRAnalysis);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Margin = new System.Windows.Forms.Padding(2);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(827, 509);
            this.tabMain.TabIndex = 29;
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 300;
            this.toolTip1.IsBalloon = true;
            this.toolTip1.OwnerDraw = true;
            this.toolTip1.ReshowDelay = 100;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(630, 115);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 17);
            this.label2.TabIndex = 45;
            this.label2.Text = "Total Inflow:";
            // 
            // nudTotalInflow
            // 
            this.nudTotalInflow.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudTotalInflow.Location = new System.Drawing.Point(746, 111);
            this.nudTotalInflow.Margin = new System.Windows.Forms.Padding(2);
            this.nudTotalInflow.Name = "nudTotalInflow";
            this.nudTotalInflow.Size = new System.Drawing.Size(58, 25);
            this.nudTotalInflow.TabIndex = 46;
            this.nudTotalInflow.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // chkCompleteFailureTime
            // 
            this.chkCompleteFailureTime.AutoSize = true;
            this.chkCompleteFailureTime.Checked = true;
            this.chkCompleteFailureTime.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCompleteFailureTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.chkCompleteFailureTime.Location = new System.Drawing.Point(232, 56);
            this.chkCompleteFailureTime.Margin = new System.Windows.Forms.Padding(2);
            this.chkCompleteFailureTime.Name = "chkCompleteFailureTime";
            this.chkCompleteFailureTime.Size = new System.Drawing.Size(119, 20);
            this.chkCompleteFailureTime.TabIndex = 79;
            this.chkCompleteFailureTime.Text = "Complete Time";
            this.chkCompleteFailureTime.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(827, 509);
            this.Controls.Add(this.tabMain);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "O-SWMM GRA Toolbox";
            this.tabGRAnalysis.ResumeLayout(false);
            this.tabGRAnalysis.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalFailureTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chtResilience)).EndInit();
            this.tabGRASimulation.ResumeLayout(false);
            this.tabGRASimulation.PerformLayout();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picFerdowsiUniversityLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picEpaLogo)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAVGDWFCoef)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picOutputDirectory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picInputFile)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picRW_FailureScenarioInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRW_StartFailureLevelInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNumRW_Scenarios)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTopFailuringInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNumSR_Scenarios)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCoresInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFailureEndInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFailureStartInfor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRoughnessInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDiameterInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFailureDiameter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFailureStartTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFailureEndTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTopFailuring)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoreNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFailuringRoughness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSequentialFailuring)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRouletteWheelStartFailureScenario)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRouletteWheelStartFailureLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTargetNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLinksInfo)).EndInit();
            this.flpLinks.ResumeLayout(false);
            this.flpLinks.PerformLayout();
            this.tabMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalInflow)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabPage tabGRAnalysis;
        private System.Windows.Forms.DataVisualization.Charting.Chart chtResilience;
        private System.Windows.Forms.Label lblSimulationTime;
        private System.Windows.Forms.NumericUpDown nudTotalFailureTime;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblAnalysis;
        private System.Windows.Forms.Button btnResultsDirectory;
        private System.Windows.Forms.TextBox txtResultsDirectory;
        private System.Windows.Forms.Label lblResultsDirectory;
        private System.Windows.Forms.TabPage tabGRASimulation;
        private System.Windows.Forms.Button btnStartCombinationOSWMMDLL;
        private System.Windows.Forms.Button btnStartCombinationEXE;
        private System.Windows.Forms.Button btnStartRandom;
        private System.Windows.Forms.Label lblMaxCores;
        private System.Windows.Forms.TextBox txtInputFileName;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.NumericUpDown nudRouletteWheelStartFailureScenario;
        private System.Windows.Forms.Label lblRouletteWheelStartFile;
        private System.Windows.Forms.NumericUpDown nudRouletteWheelStartFailureLevel;
        private System.Windows.Forms.Label lblRouletteWheelStartFolder;
        private System.Windows.Forms.Button btnStartRouletteWheel;
        private System.Windows.Forms.NumericUpDown nudTopFailuring;
        private System.Windows.Forms.NumericUpDown nudSequentialFailuring;
        private System.Windows.Forms.Label lblFailuringSequential;
        private System.Windows.Forms.Button btnStartSequential;
        private System.Windows.Forms.NumericUpDown nudFailuringRoughness;
        private System.Windows.Forms.Label lblFailuringRoughness;
        private System.Windows.Forms.CheckBox chkIMPLinks;
        private System.Windows.Forms.Label lblInputFileName;
        private System.Windows.Forms.NumericUpDown nudCoreNumber;
        private System.Windows.Forms.Label lblNumOfCores;
        private System.Windows.Forms.Button btnSelectFileName;
        private System.Windows.Forms.CheckBox chkAllLinks;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.CheckBox chkHighInflowLinks;
        private System.Windows.Forms.CheckBox chkBranchLinks;
        private System.Windows.Forms.Button btnOutputFolder;
        private System.Windows.Forms.CheckBox chkMainLinks;
        private System.Windows.Forms.NumericUpDown nudFailureEndTime;
        private System.Windows.Forms.Label lblFailureLevelCounter;
        private System.Windows.Forms.Label lblEndFailureTime;
        private System.Windows.Forms.Label lblFailureDiameter;
        private System.Windows.Forms.NumericUpDown nudFailureStartTime;
        private System.Windows.Forms.NumericUpDown numFailureDiameter;
        private System.Windows.Forms.Label lblStartFailureTime;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.PictureBox picLinksInfo;
        private System.Windows.Forms.Label lblLinks;
        private System.Windows.Forms.FlowLayoutPanel flpLinks;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.NumericUpDown nudTargetNumber;
        private System.Windows.Forms.Label lblNumOfTarget;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox picRW_FailureScenarioInfo;
        private System.Windows.Forms.PictureBox picRW_StartFailureLevelInfo;
        private System.Windows.Forms.PictureBox picNumRW_Scenarios;
        private System.Windows.Forms.PictureBox picTopFailuringInfo;
        private System.Windows.Forms.PictureBox picNumSR_Scenarios;
        private System.Windows.Forms.PictureBox picCoresInfo;
        private System.Windows.Forms.PictureBox picFailureEndInfo;
        private System.Windows.Forms.PictureBox picFailureStartInfor;
        private System.Windows.Forms.PictureBox picRoughnessInfo;
        private System.Windows.Forms.PictureBox picDiameterInfo;
        private System.Windows.Forms.Label lblSimulationProperties;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox picOutputDirectory;
        private System.Windows.Forms.PictureBox picInputFile;
        private System.Windows.Forms.Label lbliInputOutput;
        private System.Windows.Forms.Label lblTotalTime;
        private System.Windows.Forms.Label lblScenarioCounter;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.PictureBox picFerdowsiUniversityLogo;
        private System.Windows.Forms.PictureBox picEpaLogo;
        private System.Windows.Forms.RichTextBox rtbTopFlooding;
        private CheckBox chkNoReportFiles;
        private NumericUpDown nudAVGDWFCoef;
        private Label label1;
        private NumericUpDown nudTotalInflow;
        private Label label2;
        private CheckBox chkCompleteFailureTime;
    }

}

