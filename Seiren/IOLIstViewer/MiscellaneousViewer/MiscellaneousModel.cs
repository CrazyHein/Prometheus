using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class MiscellaneousModel : RecordContainerModel
    {
        private Miscellaneous __misc;
        public MiscellaneousModel(Miscellaneous misc)
        {
            __misc = misc;
            Modified = false;
        }

        public string BasicInfoName
        {
            get { return __misc.IOListName; }
            set 
            {
                value = value.Trim();
                if (__misc.IOListName == value)
                {
                    //Modified = false;
                }
                else
                {
                    __misc.IOListName = value;
                    Modified = true;
                }
            }
        }

        public string BasicInfoDescription
        {
            get { return __misc.Description; }
            set 
            {
                if(__misc.Description == value)
                {
                    //Modified = false;
                }
                else
                {
                    __misc.Description = value;
                    Modified = true;
                }
            }
        }

        public string VariableDictionary
        {
            get { return __misc.VariableDictionary; }
            set
            {
                value = value.Trim();
                if (__misc.VariableDictionary == value)
                {
                    //Modified = false;
                }
                else
                {
                    __misc.VariableDictionary = value;
                    Modified = true;
                }
            }
        }

        public string MCServerIPv4
        {
            get { return __misc.MCServerIPv4; }
            set 
            {
                try
                {
                    value = value.Trim();
                    if (__misc.MCServerIPv4 == value)
                    {
                        //Modified = false;
                    }
                    else
                    {
                        __misc.MCServerIPv4 = value;
                        Modified = true;
                    }
                }
                catch (LombardiaException)
                {
                    throw;
                }
            }
        }

        public ushort MCServerPort
        {
            get { return __misc.MCServerPort; }
            set 
            {
                if (__misc.MCServerPort == value)
                {
                    //Modified = false;
                }
                else
                {
                    __misc.MCServerPort = value;
                    Modified = true;
                }
            }
        }
    }
}
