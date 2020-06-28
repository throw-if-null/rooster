using System.Collections.ObjectModel;

namespace Rooster.Adapters.Kudu.Handlers
{
    public class HttpInterceptorOptions
    {
        /// <summary>
        /// Gets or sets the path that will be intercepted.
        /// </summary>
        /// <example>/api/user/2 or /user</example>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Method name.
        /// </summary>
        /// <example>POST or GET</example>
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the status code for HTTP response message.
        /// </summary>
        /// <example>200 or 404</example>
        public int ReturnStatusCode { get; set; }

        /// <summary>
        /// Gets or sets serialized response content.
        /// </summary>
        public string ReturnJsonContent { get; set; }

        /// <summary>
        /// Gets or sets HTTP response headers.
        /// </summary>
        public Collection<HttpResponseHeader> Headers { get; set; } = new Collection<HttpResponseHeader>();

        /// <summary>
        /// Represents an HTTP response header.
        /// </summary>
        public class HttpResponseHeader
        {
            /// <summary>
            /// Gets or sets the header name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the header value.
            /// </summary>
            public string Value { get; set; }
        }
    }
}