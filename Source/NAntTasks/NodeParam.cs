
using System;
using System.Collections;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace broloco.NAntTasks
{

    /// <summary>
    /// Describes an XML-Node parameter to a custom scripted task.
    /// </summary>
    [ElementName("NodeParam")]
    public class NodeParam : Element
    {

        private string _parameterName = string.Empty;

        /// <summary>
        /// The parameter name.
        /// </summary>
        [TaskAttribute("name", Required=true)]
        public string ParamName {
            get { return _parameterName; }
            set { _parameterName = value; }
        }
    }

}

