
using System;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace broloco.NAntTasks
{

    /// <summary>
    /// Allows scripting of a custom task.
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
    [TaskName("taskdef")]
    public class TaskDefTask : Task
    {

        private string                  _tagName;
        private StringParamCollection   _stringParams   = new StringParamCollection();
        private NodeParamCollection     _nodeParams     = new NodeParamCollection();
        private RawXml                  _tasks;


        /// <summary>
        /// The name for the custom task.
        /// </summary>
        [TaskAttribute("name", Required=true)]
        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }

        /// <summary>
        /// The string parameters.
        /// </summary>
        [BuildElementCollection("stringparams", "stringparam")]
        public StringParamCollection StringParams
        {
            get { return _stringParams; }
        }

        /// <summary>
        /// The xml-node parameters.
        /// </summary>
        [BuildElementCollection("nodeparams", "nodeparam")]
        public NodeParamCollection NodeParams
        {
            get { return _nodeParams; }
        }

        /// <summary>
        /// The tasks to script
        /// </summary>
        [BuildElement("do", Required=true)]
        public RawXml Tasks
        {
            get { return _tasks; }
            set { _tasks = value; }
        }

        /// <summary>
        /// Executes the taskdef task.
        /// </summary>
        protected override void ExecuteTask()
        {
        }

    }
}

