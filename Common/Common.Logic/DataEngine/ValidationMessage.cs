namespace CheckListManager.Common.Logic.DataEngine
{
    public class ValidationMessage
    {
        public const string
            LevelError = "ERROR",
            LevelWarning = "WARNING",
            LevelInformation = "INFO";

        public string Level { get; set; }

        public string Message { get; set; }
    }
}
