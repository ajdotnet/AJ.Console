using System.Collections.Generic;
// Source: https://github.com/ajdotnet/AJ.Console
using System.Collections.Specialized;

namespace AJ.Console
{
    /// <summary>
    /// a single parameter
    /// </summary>
    internal class Parameter
    {
        /// <value>
        /// switch name (empty for arguments)
        /// </value>
        public string Name { get; set; }

        /// <value>
        /// list of values (arguments)
        /// </value>
        public List<string> Values { get; private set; }

        /// <value>
        /// <c>true</c> if the switch has been processed; otherwise, <c>false</c>.
        /// distiguishes between special and common switches.
        /// </value>
        public bool IsApplied { get; set; }

        public Parameter()
        {
            this.Values = new List<string>();
        }
    }
}
