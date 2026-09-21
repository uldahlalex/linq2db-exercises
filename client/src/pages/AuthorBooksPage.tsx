import { useState } from "react";
import toast from "react-hot-toast";
import { Api } from "../api/Api.ts";
export const api = new Api({baseUrl: process.env.NODE_ENV === "production" ? "https://linq2db-exercises.fly.dev" : "http://localhost:5234"});

// Ids from the seed data: Rosalind Kemp has no books and "Unwritten Kingdoms" has no authors.
const seedAuthor = "11";
const seedBook = "12";

export function AuthorBooksPage() {
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

  const [link, setLink] = useState({ authorId: seedAuthor, bookId: seedBook });
  const ids = (
    <>
      authorId <input size={36} value={link.authorId} onChange={(e) => setLink({ ...link, authorId: e.target.value })} />
      bookId <input size={36} value={link.bookId} onChange={(e) => setLink({ ...link, bookId: e.target.value })} />
    </>
  );

  return (
    <div>
      <h2>Author / book links</h2>
      <p>Each block calls exactly one endpoint (named in its legend) and shows its own result or error.</p>

      <fieldset>
        <legend>POST /AuthorBooks/Link: api.authorBooks.authorBooksLink(&#123; authorId, bookId &#125;)</legend>
        {ids}
        <button onClick={() => run("link", () => api.authorBooks.authorBooksLink(link))}>Run</button>
        <pre>{show("link")}</pre>
      </fieldset>

      <fieldset>
        <legend>DELETE /AuthorBooks/Unlink: api.authorBooks.authorBooksUnlink(&#123; authorId, bookId &#125;)</legend>
        {ids}
        <button onClick={() => run("unlink", () => api.authorBooks.authorBooksUnlink(link))}>Run</button>
        <pre>{show("unlink")}</pre>
      </fieldset>

      <fieldset>
        <legend>GET /AuthorBooks/IsLinked: api.authorBooks.authorBooksIsLinked(&#123; authorId, bookId &#125;)</legend>
        {ids}
        <button onClick={() => run("isLinked", () => api.authorBooks.authorBooksIsLinked(link))}>Run</button>
        <pre>{show("isLinked")}</pre>
      </fieldset>
    </div>
  );
}
