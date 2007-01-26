
using System;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace broloco.NAntTasks
{

    /// <summary>
    /// Creates a custom task containing NAnt tasks.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Define a task by specifying a name, a list of string parameters (attributes), and
    ///       a lits of node parameters (child elements).  A custom task is created that
    ///       runs each of the tasks in the &lt;do/&gt; parameter replacing string parameters and
    ///       node parameters before execution.
    ///   </para>
    ///   <para>
    ///     String parameters are referenced in the &lt;do/&gt; section using the syntax <i>__parameter name__</i>.
    ///   </para>
    ///   <para>
    ///     Node parameters are referenced in the &lt;do/&gt; section using the syntax <i>&lt;__parameter name__/&gt;</i>.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///     Create a custom task that takes two parameters.  Parameter 1 can be set to 'echo' or 'fail', and
    ///       parameter 2 is the message to echo/fail.
    ///   <code>
    ///     <![CDATA[
    ///       <taskdef name="echoFail" >
    ///
    ///         <stringparams>
    ///           <stringparam name="task" />
    ///           <stringparam name="message" required="false" />
    ///         </stringparams>
    ///
    ///         <do>
    ///           <property name="customMessage" value="__message__" />
    ///           <if test="${customMessage == ''}" >
    ///             <property name="customMessage" value="this is a default message" />
    ///           </if>
    ///           <__task__ message="${customMessage}" />
    ///         </do>
    ///
    ///       </taskdef>
    ///       ...
    ///       <!-- echos the default message -->
    ///       <echoFail task="echo" />
    ///       ...
    ///       <echoFail task="fail" message="this will fail the build" />
    ///     ]]>
    ///   </code>
    ///   </para>
    /// </example>
    /// <example>
    ///   <para>
    ///     Create a custom task that takes a list of source files, target files, and
    ///       a list of tasks to execute when the target files are not up to date.
    ///   <code>
    ///     <![CDATA[
    ///       <taskdef name="execIfNotUpToDate" >
    ///
    ///         <nodeparams>
    ///           <nodeparam name="sourceFiles" />
    ///           <nodeparam name="targetFiles" />
    ///           <nodeparam name="do" />
    ///         </nodeparams>
    ///
    ///         <do>
    ///           <uptodate property="execIfNotUpToDate.uptodate">
    ///             <sourcefiles>
    ///               <__sourceFiles__/>
    ///             </sourcefiles>
    ///             <targetfiles>
    ///               <__targetFiles__/>
    ///             </targetfiles>
    ///           </uptodate>
    ///           <if test="${not execIfNotUpToDate.uptodate}" >
    ///             <__do__/>
    ///           </if>
    ///         </do>
    ///
    ///       </taskdef>
    ///       ...
    ///       <!-- echos the a message if the files are not up to date -->
    ///       <execIfNotUpToDate>
    ///
    ///         <sourceFiles>
    ///           <include name="*.cs" />
    ///         </sourceFiles>
    ///
    ///         <targetFiles>
    ///           <include name="myAssembly.dll" />
    ///         </targetFiles>
    ///
    ///         <do>
    ///           <echo message="work needs done" />
    ///         </do>
    ///
    ///       </execIfNotUpToDate>
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
        /// A list of <see cref="StringParam" /> (attribute) parameters.
        /// </summary>
        [BuildElementCollection("stringparams", "stringparam")]
        public StringParamCollection StringParams
        {
            get { return _stringParams; }
        }

        /// <summary>
        /// A list of <see cref="NodeParam" /> (xml node) parameters.
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
        /// Generates C# code for the task
        /// </summary>
        public string GenerateCSharpCode()
        {
            string customTaskCode = "";

            customTaskCode +=  "[TaskName(\"" + _tagName + "\")]\n";
            customTaskCode +=  "public class " + _tagName + " : Task\n";
            customTaskCode +=  "{\n";
            customTaskCode +=  "\n";
            customTaskCode +=  "    static private string _originalXml = XmlConvert.DecodeName(\"" + XmlConvert.EncodeLocalName(_tasks.Xml.OuterXml) + "\");\n";
            customTaskCode +=  "\n";

            // generate named string parameters
            foreach (StringParam stringParam in StringParams)
            {
                customTaskCode +=  "    private string __" + stringParam.ParameterName + " = string.Empty ;\n";
                customTaskCode +=  "\n";
                customTaskCode +=  "    [TaskAttribute(\"" + stringParam.ParameterName + "\", Required=" + stringParam.Required.ToString().ToLower() + ")]\n";
                customTaskCode +=  "    public string _" + stringParam.ParameterName + "\n";
                customTaskCode +=  "    {\n";
                customTaskCode +=  "        get { return __" + stringParam.ParameterName + "; }\n";
                customTaskCode +=  "        set { __" + stringParam.ParameterName + " = value; }\n";
                customTaskCode +=  "    }\n";
                customTaskCode +=  "\n";
            }

            // generate named xml-node parameters
            foreach (NodeParam nodeParam in NodeParams)
            {
                customTaskCode +=  "    private RawXml __" + nodeParam.ParameterName + ";\n";
                customTaskCode +=  "\n";
                customTaskCode +=  "    [BuildElement(\"" + nodeParam.ParameterName + "\", Required=" + nodeParam.Required.ToString().ToLower() + ")]\n";
                customTaskCode +=  "    public RawXml _" + nodeParam.ParameterName + "\n";
                customTaskCode +=  "    {\n";
                customTaskCode +=  "        get { return __" + nodeParam.ParameterName + "; }\n";
                customTaskCode +=  "        set { __" + nodeParam.ParameterName + " = value; }\n";
                customTaskCode +=  "    }\n";
                customTaskCode +=  "\n";
            }

            customTaskCode +=  "    protected override void ExecuteTask()\n";
            customTaskCode +=  "    {\n";

            customTaskCode +=  "        Log(Level.Info, \"Running custom script\");\n";
            customTaskCode +=  "        Log(Level.Verbose, \"Original script : \" + _originalXml);\n";
            customTaskCode +=  "        string xml = _originalXml;\n";

            customTaskCode +=  "        XmlDocument scriptDom = new XmlDocument();\n";
            customTaskCode +=  "        scriptDom.LoadXml(xml);\n";

            // generate string replacements for each nodeParam
            if (NodeParams.Count > 0)
                customTaskCode +=  "        XmlNodeList nodes;\n";
            foreach (NodeParam nodeParam in NodeParams)
            {
                customTaskCode +=  "        nodes = scriptDom.SelectNodes(\"//__"  + nodeParam.ParameterName + "__\");\n";
                customTaskCode +=  "        foreach (XmlNode node in nodes)\n";
                customTaskCode +=  "        {\n";
                customTaskCode +=  "            if (_" + nodeParam.ParameterName + " != null)\n";
                customTaskCode +=  "            {\n";
                customTaskCode +=  "                foreach (XmlNode task in _" + nodeParam.ParameterName + ".Xml.ChildNodes)\n";
                customTaskCode +=  "                {\n";
                customTaskCode +=  "                    node.ParentNode.InsertBefore(scriptDom.ImportNode(task, true), node);\n";
                customTaskCode +=  "                }\n";
                customTaskCode +=  "            }\n";
                customTaskCode +=  "            node.ParentNode.RemoveChild(node);\n";
                customTaskCode +=  "        }\n";
            }

            // generate string replacements for each stringParam
            customTaskCode +=  "        xml = scriptDom.OuterXml;\n";
            foreach (StringParam stringParam in StringParams)
            {
                customTaskCode +=  "        xml = xml.Replace(\"__" + stringParam.ParameterName + "__\", _" + stringParam.ParameterName + ");\n";
            }
            customTaskCode +=  "        scriptDom.LoadXml(xml);\n";

            customTaskCode +=  "        Log(Level.Verbose, \"Generated script: \" + scriptDom.InnerXml);\n";
            customTaskCode +=  "        foreach (XmlNode node in scriptDom.ChildNodes[0].ChildNodes)\n";
            customTaskCode +=  "        {\n";
            customTaskCode +=  "            if (node.Name == \"#comment\")\n";
            customTaskCode +=  "                continue;\n";
            customTaskCode +=  "            Log(Level.Verbose, \"Running task: \" + node.OuterXml);\n";
            customTaskCode +=  "            Project.CreateTask(node).Execute();\n";
            customTaskCode +=  "        }\n";

            customTaskCode +=  "    }\n";
            customTaskCode +=  "}\n";

            return customTaskCode;
        }

        /// <summary>
        /// Executes the taskdef task.
        /// </summary>
        protected override void ExecuteTask()
        {
            Log(Level.Info, "Creating task " + _tagName);

            // Generate code for the custom task as a script task ...
            Log(Level.Verbose, "*** Custom code start ***");
            string customTaskCode = "";
            customTaskCode +=  "<script language='C#' >\n";
            customTaskCode +=  "<imports> <import namespace=\"System.Xml\" /> <import namespace=\"NAnt.Core.Types\" /> </imports> <code> <![CDATA[\n";
            customTaskCode += GenerateCSharpCode();
            customTaskCode += "]]" + "></code></script>";

            Log(Level.Verbose, customTaskCode);
            Log(Level.Verbose, "*** Custom code end ***");

            XmlDocument xmlScriptTask = new XmlDocument();
            xmlScriptTask.LoadXml(customTaskCode);
            Project.CreateTask(xmlScriptTask.ChildNodes[0]).Execute();
        }

    }
}

