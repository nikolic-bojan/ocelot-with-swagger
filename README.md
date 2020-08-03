# Ocelot Gateway with "auto-generated" Swagger

We have a Gateway API that routes all calls to services below. The simplest initial solution was to create it with controllers and to define return types and other annotations, so we could have Swagger UI for other business units that use our "common" services.

I re-invented the wheel with creating a library on top of the ``HttpClient``, just in order to be able to call services below. I was aware of ``Ocelot``, but didn't have a solution to make it work and have Swagger file generated.

Now I have a workaround, that will not require team to learn swagger's JSON/YAML syntax. We can continue working as we did and also have ``Ocelot`` gateway with all its' possibilities.

> TL;DR; 
> I am keeping my **OLD** gateway in order to generate ``swagger.json``, but will not deploy it any more to server. I am also creating new **OCELOT** gateway that will just use that generated ``swagger.json``. Visit Git repository https://github.com/nikolic-bojan/ocelot-with-swagger that contains code and build definition for this transition.


# Initial state

I guess you already opened GitHub repository, so I would like to explain what was the initial state of the solution. There were 2 projects:
1. Gateway - This is initial gateway with controllers and Swashbuckle library to generate and show Swagger UI.
2. Api - This represents some underlying service that gateway is calling.

I added some sample call from Gateway to Api, so you can verify it works. I commented it out in code so we can prove later we do not need it really, but you can uncomment to see it works. Of course, this is super-simplified code, for easier understanding. Grab a look at the ``WeatherForecastController``.

```csharp
[HttpGet]
public async Task<IEnumerable<WeatherForecast>> Get()
{
	var client = _httpClientFactory.CreateClient("api");
	var response = await client.GetAsync("weatherforecast");

	var result = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(await response.Content.ReadAsByteArrayAsync());

	return result;
}
```

OK, that works. We have return type, we can add more annotations, so Swagger definition can be more rich, with samples etc. On start it generates ``swagger.json`` and all is good. All except I use some Ocelot-like library I made, that is proven in Production, but doesn't have all features Ocelot has and it is not battle tested with hundreds of projects, so some edge cases could occur. In Production. Not so fun.


# Googling for the solution

I was hoping there is some solution to jump from internal library to Ocelot without too much fuzz. The conclusions were next:
- There is no automatic solution as it is impossible (https://github.com/ThreeMammals/Ocelot/issues/161)
- Create ``swagger.json`` and maintain it manually (entire team should learn syntax and know what they are doing)
- Somehow generate ``swagger.json`` on each model change and just use it in Ocelot gateway

Second was an option, but I am already pushing my luck with the team with jumping to new framework, changes in our CI, etc. Dropping "we need to learn Swagger syntax" would maybe be too much. Issues with adoption, bugs introduced in first few weeks/months, etc. Not worth of it. Maybe in 2020 :)


# Solution and Pros/Cons

OK, then what? Easy - we can keep the Old Gateway project to generate ``swagger.json`` and just use that in new Ocelot Gateway!

Pros:
- We use Ocelot! With all its' possibilities and extensibility.
- We do not need to learn Swagger specification.
- We change very little how we work.

Cons:
- We have a project that has sole purpose to generate ``swagger.json``.
- We need to add manually ``ReRoutes`` (we had to do similar in Old Gateway).

In my opinion pros/cons looks good. I like it. Someone could say that you can do some magic stuff in internal library with some handlers, but you can also use handlers in Ocelot. Also... magic... not sure if it is good always.


# Generate ``swagger.json`` for Ocelot

Small problem was - how to generate ``swagger.json``. It is generated when application starts. I didn't want Old Gateway to start on run anywhere.

No problem, Test project and **WebApplicationFactory** to the rescue!
You create new (or use existing) Test project and create the following test case.

```csharp
[TestMethod]
public async Task CreateSwaggerJson()
{
	WebApplicationFactory<OldGateway.Startup> factory = new WebApplicationFactory<OldGateway.Startup>();

	var client = factory.CreateClient();

	var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");

	await File.WriteAllTextAsync("../../../../../src/OcelotGateway/wwwroot/swagger.json", await swaggerResponse.Content.ReadAsStringAsync());
}
```

To explain it:
1. **WebApplicationFactory** will run your Old Gateway in-memory.
2. HttpClient will GET ``swagger.json`` that was generated on start.
3. You will save it in new Ocelot Gateway ``wwwroot`` folder so it can be picked up by SwaggerUI middleware.

That is it! Done! You have ``swagger.json`` generated from a project that will never be deployed or ran anywhere!


# Swagger UI in new Ocelot Gateway

With previous step, ``swagger.json`` is already in Ocelot's ``wwwroot`` folder and just needs to be configured with one simple step.

```csharp
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger.json", "My API V1");
});
```

I deliberately added a ``dummy.txt`` file in ``wwwroot`` as I had issue with build on Azure DevOps - as nothing was in that folder, it was not created, so saving ``swagger.json`` from Test case was failing. Now it is all good.


# Azure DevOps build

So, it was all fine in local. I manually run Tests and ``swagger.json`` is saved to Ocelot's ``wwwroot`` folder. Then I start Ocelot Gateway and it picks it up from that folder.

That is not how it will look on a build server. That is why I created a YAML build to show you how it should work.

```yaml
trigger:
- master

pool:
  vmImage: 'windows-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    projects: 'OcelotWithSwagger.sln'
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: 'test/Gateway.Tests/Gateway.Tests.csproj'
    testRunTitle: 'Generate Old Gateway swagger.json'
- task: DotNetCoreCLI@2
  inputs:
    command: 'publish'
    publishWebProjects: false
    projects: 'src/OcelotGateway/OcelotGateway.csproj'
    arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: '$(Build.ArtifactStagingDirectory)'
    ArtifactName: 'OcelotGateway'
    publishLocation: 'Container'
```

It has the following steps:
1. Build entire solution (all projects)
2. Run Gateway.Tests project, to create swagger file
3. Publish Ocelot Gateway
4. Create build artifact (zipped Ocelot Gateway as a single EXE)


# Final words

Now you have all to transform your API based Gateway to Ocelot and keep Swagger UI auto-generated.

TBH, I plan to push my team toward learning Swagger/OpenApi specification, but YAML style. Maybe in 2020, but at least we can now use great library like Ocelot!

I would like to hear comments and questions, so we can improve this some more, if needed.

BR,
Bojan
