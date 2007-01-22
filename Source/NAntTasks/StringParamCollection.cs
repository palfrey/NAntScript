
using System;
using System.Collections;

using NAnt.Core;

namespace broloco.NAntTasks
{

    /// <summary>
    /// A typed StringParam collection.
    /// </summary>
    public class StringParamCollection : CollectionBase
    {

        /// <summary>
        /// Type safe indexer
        /// </summary>
        public StringParam this[int idx] {
            get { return (StringParam) List[idx]; }
        }

        /// <summary>
        /// Type safe Add
        /// </summary>
        public void Add(StringParam stringParam) {
            List.Add(stringParam);
        }

        /// <summary>
        /// Type safe Add
        /// </summary>
        public void Add(StringParamCollection stringParams) {
            foreach (StringParam stringParam in stringParams) {
                this.Add(stringParam);
            }
        }

    }

}

