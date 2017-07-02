using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using System;
using System.Text;
using System.Data;

namespace mono_App1
{
    [Activity(Label = "VLock", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        bool Unlocked = false;
        int SetFinished = 0;
        AudioRecordClass audioRecord;
        //UploadVoice uploadVoice;
        //MFCC mfcc;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Unlocked = false;
            // Set our view from the "main" layout resource
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.Main);
            StartService(new Intent(this,typeof(MyService)));
            //uploadVoice = new UploadVoice(ExternalCacheDir.Path);
            audioRecord = new AudioRecordClass(ExternalCacheDir, "first.raw", "new.wav");
            //mfcc = new MFCC();
            
            Button button = FindViewById<Button>(Resource.Id.button1);
            TextView textView = FindViewById<TextView>(Resource.Id.textView1);
            button.Click += delegate
              {
                  if ((count & 1) == 1)
                  {
                      button.Text = "已开始录制";
                      audioRecord.StartRecord();
                      
                      count++;
                  }
                  else
                  {
                      audioRecord.Stop();
                      count++;
                      //uploadVoice.Load();
                      button.Text = textView.Text = "声纹录制成功";
                      MFCC.getMfcc(ExternalCacheDir + "/new.wav", ExternalCacheDir + "/newvoice.txt");
                      SetFinished++;
                  }

              };
            
            Button finishButton = FindViewById<Button>(Resource.Id.button2);
            EditText password1 = FindViewById<EditText>(Resource.Id.editText1);
            EditText password2 = FindViewById<EditText>(Resource.Id.editText2);

            finishButton.Click += delegate
              {
                  if (password1.Text != password2.Text)
                  {
                      textView.Text = "两次密码不一致，请重新输入";
                  }
                  else if (password1.Text.Length <= 0) 
                  {
                      textView.Text = "请输入备用密码";
                  }
                  else
                  {
                      if (SetFinished == 1)
                      {
                          textView.Text = "设置成功！";
                      }
                      else
                      {
                          textView.Text = "请录入声纹";
                      }
                      SaveFile(password1.Text);
                  }
              };
            SaveFile2("0");
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            TextView textView = FindViewById<TextView>(Resource.Id.textView1);
            textView.Text += "Success\n";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //mediaRecorder.Release();
        }

        void SaveFile(string str)
        {
            Java.IO.File file = new Java.IO.File(ExternalCacheDir, "lcyz.txt");
            //Stream fos = OpenFileOutput(file, FileCreationMode.Private);
            FileOutputStream fos = new FileOutputStream(file);
            UTF8Encoding enc = new UTF8Encoding();
            fos.Write(enc.GetBytes(str), 0, enc.GetBytes(str).Length);
            fos.Close();
        }

        void SaveFile2(string str)
        {
            Java.IO.File file = new Java.IO.File(ExternalCacheDir, "lcyz2.txt");
            //Stream fos = OpenFileOutput(file, FileCreationMode.Private);
            FileOutputStream fos = new FileOutputStream(file);
            UTF8Encoding enc = new UTF8Encoding();
            fos.Write(enc.GetBytes(str), 0, enc.GetBytes(str).Length);
            fos.Close();
        }



    }

    [Activity(Label = "UnlockActivity")]
    public class UnlockActivity : Activity
    {
        int count = 0;
        bool Unlocked = false;
        AudioRecordClass audioRecord;
        //CheckVoice checkVoice;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Unlocked = false;
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.MySettings);
            StartService(new Intent(this, typeof(MyService)));
            Button button = FindViewById<Button>(Resource.Id.button2);
            EditText editText = FindViewById<EditText>(Resource.Id.editText3);
            audioRecord = new AudioRecordClass(ExternalCacheDir, "second.raw", "old.wav");
            //Test test = new Test();
            count = 0;
            Unlocked = false;
            button.Click += delegate
            {
                string str = editText.Text;
                string ans = OpenFile();
                if (ans.Equals(str))
                {
                    SetUnlockT();
                    Finish();
                }
                else
                {
                    button.Text = "密码错误，请检查后重新输入";
                }
                if ((count & 1) == 0) 
                {
                    count++;
                    button.Text = "已开始录音";
                    audioRecord.StartRecord();
                    
                }
                else
                {
                    button.Text = "正在验证，请稍后";
                    count++;
                    audioRecord.Stop();
                    //checkVoice = new CheckVoice(ExternalCacheDir.Path);

                    var kk = Test.matchingDegree(ExternalCacheDir + "/newvoice.txt", ExternalCacheDir + "/old.wav", ExternalCacheDir + "/oldvoice.txt");
                    //if (checkVoice.Check()) 
                    SaveFile3(kk.ToString());
                    if (kk > 62 && kk < 101)
                    {
                        //Java.IO.EOFException e1;
                        //Log.Error("kksu","jgks");
                        Unlocked = true;
                        SetUnlockT();
                        Finish();
                    } 
                    else
                    {
                         Log.Error("kkfa", "jgks");
                         button.Text = "未通过，请重新验证或使用备用密码";
                         editText.SetBackgroundColor(Android.Graphics.Color.White);
                    }
                    
                }
            };

        }

        void SaveFile3(string str)
        {
            Java.IO.File file = new Java.IO.File(ExternalCacheDir, "lcyz3.txt");
            //Stream fos = OpenFileOutput(file, FileCreationMode.Private);
            FileOutputStream fos = new FileOutputStream(file);
            UTF8Encoding enc = new UTF8Encoding();
            fos.Write(enc.GetBytes(str), 0, enc.GetBytes(str).Length);
            fos.Close();
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case (Keycode.Back):
                    {
                        return true;
                    }
                case (Keycode.Menu):
                    {
                        return true;
                    }
            }
            return base.OnKeyDown(keyCode, e);
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            Unlocked = GetState();
            if (!Unlocked) StartActivity(new Intent(this, typeof(UnlockActivity)));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Unlocked = GetState();
            if (!Unlocked) StartActivity(new Intent(this, typeof(UnlockActivity)));
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (!GetState()) OnResume();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (!GetState()) OnRestart();
        }

        string OpenFile()
        {
            byte[] strbyte = new byte[1024];
            try
            {
                Java.IO.File file = new Java.IO.File(ExternalCacheDir, "lcyz.txt");
                //Stream fis = OpenFileInput("lcyz.txt");
                FileInputStream fis = new FileInputStream(file);
                if(fis.Read(strbyte,0,strbyte.Length)>0)
                {
                    UTF8Encoding enc = new UTF8Encoding();
                    char[] chars = enc.GetChars(strbyte);
                    return new string(chars).Trim('\0');
                }
            }
            catch(Java.IO.FileNotFoundException e)
            {
                Log.Error("mono1_App", e.Message); 
            }
            return "";
        }
        

        void SetUnlockT()
        {
            Java.IO.File file = new Java.IO.File(ExternalCacheDir, "0x7f.txt");
            FileOutputStream fos = new FileOutputStream(file);
            UTF8Encoding enc = new UTF8Encoding();
            fos.Write(enc.GetBytes("T"), 0, enc.GetBytes("T").Length);
            fos.Close();
        }

        bool GetState()
        {
            byte[] strbyte = new byte[1024];
            try
            {
                Java.IO.File file = new Java.IO.File(ExternalCacheDir, "0x7f.txt");
                FileInputStream fis = new FileInputStream(file);
                UTF8Encoding enc = new UTF8Encoding();
                if (fis.Read(strbyte, 0, strbyte.Length) > 0)
                {
                    char[] chars = enc.GetChars(strbyte);
                    if (chars.ToString() == "F") return false; 
                }
            }
            catch(Java.IO.FileNotFoundException e)
            {
                Log.Error("mono_App1", e.Message);
            }
            return true;
        }

        
    }

}

