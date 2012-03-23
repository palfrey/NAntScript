using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;

namespace broloco.NAntTasks
{

    /// <summary>
    /// Compiles a collection of taskdef/funcdef files into an assembly.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Compiles a collection of build files containing <see cref="TaskDefTask" /> and 
    ///       <see cref="FuncDefTask" /> definitions.
    ///       Each build file should contain a &lt;project&gt; top-level node, with taskdef/funcdef nodes
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
        
        private static string GetNamespaceDeclaration(XmlDocument document)
        {
            foreach (XmlAttribute attribute in document.DocumentElement.Attributes)
            {
                if (attribute.Name == "xmlns")
                {
                    if (attribute.Value == "http://none")
                    {
                        continue;
                    }
                    return "xmlns='" + attribute.Value + "'";
                }
            }
            return "";
        }

        internal static void UseDefaultNamespace(XmlDocument document, Project project)
        {
            string xmlCopy = document.OuterXml;
            xmlCopy = xmlCopy.Replace("xmlns=\"", "disabledxmlns=\"");
            string docStart = "<" + document.DocumentElement.Name;
            string newDocStart = docStart + " " + GetNamespaceDeclaration(project.Document);
            xmlCopy = xmlCopy.Replace(docStart, newDocStart);
            document.LoadXml(xmlCopy);
        }

        /// <summary>
        /// Executes the tdc task.
        /// </summary>
        protected override void ExecuteTask()
        {
            if (!IsOutputUpToDate())
            {
                string source =   "using System;\n"
                	            + "using System.Xml;\n"
                                + "using NAnt.Core;\n"
                                + "using NAnt.Core.Attributes;\n"
                                + "using NAnt.Core.Types;\n";

                foreach (string fileName in Sources.FileNames)
                {
                    XmlDocument tasksXml = new XmlDocument();
                    tasksXml.Load(fileName);
                    UseDefaultNamespace(tasksXml, Project);

                    foreach (XmlNode taskXml in tasksXml.SelectNodes("/*/*[local-name()='taskdef']"))
                    {
                        Log(Level.Verbose, "generating task from: " + taskXml.OuterXml);
                        TaskDefTask taskDef = (TaskDefTask) Project.CreateTask(taskXml);
                        source += taskDef.GenerateCSharpCode() + "\n";
                    }
				}
				
				Dictionary<string, List<FuncDefTask>> funcs = new Dictionary<string, List<FuncDefTask>>();

                foreach (string fileName in Sources.FileNames)
                {
                    XmlDocument tasksXml = new XmlDocument();
                    tasksXml.Load(fileName);
                    UseDefaultNamespace(tasksXml, Project);

                    foreach (XmlNode taskXml in tasksXml.SelectNodes("/*/*[local-name()='funcdef']"))
                    {
                        Log(Level.Verbose, "generating task from: " + taskXml.OuterXml);
                        FuncDefTask funcDef = (FuncDefTask) Project.CreateTask(taskXml);
						if (!funcs.ContainsKey(funcDef.Namespace))
							funcs[funcDef.Namespace] = new List<FuncDefTask>();
						funcs[funcDef.Namespace].Add(funcDef);
                    }
                }
				
				foreach(string ns in funcs.Keys)
				{
					source += string.Format("[FunctionSet(\"{0}\", \"{0}\")]", ns);
					source += string.Format("public class {0}: FunctionSetBase {{\n", ns);
				    source += string.Format("public {0}(Project project, PropertyDictionary properties) : base(project, properties) {{}}\n", ns);
					foreach(FuncDefTask fd in funcs[ns])
                    	source += fd.GenerateCSharpCode() + "\n";
					source += "}\n";
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

