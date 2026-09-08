using NJsonSchema;
using NJsonSchema.Generation;

namespace API;

public class RequireNotNullableSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        foreach (var property in context.Schema.ActualProperties)
            if (!property.Value.IsNullable(SchemaType.OpenApi3))
                property.Value.IsRequired = true;
    }
}
