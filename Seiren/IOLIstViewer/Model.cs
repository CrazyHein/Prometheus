using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using Spire.Pdf.Widget;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public abstract class ContainerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return;
            storage = value;
            if (propertyName != "Modified")
                Modified = true;
            OnPropertyChanged(propertyName);
        }

        private bool __modified;
        public bool Modified
        {
            get { return __modified; }
            protected set { SetProperty(ref __modified, value); }
        }

        public virtual void CommitChanges()
        {
            Modified = false;
        }
        public string Name { get; init; } = "Unnamed Container";
    }

    public abstract class RecordContainerModel : ContainerModel
    {
        public abstract void Undo(OperatingRecord r);
        public abstract void Redo(OperatingRecord r);
        public OperatingHistory OperatingHistory { get; init; }
    }

    public interface ISerializableRecordModel
    {
        public XmlElement ToXml(XmlDocument doc);
    }

    public interface IDeSerializableRecordModel<T>
    {
        public T? FromXml(XmlNode node);
    }

    public class RecordUtility
    {
        public static Exception? CopyRecords<T>(IEnumerable<T> records) where T : ISerializableRecordModel
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(decl);

                XmlElement root = xmlDoc.CreateElement(typeof(T).FullName + "-" + Settings.SeirenVersion);

                foreach (var model in records)
                    root.AppendChild(model.ToXml(xmlDoc));

                xmlDoc.AppendChild(root);

                StringBuilder sb = new StringBuilder();
                StringWriter writer = new StringWriter(sb);
                xmlDoc.Save(writer);

                Clipboard.SetDataObject(sb.ToString());
                DebugConsole.WriteInfo($"{root.ChildNodes.Count} {typeof(T).Name}(s) copied");
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static List<R> PasteAllRecords<C, R>(C Container) where C : IDeSerializableRecordModel<R>
        {
            List<R> records = new List<R>();
            try
            {
                if (Clipboard.ContainsText())
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(Clipboard.GetText());

                    XmlNode rootNode = xmlDoc.SelectSingleNode("/" + typeof(R).FullName + "-" + Settings.SeirenVersion);

                    foreach (XmlNode varNode in rootNode?.ChildNodes)
                    {
                        var r = Container.FromXml(varNode);
                        
                        if (r != null)
                            records.Add(r);
                        else
                        {
                            DebugConsole.WriteException($"Cannot deserialize record in clipboard to {typeof(R).Name}");
                            records.Clear();
                            return records;
                        }
                    }
                }
                DebugConsole.WriteInfo($"Pasted {records.Count} {typeof(R).Name}(s) from clipboard");
                return records;
            }
            catch (Exception)
            {
                records.Clear();
                DebugConsole.WriteInfo($"Cannot find any {typeof(R).Name} in clipboard");
                return records;
            }
        }

        public static R? DefaultRecord<C, R>(C Container) where C : IDeSerializableRecordModel<R>
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(Clipboard.GetText());

                    XmlNode rootNode = xmlDoc.SelectSingleNode("/" + typeof(R).FullName + "-" + Settings.SeirenVersion);
                    foreach (XmlNode varNode in rootNode?.ChildNodes)
                        return Container.FromXml(varNode);
                    return default(R);
                }
                else
                    return default(R);
            }
            catch
            {
                return default(R);
            }
        }
    }
}
