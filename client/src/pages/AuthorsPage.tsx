import { useEffect, useState } from "react";
import toast from "react-hot-toast";
import {
  Api,
  AuthorSort,
  type AuthorCreateRequest,
  type AuthorReplaceRequest,
  type AuthorsGetFilteredParams,
  type AuthorUpdateRequest,
} from "../api/Api.ts";
import { api } from "./AuthorBooksPage.tsx";



// Ids from the seed data, so every block below can be run on its own.
const seedAuthor = "1";
const seedBook = "6";

const sorts = Object.entries(AuthorSort).filter(([, v]) => typeof v === "number") as [string, AuthorSort][];
const textFields = ["firstName", "lastName", "bio", "nationality", "website", "birthDate"] as const;

export function AuthorsPage() {
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

  const [id, setId] = useState(seedAuthor);
  const [q, setQ] = useState("mar");
  const [paging, setPaging] = useState({ page: 1, size: 5 });
  const [sort, setSort] = useState({ by: AuthorSort.Name, descending: false });
  const [filter, setFilter] = useState<AuthorsGetFilteredParams>({ nationality: "Swedish" });
  const [bookId, setBookId] = useState(seedBook);
  const [create, setCreate] = useState<AuthorCreateRequest>({ firstName: "Test", lastName: "Author" });
  const [replace, setReplace] = useState<AuthorReplaceRequest>({
    id: seedAuthor,
    firstName: "Elena",
    lastName: "Marsh",
    bio: "Literary novelist.",
    nationality: "British",
    website: null,
    birthDate: "1978-04-12",
  });
  const [patch, setPatch] = useState<AuthorUpdateRequest>({ id: seedAuthor, firstName: "Elly" });

  useEffect(() => {
    run("getAll", () => api.authors.authorsGetAll());
  }, []);

  return (
    <div>
      <h2>Authors</h2>
      <p>Each block calls exactly one endpoint (named in its legend) and shows its own result or error.</p>

      <fieldset>
        <legend>GET /Authors/GetAll: api.authors.authorsGetAll()</legend>
        <button onClick={() => run("getAll", () => api.authors.authorsGetAll())}>Run</button>
        <pre>{show("getAll")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Authors/Count: api.authors.authorsCount()</legend>
        <button onClick={() => run("count", () => api.authors.authorsCount())}>Run</button>
        <pre>{show("count")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Authors/GetById: api.authors.authorsGetById(&#123; id &#125;)</legend>
        id <input size={36} value={id} onChange={(e) => setId(e.target.value)} />
        <button onClick={() => run("getById", () => api.authors.authorsGetById({ id }))}>Run</button>
        <pre>{show("getById")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Authors/Search: api.authors.authorsSearch(&#123; q &#125;)</legend>
        q <input value={q} onChange={(e) => setQ(e.target.value)} />
        <button onClick={() => run("search", () => api.authors.authorsSearch({ q }))}>Run</button>
        <pre>{show("search")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Authors/GetPage: api.authors.authorsGetPage(&#123; page, size &#125;)</legend>
        page <input type="number" value={paging.page} onChange={(e) => setPaging({ ...paging, page: Number(e.target.value) })} />
        size <input type="number" value={paging.size} onChange={(e) => setPaging({ ...paging, size: Number(e.target.value) })} />
        <button onClick={() => run("getPage", () => api.authors.authorsGetPage(paging))}>Run</button>
        <pre>{show("getPage")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Authors/GetSorted: api.authors.authorsGetSorted(&#123; by, descending &#125;)</legend>
        <select value={sort.by} onChange={(e) => setSort({ ...sort, by: Number(e.target.value) })}>
          {sorts.map(([name, value]) => (
            <option key={name} value={value}>{name}</option>
          ))}
        </select>
        <label>
          <input type="checkbox" checked={sort.descending} onChange={(e) => setSort({ ...sort, descending: e.target.checked })} />
          descending
        </label>
        <button onClick={() => run("getSorted", () => api.authors.authorsGetSorted(sort))}>Run</button>
        <pre>{show("getSorted")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Authors/GetFiltered: api.authors.authorsGetFiltered(filter)</legend>
        <input placeholder="q" value={filter.q ?? ""} onChange={(e) => setFilter({ ...filter, q: e.target.value })} />
        <input placeholder="nationality" value={filter.nationality ?? ""} onChange={(e) => setFilter({ ...filter, nationality: e.target.value })} />
        bornAfter <input type="date" value={filter.bornAfter ?? ""} onChange={(e) => setFilter({ ...filter, bornAfter: e.target.value })} />
        bornBefore <input type="date" value={filter.bornBefore ?? ""} onChange={(e) => setFilter({ ...filter, bornBefore: e.target.value })} />
        <button onClick={() => run("getFiltered", () => api.authors.authorsGetFiltered(filter))}>Run</button>
        <pre>{show("getFiltered")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Authors/GetForBook: api.authors.authorsGetForBook(&#123; bookId &#125;)</legend>
        bookId <input size={36} value={bookId} onChange={(e) => setBookId(e.target.value)} />
        <button onClick={() => run("getForBook", () => api.authors.authorsGetForBook({ bookId }))}>Run</button>
        <pre>{show("getForBook")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /Authors/GetWithoutBooks: api.authors.authorsGetWithoutBooks()</legend>
        <button onClick={() => run("getWithoutBooks", () => api.authors.authorsGetWithoutBooks())}>Run</button>
        <pre>{show("getWithoutBooks")}</pre>
      </fieldset>

      <fieldset>
        <legend>POST /Authors/Create: api.authors.authorsCreate(request)</legend>
        {textFields.map((k) => (
          <input key={k} placeholder={k} value={create[k] ?? ""} onChange={(e) => setCreate({ ...create, [k]: e.target.value || null })} />
        ))}
        <button onClick={() => run("create", () => api.authors.authorsCreate(create))}>Run</button>
        <pre>{show("create")}</pre>
      </fieldset>

      <fieldset>
        <legend>PUT /Authors/Replace: api.authors.authorsReplace(request). Empty fields become null.</legend>
        id <input size={36} value={replace.id} onChange={(e) => setReplace({ ...replace, id: e.target.value })} />
        {textFields.map((k) => (
          <input key={k} placeholder={k} value={replace[k] ?? ""} onChange={(e) => setReplace({ ...replace, [k]: e.target.value || null })} />
        ))}
        <button onClick={() => run("replace", () => api.authors.authorsReplace(replace))}>Run</button>
        <pre>{show("replace")}</pre>
      </fieldset>

      <fieldset>
        <legend>PATCH /Authors/Update: api.authors.authorsUpdate(request). Only what is set changes.</legend>
        id <input size={36} value={patch.id} onChange={(e) => setPatch({ ...patch, id: e.target.value })} />
        {textFields.map((k) => (
          <input key={k} placeholder={k} value={patch[k] ?? ""} onChange={(e) => setPatch({ ...patch, [k]: e.target.value || null })} />
        ))}
        <button onClick={() => run("update", () => api.authors.authorsUpdate(patch))}>Run</button>
        <pre>{show("update")}</pre>
      </fieldset>

      <fieldset>
        <legend>DELETE /Authors/Delete: api.authors.authorsDelete(&#123; id &#125;)</legend>
        id <input size={36} value={id} onChange={(e) => setId(e.target.value)} />
        <button onClick={() => run("delete", () => api.authors.authorsDelete({ id }))}>Run</button>
        <pre>{show("delete")}</pre>
      </fieldset>
    </div>
  );
}
