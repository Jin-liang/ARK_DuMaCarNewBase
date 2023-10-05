using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinAssist.Device
{
    public  class ArkCenterCom:CenterCom
    {
        public string DevVersionName = "";
        public ArkCenterCom()
        {
            //默认Y310
            DevVersionName = "Y310";
        }
        public ArkCenterCom(string strVersion)
        {
            //默认Y310
            DevVersionName = strVersion;
        }
    }
}
