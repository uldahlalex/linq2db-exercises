using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

/// <summary>
///     Commands and queries on the <see cref="AuthorBook" /> junction itself, rather than on
///     <see cref="Author" /> or <see cref="Book" />. Listing one side of the relationship lives on the
///     controller that owns the returned type — <c>AuthorsController.GetForBook</c> and
///     <c>BooksController.GetByAuthor</c> — this controller only manages the link row.
/// </summary>
[Route("[controller]")]
public class AuthorBooksController(LibraryDatabase db) : ControllerBase
{
    /// <summary>Credits an author on a book. Idempotent: linking an already-linked pair is a no-op.</summary>
    /// <exception cref="KeyNotFoundException">Either id is unknown.</exception>
    [HttpPost(nameof(Link))]
    public void Link([FromQuery] string authorId, [FromQuery] string bookId)
    {
        throw new NotImplementedException();
    }

    /// <summary>Removes an author's credit on a book. Idempotent: unlinking an already-unlinked pair is a no-op.</summary>
    [HttpDelete(nameof(Unlink))]
    public void Unlink([FromQuery] string authorId, [FromQuery] string bookId)
    {
        throw new NotImplementedException();
    }

    /// <summary>Whether an author is currently credited on a book.</summary>
    [HttpGet(nameof(IsLinked))]
    public bool IsLinked([FromQuery] string authorId, [FromQuery] string bookId)
    {
        throw new NotImplementedException();
    }

    #region Tests: Link

    public class LinkTests : LibraryTest
    {
        [Fact]
        public void Creates_the_link()
        {
            var author = LibrarySeed.AuthorIdOf(11);
            var book = LibrarySeed.BookIdOf(12);
            AuthorBooksController.Link(author, book);
            Assert.True(IsLinked(author, book));
        }

        [Fact]
        public void Linking_twice_is_fine()
        {
            var author = LibrarySeed.AuthorIdOf(11);
            var book = LibrarySeed.BookIdOf(12);
            AuthorBooksController.Link(author, book);
            AuthorBooksController.Link(author, book);
            Assert.Equal(1, LinkRows.Count(l => l.AuthorId == author && l.BookId == book));
        }

        [Fact]
        public void Throws_for_an_unknown_author_or_book()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                AuthorBooksController.Link(Guid.NewGuid().ToString(), LibrarySeed.BookIdOf(1)));
            Assert.Throws<KeyNotFoundException>(() =>
                AuthorBooksController.Link(LibrarySeed.AuthorIdOf(1), Guid.NewGuid().ToString()));
        }
    }

    #endregion

    #region Tests: Unlink

    public class UnlinkTests : LibraryTest
    {
        [Fact]
        public void Removes_the_link()
        {
            var author = LibrarySeed.AuthorIdOf(1);
            var book = LibrarySeed.BookIdOf(1);
            AuthorBooksController.Unlink(author, book);
            Assert.False(IsLinked(author, book));
        }

        [Fact]
        public void Unlinking_twice_is_fine()
        {
            var author = LibrarySeed.AuthorIdOf(1);
            var book = LibrarySeed.BookIdOf(1);
            AuthorBooksController.Unlink(author, book);
            AuthorBooksController.Unlink(author, book);
            Assert.False(IsLinked(author, book));
        }

    }

    #endregion

    #region Tests: IsLinked

    public class IsLinkedTests : LibraryTest
    {
        [Fact]
        public void Reports_presence_and_absence()
        {
            Assert.True(AuthorBooksController.IsLinked(LibrarySeed.AuthorIdOf(1), LibrarySeed.BookIdOf(1)));
            Assert.False(AuthorBooksController.IsLinked(LibrarySeed.AuthorIdOf(11), LibrarySeed.BookIdOf(12)));
        }
    }

    #endregion
}
