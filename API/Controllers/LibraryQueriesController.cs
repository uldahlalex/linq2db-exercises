using System.ComponentModel.DataAnnotations;
using API.Dtos;
using Infa;
using LinqToDB;
using LinqToDB.Internal.Linq;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

/// <summary>
///     Reads that cross the many-to-many and return nested objects: a book with its authors, an author
///     with their books. <see cref="Book.Authors" /> and <see cref="Author.Books" /> are association
///     properties, not columns — they stay empty until a query asks for them with <c>LoadWith</c>.
/// </summary>
[Route("[controller]")]
public class LibraryQueriesController(LibraryDatabase db) : ControllerBase
{
    /// <summary>
    ///     One page of books, ordered by title, each with the authors credited on it.
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="size">Rows per page, at least 1.</param>
    /// <exception cref="ValidationException"><paramref name="page" /> or <paramref name="size" /> is below 1.</exception>
    [HttpGet(nameof(GetBooksWithAuthors))]
    public List<BookWithAuthorsResponse> GetBooksWithAuthors([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        throw new NotImplementedException();

    }

    /// <summary>One book with its authors.</summary>
    /// <exception cref="KeyNotFoundException">No book has that id.</exception>
    [HttpGet(nameof(GetBookWithAuthors))]
    public BookWithAuthorsResponse GetBookWithAuthors([FromQuery] string id)
    {
        throw new NotImplementedException();

    }

    /// <summary>
    ///     Books whose authors match a name — first or last, partial, case-insensitive — each still
    ///     carrying <em>all</em> of its authors, not only the ones that matched. Ordered by title.
    /// </summary>
    /// <param name="q">The text to look for.</param>
    /// <exception cref="ValidationException"><paramref name="q" /> is null or blank.</exception>
    [HttpGet(nameof(SearchBooksByAuthor))]
    public List<BookWithAuthorsResponse> SearchBooksByAuthor([FromQuery] string q)
    {
        //1 validate:
        if (string.IsNullOrWhiteSpace(q))
            throw new ValidationException();

        //2 lookup
        IQueryable<Book> query = db.Books().LoadWith(b => b.Authors);
        
        //3 filter
        query = query.Where(b => b.Authors.Any(a =>
            a.FirstName.ToLower().Contains(q.ToLower()) || a.LastName.ToLower().Contains(q.ToLower())));
      
        //4 order
        query = query.OrderBy(b => b.Title);
        //5 map and return
        return query.Select(b => new BookWithAuthorsResponse(b)).ToList();
    }

    /// <summary>
    ///     One page of authors, ordered by last name then first name, each with the books credited to
    ///     them.
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="size">Rows per page, at least 1.</param>
    /// <exception cref="ValidationException"><paramref name="page" /> or <paramref name="size" /> is below 1.</exception>
    [HttpGet(nameof(GetAuthorsWithBooks))]
    public List<AuthorWithBooksResponse> GetAuthorsWithBooks([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        throw new NotImplementedException();
    }

    /// <summary>One author with their books.</summary>
    /// <exception cref="KeyNotFoundException">No author has that id.</exception>
    [HttpGet(nameof(GetAuthorWithBooks))]
    public AuthorWithBooksResponse GetAuthorWithBooks([FromQuery] string id)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetBooksWithAuthors

    public class GetBooksWithAuthorsTests : LibraryTest
    {
        [Fact]
        public void Returns_every_book_with_all_of_its_authors()
        {
            var result = LibraryQueriesController.GetBooksWithAuthors(1, 100);
            Assert.Equal(13, result.Count);

            var paperMoons = result.Single(b => b.Title == "Paper Moons");
            Assert.Equal(["Alderholt", "Lindqvist", "Solberg"], paperMoons.Authors.Select(a => a.LastName).Order());

            var echoes = result.Single(b => b.Title == "Echoes of Tomorrow");
            Assert.Equal(["Marsh", "Reyne"], echoes.Authors.Select(a => a.LastName).Order());
        }

        [Fact]
        public void Pages_the_books_by_title()
        {
            var page = LibraryQueriesController.GetBooksWithAuthors(2, 5);
            Assert.Equal(5, page.Count);
            Assert.Equal(BookRows.OrderBy(b => b.Title).Skip(5).Take(5).Select(b => b.Title), page.Select(b => b.Title));
        }

        [Fact]
        public void Throws_on_nonsense_paging()
        {
            Assert.Throws<ValidationException>(() => LibraryQueriesController.GetBooksWithAuthors(0, 10));
            Assert.Throws<ValidationException>(() => LibraryQueriesController.GetBooksWithAuthors(1, 0));
        }
    }

    #endregion

    #region Tests: GetBookWithAuthors

    public class GetBookWithAuthorsTests : LibraryTest
    {
        [Fact]
        public void Returns_the_book_with_its_authors()
        {
            var book = LibraryQueriesController.GetBookWithAuthors(LibrarySeed.BookIdOf(11));
            Assert.Equal("Wolves at the Border", book.Title);
            Assert.Equal(["Voss", "Whitfield"], book.Authors.Select(a => a.LastName).Order());
        }

        [Fact]
        public void Throws_for_an_unknown_id()
        {
            Assert.Throws<KeyNotFoundException>(() => LibraryQueriesController.GetBookWithAuthors(Guid.NewGuid().ToString()));
        }
    }

    #endregion

    #region Tests: SearchBooksByAuthor

    public class SearchBooksByAuthorTests : LibraryTest
    {
        [Fact]
        public void Finds_the_books_of_a_matching_author()
        {
            var result = LibraryQueriesController.SearchBooksByAuthor("alderholt");
            Assert.Equal(["Northern Wake", "Paper Moons", "The Long Wager"], result.Select(b => b.Title));
        }

        [Fact]
        public void Matching_books_keep_all_of_their_authors()
        {
            var paperMoons = LibraryQueriesController.SearchBooksByAuthor("Solberg").Single();
            Assert.Equal("Paper Moons", paperMoons.Title);
            Assert.Equal(3, paperMoons.Authors.Count);
        }

        [Fact]
        public void Throws_on_a_blank_term()
        {
            Assert.Throws<ValidationException>(() => LibraryQueriesController.SearchBooksByAuthor("   "));
        }
    }

    #endregion

    #region Tests: GetAuthorsWithBooks

    public class GetAuthorsWithBooksTests : LibraryTest
    {
        [Fact]
        public void Returns_every_author_with_all_of_their_books()
        {
            var result = LibraryQueriesController.GetAuthorsWithBooks(1, 100);
            Assert.Equal(11, result.Count);

            var magnus = result.Single(a => a.LastName == "Alderholt");
            Assert.Equal(["Northern Wake", "Paper Moons", "The Long Wager"], magnus.Books.Select(b => b.Title).Order());
        }

        [Fact]
        public void Throws_on_nonsense_paging()
        {
            Assert.Throws<ValidationException>(() => LibraryQueriesController.GetAuthorsWithBooks(0, 10));
        }
    }

    #endregion

    #region Tests: GetAuthorWithBooks

    public class GetAuthorWithBooksTests : LibraryTest
    {
        [Fact]
        public void Returns_the_author_with_their_books()
        {
            var author = LibraryQueriesController.GetAuthorWithBooks(LibrarySeed.AuthorIdOf(2));
            Assert.Equal("Reyne", author.LastName);
            Assert.Equal(["Cinder and Salt", "Echoes of Tomorrow"], author.Books.Select(b => b.Title).Order());
        }

        [Fact]
        public void Throws_for_an_unknown_id()
        {
            Assert.Throws<KeyNotFoundException>(() => LibraryQueriesController.GetAuthorWithBooks(Guid.NewGuid().ToString()));
        }
    }

    #endregion
}
