using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class Miscellaneous : IComparable<Miscellaneous>
    {
        public Miscellaneous()
        {

        }

        public Miscellaneous(XmlNode infoNode, XmlNode mcserverNode)
        {
            try
            {
                if (infoNode?.NodeType == XmlNodeType.Element)
                {
                    IOListName = infoNode.SelectSingleNode("Name").FirstChild?.Value;
                    Description = infoNode.SelectSingleNode("Description").FirstChild?.Value;
                    VariableDictionary = infoNode.SelectSingleNode("VariableDictionary")?.FirstChild?.Value;
                }
                else
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_BASIC_INFO_ELEMENT_NODE);

                if (mcserverNode?.NodeType == XmlNodeType.Element)
                {
                    MCServerIPv4 = mcserverNode.SelectSingleNode("IP").FirstChild.Value;
                    MCServerPort = Convert.ToUInt16(mcserverNode.SelectSingleNode("Port").FirstChild?.Value, 10);
                }
                else
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_BASIC_INFO_ELEMENT_NODE);
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

        private string __io_list_name = "unnamed";
        public string IOListName 
        {
            get { return __io_list_name; }
            set 
            {
                if (value == null)
                    value = String.Empty;
                __io_list_name = value.Trim(); 
            } 
        }
        private string __description = String.Empty;
        public string Description 
        {
            get { return __description; }
            set
            {
                if (value == null)
                    value = String.Empty;
                __description = value.Trim();
            }
        }
        private string? __variable_dictionary = "variable_catalogue.xml";
        public string? VariableDictionary
        {
            get { return __variable_dictionary; }
            set 
            {
                if(value == null)
                    value = String.Empty;
                value = value.Trim();
                if (value.Length != 0 && value.EndsWith(".xml") == false)
                    value += ".xml";
                __variable_dictionary = value;
            }
        }

        private string __mc_server_ipv4 = "127.0.0.1";
        public string MCServerIPv4
        {
            get { return __mc_server_ipv4;}
            set
            {
                if (value == null || Helper.VALID_IPV4_ADDRESS.IsMatch(value.Trim()) == false)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_MC_SERVER_IPV4_ADDRESS);
                __mc_server_ipv4 = value.Trim();
            }
        }

        public ushort MCServerPort { get; set; } = 5010;

        public void Save(XmlDocument doc, XmlElement targetInfo, XmlElement mcserver, uint version = 1)
        {
            try
            {
                XmlElement sub = doc.CreateElement("Name");
                sub.AppendChild(doc.CreateTextNode(IOListName));
                targetInfo.AppendChild(sub);

                sub = doc.CreateElement("Description");
                sub.AppendChild(doc.CreateTextNode(Description));
                targetInfo.AppendChild(sub);

                sub = doc.CreateElement("VariableDictionary");
                sub.AppendChild(doc.CreateTextNode(VariableDictionary));
                targetInfo.AppendChild(sub);

                sub = doc.CreateElement("IP");
                sub.AppendChild(doc.CreateTextNode(MCServerIPv4));
                mcserver.AppendChild(sub);

                sub = doc.CreateElement("Port");
                sub.AppendChild(doc.CreateTextNode(MCServerPort.ToString()));
                mcserver.AppendChild(sub);
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public void Save(Worksheet sheet, CellStyle title, CellStyle content, uint version = 1)
        {
            try
            {
                sheet.Range[1, 1].Text = "Basic Information";
                sheet.Range[1, 1].Style = title;
                sheet.Range[1, 2].Text = "IO List Name";
                sheet.Range[1, 2].Style = title;
                sheet.Range[2, 2].Text = "Description";
                sheet.Range[2, 2].Style = title;

                sheet.Range[1, 3].Text = __io_list_name;
                sheet.Range[1, 3].Style = content;
                sheet.Range[2, 3].Text = Description;
                sheet.Range[2, 3].Style = content;

                sheet.Range[3, 1].Text = "MC Server Information";
                sheet.Range[3, 1].Style = title;
                sheet.Range[3, 2].Text = "IPv4";
                sheet.Range[3, 2].Style = title;
                sheet.Range[4, 2].Text = "Port";
                sheet.Range[4, 2].Style = title;


                sheet.Range[3, 3].Text = MCServerIPv4;
                sheet.Range[3, 3].Style = content;
                sheet.Range[4, 3].Text = MCServerPort.ToString();
                sheet.Range[4, 3].Style = content;

                sheet.AllocatedRange.AutoFitColumns();
                sheet.AllocatedRange.AutoFitRows();
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public bool IsEquivalent(Miscellaneous? other)
        {
            return other != null && other.Description == Description && 
                other.IOListName == IOListName && 
                other.MCServerIPv4 == MCServerIPv4 && other.MCServerPort == MCServerPort &&
                other.VariableDictionary == VariableDictionary;
        }
    }
}
