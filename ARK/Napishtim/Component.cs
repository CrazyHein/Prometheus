using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public abstract class Component : INotifyPropertyChanged, ISerializableElementModel
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool __component_virgin = true;
        protected void _notify_property_changed([CallerMemberName] string? propertyName = null)
        {
            //if(propertyName == "Modified" || propertyName == "Validated")
            //return;
            if (_modified == false)
            {
                _modified = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Modified"));
                if (_validated == true)
                    __component_virgin = false;
                _validated = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Validated"));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (Owner != null)
                Owner.SubComponentChangesHappened(this);
        }

        protected void _reload_property(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract string Summary { get; }

        private Component? __owner = null;
        public required Component? Owner
        {
            get { return __owner; }
            set
            {
                if (value != null)
                {
                    if (__owner != null)
                        throw new ArgumentException($"The Component already has an owner.");
                }
                __owner = value;
            }
        }
        public abstract void SubComponentChangesApplied(Component sub);
        public abstract void SubComponentChangesHappened(Component sub);

        public abstract JsonNode ToJson();

        protected bool _modified = false;
        public bool Modified
        {
            get { return _modified; }
        }

        protected bool _validated = false;
        public bool Validated
        {
            get { return _validated; }
        }

        public virtual void ApplyChanges()
        {
            if (_modified)
            {
                _modified = false;
                _validated = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Modified"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Validated"));

                try
                {
                    if (Owner != null)
                        Owner.SubComponentChangesApplied(this);
                }
                catch
                {
                    RollbackChanges();
                    _modified = true;
                    _validated = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Modified"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Validated"));
                    throw;
                }
            }
        }

        protected abstract void RollbackChanges();

        public virtual void DiscardChanges()
        {
            if (_modified)
            {
                _modified = false;
                _validated = __component_virgin ? false : true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Modified"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Validated"));
            }
        }

        public abstract string Header { get; }
        public abstract BitmapImage ImageIcon { get; }


        //private delegate ControlBlockModel CONTROL_BLOCK_MAKER(ControlBlockModelCollection collection, JsonNode seqNode);
        //private static SortedDictionary<string, CONTROL_BLOCK_MAKER> __CONTROL_BLOCK_MAKERS;

        public static (Type type, JsonNode node)? COMPONENT_IN_CLIPBOARD()
        {
            Type type = null;
            JsonNode node = null;
            if (Clipboard.ContainsText())
            {
                try
                {
                    node = JsonNode.Parse(Clipboard.GetText());
                    if (node.GetValueKind() != System.Text.Json.JsonValueKind.Object)
                        return null;
                    type = Type.GetType(node["ASSEMBLY"].GetValue<string>());
                    if (node["VERSION"].GetValue<string>() != Settings.ArkVersion || type.IsSubclassOf(typeof(Component)) == false)
                        return null;
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
            return (type, node);
        }

        public static (Type type, JsonNode node)? COMPONENT_ARRAY_IN_CLIPBOARD()
        {
            Type type = null;
            JsonNode node = null;
            if (Clipboard.ContainsText())
            {
                try
                {
                    node = JsonNode.Parse(Clipboard.GetText());
                    if (node.GetValueKind() != System.Text.Json.JsonValueKind.Array)
                        return null;
                    if (node.AsArray().Count == 0)
                        return null;
                    foreach (var o in node.AsArray())
                    {
                        if (o.GetValueKind() != System.Text.Json.JsonValueKind.Object)
                            return null;
                        if (type == null)
                            type = Type.GetType(o["ASSEMBLY"].GetValue<string>());
                        else if (type != Type.GetType(o["ASSEMBLY"].GetValue<string>()))
                            return null;
                        if (o["VERSION"].GetValue<string>() != Settings.ArkVersion || type.IsSubclassOf(typeof(Component)) == false)
                            return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
            return (type!, node);
        }
        public static ControlBlockModel? BUILD_CONTROL_BLOCK_FROM_CLIPBOARD(ControlBlockModelCollection collection)
        {
            var ret = COMPONENT_IN_CLIPBOARD();
            if (ret.HasValue)
            {
                if (ret.Value.type.IsSubclassOf(typeof(ControlBlockModel)))
                {
                    var blk_s = ControlBlockSource.MAKE_BLK(ret.Value.node["SOURCE"].AsObject(), null);
                    return ControlBlockModel.MAKE_CONTROL_BLOCK(collection, blk_s, null);
                }
                else
                    return null;
            }
            else
                return null;
        }
    }

    public interface ISerializableElementModel
    {
        public JsonNode ToJson();
    }

    public abstract class ComponentCollection<T> : INotifyPropertyChanged where T : Component
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void _notify_property_changed([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool __dirty = false;
        public bool IsDirty
        {
            get { return __dirty; }
            set
            {
                if (__dirty != value)
                {
                    __dirty = value;
                    _notify_property_changed();
                }
            }
        }

        public abstract IEnumerable<T> Components { get; }
        public abstract string Summary { get; }
    }
}
