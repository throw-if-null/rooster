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

        /// <summary>
        /// Gets or sets the value in minutes that will be deduced from current date when compared to log's last update date.
        /// </summary>
        /// <remarks>
        /// Meaning if the current date is `2020-07-07 12:15:00` and the log's last update date is `2020-07-07 12:10:00` and if the CurrentDateVariance
        /// has the value 5, application would consider the to dates to be equal and it would proceed with reading the log.
        /// Default value is 5 (minutes).
        /// </remarks>
        public int CurrentDateVariance { get; set; } = 5;
    }
}