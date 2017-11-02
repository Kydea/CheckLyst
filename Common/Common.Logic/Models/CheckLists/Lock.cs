using System;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public class Lock
    {
        public Lock(Client client)
        {
            this.Client = client;
            User = (client.User ?? Security.CurrentUser).Description;
            Date = DateTime.Now;
        }

        [JsonIgnore]
        public Client Client { get; private set; }

        public string User { get; private set; }

        public DateTime Date { get; private set; }

        public uint ClientId { get { return Client.Id; } }
    }
}
