using Android.Media;
using Android.Util;
using Java.IO;
using Microsoft.ProjectOxford.Speech.SpeakerVerification;
using System.IO.IsolatedStorage;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mono_App1
{
    public class AudioRecordClass
    {

        private AudioSource audioSource = AudioSource.Mic;
        // 设置音频采样率，44100是目前的标准，但是某些设备仍然支持22050，16000，11025  
        private static int sampleRateInHz = 44100;
        // 设置音频的录制的声道CHANNEL_IN_STEREO为双声道，CHANNEL_CONFIGURATION_MONO为单声道  
        private static ChannelIn channelConfig = ChannelIn.Mono;
        // 音频数据格式:PCM 16位每个样本。保证设备支持。PCM 8位每个样本。不一定能得到设备支持。  
        private Android.Media.Encoding audioFormat = Android.Media.Encoding.Pcm16bit;
        // 缓冲区字节大小  
        private int bufferSizeInBytes = 0;
        private AudioRecord audioRecord;
        private bool isRecord = false;// 设置正在录制的状态  
        //AudioName裸音频数据文件  
        private string AudioName;
        //NewAudioName可播放的音频文件  
        private string NewAudioName;
        private static File fileDir;
        System.Threading.Thread worker = null;


        public AudioRecordClass(File _fileDir, string file1, string file2)
        {
            bufferSizeInBytes = 4096;// AudioRecord.GetMinBufferSize(sampleRateInHz, channelConfig, audioFormat);
            audioRecord = new AudioRecord(audioSource, sampleRateInHz, channelConfig, audioFormat, bufferSizeInBytes);
            fileDir = _fileDir;
            AudioName = file1;
            NewAudioName = file2; 
        }

        public void StartRecord()
        {
            audioRecord.StartRecording();
            isRecord = true;
            if (worker == null || !worker.IsAlive)
            {
                worker = new System.Threading.Thread(new ThreadStart(WriteDataToFile));
                worker.Start();
            }
            //WriteDataToFile();
            
            
        }

        private void WriteDataToFile()
        {
            byte[] audiodata = new byte[bufferSizeInBytes];
            FileOutputStream fos = null;
            int readsize = 0;
            try
            {
                File file = new File(fileDir, AudioName);
                if (file.Exists())
                {
                    file.Delete();
                }
                fos = new FileOutputStream(file);// 建立一个可存取字节的文件  

            }
            catch (Java.Lang.Exception e)
            {
                //e.StackTrace;
                return ;
            }
            while (isRecord == true)
            {
                readsize = audioRecord.Read(audiodata, 0, bufferSizeInBytes);
                if (0 != readsize)
                {
                    fos.Write(audiodata);
                }
            }
            fos.Close();// 关闭写入流  

        }

        private void CopyWaveFile(string inFilename, string outFilename, File files)
        {
            File inFile = new File(files, inFilename);
            File outFile = new File(files, outFilename);
            if (!outFile.Exists())
                outFile.CreateNewFile();
            FileInputStream infile = null;
            FileOutputStream outfile = null;
            long totalAudioLen = 0;
            long totalDataLen = totalAudioLen + 36;
            long longSampleRate = sampleRateInHz;
            int channels = 2;
            long byteRate = 16 * sampleRateInHz * channels / 8;
            byte[] data = new byte[bufferSizeInBytes];
            try
            {
                infile = new FileInputStream(inFile);
                outfile = new FileOutputStream(outFile);
                totalAudioLen = infile.Channel.Size();
                totalDataLen = totalAudioLen + 36;
                WriteWaveFileHeader(outfile, totalAudioLen, totalDataLen,
                longSampleRate, channels, byteRate);
                while (infile.Read(data) != -1)
                {
                    outfile.Write(data);
                }
                infile.Close();
                outfile.Close();
            }
            catch (System.Exception e)
            {
                //e.printStackTrace();
                Log.Error("audio", e.Message);
            }
        }


        private void WriteWaveFileHeader(FileOutputStream outfile, long totalAudioLen, long totalDataLen, long longSampleRate, int channels, long byteRate)// throw IOException
        {
            byte[] header = new byte[44];
            header[0] = (byte)'R'; // RIFF/WAVE header  
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';
            header[4] = (byte)(totalDataLen & 0xff);
            header[5] = (byte)((totalDataLen >> 8) & 0xff);
            header[6] = (byte)((totalDataLen >> 16) & 0xff);
            header[7] = (byte)((totalDataLen >> 24) & 0xff);
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';
            header[12] = (byte)'f'; // 'fmt ' chunk  
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';
            header[16] = 16; // 4 bytes: size of 'fmt ' chunk  
            header[17] = 0;
            header[18] = 0;
            header[19] = 0;
            header[20] = 1; // format = 1  
            header[21] = 0;
            header[22] = (byte)channels;
            header[23] = 0;
            header[24] = (byte)(longSampleRate & 0xff);
            header[25] = (byte)((longSampleRate >> 8) & 0xff);
            header[26] = (byte)((longSampleRate >> 16) & 0xff);
            header[27] = (byte)((longSampleRate >> 24) & 0xff);
            header[28] = (byte)(byteRate & 0xff);
            header[29] = (byte)((byteRate >> 8) & 0xff);
            header[30] = (byte)((byteRate >> 16) & 0xff);
            header[31] = (byte)((byteRate >> 24) & 0xff);
            header[32] = (byte)(2 * 16 / 8); // block align  
            header[33] = 0;
            header[34] = 16; // bits per sample  
            header[35] = 0;
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';
            header[40] = (byte)(totalAudioLen & 0xff);
            header[41] = (byte)((totalAudioLen >> 8) & 0xff);
            header[42] = (byte)((totalAudioLen >> 16) & 0xff);
            header[43] = (byte)((totalAudioLen >> 24) & 0xff);
            outfile.Write(header, 0, 44);
        }

        public void Stop()
        {
            if (audioRecord != null)
            {
                isRecord = false;//停止文件写入  
                audioRecord.Stop();
                audioRecord.Release();//释放资源  
                audioRecord = null;
                CopyWaveFile(AudioName, NewAudioName, fileDir);
            }
        }
    }
    
    
   /* public class UploadVoice
    {
        private static string myKey = "1ca2249c42014879b5f5f6b36fc325c5";
        private string SelectedFile;
        private SpeechVerificationServiceClient serviceClient;
        private string _speakerId = null;
        private int _remainingEnrollments;
        private static string speakerFileName;
        private static string speakerPhraseFileName;
        private static string speakerEnroolMents;
        private System.IO.Stream stream;
        private string messages;

        public UploadVoice(string filePath)
        {
            serviceClient = new SpeechVerificationServiceClient(myKey);
            SelectedFile = filePath + "/new.wav";
            speakerFileName = filePath + "/id.txt";
            speakerEnroolMents = filePath + "/ments.txt";
            speakerPhraseFileName = filePath + "/phrase.txt";
            initializeSpeaker();
        }

        public void Load()
        {
            stream = System.IO.File.OpenRead(SelectedFile);
            EnrollSpeaker(stream);
        }

        private async void initializeSpeaker()
        {
            File file = new File(speakerFileName);
            if (!file.Exists())
                file.CreateNewFile();
            BufferedReader fileReader = new BufferedReader(new FileReader(file));
            _speakerId = fileReader.ReadLine();
            if (_speakerId == null)
            {
                bool created = await CreateProfile();
                if (!created)
                {

                }
            }
            fileReader.Close();

        }
        

        private async Task<bool> CreateProfile()
        {
            try
            {
                SpeakerProfile response = await serviceClient.CreateProfileAsync("en-us");
                FileOutputStream fos = new FileOutputStream(speakerFileName);
                UTF8Encoding enc = new UTF8Encoding();
                _speakerId = response.VerificationProfileId;
                fos.Write(enc.GetBytes(_speakerId), 0, enc.GetBytes(_speakerId).Length);
                fos.Close();
                return true;
            }
            catch (ProfileCreationException exception)
            {
                return false;
            }
            catch (System.Exception gexp)
            {
                string l = gexp.Message;
                return false;
            }
        }

        private async void EnrollSpeaker(System.IO.Stream audioStream)
        {
            try
            {
                EnrollmentResponse response = await serviceClient.EnrollStreamAsync(audioStream, _speakerId);
                _remainingEnrollments = response.RemainingEnrollments;
                SetUserPhrase(response.Phrase);
                FileOutputStream fos = new FileOutputStream(speakerEnroolMents);
                UTF8Encoding eoc = new UTF8Encoding();
                fos.Write(eoc.GetBytes("Done"), 0, eoc.GetBytes("Done").Length);
            }
            catch (EnrollmentException exception)
            {
                
            }
            catch (System.Exception gexp)
            {
                
            }
        }



        public string FX()
        {
            return messages;
        }

        private void SetUserPhrase(string phrase)
        {
            FileOutputStream fos = new FileOutputStream(speakerPhraseFileName);
            UTF8Encoding eoc = new UTF8Encoding();
            fos.Write(eoc.GetBytes(phrase), 0, eoc.GetBytes(phrase).Length);
            fos.Close();
        }
    }*/

    /*public class CheckVoice
    {
        private static string myKey = "1ca2249c42014879b5f5f6b36fc325c5";
        private string _speakerId = null;
        private System.IO.Stream stream;
        private SpeechVerificationServiceClient _serviceClient;
        private static string speakerFileName;
        private static string speakerPhraseFileName;
        private static string speakerEnroolMents;
        private static string SelectedFile;
        private bool y;

        public CheckVoice(string filePath)
        {
            SelectedFile = filePath + "/new.wav";
            speakerFileName = filePath + "/id.txt";
            speakerEnroolMents = filePath + "/ments.txt";
            speakerPhraseFileName = filePath + "/phrase.txt";
            _serviceClient = new SpeechVerificationServiceClient(myKey);
        }
        
        private async void verifySpeaker(System.IO.Stream audioStream)
        {
            try
            {
                VerificationResult response = await _serviceClient.VerifyAsync(audioStream, _speakerId);

                if (response.Result == VerificationResult.SpeakerVerificationResult.Accept || VerificationResult.ConfidenceLevel.High == response.Confidence) 
                {
                    y = true;
                }
                else
                {
                    y = false;
                }
            }
            catch (Microsoft.ProjectOxford.Speech.SpeakerVerification.VerificationException exception)
            {

            }
            catch (System.Exception e)
            {

            }
        }


        private string GetConfidenceValue(VerificationResult.ConfidenceLevel level)
        {
            switch (level)
            {
                case VerificationResult.ConfidenceLevel.High:
                    return "High";
                case VerificationResult.ConfidenceLevel.Normal:
                    return "Normal";
                case VerificationResult.ConfidenceLevel.Low:
                    return "Low";
                default:
                    return "Unknown value";
            }
        }

        private string GetResponseValue(VerificationResult.SpeakerVerificationResult result)
        {
            switch (result)
            {
                case VerificationResult.SpeakerVerificationResult.Accept:
                    return "Accept";
                case VerificationResult.SpeakerVerificationResult.Reject:
                    return "Reject";
                default:
                    return "Unknown value";
            }
        }

        public bool Check()
        {
            File file = new File(speakerFileName);
            if (!file.Exists())
                file.CreateNewFile();
            BufferedReader fin = new BufferedReader(new FileReader(file));
            BufferedReader fin2 = new BufferedReader(new FileReader(speakerPhraseFileName));
            
            _speakerId = fin.ReadLine();
            if (_speakerId != null)
            {
                string userPhrase = fin2.ReadLine();
            }
            stream = System.IO.File.OpenRead(SelectedFile);
            verifySpeaker(stream);
            fin.Close();
            fin2.Close();
            return y;
        }
    }*/

    /*internal class IsolatedStorageHelper
    {
        private static IsolatedStorageHelper s_helper;

        private IsolatedStorageHelper()
        {

        }

        public static IsolatedStorageHelper getInstance()
        {
            if (s_helper == null)
                s_helper = new IsolatedStorageHelper();
            return s_helper;
        }

        public string readValue(string filename)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStream = new IsolatedStorageFileStream(filename, System.IO.FileMode.Open, isoStore))
                    {
                        using (var reader = new System.IO.StreamReader(iStream))
                        {
                            return reader.ReadLine();
                        }
                    }
                }
                catch (System.Exception e)
                {
                    return null;
                }
            }
        }

        public void writeValue(string fileName, string value)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(fileName, System.IO.FileMode.Create, isoStore))
                {
                    using (var writer = new System.IO.StreamWriter(oStream))
                    {
                        writer.WriteLine(value);
                    }
                }
            }
        }

    }*/
}