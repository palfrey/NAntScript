using System;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace broloco.NAntTasks
{
    /// <summary>
    /// Creates a custom function containing NAnt tasks.
    /// </summary>	
    /// <remarks>
    ///   <para>
    ///     Define a function by specifying a name, a namespace and a list of function parameters.
    ///     A custom function is created that runs each of the tasks in the &lt;do/&gt; parameter replacing
    ///     function parameters before execution.
    ///   </para>
    ///   <para>
    /// 	A special parameter __$return__ must be set during the &lt;do/&gt; block that is the return value
    ///     of the function. Default type of this is 'string', but 'int' and 'bool' functions can also be written
    ///     by setting a attribute returnType on the funcdef.
    ///   </para>
    ///   <para>
    ///      Function parameters are referenced in the &lt;do/&gt; section using the syntax <i>__parameter name__</i>.
    ///      They are all required, and are given to the function in the same order as they are in the funcparams list.
    ///      The type of a parameter defaults to 'string', but 'int' and 'bool' parameters can also be done by setting
    ///      the 'type' attribute on the funcparam.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///     Create a custom function that takes one parameter, and which returns that value.
    ///   <code>
    ///     <![CDATA[
    ///		<funcdef name="testFunc" namespace="tests">
    ///			<funcparams>
    ///				<funcparam name="test" type="string" />
    ///			</funcparams>
    ///			<do>
    ///				<property name="__$return__" value="__test__" />
    ///			</do>
    ///		</funcdef>
    ///		<fail unless="${tests::testFunc('foo') == 'foo'}" />
    ///     ]]>
    ///   </code>
    ///   </para>
    /// </example>
	[TaskName("funcdef")]
    public class FuncDefTask: Task
    {
		private string _namespace;	
		/// <summary>
        /// The namespace for the custom function
        /// </summary>
        [TaskAttribute("namespace", Required=true)]
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        private string _tagName;		
		/// <summary>
        /// The name for the custom function
        /// </summary>
        [TaskAttribute("name", Required=true)]
        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }
		
		private string _returnType = "string";
		/// <summary>
        /// The name for the custom function
        /// </summary>
        [TaskAttribute("returnType", Required=false)]
        public string ReturnType
        {
            get { return _returnType; }
            set { _returnType = value; }
        }		

	    private FuncParamCollection _funcParams   = new FuncParamCollection();		
        /// <summary>
        /// A list of <see cref="FuncParam" /> (attribute) parameters.
        /// </summary>
        [BuildElementCollection("funcparams", "funcparam")]
        public FuncParamCollection FuncParams
        {
            get { return _funcParams; }
        }
		
        private RawXml _tasks;		
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
        /// Generates C# code for the function
        /// </summary>
        public string GenerateCSharpCode()
        {
            string customTaskCode = "";
			customTaskCode +=  "    static private string _original"+ TagName +"Xml = XmlConvert.DecodeName(\"" + XmlConvert.EncodeLocalName(_tasks.Xml.OuterXml) + "\");\n";
			
			customTaskCode +=  "    [Function(\""+_tagName + "\")]\n";			

            customTaskCode +=  "    public "+ _returnType + " " + _tagName + "(";
			
			// generate named string parameters
            Log(Level.Verbose, "Number of func parameters: " + FuncParams.Count);
			
			string args = string.Empty;
            foreach (FuncParam funcParam in FuncParams)
            {
				if (args != string.Empty)
					args += ", ";
				args += funcParam.Type + " _"+funcParam.ParameterName;
            }
			customTaskCode += args;
			
            customTaskCode +=  ")\n";
            customTaskCode +=  "    {\n";
            customTaskCode +=  "        Project.Log(Level.Verbose, \"Original script : \" + _original"+ TagName +"Xml);\n";
            customTaskCode +=  "        string xml = _original"+ TagName +"Xml;\n";

            customTaskCode +=  "        XmlDocument scriptDom = new XmlDocument();\n";
            customTaskCode +=  "        scriptDom.LoadXml(xml);\n";

            customTaskCode +=  "        xml = scriptDom.OuterXml;\n";
            // generate string replacements for each funcParam
            foreach (FuncParam funcParam in FuncParams)
            {
                customTaskCode +=  "        xml = xml.Replace(\"__" + funcParam.ParameterName + "__\", _" + funcParam.ParameterName + ".ToString());\n";
            }
            customTaskCode +=  string.Format("        if (xml.IndexOf(\"__$return__\")==-1) throw new Exception(\"No __$return__ in do section of {0}::{1}!\");\n", Namespace, TagName);
            customTaskCode +=  "        xml = xml.Replace(\"__$return__\", \"_ReturnParam\");\n";
            customTaskCode +=  "        scriptDom.LoadXml(xml);\n";

            customTaskCode +=  "        Project.Log(Level.Verbose, \"Generated script: \" + scriptDom.InnerXml);\n";
            customTaskCode +=  "        foreach (XmlNode node in scriptDom.ChildNodes[0].ChildNodes)\n";
            customTaskCode +=  "        {\n";
            customTaskCode +=  "            if (node.Name == \"#comment\")\n";
            customTaskCode +=  "                continue;\n";
            customTaskCode +=  "            Project.Log(Level.Verbose, \"Running task: \" + node.OuterXml);\n";
            customTaskCode +=  "            Project.CreateTask(node).Execute();\n";
            customTaskCode +=  "        }\n";
			if (_returnType.ToLower() == "string")
				customTaskCode +=  "        return ("+ _returnType +") Project.Properties[\"_ReturnParam\"];\n";
			else if (_returnType.ToLower() == "int" || _returnType.ToLower() == "bool")
				customTaskCode +=  "        return "+ _returnType +".Parse(Project.Properties[\"_ReturnParam\"]);\n";
			else
				throw new BuildException(string.Format("Don't know how to handle return type '{0}'", _returnType));
            customTaskCode +=  "    }\n";

            return customTaskCode;
        }
			
        /// <summary>
        /// Executes the funcdef task.
        /// </summary>		
	    protected override void ExecuteTask()
        {
            Log(Level.Info, "Creating function " + _tagName);

            // Generate code for the custom function as a script task ...
            Log(Level.Verbose, "*** Custom code start ***");
            string customTaskCode = "";
            customTaskCode +=  "<script language='C#' prefix=\""+_namespace +"\" >\n";
            customTaskCode +=  "<imports> <import namespace=\"System.Xml\" /> <import namespace=\"NAnt.Core.Types\" /> </imports> <code> <![CDATA[\n";
			customTaskCode += GenerateCSharpCode();
            customTaskCode += "]]" + "></code></script>";

            XmlDocument xmlScriptTask = new XmlDocument();
            xmlScriptTask.LoadXml(customTaskCode);
            TdcTask.UseDefaultNamespace(xmlScriptTask, Project);

            Log(Level.Verbose, xmlScriptTask.OuterXml);
            Log(Level.Verbose, "*** Custom code end ***");

            Project.CreateTask(xmlScriptTask.ChildNodes[0]).Execute();
        }
	}

}
