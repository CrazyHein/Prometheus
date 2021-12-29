using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class ControllerModelCatalogue
    {
        public IReadOnlyDictionary<ushort, LocalExtensionModel> LocalExtensionModels { get; private set; }
        public IReadOnlyDictionary<ushort, RemoteEthernetModel> RemoteEthernetModels { get; private set; }
        public uint FileFormatVersion { get; private set; }
        private readonly uint __supported_file_format_version;
        private byte[] __md5;
        public byte[] MD5Code
        {
            get
            {
                byte[] code = new byte[__md5.Length];
                __md5.CopyTo(code, 0);
                return code;
            }
        }

        public ControllerModelCatalogue(string catalogueConfiguration)
        {
            __supported_file_format_version = 1;
            (LocalExtensionModels, RemoteEthernetModels) = __load(catalogueConfiguration, out __md5);
        }

        private (IReadOnlyDictionary<ushort, LocalExtensionModel>, IReadOnlyDictionary<ushort, RemoteEthernetModel>) __load(string catalogueConfiguration, out byte[] code)
        {
            XmlDocument xmlDoc = new XmlDocument();
            ushort id = 0;
            string name = String.Empty;
            ushort bitSize = 0;
            Dictionary<string, uint> txVariables;
            Dictionary<string, uint> rxVariables;
            try
            {
                using FileStream stream = File.Open(catalogueConfiguration, FileMode.Open);
                using (MD5 hash = MD5.Create())
                {
                    code = hash.ComputeHash(stream);
                    stream.Position = 0;
                    xmlDoc.Load(stream);
                }

                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECControllerModels");
                FileFormatVersion = uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value);
                if (__supported_file_format_version < FileFormatVersion)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION);
            }
            catch (LombardiaException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LombardiaException(e);
            }

            Dictionary<ushort, LocalExtensionModel> localExtensionModels = new Dictionary<ushort, LocalExtensionModel>();
            try
            {
                XmlNode extensionModelsNode = xmlDoc.SelectSingleNode("/AMECControllerModels/ExtensionModels");
                if (extensionModelsNode != null && extensionModelsNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModelNode in extensionModelsNode.ChildNodes)
                    {
                        if (extensionModelNode.NodeType != XmlNodeType.Element || extensionModelNode.Name != "ExtensionModel")
                            continue;

                        id = Convert.ToUInt16(extensionModelNode.SelectSingleNode("ID").FirstChild.Value, 16);
                        if (localExtensionModels.ContainsKey(id))
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_MODEL_ID);

                        txVariables = new Dictionary<string, uint>();
                        rxVariables = new Dictionary<string, uint>();
                        name = extensionModelNode.SelectSingleNode("Name").FirstChild.Value.Trim();
                        bitSize = Convert.ToUInt16(extensionModelNode.SelectSingleNode("BitSize").FirstChild.Value, 16);
                        XmlNode x = extensionModelNode["TX"];
                        if (x != null)
                        {
                            foreach (XmlNode tx in x.ChildNodes)
                            {
                                if (tx.NodeType != XmlNodeType.Element)
                                    continue;
                                txVariables.Add(tx.Name.Trim(), Convert.ToUInt32(tx.FirstChild.Value, 10));
                            }
                        }
                        x = extensionModelNode["RX"];
                        if (x != null)
                        {
                            foreach (XmlNode rx in x.ChildNodes)
                            {
                                if (rx.NodeType != XmlNodeType.Element)
                                    continue;
                                rxVariables.Add(rx.Name.Trim(), Convert.ToUInt32(rx.FirstChild.Value, 10));
                            }
                        }
                        localExtensionModels.Add(id, new LocalExtensionModel { ID = id, Name = name, BitSize = bitSize, TxVariables = txVariables, RxVariables = rxVariables });
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

            Dictionary<ushort, RemoteEthernetModel> remoteEthernetModels = new Dictionary<ushort, RemoteEthernetModel>();
            try
            {
                XmlNode ethernetModelsNode = xmlDoc.SelectSingleNode("/AMECControllerModels/EthernetModels");

                if (ethernetModelsNode != null && ethernetModelsNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModelNode in ethernetModelsNode.ChildNodes)
                    {
                        if (extensionModelNode.NodeType != XmlNodeType.Element || extensionModelNode.Name != "EthernetModel")
                            continue;

                        id = Convert.ToUInt16(extensionModelNode.SelectSingleNode("ID").FirstChild.Value, 16);
                        if (remoteEthernetModels.ContainsKey(id))
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_MODEL_ID);

                        txVariables = new Dictionary<string, uint>();
                        rxVariables = new Dictionary<string, uint>();
                        name = extensionModelNode.SelectSingleNode("Name").FirstChild.Value.Trim();
                        XmlNode x = extensionModelNode["TX"];
                        if (x != null)
                        {
                            foreach (XmlNode tx in x.ChildNodes)
                            {
                                if (tx.NodeType != XmlNodeType.Element)
                                    continue;
                                txVariables.Add(tx.Name.Trim(), Convert.ToUInt32(tx.FirstChild.Value, 10));
                            }
                        }
                        x = extensionModelNode["RX"];
                        if (x != null)
                        {
                            foreach (XmlNode rx in x.ChildNodes)
                            {
                                if (rx.NodeType != XmlNodeType.Element)
                                    continue;
                                rxVariables.Add(rx.Name.Trim(), Convert.ToUInt32(rx.FirstChild.Value, 10));
                            }
                        }
                        remoteEthernetModels.Add(id, new RemoteEthernetModel { ID = id, Name = name, TxVariables = txVariables, RxVariables = rxVariables });
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
            return (localExtensionModels, remoteEthernetModels);
        }
    }

    public abstract class DeviceModel
    {
        public ushort ID { get; init; }
        public string Name { get; init; } = "unnamed";
        public IReadOnlyDictionary<string, uint> TxVariables { get; init; } = new Dictionary<string, uint>();
        public IReadOnlyDictionary<string, uint> RxVariables { get; init; } = new Dictionary<string, uint>();
        public override string ToString()
        {
            return Name + " [ 0x" + ID.ToString("X04") + " ]";
        }
    }

    public class LocalExtensionModel : DeviceModel
    {
        public ushort BitSize { get; init; }
    }

    public class RemoteEthernetModel : DeviceModel
    {

    }
}
