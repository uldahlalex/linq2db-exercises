import { useEffect, useState } from "react";
import toast from "react-hot-toast";
import {
  Api,
  BookSort,
  Genre,
  type BookCreateRequest,
  type BooksGetFilteredParams,
  type BookReplaceRequest,
  type BookUpdateRequest,
} from "../api/Api.ts";
import { api } from "./AuthorBooksPage.tsx";


// Ids from the seed data, so every block below can be run on its own.
const seedBook = "1";
const seedAuthor = "4";

const genres = Object.entries(Genre).filter(([, v]) => typeof v === "number") as [string, Genre][];
const sorts = Object.entries(BookSort).filter(([, v]) => typeof v === "number") as [string, BookSort][];

export function BooksPage() {
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

  const [id, setId] = useState(seedBook);
  const [q, setQ] = useState("the");
  const [paging, setPaging] = useState({ page: 1, size: 5 });
  const [sort, setSort] = useState({ by: BookSort.Price, descending: true });
  const [filter, setFilter] = useState<BooksGetFilteredParams>({ genre: Genre.Fantasy });
  const [authorId, setAuthorId] = useState(seedAuthor);
  const [create, setCreate] = useState<BookCreateRequest>({ title: "Test Book", genre: Genre.Fiction, priceDkk: 99 });
  const [replace, setReplace] = useState<BookReplaceRequest>({
    id: seedBook,
    title: "The Glass Meridian",
    isbn: "9788700000011",
    genre: Genre.Fiction,
    priceDkk: 149,
    isOutOfPrint: false,
    publishedDate: "2015-04-10",
  });
  const [patch, setPatch] = useState<BookUpdateRequest>({ id: seedBook, priceDkk: 150 });

  useEffect(() => {
    run("getAll", () => api.books.booksGetAll());
  }, []);

  return (
    <div>
      <h2>Books</h2>
      <p>Each block calls exactly one endpoint (named in its legend) and shows its own result or error.</p>

      <fieldset>
        <legend>GET /Books/GetAll: api.books.booksGetAll()</legend>
        <button onClick={() => run("getAll", () => api.books.booksGetAll())}>Run</button>
        <pre>{show("getAll")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Books/Count: api.books.booksCount()</legend>
        <button onClick={() => run("count", () => api.books.booksCount())}>Run</button>
        <pre>{show("count")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Books/GetById: api.books.booksGetById(&#123; id &#125;)</legend>
        id <input size={36} value={id} onChange={(e) => setId(e.target.value)} />
        <button onClick={() => run("getById", () => api.books.booksGetById({ id }))}>Run</button>
        <pre>{show("getById")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Books/Search: api.books.booksSearch(&#123; q &#125;)</legend>
        q <input value={q} onChange={(e) => setQ(e.target.value)} />
        <button onClick={() => run("search", () => api.books.booksSearch({ q }))}>Run</button>
        <pre>{show("search")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Books/GetPage: api.books.booksGetPage(&#123; page, size &#125;)</legend>
        page <input type="number" value={paging.page} onChange={(e) => setPaging({ ...paging, page: Number(e.target.value) })} />
        size <input type="number" value={paging.size} onChange={(e) => setPaging({ ...paging, size: Number(e.target.value) })} />
        <button onClick={() => run("getPage", () => api.books.booksGetPage(paging))}>Run</button>
        <pre>{show("getPage")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Books/GetSorted: api.books.booksGetSorted(&#123; by, descending &#125;)</legend>
        <select value={sort.by} onChange={(e) => setSort({ ...sort, by: Number(e.target.value) })}>
          {sorts.map(([name, value]) => (
            <option key={name} value={value}>{name}</option>
          ))}
        </select>
        <label>
          <input type="checkbox" checked={sort.descending} onChange={(e) => setSort({ ...sort, descending: e.target.checked })} />
          descending
        </label>
        <button onClick={() => run("getSorted", () => api.books.booksGetSorted(sort))}>Run</button>
        <pre>{show("getSorted")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Books/GetFiltered: api.books.booksGetFiltered(filter)</legend>
        <input placeholder="q" value={filter.q ?? ""} onChange={(e) => setFilter({ ...filter, q: e.target.value })} />
        <select value={filter.genre ?? ""} onChange={(e) => setFilter({ ...filter, genre: e.target.value === "" ? null : Number(e.target.value) })}>
          <option value="">any genre</option>
          {genres.map(([name, value]) => (
            <option key={name} value={value}>{name}</option>
          ))}
        </select>
        <select
          value={filter.outOfPrint == null ? "" : String(filter.outOfPrint)}
          onChange={(e) => setFilter({ ...filter, outOfPrint: e.target.value === "" ? null : e.target.value === "true" })}
        >
          <option value="">in or out of print</option>
          <option value="false">in print</option>
          <option value="true">out of print</option>
        </select>
        minPrice <input type="number" value={filter.minPrice ?? ""} onChange={(e) => setFilter({ ...filter, minPrice: e.target.value === "" ? null : Number(e.target.value) })} />
        maxPrice <input type="number" value={filter.maxPrice ?? ""} onChange={(e) => setFilter({ ...filter, maxPrice: e.target.value === "" ? null : Number(e.target.value) })} />
        <button onClick={() => run("getFiltered", () => api.books.booksGetFiltered(filter))}>Run</button>
        <pre>{show("getFiltered")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Books/GetByAuthor: api.books.booksGetByAuthor(&#123; authorId &#125;)</legend>
        authorId <input size={36} value={authorId} onChange={(e) => setAuthorId(e.target.value)} />
        <button onClick={() => run("getByAuthor", () => api.books.booksGetByAuthor({ authorId }))}>Run</button>
        <pre>{show("getByAuthor")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Books/GetWithoutAuthors: api.books.booksGetWithoutAuthors()</legend>
        <button onClick={() => run("getWithoutAuthors", () => api.books.booksGetWithoutAuthors())}>Run</button>
        <pre>{show("getWithoutAuthors")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Books/GetAveragePrice: api.books.booksGetAveragePrice()</legend>
        <button onClick={() => run("getAveragePrice", () => api.books.booksGetAveragePrice())}>Run</button>
        <pre>{show("getAveragePrice")}</pre>
      </fieldset>

      <fieldset>
        <legend>POST /Books/Create: api.books.booksCreate(request)</legend>
        <input placeholder="title" value={create.title} onChange={(e) => setCreate({ ...create, title: e.target.value })} />
        <input placeholder="isbn" value={create.isbn ?? ""} onChange={(e) => setCreate({ ...create, isbn: e.target.value || null })} />
        <select value={create.genre} onChange={(e) => setCreate({ ...create, genre: Number(e.target.value) })}>
          {genres.map(([name, value]) => (
            <option key={name} value={value}>{name}</option>
          ))}
        </select>
        priceDkk <input type="number" value={create.priceDkk} onChange={(e) => setCreate({ ...create, priceDkk: Number(e.target.value) })} />
        publishedDate <input type="date" value={create.publishedDate ?? ""} onChange={(e) => setCreate({ ...create, publishedDate: e.target.value || null })} />
        <button onClick={() => run("create", () => api.books.booksCreate(create))}>Run</button>
        <pre>{show("create")}</pre>
      </fieldset>

      <fieldset>
        <legend>PUT /Books/Replace: api.books.booksReplace(request). Empty fields become null.</legend>
        id <input size={36} value={replace.id} onChange={(e) => setReplace({ ...replace, id: e.target.value })} />
        <input placeholder="title" value={replace.title} onChange={(e) => setReplace({ ...replace, title: e.target.value })} />
        <input placeholder="isbn" value={replace.isbn ?? ""} onChange={(e) => setReplace({ ...replace, isbn: e.target.value || null })} />
        <select value={replace.genre} onChange={(e) => setReplace({ ...replace, genre: Number(e.target.value) })}>
          {genres.map(([name, value]) => (
            <option key={name} value={value}>{name}</option>
          ))}
        </select>
        priceDkk <input type="number" value={replace.priceDkk} onChange={(e) => setReplace({ ...replace, priceDkk: Number(e.target.value) })} />
        <label>
          <input type="checkbox" checked={replace.isOutOfPrint} onChange={(e) => setReplace({ ...replace, isOutOfPrint: e.target.checked })} />
          isOutOfPrint
        </label>
        publishedDate <input type="date" value={replace.publishedDate ?? ""} onChange={(e) => setReplace({ ...replace, publishedDate: e.target.value || null })} />
        <button onClick={() => run("replace", () => api.books.booksReplace(replace))}>Run</button>
        <pre>{show("replace")}</pre>
      </fieldset>

      <fieldset>
        <legend>PATCH /Books/Update: api.books.booksUpdate(request). Only what is set changes.</legend>
        id <input size={36} value={patch.id} onChange={(e) => setPatch({ ...patch, id: e.target.value })} />
        <input placeholder="title" value={patch.title ?? ""} onChange={(e) => setPatch({ ...patch, title: e.target.value || null })} />
        priceDkk <input type="number" value={patch.priceDkk ?? ""} onChange={(e) => setPatch({ ...patch, priceDkk: e.target.value === "" ? null : Number(e.target.value) })} />
        <button onClick={() => run("update", () => api.books.booksUpdate(patch))}>Run</button>
        <pre>{show("update")}</pre>
      </fieldset>

      <fieldset>
        <legend>POST /Books/MarkOutOfPrint: api.books.booksMarkOutOfPrint(&#123; id &#125;)</legend>
        id <input size={36} value={id} onChange={(e) => setId(e.target.value)} />
        <button onClick={() => run("markOutOfPrint", () => api.books.booksMarkOutOfPrint({ id }))}>Run</button>
        <pre>{show("markOutOfPrint")}</pre>
      </fieldset>

      <fieldset>
        <legend>POST /Books/MarkInPrint: api.books.booksMarkInPrint(&#123; id &#125;)</legend>
        id <input size={36} value={id} onChange={(e) => setId(e.target.value)} />
        <button onClick={() => run("markInPrint", () => api.books.booksMarkInPrint({ id }))}>Run</button>
        <pre>{show("markInPrint")}</pre>
      </fieldset>

      <fieldset>
        <legend>DELETE /Books/Delete: api.books.booksDelete(&#123; id &#125;)</legend>
        id <input size={36} value={id} onChange={(e) => setId(e.target.value)} />
        <button onClick={() => run("delete", () => api.books.booksDelete({ id }))}>Run</button>
        <pre>{show("delete")}</pre>
      </fieldset>
    </div>
  );
}
