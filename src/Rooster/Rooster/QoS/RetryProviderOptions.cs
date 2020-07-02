using System.Collections.ObjectModel;

namespace Rooster.QoS
{
    public class RetryProviderOptions
    {
        public Collection<int> Delays { get; set; } = new Collection<int>();
    }
}