using RawConverter.Converter;
using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace RawConverter.GUI
{
    public partial class RawConverterGUI : Form
    {
        private List<string> _inFileList = null;
        private Thread _mainThread;
        private RawConverter _rawXtract = null;
        private bool _aborted = false;
        private string _currentInputFormat;

        public string CurrentInputFormat
        {
            get
            {
                return _currentInputFormat;
            }

            set
            {
                _currentInputFormat = value;
                OnCurrentInputFormatChanged();
            }
        }

        protected virtual void OnCurrentInputFormatChanged()
        {
            cbOutFormat.SelectedIndex = -1;
            cbMs1.Enabled = false;
            cbMs2.Enabled = false;
            cbMs3.Enabled = false;
            cbMgf.Enabled = false;
            cbMzXML.Enabled = false;
            cbMzML.Enabled = false;
            cbLog.Enabled = false;

            cbMs1.Checked = false;
            cbMs2.Checked = false;
            cbMs3.Checked = false;
            cbMgf.Checked = false;
            cbMzXML.Checked = false;
            cbMzML.Checked = false;
            cbLog.Checked = false;
        }
 
        public RawConverterGUI()
        {
            // get an instance of RawXtract;
            _rawXtract = new RawConverter();

            // initialize components;
            InitializeComponent();

            rbDDA.Checked = true;
            cbMassCorrection.Enabled = true;
            cbMassCorrection.Checked = false;
            cbPredDiaPrecs.Enabled = false;
            cbPredDiaPrecs.Checked = false;
            cbCentroid.Enabled = false;
            cbCentroid.Checked = true;

            cbOutFormat.SelectedIndex = -1;

            fileBrowserDialog.Filter = "Thermo RAW Files|*.raw|MzXML Files|*.mzXML|MzML Files|*.mzml|MS2 Files|*.ms2|MS3 Files|*.ms3|MGF Files|*.mgf";
        }

        private void btAddFile_Click(object sender, EventArgs e)
        {
            if (_inFileList == null) 
            {
                _inFileList = new List<string>();
            }

            if (fileBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string[] addedFiles = fileBrowserDialog.FileNames;
                foreach (string file in addedFiles)
                {
                    if (!_inFileList.Contains(file))
                    {
                        lbInputFiles.Items.Add(file);
                        _inFileList.Add(file);
                    }                    
                }

                // show the default output directory;
                if (tbOutDir.Text == "")
                {
                    tbOutDir.Text = Path.GetDirectoryName(addedFiles[0]);
                }
            }

            // check the selected files formats;
            if (_inFileList.Count > 0)
            {
                if (!string.Equals(CurrentInputFormat, Path.GetExtension(_inFileList[0]), StringComparison.OrdinalIgnoreCase))
                {
                    CurrentInputFormat = Path.GetExtension(_inFileList[0]);
                }
                foreach (string file in _inFileList)
                {
                    if (!string.Equals(CurrentInputFormat, Path.GetExtension(file), StringComparison.OrdinalIgnoreCase)) 
                    {
                        MessageBox.Show("Different file formats? Please do NOT!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void btDelFile_Click(object sender, EventArgs e)
        {
            if (lbInputFiles.SelectedItem == null)
            {
                return;
            }
            string curItem = lbInputFiles.SelectedItem.ToString();

            // find the selected item in inFileList;
            if (!_inFileList.Remove(curItem))
            {
                MessageBox.Show("Selected item is not removed!");
            }

            // deleted the selected item from lbInputFiles; if the index is bigger than zero, 
            // make the previous be selected after the deletion;
            int idx = lbInputFiles.FindString(curItem);
            lbInputFiles.Items.RemoveAt(idx);
            if (idx > 0) 
            {
                lbInputFiles.SelectedIndex = idx - 1;
            } 
            else // otherwise, make the following one be selected;
            {               
                if (lbInputFiles.Items.Count > 0)
                {
                    lbInputFiles.SelectedIndex = idx;
                }
            }

            if (lbInputFiles.Items.Count > 0)
            {
                CurrentInputFormat = Path.GetExtension(_inFileList[0]);
            }
            else if (lbInputFiles.Items.Count == 0)
            {
                CurrentInputFormat = null;
            }
        }

        private void btBrowse_Click(object sender, EventArgs e)
        {
            if (outDirBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tbOutDir.Text = outDirBrowserDialog.SelectedPath;
            }
        }

        private void cbOutFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = cbOutFormat.SelectedIndex;
            if (selectedIndex == 0) // "MS1, MS2, and MS3" is selected; 
            {
                if (CurrentInputFormat != null && (string.Equals(CurrentInputFormat, ".MS1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(CurrentInputFormat, ".MS2", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(CurrentInputFormat, ".MS3", StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("The input and the output formats cannot be the same!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = false;

                    cbMs1.Checked = false;
                    cbMs2.Checked = false;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
                else if (CurrentInputFormat != null && string.Equals(CurrentInputFormat, ".MGF", StringComparison.OrdinalIgnoreCase))
                {
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = true;
                    cbMs3.Enabled = true;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = false;

                    cbMs1.Checked = false;
                    cbMs2.Checked = true;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
                else if (CurrentInputFormat != null && string.Equals(CurrentInputFormat, ".mzXML", StringComparison.OrdinalIgnoreCase))
                {
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = true;
                    cbMs3.Enabled = true;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = false;

                    cbMs1.Checked = false;
                    cbMs2.Checked = true;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
                else
                {
                    cbMs1.Enabled = true;
                    cbMs2.Enabled = true;
                    cbMs3.Enabled = true;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = true;

                    cbMs1.Checked = false;
                    cbMs2.Checked = true;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
            }
            else if (selectedIndex == 1) // "MGF" is selected;
            {
                if (CurrentInputFormat != null && string.Equals(CurrentInputFormat, ".MGF", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The input and the output formats cannot be the same!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = false;

                    cbMs1.Checked = false;
                    cbMs2.Checked = false;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
                else if (CurrentInputFormat != null && (string.Equals(CurrentInputFormat, ".MS1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(CurrentInputFormat, ".MS2", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(CurrentInputFormat, ".MS3", StringComparison.OrdinalIgnoreCase))) 
                {
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = false;

                    cbMs1.Checked = false;
                    cbMs2.Checked = false;
                    cbMs3.Checked = false;
                    cbMgf.Checked = true;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
                else
                {
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = true;

                    cbMs1.Checked = false;
                    cbMs2.Checked = false;
                    cbMs3.Checked = false;
                    cbMgf.Checked = true;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
            }
            else if (selectedIndex == 2) // "mzXML" is selected;
            {
                if (CurrentInputFormat != null && !string.Equals(CurrentInputFormat, ".RAW", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The input can only be RAW files if mzXML is selected", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = false;

                    cbMs1.Checked = false;
                    cbMs2.Checked = false;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = true;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
                else
                {
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = true;

                    cbMs1.Checked = false;
                    cbMs2.Checked = false;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = true;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
            }
            else if (selectedIndex == 3) // "MzML" is selected;
            {
                if (CurrentInputFormat != null && !string.Equals(CurrentInputFormat, ".RAW", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The input can only be RAW files if mzML is selected", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = false;

                    cbMs1.Checked = false;
                    cbMs2.Checked = false;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = true;
                    cbLog.Checked = false;
                }
                else
                {
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbMzML.Enabled = false;
                    cbLog.Enabled = true;

                    cbMs1.Checked = false;
                    cbMs2.Checked = false;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = true;
                    cbLog.Checked = false;
                }
            }
            else if (selectedIndex == 4) // "All" is selected;
            {
                if (CurrentInputFormat != null && (!string.Equals(CurrentInputFormat, ".RAW", StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("This option is only availabe for RAW data extraction!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbMzXML.Enabled = false;
                    cbLog.Enabled = false;

                    cbMs1.Checked = false;
                    cbMs2.Checked = false;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = false;
                    cbLog.Checked = false;
                }
                else
                {
                    cbMs1.Enabled = true;
                    cbMs2.Enabled = true;
                    cbMs3.Enabled = true;
                    cbMgf.Enabled = true;
                    cbMzXML.Enabled = true;
                    cbMzML.Enabled = true;
                    cbLog.Enabled = true;

                    cbMs1.Checked = false;
                    cbMs2.Checked = true;
                    cbMs3.Checked = false;
                    cbMgf.Checked = false;
                    cbMzXML.Checked = false;
                    cbMzML.Checked = false;
                    cbLog.Checked = false;
                }
            }
        }

        private void btGo_Click(object sender, EventArgs e)
        {

            if (btGo.Text.Equals("Go!"))
            {
                _aborted = false;
                // reset the prgress bar and the log list;
                progBar.Value = progBar.Minimum;
                lbLog.Items.Clear();

                if (_inFileList == null || _inFileList.Count == 0)
                {
                    MessageBox.Show("No input file! Please select input file(s).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!cbMs1.Checked && !cbMs2.Checked && !cbMs3.Checked && !cbMgf.Checked && !cbMzXML.Checked && !cbMzML.Checked && !cbLog.Checked)
                {
                    MessageBox.Show("Please select an output format!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (tbOutDir.Text == "")
                {
                    MessageBox.Show("Please select a destination directory!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (!Directory.Exists(tbOutDir.Text))
                {
                    Directory.CreateDirectory(tbOutDir.Text);
                }

                _rawXtract.OutFileFolder = tbOutDir.Text;

                if (rbDDA.Checked)
                {
                    _rawXtract.ExpType = ExperimentType.DDA;
                }
                else if (rbDIA.Checked)
                {
                    _rawXtract.ExpType = ExperimentType.DIA;
                }

                if (cbMassCorrection.Checked)
                {
                    _rawXtract.correctPrecMz = true;
                }
                else
                {
                    _rawXtract.correctPrecMz = false;
                }

                if (cbCentroid.Checked)
                {
                    _rawXtract.isCentroided = true;
                }
                else
                {
                    _rawXtract.isCentroided = false;
                }

                if (cbPredDiaPrecs.Checked)
                {
                    _rawXtract.predictPrecursors = true;
                }
                else
                {
                    _rawXtract.predictPrecursors = false;
                }

                if (extractPrecursorByMzMenuItem.Checked)
                {
                    _rawXtract.ExtractPrecursorByMz = true;
                }
                else
                {
                    _rawXtract.ExtractPrecursorByMz = false;
                }

                if (bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.Checked)
                {
                    _rawXtract.ByPassThermoAlgorithm = true;
                }
                else
                {
                    _rawXtract.ByPassThermoAlgorithm = false;
                }

                // possible charge states;
                List<int> precZList = new List<int>();
                if (chargeState1MenuItem.Checked)
                {
                    precZList.Add(1);
                }
                if (chargeState2MenuItem.Checked)
                {
                    precZList.Add(2);
                }
                if (chargeState3MenuItem.Checked)
                {
                    precZList.Add(3);
                }
                if (chargeState4MenuItem.Checked)
                {
                    precZList.Add(4);
                }
                if (chargeState5MenuItem.Checked)
                {
                    precZList.Add(5);
                }
                if (chargeState6MenuItem.Checked)
                {
                    precZList.Add(6);
                }
                if (chargeState7MenuItem.Checked)
                {
                    precZList.Add(7);
                }
                if (chargeState8MenuItem.Checked)
                {
                    precZList.Add(8);
                }
                if (chargeState9MenuItem.Checked)
                {
                    precZList.Add(9);
                }
                if (chargeState10MenuItem.Checked)
                {
                    precZList.Add(10);
                }
                // if no charge state is selected, check charge state 2 and 3;
                if (precZList.Count() == 0)
                {
                    chargeState2MenuItem.Checked = true;
                    chargeState3MenuItem.Checked = true;
                    precZList.Add(2);
                    precZList.Add(3);
                }
                _rawXtract.Ms2PrecZ = precZList.ToArray();

                _rawXtract.inputFiles = _inFileList;

                if (_mainThread == null)
                {
                    _rawXtract.ms1Converter = cbMs1.Checked;
                    _rawXtract.ms2Converter = cbMs2.Checked;
                    _rawXtract.ms3Converter = cbMs3.Checked;
                    _rawXtract.mgfConverter = cbMgf.Checked;
                    _rawXtract.mzXMLConverter = cbMzXML.Checked;
                    _rawXtract.mzMLConverter = cbMzML.Checked;
                    _rawXtract.logConverter = cbLog.Checked;

                    _mainThread = new Thread(new ThreadStart(_rawXtract.ConvertFiles));

                    btAddFile.Enabled = false;
                    btDelFile.Enabled = false;
                    btBrowse.Enabled = false;
                    cbMs1.Enabled = false;
                    cbMs2.Enabled = false;
                    cbMs3.Enabled = false;
                    cbMgf.Enabled = false;
                    cbLog.Enabled = false;

                    _mainThread.Start();
                }
                btGo.Text = "Stop!";
            }
            else
            {
                DialogResult answer = MessageBox.Show("Stop the converting?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (answer == DialogResult.Yes)
                {
                    _rawXtract.ExtractProgress.Aborted = true;
                    _aborted = true;
                    btAddFile.Enabled = true;
                    btDelFile.Enabled = true;
                    btBrowse.Enabled = true;
                    cbMs1.Enabled = true;
                    cbMs2.Enabled = true;
                    cbMs3.Enabled = true;
                    cbMgf.Enabled = true;
                    cbMzXML.Enabled = true;
                    cbMzML.Enabled = true;
                    cbLog.Enabled = true;
                    btGo.Text = "Go!";
                    lbLog.Items.Clear();
                    labelFileProcessing.Text = "File 0 / 0";
                    Thread.Sleep(1000);
                    _mainThread.Abort();
                    _mainThread = null;
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (_rawXtract == null)
            {
                return;
            }

            // update the log list and the progress bar;
            lock (_rawXtract.LogList)
            {
                foreach (string line in _rawXtract.LogList)
                {
                    lbLog.Items.Add(line);
                }
                _rawXtract.LogList.Clear();
            }
            if (_aborted)
            {
                labelFileProcessing.Text = "File 0 / 0";
                progBar.Value = 0;
            }
            else
            {
                if (!labelFileProcessing.Text.Equals(_rawXtract.CurrentFileLabel)) 
                {
                    labelFileProcessing.Text = _rawXtract.CurrentFileLabel;
                }
                progBar.Value = _rawXtract.ExtractProgress.CurrentProgress;
            }

            // check whether the converting is finished;
            int termCode = _rawXtract.terminateCode;
            if (termCode <= 0)
            {
                _rawXtract.resetTerminateCode();
                if (termCode == -1)
                {
                    MessageBox.Show("Finished converting unsuccessfully!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (termCode == 0)
                {
                    MessageBox.Show("Finished converting successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                _mainThread = null;
                btAddFile.Enabled = true;
                btDelFile.Enabled = true;
                btBrowse.Enabled = true;
                cbMs1.Enabled = true;
                cbMs2.Enabled = true;
                cbMs3.Enabled = true;
                cbMgf.Enabled = true;
                cbLog.Enabled = true;
                btGo.Text = "Go!";
            }
        }

        private void rbDDA_CheckedChanged(object sender, EventArgs e)
        {
            cbMassCorrection.Enabled = true;
            cbMassCorrection.Checked = true;
            cbPredDiaPrecs.Enabled = false;
            cbPredDiaPrecs.Checked = false;
            cbCentroid.Enabled = true;
            cbCentroid.Checked = true;
        }

        private void rbDIA_CheckedChanged(object sender, EventArgs e)
        {
            cbMassCorrection.Enabled = false;
            cbMassCorrection.Checked = false;
            cbPredDiaPrecs.Enabled = true;
            cbPredDiaPrecs.Checked = false;
            cbCentroid.Enabled = false;
            cbCentroid.Checked = true;
        }

        private void singleSpectrumAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new DIAPrecursorPredictorGUI().Show();
        }

        private void uncheckAllMzDecimal1MenuItems()
        {
            mzDecimal1MenuItem.Checked = false;
            mzDecimal2MenuItem.Checked = false;
            mzDecimal3MenuItem.Checked = false;
            mzDecimal4MenuItem.Checked = false;
            mzDecimal5MenuItem.Checked = false;
        }

        private void mzDecimal1MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllMzDecimal1MenuItems();
            if (mzDecimal1MenuItem.Checked)
            {
                mzDecimal1MenuItem.Checked = false;
            }
            else
            {
                mzDecimal1MenuItem.Checked = true;
                _rawXtract.MzDecimalPlace = 1;
            }
            optionsToolStripMenuItem.ShowDropDown();
            mzDecimalMenuItem.ShowDropDown();
        }

        private void mzDecimal2MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllMzDecimal1MenuItems();
            if (mzDecimal2MenuItem.Checked)
            {
                mzDecimal2MenuItem.Checked = false;
            }
            else
            {
                mzDecimal2MenuItem.Checked = true;
                _rawXtract.MzDecimalPlace = 2;
            }
            optionsToolStripMenuItem.ShowDropDown();
            mzDecimalMenuItem.ShowDropDown();
        }

        private void mzDecimal3MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllMzDecimal1MenuItems();
            if (mzDecimal3MenuItem.Checked)
            {
                mzDecimal3MenuItem.Checked = false;
            }
            else
            {
                mzDecimal3MenuItem.Checked = true;
                _rawXtract.MzDecimalPlace = 3;
            }
            optionsToolStripMenuItem.ShowDropDown();
            mzDecimalMenuItem.ShowDropDown();
        }

        private void mzDecimal4MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllMzDecimal1MenuItems();
            if (mzDecimal4MenuItem.Checked)
            {
                mzDecimal4MenuItem.Checked = false;
            }
            else
            {
                mzDecimal4MenuItem.Checked = true;
                _rawXtract.MzDecimalPlace = 4;
            }
            optionsToolStripMenuItem.ShowDropDown();
            mzDecimalMenuItem.ShowDropDown();
        }

        private void mzDecimal5MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllMzDecimal1MenuItems();
            if (mzDecimal5MenuItem.Checked)
            {
                mzDecimal5MenuItem.Checked = false;
            }
            else
            {
                mzDecimal5MenuItem.Checked = true;
                _rawXtract.MzDecimalPlace = 5;
            }
            optionsToolStripMenuItem.ShowDropDown();
            mzDecimalMenuItem.ShowDropDown();
        }

        private void uncheckAllIntensityDecimalMenuItems()
        {
            intensityDecimal0MenuItem.Checked = false;
            intensityDecimal1MenuItem.Checked = false;
            intensityDecimal2MenuItem.Checked = false;
            intensityDecimal3MenuItem.Checked = false;
            intensityDecimal4MenuItem.Checked = false;
            intensityDecimal5MenuItem.Checked = false;
        }

        private void intensityDecimal0MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllIntensityDecimalMenuItems();
            if (intensityDecimal0MenuItem.Checked)
            {
                intensityDecimal0MenuItem.Checked = false;
            }
            else
            {
                intensityDecimal0MenuItem.Checked = true;
                _rawXtract.IntensityDecimalPlace = 0;
            }
            optionsToolStripMenuItem.ShowDropDown();
            intensityDecimalMenuItem.ShowDropDown();
        }

        private void intensityDecimal1MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllIntensityDecimalMenuItems();
            if (intensityDecimal1MenuItem.Checked)
            {
                intensityDecimal1MenuItem.Checked = false;
            }
            else
            {
                intensityDecimal1MenuItem.Checked = true;
                _rawXtract.IntensityDecimalPlace = 1;
            }
            optionsToolStripMenuItem.ShowDropDown();
            intensityDecimalMenuItem.ShowDropDown();
        }

        private void intensityDecimal2MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllIntensityDecimalMenuItems();
            if (intensityDecimal2MenuItem.Checked)
            {
                intensityDecimal2MenuItem.Checked = false;
            }
            else
            {
                intensityDecimal2MenuItem.Checked = true;
                _rawXtract.IntensityDecimalPlace = 2;
            }
            optionsToolStripMenuItem.ShowDropDown();
            intensityDecimalMenuItem.ShowDropDown();
        }

        private void intensityDecimal3MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllIntensityDecimalMenuItems();
            if (intensityDecimal3MenuItem.Checked)
            {
                intensityDecimal3MenuItem.Checked = false;
            }
            else
            {
                intensityDecimal3MenuItem.Checked = true;
                _rawXtract.IntensityDecimalPlace = 3;
            }
            optionsToolStripMenuItem.ShowDropDown();
            intensityDecimalMenuItem.ShowDropDown();
        }

        private void intensityDecimal4MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllIntensityDecimalMenuItems();
            if (intensityDecimal4MenuItem.Checked)
            {
                intensityDecimal4MenuItem.Checked = false;
            }
            else
            {
                intensityDecimal4MenuItem.Checked = true;
                _rawXtract.IntensityDecimalPlace = 4;
            }
            optionsToolStripMenuItem.ShowDropDown();
            intensityDecimalMenuItem.ShowDropDown();
        }

        private void intensityDecimal5MenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllIntensityDecimalMenuItems();
            if (intensityDecimal5MenuItem.Checked)
            {
                intensityDecimal5MenuItem.Checked = false;
            }
            else
            {
                intensityDecimal5MenuItem.Checked = true;
                _rawXtract.IntensityDecimalPlace = 5;
            }
            optionsToolStripMenuItem.ShowDropDown();
            intensityDecimalMenuItem.ShowDropDown();
        }

        private void chargeState1MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState1MenuItem.Checked)
            {
                chargeState1MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(1);
            }
            else
            {
                chargeState1MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(1);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void chargeState2MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState2MenuItem.Checked)
            {
                chargeState2MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(2);
            }
            else
            {
                chargeState2MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(2);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void chargeState3MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState3MenuItem.Checked)
            {
                chargeState3MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(3);
            }
            else
            {
                chargeState3MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(3);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void chargeState4MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState4MenuItem.Checked)
            {
                chargeState4MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(4);
            }
            else
            {
                chargeState4MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(4);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void chargeState5MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState5MenuItem.Checked)
            {
                chargeState5MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(5);
            }
            else
            {
                chargeState5MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(5);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void chargeState6MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState6MenuItem.Checked)
            {
                chargeState6MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(6);
            }
            else
            {
                chargeState6MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(6);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void chargeState7MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState7MenuItem.Checked)
            {
                chargeState7MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(7);
            }
            else
            {
                chargeState7MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(7);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void chargeState8MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState8MenuItem.Checked)
            {
                chargeState8MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(8);
            }
            else
            {
                chargeState8MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(8);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void chargeState9MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState9MenuItem.Checked)
            {
                chargeState9MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(9);
            }
            else
            {
                chargeState9MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(9);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void chargeState10MenuItem_Click(object sender, EventArgs e)
        {
            if (chargeState10MenuItem.Checked)
            {
                chargeState10MenuItem.Checked = false;
                _rawXtract.DDADataChargeStates.Remove(10);
            }
            else
            {
                chargeState10MenuItem.Checked = true;
                _rawXtract.DDADataChargeStates.Add(10);
            }
            optionsToolStripMenuItem.ShowDropDown();
            ddaMenuItem.ShowDropDown();
            chargeStateToolStripMenuItem.ShowDropDown();
        }

        private void singleSpectrumAnalysisToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new DDAPrecursorCorrectorGUI().Show();
        }

        private void extractPrecursorByMzToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (extractPrecursorByMzMenuItem.Checked)
            {
                extractPrecursorByMzMenuItem.Checked = false;
            }
            else
            {
                extractPrecursorByMzMenuItem.Checked = true;
            }
        }
        
        private void showPeakChargeStatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showPeakChargeStatesToolStripMenuItem.Checked)
            {
                showPeakChargeStatesToolStripMenuItem.Checked = false;
                _rawXtract.showPeakChargeStates = true;
            }
            else
            {
                showPeakChargeStatesToolStripMenuItem.Checked = true;
                _rawXtract.showPeakChargeStates = false;
            }
            optionsToolStripMenuItem.ShowDropDown();
            msnOptioinMenuItem.ShowDropDown();
        }

        private void showPeakResolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showPeakResolutionToolStripMenuItem.Checked)
            {
                showPeakResolutionToolStripMenuItem.Checked = false;
                _rawXtract.showPeakResolution = false;
            }
            else
            {
                showPeakResolutionToolStripMenuItem.Checked = true;
                _rawXtract.showPeakResolution = true;
            }
            optionsToolStripMenuItem.ShowDropDown();
            msnOptioinMenuItem.ShowDropDown();
        }

        private void exportChargeStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(exportChargeStateToolStripMenuItem.Checked)
            {
                exportChargeStateToolStripMenuItem.Checked = false;
                _rawXtract.exportChargeState = false;
            }
            else
            {
                exportChargeStateToolStripMenuItem.Checked = true;
                _rawXtract.exportChargeState = true;
            }
            optionsToolStripMenuItem.ShowDropDown();
            msnOptioinMenuItem.ShowDropDown();
        }

        private void bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.Checked)
            {
                bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.Checked = false;
                _rawXtract.ByPassThermoAlgorithm = false;
            }
            else
            {
                bypassThermoMonoisotopeSelectionAlgorithmToolStripMenuItem.Checked = true;
                _rawXtract.ByPassThermoAlgorithm = true;
            }
            optionsToolStripMenuItem.ShowDropDown();
            msnOptioinMenuItem.ShowDropDown();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MzXMLFileWriter mxw = new MzXMLFileWriter("D:/Test.mzXML");
            //mxw.WriteHeader(100, 233, 334.3, "rawFile", "Thermo", "model", "CID", "Ion trap", "IonDetec", "softtype", "softare", "2343.33", "23434.332", 1, "1.0.0");
            //long pos = mxw.WriteScan();
            //mxw.WriteEnd(pos);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.Show();
        }
    }
}
