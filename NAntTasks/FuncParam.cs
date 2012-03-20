
using System;
using System.Collections;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace broloco.NAntTasks
{

    /// <summary>
    /// Describes a func parameter to a custom scripted task.
    /// </summary>
    [ElementName("FuncParam")]
    public class FuncParam : Element
    {
        private string  _parameterName  = string.Empty;
        /// <summary>
        /// The parameter name.
        /// </summary>
        [TaskAttribute("name", Required=true)]
        public string ParameterName
        {
            get { return _parameterName; }
            set { _parameterName = value; }
        }

        private string  _type  = string.Empty;
        /// <summary>
        /// The parameter name.
        /// </summary>
        [TaskAttribute("type")]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }

}

