using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiEcomm.API.Controllers;

namespace WebApiEcomm.Tests.Integration;

public class AuthControllerContractTests
{
    [Fact]
    public void AuthController_ShouldUseExpectedRoutePrefix()
    {
        var routeAttribute = (RouteAttribute?)Attribute.GetCustomAttribute(typeof(AuthController), typeof(RouteAttribute));
        Assert.NotNull(routeAttribute);
        Assert.Equal("api/v1/auth", routeAttribute!.Template);
    }

    [Fact]
    public void AuthController_LogoutEndpoint_ShouldRequireAuthorization()
    {
        var method = typeof(AuthController).GetMethod("Logout");
        var authorize = method?.GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault();
        Assert.NotNull(authorize);
    }
}
