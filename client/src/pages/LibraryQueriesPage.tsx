import { useEffect, useState } from "react";
import toast from "react-hot-toast";
import { Api } from "../api/Api.ts";
import { api } from "./AuthorBooksPage.tsx";


// Ids from the seed data, so every block below can be run on its own.
const seedBook = "6";
const seedAuthor = "4";

export function LibraryQueriesPage() {
  // One result per block, keyed by block name: a failing endpoint only affects its own block.
  const [out, setOut] = useState<Record<string, unknown>>({});
  const run = async (key: string, call: () => Promise<unknown>) => {
    try {
      setOut((o) => ({ ...o, [key]: "..." }));
      const data = await call();
      setOut((o) => ({ ...o, [key]: data ?? "ok" }));
    } catch (e: any) {
      // endpoints without a response body leave the error body unread on the thrown Response
      const body = e?.error ?? (await e?.json?.().catch(() => null));
      const error = body?.title ?? String(e);
      toast.error(error);
      setOut((o) => ({ ...o, [key]: { error } }));
    }
  };
  const show = (key: string) => (key in out ? JSON.stringify(out[key], null, 2) : "");

  const [booksPaging, setBooksPaging] = useState({ page: 1, size: 3 });
  const [bookId, setBookId] = useState(seedBook);
  const [q, setQ] = useState("alderholt");
  const [authorsPaging, setAuthorsPaging] = useState({ page: 1, size: 3 });
  const [authorId, setAuthorId] = useState(seedAuthor);

  useEffect(() => {
    run("booksWithAuthors", () => api.libraryQueries.libraryQueriesGetBooksWithAuthors(booksPaging));
  }, []);

  return (
    <div>
      <h2>Nested reads</h2>
      <p>Each block calls exactly one endpoint (named in its legend) and shows its own result or error.</p>

      <fieldset>
        <legend>GET /LibraryQueries/GetBooksWithAuthors: api.libraryQueries.libraryQueriesGetBooksWithAuthors(&#123; page, size &#125;)</legend>
        page <input type="number" value={booksPaging.page} onChange={(e) => setBooksPaging({ ...booksPaging, page: Number(e.target.value) })} />
        size <input type="number" value={booksPaging.size} onChange={(e) => setBooksPaging({ ...booksPaging, size: Number(e.target.value) })} />
        <button onClick={() => run("booksWithAuthors", () => api.libraryQueries.libraryQueriesGetBooksWithAuthors(booksPaging))}>Run</button>
        <pre>{show("booksWithAuthors")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /LibraryQueries/GetBookWithAuthors: api.libraryQueries.libraryQueriesGetBookWithAuthors(&#123; id &#125;)</legend>
        id <input size={36} value={bookId} onChange={(e) => setBookId(e.target.value)} />
        <button onClick={() => run("bookWithAuthors", () => api.libraryQueries.libraryQueriesGetBookWithAuthors({ id: bookId }))}>Run</button>
        <pre>{show("bookWithAuthors")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /LibraryQueries/SearchBooksByAuthor: api.libraryQueries.libraryQueriesSearchBooksByAuthor(&#123; q &#125;)</legend>
        q <input value={q} onChange={(e) => setQ(e.target.value)} />
        <button onClick={() => run("searchBooksByAuthor", () => api.libraryQueries.libraryQueriesSearchBooksByAuthor({ q }))}>Run</button>
        <pre>{show("searchBooksByAuthor")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /LibraryQueries/GetAuthorsWithBooks: api.libraryQueries.libraryQueriesGetAuthorsWithBooks(&#123; page, size &#125;)</legend>
        page <input type="number" value={authorsPaging.page} onChange={(e) => setAuthorsPaging({ ...authorsPaging, page: Number(e.target.value) })} />
        size <input type="number" value={authorsPaging.size} onChange={(e) => setAuthorsPaging({ ...authorsPaging, size: Number(e.target.value) })} />
        <button onClick={() => run("authorsWithBooks", () => api.libraryQueries.libraryQueriesGetAuthorsWithBooks(authorsPaging))}>Run</button>
        <pre>{show("authorsWithBooks")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /LibraryQueries/GetAuthorWithBooks: api.libraryQueries.libraryQueriesGetAuthorWithBooks(&#123; id &#125;)</legend>
        id <input size={36} value={authorId} onChange={(e) => setAuthorId(e.target.value)} />
        <button onClick={() => run("authorWithBooks", () => api.libraryQueries.libraryQueriesGetAuthorWithBooks({ id: authorId }))}>Run</button>
        <pre>{show("authorWithBooks")}</pre>
      </fieldset>
    </div>
  );
}
