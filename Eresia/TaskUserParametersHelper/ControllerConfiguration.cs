using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Eresia
{
    public class CONTROLLER_EXTENSION_MODULE_T
    {
        public static IReadOnlyList<string> USER_FIELDS { get; private set; }

        public ControllerExtensionModel MODEL { get; private set; }
        public uint SWITCH { get; private set; }

        private ushort address;
        public ushort ADDRESS
        {
            get { return address; }
            private set
            {
                if (value % 16 != 0)
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_EXTENSION_MODULE_ADDRESS, null);
                address = value;
            }
        }

        public IReadOnlyDictionary<string, string> USER_CONFIGURATIONS { get { return __user_configurations; } }
        private Dictionary<string, string> __user_configurations;

        static CONTROLLER_EXTENSION_MODULE_T()
        {
            USER_FIELDS = new List<string>
            {
                "ExtraInputDataOffset",
                "ExtraInputDataSize",
                "ExtraOutputDataOffset",
                "ExtraOutputDataSize"
            }; 
        }

        public CONTROLLER_EXTENSION_MODULE_T(ControllerExtensionModel model, uint sw, ushort address)
        {
            MODEL = model;
            SWITCH = sw;
            ADDRESS = address;
            __user_configurations = new Dictionary<string, string>(USER_FIELDS.Count);
            foreach (var k in USER_FIELDS)
                __user_configurations[k] = "";
        }

        public CONTROLLER_EXTENSION_MODULE_T(ControllerExtensionModel model, uint sw, ushort address, IEnumerable<Tuple<string, string>> users)
        {
            MODEL = model;
            SWITCH = sw;
            ADDRESS = address;
            __user_configurations = new Dictionary<string, string>(USER_FIELDS.Count);
            foreach (var k in USER_FIELDS)
                __user_configurations[k] = "";
            foreach (var k in users)
            {
                if (__user_configurations.ContainsKey(k.Item1) == false)
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_EXTENSION_MODULE_USER_FIELD, null);
                else if(__user_configurations[k.Item1] != "")
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.DUPLICATED_EXTENSION_MODULE_USERCONFIGURATION, null);
                __user_configurations[k.Item1] = k.Item2.Trim();
            }
        }

        public CONTROLLER_EXTENSION_MODULE_T(XmlNode node, ControllerModelCatalogue modelCatalogue)
        {
            ushort id = 0;
            string name;
            ushort bitSize = 0;
            try
            {
                if (node.NodeType != XmlNodeType.Element)
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.ELEMENT_MISSING, null);

                id = Convert.ToUInt16(node.SelectSingleNode("ID").FirstChild.Value, 16);
                name = node.SelectSingleNode("Name").FirstChild.Value;
                SWITCH = Convert.ToUInt32(node.SelectSingleNode("Switch").FirstChild.Value, 16);
                bitSize = Convert.ToUInt16(node.SelectSingleNode("BitSize").FirstChild.Value, 16);

                ADDRESS = Convert.ToUInt16(node.SelectSingleNode("Address").FirstChild.Value, 16);
                MODEL = __INQUIRE_EXTENSION_MODEL(modelCatalogue, id, name, bitSize);

                __user_configurations = new Dictionary<string, string>(USER_FIELDS.Count);
                foreach (var k in USER_FIELDS)
                {
                    XmlNode user = node[k];
                    if (user != null && user.NodeType == XmlNodeType.Element && user.FirstChild != null && user.FirstChild.Value != null)
                        __user_configurations[k] = user.FirstChild.Value.Trim();
                    else
                        __user_configurations[k] = "";
                }
            }
            catch (TaskUserParametersExcepetion)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public XmlElement ToXmlDoc(XmlDocument doc)
        {
            XmlElement moduleNode = doc.CreateElement("ExtensionModule");

            XmlElement sub = doc.CreateElement("ID");
            sub.AppendChild(doc.CreateTextNode($"0x{MODEL.ID:X4}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Name");
            sub.AppendChild(doc.CreateTextNode(MODEL.Name));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Switch");
            sub.AppendChild(doc.CreateTextNode($"0x{SWITCH:X8}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Address");
            sub.AppendChild(doc.CreateTextNode($"0x{ADDRESS:X4}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("BitSize");
            sub.AppendChild(doc.CreateTextNode($"0x{MODEL.BitSize:X4}"));
            moduleNode.AppendChild(sub);

            foreach (var c in USER_CONFIGURATIONS)
            {
                if (c.Value.Trim() != "")
                {
                    sub = doc.CreateElement(c.Key);
                    sub.AppendChild(doc.CreateTextNode(c.Value.Trim()));
                    moduleNode.AppendChild(sub);
                }
            }

            return moduleNode;
        }

        private static ControllerExtensionModel __INQUIRE_EXTENSION_MODEL(ControllerModelCatalogue catalogue, ushort id, string name, ushort bitSize)
        {
            bool res = catalogue.ExtensionModels.TryGetValue(id, out ControllerExtensionModel model);
            if (res == true && model.Name == name && model.BitSize == bitSize)
                return model;
            else
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_EXTENSION_MODULE_MODEL, null);
        }
    }

    public class CONTROLLER_ETHERNET_MODULE_T
    {
        public static IReadOnlyList<string> USER_FIELDS { get; private set; }

        public ControllerEthernetModel MODEL { get; private set; }
        public uint SWITCH { get; private set; }
        public ushort PORT { get; private set; }
        private string __ip_address;
        public string IP_ADDRESS
        {
            get { return __ip_address; }
            private set
            {
                if (TaskUserParametersHelper.VALID_IPV4_ADDRESS.IsMatch(value) != true)
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_IPV4_ADDRESS, null);
                __ip_address = value;
            }
        }

        public IReadOnlyDictionary<string, string> USER_CONFIGURATIONS { get { return __user_configurations; } }
        private Dictionary<string, string> __user_configurations;

        static CONTROLLER_ETHERNET_MODULE_T()
        {
            USER_FIELDS = new List<string>
            {
                "Timeout",
                "Timer",
                "Interval",
                "Priority",
                "InputDataOffset",
                "InputDataSize",
                "OutputDataOffset",
                "OutputDataSize"
            };
        }

        public CONTROLLER_ETHERNET_MODULE_T(ControllerEthernetModel model, uint sw, string ip, ushort port)
        {
            MODEL = model;
            SWITCH = sw;
            IP_ADDRESS = ip;
            PORT = port;

            __user_configurations = new Dictionary<string, string>(USER_FIELDS.Count);
            foreach (var k in USER_FIELDS)
                __user_configurations[k] = "";
        }

        public CONTROLLER_ETHERNET_MODULE_T(ControllerEthernetModel model, uint sw, string ip, ushort port, IEnumerable<Tuple<string, string>> users)
        {
            MODEL = model;
            SWITCH = sw;
            IP_ADDRESS = ip;
            PORT = port;
            __user_configurations = new Dictionary<string, string>(USER_FIELDS.Count);
            foreach (var k in USER_FIELDS)
                __user_configurations[k] = "";
            foreach (var k in users)
            {
                if (__user_configurations.ContainsKey(k.Item1) == false)
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_USER_FIELD, null);
                else if (__user_configurations[k.Item1] != "")
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.DUPLICATED_ETHERNET_MODULE_USERCONFIGURATION, null);
                __user_configurations[k.Item1] = k.Item2.Trim();
            }
        }

        public CONTROLLER_ETHERNET_MODULE_T(XmlNode node, ControllerModelCatalogue modelCatalogue)
        {
            ushort id = 0;
            string name;
            try
            {
                if (node.NodeType != XmlNodeType.Element)
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.ELEMENT_MISSING, null);

                id = Convert.ToUInt16(node.SelectSingleNode("ID").FirstChild.Value, 16);
                name = node.SelectSingleNode("Name").FirstChild.Value;
                SWITCH = Convert.ToUInt32(node.SelectSingleNode("Switch").FirstChild.Value, 16);

                IP_ADDRESS = node.SelectSingleNode("IP").FirstChild.Value;
                PORT = Convert.ToUInt16(node.SelectSingleNode("Port").FirstChild.Value, 10);
                MODEL = __INQUIRE_ETHERNET_MODEL(modelCatalogue, id, name);

                __user_configurations = new Dictionary<string, string>(USER_FIELDS.Count);
                foreach (var k in USER_FIELDS)
                {
                    XmlNode user = node[k];
                    if (user != null && user.NodeType == XmlNodeType.Element && user.FirstChild != null && user.FirstChild.Value != null)
                        __user_configurations[k] = user.FirstChild.Value.Trim();
                    else
                        __user_configurations[k] = "";
                }
            }
            catch (TaskUserParametersExcepetion)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public XmlElement ToXmlDoc(XmlDocument doc)
        {
            XmlElement moduleNode = doc.CreateElement("EthernetModule");

            XmlElement sub = doc.CreateElement("ID");
            sub.AppendChild(doc.CreateTextNode($"0x{MODEL.ID:X4}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Name");
            sub.AppendChild(doc.CreateTextNode(MODEL.Name));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Switch");
            sub.AppendChild(doc.CreateTextNode($"0x{SWITCH:X8}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("IP");
            sub.AppendChild(doc.CreateTextNode(IP_ADDRESS));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Port");
            sub.AppendChild(doc.CreateTextNode(PORT.ToString()));
            moduleNode.AppendChild(sub);

            foreach (var c in USER_CONFIGURATIONS)
            {
                if (c.Value.Trim() != "")
                {
                    sub = doc.CreateElement(c.Key);
                    sub.AppendChild(doc.CreateTextNode(c.Value.Trim()));
                    moduleNode.AppendChild(sub);
                }
            }

            return moduleNode;
        }

        private ControllerEthernetModel __INQUIRE_ETHERNET_MODEL(ControllerModelCatalogue catalogue, ushort id, string name)
        {
            bool res = catalogue.EthernetModels.TryGetValue(id, out ControllerEthernetModel model);
            if (res == true && model.Name == name)
                return model;
            else
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_MODEL, null);
        }
    }

}