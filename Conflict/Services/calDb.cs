using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using MudBlazor;
using System.Xml.Linq;
using System.Runtime.InteropServices.JavaScript;

public class calDb
{
	public async Task<string> create(string name, DateRange? dateRange)
	{
		HttpClient client = new HttpClient();
		HttpRequestMessage req = new HttpRequestMessage();
		req.RequestUri = new Uri("https://api.jsonbin.io/v3/b/");
		req.Method = HttpMethod.Post;
		req.Headers.Add("X-Master-Key", "SECRET");
		req.Headers.Add("X-Bin-Name", name);


		if(dateRange != null)
		{
			//string start = dateRange.Start.ToString("dd/MM/yyyy");
			string start = ((DateTime)dateRange.Start).ToString("dd/MM/yyyy");
			string end = ((DateTime)dateRange.End).ToString("dd/MM/yyyy");

			req.Content = new StringContent("{\"Name\":\"" + name + "\", \"Dates\":{}, \"Users\":[], \"Start\":\""+ start +"\", \"End\":\""+ end +"\"}", Encoding.UTF8, "application/json");
		}
		else
			req.Content = new StringContent("{\"Name\":\"" + name + "\", \"Dates\":{}, \"Users\":{}, \"Start\":\""+DateTime.MinValue.ToString("dd/MM/yyyy") + "\", \"End\":\""+DateTime.MaxValue.ToString("dd/MM/yyyy") + "\"}", Encoding.UTF8, "application/json");
		var resp = await client.SendAsync(req);
		JObject respObj = JObject.Parse(await resp.Content.ReadAsStringAsync());
		return respObj["metadata"]["id"].ToString();
	}

	public async Task<JObject> get(string id)
	{
		HttpClient client = new HttpClient();
		HttpRequestMessage req = new HttpRequestMessage();
		req.RequestUri = new Uri("https://api.jsonbin.io/v3/b/" + id);
		req.Method = HttpMethod.Get;
		req.Headers.Add("X-Master-Key", "SECRET");
		var resp = await client.SendAsync(req);
		JObject respObj = JObject.Parse(await resp.Content.ReadAsStringAsync());
		return respObj;
	}

	public async Task<JObject> update(string id, string name, string userId, List<DateTime?> times, bool clearUser = false)
	{
		times.Remove(null);
		dynamic calObj = await get(id);
		Dictionary<string, List<string>> dates = calObj["record"]["Dates"].ToObject<Dictionary<string, List<string>>>();
		Dictionary<string, string> users = calObj["record"]["Users"].ToObject<Dictionary<string, string>>();
		string calName = calObj["record"]["Name"].ToString();
		string start = calObj["record"]["Start"].ToString();
		string end = calObj["record"]["End"].ToString();

		foreach (DateTime obj in times)
		{
			string date = obj.ToString("dd/MM/yyyy");
			if (dates.ContainsKey(date))
			{
				if (!dates[date].Contains(userId))
				{
					dates[date].Add(userId);
				}
			}
			else
			{
				dates.Add(date, new List<string> { userId } );
			}

		}
		if (!users.ContainsKey(userId))
			users.Add(userId, name);
		var l1 = dates.Where(_ => _.Value.Contains(userId)).ToList();
		var l2 = l1.Where(_ => !times.Select(_ => ((DateTime)_).ToString("dd/MM/yyyy")).Contains(_.Key)).ToList();
		foreach (var item in l2)
		{
			dates.Remove(item.Key);
		}

		if (clearUser)
			users.Remove(userId);

		HttpClient client = new HttpClient();
		HttpRequestMessage req = new HttpRequestMessage();
		req.RequestUri = new Uri("https://api.jsonbin.io/v3/b/" + id);
		req.Method = HttpMethod.Put;
		req.Headers.Add("X-Master-Key", "SECRET");
		req.Content = new StringContent("{\"Name\":\"" + calName + "\", \"Dates\":"+ JsonSerializer.Serialize(dates) +", \"Users\":" + JsonSerializer.Serialize(users) + ", \"Start\":\""+start+"\", \"End\":\""+end+"\"}", Encoding.UTF8, "application/json");

		var resp = await client.SendAsync(req);
		JObject respObj = JObject.Parse(await resp.Content.ReadAsStringAsync());
		return respObj;
	}

	public async Task<string> initUser(List<Dictionary<string, string>> cals, string name)
	{
		HttpClient client = new HttpClient();
		HttpRequestMessage req = new HttpRequestMessage();
		req.RequestUri = new Uri("https://api.jsonbin.io/v3/b/");
		req.Method = HttpMethod.Post;
		req.Headers.Add("X-Master-Key", "SECRET");
		req.Headers.Add("X-Bin-Name", name);
		req.Content = new StringContent(JsonSerializer.Serialize(cals), Encoding.UTF8, "application/json");
		var resp = await client.SendAsync(req);
		JObject respObj = JObject.Parse(await resp.Content.ReadAsStringAsync());
		return respObj["metadata"]["id"].ToString();
	}

	public async Task delete(string id)
	{
		HttpClient client = new HttpClient();
		HttpRequestMessage req = new HttpRequestMessage();
		req.RequestUri = new Uri("https://api.jsonbin.io/v3/b/" + id);
		req.Method = HttpMethod.Delete;
		req.Headers.Add("X-Master-Key", "SECRET");
		//req.Headers.Add("X-Access-Key", "$2a$10$t8EhIzeJtwqhh38u2ftVG.N7zT8T3bgrSaHv3fqS3gKTxc9r6RCGS");
		var resp = await client.SendAsync(req);
	}

	public async Task updateUser(List<Dictionary<string, string>> calendars, string id )
	{
		HttpClient client = new HttpClient();
		HttpRequestMessage req = new HttpRequestMessage();
		req.RequestUri = new Uri("https://api.jsonbin.io/v3/b/" + id);
		req.Method = HttpMethod.Put;
		req.Headers.Add("X-Master-Key", "SECRET");
		if(calendars.Count() == 0)
		{
			req.Method = HttpMethod.Delete;
		}
		else
		{
			req.Method = HttpMethod.Put;
			req.Content = new StringContent(JsonSerializer.Serialize(calendars), Encoding.UTF8, "application/json");

		}
		var resp = await client.SendAsync(req);
	}
}
