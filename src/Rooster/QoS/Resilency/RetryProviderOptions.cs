using System.Collections.ObjectModel;

namespace Rooster.QoS.Resilency
{
    public class RetryProviderOptions
    {
        public Collection<int> Delays { get; set; } = new Collection<int>();
    }
}