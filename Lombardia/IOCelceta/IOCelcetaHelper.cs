using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class IOCelcetaHelper
    {
        public static uint SupportedFileFormatVersion { get; private set; } = 1;
        public static uint SupportedVariableFileFormatVersion { get; private set; } = VariableDictionary.SupportedFileFormatVersion;
        public static uint SupportedIOFileFormatVersion { get; private set; } = 1;

        public static VariableDictionary Import(string variableDictionary, DataTypeCatalogue dataTypes, out byte[] md5code)
        {
            XmlDocument xmlDoc = new XmlDocument();
            return new VariableDictionary(dataTypes, variableDictionary, out md5code);
        }

        public static (ControllerConfiguration, ObjectDictionary,
            ProcessDataImage, ProcessDataImage, ProcessDataImage,
            ProcessDataImage, ProcessDataImage, ProcessDataImage, InterlockCollection,
            Miscellaneous) Import(string iolist, VariableDictionary variables,
                                    DataTypeCatalogue dataTypes, ControllerModelCatalogue models,
                                    out byte[] md5code)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                using (FileStream stream = File.Open(iolist, FileMode.Open))
                using (MD5 hash = MD5.Create())
                {
                    md5code = hash.ComputeHash(stream);
                    stream.Position = 0;
                    xmlDoc.Load(stream);
                }
                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECIOList");
                if (SupportedFileFormatVersion < uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value))
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION);
                var (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc) = __load_io_list(rootNode, variables, dataTypes, models);
                txbit.Associated = rxbit;
                rxbit.Associated = txbit;
                return (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);
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

        public static VariableDictionary Import(Stream variableDictionary, DataTypeCatalogue dataTypes)
        {
            XmlDocument xmlDoc = new XmlDocument();
            return new VariableDictionary(dataTypes, variableDictionary);
        }

        public static (ControllerConfiguration, ObjectDictionary,
            ProcessDataImage, ProcessDataImage, ProcessDataImage,
            ProcessDataImage, ProcessDataImage, ProcessDataImage, InterlockCollection,
            Miscellaneous) Import(Stream iolist, VariableDictionary variables,
                                    DataTypeCatalogue dataTypes, ControllerModelCatalogue models)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(iolist);
                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECIOList");
                if (SupportedFileFormatVersion < uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value))
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION);
                var (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc) = __load_io_list(rootNode, variables, dataTypes, models);
                txbit.Associated = rxbit;
                rxbit.Associated = txbit;
                return (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);
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

        public static (VariableDictionary, ControllerConfiguration, ObjectDictionary,
            ProcessDataImage, ProcessDataImage, ProcessDataImage,
            ProcessDataImage, ProcessDataImage, ProcessDataImage, InterlockCollection,
            Miscellaneous) Load(string path, 
            DataTypeCatalogue dataTypes, ControllerModelCatalogue models, out byte[] md5code)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                using FileStream stream = File.Open(path, FileMode.Open);
                using (MD5 hash = MD5.Create())
                {
                    md5code = hash.ComputeHash(stream);
                    stream.Position = 0;
                    xmlDoc.Load(stream);
                }

                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECIOList");
                if (SupportedFileFormatVersion < uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value))
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION);

                XmlNode infoNode = xmlDoc.SelectSingleNode("/AMECIOList/AMECVariables");
                VariableDictionary variables = new VariableDictionary(dataTypes, infoNode);

                var (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc) = __load_io_list(rootNode, variables, dataTypes, models);
                txbit.Associated = rxbit;
                rxbit.Associated = txbit;
                return (variables, cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);
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

        public static (VariableDictionary, ControllerConfiguration, ObjectDictionary,
            ProcessDataImage, ProcessDataImage, ProcessDataImage,
            ProcessDataImage, ProcessDataImage, ProcessDataImage, InterlockCollection,
            Miscellaneous) Default(DataTypeCatalogue dataTypes, ControllerModelCatalogue models)
        {
            Miscellaneous misc = new Miscellaneous();
            VariableDictionary variables = new VariableDictionary(dataTypes);
            ControllerConfiguration cc = new ControllerConfiguration(models);
            ObjectDictionary od = new ObjectDictionary(variables, cc);
            Dictionary<ProcessObject, ProcessData> hash = new Dictionary<ProcessObject, ProcessData>();
            ProcessDataImage txdiag = new ProcessDataImage(od, hash, ProcessDataImageLayout.System, ProcessDataImageAccess.TX);
            ProcessDataImage txbit = new ProcessDataImage(od, hash, ProcessDataImageLayout.Bit, ProcessDataImageAccess.TX);
            ProcessDataImage txblk = new ProcessDataImage(od, hash, ProcessDataImageLayout.Block, ProcessDataImageAccess.TX);
            ProcessDataImage rxctl = new ProcessDataImage(od, hash, ProcessDataImageLayout.System, ProcessDataImageAccess.RX);
            ProcessDataImage rxbit = new ProcessDataImage(od, hash, ProcessDataImageLayout.Bit, ProcessDataImageAccess.RX);
            ProcessDataImage rxblk = new ProcessDataImage(od, hash, ProcessDataImageLayout.Block, ProcessDataImageAccess.RX);
            InterlockCollection intlk = new InterlockCollection(od, txbit, rxbit);
            txbit.Associated = rxbit;
            rxbit.Associated = txbit;
            return (variables, cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);
        }

        public static byte[] Save(string path,
            VariableDictionary variables, IEnumerable<string> variableNames,
            ControllerConfiguration cc, IEnumerable<string> configurationNames,
            ObjectDictionary od, IEnumerable<uint> objectIndexes,
            ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
            ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk, InterlockCollection intlk,
            Miscellaneous misc)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(decl);

                XmlElement root = xmlDoc.CreateElement("AMECIOList");
                root.SetAttribute("FormatVersion", SupportedFileFormatVersion.ToString());
                xmlDoc.AppendChild(root);

                root.AppendChild(xmlDoc.CreateElement("AMECVariables"));
                variables.Save(xmlDoc, variableNames, xmlDoc.SelectSingleNode("/AMECIOList/AMECVariables") as XmlElement);

                __save_io_list(xmlDoc, root, cc, configurationNames, od, objectIndexes, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);

                xmlDoc.Save(path);
                using (MD5 hash = MD5.Create())
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    return hash.ComputeHash(stream);
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

        public static byte[] Export(string variableDictionary, bool enableSerialNumber, VariableDictionary variables, IEnumerable<string> variableNames)
        {
            return variables.Save(variableDictionary, variableNames, enableSerialNumber);
        }

        public static void Export(Stream variableDictionary, bool enableSerialNumber, VariableDictionary variables, IEnumerable<string> variableNames)
        {
            variables.Save(variableDictionary, variableNames, enableSerialNumber);
        }

        public static byte[] Export(string iolist,
            ControllerConfiguration cc, IEnumerable<string> configurationNames,
            ObjectDictionary od, IEnumerable<uint> objectIndexes,
            ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
            ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk, InterlockCollection intlk,
            Miscellaneous misc)
        {
            byte[] i = null;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(decl);

                XmlElement root = xmlDoc.CreateElement("AMECIOList");
                root.SetAttribute("FormatVersion", SupportedIOFileFormatVersion.ToString());
                xmlDoc.AppendChild(root);

                __save_io_list(xmlDoc, root, cc, configurationNames, od, objectIndexes, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);

                xmlDoc.Save(iolist);
                using (MD5 hash = MD5.Create())
                using (FileStream stream = File.Open(iolist, FileMode.Open))
                {
                    i = hash.ComputeHash(stream);
                }
                return i;
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

        public static void Export(Stream iolist,
            ControllerConfiguration cc, IEnumerable<string> configurationNames,
            ObjectDictionary od, IEnumerable<uint> objectIndexes,
            ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
            ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk, InterlockCollection intlk,
            Miscellaneous misc)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(decl);

                XmlElement root = xmlDoc.CreateElement("AMECIOList");
                root.SetAttribute("FormatVersion", SupportedIOFileFormatVersion.ToString());
                xmlDoc.AppendChild(root);

                __save_io_list(xmlDoc, root, cc, configurationNames, od, objectIndexes, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);

                xmlDoc.Save(iolist);
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

        public static void Export(string path, string workbookOpenProtectionPassword,  string workbookWriteProtectionPassword, string sheetWriteProtectionPassword,
            VariableDictionary variables, IEnumerable<string> variableNames,
            ControllerConfiguration cc, IEnumerable<string> configurationNames,
            ObjectDictionary od, IEnumerable<uint> objectIndexes,
            ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
            ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk, InterlockCollection intlk,
            Miscellaneous misc)
        {
            bool res = IOCelcetaHelper.OVERLAP_DETECTOR(new List<(uint, uint)>()
            {
                (txdiag.OffsetInWord, txdiag.SizeInWord),
                (txbit.OffsetInWord, txbit.SizeInWord),
                (txblk.OffsetInWord, txblk.SizeInWord),
                (rxctl.OffsetInWord, rxctl.SizeInWord),
                (rxbit.OffsetInWord, rxbit.SizeInWord),
                (rxblk.OffsetInWord, rxblk.SizeInWord)
            });
            if (res)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_IMAGE_ADDRESS_OVERLAPPING);
            try
            {
                Workbook xlsxWorkbook = new Workbook();
                xlsxWorkbook.Protect(workbookOpenProtectionPassword, true, true);
                if (workbookWriteProtectionPassword != null && workbookWriteProtectionPassword != "")
                    xlsxWorkbook.SetWriteProtectionPassword(workbookWriteProtectionPassword);
                CellStyle title = xlsxWorkbook.Styles.Add("Title");
                title.Font.IsItalic = true;
                title.Font.IsBold = true;
                title.VerticalAlignment = VerticalAlignType.Center;
                title.HorizontalAlignment = HorizontalAlignType.Left;

                CellStyle content = xlsxWorkbook.Styles.Add("Content");
                content.VerticalAlignment = VerticalAlignType.Center;
                content.WrapText = true;
                content.HorizontalAlignment = HorizontalAlignType.Left;

                xlsxWorkbook.Worksheets.Clear();
                Worksheet sheet = xlsxWorkbook.Worksheets.Add("Variable Dictionary");
                variables.Save(sheet, title, content, variableNames);
                sheet = xlsxWorkbook.Worksheets.Add("Controller Configuration");
                cc.Save(sheet, title, content, configurationNames);
                sheet = xlsxWorkbook.Worksheets.Add("Object Dictionary");
                od.Save(sheet, title, content, objectIndexes);
                sheet = xlsxWorkbook.Worksheets.Add("Tx Diagnotic Area");
                txdiag.Save(sheet, title, content);
                sheet = xlsxWorkbook.Worksheets.Add("Tx Bit Area");
                txbit.Save(sheet, title, content);
                sheet = xlsxWorkbook.Worksheets.Add("Tx Block Area");
                txblk.Save(sheet, title, content);
                sheet = xlsxWorkbook.Worksheets.Add("Rx Control Area");
                rxctl.Save(sheet, title, content);
                sheet = xlsxWorkbook.Worksheets.Add("Rx Bit Area");
                rxbit.Save(sheet, title, content);
                sheet = xlsxWorkbook.Worksheets.Add("Rx Block Area");
                rxblk.Save(sheet, title, content);
                sheet = xlsxWorkbook.Worksheets.Add("Interlock Logic Area");
                intlk.Save(sheet, title, content);
                sheet = xlsxWorkbook.Worksheets.Add("Miscellaneous");
                misc.Save(sheet, title, content);

                if (sheetWriteProtectionPassword != null && sheetWriteProtectionPassword != "")
                    foreach (var s in xlsxWorkbook.Worksheets)
                        s.Protect(sheetWriteProtectionPassword, SheetProtectionType.LockedCells | SheetProtectionType.UnLockedCells);
                xlsxWorkbook.SaveToFile(path, FileFormat.Version2007);
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

        public static bool OVERLAP_DETECTOR(List<ValueTuple<uint, uint>> ranges)
        {
            for (int i = 0; i < ranges.Count - 1; ++i)
            {
                for (int j = 0; j < ranges.Count - 1 - i; ++j)
                {
                    if (ranges[j].Item1 > ranges[j + 1].Item1)
                    {
                        ValueTuple<uint, uint> temp = ranges[j];
                        ranges[j] = ranges[j + 1];
                        ranges[j + 1] = temp;
                    }
                }
            }

            for (int i = 0; i < ranges.Count - 1; ++i)
            {
                if (ranges[i].Item1 + ranges[i].Item2 > ranges[i + 1].Item1)
                    return true;
            }

            return false;
        }

        private static (ControllerConfiguration, ObjectDictionary,
            ProcessDataImage, ProcessDataImage, ProcessDataImage,
            ProcessDataImage, ProcessDataImage, ProcessDataImage, InterlockCollection,
            Miscellaneous) __load_io_list(XmlNode root, 
            VariableDictionary variables, DataTypeCatalogue dataTypes, ControllerModelCatalogue models)
        {
            Miscellaneous misc = new Miscellaneous(root.SelectSingleNode("TargetInfo"), root.SelectSingleNode("/AMECIOList/ControllerInfo/MCServer"));

            XmlNode infoNode = root.SelectSingleNode("ControllerInfo");
            ControllerConfiguration cc = new ControllerConfiguration(models, infoNode);

            infoNode = root.SelectSingleNode("Objects");
            ObjectDictionary od = new ObjectDictionary(variables, cc, infoNode);

            Dictionary<ProcessObject, ProcessData> hash = new Dictionary<ProcessObject, ProcessData>();

            infoNode = root.SelectSingleNode("TxPDO/DiagArea");
            ProcessDataImage txdiag = new ProcessDataImage(od, hash, ProcessDataImageLayout.System, ProcessDataImageAccess.TX, infoNode);

            infoNode = root.SelectSingleNode("TxPDO/BitArea");
            ProcessDataImage txbit = new ProcessDataImage(od, hash, ProcessDataImageLayout.Bit, ProcessDataImageAccess.TX, infoNode);

            infoNode = root.SelectSingleNode("TxPDO/BlockArea");
            ProcessDataImage txblk = new ProcessDataImage(od, hash, ProcessDataImageLayout.Block, ProcessDataImageAccess.TX, infoNode);

            infoNode = root.SelectSingleNode("RxPDO/ControlArea");
            ProcessDataImage rxctl = new ProcessDataImage(od, hash, ProcessDataImageLayout.System, ProcessDataImageAccess.RX, infoNode);

            infoNode = root.SelectSingleNode("RxPDO/BitArea");
            ProcessDataImage rxbit = new ProcessDataImage(od, hash, ProcessDataImageLayout.Bit, ProcessDataImageAccess.RX, infoNode);

            infoNode = root.SelectSingleNode("RxPDO/BlockArea");
            ProcessDataImage rxblk = new ProcessDataImage(od, hash, ProcessDataImageLayout.Block, ProcessDataImageAccess.RX, infoNode);

            infoNode = root.SelectSingleNode("Interlocks");
            InterlockCollection intlk = new InterlockCollection(od, txbit, rxbit, infoNode);

            return (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);
        }

        private static void __save_io_list(XmlDocument xmlDoc, XmlElement root,
            ControllerConfiguration cc, IEnumerable<string> configurationNames,
            ObjectDictionary od, IEnumerable<uint> objectIndexes,
            ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
            ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk, InterlockCollection intlk,
            Miscellaneous misc)
        {
            bool res = IOCelcetaHelper.OVERLAP_DETECTOR(new List<(uint, uint)>()
            {
                (txdiag.OffsetInWord, txdiag.SizeInWord),
                (txbit.OffsetInWord, txbit.SizeInWord),
                (txblk.OffsetInWord, txblk.SizeInWord),
                (rxctl.OffsetInWord, rxctl.SizeInWord),
                (rxbit.OffsetInWord, rxbit.SizeInWord),
                (rxblk.OffsetInWord, rxblk.SizeInWord)
            });
            if (res)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_IMAGE_ADDRESS_OVERLAPPING);

            root.AppendChild(xmlDoc.CreateElement("TargetInfo"));
            root.AppendChild(xmlDoc.CreateElement("ControllerInfo"));
            root.AppendChild(xmlDoc.CreateElement("Objects"));
            root.SelectSingleNode("ControllerInfo").AppendChild(xmlDoc.CreateElement("MCServer"));

            misc.Save(xmlDoc, root.SelectSingleNode("TargetInfo") as XmlElement,
                root.SelectSingleNode("ControllerInfo/MCServer") as XmlElement);
            cc.Save(xmlDoc, configurationNames, root.SelectSingleNode("ControllerInfo") as XmlElement);
            od.Save(xmlDoc, objectIndexes, root.SelectSingleNode("Objects") as XmlElement);

            root.AppendChild(xmlDoc.CreateElement("TxPDO"));
            root.SelectSingleNode("TxPDO").AppendChild(xmlDoc.CreateElement("DiagArea"));
            root.SelectSingleNode("TxPDO").AppendChild(xmlDoc.CreateElement("BitArea"));
            root.SelectSingleNode("TxPDO").AppendChild(xmlDoc.CreateElement("BlockArea"));
            root.AppendChild(xmlDoc.CreateElement("RxPDO"));
            root.SelectSingleNode("RxPDO").AppendChild(xmlDoc.CreateElement("ControlArea"));
            root.SelectSingleNode("RxPDO").AppendChild(xmlDoc.CreateElement("BitArea"));
            root.SelectSingleNode("RxPDO").AppendChild(xmlDoc.CreateElement("BlockArea"));

            txdiag.Save(xmlDoc, root.SelectSingleNode("TxPDO/DiagArea") as XmlElement);
            txbit.Save(xmlDoc, root.SelectSingleNode("TxPDO/BitArea") as XmlElement);
            txblk.Save(xmlDoc, root.SelectSingleNode("TxPDO/BlockArea") as XmlElement);
            rxctl.Save(xmlDoc, root.SelectSingleNode("RxPDO/ControlArea") as XmlElement);
            rxbit.Save(xmlDoc, root.SelectSingleNode("RxPDO/BitArea") as XmlElement);
            rxblk.Save(xmlDoc, root.SelectSingleNode("RxPDO/BlockArea") as XmlElement);

            root.AppendChild(xmlDoc.CreateElement("Interlocks"));

            intlk.Save(xmlDoc, root.SelectSingleNode("Interlocks") as XmlElement);
        }
    }
}
