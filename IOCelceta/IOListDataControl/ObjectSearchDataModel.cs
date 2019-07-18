using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public class ObjectSearchDataModel
    {
        public IReadOnlyList<ObjectItemDataModel> AvailableObjects { get; private set; }
        private List<ObjectItemDataModel> __available_objects;

        public ObjectSearchDataModel(PDOCollectionDataModel hostDataModel, ObjectItemDataModel dataModel)
        {
            __available_objects = new List<ObjectItemDataModel>(hostDataModel.DataHelper.IOObjectCollection.Count);
            foreach (var o in hostDataModel.DataHelper.IOObjectCollection)
            {
                IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T definition = new IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T()
                {
                    index = o.index,
                    binding = o.binding,
                    converter = o.converter,
                    data_type = o.data_type,
                    friendly_name = o.friendly_name
                };
                ObjectItemDataModel temp = new ObjectItemDataModel(definition);
                __available_objects.Add(temp);
                AvailableObjects = __available_objects;
            }
        }
    }
}
