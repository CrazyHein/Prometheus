using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class ShaderModel : Component
    {
        public ProcessShader Shader { get; private set; }
        private ProcessShader __shader;

        public ShaderModel(ProcessShader shader)
        {
            __name = shader.Name.Trim();
            __leftvalue = shader.Shader.Operand.ToString() ?? "@0x00000000";
            __rightvalue = shader.Shader.Expr.ToString();
            Shader = shader;
            __shader = shader;
        }

        public ShaderModel()
        {
            __name = "unnamed_shader";
            __leftvalue = "@0x00000000";
            __rightvalue = "0";
            Shader = new ProcessShader(__name, new Shader(__leftvalue, __rightvalue));
            __shader = Shader;
        }

        public ShaderModel(JsonObject node)
        {
            Shader = new ProcessShader(node["SOURCE"]["NAME"].GetValue<string>(), new Shader(node["SOURCE"]));
            __name = Shader.Name;
            __leftvalue = Shader.Shader.Operand.ToString() ?? "@0x00000000"; ;
            __rightvalue = Shader.Shader.Expr.ToString();
            __shader = Shader;
        }

        public override string Header => throw new NotImplementedException();
        public override BitmapImage ImageIcon => throw new NotImplementedException();
        public override void SubComponentChangesApplied(Component sub)
        {
            throw new NotSupportedException();
        }
        public override void SubComponentChangesHappened(Component sub)
        {
            throw new NotSupportedException();
        }
        protected override void RollbackChanges()
        {
            throw new NotSupportedException();
        }

        public override JsonNode ToJson()
        {
            //if (Modified)
            //throw new InvalidOperationException("Unapplied changes detected.");
            var json = new JsonObject();
            json["VERSION"] = Settings.ArkVersion;
            json["ASSEMBLY"] = this.GetType().FullName;
            json["SOURCE"] = ToShader().ToJson();
            return json;
        }

        public ProcessShader ToShader()
        {
            var s = new UserProcessShaders([(__name, __leftvalue, __rightvalue)]);
            return s[0];
        }

        public override string Summary
        {
            get
            {
                string summary;
                try
                {
                    summary = ToJson().ToString();
                }
                catch (Exception ex)
                {
                    summary = ex.ToString();
                }
                return summary;
            }
        }

        public override void ApplyChanges()
        {
            if (Modified)
            {
                var s = new UserProcessShaders([(__name, __leftvalue, __rightvalue)]);
                Shader = s[0];
                base.ApplyChanges();
            }
        }

        public override void DiscardChanges()
        {
            Name = Shader.Name;
            LeftValue = Shader.Shader.Operand.ToString();
            RightValue = Shader.Shader.Expr.ToString();
            base.DiscardChanges();
        }

        private string __name;
        public string Name
        {
            get { return __name; }
            set
            {
                __name = value.Trim();
                _notify_property_changed();
                _reload_property("Representation");
                _reload_property("Summary");
            }
        }

        private string __leftvalue;
        public string LeftValue
        {
            get { return __leftvalue; } 
            set { __leftvalue = value.Trim(); 
                if(ObjectReference.PATTERN.IsMatch(__leftvalue))
                {
                    __leftvalue = new ObjectReference(__leftvalue).ToString();
                }
                _notify_property_changed();
                _reload_property("Representation");
                _reload_property("IsValid");
                _reload_property("IsObjectDirectAssignment");
                _reload_property("CanBeOmitted");
                //EvaluateSubsequentShadersOmissible();
                _reload_property("Summary");
            }
        }

        private string __rightvalue;
        public string RightValue
        {
            get { return __rightvalue; }
            set { __rightvalue = value.Trim(); 
                _notify_property_changed();
                _reload_property("Representation");
                _reload_property("IsValid");
                _reload_property("IsObjectDirectAssignment");
                _reload_property("CanBeOmitted");
                //EvaluateSubsequentShadersOmissible();
                _reload_property("Summary");
            }
        }

        public bool IsObjectDirectAssignment
        {
            get
            {
                return IsValid && __shader.Shader.Operand is ObjectReference && __shader.Shader.Expr.IsImmediateOperand;
            }
        }

        public bool IsValid
        {
            get
            {
                if(Modified)
                {
                    try
                    {
                        __shader = new UserProcessShaders([(__name, __leftvalue, __rightvalue)])[0];
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                    return true;
            }
        }

        public string Representation 
        { 
            get 
            {
                return $"{Name}: {ContextModel.PROCESS_DATA_INDEX_TO_TAG(__leftvalue)} := {ContextModel.PROCESS_DATA_INDEX_TO_TAG(__rightvalue)}";
            } 
        }

        public bool CanBeOmitted
        {
            get
            {
                bool omissible = false;
                if (IsObjectDirectAssignment)
                {
                    var step = Owner as SimpleStepModel;
                    var seq = step.Owner as SequentialModel;
                    int stepPos = seq.SubSteps.IndexOf(step);
                    int abortShaderPos = step.AbortShaders.IndexOf(this);
                    int breakShaderPos = step.BreakShaders.IndexOf(this);
                    int continueShaderPos = step.ContinueShaders.IndexOf(this);
                    int postShaderPos = step.PostShaders.IndexOf(this);
                    int shaderPos = step.Shaders.IndexOf(this);
                    if(breakShaderPos != -1)
                    {
                        if (step.BreakShaders.TakeLast(step.BreakShaders.Count() - breakShaderPos - 1).Any(x => x.IsObjectDirectAssignment && x.LeftValue == LeftValue))
                            omissible = true;
                        else
                        {
                            var range = seq.SubSteps.Cast<SimpleStepModel>().Take(stepPos).SelectMany(x => x.Shaders.Concat(x.PostShaders)).Concat(step.Shaders).Where(x => x.IsObjectDirectAssignment).Reverse();
                            var ret = range.FirstOrDefault(x => x.LeftValue == LeftValue);
                            if (ret != null && ret.__shader.Shader.Expr.Equals(__shader.Shader.Expr))
                                omissible = true;
                            else
                                omissible = false;
                        }
                    }
                    else if(continueShaderPos != -1)
                    {
                        if (step.ContinueShaders.TakeLast(step.ContinueShaders.Count() - continueShaderPos - 1).Any(x => x.IsObjectDirectAssignment && x.LeftValue == LeftValue))
                            omissible = true;
                        else
                        {
                            var range = seq.SubSteps.Cast<SimpleStepModel>().Take(stepPos).SelectMany(x => x.Shaders.Concat(x.PostShaders)).Concat(step.Shaders).Where(x => x.IsObjectDirectAssignment).Reverse();
                            var ret = range.FirstOrDefault(x => x.LeftValue == LeftValue);
                            if (ret != null && ret.__shader.Shader.Expr.Equals(__shader.Shader.Expr))
                                omissible = true;
                            else
                                omissible = false;
                        }
                    }
                    else if (abortShaderPos != -1)
                    {
                        if (step.AbortShaders.TakeLast(step.AbortShaders.Count() - abortShaderPos - 1).Any(x => x.IsObjectDirectAssignment && x.LeftValue == LeftValue))
                            omissible = true;
                        else
                        {
                            var range = seq.SubSteps.Cast<SimpleStepModel>().Take(stepPos).SelectMany(x => x.Shaders.Concat(x.PostShaders)).Concat(step.Shaders).Where(x => x.IsObjectDirectAssignment).Reverse();
                            var ret = range.FirstOrDefault(x => x.LeftValue == LeftValue);
                            if (ret != null && ret.__shader.Shader.Expr.Equals(__shader.Shader.Expr))
                                omissible = true;
                            else
                                omissible = false;
                        }
                    }
                    else if (postShaderPos != -1)
                    {
                        if (step.PostShaders.TakeLast(step.PostShaders.Count() - postShaderPos - 1).Any(x => x.IsObjectDirectAssignment && x.LeftValue == LeftValue))
                            omissible = true;
                        else
                        {
                            var range = seq.SubSteps.Cast<SimpleStepModel>().Take(stepPos).SelectMany(x => x.Shaders.Concat(x.PostShaders)).Concat(step.Shaders).Where(x => x.IsObjectDirectAssignment).Reverse();
                            var ret = range.FirstOrDefault(x => x.LeftValue == LeftValue);
                            if (ret != null && ret.__shader.Shader.Expr.Equals(__shader.Shader.Expr))
                                omissible = true;
                            else
                                omissible = false;
                        }
                    }
                    else if(shaderPos != -1)
                    {
                        if (step.Shaders.TakeLast(step.Shaders.Count() - shaderPos - 1).Any(x => x.IsObjectDirectAssignment && x.LeftValue == LeftValue))
                            omissible = true;
                        else
                        {
                            var range = seq.SubSteps.Cast<SimpleStepModel>().Take(stepPos).SelectMany(x => x.Shaders.Concat(x.PostShaders)).Where(x => x.IsObjectDirectAssignment).Reverse();
                            var ret = range.FirstOrDefault(x => x.LeftValue == LeftValue);
                            if (ret != null && ret.__shader.Shader.Expr.Equals(__shader.Shader.Expr))
                                omissible = true;
                            else
                                omissible = false;
                        }
                    }
                    else
                        omissible = false;
                }
                else
                    omissible = false;
                return omissible;
            }
        }

        public void EvaluateOmissible()
        {
            _reload_property("CanBeOmitted");
        }
    }
}
