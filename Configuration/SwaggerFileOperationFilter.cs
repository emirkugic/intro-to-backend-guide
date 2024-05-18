using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var param in operation.Parameters)
        {
            if (param.In == ParameterLocation.Query)
            {
                param.Required = false;
            }
        }

        if (context.ApiDescription.HttpMethod == "POST" || context.ApiDescription.HttpMethod == "PUT")
        {
            if (context.ApiDescription.ParameterDescriptions.Count > 0 && context.ApiDescription.ParameterDescriptions[0].Type == typeof(IFormFile))
            {
                operation.Parameters.Clear();
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["image"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
