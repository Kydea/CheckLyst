namespace CheckListManager.Common.Data.ListItems
{
    public class PrototypeVersion
    {
        public int Version { get; set; }
        public string VersionState { get; set; }
        public string Description => $"{Version} - {VersionState}";
        public override bool Equals(object obj) => obj is PrototypeVersion version && Version == version.Version;
        public override int GetHashCode() => -1677367089 + Version.GetHashCode();
    }
}
