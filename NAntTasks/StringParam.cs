
using System;
using System.Collections;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace broloco.NAntTasks
{

    /// <summary>
    /// Describes a string parameter to a custom scripted task.
    /// </summary>
    [ElementName("StringParam")]
    public class StringParam : Element
    {

        private string  _parameterName  = string.Empty;
        private bool    _required       = true;

        /// <summary>
        /// The parameter name.
        /// </summary>
        [TaskAttribute("name", Required=true)]
        public string ParameterName
        {
            get { return _parameterName; }
            set { _parameterName = value; }
        }

        /// <summary>
        /// Specifies if the parameter is required (default is true)
        /// </summary>
        [TaskAttribute("required", Required=false)]
        public bool Required
        {
            get { return _required; }
            set { _required = value; }
        }

    }

}

