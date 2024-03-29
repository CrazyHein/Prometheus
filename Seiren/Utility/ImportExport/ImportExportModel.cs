﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public class ImportExportPreference
    {
        public bool SeparateOutHardwareIntlk { get; init; } = false;
        public bool SeparateOutExclusiveIntlk { get; init; } = false;
        public string? XlsSheetProtectionPassword { get; init; } = null;
        public string HardwareIntlkSheetName { get; init; } = "Hardware";
        public string NonHardwareIntlkSheetName { get; init; } = "Software";
        public string ExclusiveIntlkSheetName { get; init; } = "Exclusive";
        public string NonExclusiveIntlkSheetName { get; init; } = "General";
    }
    internal class ImportExportModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private VariableDictionary __variable_dictionary;
        private ControllerConfiguration __controller_configuration;
        private ObjectDictionary __object_dictionary;
        private ProcessDataImage __tx_diagnotic_area;
        private ProcessDataImage __tx_bit_area;
        private ProcessDataImage __tx_block_area;
        private ProcessDataImage __rx_control_area;
        private ProcessDataImage __rx_bit_area;
        private ProcessDataImage __rx_block_area;
        private InterlockCollection __interlock_area;
        private Miscellaneous __misc_info;
        private IEnumerable<string> __variable_names;
        private IEnumerable<string> __configuration_names;
        private IEnumerable<uint> __object_indexes;
        private DataTypeCatalogue __data_type_catalogue;
        private ControllerModelCatalogue __controller_model_catalogue;
        private IOCelcetaHelper.WorksheetSelection __worksheet_selection = IOCelcetaHelper.WorksheetSelection.COMMON_USED_AREA;
        private ImportExportPreference __preference;
        public ImportExportModel(ImportExportMode mode,
            VariableDictionary vd, IEnumerable<string> variableNames, 
            ControllerConfiguration cc, IEnumerable<string> configurationNames, 
            ObjectDictionary od, IEnumerable<uint> objectIndexes,
            ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
            ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk,
            InterlockCollection intlk, Miscellaneous misc, 
            DataTypeCatalogue dataTypes, ControllerModelCatalogue models, ImportExportPreference? preference,
            string? currentlyOpenedFile)
        {
            Mode = mode;
            __variable_dictionary = vd;
            __controller_configuration = cc;
            __object_dictionary = od;
            __tx_diagnotic_area = txdiag;
            __tx_bit_area = txbit;
            __tx_block_area = txblk;
            __rx_control_area = rxctl;
            __rx_bit_area = rxbit;
            __rx_block_area = rxblk;
            __interlock_area = intlk;
            __misc_info = misc;
            __variable_names = variableNames;
            __configuration_names = configurationNames;
            __object_indexes = objectIndexes;
            __data_type_catalogue = dataTypes;
            __controller_model_catalogue = models;

            __preference = preference??new ImportExportPreference();

            if (currentlyOpenedFile == null || currentlyOpenedFile.Length == 0)
            {
                __variable_dictionary_path = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "variable_catalogue.xml");
                __io_list_path = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "io_list.xml");
                __xls_archives_path = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "io_list.xlsx");
            }
            else
            {
                string path = System.IO.Path.GetDirectoryName(currentlyOpenedFile);
                string file = System.IO.Path.GetFileNameWithoutExtension(currentlyOpenedFile);
                __variable_dictionary_path = System.IO.Path.Combine(path, "variable_catalogue.xml");
                __io_list_path = System.IO.Path.Combine(path, file + ".xml");
                __xls_archives_path = System.IO.Path.Combine(path, file + ".xlsx");
            }
        }

        public string Title
        {
            get; private set;
        }

        private string __variable_dictionary_path;// = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "variable_catalogue.xml");
        public string VariableDictionaryPath
        {
            get { return __variable_dictionary_path; }
            set { __variable_dictionary_path = value; OnPropertyChanged("VariableDictionaryPath"); }
        }
        private string __io_list_path;// = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "io_list.xml");
        public string IOListPath
        {
            get { return __io_list_path; }
            set { __io_list_path = value; OnPropertyChanged("IOListPath"); }
        }
        private string __xls_archives_path;// = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "io_list.xlsx");
        public string XlsArchivesPath
        {
            get { return __xls_archives_path; }
            set { __xls_archives_path = value; OnPropertyChanged("XlsArchivesPath"); }
        }
        private bool __busy = false;
        public bool IsBusy 
        {
            get { return __busy; }
            set { __busy = value; OnPropertyChanged("IsBusy"); }
        }
        public ImportExportMode Mode { get; private set; }

        public bool XML { get; set; } = true;
        public bool XLS { get; set; } = false;

        public bool XMLVAR { get; set; } = true;
        public bool XMLIO { get; set; } = true;

        public bool XlsExportVariableDictionary
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.VARIABLE_DICTIONARY) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.VARIABLE_DICTIONARY;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.VARIABLE_DICTIONARY;
            }
        }
        public bool XlsExportControllerConfiguration
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.CONTROLLER_CONFIGURATION) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.CONTROLLER_CONFIGURATION;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.CONTROLLER_CONFIGURATION;
            }
        }
        public bool XlsExportObjectDictionary
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.OBJECT_DICTIONARY) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.OBJECT_DICTIONARY;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.OBJECT_DICTIONARY;
            }
        }
        public bool XlsExportTxDiagnosticArea
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.TX_DIAGNOSTIC_AREA) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.TX_DIAGNOSTIC_AREA;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.TX_DIAGNOSTIC_AREA;
            }
        }
        public bool XlsExportTxBitArea
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.TX_BIT_AREA) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.TX_BIT_AREA;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.TX_BIT_AREA;
            }
        }
        public bool XlsExportTxBlockArea
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.TX_BLOCK_AREA) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.TX_BLOCK_AREA;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.TX_BLOCK_AREA;
            }
        }
        public bool XlsExportRxControlArea
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.RX_CONTROL_AREA) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.RX_CONTROL_AREA;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.RX_CONTROL_AREA;
            }
        }
        public bool XlsExportRxBitlArea
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.RX_BIT_AREA) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.RX_BIT_AREA;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.RX_BIT_AREA;
            }
        }
        public bool XlsExportRxBlocklArea
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.RX_BLOCK_AREA) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.RX_BLOCK_AREA;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.RX_BLOCK_AREA;
            }
        }
        public bool XlsExportInterlockArea
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.INTERLOCK_AREA) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.INTERLOCK_AREA;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.INTERLOCK_AREA;
            }
        }
        public bool XlsExportMiscArea
        {
            get { return (__worksheet_selection & IOCelcetaHelper.WorksheetSelection.MISCELLANEOUS_AREA) != 0; }
            set
            {
                if (value)
                    __worksheet_selection |= IOCelcetaHelper.WorksheetSelection.MISCELLANEOUS_AREA;
                else
                    __worksheet_selection &= ~IOCelcetaHelper.WorksheetSelection.MISCELLANEOUS_AREA;
            }
        }

        public void Export()
        {
            Debug.Assert(Mode == ImportExportMode.Export);
            if(XML)
            {
                if (XMLVAR)
                    IOCelcetaHelper.Export(__variable_dictionary_path, true, __variable_dictionary, __variable_names);
                if (XMLIO)
                    IOCelcetaHelper.Export(__io_list_path,__controller_configuration, __configuration_names,
                        __object_dictionary, __object_indexes,
                        __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                        __rx_control_area, __rx_bit_area, __rx_block_area,
                        __interlock_area, __misc_info);
            }
                
            if (XLS)
            {
                //var pass = System.Security.Cryptography.SHA256.Create();
                //var bytes = pass.ComputeHash(BitConverter.GetBytes((new object()).GetHashCode()));
                //pass.Dispose();

                List<Tuple<Func<uint, bool>, string>>? names = null;
                if(__preference.SeparateOutHardwareIntlk == true && __preference.SeparateOutExclusiveIntlk == false)
                {
                    names = new List<Tuple<Func<uint, bool>, string>>
                    {
                        new Tuple<Func<uint, bool>, string>(v => (v & (uint)InterlockAttribute.Hardware) == 0,
                            __preference.NonHardwareIntlkSheetName + " Intlks"),
                        new Tuple<Func<uint, bool>, string>(v => (v & (uint)InterlockAttribute.Hardware) != 0,
                            __preference.HardwareIntlkSheetName + " Intlks")
                    };
                }
                else if(__preference.SeparateOutHardwareIntlk == false && __preference.SeparateOutExclusiveIntlk == true)
                {
                    names = new List<Tuple<Func<uint, bool>, string>>
                    {
                        new Tuple<Func<uint, bool>, string>(v => (v & (uint)InterlockAttribute.Exclusive) == 0,
                            __preference.NonExclusiveIntlkSheetName + " Intlks"),
                        new Tuple<Func<uint, bool>, string>(v => (v & (uint)InterlockAttribute.Exclusive) != 0,
                            __preference.ExclusiveIntlkSheetName + " Intlks")
                    };
                }
                else if (__preference.SeparateOutHardwareIntlk == true && __preference.SeparateOutExclusiveIntlk == true)
                {
                    names = new List<Tuple<Func<uint, bool>, string>>
                    {
                        new Tuple<Func<uint, bool>, string>(v => ((v & (uint)InterlockAttribute.Hardware) == 0) && ((v & (uint)InterlockAttribute.Exclusive) == 0),
                            string.Join(" ", __preference.NonHardwareIntlkSheetName, __preference.NonExclusiveIntlkSheetName, "Intlks")),
                        new Tuple<Func<uint, bool>, string>(v => ((v & (uint)InterlockAttribute.Hardware) != 0) && ((v & (uint)InterlockAttribute.Exclusive) == 0),
                            string.Join(" ", __preference.HardwareIntlkSheetName, __preference.NonExclusiveIntlkSheetName, "Intlks")),
                        new Tuple<Func<uint, bool>, string>(v => ((v & (uint)InterlockAttribute.Hardware) == 0) && ((v & (uint)InterlockAttribute.Exclusive) != 0),
                            string.Join(" ", __preference.NonHardwareIntlkSheetName, __preference.ExclusiveIntlkSheetName, "Intlks")),
                        new Tuple<Func<uint, bool>, string>(v => ((v & (uint)InterlockAttribute.Hardware) != 0) && ((v & (uint)InterlockAttribute.Exclusive) != 0),
                            string.Join(" ", __preference.HardwareIntlkSheetName, __preference.ExclusiveIntlkSheetName, "Intlks"))
                    };
                }
                else
                {
                    names = new List<Tuple<Func<uint, bool>, string>>
                    {
                        new Tuple<Func<uint, bool>, string>(v => true,
                            "Legacy Intlk Table"),
                    };
                }

                //IOCelcetaHelper.Export(__xls_archives_path, null, null, bytes[7].ToString("X2") + bytes[15].ToString("X2") + bytes[23].ToString("X2") + bytes[31].ToString("X2"),
                IOCelcetaHelper.Export(__xls_archives_path, null, null, __preference.XlsSheetProtectionPassword,
                    __variable_dictionary, __variable_names,
                    __controller_configuration, __configuration_names,
                    __object_dictionary, __object_indexes,
                    __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info, __worksheet_selection,
                    InterlockLogicModel.AttributeValue, names);
            }
        }

        public (VariableDictionary, ControllerConfiguration, ObjectDictionary,
                    ProcessDataImage, ProcessDataImage, ProcessDataImage,
                    ProcessDataImage, ProcessDataImage, ProcessDataImage, InterlockCollection,
                    Miscellaneous) Import()
        {
            Debug.Assert(Mode == ImportExportMode.Import || Mode == ImportExportMode.Compare);
            Debug.Assert(XML == true && XLS == false);
            if (XML)
            {
                VariableDictionary vd;
                ControllerConfiguration cc;
                ObjectDictionary od;
                ProcessDataImage txdiag, txbit, txblk, rxctl, rxbit, rxblk;
                InterlockCollection intlk;
                Miscellaneous misc;
                if (XMLVAR)
                {
                    vd = IOCelcetaHelper.Import(__variable_dictionary_path, __data_type_catalogue, out _);

                    if (XMLIO)
                        (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc) = IOCelcetaHelper.Import(__io_list_path, vd, __data_type_catalogue, __controller_model_catalogue, out _);
                    else
                        (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc) = IOCelcetaHelper.Default(__data_type_catalogue, __controller_model_catalogue, vd);
                    return (vd, cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);
                }
                else
                    throw new NotImplementedException("<Variable Dictionary> must be selected.");
            }
            throw new NotImplementedException("Only XML files are supported.");
        }
    }

    public enum ImportExportMode
    {
        Import,
        Export,
        Compare
    }
}
