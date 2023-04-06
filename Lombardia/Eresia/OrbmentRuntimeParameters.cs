using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia.OrbmentParameters
{
    public enum EventLogger : ushort
    {
        CCPU_BUILT_IN_LOGGER = 0x0001
    }

    public enum ClockSource : ushort
    {
        BUILT_IN_HARDWARE_CLOCK = 0x0001,
        INTER_MODULE_SYNC_CLOCK = 0x0002
    }

    public class DeviceIOScanTaskConfiguration
    {
        private uint __posix_priority = 255 - 47;
        public uint PosixPriority 
        {
            get { return __posix_priority; }
            set { if (value > 255) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.POSIX_PRIORITY_OUT_OF_RANGE); __posix_priority = value; }
        }
        private uint __period = 5;
        public uint PeriodInMillisecond
        {
            get { return __period; }
            set 
            { 
                if (value <= 1000 && value % 5 != 0) 
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIMER_CYCLE_SETTING); 
                else if (value > 1000 && value % 1000 != 0)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIMER_CYCLE_SETTING);
                else if(value > 60000)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIMER_CYCLE_SETTING);
                __period = value; 
            }
        }
        public ClockSource ClockSource { get; set; } = ClockSource.INTER_MODULE_SYNC_CLOCK;
        public uint TaskStackInByte { get; set; } = 0;

        public bool CustomPosixPriority { get; set; } = false;
        public bool CustomPeriod { get; set; } = false;
        public bool CustomClockSource { get; set; } = false;
        public bool CustomTaskStack { get; set; } = false;

        public void ApplyDeviceRuntimeDefault()
        {
            CustomPosixPriority = false;
            CustomPeriod = false;
            CustomClockSource = false;
            CustomTaskStack = false;
        }

        public DeviceIOScanTaskConfiguration ShallowCopy()
        {
            return (DeviceIOScanTaskConfiguration)this.MemberwiseClone();
        }
    }

    public class DeviceControlTaskConfiguration
    {
        private uint __posix_priority = 255 - 201;
        public uint PosixPriority
        {
            get { return __posix_priority; }
            set { if (value > 255) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.POSIX_PRIORITY_OUT_OF_RANGE); __posix_priority = value; }
        }
        private uint __period = 100;
        public uint PeriodInMillisecond
        {
            get { return __period; }
            set
            {
                if (value <= 1000 && value % 5 != 0)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIMER_CYCLE_SETTING);
                else if (value > 1000 && value % 1000 != 0)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIMER_CYCLE_SETTING);
                else if (value > 60000)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIMER_CYCLE_SETTING);
                __period = value;
            }
        }
        public uint TaskStackInByte { get; set; } = 0;
        public bool CustomPosixPriority { get; set; } = false;
        public bool CustomPeriod { get; set; } = false;
        public bool CustomTaskStack { get; set; } = false;

        public void ApplyDeviceRuntimeDefault()
        {
            CustomPosixPriority = false;
            CustomPeriod = false;
            CustomTaskStack = false;
        }

        public DeviceControlTaskConfiguration ShallowCopy()
        {
            return (DeviceControlTaskConfiguration)this.MemberwiseClone();
        }
    }

    public class DLinkServiceConfiguration
    {
        private uint __posix_priority = 255 - 100;
        public uint PosixPriority
        {
            get { return __posix_priority; }
            set { if (value > 255) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.POSIX_PRIORITY_OUT_OF_RANGE); __posix_priority = value; }
        }
        public ushort Port { get; set; } = 8366;
        private int __recv = 20000;
        public int RecvTimeout
        {
            get { return __recv; }
            set { if (value < 0) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIME_OUT_SETTING); __recv = value; }
        }
        private int __send = 20000;
        public int SendTimeout
        {
            get { return __send; }
            set { if (value < 0) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIME_OUT_SETTING); __send = value; }
        }
        public uint UserReservedMemoryInByte { get; set; } = 0;

        public bool CustomPosixPriority { get; set; } = false;
        public bool CustomPort { get; set; } = false;
        public bool CustomRecv { get; set; } = false;
        public bool CustomSend { get; set; } = false;
        public bool CustomReservedMemory { get; set; } = false;

        public void ApplyDeviceRuntimeDefault()
        {
            CustomPosixPriority = false;
            CustomPort = false;
            CustomRecv = false;
            CustomSend = false;
            CustomReservedMemory = false;
        }
        public DLinkServiceConfiguration ShallowCopy()
        {
            return (DLinkServiceConfiguration)this.MemberwiseClone();
        }
    }

    public class ILinkServiceConfiguration
    {
        private uint __posix_priority = 255 - 202;
        public uint PosixPriority
        {
            get { return __posix_priority; }
            set { if (value > 255) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.POSIX_PRIORITY_OUT_OF_RANGE); __posix_priority = value; }
        }
        public ushort Port { get; set; } = 8367;
        private int __recv = 300000;
        public int RecvTimeout
        {
            get { return __recv; }
            set { if (value < 0) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIME_OUT_SETTING); __recv = value; }
        }
        private int __send = 300000;
        public int SendTimeout
        {
            get { return __send; }
            set { if (value < 0) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIME_OUT_SETTING); __send = value; }
        }
        public uint EngineStepCapacity { get; set; } = 4096;

        public bool CustomPosixPriority { get; set; } = false;
        public bool CustomPort { get; set; } = false;
        public bool CustomRecv { get; set; } = false;
        public bool CustomSend { get; set; } = false;
        public bool CustomStepCapacity { get; set; } = false;

        public void ApplyDeviceRuntimeDefault()
        {
            CustomPosixPriority = false;
            CustomPort = false;
            CustomRecv = false;
            CustomSend = false;
            CustomStepCapacity = false;
        }

        public ILinkServiceConfiguration ShallowCopy()
        {
            return (ILinkServiceConfiguration)this.MemberwiseClone();
        }
    }

    public class RLinkServiceConfiguration
    {
        private uint __posix_priority = 255 - 50;
        public uint PosixPriority
        {
            get { return __posix_priority; }
            set { if (value > 255) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.POSIX_PRIORITY_OUT_OF_RANGE); __posix_priority = value; }
        }
        public ushort Port { get; set; } = 8368;
        private int __recv = 5000;
        public int RecvTimeout
        {
            get { return __recv; }
            set { if (value < 0) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIME_OUT_SETTING); __recv = value; }
        }
        private int __send = 10000;
        public int SendTimeout
        {
            get { return __send; }
            set { if (value < 0) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_TIME_OUT_SETTING); __send = value; }
        }
        public uint ReservedMemoryInWord { get; set; } = 1024 * 1024 * 8;
        public uint AcquisitionRate { get; set; } = 1;

        public bool CustomPosixPriority { get; set; } = false;
        public bool CustomPort { get; set; } = false;
        public bool CustomRecv { get; set; } = false;
        public bool CustomSend { get; set; } = false;
        public bool CustomReservedMemory { get; set; } = false;
        public bool CustomAcquisitionRate { get; set; } = false;

        public void ApplyDeviceRuntimeDefault()
        {
            CustomPosixPriority = false;
            CustomPort = false;
            CustomRecv = false;
            CustomSend = false;
            CustomReservedMemory = false;
            CustomAcquisitionRate = false;
        }

        public RLinkServiceConfiguration ShallowCopy()
        {
            return (RLinkServiceConfiguration)this.MemberwiseClone();
        }
    }

    public class RuntimeConfiguration
    {
        public EventLogger EventLogger { get; set; } = EventLogger.CCPU_BUILT_IN_LOGGER;
        public bool CustomEventLogger { get; set; } = false;
        public DeviceIOScanTaskConfiguration DeviceIOScanTaskConfiguration { get; init; } = new DeviceIOScanTaskConfiguration();
        public DeviceControlTaskConfiguration DeviceControlTaskConfiguration { get; init; } = new DeviceControlTaskConfiguration();
        public DLinkServiceConfiguration DLinkServiceConfiguration { get; init; } = new DLinkServiceConfiguration();
        public ILinkServiceConfiguration ILinkServiceConfiguration { get; init; } = new ILinkServiceConfiguration(); 
        public RLinkServiceConfiguration RLinkServiceConfiguration { get; init; } = new RLinkServiceConfiguration();
        public RuntimeConfiguration()
        {

        }

        public RuntimeConfiguration(XmlNode node)
        {
            __ReLoad(node);
        }

        public void __ReLoad(XmlNode node)
        {
            try
            {
                if(node != null && node.NodeType == XmlNodeType.Element && node.Name == "RuntimeConfiguration")
                {
                    foreach (XmlNode sub in node.ChildNodes)
                    {
                        if (sub.NodeType != XmlNodeType.Element)
                            continue;

                        switch (sub.Name)
                        {
                            case "EventLogger":
                                EventLogger = Enum.Parse<EventLogger>(sub.FirstChild.Value);
                                CustomEventLogger = true;
                                break;
                            case "DeviceIOScanTask":
                                if (sub.SelectSingleNode("Priority") != null)
                                {
                                    DeviceIOScanTaskConfiguration.PosixPriority = Convert.ToUInt32(sub.SelectSingleNode("Priority").FirstChild.Value);
                                    DeviceIOScanTaskConfiguration.CustomPosixPriority = true;
                                }
                                if (sub.SelectSingleNode("Clock") != null)
                                {
                                    DeviceIOScanTaskConfiguration.ClockSource = Enum.Parse<ClockSource>(sub.SelectSingleNode("Clock").FirstChild.Value);
                                    DeviceIOScanTaskConfiguration.CustomClockSource = true;
                                }
                                if (sub.SelectSingleNode("Period") != null)
                                {
                                    DeviceIOScanTaskConfiguration.PeriodInMillisecond = Convert.ToUInt32(sub.SelectSingleNode("Period").FirstChild.Value);
                                    DeviceIOScanTaskConfiguration.CustomPeriod = true;
                                }
                                if (sub.SelectSingleNode("StackBytes") != null)
                                {
                                    DeviceIOScanTaskConfiguration.TaskStackInByte = Convert.ToUInt32(sub.SelectSingleNode("StackBytes").FirstChild.Value);
                                    DeviceIOScanTaskConfiguration.CustomTaskStack = true;
                                }
                                break;
                            case "DeviceControlTask":
                                if (sub.SelectSingleNode("Priority") != null)
                                {
                                    DeviceControlTaskConfiguration.PosixPriority = Convert.ToUInt32(sub.SelectSingleNode("Priority").FirstChild.Value);
                                    DeviceControlTaskConfiguration.CustomPosixPriority = true;
                                }
                                if (sub.SelectSingleNode("Period") != null)
                                {
                                    DeviceControlTaskConfiguration.PeriodInMillisecond = Convert.ToUInt32(sub.SelectSingleNode("Period").FirstChild.Value);
                                    DeviceControlTaskConfiguration.CustomPeriod = true;
                                }
                                if (sub.SelectSingleNode("StackBytes") != null)
                                {
                                    DeviceControlTaskConfiguration.TaskStackInByte = Convert.ToUInt32(sub.SelectSingleNode("StackBytes").FirstChild.Value);
                                    DeviceControlTaskConfiguration.CustomTaskStack = true;
                                }
                                break;
                            case "DLinkService":
                                if (sub.SelectSingleNode("Priority") != null)
                                {
                                    DLinkServiceConfiguration.PosixPriority = Convert.ToUInt32(sub.SelectSingleNode("Priority").FirstChild.Value);
                                    DLinkServiceConfiguration.CustomPosixPriority = true;
                                }
                                if (sub.SelectSingleNode("Port") != null)
                                {
                                    DLinkServiceConfiguration.Port = Convert.ToUInt16(sub.SelectSingleNode("Port").FirstChild.Value);
                                    DLinkServiceConfiguration.CustomPort = true;
                                }
                                if (sub.SelectSingleNode("Recv") != null)
                                {
                                    DLinkServiceConfiguration.RecvTimeout = Convert.ToInt32(sub.SelectSingleNode("Recv").FirstChild.Value);
                                    DLinkServiceConfiguration.CustomRecv = true;
                                }
                                if (sub.SelectSingleNode("Send") != null)
                                {
                                    DLinkServiceConfiguration.SendTimeout = Convert.ToInt32(sub.SelectSingleNode("Send").FirstChild.Value);
                                    DLinkServiceConfiguration.CustomSend = true;
                                }
                                if (sub.SelectSingleNode("ReservedBytes") != null)
                                {
                                    DLinkServiceConfiguration.UserReservedMemoryInByte = Convert.ToUInt32(sub.SelectSingleNode("ReservedBytes").FirstChild.Value);
                                    DLinkServiceConfiguration.CustomReservedMemory = true;
                                }
                                break;
                            case "ILinkService":
                                if (sub.SelectSingleNode("Priority") != null)
                                {
                                    ILinkServiceConfiguration.PosixPriority = Convert.ToUInt32(sub.SelectSingleNode("Priority").FirstChild.Value);
                                    ILinkServiceConfiguration.CustomPosixPriority = true;
                                }
                                if (sub.SelectSingleNode("Port") != null)
                                {
                                    ILinkServiceConfiguration.Port = Convert.ToUInt16(sub.SelectSingleNode("Port").FirstChild.Value);
                                    ILinkServiceConfiguration.CustomPort = true;
                                }
                                if (sub.SelectSingleNode("Recv") != null)
                                {
                                    ILinkServiceConfiguration.RecvTimeout = Convert.ToInt32(sub.SelectSingleNode("Recv").FirstChild.Value);
                                    ILinkServiceConfiguration.CustomRecv = true;
                                }
                                if (sub.SelectSingleNode("Send") != null)
                                {
                                    ILinkServiceConfiguration.SendTimeout = Convert.ToInt32(sub.SelectSingleNode("Send").FirstChild.Value);
                                    ILinkServiceConfiguration.CustomSend = true;
                                }
                                if (sub.SelectSingleNode("StepCapacity") != null)
                                {
                                    ILinkServiceConfiguration.EngineStepCapacity = Convert.ToUInt32(sub.SelectSingleNode("StepCapacity").FirstChild.Value);
                                    ILinkServiceConfiguration.CustomStepCapacity = true;
                                }
                                break;
                            case "RLinkService":
                                if (sub.SelectSingleNode("Priority") != null)
                                { 
                                    RLinkServiceConfiguration.PosixPriority = Convert.ToUInt32(sub.SelectSingleNode("Priority").FirstChild.Value);
                                    RLinkServiceConfiguration.CustomPosixPriority = true;
                                }
                                if (sub.SelectSingleNode("Port") != null)
                                {
                                    RLinkServiceConfiguration.Port = Convert.ToUInt16(sub.SelectSingleNode("Port").FirstChild.Value);
                                    RLinkServiceConfiguration.CustomPort = true;
                                }
                                if (sub.SelectSingleNode("Recv") != null)
                                {
                                    RLinkServiceConfiguration.RecvTimeout = Convert.ToInt32(sub.SelectSingleNode("Recv").FirstChild.Value);
                                    RLinkServiceConfiguration.CustomRecv = true;
                                }
                                if (sub.SelectSingleNode("Send") != null)
                                {
                                    RLinkServiceConfiguration.SendTimeout = Convert.ToInt32(sub.SelectSingleNode("Send").FirstChild.Value);
                                    RLinkServiceConfiguration.CustomSend = true;
                                }
                                if (sub.SelectSingleNode("ReservedWords") != null)
                                {
                                    RLinkServiceConfiguration.ReservedMemoryInWord = Convert.ToUInt32(sub.SelectSingleNode("ReservedWords").FirstChild.Value);
                                    RLinkServiceConfiguration.CustomReservedMemory = true;
                                }
                                if (sub.SelectSingleNode("AcquisitionRate") != null)
                                {
                                    RLinkServiceConfiguration.AcquisitionRate = Convert.ToUInt32(sub.SelectSingleNode("AcquisitionRate").FirstChild.Value);
                                    RLinkServiceConfiguration.CustomAcquisitionRate = true;
                                }
                                break;
                        }
                    }
                }
            }
            catch (LombardiaException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LombardiaException(e);
            }
        }

        public void ReLoad(XmlNode node)
        {
            CustomEventLogger = false;
            DeviceIOScanTaskConfiguration.ApplyDeviceRuntimeDefault();
            DeviceControlTaskConfiguration.ApplyDeviceRuntimeDefault();
            DLinkServiceConfiguration.ApplyDeviceRuntimeDefault();
            ILinkServiceConfiguration.ApplyDeviceRuntimeDefault();
            RLinkServiceConfiguration.ApplyDeviceRuntimeDefault();
            __ReLoad(node);
        }

        public XmlElement ToXml(XmlDocument doc)
        {
            XmlElement configurationNode = doc.CreateElement("RuntimeConfiguration");

            XmlElement areaNode, propertyNode;

            if (CustomEventLogger)
            {
                areaNode = doc.CreateElement("EventLogger");
                areaNode.AppendChild(doc.CreateTextNode(EventLogger.ToString()));
                configurationNode.AppendChild(areaNode);
            }

            areaNode = doc.CreateElement("DeviceIOScanTask");
            if (DeviceIOScanTaskConfiguration.CustomPosixPriority)
            {
                propertyNode = doc.CreateElement("Priority");
                propertyNode.AppendChild(doc.CreateTextNode(DeviceIOScanTaskConfiguration.PosixPriority.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (DeviceIOScanTaskConfiguration.CustomClockSource)
            {
                propertyNode = doc.CreateElement("Clock");
                propertyNode.AppendChild(doc.CreateTextNode(DeviceIOScanTaskConfiguration.ClockSource.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (DeviceIOScanTaskConfiguration.CustomPeriod)
            {
                propertyNode = doc.CreateElement("Period");
                propertyNode.AppendChild(doc.CreateTextNode(DeviceIOScanTaskConfiguration.PeriodInMillisecond.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (DeviceIOScanTaskConfiguration.CustomPeriod)
            {
                propertyNode = doc.CreateElement("StackBytes");
                propertyNode.AppendChild(doc.CreateTextNode(DeviceIOScanTaskConfiguration.TaskStackInByte.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            configurationNode.AppendChild(areaNode);

            areaNode = doc.CreateElement("DeviceControlTask");
            if (DeviceControlTaskConfiguration .CustomPosixPriority)
            {
                propertyNode = doc.CreateElement("Priority");
                propertyNode.AppendChild(doc.CreateTextNode(DeviceControlTaskConfiguration.PosixPriority.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (DeviceControlTaskConfiguration.CustomPeriod)
            {
                propertyNode = doc.CreateElement("Period");
                propertyNode.AppendChild(doc.CreateTextNode(DeviceControlTaskConfiguration.PeriodInMillisecond.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (DeviceControlTaskConfiguration.CustomTaskStack)
            {
                propertyNode = doc.CreateElement("StackBytes");
                propertyNode.AppendChild(doc.CreateTextNode(DeviceControlTaskConfiguration.TaskStackInByte.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            configurationNode.AppendChild(areaNode);

            areaNode = doc.CreateElement("DLinkService");
            if (DLinkServiceConfiguration.CustomPosixPriority)
            {
                propertyNode = doc.CreateElement("Priority");
                propertyNode.AppendChild(doc.CreateTextNode(DLinkServiceConfiguration.PosixPriority.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (DLinkServiceConfiguration.CustomPort)
            {
                propertyNode = doc.CreateElement("Port");
                propertyNode.AppendChild(doc.CreateTextNode(DLinkServiceConfiguration.Port.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (DLinkServiceConfiguration.CustomRecv)
            {
                propertyNode = doc.CreateElement("Recv");
                propertyNode.AppendChild(doc.CreateTextNode(DLinkServiceConfiguration.RecvTimeout.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (DLinkServiceConfiguration.CustomSend)
            {
                propertyNode = doc.CreateElement("Send");
                propertyNode.AppendChild(doc.CreateTextNode(DLinkServiceConfiguration.SendTimeout.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (DLinkServiceConfiguration.CustomReservedMemory)
            {
                propertyNode = doc.CreateElement("ReservedBytes");
                propertyNode.AppendChild(doc.CreateTextNode(DLinkServiceConfiguration.UserReservedMemoryInByte.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            configurationNode.AppendChild(areaNode);

            areaNode = doc.CreateElement("ILinkService");
            if (ILinkServiceConfiguration.CustomPosixPriority)
            {
                propertyNode = doc.CreateElement("Priority");
                propertyNode.AppendChild(doc.CreateTextNode(ILinkServiceConfiguration.PosixPriority.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (ILinkServiceConfiguration.CustomPort)
            {
                propertyNode = doc.CreateElement("Port");
                propertyNode.AppendChild(doc.CreateTextNode(ILinkServiceConfiguration.Port.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (ILinkServiceConfiguration.CustomRecv)
            {
                propertyNode = doc.CreateElement("Recv");
                propertyNode.AppendChild(doc.CreateTextNode(ILinkServiceConfiguration.RecvTimeout.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (ILinkServiceConfiguration.CustomSend)
            {
                propertyNode = doc.CreateElement("Send");
                propertyNode.AppendChild(doc.CreateTextNode(ILinkServiceConfiguration.SendTimeout.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (ILinkServiceConfiguration.CustomStepCapacity)
            {
                propertyNode = doc.CreateElement("StepCapacity");
                propertyNode.AppendChild(doc.CreateTextNode(ILinkServiceConfiguration.EngineStepCapacity.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            configurationNode.AppendChild(areaNode);

            areaNode = doc.CreateElement("RLinkService");
            if (RLinkServiceConfiguration.CustomPosixPriority)
            {
                propertyNode = doc.CreateElement("Priority");
                propertyNode.AppendChild(doc.CreateTextNode(RLinkServiceConfiguration.PosixPriority.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (RLinkServiceConfiguration.CustomPort)
            {
                propertyNode = doc.CreateElement("Port");
                propertyNode.AppendChild(doc.CreateTextNode(RLinkServiceConfiguration.Port.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (RLinkServiceConfiguration.CustomRecv)
            {
                propertyNode = doc.CreateElement("Recv");
                propertyNode.AppendChild(doc.CreateTextNode(RLinkServiceConfiguration.RecvTimeout.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (RLinkServiceConfiguration.CustomSend)
            {
                propertyNode = doc.CreateElement("Send");
                propertyNode.AppendChild(doc.CreateTextNode(RLinkServiceConfiguration.SendTimeout.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (RLinkServiceConfiguration.CustomReservedMemory)
            {
                propertyNode = doc.CreateElement("ReservedWords");
                propertyNode.AppendChild(doc.CreateTextNode(RLinkServiceConfiguration.ReservedMemoryInWord.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            if (RLinkServiceConfiguration.CustomAcquisitionRate)
            {
                propertyNode = doc.CreateElement("AcquisitionRate");
                propertyNode.AppendChild(doc.CreateTextNode(RLinkServiceConfiguration.AcquisitionRate.ToString()));
                areaNode.AppendChild(propertyNode);
            }
            configurationNode.AppendChild(areaNode);

            return configurationNode;
        }
    }
}
