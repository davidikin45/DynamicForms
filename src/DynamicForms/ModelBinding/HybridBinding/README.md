# ASP.NET Core MVC Hybrid Model Binding And ViewResult to ObjectResult

By default ASP.NET Core doesn't allow a controllers action to handle both Mvc and Api requests. This library allows you to do so. 
I'm not suggesting this be used in production (yet) but as it is all all attribute based it means it can be switched on in Development without affecting production pipeline. 

I think it could be most useful for the folloiwng scenarios:
1. Allowing Developers to Test/Develop/Debug Mvc Forms without worrying about UI.
2. Integration Tests for Mvc without the need of [WebApplicationFactory](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.2).

Currently to create a controller which handles Api and Mvc requests you would need to write something along the lines of below.

```
[Route("contact")]
[HttpGet]
public IActionResult ContactMvc()
{
    return View(new ContactViewModel());
}

[ValidateAntiForgeryToken]
[Route("contact")]
[HttpPost]
public IActionResult ContactMvc(ContactViewModel viewModel)
{
    if(ModelState.IsValid)
    {
        //Submit Contact Form

        return RedirectToAction("Home");
    }

    return View(viewModel);
}

[Route("api/contact")]
[HttpGet]
public ActionResult<ContactViewModel> ContactApi()
{
    return new ContactViewModel();
}

[Route("api/contact")]
[HttpPost]
public IActionResult ContactApi(ContactViewModel viewModel)
{
    if (ModelState.IsValid)
    {
        //Submit Contact Form
        return Ok();
    }
            
    return ValidationProblem(ModelState);
}
```

* This library give thes ability to add attributes/conventions which allow an Mvc controller action to return and accept data as if it were an Api action method. An example of the attributes required can be seen below.

```
[ViewResultToObjectResult]
[Route("contact")]
[HttpGet]
public IActionResult ContactMvc()
{
    return View(new ContactViewModel());
}

[AutoValidateFormAntiForgeryToken]
[Route("contact")]
[HttpPost]
public IActionResult ContactMvc([FromBodyAndModelBinding] ContactViewModel viewModel)
{
    if(ModelState.IsValid)
    {
        //Submit Contact Form

        return RedirectToAction("Home");
    }

    return View(viewModel);
}
```
* There are two conventions which add required attributes and switch [ValidateAntiForgeryToken] > [AutoValidateFormAntiForgeryToken]. This ensures AntiForgeryToken still occurs for Mvc but is bypassed for Api requests.

```
 services.AddMvc(options =>
{
	if(HostingEnvironment.IsDevelopment())
	{
	    //Post data to MVC Controller from API
		options.Conventions.Add(new FromBodyAndOtherSourcesAttributeConvention(true, true, true));
		//Return data uisng output formatter when acccept header is application/json or application/xml
		options.Conventions.Add(new ConvertViewResultToObjectResultConvention());
	}
});

[Route("contact")]
[HttpGet]
public IActionResult ContactMvc()
{
    return View(new ContactViewModel());
}

[ValidateAntiForgeryToken]
[Route("contact")]
[HttpPost]
public IActionResult ContactMvc(ContactViewModel viewModel)
{
    if(ModelState.IsValid)
    {
        //Submit Contact Form

        return RedirectToAction("Home");
    }

    return View(viewModel);
}
```

* By default only [JsonInputFormatter](https://github.com/aspnet/Mvc/blob/master/src/Microsoft.AspNetCore.Mvc.Formatters.Json/JsonInputFormatter.cs) binds dynamic as JObject. [ComplexTypeModelBinderProvider](https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.Core/src/ModelBinding/Binders/ComplexTypeModelBinder.cs) doesn't bind to dynamic so I have created an additional optional ModelBinder which allows the same functionality for Mvc.
* https://github.com/aspnet/AspNetCore/issues/1748
* https://stackoverflow.com/questions/9450619/using-dynamic-objects-with-asp-net-mvc-model-binding

```
 services.AddMvc(options =>
{
	if(HostingEnvironment.IsDevelopment())
	{
		//Post data to MVC Controller from API
		options.Conventions.Add(new FromBodyAndOtherSourcesAttributeConvention(true, true, true));
		//Return data uisng output formatter when acccept header is application/json or application/xml
		options.Conventions.Add(new ConvertViewResultToObjectResultConvention());
	}
})
.AddDynamicModelBinder();

[Route("contact")]
[HttpGet]
public IActionResult ContactMvc()
{
    return View(new ContactViewModel());
}

[ValidateAntiForgeryToken]
[Route("contact")]
[HttpPost]
public IActionResult ContactMvc(dynamic viewModel)
{
    //Submit Contact Form

    return RedirectToAction("Home");
}
```

## Authorization 
* [AutoValidateFormAntiforgeryTokenAttribute] ensures only Post requests with Form content-type is checked for AntiForgeryToken.

## Model Binding Attributes
* https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-2.2
* https://andrewlock.net/model-binding-json-posts-in-asp-net-core/
* https://stackoverflow.com/questions/45495432/asp-net-core-mvc-mixed-route-frombody-model-binding-validation
* https://github.com/billbogaiv/hybrid-model-binding

* [FromBodyOrFormAttribute] = Binds Model to Body or Form
* [FromBodyOrQueryAttribute] = Binds Model to Body or Query
* [FromBodyOrRouteAttribute] = Binds Model to Body or Route
* [FromBodyOrFormRouteQueryAttribute] or [FromBodyOrModelBindingAttribute] = Binds Model to Body or Form/Route/Query

* [FromBodyAndQueryAttribute] = Binds Model to Body and Query
* [FromBodyAndRouteAttribute] = Binds Model to Body and Route
* [FromBodyFormAndRouteQueryAttribute] or [FromBodyAndModelBindingAttribute] = Binds Model to Body/Form and Route/Query

* [FromBodyExplicitAttribute] = If conventions are used to change [FromBody] attributes this can be used to prevent doing so.

## Output Formatting Attributes
* [ConvertViewResultToObjectResultAttribute] = Converts ViewResult to ObjectResult when Accept header matches output formatter SupportedMediaTypes.

## Conventions
* FromBodyAndOtherSourcesAttributeConvention = Adds required attributes to all Controllers, Actions and Parameters. Good for Development environment. In production only recommending passing true for first arguement which applys convention to params with no binding source.
* FromBodyOrOtherSourcesAttributeConvention = Adds required attributes to all Controllers, Actions and Parameters. Good for Development environment. In production only recommending passing true for first arguement which applys convention to params with no binding source.

## Authors

* **Dave Ikin** - [davidikin45](https://github.com/davidikin45)

## License

This project is licensed under the MIT License