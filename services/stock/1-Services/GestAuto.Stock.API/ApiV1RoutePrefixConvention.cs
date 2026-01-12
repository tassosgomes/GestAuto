using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GestAuto.Stock.API;

public sealed class ApiV1RoutePrefixConvention : IApplicationModelConvention
{
    private static readonly AttributeRouteModel RoutePrefix = new(new Microsoft.AspNetCore.Mvc.RouteAttribute("api/v1"));

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel is null)
                {
                    continue;
                }

                var template = selector.AttributeRouteModel.Template;
                if (string.IsNullOrWhiteSpace(template))
                {
                    continue;
                }

                // Absolute routes should not be prefixed (e.g., "~/health").
                if (template.StartsWith("~/", StringComparison.Ordinal) || template.StartsWith("/", StringComparison.Ordinal))
                {
                    continue;
                }

                // Avoid double-prefixing when a controller already includes "api/..." in its route.
                if (template.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(RoutePrefix, selector.AttributeRouteModel);
            }
        }
    }
}
