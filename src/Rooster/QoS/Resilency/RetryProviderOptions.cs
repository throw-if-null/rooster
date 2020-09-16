using System.Collections.ObjectModel;

namespace Rooster.QoS.Resilency
{
    public class RetryProviderOptions
    {
        public int JitterMaximum { get; set; }

        public Collection<int> Delays { get; set; } = new Collection<int>();
    }
}