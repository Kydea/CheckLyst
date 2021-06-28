using System;
using Autofac;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Instances.Logic.Models.CheckLists;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Instances.Logic.Services
{
    public class InstanceJsonConverter : JsonConverter
    {
		public InstanceJsonConverter() : base()
		{
		}

        public override bool CanConvert(Type objectType) => typeof(ICheckListElement).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var target = Create(objectType, jsonObject);
            serializer.Populate(jsonObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected Element Create(Type objectType, JObject jsonObject)
		{
			var type = (jsonObject["typeName"] ?? jsonObject["TypeName"])?.ToString();
			switch (type)
			{
				case "Field":
					return new Field();
				case "Group":
					return new Group();
				case "Iteration":
					return new Iteration();
				case "Signature":
					return new Signature();
                case "InstanceLink":
                    return new InstanceLink();
                case "Text":
					return new Text();
                case "Link":
                    return new Link();
                case "Image":
                    return new Image();
                case "Attachment":
                    return new Attachment();
                default:
                    return new Page();
            }
        }
	}
}
