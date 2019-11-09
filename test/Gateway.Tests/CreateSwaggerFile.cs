using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace TestForSwagger
{
    [TestClass]
    public class CreateSwaggerFile
    {
        [TestMethod]
        public async Task CreateSwaggerJson()
        {
            WebApplicationFactory<OldGateway.Startup> factory = new WebApplicationFactory<OldGateway.Startup>();

            var client = factory.CreateClient();

            var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");

            await File.WriteAllTextAsync("../../../../../src/OcelotGateway/wwwroot/swagger.json", await swaggerResponse.Content.ReadAsStringAsync());
        }
    }
}
