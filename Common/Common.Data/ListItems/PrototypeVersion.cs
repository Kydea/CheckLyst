namespace CheckListManager.Common.Data.ListItems
{
    public class PrototypeVersion
    {
        public int Version { get; set; }
        public string VersionState { get; set; }
        public string Description => $"{Version} - {VersionState}";

        public override bool Equals(object obj)
        {
            var version = obj as PrototypeVersion;
            return version != null && Version == version.Version;
        }

        public override int GetHashCode()
        {
            return -1677367089 + Version.GetHashCode();
        }
    }
}
