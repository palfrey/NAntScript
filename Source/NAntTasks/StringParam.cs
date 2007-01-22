
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

        private string _parameterName = string.Empty;

        /// <summary>
        /// The parameter name.
        /// </summary>
        [TaskAttribute("name", Required=true)]
        public string ParameterName {
            get { return _parameterName; }
            set { _parameterName = value; }
        }
    }

}

