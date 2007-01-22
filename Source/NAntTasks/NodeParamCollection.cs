
using System;
using System.Collections;

using NAnt.Core;

namespace broloco.NAntTasks
{

    /// <summary>
    /// A typed NodeParam collection.
    /// </summary>
    public class NodeParamCollection : CollectionBase
    {

        /// <summary>
        /// Type safe indexer
        /// </summary>
        public NodeParam this[int idx] {
            get { return (NodeParam) List[idx]; }
        }

        /// <summary>
        /// Type safe Add
        /// </summary>
        public void Add(NodeParam nodeParam) {
            List.Add(nodeParam);
        }

        /// <summary>
        /// Type safe Add
        /// </summary>
        public void Add(NodeParamCollection nodeParams) {
            foreach (NodeParam nodeParam in nodeParams) {
                this.Add(nodeParam);
            }
        }

    }

}

