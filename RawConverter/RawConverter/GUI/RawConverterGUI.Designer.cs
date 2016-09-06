namespace RawConverter.GUI
{
    partial class RawConverterGUI
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
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mzDecimalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mzDecimal1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mzDecimal2MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mzDecimal3MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mzDecimal4MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mzDecimal5MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intensityDecimalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intensityDecimal0MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intensityDecimal1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intensityDecimal2MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intensityDecimal3MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intensityDecimal4MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intensityDecimal5MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sep1MenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.ddaMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeStateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState2MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState3MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState4MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState5MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState6MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState7MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState8MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState9MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chargeState10MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.singleSpectrumAnalysisToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.dIAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.singleSpectrumAnalysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.extractPrecursorByMzMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.msnOptioinMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPeakChargeStatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPeakResolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportChargeStateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gbFiles = new System.Windows.Forms.GroupBox();
            this.btDelFile = new System.Windows.Forms.Button();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.cbCentroid = new System.Windows.Forms.CheckBox();
            this.cbPredDiaPrecs = new System.Windows.Forms.CheckBox();
            this.cbMassCorrection = new System.Windows.Forms.CheckBox();
            this.btAddFile = new System.Windows.Forms.Button();
            this.gbExpType = new System.Windows.Forms.GroupBox();
            this.rbDIA = new System.Windows.Forms.RadioButton();
            this.rbDDA = new System.Windows.Forms.RadioButton();
            this.lbInputFiles = new System.Windows.Forms.ListBox();
            this.fileBrowserDialog = new System.Windows.Forms.OpenFileDialog();
            this.outDirBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.gbOutput = new System.Windows.Forms.GroupBox();
            this.cbMzML = new System.Windows.Forms.CheckBox();
            this.cbMzXML = new System.Windows.Forms.CheckBox();
            this.cbMs2 = new System.Windows.Forms.CheckBox();
            this.labelOutFormat = new System.Windows.Forms.Label();
            this.cbMs3 = new System.Windows.Forms.CheckBox();
            this.cbMgf = new System.Windows.Forms.CheckBox();
            this.tbOutDir = new System.Windows.Forms.TextBox();
            this.cbLog = new System.Windows.Forms.CheckBox();
            this.btBrowse = new System.Windows.Forms.Button();
            this.cbMs1 = new System.Windows.Forms.CheckBox();
            this.labelOutDir = new System.Windows.Forms.Label();
            this.cbOutFormat = new System.Windows.Forms.ComboBox();
            this.progBar = new System.Windows.Forms.ProgressBar();
            this.btGo = new System.Windows.Forms.Button();
            this.gbLog = new System.Windows.Forms.GroupBox();
            this.lbLog = new System.Windows.Forms.ListBox();
            this.labelFileProcessing = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.mainMenu.SuspendLayout();
            this.gbFiles.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.gbExpType.SuspendLayout();
            this.gbOutput.SuspendLayout();
            this.gbLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(667, 24);
            this.mainMenu.TabIndex = 0;
            this.mainMenu.TabStop = true;
            this.mainMenu.Text = "Main Menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mzDecimalMenuItem,
            this.intensityDecimalMenuItem,
            this.sep1MenuItem,
            this.ddaMenuItem,
            this.dIAToolStripMenuItem,
            this.toolStripMenuItem1,
            this.extractPrecursorByMzMenuItem,
            this.toolStripMenuItem2,
            this.msnOptioinMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // mzDecimalMenuItem
            // 
            this.mzDecimalMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mzDecimal1MenuItem,
            this.mzDecimal2MenuItem,
            this.mzDecimal3MenuItem,
            this.mzDecimal4MenuItem,
            this.mzDecimal5MenuItem});
            this.mzDecimalMenuItem.Name = "mzDecimalMenuItem";
            this.mzDecimalMenuItem.Size = new System.Drawing.Size(204, 22);
            this.mzDecimalMenuItem.Text = "M/Z Decimal Places";
            // 
            // mzDecimal1MenuItem
            // 
            this.mzDecimal1MenuItem.Name = "mzDecimal1MenuItem";
            this.mzDecimal1MenuItem.Size = new System.Drawing.Size(80, 22);
            this.mzDecimal1MenuItem.Text = "1";
            this.mzDecimal1MenuItem.Click += new System.EventHandler(this.mzDecimal1MenuItem_Click);
            // 
            // mzDecimal2MenuItem
            // 
            this.mzDecimal2MenuItem.Name = "mzDecimal2MenuItem";
            this.mzDecimal2MenuItem.Size = new System.Drawing.Size(80, 22);
            this.mzDecimal2MenuItem.Text = "2";
            this.mzDecimal2MenuItem.Click += new System.EventHandler(this.mzDecimal2MenuItem_Click);
            // 
            // mzDecimal3MenuItem
            // 
            this.mzDecimal3MenuItem.Name = "mzDecimal3MenuItem";
            this.mzDecimal3MenuItem.Size = new System.Drawing.Size(80, 22);
            this.mzDecimal3MenuItem.Text = "3";
            this.mzDecimal3MenuItem.Click += new System.EventHandler(this.mzDecimal3MenuItem_Click);
            // 
            // mzDecimal4MenuItem
            // 
            this.mzDecimal4MenuItem.Checked = true;
            this.mzDecimal4MenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mzDecimal4MenuItem.Name = "mzDecimal4MenuItem";
            this.mzDecimal4MenuItem.Size = new System.Drawing.Size(80, 22);
            this.mzDecimal4MenuItem.Text = "4";
            this.mzDecimal4MenuItem.Click += new System.EventHandler(this.mzDecimal4MenuItem_Click);
            // 
            // mzDecimal5MenuItem
            // 
            this.mzDecimal5MenuItem.Name = "mzDecimal5MenuItem";
            this.mzDecimal5MenuItem.Size = new System.Drawing.Size(80, 22);
            this.mzDecimal5MenuItem.Text = "5";
            this.mzDecimal5MenuItem.Click += new System.EventHandler(this.mzDecimal5MenuItem_Click);
            // 
            // intensityDecimalMenuItem
            // 
            this.intensityDecimalMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.intensityDecimal0MenuItem,
            this.intensityDecimal1MenuItem,
            this.intensityDecimal2MenuItem,
            this.intensityDecimal3MenuItem,
            this.intensityDecimal4MenuItem,
            this.intensityDecimal5MenuItem});
            this.intensityDecimalMenuItem.Name = "intensityDecimalMenuItem";
            this.intensityDecimalMenuItem.Size = new System.Drawing.Size(204, 22);
            this.intensityDecimalMenuItem.Text = "Intensity Decimal Places";
            // 
            // intensityDecimal0MenuItem
            // 
            this.intensityDecimal0MenuItem.Name = "intensityDecimal0MenuItem";
            this.intensityDecimal0MenuItem.Size = new System.Drawing.Size(80, 22);
            this.intensityDecimal0MenuItem.Text = "0";
            this.intensityDecimal0MenuItem.Click += new System.EventHandler(this.intensityDecimal0MenuItem_Click);
            // 
            // intensityDecimal1MenuItem
            // 
            this.intensityDecimal1MenuItem.Checked = true;
            this.intensityDecimal1MenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.intensityDecimal1MenuItem.Name = "intensityDecimal1MenuItem";
            this.intensityDecimal1MenuItem.Size = new System.Drawing.Size(80, 22);
            this.intensityDecimal1MenuItem.Text = "1";
            this.intensityDecimal1MenuItem.Click += new System.EventHandler(this.intensityDecimal1MenuItem_Click);
            // 
            // intensityDecimal2MenuItem
            // 
            this.intensityDecimal2MenuItem.Name = "intensityDecimal2MenuItem";
            this.intensityDecimal2MenuItem.Size = new System.Drawing.Size(80, 22);
            this.intensityDecimal2MenuItem.Text = "2";
            this.intensityDecimal2MenuItem.Click += new System.EventHandler(this.intensityDecimal2MenuItem_Click);
            // 
            // intensityDecimal3MenuItem
            // 
            this.intensityDecimal3MenuItem.Name = "intensityDecimal3MenuItem";
            this.intensityDecimal3MenuItem.Size = new System.Drawing.Size(80, 22);
            this.intensityDecimal3MenuItem.Text = "3";
            this.intensityDecimal3MenuItem.Click += new System.EventHandler(this.intensityDecimal3MenuItem_Click);
            // 
            // intensityDecimal4MenuItem
            // 
            this.intensityDecimal4MenuItem.Name = "intensityDecimal4MenuItem";
            this.intensityDecimal4MenuItem.Size = new System.Drawing.Size(80, 22);
            this.intensityDecimal4MenuItem.Text = "4";
            this.intensityDecimal4MenuItem.Click += new System.EventHandler(this.intensityDecimal4MenuItem_Click);
            // 
            // intensityDecimal5MenuItem
            // 
            this.intensityDecimal5MenuItem.Name = "intensityDecimal5MenuItem";
            this.intensityDecimal5MenuItem.Size = new System.Drawing.Size(80, 22);
            this.intensityDecimal5MenuItem.Text = "5";
            this.intensityDecimal5MenuItem.Click += new System.EventHandler(this.intensityDecimal5MenuItem_Click);
            // 
            // sep1MenuItem
            // 
            this.sep1MenuItem.Name = "sep1MenuItem";
            this.sep1MenuItem.Size = new System.Drawing.Size(201, 6);
            // 
            // ddaMenuItem
            // 
            this.ddaMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chargeStateToolStripMenuItem,
            this.singleSpectrumAnalysisToolStripMenuItem1});
            this.ddaMenuItem.Name = "ddaMenuItem";
            this.ddaMenuItem.Size = new System.Drawing.Size(204, 22);
            this.ddaMenuItem.Text = "DDA";
            // 
            // chargeStateToolStripMenuItem
            // 
            this.chargeStateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chargeState1MenuItem,
            this.chargeState2MenuItem,
            this.chargeState3MenuItem,
            this.chargeState4MenuItem,
            this.chargeState5MenuItem,
            this.chargeState6MenuItem,
            this.chargeState7MenuItem,
            this.chargeState8MenuItem,
            this.chargeState9MenuItem,
            this.chargeState10MenuItem});
            this.chargeStateToolStripMenuItem.Name = "chargeStateToolStripMenuItem";
            this.chargeStateToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.chargeStateToolStripMenuItem.Text = "Charge States";
            // 
            // chargeState1MenuItem
            // 
            this.chargeState1MenuItem.Name = "chargeState1MenuItem";
            this.chargeState1MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState1MenuItem.Text = "1+";
            this.chargeState1MenuItem.Click += new System.EventHandler(this.chargeState1MenuItem_Click);
            // 
            // chargeState2MenuItem
            // 
            this.chargeState2MenuItem.Checked = true;
            this.chargeState2MenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chargeState2MenuItem.Name = "chargeState2MenuItem";
            this.chargeState2MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState2MenuItem.Text = "2+";
            this.chargeState2MenuItem.Click += new System.EventHandler(this.chargeState2MenuItem_Click);
            // 
            // chargeState3MenuItem
            // 
            this.chargeState3MenuItem.Checked = true;
            this.chargeState3MenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chargeState3MenuItem.Name = "chargeState3MenuItem";
            this.chargeState3MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState3MenuItem.Text = "3+";
            this.chargeState3MenuItem.Click += new System.EventHandler(this.chargeState3MenuItem_Click);
            // 
            // chargeState4MenuItem
            // 
            this.chargeState4MenuItem.Name = "chargeState4MenuItem";
            this.chargeState4MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState4MenuItem.Text = "4+";
            this.chargeState4MenuItem.Click += new System.EventHandler(this.chargeState4MenuItem_Click);
            // 
            // chargeState5MenuItem
            // 
            this.chargeState5MenuItem.Name = "chargeState5MenuItem";
            this.chargeState5MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState5MenuItem.Text = "5+";
            this.chargeState5MenuItem.Click += new System.EventHandler(this.chargeState5MenuItem_Click);
            // 
            // chargeState6MenuItem
            // 
            this.chargeState6MenuItem.Name = "chargeState6MenuItem";
            this.chargeState6MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState6MenuItem.Text = "6+";
            this.chargeState6MenuItem.Click += new System.EventHandler(this.chargeState6MenuItem_Click);
            // 
            // chargeState7MenuItem
            // 
            this.chargeState7MenuItem.Name = "chargeState7MenuItem";
            this.chargeState7MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState7MenuItem.Text = "7+";
            this.chargeState7MenuItem.Click += new System.EventHandler(this.chargeState7MenuItem_Click);
            // 
            // chargeState8MenuItem
            // 
            this.chargeState8MenuItem.Name = "chargeState8MenuItem";
            this.chargeState8MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState8MenuItem.Text = "8+";
            this.chargeState8MenuItem.Click += new System.EventHandler(this.chargeState8MenuItem_Click);
            // 
            // chargeState9MenuItem
            // 
            this.chargeState9MenuItem.Name = "chargeState9MenuItem";
            this.chargeState9MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState9MenuItem.Text = "9+";
            this.chargeState9MenuItem.Click += new System.EventHandler(this.chargeState9MenuItem_Click);
            // 
            // chargeState10MenuItem
            // 
            this.chargeState10MenuItem.Name = "chargeState10MenuItem";
            this.chargeState10MenuItem.Size = new System.Drawing.Size(94, 22);
            this.chargeState10MenuItem.Text = "10+";
            this.chargeState10MenuItem.Click += new System.EventHandler(this.chargeState10MenuItem_Click);
            // 
            // singleSpectrumAnalysisToolStripMenuItem1
            // 
            this.singleSpectrumAnalysisToolStripMenuItem1.Name = "singleSpectrumAnalysisToolStripMenuItem1";
            this.singleSpectrumAnalysisToolStripMenuItem1.Size = new System.Drawing.Size(206, 22);
            this.singleSpectrumAnalysisToolStripMenuItem1.Text = "Single Spectrum Analysis";
            this.singleSpectrumAnalysisToolStripMenuItem1.Click += new System.EventHandler(this.singleSpectrumAnalysisToolStripMenuItem1_Click);
            // 
            // dIAToolStripMenuItem
            // 
            this.dIAToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.singleSpectrumAnalysisToolStripMenuItem});
            this.dIAToolStripMenuItem.Name = "dIAToolStripMenuItem";
            this.dIAToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.dIAToolStripMenuItem.Text = "DIA";
            // 
            // singleSpectrumAnalysisToolStripMenuItem
            // 
            this.singleSpectrumAnalysisToolStripMenuItem.Name = "singleSpectrumAnalysisToolStripMenuItem";
            this.singleSpectrumAnalysisToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.singleSpectrumAnalysisToolStripMenuItem.Text = "Single Spectrum Analysis";
            this.singleSpectrumAnalysisToolStripMenuItem.Click += new System.EventHandler(this.singleSpectrumAnalysisToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(201, 6);
            // 
            // extractPrecursorByMzMenuItem
            // 
            this.extractPrecursorByMzMenuItem.Name = "extractPrecursorByMzMenuItem";
            this.extractPrecursorByMzMenuItem.Size = new System.Drawing.Size(204, 22);
            this.extractPrecursorByMzMenuItem.Text = "Extract Precursor By M/Z";
            this.extractPrecursorByMzMenuItem.Click += new System.EventHandler(this.extractPrecursorByMzToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(201, 6);
            // 
            // msnOptioinMenuItem
            // 
            this.msnOptioinMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showPeakChargeStatesToolStripMenuItem,
            this.showPeakResolutionToolStripMenuItem,
            this.exportChargeStateToolStripMenuItem,
            this.bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem});
            this.msnOptioinMenuItem.Name = "msnOptioinMenuItem";
            this.msnOptioinMenuItem.Size = new System.Drawing.Size(204, 22);
            this.msnOptioinMenuItem.Text = "MSn Options";
            // 
            // showPeakChargeStatesToolStripMenuItem
            // 
            this.showPeakChargeStatesToolStripMenuItem.Checked = true;
            this.showPeakChargeStatesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showPeakChargeStatesToolStripMenuItem.Name = "showPeakChargeStatesToolStripMenuItem";
            this.showPeakChargeStatesToolStripMenuItem.Size = new System.Drawing.Size(337, 22);
            this.showPeakChargeStatesToolStripMenuItem.Text = "Show Peak Charge States";
            this.showPeakChargeStatesToolStripMenuItem.Click += new System.EventHandler(this.showPeakChargeStatesToolStripMenuItem_Click);
            // 
            // showPeakResolutionToolStripMenuItem
            // 
            this.showPeakResolutionToolStripMenuItem.Checked = true;
            this.showPeakResolutionToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showPeakResolutionToolStripMenuItem.Name = "showPeakResolutionToolStripMenuItem";
            this.showPeakResolutionToolStripMenuItem.Size = new System.Drawing.Size(337, 22);
            this.showPeakResolutionToolStripMenuItem.Text = "Show Peak Resolution";
            this.showPeakResolutionToolStripMenuItem.Click += new System.EventHandler(this.showPeakResolutionToolStripMenuItem_Click);
            // 
            // exportChargeStateToolStripMenuItem
            // 
            this.exportChargeStateToolStripMenuItem.Name = "exportChargeStateToolStripMenuItem";
            this.exportChargeStateToolStripMenuItem.Size = new System.Drawing.Size(337, 22);
            this.exportChargeStateToolStripMenuItem.Text = "Export Charge State Statistics";
            this.exportChargeStateToolStripMenuItem.Click += new System.EventHandler(this.exportChargeStateToolStripMenuItem_Click);
            // 
            // bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem
            // 
            this.bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.Checked = true;
            this.bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.Name = "bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem";
            this.bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.Size = new System.Drawing.Size(337, 22);
            this.bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.Text = "Bypass Thermo Monoisotope Selection Algorithm";
            this.bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.Click += new System.EventHandler(this.bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // gbFiles
            // 
            this.gbFiles.Controls.Add(this.btDelFile);
            this.gbFiles.Controls.Add(this.gbOptions);
            this.gbFiles.Controls.Add(this.btAddFile);
            this.gbFiles.Controls.Add(this.gbExpType);
            this.gbFiles.Controls.Add(this.lbInputFiles);
            this.gbFiles.Location = new System.Drawing.Point(16, 30);
            this.gbFiles.Name = "gbFiles";
            this.gbFiles.Size = new System.Drawing.Size(636, 203);
            this.gbFiles.TabIndex = 1;
            this.gbFiles.TabStop = false;
            this.gbFiles.Text = "Files to Convert";
            // 
            // btDelFile
            // 
            this.btDelFile.Location = new System.Drawing.Point(309, 54);
            this.btDelFile.Name = "btDelFile";
            this.btDelFile.Size = new System.Drawing.Size(29, 27);
            this.btDelFile.TabIndex = 3;
            this.btDelFile.Text = "-";
            this.btDelFile.UseVisualStyleBackColor = true;
            this.btDelFile.Click += new System.EventHandler(this.btDelFile_Click);
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.cbCentroid);
            this.gbOptions.Controls.Add(this.cbPredDiaPrecs);
            this.gbOptions.Controls.Add(this.cbMassCorrection);
            this.gbOptions.Location = new System.Drawing.Point(356, 87);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(274, 105);
            this.gbOptions.TabIndex = 7;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Options";
            // 
            // cbCentroid
            // 
            this.cbCentroid.AutoSize = true;
            this.cbCentroid.Location = new System.Drawing.Point(14, 49);
            this.cbCentroid.Name = "cbCentroid";
            this.cbCentroid.Size = new System.Drawing.Size(179, 17);
            this.cbCentroid.TabIndex = 3;
            this.cbCentroid.Text = "Export Centroided Peaks in MS1";
            this.cbCentroid.UseVisualStyleBackColor = true;
            // 
            // cbPredDiaPrecs
            // 
            this.cbPredDiaPrecs.AutoSize = true;
            this.cbPredDiaPrecs.Location = new System.Drawing.Point(14, 72);
            this.cbPredDiaPrecs.Name = "cbPredDiaPrecs";
            this.cbPredDiaPrecs.Size = new System.Drawing.Size(143, 17);
            this.cbPredDiaPrecs.TabIndex = 2;
            this.cbPredDiaPrecs.Text = "Predict precursors in DIA";
            this.cbPredDiaPrecs.UseVisualStyleBackColor = true;
            // 
            // cbMassCorrection
            // 
            this.cbMassCorrection.AutoSize = true;
            this.cbMassCorrection.Location = new System.Drawing.Point(14, 26);
            this.cbMassCorrection.Name = "cbMassCorrection";
            this.cbMassCorrection.Size = new System.Drawing.Size(179, 17);
            this.cbMassCorrection.TabIndex = 0;
            this.cbMassCorrection.Text = "Select monoisotopic m/z in DDA";
            this.cbMassCorrection.UseVisualStyleBackColor = true;
            // 
            // btAddFile
            // 
            this.btAddFile.Location = new System.Drawing.Point(309, 19);
            this.btAddFile.Name = "btAddFile";
            this.btAddFile.Size = new System.Drawing.Size(29, 27);
            this.btAddFile.TabIndex = 2;
            this.btAddFile.Text = "+";
            this.btAddFile.UseVisualStyleBackColor = true;
            this.btAddFile.Click += new System.EventHandler(this.btAddFile_Click);
            // 
            // gbExpType
            // 
            this.gbExpType.Controls.Add(this.rbDIA);
            this.gbExpType.Controls.Add(this.rbDDA);
            this.gbExpType.Location = new System.Drawing.Point(356, 19);
            this.gbExpType.Name = "gbExpType";
            this.gbExpType.Size = new System.Drawing.Size(274, 62);
            this.gbExpType.TabIndex = 1;
            this.gbExpType.TabStop = false;
            this.gbExpType.Text = "Experiment Type";
            // 
            // rbDIA
            // 
            this.rbDIA.AutoSize = true;
            this.rbDIA.Location = new System.Drawing.Point(145, 25);
            this.rbDIA.Name = "rbDIA";
            this.rbDIA.Size = new System.Drawing.Size(111, 17);
            this.rbDIA.TabIndex = 1;
            this.rbDIA.TabStop = true;
            this.rbDIA.Text = "Data Independent";
            this.rbDIA.UseVisualStyleBackColor = true;
            this.rbDIA.CheckedChanged += new System.EventHandler(this.rbDIA_CheckedChanged);
            // 
            // rbDDA
            // 
            this.rbDDA.AutoSize = true;
            this.rbDDA.Location = new System.Drawing.Point(21, 25);
            this.rbDDA.Name = "rbDDA";
            this.rbDDA.Size = new System.Drawing.Size(104, 17);
            this.rbDDA.TabIndex = 0;
            this.rbDDA.TabStop = true;
            this.rbDDA.Text = "Data Dependent";
            this.rbDDA.UseVisualStyleBackColor = true;
            this.rbDDA.CheckedChanged += new System.EventHandler(this.rbDDA_CheckedChanged);
            // 
            // lbInputFiles
            // 
            this.lbInputFiles.FormattingEnabled = true;
            this.lbInputFiles.HorizontalScrollbar = true;
            this.lbInputFiles.Location = new System.Drawing.Point(6, 19);
            this.lbInputFiles.Name = "lbInputFiles";
            this.lbInputFiles.Size = new System.Drawing.Size(297, 173);
            this.lbInputFiles.TabIndex = 0;
            // 
            // fileBrowserDialog
            // 
            this.fileBrowserDialog.Multiselect = true;
            // 
            // gbOutput
            // 
            this.gbOutput.Controls.Add(this.cbMzML);
            this.gbOutput.Controls.Add(this.cbMzXML);
            this.gbOutput.Controls.Add(this.cbMs2);
            this.gbOutput.Controls.Add(this.labelOutFormat);
            this.gbOutput.Controls.Add(this.cbMs3);
            this.gbOutput.Controls.Add(this.cbMgf);
            this.gbOutput.Controls.Add(this.tbOutDir);
            this.gbOutput.Controls.Add(this.cbLog);
            this.gbOutput.Controls.Add(this.btBrowse);
            this.gbOutput.Controls.Add(this.cbMs1);
            this.gbOutput.Controls.Add(this.labelOutDir);
            this.gbOutput.Controls.Add(this.cbOutFormat);
            this.gbOutput.Location = new System.Drawing.Point(16, 239);
            this.gbOutput.Name = "gbOutput";
            this.gbOutput.Size = new System.Drawing.Size(636, 132);
            this.gbOutput.TabIndex = 2;
            this.gbOutput.TabStop = false;
            this.gbOutput.Text = "Output";
            // 
            // cbMzML
            // 
            this.cbMzML.Enabled = false;
            this.cbMzML.Location = new System.Drawing.Point(567, 80);
            this.cbMzML.Name = "cbMzML";
            this.cbMzML.Size = new System.Drawing.Size(63, 20);
            this.cbMzML.TabIndex = 9;
            this.cbMzML.Text = "mzML";
            this.cbMzML.UseVisualStyleBackColor = true;
            // 
            // cbMzXML
            // 
            this.cbMzXML.Enabled = false;
            this.cbMzXML.Location = new System.Drawing.Point(490, 80);
            this.cbMzXML.Name = "cbMzXML";
            this.cbMzXML.Size = new System.Drawing.Size(63, 20);
            this.cbMzXML.TabIndex = 8;
            this.cbMzXML.Text = "mzXML";
            this.cbMzXML.UseVisualStyleBackColor = true;
            // 
            // cbMs2
            // 
            this.cbMs2.Enabled = false;
            this.cbMs2.Location = new System.Drawing.Point(490, 57);
            this.cbMs2.Name = "cbMs2";
            this.cbMs2.Size = new System.Drawing.Size(48, 17);
            this.cbMs2.TabIndex = 5;
            this.cbMs2.Text = "MS2";
            this.cbMs2.UseVisualStyleBackColor = true;
            // 
            // labelOutFormat
            // 
            this.labelOutFormat.AutoSize = true;
            this.labelOutFormat.Location = new System.Drawing.Point(6, 70);
            this.labelOutFormat.Name = "labelOutFormat";
            this.labelOutFormat.Size = new System.Drawing.Size(85, 13);
            this.labelOutFormat.TabIndex = 7;
            this.labelOutFormat.Text = "Output Formats: ";
            // 
            // cbMs3
            // 
            this.cbMs3.Enabled = false;
            this.cbMs3.Location = new System.Drawing.Point(567, 57);
            this.cbMs3.Name = "cbMs3";
            this.cbMs3.Size = new System.Drawing.Size(48, 17);
            this.cbMs3.TabIndex = 4;
            this.cbMs3.Text = "MS3";
            this.cbMs3.UseVisualStyleBackColor = true;
            // 
            // cbMgf
            // 
            this.cbMgf.Enabled = false;
            this.cbMgf.Location = new System.Drawing.Point(411, 83);
            this.cbMgf.Name = "cbMgf";
            this.cbMgf.Size = new System.Drawing.Size(49, 17);
            this.cbMgf.TabIndex = 3;
            this.cbMgf.Text = "MGF";
            this.cbMgf.UseVisualStyleBackColor = true;
            // 
            // tbOutDir
            // 
            this.tbOutDir.Location = new System.Drawing.Point(114, 19);
            this.tbOutDir.Name = "tbOutDir";
            this.tbOutDir.Size = new System.Drawing.Size(453, 20);
            this.tbOutDir.TabIndex = 5;
            // 
            // cbLog
            // 
            this.cbLog.Enabled = false;
            this.cbLog.Location = new System.Drawing.Point(411, 109);
            this.cbLog.Name = "cbLog";
            this.cbLog.Size = new System.Drawing.Size(44, 17);
            this.cbLog.TabIndex = 2;
            this.cbLog.Text = "Log";
            this.cbLog.UseVisualStyleBackColor = true;
            // 
            // btBrowse
            // 
            this.btBrowse.Location = new System.Drawing.Point(573, 19);
            this.btBrowse.Name = "btBrowse";
            this.btBrowse.Size = new System.Drawing.Size(57, 20);
            this.btBrowse.TabIndex = 4;
            this.btBrowse.Text = "Browse";
            this.btBrowse.UseVisualStyleBackColor = true;
            this.btBrowse.Click += new System.EventHandler(this.btBrowse_Click);
            // 
            // cbMs1
            // 
            this.cbMs1.Enabled = false;
            this.cbMs1.Location = new System.Drawing.Point(411, 57);
            this.cbMs1.Name = "cbMs1";
            this.cbMs1.Size = new System.Drawing.Size(48, 17);
            this.cbMs1.TabIndex = 1;
            this.cbMs1.Text = "MS1";
            this.cbMs1.UseVisualStyleBackColor = true;
            // 
            // labelOutDir
            // 
            this.labelOutDir.AutoSize = true;
            this.labelOutDir.Location = new System.Drawing.Point(6, 22);
            this.labelOutDir.Name = "labelOutDir";
            this.labelOutDir.Size = new System.Drawing.Size(111, 13);
            this.labelOutDir.TabIndex = 6;
            this.labelOutDir.Text = "Destination Directory: ";
            // 
            // cbOutFormat
            // 
            this.cbOutFormat.FormattingEnabled = true;
            this.cbOutFormat.Items.AddRange(new object[] {
            "MS1, MS2, and MS3",
            "MGF",
            "mzXML",
            "mzML",
            "All"});
            this.cbOutFormat.Location = new System.Drawing.Point(114, 66);
            this.cbOutFormat.Name = "cbOutFormat";
            this.cbOutFormat.Size = new System.Drawing.Size(278, 21);
            this.cbOutFormat.TabIndex = 0;
            this.cbOutFormat.Text = "Choose the output format...";
            this.cbOutFormat.SelectedIndexChanged += new System.EventHandler(this.cbOutFormat_SelectedIndexChanged);
            // 
            // progBar
            // 
            this.progBar.Location = new System.Drawing.Point(16, 394);
            this.progBar.Name = "progBar";
            this.progBar.Size = new System.Drawing.Size(554, 22);
            this.progBar.TabIndex = 3;
            // 
            // btGo
            // 
            this.btGo.Location = new System.Drawing.Point(576, 377);
            this.btGo.Name = "btGo";
            this.btGo.Size = new System.Drawing.Size(75, 39);
            this.btGo.TabIndex = 4;
            this.btGo.Text = "Go!";
            this.btGo.UseVisualStyleBackColor = true;
            this.btGo.Click += new System.EventHandler(this.btGo_Click);
            // 
            // gbLog
            // 
            this.gbLog.Controls.Add(this.lbLog);
            this.gbLog.Location = new System.Drawing.Point(16, 422);
            this.gbLog.Name = "gbLog";
            this.gbLog.Size = new System.Drawing.Size(636, 183);
            this.gbLog.TabIndex = 5;
            this.gbLog.TabStop = false;
            this.gbLog.Text = "Log";
            // 
            // lbLog
            // 
            this.lbLog.FormattingEnabled = true;
            this.lbLog.Location = new System.Drawing.Point(6, 21);
            this.lbLog.Name = "lbLog";
            this.lbLog.Size = new System.Drawing.Size(624, 147);
            this.lbLog.TabIndex = 0;
            // 
            // labelFileProcessing
            // 
            this.labelFileProcessing.AutoSize = true;
            this.labelFileProcessing.Location = new System.Drawing.Point(19, 377);
            this.labelFileProcessing.Name = "labelFileProcessing";
            this.labelFileProcessing.Size = new System.Drawing.Size(52, 13);
            this.labelFileProcessing.TabIndex = 6;
            this.labelFileProcessing.Text = "File 0 / 0:";
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 500;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // RawConverterGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 610);
            this.Controls.Add(this.labelFileProcessing);
            this.Controls.Add(this.gbLog);
            this.Controls.Add(this.btGo);
            this.Controls.Add(this.progBar);
            this.Controls.Add(this.gbOutput);
            this.Controls.Add(this.gbFiles);
            this.Controls.Add(this.mainMenu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.mainMenu;
            this.Name = "RawConverterGUI";
            this.Text = "RawConverter";
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.gbFiles.ResumeLayout(false);
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.gbExpType.ResumeLayout(false);
            this.gbExpType.PerformLayout();
            this.gbOutput.ResumeLayout(false);
            this.gbOutput.PerformLayout();
            this.gbLog.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mzDecimalMenuItem;
        private System.Windows.Forms.GroupBox gbFiles;
        private System.Windows.Forms.GroupBox gbOutput;
        private System.Windows.Forms.ListBox lbInputFiles;
        private System.Windows.Forms.Button btDelFile;
        private System.Windows.Forms.Button btAddFile;
        private System.Windows.Forms.OpenFileDialog fileBrowserDialog;
        private System.Windows.Forms.FolderBrowserDialog outDirBrowserDialog;
        private System.Windows.Forms.GroupBox gbExpType;
        private System.Windows.Forms.RadioButton rbDIA;
        private System.Windows.Forms.RadioButton rbDDA;
        private System.Windows.Forms.CheckBox cbMs2;
        private System.Windows.Forms.Label labelOutFormat;
        private System.Windows.Forms.CheckBox cbMs3;
        private System.Windows.Forms.CheckBox cbMgf;
        private System.Windows.Forms.TextBox tbOutDir;
        private System.Windows.Forms.CheckBox cbLog;
        private System.Windows.Forms.Button btBrowse;
        private System.Windows.Forms.CheckBox cbMs1;
        private System.Windows.Forms.Label labelOutDir;
        private System.Windows.Forms.ComboBox cbOutFormat;
        private System.Windows.Forms.ProgressBar progBar;
        private System.Windows.Forms.Button btGo;
        private System.Windows.Forms.GroupBox gbLog;
        private System.Windows.Forms.ListBox lbLog;
        private System.Windows.Forms.Label labelFileProcessing;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.CheckBox cbMassCorrection;
        private System.Windows.Forms.CheckBox cbPredDiaPrecs;
        private System.Windows.Forms.CheckBox cbCentroid;
        private System.Windows.Forms.ToolStripMenuItem mzDecimal1MenuItem;
        private System.Windows.Forms.ToolStripMenuItem mzDecimal2MenuItem;
        private System.Windows.Forms.ToolStripMenuItem mzDecimal3MenuItem;
        private System.Windows.Forms.ToolStripMenuItem mzDecimal4MenuItem;
        private System.Windows.Forms.ToolStripMenuItem mzDecimal5MenuItem;
        private System.Windows.Forms.ToolStripMenuItem intensityDecimalMenuItem;
        private System.Windows.Forms.ToolStripMenuItem intensityDecimal0MenuItem;
        private System.Windows.Forms.ToolStripMenuItem intensityDecimal1MenuItem;
        private System.Windows.Forms.ToolStripMenuItem intensityDecimal2MenuItem;
        private System.Windows.Forms.ToolStripMenuItem intensityDecimal3MenuItem;
        private System.Windows.Forms.ToolStripMenuItem intensityDecimal4MenuItem;
        private System.Windows.Forms.ToolStripMenuItem intensityDecimal5MenuItem;
        private System.Windows.Forms.ToolStripMenuItem ddaMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeStateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState1MenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState2MenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState3MenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState4MenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState5MenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState6MenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState7MenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState8MenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState9MenuItem;
        private System.Windows.Forms.ToolStripMenuItem chargeState10MenuItem;
        private System.Windows.Forms.ToolStripMenuItem dIAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem singleSpectrumAnalysisToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator sep1MenuItem;
        private System.Windows.Forms.ToolStripMenuItem singleSpectrumAnalysisToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem extractPrecursorByMzMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem msnOptioinMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showPeakChargeStatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showPeakResolutionToolStripMenuItem;
        private System.Windows.Forms.CheckBox cbMzXML;
        private System.Windows.Forms.ToolStripMenuItem exportChargeStateToolStripMenuItem;
        private System.Windows.Forms.CheckBox cbMzML;
        private System.Windows.Forms.ToolStripMenuItem bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}