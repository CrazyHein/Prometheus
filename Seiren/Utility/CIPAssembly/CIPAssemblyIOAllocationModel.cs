using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public class CIPAssemblyIOAllocationModel
    {
        private List<CIPAssemblyPDO> __cip_assembly_pdos;
        private List<CIPAssemblyIOInfo> __cip_assembly_ios;

        public IReadOnlyList<CIPAssemblyIOInfo> CIPAssemblyIOs { get { return __cip_assembly_ios; } }
        public string AInfoPath { get; private set; }
        public string IOTotalSizeMessage { get; private set; }

        public CIPAssemblyIOAllocationModel(string path)
        {
            XmlDocument eip = new XmlDocument();
            __cip_assembly_pdos = new List<CIPAssemblyPDO>();
            eip.Load(path);

            IOTotalSizeMessage = eip.SelectSingleNode("/IOAllocation/IOTotalSizeMessage").FirstChild.Value;
            var pdoNode = eip.SelectSingleNode("/IOAllocation/TxPdo");

            if (pdoNode != null && pdoNode.NodeType == XmlNodeType.Element)
                __cip_assembly_pdos.Add(__INITIALIZE_PDO_CONTENT(pdoNode));

            pdoNode = eip.SelectSingleNode("/IOAllocation/RxPdo");

            if (pdoNode != null && pdoNode.NodeType == XmlNodeType.Element)
                __cip_assembly_pdos.Add(__INITIALIZE_PDO_CONTENT(pdoNode));

            __cip_assembly_ios = new List<CIPAssemblyIOInfo>();
            foreach(var pdo in __cip_assembly_pdos)
            {
                foreach (var unit in pdo.Units)
                    foreach (var entry in unit.Entries)
                    {
                        __cip_assembly_ios.Add(new CIPAssemblyIOInfo()
                        {
                            PdoName = pdo.Name,
                            UnitNo = unit.UnitNo,
                            ModelName = unit.ModelName,
                            EntryName = entry.Name,
                            EntryBitOffset = entry.BitOffset,
                            EntryBitSize = entry.BitSize,
                        });

                        foreach (var subentry in entry.SubEntries)
                            __cip_assembly_ios.Add(new CIPAssemblyIOInfo()
                            {
                                PdoName = pdo.Name,
                                UnitNo = unit.UnitNo,
                                ModelName = unit.ModelName,
                                EntryName = entry.Name,
                                EntryBitOffset = entry.BitOffset,
                                EntryBitSize = entry.BitSize,
                                SubEntryName = subentry.Name,
                                SubEntryBitOffset = subentry.BitOffset,
                                SubEntryBitSize = subentry.BitSize
                            });
                    }
            }

            AInfoPath = path;
        }

        private static CIPAssemblyPDO __INITIALIZE_PDO_CONTENT(XmlNode pdoNode)
        {
            CIPAssemblyPDO pdo = new CIPAssemblyPDO() { Name = pdoNode.Attributes["Name"].Value, Units = new List<CIPAssemblyUnit>() };
            foreach (XmlNode unitNode in pdoNode.SelectNodes("Unit"))
            {
                CIPAssemblyUnit unit = new CIPAssemblyUnit()
                {
                    UnitNo = unitNode.Attributes["UnitNo"].Value,
                    ModelName = unitNode.Attributes["ModelName"].Value,
                    Entries = new List<CIPAssemblyEntry>()
                };
                
                foreach(XmlNode entryNode in unitNode.SelectNodes("Entry"))
                {
                    CIPAssemblyEntry entry = new CIPAssemblyEntry()
                    {
                        Name = entryNode.SelectSingleNode("Name").FirstChild.Value,
                        BitOffset = Convert.ToUInt32(entryNode.SelectSingleNode("BitOffset").FirstChild.Value),
                        BitSize = Convert.ToUInt32(entryNode.SelectSingleNode("Size").FirstChild.Value),
                        SubEntries = new List<CIPAssemblySubEntry>()
                    };

                    foreach (XmlNode subentryNode in entryNode.SelectNodes("SubEntry"))
                    {
                        CIPAssemblySubEntry subentry = new CIPAssemblySubEntry()
                        {
                            Name = subentryNode.SelectSingleNode("Name").FirstChild.Value,
                            BitOffset = Convert.ToUInt32(subentryNode.SelectSingleNode("BitOffset").FirstChild.Value),
                            BitSize = Convert.ToUInt32(subentryNode.SelectSingleNode("Size").FirstChild.Value)
                        };
                        entry.SubEntries.Add(subentry);
                    }

                    unit.Entries.Add(entry);
                }

                pdo.Units.Add(unit);
            }
            return pdo;
        }
    }

    public class CIPAssemblyIOInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void _notify_property_changed([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool __checked = false;
        public bool IsChecked
        {
            get { return __checked; }
            set { __checked = value; _notify_property_changed(); }
        }

        public string PdoName { get; init; }
        public string UnitNo { get; init; }
        public string ModelName { get; init; }
        public string UnitName { get { return $"{UnitNo} - {ModelName}"; } }
        public string EntryName { get; init; }
        public uint EntryBitOffset { get; init; }
        public uint EntryBitSize { get; init; }
        public string? SubEntryName { get; init; }
        public uint? SubEntryBitOffset { get; init; }
        public uint? SubEntryBitSize { get; init; }
    }

    public class CIPAssemblyPDO
    {
        public string Name { get; init; }
        public List<CIPAssemblyUnit> Units { get; init; }
    }

    public class CIPAssemblyUnit
    {
        public string UnitNo { get; init; }
        public string ModelName { get; init; }
        public List<CIPAssemblyEntry> Entries { get; init; }
    }

    public class CIPAssemblyEntry
    {
        public string Name { get; init; }
        public uint BitOffset { get; init; }
        public uint BitSize { get; init; }
        public uint ByteOffset { get { return BitOffset / 8; } }
        public List<CIPAssemblySubEntry> SubEntries{ get;  init; }
    }

    public class CIPAssemblySubEntry
    {
        public string Name { get; init; }
        public uint BitOffset { get; init; }
        public uint BitSize { get; init; }
        public uint ByteOffset { get { return BitOffset / 8; } }
    }

    public class CIPAssemblyIODataTypeConverter
    {
        public CIPAssemblyIODataTypeConverter(DataTypeCatalogue catalogue)
        {
            DefaultLombardiaDataType = catalogue.DataTypes.Values.FirstOrDefault(dt => dt.BitSize == 8);

            __lombardia_data_type_dictionary[1] = catalogue.DataTypes.Values.FirstOrDefault(dt => dt.BitSize == 1);
            __lombardia_data_type_dictionary[8] = catalogue.DataTypes.Values.FirstOrDefault(dt => dt.BitSize == 8);
            __lombardia_data_type_dictionary[16] = catalogue.DataTypes.Values.FirstOrDefault(dt => dt.BitSize == 16);
            __lombardia_data_type_dictionary[32] = catalogue.DataTypes.Values.FirstOrDefault(dt => dt.BitSize == 32);
            __lombardia_data_type_dictionary[64] = catalogue.DataTypes.Values.FirstOrDefault(dt => dt.BitSize == 64);
        }

        private Dictionary<uint, DataType> __lombardia_data_type_dictionary = new Dictionary<uint, DataType>();
        public IReadOnlyDictionary<uint, DataType> DataTypeDictionary { get { return __lombardia_data_type_dictionary; } }

        public DataType DefaultLombardiaDataType { get; init; }
    }
}
