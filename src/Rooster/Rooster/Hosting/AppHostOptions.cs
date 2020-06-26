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
    }
}