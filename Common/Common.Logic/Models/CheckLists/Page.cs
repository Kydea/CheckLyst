using System.Collections.Generic;
using System.Linq;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public class Page: Element
    {
        //public void ToggleLock(Client client, bool value)
        //{
        //    Lock = value ? new Lock(client) : null;
        //}

        public override void Setup()
        {
            base.Setup();
            Group previous = null;
            foreach (var group in Groups)
            {
                group.CheckList = CheckList;
                group.Page = this;
                group.Previous = previous;
                group.Setup();
                previous = group;
            }
        }

        public override bool IsReadOnly(uint clientId)
        {
			//TODO: get right value
	        return false;
	        //var instance = CheckList as Instance;
	        //return instance?.State != "In progress" && instance.PrototypeState != "Draft" && instance.PrototypeState != "In simulation" || base.IsReadOnly(clientId) || IsLocked(clientId);
        }

        private bool _valid;
        public override bool IsValidByDefault()
        {
            return _valid;
        }

        public override void Update()
        {
            _valid = true;
            foreach (var group in Groups)
            {
                group.Update();
                _valid = _valid && group.Valid;
            }
        }

        public void UpdateValues(uint clientId)
        {
            foreach (var group in Groups)
                group.UpdateValues(clientId);
        }

        public List<Group> Groups { get; set; } = new List<Group>();

        public string Description { get; set; }

		//public Lock Lock { get; private set; }

		public bool IsLocked(uint clientId)
		{
			//TODO return right value
			return false;
		}

		public override dynamic GetModel(uint clientId)
        {
            var result = base.GetModel(clientId);
            result.Groups = Groups.Select(group => group.GetModel(clientId));
            //result.Lock = Lock;
            result.Locked = IsLocked(clientId);
            return result;
        }
    }
}
