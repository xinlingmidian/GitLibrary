using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.InputMethodServices;
using Java.IO;

namespace mono_App1
{
    [Service]
    public class MyService : Service
    {
        /*Thread work = null;
        Handler handler = new Handler();
        int startCount = 0;*/
        private string TAG = "ScreenReceiver Log";
        private KeyguardManager keyguardManager = null;
#pragma warning disable
        private KeyguardManager.KeyguardLock keyguardLock = null;
#pragma warning restore
        private MyBroadcastReceiver ScreenReceiver = new MyBroadcastReceiver();
        Intent toMainIntent;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            toMainIntent = new Intent(this, typeof(UnlockActivity));//#设置Main.class为要跳转到的界面，既当解锁时要打开的界面
                                                                  //toMainIntent = new Intent();
            toMainIntent.AddFlags(ActivityFlags.NewTask);//必须得有，不知为何

            //注册广播
            IntentFilter intentFilter = new IntentFilter(Intent.ActionScreenOff);
            intentFilter.AddAction(Intent.ActionScreenOn);
            

            ScreenReceiver.Receive += (Context, Intent) =>
              {
                  string action = Intent.Action;
                  Log.Error(TAG, "intent.action = " + action);
                  if(action.Equals(Intent.ActionScreenOff))
                  {
                      SetUnlockF(this);
                  }
                  if (!GetState())
                      StartActivity(new Intent(this, typeof(UnlockActivity)));
                  if (action.Equals(Intent.ActionScreenOn) || action.Equals(Intent.ActionScreenOff))
                  {
                      //关闭锁屏
                      keyguardManager = (KeyguardManager)Context.GetSystemService(KeyguardService);
#pragma warning disable CS0618 // 类型或成员已过时
                      keyguardLock = keyguardManager.NewKeyguardLock("");
#pragma warning restore CS0618 // 类型或成员已过时
                      keyguardLock.DisableKeyguard();
                      ReferenceEquals("", "closed the keyGuard");

                      //打开主界面
                      StartActivity(toMainIntent);
                  }

              };

            RegisterReceiver(ScreenReceiver, intentFilter);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags startCommandFlags, int i)
        {
            //return base.OnStartCommand(intent, startCommandFlags, i);
            
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterReceiver(ScreenReceiver);
            Intent intent;
            intent = new Intent();
            intent.SetAction("MyService");
            StartService(intent);
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
            if (!GetState())
                StartActivity(new Intent(this, typeof(UnlockActivity)));
        }

        void SetUnlockF(Context context)
        {
            Java.IO.File file = new Java.IO.File(context.ExternalCacheDir, "0x7f.txt");
            FileOutputStream fos = new FileOutputStream(file);
            UTF8Encoding enc = new UTF8Encoding();
            fos.Write(enc.GetBytes("F"), 0, enc.GetBytes("F").Length);
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
            catch (Java.IO.FileNotFoundException e)
            {
                Log.Error("mono_App1", e.Message);
            }
            return true;
        }

    }
    
    public class MyBroadcastReceiver:BroadcastReceiver
    {
        public event Action<Context, Intent> Receive;
        public override void OnReceive(Context context, Intent intent)
        {
            if (this.Receive != null) this.Receive(context, intent);
        }
    }
}