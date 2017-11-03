using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public class Instance : ICheckList
	{
		//private static Dictionary<InstanceKey, Instance> _instances = new Dictionary<InstanceKey, Instance>(10);
		//private List<Client> _clients = new List<Client>();

		private int _itemsSequence;

		//[MethodImpl(MethodImplOptions.Synchronized)]
		//private void Close()
		//{
		//    TrySave();
		//    _instances.Remove(Key);
		//}

		//public void Add(Client client)
		//{
		//    _clients.Add(client);
		//}

		//public void Remove(Client client)
		//{
		//    _clients.Remove(client);
		//    if (_clients.Count() == 0)
		//        Close();
		//}

		public void SetAsChanged(string modifiedBy)
		{
			HasChanges = true;
			ModifiedBy = modifiedBy;
			ModifiedOn = DateTime.Now;
		}

		public Group Header { get; set; }

		public List<Page> Pages { get; set; }

		public string Title { get; set; }

		public Formula Description { get; set; }

		public DateTime? ModifiedOn { get; private set; }

		public string ModifiedBy { get; private set; }

		[JsonIgnore]
		public bool HasChanges { private get; set; }

		public Dictionary<string, Expression> Expressions { get; set; }

		public Dictionary<string, string> ExternalSources { get; set; }

		public Dictionary<string, Dictionary<string, string>> ExternalSourceRows { get; set; }

		//public InstanceKey Key { get; set; }

		[JsonIgnore]
		public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>(10);

		[JsonIgnore]
		public Dictionary<string, Element> Elements { get; } = new Dictionary<string, Element>();

		public string PrototypeState { get; set; }

		public string State { get; set; }

		public string Note { get; set; }

		public void AddItem(Item item)
		{
			if (item.Id == 0)
				item.Id = ++_itemsSequence;
			else
				_itemsSequence = Math.Max(_itemsSequence, item.Id);

			Items.Add(item.Id, item);
		}

		public void Setup()
		{
			Header.CheckList = this;
			Header.Setup();

			foreach (var page in Pages)
			{
				page.CheckList = this;
				page.Setup();
			}

			foreach (var expression in Expressions.Values)
				expression.Parse(this);
		}

		public void Update()
		{
			foreach (var expression in Expressions.Values)
			{
				expression.Parse(this);
				expression.Evaluate(this);
			}

			Description?.Update(Expressions, null);

			Header.Update();
			foreach (var page in Pages)
				page.Update();
		}

		public void UpdateValues(uint clientId)
		{
			Header.UpdateValues(clientId);
			foreach (var page in Pages)
				page.UpdateValues(clientId);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public object GetModel(uint clientId)
		{
			Update();
			return new
			{
				ClientId = clientId,
				//Key,
				Description = Description.Value,
				Header = Header.GetModel(clientId),
				Pages = Pages.Select(page => page.GetModel(clientId)),
				State,
				PrototypeState,
				Expressions = Expressions.Select(value => new { Expression = value.Key, Type = value.Value?.Value?.Type.TypeName ?? "", Value = value.Value?.Value?.Value ?? "" }),
				ExternalSources,
				ExternalSourceRows
			};
		}

		//     public bool TrySave()
		//     {
		//         try
		//         {
		//             if (HasChanges && PrototypeState != "Draft" && PrototypeState != "In simulation")
		//                 using (var db = new EntitiesContainer())
		//                     Save(db, null);
		//             return true;
		//         }
		//         catch
		//         {
		//             return false;
		//         }
		//     }

		//     [MethodImpl(MethodImplOptions.Synchronized)]
		//     public InstanceKey Save(EntitiesContainer context, string state, string note = null)
		//     {
		//         if (PrototypeState == "Draft")
		//             throw new Exception("Instances of draft prototype versions cannot be saved");

		//         Update();
		//         InstanceKey key = this.Key;

		//         if (key.Number < 0 && PrototypeState != "Draft" && PrototypeState != "In simulation")
		//         {
		//             //draft > in progress
		//             if (!Header.Valid)
		//                 throw new Exception("The header is not valid");

		//             ValidateExpressions(context);
		//}

		//var changingState = false;
		//string oldState = null;
		//if (!string.IsNullOrEmpty(state) && this.State != state)
		//         {
		//             //change of state
		//             var unvalidItem = Items.Values.LastOrDefault(i => !i.Valid && !i.IteratorModelItem);
		//             if (state == "Confirmed" && unvalidItem != null)
		//                 throw new Exception($"The item with id {unvalidItem.Id} is not valid");

		//             if (!Prototypes.Get(context, key.Prototype).PrototypeTypes.UserFunctions.Any(x => x.User == Security.CurrentUser.Code && x.Function == "InstanceApprover"))
		//                 throw new Exception("Instance approve not allowed for this prototype type");

		//             oldState = this.State;
		//             this.State = state;
		//	changingState = true;
		//         }

		//         if (!string.IsNullOrEmpty(note))
		//             this.Note = note;

		//         if (key.Number < 0)
		//             key = Entities.Instances.Create(context, this);
		//         else
		//             Entities.Instances.Update(context, this);

		////In caso di create (open), this.Key contiene la vecchia chiave temporanea, Key invece mantiene la nuova chiave
		//if (this.Key.Number < 0) //Instance open
		//{
		//	Events.Insert("Instance created", $"{key.Prototype}.{key.Version}.{key.Year}.{key.Number}", null, key.ToString());
		//	History.Insert(new History
		//	{
		//		Prototype = key.Prototype,
		//		Version = key.Version,
		//		Year = key.Year,
		//		Number = key.Number,
		//		State = PrototypeState == "In simulation" ? "Simulation evidence" : "In progress",
		//		User = Security.CurrentUser.Code,
		//		Date = DateTime.Now
		//	});
		//}
		//if (changingState)
		//{
		//	Events.Insert("Instance state changed", $"{key.Prototype}.{key.Version}.{key.Year}.{key.Number} - Old state: {oldState} - New state: {state}" + (note == null ? "" : " - Comment: " + note), null, key.ToString());
		//	History.Insert(new History
		//	{
		//		Prototype = key.Prototype,
		//		Version = key.Version,
		//		Year = key.Year,
		//		Number = key.Number,
		//		State = this.State,
		//		User = Security.CurrentUser.Code,
		//		Date = DateTime.Now,
		//                 SignatureComment = note
		//	});

		//             if (state == "Confirmed")
		//                 SaveToFile(context);
		//}

		//HasChanges = false;
		//         return key;
		//     }

		public void SetExternalSourceRow(string name, Dictionary<string, string> row)
		{
			if (ExternalSourceRows == null) ExternalSourceRows = new Dictionary<string, Dictionary<string, string>>();
			ExternalSourceRows[name] = row;
			Update();
		}

		//private void ValidateExpressions(EntitiesContainer context)
		//{
		//    var instances = Entities.Instances.Get(context, Key.Prototype).Where(i => i.InstanceState == InstanceStates.PROGRESS);
		//    foreach (var instance in instances)
		//    {
		//        var key = new InstanceKey(instance.Prototype, instance.Version, instance.Year, instance.Number);
		//        if (CheckLists.Instance._instances.ContainsKey(key))
		//            ValidateExpressions(CheckLists.Instance._instances[key]);
		//        else
		//            ValidateExpressions(instance);
		//    }
		//}

		//private void ValidateExpressions(Instance instance)
		//{
		//	var list = Expressions.Where(e =>
		//		e.Value.Unique
		//		&& instance.Expressions.ContainsKey(e.Key)
		//		&& !e.Value.Value.IsNull
		//		&& !instance.Expressions[e.Key].Value.IsNull
		//		&& instance.Expressions[e.Key].SerializedValue == e.Value.Value.SerializedValue
		//	);

		//	if (list.Any())
		//	{
		//		var expression = list.First();
		//		throw new Exception($"The value {expression.Value.Value.SerializedValue} for the expression {expression.Key} has been found in the in progress instance {instance.Key.Year} - {instance.Key.Number}");
		//	}
		//}

		//private void ValidateExpressions(Instances instance)
		//{
		//    var list = Expressions.Where(e =>
		//        e.Value.Unique
		//        && !e.Value.Value.IsNull
		//        && instance.InstanceExpressionValues.Any(
		//            e2 =>
		//                e2.Code == e.Key
		//                && e2.Value == e.Value.Value?.SerializedValue
		//        )
		//    );

		//    if (list.Any())
		//    {
		//        var expression = list.First();
		//        throw new Exception($"The value {expression.Value.Value.SerializedValue} for the expression {expression.Key} has been found in the in progress instance {instance.Year} - {instance.Number}");
		//    }
		//}

		//private void AddTags(Dictionary<string, InstanceTags> tags, Group group)
		//{
		//	if (group != null)
		//		foreach (var item in group.Items)
		//			if (item is Group subgroup)
		//				AddTags(tags, subgroup);
		//			else if (!string.IsNullOrWhiteSpace(item.Description) &&
		//			         (item.TypeName == "Field" || item.TypeName == "Signature") && !tags.ContainsKey(item.Description))
		//				tags[item.Description] = new InstanceTags
		//				{
		//					Prototype = Key.Prototype,
		//					Version = Key.Version,
		//					Year = Key.Year,
		//					Number = Key.Number,
		//					Tag = item.Description,
		//					Value = (item.TypeName == "Field" ? ((Field)item).GetValue() : ((Signature)item).SignedBy) ?? string.Empty
		//				};
		//}

		//public IEnumerable<InstanceTags> CreateTags()
		//{
		//	var tags = new Dictionary<string, InstanceTags>();
		//	AddTags(tags, Header);
		//	return tags.Values;
		//}

		//public IEnumerable<InstanceTags> CreateExtpressionValues()
		//{
		//	var tags = new Dictionary<string, InstanceTags>();
		//	AddTags(tags, Header);
		//	return tags.Values;
		//}

		#region Static

		//     [MethodImpl(MethodImplOptions.Synchronized)]
		//     public static Instance Create(EntitiesContainer context, uint clientId, string prototypeCode, int version)
		//     {
		//         var prototype = CheckLists.Prototype.Load(context, prototypeCode, version);

		//         if (prototype.State != "Draft" && prototype.State != "In simulation" && !Prototypes.Get(context, prototypeCode).PrototypeTypes.UserFunctions.Any(x => x.User == Security.CurrentUser.Code && x.Function == "InstanceCreator"))
		//             throw new Exception("Instance creation not allowed for this prototype type");

		//         var key = new InstanceKey(prototypeCode, version, DateTime.Today.Year, -(int)clientId);
		//         if (_instances.ContainsKey(key))
		//             return _instances[key];

		//         if (prototype.State != "Draft" && prototype.State != "In simulation" && prototype.State != "Active")
		//             throw new Exception("Instance of prototype versions with state " + prototype.State + " not allowed");

		//         var instance = new Instance
		//         {
		//             Key = key,
		//             Header = prototype.Header,
		//             Pages = prototype.Pages,
		//             Expressions = prototype.Expressions,
		//             ExternalSources = prototype.ExternalSources,
		//             ExternalSourceRows = prototype.ExternalSources.ToDictionary(x => x.Key, x => new Dictionary<string, string>()),
		//             Title = prototype.Description,
		//             Description = new Formula(prototype.InstanceDescription),
		//             PrototypeState = prototype.State,
		//             State =
		//                 prototype.State == "In simulation" ? InstanceStates.SIMULATION :
		//                 prototype.State == "Active" ? InstanceStates.DRAFT :
		//                 null
		//         };

		//         instance.Setup();
		//         _instances.Add(instance.Key, instance);
		//         Events.Insert("Instance created", $"Prototype: {prototypeCode} - Version: {version}", null, instance.Key.ToString());
		//         return instance;
		//     }

		//     [MethodImpl(MethodImplOptions.Synchronized)]
		//     public static Instance Open(EntitiesContainer context, InstanceKey instanceKey, bool trace = true)
		//     {
		//         Instance instance;
		//         if (_instances.ContainsKey(instanceKey))
		//             instance = _instances[instanceKey];
		//         else
		//         {
		//             instance = Load(context, instanceKey);
		//             instance.Key = instanceKey;
		//             _instances.Add(instanceKey, instance);
		//         }
		//if (trace) Events.Insert("Instance opened", $"{instanceKey.Prototype}.{instanceKey.Version}.{instanceKey.Year}.{(instanceKey.Number > 0 ? instanceKey.Number.ToString() : "?")}", null, instanceKey.ToString());
		//         return instance;
		//     }

		//     public static Instance Load(EntitiesContainer context, InstanceKey instanceKey)
		//     {
		//         var row = Entities.Instances.Get(context, instanceKey.Prototype, instanceKey.Version, instanceKey.Year, instanceKey.Number);
		//         if (row == null) throw new NullReferenceException("Instance not found");

		//         var instance = Deserialize(row.Data);
		//         instance.State = row.InstanceState;
		//         instance.Setup();
		//         return instance;
		//     }

		public static string Serialize(Instance instance)
		{
			return JsonConvert.SerializeObject(instance, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
		}

		public static Instance Deserialize(string instance)
		{
			return JsonConvert.DeserializeObject<Instance>(instance, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
		}

		#endregion
	}
}
