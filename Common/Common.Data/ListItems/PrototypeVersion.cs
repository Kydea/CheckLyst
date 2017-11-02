namespace CheckListManager.Common.Data.ListItems
{
    public class PrototypeVersion
    {
        public int Version { get; set; }
        public string VersionState { get; set; }
        public string Description => $"{Version} - {VersionState}";
    }
}
