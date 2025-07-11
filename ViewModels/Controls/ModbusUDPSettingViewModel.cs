using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.ViewModels.Controls
{
    public partial class ModbusUDPSettingViewModel : ObservableObject
    {

        [ObservableProperty]
        private string _address="192.168.10.1";

        [ObservableProperty]
        private int _port=502;

    }
}
