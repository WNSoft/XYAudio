using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace XYAudio
{
    class SoundFile
    {
        //full filename and path
        private String filename;
        //number of channels in audio file
        private int channels;
        //sample rate of audio file
        private int sampleRate;
        //number of bits in each sample
        private int bitsPerSample;
        //number of bytes of audio data
        private int dataSize;
        //object to store the sound data
        private SoundData soundData;
        //file data
        Byte[] fileData;
        //wav duration
        Double duration;

        //constructor
        public SoundFile(String filename)
        {
            this.filename = filename;
            readFile();
        }

        //load file properties
        private void readFile()
        {
            //get byte array of entire file
            fileData = File.ReadAllBytes(filename);
            //check for valid wave file
            if (!(fileData[0] == 'R' && fileData[1] == 'I' && fileData[2] == 'F' && fileData[3] == 'F'))
            {
                throw new System.ArgumentException("File is not a valid wave file.");
            }
            if (!(fileData[8] == 'W' && fileData[9] == 'A' && fileData[10] == 'V' && fileData[11] == 'E'))
            {
                throw new System.ArgumentException("File is not a valid wave file.");
            }
            //get channels, sample rate, and bits per sample from header data
            channels = BitConverter.ToInt16(fileData, 22);
            sampleRate = BitConverter.ToInt32(fileData, 24);
            bitsPerSample = BitConverter.ToInt16(fileData, 34);
            //check for supported bit depth
            if (!(bitsPerSample == 8 || bitsPerSample == 16 || bitsPerSample == 32))
            {
                throw new System.ArgumentException("File bit depth is not supported.");
            }
            //find index of start of sound data
            int soundDataIndex = -1;
            for (int i = 36; i < fileData.Length; i++)
            {
                if (fileData[i] == 'd' && fileData[i + 1] == 'a' && fileData[i + 2] == 't' && fileData[i + 3] == 'a')
                {
                    soundDataIndex = i + 8;
                    break;
                }
            }
            //get number of bytes of sound data
            dataSize = BitConverter.ToInt32(fileData, soundDataIndex - 4) - 8;
            //calculate length in ms
            duration = (double) ((double) dataSize / ( (double) sampleRate * channels * (bitsPerSample / 8)));
            //initialize sound data object
            soundData = new SoundData(fileData, channels, bitsPerSample, sampleRate, dataSize, soundDataIndex);
        }

        //get methods for file properties

        public String getFilename()
        {
            return filename;
        }

        public int getChannels()
        {
            return channels;
        }

        public int getSampleRate()
        {
            return sampleRate;
        }

        public int getBitsPerSample()
        {
            return bitsPerSample;
        }

        public int[] getChannelData(int channel)
        {
            return soundData.getChannelData(channel);
        }

        public Byte[] getFileData()
        {
            return fileData;
        }

        public Double getDuration()
        {
            return duration;
        }

        //generate waveform points from sound data
        public Point[] getWaveformPoints(int w, int h, int channel)
        {
            return soundData.getWaveformPoints(w, h, channel);
        }

        //generate spectrum points from sound data
        public Point[] getSpectrumPoints(int w, int h, double time, int channel)
        {
            return soundData.getSpectrumPoints(w, h, time, channel);
        }

        //custom tostring method
        public override String ToString()
        {
            return ("Filename: " + filename
                + "\nChannels: " + channels
                + "\nSample Rate: " + sampleRate
                + "\nBits Per Sample: " + bitsPerSample);
        }
    }
}
