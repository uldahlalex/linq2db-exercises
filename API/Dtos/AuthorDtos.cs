using Facet;
using Infa;

namespace API.Dtos;

[Facet(typeof(Author), [nameof(Author.Books)], GenerateToSource = false)]
public partial record AuthorResponse;

[Facet(typeof(Author), [nameof(Author.Id), nameof(Author.CreatedAtUtc), nameof(Author.Books)], GenerateToSource = false)]
public partial record AuthorCreateRequest;

/// <summary>Every property except <c>Id</c> is optional: a null one is left alone.</summary>
[Facet(typeof(Author),
    [nameof(Author.Id), nameof(Author.CreatedAtUtc), nameof(Author.Books)],
    NullableProperties = true,
    GenerateToSource = false)]
public partial record AuthorUpdateRequest
{
    public string Id { get; init; } = "";
}

/// <summary>The whole mutable row: every property is required as the entity requires it, and a null one clears the column.</summary>
[Facet(typeof(Author), [nameof(Author.CreatedAtUtc), nameof(Author.Books)], GenerateToSource = false)]
public partial record AuthorReplaceRequest;
