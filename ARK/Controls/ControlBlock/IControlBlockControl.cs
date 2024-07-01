using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.ControlBlock
{
    internal interface IControlBlockControl
    {
        void ResetDataModel(ControlBlockModel blk);
        public void UpdateBindingSource();
    }
}
