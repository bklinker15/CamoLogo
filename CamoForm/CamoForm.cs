using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;

namespace CamoForm {


    public partial class CamoForm : Form {
        enum _generationType { random, smartRandom, spiral };

        private int _imagesToPaste = 10000;
        int _camoHeight = 8000;
        int _camoWidth = 8000;
       

        List<string> fileNames = new List<string>();
        Generator gen = new Generator();
        Bitmap camo;

        public CamoForm() {
            InitializeComponent();
            panel0.Visible = true;
        }

        //1st page next button
        private void NextButton1_Click(object sender, EventArgs e) {
            panel0.Visible = false;
            panel2.Visible = true;
        }

        //1st page exit button
        private void ExitButton_Click(object sender, EventArgs e) {
            Application.Exit();
        }


        //select files
        private void button2_Click(object sender, EventArgs e) {
            List<string> newFiles = gen.GetFileNames();
            foreach(var file in newFiles) {
                fileNames.Add(file);
            }
            
            for (int i = fileNames.Count - newFiles.Count; i < fileNames.Count; i++) {
                dataGridView1.Rows.AddCopy(0);
                dataGridView1[0, i].Value = true;
                dataGridView1[0, i].ReadOnly = false;
                dataGridView1[1, i].Value = gen.TrimFileDirectory(fileNames[i]);
                DataGridViewComboBoxCell cmb = (DataGridViewComboBoxCell)dataGridView1[2, i];
                cmb.Value = cmb.Items[0];
                cmb.ReadOnly = false;
                cmb = (DataGridViewComboBoxCell)dataGridView1[3, i];
                cmb.ReadOnly = false;
                cmb.Value = cmb.Items[1];

            }

            if(fileNames.Count > 0) {
                button3.Enabled = true;
                button6.Enabled = true;
            }
            else {
                button3.Enabled = false;
                button6.Enabled = false;
            }
        }

        //clear files button
        private void button3_Click(object sender, EventArgs e) {
            fileNames.Clear();
            dataGridView1.Rows.Clear();
            //dataGridView1.Rows.Add(dgvr);
            button6.Enabled = false;
            button3.Enabled = false;
        }

        //2nd page next button
        private void button4_Click(object sender, EventArgs e) {
            //panel2.Visible = false;
            //panel3.Visible = true;
        }

        //2nd page previous button
        private void button5_Click(object sender, EventArgs e) {
            panel0.Visible = true;
            panel2.Visible = false;
            button7_Click(sender, e);

        }

        //Generate Camo Button
        private void button6_Click(object sender, EventArgs e) {

            if (!bw.IsBusy) {
                bw.RunWorkerAsync();
            }
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            button8.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            dataGridView1.ReadOnly = true;
            


        }

        private void bw_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            GC.Collect();
            Bitmap[] imgs = gen.LoadImages(fileNames);
            camo = new Bitmap(8000, 8000);
            Random rand = new Random();

            for (int i = 0; (i < _imagesToPaste / imgs.Length); i++) {
                if ((worker.CancellationPending == true)) {
                    e.Cancel = true;
                    worker.ReportProgress(0);
                    break;
                }
                else {
                    // Perform a time consuming operation and report progress.
                    for (int j = 0; j < imgs.Length; j++) {
                        camo = gen.Superimpose(camo, imgs[j], rand);
                    }
                    worker.ReportProgress((i + 1) * 100 / (_imagesToPaste / imgs.Length));
                }
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            progressBar1.Value = e.ProgressPercentage;
        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if ((e.Cancelled == true)) {
                Console.WriteLine("Camo generation canceled");
                pictureBox2.Image = null;

            }

            else if (!(e.Error == null)) {
                Console.WriteLine("Encountered error: " + e.Error);
                pictureBox2.Image = null;
            }

            else {
                pictureBox2.Image = new Bitmap(camo, new Size(pictureBox2.Width, pictureBox2.Height));
                button8.Enabled = true;
            }
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            textBox4.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;
            dataGridView1.ReadOnly = false;
        }

        //Cancel button
        private void button7_Click(object sender, EventArgs e) {
            bw.CancelAsync();
        }

        //Save File button
        private void button8_Click(object sender, EventArgs e) {
            saveFileDialog1.DefaultExt = ".jpg";
            saveFileDialog1.Filter = "JPeg Image|*.jpg";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "") {
                FileStream fs = (FileStream)saveFileDialog1.OpenFile();
                camo.Save(fs, ImageFormat.Jpeg);
                fs.Close();
            }            
        }
        //Iterations box
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void textBox2_MouseDown(object sender, MouseEventArgs e) {
            textBox2.Text = "";
        }

        private void textBox2_Leave(object sender, EventArgs e) {
            if (textBox2.Text == "") {
                textBox2.Text = "0";
            }
            _imagesToPaste = Int32.Parse(textBox2.Text);
        }

        //height box
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void textBox3_MouseDown(object sender, MouseEventArgs e) {
            textBox3.Text = "";
        }

        private void textBox3_Leave(object sender, EventArgs e) {
            if (textBox3.Text == "") {
                textBox3.Text = "0";
            }
            _camoHeight = Int32.Parse(textBox3.Text);
        }

        //length box
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void textBox4_MouseDown(object sender, MouseEventArgs e) {
            textBox4.Text = "";
        }

        private void textBox4_Leave(object sender, EventArgs e) {
            if (textBox4.Text == "") {
                textBox4.Text = "0";
            }
            _camoWidth = Int32.Parse(textBox4.Text);
        }
    }
}
