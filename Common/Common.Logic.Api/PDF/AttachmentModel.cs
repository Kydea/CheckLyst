using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public class AttachmentModel
    {
        public Models.StorageFile File { get; set; }

        public string CompleteKey => Bucket + Key;

        public ICheckListItem Item { get; set; }

        public string Bucket { get; set; }

        public string Key { get; set; }

        public int Index { get; set; }

        public string Description { get; set; }
    }
}
