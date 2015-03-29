// Source: https://github.com/ajdotnet/AJ.Console
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace AJ.Console
{
    /// <summary>
    /// Exception for handled but breaking conditions
    /// </summary>
    [Serializable]
    public class ConsoleException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleException"/> class.
        /// </summary>
        public ConsoleException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ConsoleException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleException"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public ConsoleException(string format, params object[] args) : base(string.Format(CultureInfo.CurrentCulture, format, args)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ConsoleException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected ConsoleException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
