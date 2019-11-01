using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;

namespace CheckListManager.Common.Logic.Api.MessageHandlers
{
    public class DateTimeOffsetHandler : DelegatingHandler
		{
			private readonly Dictionary<Type, List<PropertyInfo>> _properties = new Dictionary<Type, List<PropertyInfo>>();
			
			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var timeZone = request.Headers.GetValues("CompanyTimeZone").SingleOrDefault()
				               ?? "SA Western Standard Time";
				
				var response = await base.SendAsync(request, cancellationToken);
				if (response.Content is ObjectContent content)
				{
					Traverse(content.Value, (propertyInfo, container, value) =>
					{
						if (value is DateTimeOffset dateTimeOffset)
						{
							propertyInfo.SetValue(container, dateTimeOffset.ToLocalTime(timeZone));
						}
					});
				}
				return response;
			}

			private void Traverse(object o, Action<PropertyInfo, object, object> onEachPropertyAction)
			{
				if (o == null)
				{
					return;
				}

				var properties = GetProperties(o.GetType());
				foreach (var property in properties)
				{
					var value = property.GetValue(o);
					if (value != null)
					{
						onEachPropertyAction(property, o, value);
						if (!IsSimple(property.PropertyType))
						{
							if (IsCollection(property.PropertyType))
							{
								var enumerator = ((IEnumerable)value).GetEnumerator();
								while (enumerator.MoveNext())
								{
									Traverse(enumerator.Current, onEachPropertyAction);
								}
							}
							else
							{
								Traverse(value, onEachPropertyAction);
							}
						}
					}
				}
			}

			private bool IsSimple(Type type)
				=> type.IsPrimitive
				   || type == typeof(string)
				   || type == typeof(DateTime)
				   || type == typeof(DateTimeOffset);

			private bool IsCollection(Type type)
				=> type.GetInterface(nameof(IEnumerable)) != null;

			private List<PropertyInfo> GetProperties(Type type)
			{
				if (_properties.ContainsKey(type))
				{
					return _properties[type];
				}
				
				var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
				_properties[type] = properties;

				return properties;
			}
		}
}