using System.Text.Json;
using Demolite.Http.Interfaces;

namespace Demolite.Http.Serialization;

public abstract class AbstractSerializer
{
	public virtual JsonSerializerOptions CreateSerializerOptions()
	{
		return new JsonSerializerOptions()
		{
			WriteIndented = true,
		};
	}
	
	public T? Deserialize<T>(string json)
		=> JsonSerializer.Deserialize<T>(json, CreateSerializerOptions());

	public T? DeserializeOrDefault<T>(IHttpResponse response)
	{
		if (!response.IsSuccess || response.ResponseBody == null)
			return default;
		
		return JsonSerializer.Deserialize<T>(response.ResponseBody, CreateSerializerOptions());
	}

	public string Serialize<T>(T obj)
		=> JsonSerializer.Serialize(obj, CreateSerializerOptions());
}