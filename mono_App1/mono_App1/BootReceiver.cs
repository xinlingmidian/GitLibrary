using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace mono_App1
{
    [BroadcastReceiver]

    public class BootReceiver : BroadcastReceiver
    {
        string myPkgName = "mono_App1.mono_App1";//#包名
        string myActName = "MyService";//#类名

        public override void OnReceive(Context context, Intent intent)
        {
            //启动监听服务
            Intent myIntent = new Intent();
            myIntent.SetAction(myPkgName + "." + myActName);
            context.StartService(myIntent);
            throw new NotImplementedException();
        }
        
        
    }
}