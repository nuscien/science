using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Net;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// Unit test suite for JSON operation.
/// </summary>
[TestClass]
public class JsonOperationTest
{
    [TestMethod]
    public async Task TestOperationAsync()
    {
        var api = new JsonOperationApi();
        api.RegisterRange(typeof(JsonOperationRegistry));
        var desc = JsonOperations.CreateDescription(api);
        Assert.IsNotNull(desc);
        var yaml = desc.ToJson().ToYamlString();
        Assert.IsNotNull(yaml);
        var resp = await api.ProcessAsync("/test/something", HttpMethod.Post, new JsonObjectNode()
        {
            { "name", "Unit test" },
            { "num", 1000 }
        }, null);
        Assert.IsNotNull(resp);
        Assert.AreEqual("Unit test", resp.TryGetStringValue("name"));
        Assert.AreEqual(1000, resp.TryGetInt32Value("num"));
        resp = await api.ProcessAsync("/test/a", HttpMethod.Post, new JsonObjectNode()
        {
            { "value", "test" }
        }, null);
        Assert.IsTrue(resp.TryGetStringValue("url").Contains("v=test"));
    }
}

public static class JsonOperationRegistry
{
    [JsonOperationPath("/test/a")]
    public static TestRoutedJsonOperation AnOperation { get; set; } = new();

    [JsonOperationPath("/test/something")]
    [System.ComponentModel.Description("Do something.")]
    public static Task<TestDataModel> DoSomething(TestDataModel m, CancellationToken cancellation = default)
        => Task.FromResult(m);
}

public class TestDataModel
{
    [JsonPropertyName("name")]
    [System.ComponentModel.Description("The name.")]
    public string Name { get; set; }

    [JsonPropertyName("num")]
    [System.ComponentModel.Description("A number.")]
    public int Number { get; set; }

    [JsonIgnore]
    public object Tag { get; set; }
}

public class TestRoutedJsonOperation : BaseRoutedJsonOperation
{
    public TestRoutedJsonOperation() : base(new Uri("https://www.kingcean.net"), "test", "This is a test routed JSON operation.")
    {
        Register("key", "k", "This is a key.");
        Register("value", "v", "This is a value.");
        ResultSchema = new JsonObjectSchemaDescription();
    }

    protected override JsonOperationPathAttribute GetPathInfo()
        => new("/test/an");

    protected override Task<JsonObjectNode> SendAsync(JsonHttpClient<JsonObjectNode> http, string url, RoutedJsonOperationContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new JsonObjectNode
        {
            { "state", true },
            { "url", url }
        });
    }
}
