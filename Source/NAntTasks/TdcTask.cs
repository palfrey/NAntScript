
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;

namespace broloco.NAntTasks
{

    /// <summary>
    /// Compiles a collection of taskdef files into an assembly.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Compiles a collection of build files containing <see cref="TaskDefTask" /> definitions.
    ///       Each build file should contain a &lt;project&gt; top-level node, with taskdef nodes
    ///       as child elements.
    ///       The compiled assembly's tasks are loaded.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///     Compile all .taskdef files to an assembly in a temporary directory, and load the compiled tasks.
    ///   <code>
    ///     <![CDATA[
    ///       <tdc output="${path::get-temp-path()}/myTemporaryBuildAssembly.dll" >
    ///         <sources>
    ///           <include name="*.taskdef" />
    ///         </sources>
    ///       </tdc>
    ///     ]]>
    ///   </code>
    ///   </para>
    /// </example>
    [TaskName("tdc")]
    public class TdcTask : Task
    {

        private string  _output;
        private FileSet _sources;

        /// <summary>
        /// The output for the compiled library.
        /// </summary>
        [TaskAttribute("output", Required=true)]
        public string Output
        {
            get { return _output; }
            set { _output = value; }
        }

        /// <summary>
        /// The <see cref="FileSet" /> of source files for compilation.
        /// </summary>
        [BuildElement("sources", Required=true)]
        public FileSet Sources {
            get { return _sources; }
            set { _sources = value; }
        }

        private bool IsOutputUpToDate()
        {
            string tmpPropertyName = "__tdcTemporaryProperty";
            UpToDateTask upToDateTask = new UpToDateTask();
            upToDateTask.Project = Project;
            upToDateTask.PropertyName = tmpPropertyName;
            upToDateTask.SourceFiles = Sources;
            upToDateTask.TargetFiles = new FileSet();
            upToDateTask.TargetFiles.Includes.Add(Output);
            upToDateTask.Execute();

            return Convert.ToBoolean(Project.Properties[tmpPropertyName]);
        }

        private void CompileSource(string source)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameters = new CompilerParameters();
            parameters.OutputAssembly = Output;
            parameters.ReferencedAssemblies.Add(Project.GetType().Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(XmlDocument).Assembly.Location);
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, new string[] { source });

            if (results.Errors.Count > 0)
            {
                string errors = "Compiler error:" + Environment.NewLine;
                foreach (CompilerError err in results.Errors)
                {
                    errors += err.ToString() + Environment.NewLine;
                }
                errors += source;
                throw new BuildException(errors, Location);
            }
        }

        private static void StripNamespaces(XmlDocument document)
        {
            string xmlCopy = document.OuterXml;
            xmlCopy = xmlCopy.Replace("xmlns=\"", "disabledxmlns=\"");
            document.LoadXml(xmlCopy);
        }

        /// <summary>
        /// Executes the tdc task.
        /// </summary>
        protected override void ExecuteTask()
        {
            if (!IsOutputUpToDate())
            {
                string source =   "using System.Xml;\n"
                                + "using NAnt.Core;\n"
                                + "using NAnt.Core.Attributes;\n"
                                + "using NAnt.Core.Types;\n";

                foreach (string fileName in Sources.FileNames)
                {
                    XmlDocument tasksXml = new XmlDocument();
                    tasksXml.Load(fileName);
                    StripNamespaces(tasksXml);

                    foreach (XmlNode taskXml in tasksXml.SelectNodes("/*/taskdef"))
                    {
                        Log(Level.Verbose, "generating task from: " + taskXml.OuterXml);
                        TaskDefTask taskDef = (TaskDefTask) Project.CreateTask(taskXml);
                        source += taskDef.GenerateCSharpCode() + "\n";
                    }
                }

                Log(Level.Verbose, source);
                CompileSource(source);
            }

            LoadTasksTask loadTasksTask = new LoadTasksTask();
            loadTasksTask.Project = Project;
            loadTasksTask.AssemblyPath = new FileInfo(Output);
            loadTasksTask.Execute();
        }

    }
}

