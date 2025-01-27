//---------------------------------------------------------------------
// <copyright file="ODataContentTypeException.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Core;
using System;
using System.Diagnostics;

namespace Microsoft.OData
{
    /// <summary>Exception type representing exception when Content-Type of a message is not supported.</summary>
    [DebuggerDisplay("{Message}")]
    public class ODataContentTypeException : ODataException
    {
        /// <summary>Creates a new instance of the <see cref="Microsoft.OData.ODataContentTypeException" /> class.</summary>
        /// <remarks>
        /// The Message property is initialized to a system-supplied message
        /// that describes the error. This message takes into account the
        /// current system culture.
        /// </remarks>
        public ODataContentTypeException()
            : this(SRResources.ODataException_GeneralError)
        {
        }

        /// <summary>Creates a new instance of the <see cref="Microsoft.OData.ODataContentTypeException" /> class.</summary>
        /// <param name="message">Plain text error message for this exception.</param>
        public ODataContentTypeException(string message)
            : this(message, null)
        {
        }

        /// <summary>Creates a new instance of the <see cref="Microsoft.OData.ODataContentTypeException" /> class.</summary>
        /// <param name="message">Plain text error message for this exception.</param>
        /// <param name="innerException">Exception that caused this exception to be thrown.</param>
        public ODataContentTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
