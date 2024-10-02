
using SMM.Helper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
//using static SMM.Helper.CredentialsUI;

namespace SMM.Automation
{
    public class SimpleScriptRunner : IDisposable
    {
        #region fields

        private object instanceLock = new object();
        private PowerShell currentPowerShell;
        private List<ProxyVar> proxyVariables = new List<ProxyVar>();
        internal string _source;

        #endregion  // fields

        #region constructor

        /// <summary>
        /// Constructs a script runner that can be used for execution of simple PowerShell fragements.
        /// </summary>
        /// <param name="ScriptSource"></param>
        public SimpleScriptRunner(string ScriptSource)
        {
            currentPowerShell = PowerShell.Create();
            _source = ScriptSource;
            Script = ScriptBlock.Create(ScriptSource);
        }

        ~SimpleScriptRunner()
        {
            // Dispose the PowerShell object and set currentPowerShell to null.
            // It is locked because currentPowerShell may be accessed by the
            // ctrl-C handler.
            this.Dispose();
        }

        public void Dispose()
        {
            currentPowerShell?.Stop();
            currentPowerShell?.Runspace?.Dispose();
            currentPowerShell?.Dispose();
            currentPowerShell = null;
        }

        #endregion  // constructor

        #region properties

        public int ExitCode { get; private set; } = 0;

        public bool HadErrors => currentPowerShell?.HadErrors ?? false;

        public Exception LastError  {
            get {
                var errors = currentPowerShell.Runspace.SessionStateProxy.GetVariable("Error");
                if (errors is ArrayList a)
                {
                    foreach (var e in a)
                    {
                        if (e is ErrorRecord err)
                        {
                            return err.Exception;
                        }
                        break;
                    }
                }

                return null;
            }
        }

        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        private PSDataCollection<PSObject> _results = new PSDataCollection<PSObject>();
        public PSDataCollection<PSObject> Results { get; set; }

        public ObservableCollection<string> Console { get; set; } = new ObservableCollection<string>();

        public ScriptBlock Script { get; private set; }

        public NetworkCredential Credential { get; set; }

        #endregion  // properties

        #region methods

        private void CheckScript()
        {
            if (Script?.ToString() != _source)
            {
                Script = ScriptBlock.Create(_source);
            }
        }

        public ParamBlockAst GetParamBlock()
        {
            CheckScript();
            dynamic ast = Script?.Ast;
            return ast?.ParamBlock;
        }

        public ScriptRequirements GetScriptRequirements()
        {
            CheckScript();
            dynamic ast = Script?.Ast;
            return ast?.ScriptRequirements;
        }

        public void SetProxyVariable(string VariableName, object Value)
        {
            proxyVariables.Add(new ProxyVar { Name = VariableName, Value = Value });
        }

        public object GetProxyVariable(string VariableName)
        {
            var ret = proxyVariables.FirstOrDefault(x => x.Name.Equals(VariableName))?.Value;
            return ret;
        }

        public void Run(object Input = null)
        {

            try
            {
                var source = Script.ToString().Trim();
                currentPowerShell.AddScript(source, true);

                if (this.Parameters != null)
                {
                    foreach (var p in Parameters)
                    {
                        currentPowerShell.AddParameter(p.Key, p.Value);
                    }
                }

                // If proxyVariables have been populated, pass them through to the
                // runspace. Can be used to variables that are expected to be in 
                // an environment but not explicitly set by a script.
                if (this.proxyVariables.Count != 0)
                {
                    foreach (var v in proxyVariables)
                    {
                        currentPowerShell.Runspace.SessionStateProxy.SetVariable(v.Name, v.Value);
                    }
                }

                // Add the default outputter to the end of the pipe and then
                // call the MergeMyResults method to merge the output and
                // error streams from the pipeline. This will result in the
                // output being written using the PSHost and PSHostUserInterface
                // classes instead of returning objects to the host application.
                currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);

                // If there is any input pass it in, otherwise just invoke
                // the pipeline.
                if (Input != null)
                {
                    Results = new PSDataCollection<PSObject>(currentPowerShell.Invoke(new object[] { Input }));
                }
                else
                {
                    try
                    {
                        Results = new PSDataCollection<PSObject>(currentPowerShell.Invoke());
                    }
                    catch (Exception e)
                    {
                        Results?.Add(new PSObject(e.GetAllMessages()));
                    }
                }

                var exit = 0;
                try { exit = Convert.ToInt32(currentPowerShell.Runspace.SessionStateProxy.GetVariable("LastExitCode")); }
                catch { /* Don't really care about this */ }
                ExitCode = exit;
            }
            finally
            {

            }
        }

        private class ProxyVar
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        //public void RunAs(ImpersonatedUser user, object Input = null)
        //{

        //    try
        //    {
        //        var source = Script.ToString().Trim();
        //        currentPowerShell.AddScript(source, true);

        //        if (this.Parameters != null)
        //        {
        //            foreach (var p in Parameters)
        //            {
        //                currentPowerShell.AddParameter(p.Key, p.Value);
        //            }
        //        }

        //        // Add the default outputter to the end of the pipe and then
        //        // call the MergeMyResults method to merge the output and
        //        // error streams from the pipeline. This will result in the
        //        // output being written using the PSHost and PSHostUserInterface
        //        // classes instead of returning objects to the host application.
        //        currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);


        //        // If there is any input pass it in, otherwise just invoke
        //        // the pipeline.
        //        // If there is any input pass it in, otherwise just invoke
        //        // the pipeline.

        //        using (Runspace runspace = RunspaceFactory.CreateRunspace())
        //        {
        //            runspace.Open();

        //            currentPowerShell.Runspace = runspace;

        //            foreach (var proxyVar in proxyVariables)
        //            {
        //                currentPowerShell?.Runspace.SessionStateProxy.SetVariable(proxyVar.Item1, proxyVar.Item2);
        //            }

        //            // TODO: needs lots of testing
        //            WindowsIdentity.RunImpersonated(user.IdentityHandle, () =>
        //            {
        //                if (Input != null)
        //                {
        //                    Results = new PSDataCollection<PSObject>(currentPowerShell.Invoke(new object[] { Input }));
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        Results = new PSDataCollection<PSObject>(currentPowerShell.Invoke());
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        Results?.Add(new PSObject(e.GetAllMessages()));
        //                    }
        //                }
        //            });

        //            var exit = 0;
        //            try { exit = Convert.ToInt32(currentPowerShell.Runspace.SessionStateProxy.GetVariable("LastExitCode")); }
        //            catch { /* Don't really care about this */ }
        //            ExitCode = exit;

        //            runspace.Close();
        //        }
        //    }
        //    finally
        //    {

        //    }
        //}



        /* Dont use any of this!!!! It doesn't work yet!!! */
        private BlockingCollection<InformationalRecord> PSOutput = new BlockingCollection<InformationalRecord>();
        private Task ForwardMessages;


        private async Task<PSDataCollection<PSObject>> InvokeAsync(PowerShell Session, object Input = null)
        {

            return await Task.Run(() => {
                IAsyncResult runHandle;
                if (Input != null)
                {
                    runHandle = currentPowerShell.BeginInvoke<object, PSObject>(new PSDataCollection<object>(new object[] { Input }), new PSDataCollection<PSObject>(Results));
                }
                else
                {
                    runHandle = currentPowerShell.BeginInvoke();
                }
                return currentPowerShell.EndInvoke(runHandle);
            });
        }

        public async Task<PSDataCollection<PSObject>> RunAsync(object Input = null)
        {
            try
            {
                var source = Script.ToString().Trim();
                currentPowerShell.AddScript(source, true);

                if (this.Parameters != null)
                {
                    foreach (var p in Parameters)
                    {
                        currentPowerShell.AddParameter(p.Key, p.Value);
                    }
                }

                // Add the default outputter to the end of the pipe and then
                // call the MergeMyResults method to merge the output and
                // error streams from the pipeline. This will result in the
                // output being written using the PSHost and PSHostUserInterface
                // classes instead of returning objects to the host application.
                currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);


                // If there is any input pass it in, otherwise just invoke
                // the pipeline.
                // If there is any input pass it in, otherwise just invoke
                // the pipeline.

                return await Task.Factory.FromAsync(currentPowerShell.BeginInvoke<object>(Input), result => currentPowerShell.EndInvoke(result));

            }
            finally
            {

            }
        }

        #endregion  // methods
    }
}
