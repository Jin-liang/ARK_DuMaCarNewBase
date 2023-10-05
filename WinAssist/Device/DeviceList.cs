using DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinAssist.Device
{
    public class DeviceList
    {
        public string devVersion = "";
        public string devName = "";
        public List<DrugboxType> DevList;
    }
    public class DrugboxType
    {
        public t_Drugbox_Info _Drugbox;
        public t_Drugbox_Accurateinfo _Accurate;
        public t_Drugbox_CZinfo _CZ;
        public t_Drugbox_HSinfo _HS;
    }
}
