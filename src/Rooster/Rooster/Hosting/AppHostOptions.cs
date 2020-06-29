namespace Rooster.Hosting
{
    public class AppHostOptions
    {
        /// <summary>
        /// Gets or sets polling interval.
        /// The amount of time that will pass between tow consecutive calls toward Kudu API.
        /// </summary>
        /// <remarks>
        /// Default value is 60 seconds.
        /// </remarks>
        public double PoolingIntervalInSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets flag that determines whether the internal poller will be used.
        /// </summary>
        /// <remarks>
        /// Internal poller means that code will run in infinite while loop, while external poller means that code will run once and then
        /// some scheduler of your choice will execute the app again, in this scenario Rooster app becomes a job.
        /// </remarks>
        public bool UseInternalPoller { get; set; } = false;
    }
}