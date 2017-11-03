using System;
using System.Collections.Generic;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public class Prototype : ICheckList
	{
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
			{
				try
				{
					expression.Parse(this);
					expression.Check(this);
				}
				catch (ParseException exception)
				{
					expression.Error = exception.Message + " at position " + exception.Position;
				}
				catch (Exception exception)
				{
					expression.Error = exception.Message;
				}
			}

			//TODO: get right value
			//CanApprove = UserFunctions.Contains(Security.CurrentUser.Code, Company, Department, PrototypeType, UserFunctions.PROTOTYPEAPPROVER);
		}

		public List<Page> Pages { get; } = new List<Page>();

		[JsonIgnore]
		public Dictionary<string, Element> Elements { get; } = new Dictionary<string, Element>();

		public Dictionary<string, Expression> Expressions { get; set; } = new Dictionary<string, Expression>();

		public Dictionary<string, string> ExternalSources { get; set; } = new Dictionary<string, string>();

		public Group Header { get; set; }

		public string Code { get; set; }

		public int Version { get; set; }

		public string Company { get; set; }

		public string Department { get; set; }

		public string PrototypeType { get; set; }

		public string Description { get; set; }

		public string InstanceDescription { get; set; }

		public DateTimeOffset? ValidFrom { get; set; }

		public DateTimeOffset? ValidUntil { get; set; }

		public string State { get; set; }

		public string Note { get; set; }

		public bool CanApprove { get; set; }

		//[MethodImpl(MethodImplOptions.Synchronized)]
		//public void Save(EntitiesContainer context, bool isNew, string templateCode, int? templateVersion)
		//{
		//	if (string.IsNullOrWhiteSpace(Code)) throw new Exception("Specify a valid code");
		//	if (string.IsNullOrWhiteSpace(Description)) throw new Exception("Specify a valid description");
		//	if (string.IsNullOrWhiteSpace(Company)) throw new Exception("Specify a valid company");
		//	if (string.IsNullOrWhiteSpace(Department)) throw new Exception("Specify a valid department");
		//	if (string.IsNullOrWhiteSpace(PrototypeType)) throw new Exception("Specify a valid prototype type");

		//	if (isNew)
		//	{
		//		//new version/prototype
		//		var prototype = Prototypes.Get(context, Code);
		//		if (prototype == null)
		//		{
		//			//new prototype
		//			prototype = new Prototypes
		//			{
		//				Code = Code,
		//				Company = Company,
		//				Department = Department,
		//				PrototypeType = PrototypeType,
		//				Description = Description,
		//				CreatedOn = DateTime.Now,
		//				CreatedBy = Security.CurrentUser.Code
		//			};
		//			context.Prototypes.Add(prototype);
		//		}

		//		prototype.Versions.Add(new Versions
		//		{
		//			Description = Description,
		//			VersionState = this.State = VersionStates.DRAFT,
		//			Version = this.Version = (prototype.Versions.Count == 0 ? 0 : prototype.Versions.Max(v => v.Version)) + 1,
		//			Data = Serialize(this),
		//			CreatedOn = DateTime.Now,
		//			CreatedBy = Security.CurrentUser.Code
		//		});

		//		context.SaveChanges();
		//	}
		//	else
		//	{
		//		//update
		//		var prototype = Prototypes.Get(context, Code);
		//		if (prototype == null)
		//			throw new Exception($"Prototype {Code} not found");
		//		var version = prototype.Versions.FirstOrDefault(v => v.Version == this.Version);
		//		if (version == null)
		//			throw new Exception($"Version {this.Version} on Prototype {Code} not found");

		//		if (State == "Draft")
		//		{
		//			prototype.Description = Description;
		//			prototype.ModifiedOn = DateTime.Now;
		//			prototype.ModifiedBy = Security.CurrentUser.Code;
		//			version.Description = Description;
		//			version.Data = Serialize(this);
		//			version.ValidFrom = ValidFrom;
		//			version.ValidUntil = ValidUntil;

		//			version.ModifiedOn = DateTime.Now;
		//			version.ModifiedBy = Security.CurrentUser.Code;
		//			if (RowVersion != null) version.SetOriginalRowVersion(context, RowVersion);
		//			context.SaveChanges();
		//			RowVersion = version.RowVersion;
		//		}
		//	}
		//}

		//public static Prototype Load(EntitiesContainer context, string prototype, int? version)
		//{
		//	var row = version.HasValue && version > 0 ? Versions.Get(context, prototype, version.Value) : null;
		//	return row == null ? null : Deserialize(row);
		//}

		public static string Serialize(Prototype prototype)
		{
			return JsonConvert.SerializeObject(prototype, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
		}

		public static Prototype Deserialize(PrototypeVersions version)
		{
			var prototype = JsonConvert.DeserializeObject<Prototype>(version.Data, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, CheckAdditionalContent = false });
			prototype.State = version.VersionState;
			prototype.Version = version.Version;
			prototype.ValidFrom = version.ValidFrom;
			prototype.ValidUntil = version.ValidUntil;
			prototype.Setup();
			return prototype;
		}

		public void AddItem(Item item)
		{
		}

		//private void AddTags(Dictionary<string, VersionTags> tags, Group @group)
		//{
		//	if (@group != null)
		//		foreach (var item in @group.Items)
		//			if (item is Group)
		//				AddTags(tags, (Group)item);
		//			else if (!string.IsNullOrWhiteSpace(item.Description) && (item.TypeName == "Field" || item.TypeName == "Signature") && !tags.ContainsKey(item.Description))
		//				tags[item.Description] = (new VersionTags { Prototype = Code, Version = Version, Code = item.Description });
		//}

		//public IEnumerable<VersionTags> CreateTags()
		//{
		//	var tags = new Dictionary<string, VersionTags>();
		//	AddTags(tags, Header);
		//	return tags.Values;
		//}

	}
}
