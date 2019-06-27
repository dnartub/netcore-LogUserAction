using System;
using System.Collections.Generic;
using System.Text;

namespace LogUserAction
{
    public class LogRequestDetails
    {
        /// <summary>
        /// Full controller method Path: Namespace.Classname.Method(params)
        /// </summary>
        public string MethodInfo { get; set; }
        /// <summary>
        /// Route-Url
        /// </summary>
        public string RoutePath { get; set; }
        /// <summary>
        /// Request headers
        /// </summary>
        public object Headers { get; set; }
        /// <summary>
        /// Route values
        /// </summary>
        public object RouteValues { get; set; }
        /// <summary>
        /// Query values
        /// </summary>
        public object QueryValues { get; set; }
        /// <summary>
        /// Values in FormData
        /// </summary>
        public object FormValues { get; set; }
        /// <summary>
        /// Object in Body 
        /// for Content-Type json
        /// </summary>
        public object BodyValues { get; set; }
    }
}
