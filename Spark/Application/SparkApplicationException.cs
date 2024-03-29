﻿namespace Spark.Application
{
    using System;
    using System.Runtime.Serialization;
    
    /// <summary>
    /// Engine exception for an error related to application services.
    /// </summary>
    [Serializable]
    public class SparkApplicationException : SparkException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SparkApplicationException"/> class.
        /// </summary>
        public SparkApplicationException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkApplicationException"/> class.
        /// </summary>
        /// <param name="msg">Error message that explains the reason for the exception</param>
        public SparkApplicationException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkApplicationException"/> class.
        /// </summary>
        /// <param name="paramName">Parameter name that caused the exception.</param>
        /// <param name="msg">Error message that explains the reason for the exception.</param>
        public SparkApplicationException(string paramName, string msg)
            : base(paramName, msg)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkApplicationException"/> class.
        /// </summary>
        /// <param name="msg">Error message that explains the reason for the exception.</param>
        /// <param name="innerException">Exception that caused this exception to be thrown, generally a more specific exception.</param>
        public SparkApplicationException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkApplicationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public SparkApplicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
