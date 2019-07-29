using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public class InterlockCollectionDataModel : IOListDataModel
    {
        private ObjectCollectionDataModel __object_collection_data_model;
        private ObservableCollection<InterlocklogicLoop> __interlock_logic_loops;

        public InterlockCollectionDataModel(IOListDataHelper helper, ObjectCollectionDataModel objectCollectionDataModel) : base(helper)
        {
            __object_collection_data_model = objectCollectionDataModel;
            __interlock_logic_loops = new ObservableCollection<InterlocklogicLoop>();
        }

        public override void UpdateDataHelper()
        {
            
        }

        public override void UpdateDataModel()
        {
            foreach(var loop in _data_helper.InterlockLoops)
            {
                ObjectItemDataModel objectDataModel = __object_collection_data_model.ObjectDictionary[loop.target_object.index];

                InterlockLogicExpression expressionDataModel =
                    __load_interlock_logic_statement(loop.statement, null) as InterlockLogicExpression;

                __interlock_logic_loops.Add(new InterlocklogicLoop(loop.name, objectDataModel, expressionDataModel));
            }
        }

        private InterlockLogicElement __load_interlock_logic_statement(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_ELEMEMT_T element,
            InterlockLogicExpression root)
        {
            if(element.type == IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.OPERAND)
            {
                IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_OPERAND_T operand = element as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_OPERAND_T;
                return new InterlockLogicOperand(__object_collection_data_model.ObjectDictionary[operand.Operand.index], root);
            }
            else
            {
                IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T expression = element as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T;
                InterlockLogicExpression expressionDataModel = new InterlockLogicExpression(expression.logic_operator, root);
                foreach (var e in expression.elements)
                    expressionDataModel.elements.Add(__load_interlock_logic_statement(e, expressionDataModel));
                return expressionDataModel;
            }
        }
    }

    public abstract class InterlockLogicElement
    {
        public IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE Type { get; private set; }
        public InterlockLogicExpression Root { get; private set; }
        public int Layer { get; private set; }
        public InterlockLogicElement(IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE type, InterlockLogicExpression root)
        {
            Type = type;
            Root = root;
            if (root == null)
                Layer = 0;
            else
                Layer = root.Layer + 1;
        }
    }

    public class InterlockLogicOperand : InterlockLogicElement
    {
        public ObjectItemDataModel ObjectDataModel { get; private set; }
        public InterlockLogicOperand(ObjectItemDataModel objectDataModel, InterlockLogicExpression root) :base(IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.OPERAND, root)
        {
            ObjectDataModel = objectDataModel;
        }
    }

    public class InterlockLogicExpression : InterlockLogicElement
    {
        public IO_LIST_INTERLOCK_LOGIC_OPERATOR_T LogicOperator { get; private set; }
        public ObservableCollection<InterlockLogicElement> elements { get; private set; }

        public InterlockLogicExpression(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T op, InterlockLogicExpression root):base(IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.EXPRESSION, root)
        {
            LogicOperator = op;
            elements = new ObservableCollection<InterlockLogicElement>();
        }
    }

    public class InterlocklogicLoop
    {
        public string Name { get; private set; }
        public ObjectItemDataModel TargetObjectDataModel { get; private set; }
        public InterlockLogicExpression Statement { get; private set; }

        public InterlocklogicLoop(string name, ObjectItemDataModel target, InterlockLogicExpression statement)
        {
            Name = name;
            TargetObjectDataModel = target;
            Statement = statement;
        }
    }
}
