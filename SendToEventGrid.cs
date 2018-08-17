#r "Microsoft.Azure.NotificationHubs"
#r "Newtonsoft.Json"

using System;
using System.Text;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.NotificationHubs;
using Newtonsoft.Json;
using System.Collections.Generic; 
using System.Net.Http.Headers;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    string jsonContent = await req.Content.ReadAsStringAsync();
    Order item = JsonConvert.DeserializeObject<Order>(jsonContent);
    log.Info("Item: " + jsonContent);
    log.Info("ItemObject: " + JsonConvert.SerializeObject(item));

    GridEvent<Order> eventMsg = new GridEvent<Order>();
    eventMsg.Id = Guid.NewGuid().ToString();
    eventMsg.Subject = "abcOrder/1/2";
    eventMsg.EventType = "abc.order.addItem";
    eventMsg.EventTime = DateTime.UtcNow;
    eventMsg.Data = item;

    log.Info("Grid Item" + JsonConvert.SerializeObject(eventMsg));

    List<GridEvent<Order>> msges = new List<GridEvent<Order>>();
    msges.Add(eventMsg);

    string topicEndpoint = "https://abc-order.eastus-1.eventgrid.azure.net/api/events";
    
    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, topicEndpoint);
    
    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(msges), Encoding.UTF8, "application/json");

    HttpClient client = new HttpClient();
    client.BaseAddress = new Uri(topicEndpoint);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Add("aeg-sas-key", "MamvCfhu4DPfzvvt7iRAOS8qCQZncDlmSzxJcgJDbss=");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("democlient");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));    

    HttpResponseMessage msg = await client.SendAsync(requestMessage);
    log.Info("Result: " + JsonConvert.SerializeObject(msg));
    return req.CreateResponse(msg);
}

public class Order {
    public string OrderId {get; set;}
    public string ItemName {get; set;}
}

public class GridEvent<T> where T: class
    {
        public string Id { get; set;}
        public string EventType { get; set;}
        public string Subject {get; set;}
        public DateTime EventTime { get; set; } 
        public T Data { get; set; } 
        public string Topic { get; set; }
    }
