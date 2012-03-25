using System;
using System.Collections;

using NAnt.Core;

namespace broloco.NAntTasks
{
    /// <summary>
    /// A typed FuncParam collection.
    /// </summary>
    public class FuncParamCollection : CollectionBase
    {

        /// <summary>
        /// Type safe indexer
        /// </summary>
        public FuncParam this[int idx]
        {
            get { return (FuncParam) List[idx]; }
        }

        /// <summary>
        /// Type safe Add
        /// </summary>
        public void Add(FuncParam stringParam)
        {
            List.Add(stringParam);
        }

        /// <summary>
        /// Type safe Add
        /// </summary>
        public void Add(FuncParamCollection stringParams)
        {
            foreach (FuncParam stringParam in stringParams)
            {
                this.Add(stringParam);
            }
        }

    }

}

