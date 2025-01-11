using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FileUploadOperation : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var requestBody = operation.RequestBody;

        if (requestBody != null && context.MethodInfo.GetParameters().Any(p => p.ParameterType == typeof(IFormFile)))
        {
            // Zorg ervoor dat zowel het bestand als andere velden in de request body worden opgenomen
            requestBody.Content["multipart/form-data"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {

                        { "name", new OpenApiSchema { Type = "string" } },
                        { "userId", new OpenApiSchema { Type = "int" } },
                        { "distance", new OpenApiSchema { Type = "string" } },
                        { "ascent", new OpenApiSchema { Type = "string" } },
                        { "descent", new OpenApiSchema { Type = "string" } },
                        { "difficulty", new OpenApiSchema { Type = "string" } },
                        { "description", new OpenApiSchema { Type = "string" } },
                        { "location", new OpenApiSchema { Type = "string" } },
                        { "gpxFile", new OpenApiSchema { Type = "string", Format = "binary" } },
                        { "photoFiles", new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string", Format = "binary" } } }  // Voeg een array van foto-bestanden toe
                    }
                }
            };
        }
    }
}
