using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

namespace XYAudio
{
    class SoundData
    {
        //number of channels in wav file
        private int channels;
        //number of samples in each wav file channel
        private int numSamples;
        //bit depth of wav file
        private int bitDepth;
        //list to store arrays of data for each channel
        private ArrayList channelData = new ArrayList();
        //sample rate of wav file
        private int sampleRate;

        //constructor
        public SoundData(Byte[] fileData, int channels, int bitsPerSample, int sampleRate, int dataSize, int soundDataIndex)
        {
            this.channels = channels;
            this.bitDepth = bitsPerSample;
            this.sampleRate = sampleRate;
            extractChannelData(fileData, bitsPerSample, dataSize, soundDataIndex);
        }

        //extract data for each channel in wav file
        private void extractChannelData(Byte[] fileData, int bitsPerSample, int dataSize, int soundDataIndex)
        {
            //calculate number of samples for each channel
            numSamples = dataSize / (channels * (bitsPerSample / 8));
            //get samples for each channel
            for (int i = 0; i < channels; i++)
            {
                //start index for current channel
                int sampleIndex = soundDataIndex + i*(bitsPerSample / 8);
                //hold samples for current channel
                int[] samples = new int[numSamples];
                //extract samples for each channel
                int curSample = 0;
                while (curSample < numSamples)
                {
                    switch (bitsPerSample)
                    {
                        case 8:
                            samples[curSample] = (int)fileData[sampleIndex];
                            sampleIndex += channels;
                            break;
                        case 16:
                            samples[curSample] = (int)BitConverter.ToInt16(fileData, sampleIndex);
                            sampleIndex += channels*2;
                            break;
                        case 32:
                            samples[curSample] = (int)BitConverter.ToInt32(fileData, sampleIndex);
                            sampleIndex += channels*4;
                            break;
                    }
                    curSample++;
                }
                //add channel data to list
                channelData.Add(samples);
            }
        }

        //get data for specified channel
        public int[] getChannelData(int channel)
        {
            return (int[])channelData[channel - 1];
        }

        //generate waveform points based on a specified width and height for a channel
        public Point[] getWaveformPoints(int w, int h, int channel)
        {
            int[] audioData = (int[]) channelData[channel - 1];
            Point[] waveformPoints = new Point[numSamples];
            double interval = (double) w / numSamples;
            int min = -1 * (int)(Math.Pow(2, bitDepth) / 2);
            int max = (int)(Math.Pow(2, bitDepth) / 2) - 1;
            int yMid = h / 2;
            for (int i = 0; i < numSamples; i++)
            {
                if (audioData[i] < 0)
                {
                    double displacePercent = (double) audioData[i] / min;
                    waveformPoints[i] = new Point((int)(interval * i), yMid - (int) (displacePercent * yMid));
                }
                else
                {
                    double displacePercent = (double)audioData[i] / max;
                    waveformPoints[i] = new Point((int)(interval * i), yMid + (int) (displacePercent * yMid));
                }
            }
            return waveformPoints;
        }

        public Point[] getSpectrumPoints(int w, int h, double time, int channel)
        {
            int curByte = (int) (time * sampleRate * (bitDepth / 8));
            curByte = curByte / channels;
            int[] channelIntData = (int[])channelData[channel - 1];
            ArrayList spectData = new ArrayList();
            if (!(curByte < 32 || curByte > channelIntData.Length - 32))
            {
                for (int i = -32; i < 32; i++)
                {
                    spectData.Add(new Complex(channelIntData[curByte + i], 0));
                }
                FFT(spectData);
                int[] mags = new int[spectData.Count];
                for (int i = 0; i < spectData.Count; i++)
                {
                    Complex c = (Complex)spectData[i];
                    mags[i] = (int)Math.Sqrt(Math.Pow(c.Real, 2) + Math.Pow(c.Imaginary, 2));
                }
                Point[] spectrum = new Point[mags.Length / 2];
                Double spectrumInterval = (Double) w / spectrum.Length;
                for (int i = 0; i < spectrum.Length; i++)
                {
                    int freqHeight = (int)(((double)mags[i] / mags.Max()) * h);
                    spectrum[i] = new Point((int)(spectrumInterval * i), h - freqHeight);
                }
                return spectrum;
            }
            return null;
        }

        //Fast Fourier Transform Method
        private void FFT(ArrayList x)
        {
            int n = x.Count;
            if (n <= 1) return;
            ArrayList even = new ArrayList();
            ArrayList odd = new ArrayList();
            for (int i = 0; i < n; i++)
            {
                if (i % 2 == 0) even.Add(x[i]);
                else odd.Add(x[i]);
            }
            FFT(even);
            FFT(odd);
            for (int k = 0; k < n / 2; k++)
            {
                Complex t = Complex.FromPolarCoordinates(1.0, -2 * Math.PI * k / n) * ((Complex)odd[k]);
                x[k] = ((Complex)even[k]) + t;
                x[k + (n / 2)] = ((Complex)even[k]) - t;
            }
        }
    }
}
