namespace Ithline.Extensions.Logging.File
{
    /// <summary>
    /// Specifies the frequency at which the log file should roll.
    /// </summary>
    public enum FileRollingInterval
    {
        /// <summary>
        /// The log file will never roll.
        /// </summary>
        None,
        /// <summary>
        /// Roll every year. File names will have current time appended in the pattern <c>yyyy</c>.
        /// </summary>
        Year,
        /// <summary>
        /// Roll every calendar month. File names will have current time appended in the pattern <c>yyyyMM</c>.
        /// </summary>
        Month,
        /// <summary>
        /// Roll every day. File names will have current time appended in the pattern <c>yyyyMMdd</c>.
        /// </summary>
        Day,
        /// <summary>
        /// Roll every hour. File names will have current time appended in the pattern <c>yyyyMMddHH</c>.
        /// </summary>
        Hour,
    }
}
