using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public class ENIUtilityModel
    {
        private List<EtherCATSlave> __ethercat_slaves = null;
        private List<EtherCATVariableInfo> __ethercat_variables = null;

        public string ENIPath { get; private set; }
        public IReadOnlyList<EtherCATSlave> EtherCATSlaves { get { return __ethercat_slaves; } }
        public IReadOnlyList<EtherCATVariableInfo> EtherCATVariables { get { return __ethercat_variables; } }
        public ENIUtilityModel(string eniPath)
        {
            XmlDocument eni = new XmlDocument();
            List<EtherCATSlave> ethercatSlaves = new List<EtherCATSlave>();

            string slaveName = null;
            ushort slaveAddr = 0;
            uint globalTxOffset = 0;
            uint globalRxOffset = 0;

            eni.Load(eniPath);
            
            foreach (XmlNode slaveNode in eni.SelectNodes("/EtherCATConfig/Config/Slave"))
            {
                XmlNode slaveInfoNode;
                slaveName = slaveNode.SelectSingleNode("Info/Name").FirstChild.Value;
                slaveInfoNode = slaveNode.SelectSingleNode("Info/Identification");
                if (slaveInfoNode != null)
                {
                    slaveAddr = Convert.ToUInt16(slaveInfoNode.Attributes["Value"].Value);
                }
                else
                {
                    slaveInfoNode = slaveNode.SelectSingleNode("Info/PhysAddr");
                    slaveAddr = Convert.ToUInt16(slaveInfoNode.FirstChild.Value);
                }

                List<EtherCATSlavePDO> txPDOs = new List<EtherCATSlavePDO>();
                ushort localOffset = 0;
                uint globalOffset = globalTxOffset;
                foreach (XmlNode pdoNode in slaveNode.SelectNodes("ProcessData/TxPdo"))
                {
                    if (pdoNode.Attributes["Sm"] == null)
                        continue;
                    string pdoName = pdoNode.SelectSingleNode("Name").FirstChild.Value;
                    ushort pdoIndex = Convert.ToUInt16(pdoNode.SelectSingleNode("Index").FirstChild.Value.Substring(2), 16);
                    List < EtherCATSlaveVariable > variables = new List < EtherCATSlaveVariable >();
                    foreach (XmlNode varEntry in pdoNode.SelectNodes("Entry"))
                    {
                        ushort varSize = Convert.ToUInt16(varEntry.SelectSingleNode("BitLen").FirstChild.Value);
                        if (varEntry.SelectSingleNode("Name") != null)
                        {
                            string? varName = varEntry.SelectSingleNode("Name").FirstChild.Value;
                            string? varType = varEntry.SelectSingleNode("DataType").FirstChild.Value;
                            ushort varIndex = Convert.ToUInt16(varEntry.SelectSingleNode("Index").FirstChild.Value.Substring(2), 16);
                            byte varSubIndex = Convert.ToByte(varEntry.SelectSingleNode("SubIndex").FirstChild.Value, 10);

                            variables.Add(new EtherCATSlaveVariable() { Index = varIndex, SubIndex = varSubIndex, Name = varName, DataType = varType, BitSize = varSize, LocalBitOffset = localOffset, GlobalBitOffset = localOffset + globalOffset });
                        }
                        localOffset += varSize;
                    }
                    txPDOs.Add(new EtherCATSlavePDO() { Index = pdoIndex, Name = pdoName, Access = "Tx", Variables = variables });
                }

                List<EtherCATSlavePDO> rxPDOs = new List<EtherCATSlavePDO>();
                localOffset = 0;
                globalOffset = globalRxOffset;
                foreach (XmlNode pdoNode in slaveNode.SelectNodes("ProcessData/RxPdo"))
                {
                    if (pdoNode.Attributes["Sm"] == null)
                        continue;
                    string pdoName = pdoNode.SelectSingleNode("Name").FirstChild.Value;
                    ushort pdoIndex = Convert.ToUInt16(pdoNode.SelectSingleNode("Index").FirstChild.Value.Substring(2), 16);
                    List<EtherCATSlaveVariable> variables = new List<EtherCATSlaveVariable>();
                    foreach (XmlNode varEntry in pdoNode.SelectNodes("Entry"))
                    {
                        ushort varSize = Convert.ToUInt16(varEntry.SelectSingleNode("BitLen").FirstChild.Value);
                        if (varEntry.SelectSingleNode("Name") != null)
                        {
                            string? varName = varEntry.SelectSingleNode("Name").FirstChild.Value;
                            string? varType = varEntry.SelectSingleNode("DataType").FirstChild.Value;
                            ushort varIndex = Convert.ToUInt16(varEntry.SelectSingleNode("Index").FirstChild.Value.Substring(2), 16);
                            byte varSubIndex = Convert.ToByte(varEntry.SelectSingleNode("SubIndex").FirstChild.Value, 10);

                            variables.Add(new EtherCATSlaveVariable() { Index = varIndex, SubIndex = varSubIndex, Name = varName, DataType = varType, BitSize = varSize, LocalBitOffset = localOffset, GlobalBitOffset = localOffset + globalOffset });
                        }
                        localOffset += varSize;
                    }
                    rxPDOs.Add(new EtherCATSlavePDO() { Index = pdoIndex, Name = pdoName, Access = "Rx", Variables = variables });
                }

                ethercatSlaves.Add(new EtherCATSlave() { Addr = slaveAddr, Name = slaveName, TxPDOs = txPDOs, RxPDOs = rxPDOs, TxGlobalByteOffset = globalTxOffset/8, RxGlobalByteOffset = globalRxOffset/8});

                uint size = 0;
                if (slaveNode.SelectSingleNode("ProcessData/Recv/BitLength") != null)
                {
                    size = Convert.ToUInt32(slaveNode.SelectSingleNode("ProcessData/Recv/BitLength").FirstChild.Value);
                    globalTxOffset += (uint)(size / 16 + (size % 16 == 0 ? 0 : 1)) * 16;
                }
                if (slaveNode.SelectSingleNode("ProcessData/Send/BitLength") != null)
                {
                    size = Convert.ToUInt32(slaveNode.SelectSingleNode("ProcessData/Send/BitLength").FirstChild.Value);
                    globalRxOffset += (uint)(size / 16 + (size % 16 == 0 ? 0 : 1)) * 16;
                }
            }

            __ethercat_slaves = ethercatSlaves;

            __ethercat_variables = new List<EtherCATVariableInfo>();
            foreach (var slv in __ethercat_slaves)
            {
                foreach (var pdo in slv.TxPDOs.Concat(slv.RxPDOs))
                    foreach (var v in pdo.Variables)
                        __ethercat_variables.Add( new EtherCATVariableInfo()
                        {
                            SlaveAddr = slv.Addr,
                            SlaveName = slv.Name,
                            SlaveTxGlobalByteOffset = slv.TxGlobalByteOffset,
                            SlaveRxGlobalByteOffset = slv.RxGlobalByteOffset,
                            PDOIndex = pdo.Index,
                            PDOAccess = pdo.Access,
                            PDOName = pdo.Name,
                            VariableIndex = v.Index,
                            VariableSubIndex = v.SubIndex,
                            VariableName = v.Name,
                            VariableDataType = v.DataType,
                            VariableBitSize = v.BitSize,
                            VariableLocalBitOffset = v.LocalBitOffset,
                            VariableGlobalBitOffset = v.GlobalBitOffset,
                        });
            }

            ENIPath = eniPath;
        }
    }

    public class EtherCATVariableInfo: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void _notify_property_changed([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ushort SlaveAddr { get; init; }
        public string SlaveName { get; init; }
        public string SlaveFullName { get { return $"[{SlaveAddr:D05}][Tx:{SlaveTxGlobalByteOffset}, Rx:{SlaveRxGlobalByteOffset}] {SlaveName}"; } }
        public uint SlaveTxGlobalByteOffset { get; init; }
        public uint SlaveRxGlobalByteOffset { get; init; }
        public ushort PDOIndex { get; init; }
        public string PDOAccess { get; init; }
        public string PDOName { get; init; }
        public string PDOFullName { get { return $"[{PDOAccess}][{PDOIndex:X04}] {PDOName}"; } }
        public ushort VariableIndex { get; init; }
        public byte VariableSubIndex { get; init; }

        public string VariableFullIndex { get { return $"0x{VariableIndex:X4}:{VariableSubIndex:X2}"; } }
        public string VariableName { get; init; }
        public string VariableDataType { get; init; }
        public ushort VariableBitSize { get; init; }
        public ushort VariableLocalBitOffset { get; init; }
        public ushort VariableLocalByteOffset { get { return (ushort)(VariableLocalBitOffset / 8); } }
        public uint VariableGlobalBitOffset { get; init; }
        public uint VariableGlobalByteOffset { get { return (ushort)(VariableGlobalBitOffset / 8); } }

        private bool __checked = false;
        public bool IsChecked
        {
            get { return __checked; }
            set { __checked = value; _notify_property_changed(); }
        }
    }

    public class EtherCATSlave
    {
        public ushort Addr { get; init; }
        public string Name { get; init; }
        public List<EtherCATSlavePDO> TxPDOs { get; init; }
        public List<EtherCATSlavePDO> RxPDOs { get; init; }
        public uint TxGlobalByteOffset { get; init; }
        public uint RxGlobalByteOffset { get; init; }
        public override string ToString()
        {
            return $"[{Addr:D5}] {Name}";
        }
    }

    public class EtherCATSlavePDO
    {
        public ushort Index { get; init; }
        public string Name { get; init; }
        public string Access { get; init; }
        public List<EtherCATSlaveVariable> Variables { get; init; }

        public override string ToString()
        {
            return $"[0x{Index:X4}] [{Access}] {Name}";
        }
    }

    public class EtherCATSlaveVariable
    {
        public ushort Index { get; init; }
        public byte SubIndex { get; init; }
        public string Name { get; init; }
        public string DataType { get; init; }
        public ushort BitSize { get; init; }
        public ushort LocalBitOffset { get; init; }
        public ushort LocalByteOffset { get { return (ushort)(LocalBitOffset / 8); } }
        public uint GlobalBitOffset { get; init; }
        public uint GlobalByteOffset { get { return (ushort)(GlobalBitOffset / 8); } }
    }

    public class EtherCATVaribleDataTypeConverter
    {
        public EtherCATVaribleDataTypeConverter(DataTypeCatalogue catalogue) 
        {
            DefaultLombardiaDataType = catalogue.DataTypes.Values.FirstOrDefault();
            foreach (var dt in catalogue.DataTypes)
            {
                switch(dt.Key)
                {
                    case "BIT":
                        __lombardia_data_type_dictionary["BOOL"] = dt.Value; break;
                    case "BYTE":
                        __lombardia_data_type_dictionary["USINT"] = dt.Value;
                        DefaultLombardiaDataType = dt.Value;
                        break;
                    case "SBYTE":
                        __lombardia_data_type_dictionary["SINT"] = dt.Value; break;
                    case "USHORT":
                        __lombardia_data_type_dictionary["UINT"] = dt.Value; break;
                    case "SHORT":
                        __lombardia_data_type_dictionary["INT"] = dt.Value; break;
                    case "INT":
                        __lombardia_data_type_dictionary["DINT"] = dt.Value; break;
                    case "UINT":
                        __lombardia_data_type_dictionary["UDINT"] = dt.Value; break;
                    case "DINT":
                        __lombardia_data_type_dictionary["LINT"] = dt.Value; break;
                    case "UDINT":
                        __lombardia_data_type_dictionary["ULINT"] = dt.Value; break;
                    case "FLOAT":
                        __lombardia_data_type_dictionary["REAL"] = dt.Value; break;
                }
            }
        }

        private Dictionary<string, DataType> __lombardia_data_type_dictionary = new Dictionary<string, DataType>();

        public IReadOnlyDictionary<string, DataType> DataTypeDictionary { get { return __lombardia_data_type_dictionary; } }

        public DataType DefaultLombardiaDataType { get; init; }
    }
}
