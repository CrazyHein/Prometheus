using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public class TargetInformationDataModel : IOListDataModel
    {
        private string __name;
        private string __description;

        public TargetInformationDataModel(IOListDataHelper helper) : base(helper)
        {
        }

        public override void UpdateDataModel(bool clearDirtyFlag = true)
        {
            Name = _data_helper.TargetInformation.name;
            Description = _data_helper.TargetInformation.description;
            if(clearDirtyFlag)
                Dirty = false;
        }

        public override void UpdateDataHelper(bool clearDirtyFlag = false)
        {
            _data_helper.TargetInformation.name = Name;
            _data_helper.TargetInformation.description = Description;
            if(clearDirtyFlag)
                Dirty = false;
        }

        public string Name
        {
            get { return __name; }
            set { SetProperty(ref __name, value); }
        }

        public string Description
        {
            get { return __description; }
            set { SetProperty(ref __description, value); }
        }
    }
}
