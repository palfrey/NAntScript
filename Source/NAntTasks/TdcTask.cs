
using System;
using System.CodeDom.Compiler;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace broloco.NAntTasks
{

    /// <summary>
    /// Allows compilation of taskdef files.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   <code>
    ///     <![CDATA[
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
        /// The set of source files for compilation.
        /// </summary>
        [BuildElement("sources", Required=true)]
        public FileSet Sources {
            get { return _sources; }
            set { _sources = value; }
        }

        private void CompileSource(string source)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameters = new CompilerParameters();
            parameters.OutputAssembly = Output;
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

        /// <summary>
        /// Executes the tdc task.
        /// </summary>
        protected override void ExecuteTask()
        {
            foreach (string fileName in Sources.FileNames)
            {
            }

            string source = "public class tst {}";
            CompileSource(source);
        }

    }
}

