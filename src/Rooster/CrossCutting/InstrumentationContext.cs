using System;
using System.Threading;

namespace Rooster.CrossCutting
{
    public interface IInstrumentationContext
    {

        /// <summary>
        /// Returns the correlation id values stored in <see cref="AsyncLocal{T}"/> store.
        /// Note: Value can be null.
        /// </summary>
        /// <returns>Correlation id or null.</returns>
        string CorrelationValue { get; }
    }

    public sealed class InstrumentationContext : IInstrumentationContext
    {
        private const string X = "x";

        private readonly AsyncLocal<string> _correlationId = new AsyncLocal<string>();

        public string CorrelationValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_correlationId.Value))
                    _correlationId.Value = Random().ToString(X);

                return _correlationId.Value;
            }
        }


        private static ulong Random()
        {
            var guid = Guid.Parse(Guid.NewGuid().ToString());
            var bytes = guid.ToByteArray();

            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
