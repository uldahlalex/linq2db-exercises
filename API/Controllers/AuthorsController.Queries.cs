using System.ComponentModel.DataAnnotations;
using API.Dtos;
using API.Enums;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

[Route("[controller]")]
public partial class AuthorsController(LibraryDatabase db) : ControllerBase
{
    /// <summary>Every author, ordered by last name then first name.</summary>
    /// <returns>All 11 seeded authors.</returns>
    [HttpGet(nameof(GetAll))]
    public List<AuthorResponse> GetAll()
    {
        //lookup (if theres no validaiton rules)
        IQueryable<Author> query = db.Authors();

        //Filtering
        //Where(a => a....)
        
        //Ordering
        query = query
            .OrderBy(a => a.LastName)
            .ThenOrBy(a => a.FirstName);
        
        //Map and return
        return query.Select(a => new AuthorResponse(a)).ToList();

    }

    /// <summary>One author looked up by primary key.</summary>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpGet(nameof(GetById))]
    public AuthorResponse GetById([FromQuery] string id)
    {
        throw new NotImplementedException();

    }

    /// <summary>How many authors the table holds.</summary>
    [HttpGet(nameof(Count))]
    public int Count()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Free-text search over <see cref="Author.FirstName" /> and <see cref="Author.LastName" />.
    ///     A partial, case-insensitive match is enough.
    /// </summary>
    /// <param name="q">The text to look for.</param>
    /// <exception cref="ValidationException"><paramref name="q" /> is null or blank.</exception>
    [HttpGet(nameof(Search))]
    public List<AuthorResponse> Search([FromQuery] string q)
    {
        throw new NotImplementedException();
    }

    /// <summary>One page of authors, ordered by last name then first name.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="size">Rows per page, at least 1.</param>
    /// <exception cref="ValidationException"><paramref name="page" /> or <paramref name="size" /> is below 1.</exception>
    [HttpGet(nameof(GetPage))]
    public List<AuthorResponse> GetPage([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Every author, sorted by a caller-chosen column. Sorting happens in SQL.
    /// </summary>
    [HttpGet(nameof(GetSorted))]
    public List<AuthorResponse> GetSorted([FromQuery] AuthorSort by, [FromQuery] bool descending = false)
    {
        IQueryable<Author> query = db.Authors();
        if (by == AuthorSort.BirthDate)
            query = query.OrderBy(a => a.BirthDate);
        
        throw new NotImplementedException();
    }

    /// <summary>
    ///     A filtered list. Every parameter that is supplied narrows the result; every parameter left
    ///     null is ignored.
    /// </summary>
    /// <param name="q">Matches first or last name, case-insensitive and partial.</param>
    /// <param name="nationality">Exact match.</param>
    /// <param name="bornAfter">Inclusive lower bound on <see cref="Author.BirthDate" />.</param>
    /// <param name="bornBefore">Inclusive upper bound on <see cref="Author.BirthDate" />.</param>
    /// <returns>Matching authors, ordered by last name. No parameters at all returns everyone.</returns>
    [HttpGet(nameof(GetFiltered))]
    public List<AuthorResponse> GetFiltered(
        [FromQuery] string? q = null,
        [FromQuery] string? nationality = null,
        [FromQuery] DateOnly? bornAfter = null,
        [FromQuery] DateOnly? bornBefore = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>Every author credited on one book, ordered by last name.</summary>
    /// <exception cref="KeyNotFoundException">No book has that id.</exception>
    [HttpGet(nameof(GetForBook))]
    public List<AuthorResponse> GetForBook([FromQuery] string bookId)
    {
        var book = db.Books().LoadWith(b => b.Authors)
            .FirstOrDefault(b => b.Id == bookId) ?? throw new KeyNotFoundException();

        return book.Authors.Select(a => new AuthorResponse(a)).ToList();

    }

    /// <summary>Authors with no book credited to them at all.</summary>
    /// <returns>1 author on the seed data.</returns>
    [HttpGet(nameof(GetWithoutBooks))]
    public List<AuthorResponse> GetWithoutBooks()
    {
        //step 1: Lookup
        IQueryable<Author> query = db.Authors().LoadWith(a => a.Books);
        
        //step 2: Filter
        query = query.Where(a => !a.Books.Any());

        //Step 3: Map / projection (send the right object back)
        return query.Select(a => new AuthorResponse(a)).ToList();

    }

    #region Tests: GetAll

    public class GetAllTests : LibraryTest
    {
        [Fact]
        public void Returns_every_seeded_author()
        {
            Assert.Equal(11, AuthorsController.GetAll().Count);
        }

        [Fact]
        public void Is_ordered_by_last_name_then_first_name()
        {
            var names = AuthorsController.GetAll().Select(a => (a.LastName, a.FirstName)).ToList();
            Assert.Equal(
                names.OrderBy(n => n.LastName, StringComparer.Ordinal).ThenBy(n => n.FirstName, StringComparer.Ordinal),
                names);
        }
    }

    #endregion

    #region Tests: GetById

    public class GetByIdTests : LibraryTest
    {
        [Fact]
        public void Returns_the_author_for_a_known_id()
        {
            var author = AuthorsController.GetById(LibrarySeed.AuthorIdOf(1));
            Assert.Equal("Elena", author.FirstName);
            Assert.Equal("Marsh", author.LastName);
        }

        [Fact]
        public void Throws_when_the_id_is_unknown()
        {
            Assert.Throws<KeyNotFoundException>(() => AuthorsController.GetById(Guid.NewGuid().ToString()));
        }
    }

    #endregion

    #region Tests: Count

    public class CountTests : LibraryTest
    {
        [Fact]
        public void Counts_the_seeded_rows()
        {
            Assert.Equal(11, AuthorsController.Count());
        }
    }

    #endregion

    #region Tests: Search

    public class SearchTests : LibraryTest
    {
        [Fact]
        public void Matches_first_or_last_name()
        {
            var result = AuthorsController.Search("mar");
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.LastName == "Marsh");
            Assert.Contains(result, a => a.FirstName == "Marcus");
        }

        [Fact]
        public void Throws_on_a_blank_term()
        {
            Assert.Throws<ValidationException>(() => AuthorsController.Search("   "));
        }
    }

    #endregion

    #region Tests: GetPage

    public class GetPageTests : LibraryTest
    {
        [Fact]
        public void Returns_the_requested_slice()
        {
            var page = AuthorsController.GetPage(2, 5);
            Assert.Equal(5, page.Count);
        }

        [Fact]
        public void Throws_on_nonsense_paging()
        {
            Assert.Throws<ValidationException>(() => AuthorsController.GetPage(0));
            Assert.Throws<ValidationException>(() => AuthorsController.GetPage(1, 0));
        }
    }

    #endregion

    #region Tests: GetSorted

    public class GetSortedTests : LibraryTest
    {
        [Fact]
        public void Sorts_by_creation_descending()
        {
            var result = AuthorsController.GetSorted(AuthorSort.Created, true);
            Assert.Equal("Kemp", result[0].LastName);
        }

    }

    #endregion

    #region Tests: GetFiltered

    public class GetFilteredTests : LibraryTest
    {
        [Fact]
        public void No_criteria_returns_everyone()
        {
            Assert.Equal(11, AuthorsController.GetFiltered().Count);
        }

        [Fact]
        public void Combines_every_supplied_criterion()
        {
            var result = AuthorsController.GetFiltered(nationality: "Swedish");
            Assert.Equal(2, result.Count);
            Assert.All(result, a => Assert.Equal("Swedish", a.Nationality));
        }

        [Fact]
        public void Filters_on_birth_date_range()
        {
            var result = AuthorsController.GetFiltered(bornAfter: new DateOnly(1980, 1, 1),
                bornBefore: new DateOnly(1990, 1, 1));
            Assert.All(result,
                a => Assert.InRange(a.BirthDate!.Value, new DateOnly(1980, 1, 1), new DateOnly(1990, 1, 1)));
        }

    }

    #endregion

    #region Tests: GetForBook

    public class GetForBookTests : LibraryTest
    {
        [Fact]
        public void Returns_every_credited_author()
        {
            var result = AuthorsController.GetForBook(LibrarySeed.BookIdOf(6));
            Assert.Equal(3, result.Count);
            Assert.Contains(result, a => a.LastName == "Alderholt");
            Assert.Contains(result, a => a.LastName == "Solberg");
            Assert.Contains(result, a => a.LastName == "Lindqvist");
        }

        [Fact]
        public void Throws_for_an_unknown_book()
        {
            Assert.Throws<KeyNotFoundException>(() => AuthorsController.GetForBook(Guid.NewGuid().ToString()));
        }
    }

    #endregion

    #region Tests: GetWithoutBooks

    public class GetWithoutBooksTests : LibraryTest
    {
        [Fact]
        public void Finds_the_author_with_no_books()
        {
            var result = AuthorsController.GetWithoutBooks();
            Assert.Single(result);
            Assert.Equal("Kemp", result[0].LastName);
        }
    }

    #endregion
}
