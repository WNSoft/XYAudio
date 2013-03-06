using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Threading;
using System.Diagnostics;

namespace XYAudio
{
    public partial class MainWindow : Form
    {
        Bitmap waveformBMP;
        SoundFile sf = null;
        double duration;
        Graphics g;
        SoundPlayer snd;
        Stopwatch stopwatch;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "WAV Files (*.wav)|*.wav";
            dialog.InitialDirectory = @"C:\";
            dialog.Title = "Please select an audio file.";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                openFile(dialog.FileName);
            }      
        }

        private void openFile(String fn)
        {
            try
            {
                sf = new SoundFile(fn);
                drawWaveform(pictureBox1, sf, 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void drawWaveform(PictureBox pb, SoundFile sf, int channel)
        {
            Point[] points = sf.getWaveformPoints(pb.Width, pb.Height, 1);
            waveformBMP = new Bitmap(pb.Size.Width, pb.Size.Height);
            pb.Image = waveformBMP;
            g = Graphics.FromImage(waveformBMP);
            Pen p = new Pen(Color.Black);
            for (int i = 0; i < points.Length - 1; i++)
            {
                g.DrawLine(p, points[i], points[i + 1]);
            }
            pb.Image = waveformBMP;
            g.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (sf != null)
            {
                Stream str = new FileStream(sf.getFilename(), FileMode.Open, FileAccess.Read);
                snd = new SoundPlayer(str);
                duration = sf.getDuration();
                Thread markerThread = new Thread(new ThreadStart(this.drawMarker));
                snd.Load();
                markerThread.Start();
                snd.Play();
                str.Close();
            }
        }

        private void drawMarker()
        {

            stopwatch = new Stopwatch();
            stopwatch.Start();
            int s = 0;
            while (stopwatch.ElapsedMilliseconds < (duration * 1000))
            {
                if (stopwatch.ElapsedMilliseconds / 50 > s)
                {
                    s++;
                    Bitmap overlayWaveform = new Bitmap(waveformBMP);
                    double percentDone = (double)stopwatch.ElapsedMilliseconds / (duration*1000);
                    g = Graphics.FromImage(overlayWaveform);
                    Pen p = new Pen(Color.Red);
                    g.DrawLine(p, new Point((int)(percentDone * pictureBox1.Width), 0), new Point((int)(percentDone * pictureBox1.Width), pictureBox1.Height));
                    pictureBox1.Image = overlayWaveform;
                    g.Dispose();

                    Point[] spectrum = sf.getSpectrumPoints(pictureBox2.Width, pictureBox2.Height, (double)stopwatch.ElapsedMilliseconds / 1000, 1);
                    if (spectrum != null)
                    {
                        p = new Pen(Color.Black);
                        p.Width = 4.0F;
                        Bitmap spect = new Bitmap(pictureBox2.Width, pictureBox2.Height);
                        g = Graphics.FromImage(spect);
                        for (int i = 0; i < spectrum.Length; i++)
                        {
                            g.DrawLine(p, new Point(spectrum[i].X, pictureBox2.Height), spectrum[i]);
                        }
                        pictureBox2.Image = spect;
                        g.Dispose();
                    }
                }
            }
            stopwatch.Stop();
            clearOverlays();
        }

        private void clearOverlays()
        {
            pictureBox1.Image = waveformBMP;
            pictureBox2.Image = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            snd.Stop();
            snd.Dispose();
            duration = -1;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
