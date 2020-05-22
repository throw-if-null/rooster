using System.Threading.Tasks;

namespace Rooster
{
    public interface ILogReporter
    {
        Task Report(DockerLogReference logReference);
    }
}