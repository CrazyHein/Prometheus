using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe
{
    public class Context
    {
        //public static readonly uint DefaultStepCapacity = 4096;
        //public static readonly uint DefaultUserVariableCapacity = 500;
        //public uint StepCapacity { get; set; }

        //private uint __reserved_user_variable_capacity;
        /*
        public uint ReservedUserVariableCapacity 
        {
            get { return __reserved_user_variable_capacity; }
            set
            {
                if (value > UserVariableCapacity)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The max capacity of reserved user variable is {UserVariableCapacity}.");
                __reserved_user_variable_capacity = value;
            }
        }
        */
        public static readonly int UserVariableCapacity = 1000;

        public Context()
        {
            //StepCapacity = Context.DefaultStepCapacity;
            //ReservedUserVariableCapacity = Context.DefaultUserVariableCapacity;
        }

        //public Context(uint stepCapacity)
        //{
            //StepCapacity = stepCapacity;
            //ReservedUserVariableCapacity = reservedUserVariableCapacity;
        //}

        private static void SearchEngineInterface(ProcessDataImage diag, ProcessDataImage txbit, ProcessDataImage txblk, ProcessDataImage ctrl, ProcessDataImage rxbit, ProcessDataImage rxblk,
            out ProcessData controlRequest, out ProcessData initialStep, out ProcessData controlResponse, out ProcessData controlResult, out ProcessData millisecondTick, out ProcessData currentStep,
            out ProcessData state, out ProcessData userReturnCode, out ProcessData stepBeginPoint, out ProcessData stepDurationTime)
        {
            try
            {
                controlRequest = ctrl.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine Control Request" && p.ProcessObject.Variable.Type.Name == "UINT");
                initialStep = ctrl.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine Initial Step" && p.ProcessObject.Variable.Type.Name == "UINT");
                controlResponse = diag.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine Control Response" && p.ProcessObject.Variable.Type.Name == "UINT");
                controlResult = diag.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine Control Result" && p.ProcessObject.Variable.Type.Name == "INT");
                millisecondTick = diag.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine Millisecond Tick" && p.ProcessObject.Variable.Type.Name == "UINT");
                currentStep = diag.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine Current Step" && p.ProcessObject.Variable.Type.Name == "UINT");
                state = diag.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine State" && p.ProcessObject.Variable.Type.Name == "UINT");
                userReturnCode = diag.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine User Return Code" && p.ProcessObject.Variable.Type.Name == "INT");
                stepBeginPoint = diag.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine Step Begin Point" && p.ProcessObject.Variable.Type.Name == "UINT");
                stepDurationTime = diag.ProcessDatas.Single(p => p.ProcessObject.Variable.Name == "Recipe Engine Step Duration Time" && p.ProcessObject.Variable.Type.Name == "UINT");
            }
            catch (Exception)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, "Please map all required process data objects to activate the recipe engine and map them only once.");
            }
        }

        public static ProcessData SearchObject(uint index, ProcessDataImage diag, ProcessDataImage txbit, ProcessDataImage txblk, ProcessDataImage ctrl, ProcessDataImage rxbit, ProcessDataImage rxblk)
        {
            try
            {
                return diag.ProcessDatas.Concat(txbit.ProcessDatas).Concat(txblk.ProcessDatas).Concat(ctrl.ProcessDatas).Concat(rxbit.ProcessDatas).Concat(rxblk.ProcessDatas).First(x => x.ProcessObject.Index == index);
            }
            catch (Exception)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"Can not find ProcessData with the specified object index {index}.");
            }
        }
    }
}
