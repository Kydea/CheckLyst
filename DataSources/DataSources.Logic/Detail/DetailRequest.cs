using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.DataSources.Logic.Detail
{
    public class DetailRequest : Request
    {
        public string Code { get; set; }
        public string Description { get; set; }

        //TODO: add interface information to store as json in data
    }
}
