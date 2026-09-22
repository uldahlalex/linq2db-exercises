using System.ComponentModel.DataAnnotations;
using API.Dtos;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

public partial class AuthorsController
{
    /// <summary>Adds a new author to the catalogue.</summary>
    /// <remarks>
    ///     The one validation rule, a <see cref="ValidationException" />: first and last name are
    ///     required (not null, empty or whitespace).
    /// </remarks>
    /// <exception cref="ValidationException">A name is blank.</exception>
    [HttpPost(nameof(Create))]
    public AuthorResponse Create([FromBody] AuthorCreateRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Changes only the columns the request actually supplies; anything left out is left alone.
    ///     This is the <c>PATCH</c> half of updating: to set a nullable column back to <c>null</c>, use
    ///     <see cref="Replace" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every property on the request except the id is optional: <c>null</c> means "leave this
    ///         alone". There is no way to null a nullable column through this endpoint.
    ///     </para>
    ///     <para>
    ///         The same validation rules as <see cref="Create" /> apply to whichever fields are actually
    ///         supplied. This is idempotent: sending the same request twice leaves the same row state and
    ///         throws nothing the second time.
    ///     </para>
    /// </remarks>
    /// <exception cref="ValidationException">A validation rule is broken.</exception>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpPatch(nameof(Update))]
    public AuthorResponse Update([FromBody] AuthorUpdateRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Replaces the whole mutable row with the request. Every column is overwritten with what was
    ///     sent, nulls included, so this is also how a nullable column is cleared.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The row is found by <see cref="AuthorReplaceRequest.Id" />. <see cref="Author.Id" /> and
    ///         <see cref="Author.CreatedAtUtc" /> are never changed. The validation rules of
    ///         <see cref="Create" /> apply to the values sent.
    ///     </para>
    ///     <para>Idempotent: sending the same request twice leaves the same row state.</para>
    /// </remarks>
    /// <exception cref="ValidationException">A validation rule is broken.</exception>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpPut(nameof(Replace))]
    public AuthorResponse Replace([FromBody] AuthorReplaceRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>Removes an author for good — but only once nothing they wrote is still credited to them.</summary>
    /// <exception cref="KeyNotFoundException">No row has that id, including when it was already deleted.</exception>
    /// <exception cref="InvalidOperationException">The author is still credited on at least one book.</exception>
    [HttpDelete(nameof(Delete))]
    public void Delete([FromQuery] string id)
    {
        throw new NotImplementedException();
    }

    #region Tests: Create

    public class CreateTests : LibraryTest
    {
        private static AuthorCreateRequest Sample()
        {
            return new AuthorCreateRequest(
                "Test",
                "Writer",
                "A brand new author.",
                "Danish",
                "https://example.com",
                new DateOnly(1990, 1, 1));
        }

        [Fact]
        public void Inserts_the_author_and_returns_it()
        {
            var created = AuthorsController.Create(Sample());
            Assert.False(string.IsNullOrEmpty(created.Id));
            Assert.Equal(12, AuthorCount);
            Assert.Equal("Test", AuthorRow(created.Id).FirstName);
        }

        [Fact]
        public void Stamps_the_creation_time()
        {
            var created = AuthorsController.Create(Sample());
            Assert.InRange(AuthorRow(created.Id).CreatedAtUtc, DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(1));
        }

        [Fact]
        public void Rejects_a_blank_name()
        {
            Assert.Throws<ValidationException>(() => AuthorsController.Create(Sample() with { FirstName = "  " }));
            Assert.Throws<ValidationException>(() => AuthorsController.Create(Sample() with { LastName = "" }));
        }

    }

    #endregion

    #region Tests: Update

    public class UpdateTests : LibraryTest
    {
        [Fact]
        public void Changes_only_the_supplied_columns()
        {
            var before = AuthorRow(LibrarySeed.AuthorIdOf(1));
            AuthorsController.Update(new AuthorUpdateRequest { Id = before.Id, FirstName = "Renamed" });

            var after = AuthorRow(before.Id);
            Assert.Equal("Renamed", after.FirstName);
            Assert.Equal(before.LastName, after.LastName);
            Assert.Equal(before.Nationality, after.Nationality);
        }

        [Fact]
        public void Throws_when_the_row_does_not_exist()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                AuthorsController.Update(new AuthorUpdateRequest { Id = Guid.NewGuid().ToString(), FirstName = "Nope" }));
        }

        [Fact]
        public void Throws_when_a_supplied_field_is_invalid()
        {
            var id = LibrarySeed.AuthorIdOf(1);
            Assert.Throws<ValidationException>(() =>
                AuthorsController.Update(new AuthorUpdateRequest { Id = id, FirstName = "  " }));
            Assert.Equal("Elena", AuthorRow(id).FirstName);
        }
    }

    #endregion

    #region Tests: Replace

    public class ReplaceTests : LibraryTest
    {
        private static AuthorReplaceRequest Full(string id)
        {
            return new AuthorReplaceRequest(
                id,
                "New",
                "Name",
                "A new bio.",
                "Danish",
                "https://new.example.com",
                new DateOnly(1980, 2, 3));
        }

        [Fact]
        public void Replaces_every_column()
        {
            var id = LibrarySeed.AuthorIdOf(1);
            AuthorsController.Replace(Full(id));

            var saved = AuthorRow(id);
            Assert.Equal("New", saved.FirstName);
            Assert.Equal("Name", saved.LastName);
            Assert.Equal("A new bio.", saved.Bio);
            Assert.Equal("Danish", saved.Nationality);
            Assert.Equal("https://new.example.com", saved.Website);
            Assert.Equal(new DateOnly(1980, 2, 3), saved.BirthDate);
        }

        [Fact]
        public void Clears_nullable_columns_sent_as_null()
        {
            var id = LibrarySeed.AuthorIdOf(1);
            Assert.NotNull(AuthorRow(id).Website);

            AuthorsController.Replace(Full(id) with { Bio = null, Nationality = null, Website = null, BirthDate = null });

            var saved = AuthorRow(id);
            Assert.Null(saved.Bio);
            Assert.Null(saved.Nationality);
            Assert.Null(saved.Website);
            Assert.Null(saved.BirthDate);
        }

        [Fact]
        public void Is_idempotent()
        {
            var id = LibrarySeed.AuthorIdOf(1);
            AuthorsController.Replace(Full(id));
            var first = AuthorRow(id);
            AuthorsController.Replace(Full(id));
            var second = AuthorRow(id);

            Assert.Equal(first.FirstName, second.FirstName);
            Assert.Equal(first.Website, second.Website);
            Assert.Equal(11, AuthorCount);
        }

        [Fact]
        public void Throws_when_the_row_does_not_exist()
        {
            Assert.Throws<KeyNotFoundException>(() => AuthorsController.Replace(Full(Guid.NewGuid().ToString())));
        }

        [Fact]
        public void Throws_when_a_value_is_invalid_and_changes_nothing()
        {
            var id = LibrarySeed.AuthorIdOf(1);
            Assert.Throws<ValidationException>(() => AuthorsController.Replace(Full(id) with { FirstName = "  " }));
            Assert.Equal("Elena", AuthorRow(id).FirstName);
        }
    }

    #endregion

    #region Tests: Delete

    public class DeleteTests : LibraryTest
    {
        [Fact]
        public void Removes_an_author_with_no_books()
        {
            var id = LibrarySeed.AuthorIdOf(11);
            AuthorsController.Delete(id);
            Assert.Equal(10, AuthorCount);
            Assert.False(AuthorExists(id));
        }

        [Fact]
        public void Refuses_while_a_book_is_still_credited()
        {
            Assert.Throws<InvalidOperationException>(() => AuthorsController.Delete(LibrarySeed.AuthorIdOf(1)));
            Assert.Equal(11, AuthorCount);
        }

    }

    #endregion
}
