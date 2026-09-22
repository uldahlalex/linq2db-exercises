using Facet;
using Infa;

namespace API.Dtos;

[Facet(typeof(Book), [nameof(Book.Authors)], GenerateToSource = false)]
public partial record BookResponse;

/// <summary>
///     <see cref="Book.IsOutOfPrint" /> is excluded: every new book starts in print, the server decides
///     that, not the caller.
/// </summary>
[Facet(typeof(Book), [nameof(Book.Id), nameof(Book.CreatedAtUtc), nameof(Book.IsOutOfPrint), nameof(Book.Authors)], GenerateToSource = false)]
public partial record BookCreateRequest;

/// <summary>Every property except <c>Id</c> is optional: a null one is left alone.</summary>
[Facet(typeof(Book),
    [nameof(Book.Id), nameof(Book.CreatedAtUtc), nameof(Book.Authors)],
    NullableProperties = true,
    GenerateToSource = false)]
public partial record BookUpdateRequest
{
    public string Id { get; init; } = "";
}

/// <summary>The whole mutable row: every property is required as the entity requires it, and a null one clears the column.</summary>
[Facet(typeof(Book), [nameof(Book.CreatedAtUtc), nameof(Book.Authors)], GenerateToSource = false)]
public partial record BookReplaceRequest;
