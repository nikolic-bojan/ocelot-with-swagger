# Ocelot Gateway with "auto-generated" Swagger

I wanted to have my Gateway work with Ocelot library. In general, that was not an issue. 

Problem was that we already had Gateway that was an API project. It had controllers, actions, typed responses and of course - Swagger UI.

I was looking online for a solution to join Ocelot and Swagger somehow. Solutions I stumbled upon online mostly pointed to maintain Swagger file manually, but the majority were not too eager to go that way. Ocelot library author said it would be impossible for him to create some middleware to figure out the definitions of underlying services.

That is why I created this workaround:
1. Keep my Old Gateway and maintain it with controllers, actions and a model (easy task).
2. Create a test in Test project that will use WebApplicationFactory to start Old Gateway, retrieve generated ``swagger.json`` and save it to a new Ocelot Gateway's ``wwwroot`` folder.
3. Create new Ocelot Gateway, setup routes, add SwaggerUI and use that generated ``swagger.json`` that is saved to ``wwwroot`` folder.

Old Gateway is not deployed to servers, but new Ocelot Gateway. When I hit ``/swagger/`` on my new Ocelot Gateway, I have exactly the same Swagger and no one can tell that I am running Ocelot now!

You can read more details in blog post https://dev.to/nikolicbojan/ocelot-gateway-with-auto-generated-swagger-4ofc

BR,
Bojan
