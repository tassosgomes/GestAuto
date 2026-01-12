using FluentAssertions;
using GestAuto.Stock.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;

namespace GestAuto.Stock.UnitTest;

public class ApiV1RoutePrefixConventionTests
{
    [Fact]
    public void Apply_WhenRouteIsRelative_ShouldPrefixWithApiV1()
    {
        // Arrange
        var applicationModel = BuildApplicationModelWithRouteTemplate("vehicles");
        var sut = new ApiV1RoutePrefixConvention();

        // Act
        sut.Apply(applicationModel);

        // Assert
        applicationModel.Controllers[0].Selectors[0].AttributeRouteModel!.Template
            .Should().Be("api/v1/vehicles");
    }

    [Fact]
    public void Apply_WhenRouteAlreadyStartsWithApi_ShouldNotDoublePrefix()
    {
        // Arrange
        var applicationModel = BuildApplicationModelWithRouteTemplate("api/v1/vehicles");
        var sut = new ApiV1RoutePrefixConvention();

        // Act
        sut.Apply(applicationModel);

        // Assert
        applicationModel.Controllers[0].Selectors[0].AttributeRouteModel!.Template
            .Should().Be("api/v1/vehicles");
    }

    [Fact]
    public void Apply_WhenRouteIsAbsolute_ShouldNotPrefix()
    {
        // Arrange
        var applicationModel = BuildApplicationModelWithRouteTemplate("~/health");
        var sut = new ApiV1RoutePrefixConvention();

        // Act
        sut.Apply(applicationModel);

        // Assert
        applicationModel.Controllers[0].Selectors[0].AttributeRouteModel!.Template
            .Should().Be("~/health");
    }

    private static ApplicationModel BuildApplicationModelWithRouteTemplate(string template)
    {
        var controller = new ControllerModel(typeof(DummyController).GetTypeInfo(), Array.Empty<object>())
        {
            ControllerName = "Dummy"
        };

        controller.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(template))
        });

        var applicationModel = new ApplicationModel();
        applicationModel.Controllers.Add(controller);
        return applicationModel;
    }

    private sealed class DummyController : ControllerBase
    {
    }
}