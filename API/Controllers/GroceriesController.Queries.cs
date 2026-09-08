using API.Testing;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

public partial class GroceriesController(GroceryDatabase db) : ControllerBase
{
    /// <summary>
    /// Every item in the table, ordered by <see cref="GroceryItem.Name"/> ascending.
    /// </summary>
    /// <returns>All 32 seeded rows.</returns>
    [HttpGet(nameof(GetAllMyGroceries))]
    public List<GroceryItem> GetAllMyGroceries()
    {
        return db.Groceries().ToList();
    }

    #region Tests: GetAll

    public class GetAllTests : GroceryTest
    {
        [Fact]
        public void Returns_every_seeded_item()
        {
            Assert.Equal(32, Controller.GetAllMyGroceries().Count);
        }

        [Fact]
        public void Is_ordered_by_name()
        {
            var names = Controller.GetAllMyGroceries().Select(x => x.Name).ToList();
            Assert.Equal(names.OrderBy(x => x, StringComparer.Ordinal), names);
        }
    }

    #endregion

    /// <summary>
    /// How many rows the table holds. A scalar straight out of SQL — the database counts, not you,
    /// so nothing but the number crosses the wire.
    /// </summary>
    [HttpGet(nameof(Count))]
    public int Count()
    {
        throw new NotImplementedException();
    }

    #region Tests: Count

    public class CountTests : GroceryTest
    {
        [Fact]
        public void Counts_the_seeded_rows()
        {
            Assert.Equal(32, Controller.Count());
        }

        [Fact]
        public void Follows_inserts()
        {
            InsertRow(new GroceryItem { Name = "Counted", Category = "Test", PriceDkk = 1m, WeightKg = 0.1 });
            Assert.Equal(33, Controller.Count());
        }
    }

    #endregion

    /// <summary>Items flagged organic. Expect 8.</summary>
    [HttpGet(nameof(GetOrganic))]
    public List<GroceryItem> GetOrganic()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetOrganic

    public class GetOrganicTests : GroceryTest
    {
        [Fact]
        public void Returns_only_organic_items()
        {
            var result = Controller.GetOrganic();
            Assert.Equal(8, result.Count);
            Assert.All(result, x => Assert.True(x.IsOrganic));
        }
    }

    #endregion

    /// <summary>
    /// Items kept at one storage temperature. The enum lives in the database as text
    /// ("ambient", "chilled", "frozen") but you compare against <see cref="StorageType"/> in LINQ
    /// and let the mapping do the translation.
    /// </summary>
    /// <returns>16 ambient, 12 chilled or 4 frozen items.</returns>
    [HttpGet(nameof(GetByStorage))]
    public List<GroceryItem> GetByStorage([FromQuery] StorageType storage)
    {
        var q = db.Groceries().AsQueryable();

        q = q.Where(g => g.Storage == storage);

        return q.ToList();
    }

    #region Tests: GetByStorage

    public class GetByStorageTests : GroceryTest
    {
        [Fact]
        public void Counts_match_the_seed_data()
        {
            Assert.Equal(16, Controller.GetByStorage(StorageType.Ambient).Count);
            Assert.Equal(12, Controller.GetByStorage(StorageType.Chilled).Count);
            Assert.Equal(4, Controller.GetByStorage(StorageType.Frozen).Count);
        }

        [Fact]
        public void Returns_only_that_storage_type()
        {
            Assert.All(Controller.GetByStorage(StorageType.Frozen), x => Assert.Equal(StorageType.Frozen, x.Storage));
        }
    }

    #endregion

    /// <summary>
    /// Items that need reordering: no stock left, and still part of the range.
    /// A discontinued item with no stock is not a problem, so it does not belong here.
    /// </summary>
    /// <returns>1 item (4 rows have zero stock, 3 of them are discontinued).</returns>
    [HttpGet(nameof(GetOutOfStock))]
    public List<GroceryItem> GetOutOfStock()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetOutOfStock

    public class GetOutOfStockTests : GroceryTest
    {
        [Fact]
        public void Returns_only_stocked_out_items_still_in_the_range()
        {
            var result = Controller.GetOutOfStock();
            Assert.Single(result);
            Assert.Equal("Havarti Cheese 400g", result[0].Name);
        }
    }

    #endregion

    /// <summary>
    /// Rows with missing master data: no <see cref="GroceryItem.Brand"/> or no
    /// <see cref="GroceryItem.Barcode"/>. Both columns are nullable, so this is a lesson in what
    /// <c>is null</c> looks like in SQL.
    /// </summary>
    /// <returns>14 items.</returns>
    [HttpGet(nameof(GetIncomplete))]
    public List<GroceryItem> GetIncomplete()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetIncomplete

    public class GetIncompleteTests : GroceryTest
    {
        [Fact]
        public void Returns_rows_missing_a_brand_or_a_barcode()
        {
            var result = Controller.GetIncomplete();
            Assert.Equal(14, result.Count);
            Assert.All(result, x => Assert.True(x.Brand is null || x.Barcode is null));
        }
    }

    #endregion

    /// <summary>
    /// Everything currently on discount, biggest discount first.
    /// </summary>
    /// <returns>12 items.</returns>
    [HttpGet(nameof(GetDiscounted))]
    public List<GroceryItem> GetDiscounted()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetDiscounted

    public class GetDiscountedTests : GroceryTest
    {
        [Fact]
        public void Returns_every_discounted_row_biggest_first()
        {
            var result = Controller.GetDiscounted();
            Assert.Equal(12, result.Count);
            Assert.All(result, x => Assert.NotNull(x.DiscountPercent));
            Assert.Equal(result.OrderByDescending(x => x.DiscountPercent).Select(x => x.Id), result.Select(x => x.Id));
        }

        [Fact]
        public void Skips_rows_without_a_discount()
        {
            Assert.DoesNotContain(Controller.GetDiscounted(), x => x.Name == "Whole Milk 1L");
        }
    }

    #endregion

    /// <summary>
    /// Items in one category, cheapest first. Discontinued items are never listed.
    /// </summary>
    [HttpGet(nameof(GetByCategory))]
    public List<GroceryItem> GetByCategory([FromQuery] string category)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetByCategory

    public class GetByCategoryTests : GroceryTest
    {
        [Fact]
        public void Returns_only_items_in_that_category()
        {
            var meat = Controller.GetByCategory("Meat");
            Assert.Equal(4, meat.Count);
            Assert.All(meat, x => Assert.Equal("Meat", x.Category));
        }

        [Fact]
        public void Excludes_discontinued_items()
        {
            var pantry = Controller.GetByCategory("Pantry");
            Assert.Equal(4, pantry.Count);
            Assert.DoesNotContain(pantry, x => x.Name == "Basmati Rice 1kg");
        }

        [Fact]
        public void Is_ordered_by_price_ascending()
        {
            var prices = Controller.GetByCategory("Dairy").Select(x => x.PriceDkk).ToList();
            Assert.Equal(prices.OrderBy(x => x), prices);
        }

        [Fact]
        public void Returns_an_empty_list_for_an_unknown_category()
        {
            Assert.Empty(Controller.GetByCategory("Nonsense"));
        }
    }

    #endregion

    /// <summary>
    /// One item looked up by primary key.
    /// </summary>
    /// <param name="id">Primary key. <c>GrocerySeed.IdOf(1)</c> is "Whole Milk 1L".</param>
    /// <returns>The item. Never null.</returns>
    /// <exception cref="NotFoundException">No row has that id.</exception>
    [HttpGet(nameof(GetById))]
    public GroceryItem GetById([FromQuery] Guid id)
    {
        var q = db.Groceries().AsQueryable();

        var result = q
            .FirstOrDefault(g => g.Id == id) ?? throw new NotFoundException("Not found");

        return result;
    }

    #region Tests: GetById

    public class GetByIdTests : GroceryTest
    {
        [Fact]
        public void Returns_the_item_for_a_known_id()
        {
            var item = Controller.GetById(GrocerySeed.IdOf(1));
            Assert.Equal("Whole Milk 1L", item.Name);
            Assert.Equal(StorageType.Chilled, item.Storage);
        }

        [Fact]
        public void Throws_when_the_id_is_unknown()
        {
            Assert.Throws<NotFoundException>(() => Controller.GetById(Guid.NewGuid()));
        }
    }

    #endregion

    /// <summary>
    /// Whether any row carries this barcode. Must not fetch the row — check the SQL for
    /// <c>exists</c> and make sure no columns are selected.
    /// </summary>
    [HttpGet(nameof(Exists))]
    public bool Exists([FromQuery] string barcode)
    {
        throw new NotImplementedException();
    }

    #region Tests: Exists

    public class ExistsTests : GroceryTest
    {
        [Fact]
        public void Reports_presence_and_absence()
        {
            Assert.True(Controller.Exists("5701234000011"));
            Assert.False(Controller.Exists("9999999999999"));
        }
    }

    #endregion

    /// <summary>
    /// The single item with this barcode.
    /// </summary>
    /// <param name="barcode">Exactly 13 digits.</param>
    /// <exception cref="ValidationException">The barcode is not 13 digits.</exception>
    /// <exception cref="NotFoundException">No row carries that barcode.</exception>
    [HttpGet(nameof(GetByBarcode))]
    public GroceryItem GetByBarcode([FromQuery] string barcode)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetByBarcode

    public class GetByBarcodeTests : GroceryTest
    {
        [Fact]
        public void Finds_the_item()
        {
            Assert.Equal("Whole Milk 1L", Controller.GetByBarcode("5701234000011").Name);
        }

        [Fact]
        public void Throws_when_the_barcode_is_malformed()
        {
            Assert.Throws<ValidationException>(() => Controller.GetByBarcode("57012"));
            Assert.Throws<ValidationException>(() => Controller.GetByBarcode("570123400001X"));
        }

        [Fact]
        public void Throws_when_nothing_carries_the_barcode()
        {
            Assert.Throws<NotFoundException>(() => Controller.GetByBarcode("9999999999999"));
        }
    }

    #endregion

    /// <summary>
    /// Items priced inside a range, cheapest first. A missing bound means "no bound", which is what
    /// the nullable parameters are for.
    /// </summary>
    /// <returns>15 items between 20 and 50; 4 items at 60 or above.</returns>
    /// <exception cref="ValidationException">A bound is negative, or min is greater than max.</exception>
    [HttpGet(nameof(GetByPriceRange))]
    public List<GroceryItem> GetByPriceRange([FromQuery] decimal? min, [FromQuery] decimal? max)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetByPriceRange

    public class GetByPriceRangeTests : GroceryTest
    {
        [Fact]
        public void Applies_both_bounds()
        {
            Assert.Equal(15, Controller.GetByPriceRange(20m, 50m).Count);
        }

        [Fact]
        public void An_open_upper_bound_means_no_ceiling()
        {
            Assert.Equal(4, Controller.GetByPriceRange(60m, null).Count);
        }

        [Fact]
        public void No_bounds_at_all_returns_everything()
        {
            Assert.Equal(32, Controller.GetByPriceRange(null, null).Count);
        }

        [Fact]
        public void Throws_on_an_inverted_or_negative_range()
        {
            Assert.Throws<ValidationException>(() => Controller.GetByPriceRange(50m, 10m));
            Assert.Throws<ValidationException>(() => Controller.GetByPriceRange(-1m, null));
        }
    }

    #endregion

    /// <summary>
    /// Free-text search over <see cref="GroceryItem.Name"/> and <see cref="GroceryItem.Brand"/>.
    /// A partial, case-insensitive match is enough; remember that <c>Brand</c> is nullable.
    /// </summary>
    /// <param name="q">At least two characters, otherwise the search is meaningless.</param>
    /// <exception cref="ValidationException"><paramref name="q"/> is null, blank or shorter than two characters.</exception>
    [HttpGet(nameof(Search))]
    public List<GroceryItem> Search([FromQuery] string q)
    {
        throw new NotImplementedException();
    }

    #region Tests: Search

    public class SearchTests : GroceryTest
    {
        [Fact]
        public void Matches_the_name()
        {
            Assert.Equal(2, Controller.Search("milk").Count);
        }

        [Fact]
        public void Is_case_insensitive()
        {
            Assert.Equal(Controller.Search("milk").Count, Controller.Search("MILK").Count);
        }

        [Fact]
        public void Matches_the_nullable_brand_too()
        {
            var result = Controller.Search("Bakerman");
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal("Bakerman", x.Brand));
        }

        [Fact]
        public void Throws_on_a_too_short_term()
        {
            Assert.Throws<ValidationException>(() => Controller.Search("a"));
            Assert.Throws<ValidationException>(() => Controller.Search("   "));
        }
    }

    #endregion

    /// <summary>The distinct category names, alphabetically. A list of strings, not of entities.</summary>
    /// <returns>8 names.</returns>
    [HttpGet(nameof(GetCategories))]
    public List<string> GetCategories()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetCategories

    public class GetCategoriesTests : GroceryTest
    {
        [Fact]
        public void Returns_distinct_names_in_order()
        {
            var result = Controller.GetCategories();
            Assert.Equal(8, result.Count);
            Assert.Equal(result.Distinct().OrderBy(x => x), result);
            Assert.Equal("Bakery", result[0]);
        }
    }

    #endregion

    /// <summary>
    /// How many items sit in one category — a scalar, counted by the database.
    /// </summary>
    /// <returns>4 for "Meat", 5 for "Produce", 0 for a category that does not exist.</returns>
    [HttpGet(nameof(CountInCategory))]
    public int CountInCategory([FromQuery] string category)
    {
        throw new NotImplementedException();
    }

    #region Tests: CountInCategory

    public class CountInCategoryTests : GroceryTest
    {
        [Fact]
        public void Counts_one_group()
        {
            Assert.Equal(4, Controller.CountInCategory("Meat"));
            Assert.Equal(5, Controller.CountInCategory("Produce"));
        }

        [Fact]
        public void An_unknown_category_is_zero_not_an_error()
        {
            Assert.Equal(0, Controller.CountInCategory("Nonsense"));
        }

        [Fact]
        public void The_categories_add_up_to_the_whole_table()
        {
            Assert.Equal(32, Rows.Select(x => x.Category).Distinct().ToList().Sum(Controller.CountInCategory));
        }
    }

    #endregion

    /// <summary>
    /// How many items are kept at one storage temperature: ambient 16, chilled 12, frozen 4.
    /// </summary>
    [HttpGet(nameof(CountByStorage))]
    public int CountByStorage([FromQuery] StorageType storage)
    {
        throw new NotImplementedException();
    }

    #region Tests: CountByStorage

    public class CountByStorageTests : GroceryTest
    {
        [Fact]
        public void Counts_on_the_enum()
        {
            Assert.Equal(16, Controller.CountByStorage(StorageType.Ambient));
            Assert.Equal(12, Controller.CountByStorage(StorageType.Chilled));
            Assert.Equal(4, Controller.CountByStorage(StorageType.Frozen));
        }

        [Fact]
        public void The_storage_types_add_up_to_the_whole_table()
        {
            Assert.Equal(32, Enum.GetValues<StorageType>().Sum(Controller.CountByStorage));
        }
    }

    #endregion

    /// <summary>The N best sellers, by <see cref="GroceryItem.TimesPurchased"/> descending.</summary>
    /// <param name="n">Between 1 and 50.</param>
    /// <exception cref="ValidationException"><paramref name="n"/> is outside 1..50.</exception>
    [HttpGet(nameof(GetTopPurchased))]
    public List<GroceryItem> GetTopPurchased([FromQuery] int n = 5)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetTopPurchased

    public class GetTopPurchasedTests : GroceryTest
    {
        [Fact]
        public void Returns_the_best_sellers_first()
        {
            var result = Controller.GetTopPurchased(3);
            Assert.Equal(3, result.Count);
            Assert.Equal("Chopped Tomatoes 400g", result[0].Name);
            Assert.Equal(result.OrderByDescending(x => x.TimesPurchased).Select(x => x.Id), result.Select(x => x.Id));
        }

        [Fact]
        public void Throws_on_a_nonsense_count()
        {
            Assert.Throws<ValidationException>(() => Controller.GetTopPurchased(0));
            Assert.Throws<ValidationException>(() => Controller.GetTopPurchased(51));
        }
    }

    #endregion

    /// <summary>
    /// One page of items, ordered by name. Pair it with <see cref="Count"/> when a client needs to
    /// render "page 2 of 4".
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="size">Rows per page, at most 100.</param>
    /// <exception cref="ValidationException"><paramref name="page"/> is below 1, or <paramref name="size"/> is outside 1..100.</exception>
    [HttpGet(nameof(GetPage))]
    public List<GroceryItem> GetPage([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetPage

    public class GetPageTests : GroceryTest
    {
        [Fact]
        public void Returns_the_requested_slice()
        {
            var page = Controller.GetPage(2, 10);
            Assert.Equal(10, page.Count);
            Assert.Equal(Rows.OrderBy(x => x.Name).Skip(10).First().Name, page[0].Name);
        }

        [Fact]
        public void The_last_page_can_be_short()
        {
            Assert.Equal(2, Controller.GetPage(4, 10).Count);
        }

        [Fact]
        public void Paging_past_the_end_is_empty_not_an_error()
        {
            Assert.Empty(Controller.GetPage(99, 10));
        }

        [Fact]
        public void Throws_on_nonsense_paging()
        {
            Assert.Throws<ValidationException>(() => Controller.GetPage(0, 10));
            Assert.Throws<ValidationException>(() => Controller.GetPage(1, 0));
            Assert.Throws<ValidationException>(() => Controller.GetPage(1, 500));
        }
    }

    #endregion

    /// <summary>
    /// Every item, sorted by a caller-chosen column. The column is a <see cref="GrocerySort"/>
    /// enum rather than a string so a typo is a compile error and not a runtime surprise.
    /// Sorting must happen in SQL, not with <c>List.Sort</c> afterwards.
    /// </summary>
    /// <param name="by">Which column to sort on.</param>
    /// <param name="descending">Reverses the sort.</param>
    [HttpGet(nameof(GetSorted))]
    public List<GroceryItem> GetSorted([FromQuery] GrocerySort by, [FromQuery] bool descending = false)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetSorted

    public class GetSortedTests : GroceryTest
    {
        [Fact]
        public void Sorts_by_price_ascending()
        {
            var prices = Controller.GetSorted(GrocerySort.Price).Select(x => x.PriceDkk).ToList();
            Assert.Equal(prices.OrderBy(x => x), prices);
        }

        [Fact]
        public void Sorts_by_stock_descending()
        {
            var stock = Controller.GetSorted(GrocerySort.Stock, descending: true).Select(x => x.StockCount).ToList();
            Assert.Equal(stock.OrderByDescending(x => x), stock);
        }

        [Fact]
        public void Returns_the_whole_table_whatever_the_sort()
        {
            Assert.Equal(32, Controller.GetSorted(GrocerySort.Created).Count);
        }
    }

    #endregion

    /// <summary>
    /// The average price across the catalogue, as a scalar. SQLite has no real decimal type, so
    /// expect a long tail of digits and round where you display it, not in the query.
    /// </summary>
    [HttpGet(nameof(GetAveragePrice))]
    public decimal GetAveragePrice()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetAveragePrice

    public class GetAveragePriceTests : GroceryTest
    {
        [Fact]
        public void Averages_every_row()
        {
            Assert.Equal(32.39m, Math.Round(Controller.GetAveragePrice(), 2));
        }

        [Fact]
        public void Sits_between_the_cheapest_and_the_dearest()
        {
            var prices = Rows.Select(x => x.PriceDkk).ToList();
            Assert.InRange(Controller.GetAveragePrice(), prices.Min(), prices.Max());
        }
    }

    #endregion

    /// <summary>
    /// What the shelves are worth: <c>sum(PriceDkk * StockCount)</c> over every row, computed in
    /// SQL. Comes to 56554.4 on the seed data.
    /// </summary>
    [HttpGet(nameof(GetTotalStockValue))]
    public decimal GetTotalStockValue()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetTotalStockValue

    public class GetTotalStockValueTests : GroceryTest
    {
        [Fact]
        public void Multiplies_price_by_stock_before_summing()
        {
            Assert.Equal(56554.4m, Controller.GetTotalStockValue());
        }

        [Fact]
        public void Follows_a_restock()
        {
            var before = Controller.GetTotalStockValue();
            Rows.Where(x => x.Category == "Dairy").Set(x => x.StockCount, x => x.StockCount + 10).Update();
            Assert.True(Controller.GetTotalStockValue() > before);
        }
    }

    #endregion

    /// <summary>
    /// The average rating, which is nullable on the entity. Work out what SQL <c>avg()</c> does
    /// with NULLs — it is not what <c>List.Average()</c> would do — and what should come back when
    /// nothing is rated at all.
    /// </summary>
    [HttpGet(nameof(GetAverageRating))]
    public double? GetAverageRating()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetAverageRating

    public class GetAverageRatingTests : GroceryTest
    {
        [Fact]
        public void Ignores_unrated_rows_rather_than_counting_them_as_zero()
        {
            InsertRow(new GroceryItem { Name = "Unrated", Category = "Test", PriceDkk = 1m, WeightKg = 0.1, RatingAvg = null });
            var rated = Rows.Where(x => x.RatingAvg != null).Select(x => x.RatingAvg!.Value).ToList();
            Assert.Equal(rated.Average(), Controller.GetAverageRating()!.Value, 4);
        }

        [Fact]
        public void Is_null_when_nothing_is_rated()
        {
            Db.Groceries().Set(x => x.RatingAvg, (double?)null).Update();
            Assert.Null(Controller.GetAverageRating());
        }
    }

    #endregion

    /// <summary>
    /// The average price inside one category. "Meat" averages 69.86, "Produce" 14.57.
    /// </summary>
    /// <exception cref="NotFoundException">The category holds no rows, so there is nothing to average.</exception>
    [HttpGet(nameof(GetAveragePriceInCategory))]
    public decimal GetAveragePriceInCategory([FromQuery] string category)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetAveragePriceInCategory

    public class GetAveragePriceInCategoryTests : GroceryTest
    {
        [Fact]
        public void Averages_one_group()
        {
            Assert.Equal(69.86m, Math.Round(Controller.GetAveragePriceInCategory("Meat"), 2));
            Assert.Equal(14.57m, Math.Round(Controller.GetAveragePriceInCategory("Produce"), 2));
        }

        [Fact]
        public void Throws_for_an_empty_category()
        {
            Assert.Throws<NotFoundException>(() => Controller.GetAveragePriceInCategory("Nonsense"));
        }
    }

    #endregion

    /// <summary>Items whose best-before date is strictly in the past. Expect 3.</summary>
    [HttpGet(nameof(GetExpired))]
    public List<GroceryItem> GetExpired()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetExpired

    public class GetExpiredTests : GroceryTest
    {
        [Fact]
        public void Returns_only_past_dates()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var result = Controller.GetExpired();
            Assert.Equal(3, result.Count);
            Assert.All(result, x => Assert.True(x.BestBefore < today));
        }

        [Fact]
        public void Ignores_rows_without_a_best_before()
        {
            Assert.True(Rows.Any(x => x.BestBefore == null));
            Assert.All(Controller.GetExpired(), x => Assert.NotNull(x.BestBefore));
        }
    }

    #endregion

    /// <summary>
    /// Items whose <see cref="GroceryItem.BestBefore"/> falls between today and N days from now.
    /// Already expired rows are not "expiring soon" — they belong to <see cref="GetExpired"/>.
    /// <c>BestBefore</c> is a <see cref="DateOnly"/>, so compare it against a
    /// <see cref="DateOnly"/> and not a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="days">How far ahead to look. Must be at least 1.</param>
    /// <returns>11 items for the default 7 days.</returns>
    /// <exception cref="ValidationException"><paramref name="days"/> is below 1.</exception>
    [HttpGet(nameof(GetExpiring))]
    public List<GroceryItem> GetExpiring([FromQuery] int days = 7)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetExpiring

    public class GetExpiringTests : GroceryTest
    {
        [Fact]
        public void Looks_forward_only()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var result = Controller.GetExpiring();
            Assert.Equal(11, result.Count);
            Assert.All(result, x => Assert.True(x.BestBefore >= today));
        }

        [Fact]
        public void A_wider_window_returns_at_least_as_much()
        {
            Assert.True(Controller.GetExpiring(30).Count >= Controller.GetExpiring(7).Count);
        }

        [Fact]
        public void Throws_on_a_nonsense_window()
        {
            Assert.Throws<ValidationException>(() => Controller.GetExpiring(0));
        }
    }

    #endregion

    /// <summary>Items added to the catalogue within the last N days, newest first.</summary>
    /// <exception cref="ValidationException"><paramref name="days"/> is below 1.</exception>
    [HttpGet(nameof(GetRecentlyAdded))]
    public List<GroceryItem> GetRecentlyAdded([FromQuery] int days = 180)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetRecentlyAdded

    public class GetRecentlyAddedTests : GroceryTest
    {
        [Fact]
        public void Returns_the_newest_first()
        {
            var result = Controller.GetRecentlyAdded();
            Assert.Equal(result.OrderByDescending(x => x.CreatedAtUtc).Select(x => x.Id), result.Select(x => x.Id));
            Assert.Equal("Baby Spinach 175g", result[0].Name);
        }

        [Fact]
        public void A_brand_new_row_is_always_included()
        {
            var id = InsertRow(new GroceryItem { Name = "Fresh", Category = "Test", PriceDkk = 1m, WeightKg = 0.1 });
            Assert.Contains(Controller.GetRecentlyAdded(1), x => x.Id == id);
        }
    }

    #endregion

    /// <summary>
    /// Dead stock: items never purchased at all, or not purchased for N days.
    /// <see cref="GroceryItem.LastPurchasedAtUtc"/> is nullable and null means "never", which is
    /// the whole point of this one — a plain date comparison silently drops those rows.
    /// </summary>
    /// <param name="days">How long is too long. Must be at least 1.</param>
    /// <exception cref="ValidationException"><paramref name="days"/> is below 1.</exception>
    [HttpGet(nameof(GetStale))]
    public List<GroceryItem> GetStale([FromQuery] int days = 30)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetStale

    public class GetStaleTests : GroceryTest
    {
        [Fact]
        public void Includes_items_that_were_never_purchased()
        {
            var result = Controller.GetStale();
            Assert.Contains(result, x => x.Name == "Baby Spinach 175g");
            Assert.Contains(result, x => x.Name == "Tea Light Candles 50pcs");
            Assert.All(result, x => Assert.True(x.LastPurchasedAtUtc is null || x.LastPurchasedAtUtc < DateTime.UtcNow.AddDays(-30)));
        }

        [Fact]
        public void A_shorter_window_returns_at_least_as_much()
        {
            Assert.True(Controller.GetStale(7).Count >= Controller.GetStale(60).Count);
        }
    }

    #endregion

    /// <summary>
    /// Items that can be prepared within a time budget. <see cref="GroceryItem.PreparationTime"/>
    /// is a <see cref="TimeSpan"/> stored as ticks, so the comparison happens on ticks — check the
    /// generated SQL and convince yourself.
    /// </summary>
    /// <param name="maxMinutes">Inclusive upper bound. 15 minutes gives 5 items.</param>
    /// <exception cref="ValidationException"><paramref name="maxMinutes"/> is below 1.</exception>
    [HttpGet(nameof(GetQuickToPrepare))]
    public List<GroceryItem> GetQuickToPrepare([FromQuery] int maxMinutes = 15)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetQuickToPrepare

    public class GetQuickToPrepareTests : GroceryTest
    {
        [Fact]
        public void The_bound_is_inclusive()
        {
            var result = Controller.GetQuickToPrepare(15);
            Assert.Equal(5, result.Count);
            Assert.Contains(result, x => x.PreparationTime == TimeSpan.FromMinutes(15));
        }

        [Fact]
        public void Skips_rows_without_a_preparation_time()
        {
            Assert.All(Controller.GetQuickToPrepare(600), x => Assert.NotNull(x.PreparationTime));
            Assert.Equal(9, Controller.GetQuickToPrepare(600).Count);
        }
    }

    #endregion

    /// <summary>
    /// The most recently created row, judged by <see cref="GroceryItem.CreatedAtUtc"/>.
    /// When several rows share the newest timestamp, the one whose name sorts first wins —
    /// a query that can return two different rows on two runs is a broken query.
    /// </summary>
    /// <returns>The newest item ("Baby Spinach 175g" in the seed data).</returns>
    /// <exception cref="NotFoundException">The table is empty.</exception>
    [HttpGet(nameof(GetLatest))]
    public GroceryItem GetLatest()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetLatest

    public class GetLatestTests : GroceryTest
    {
        [Fact]
        public void Returns_the_newest_row()
        {
            Assert.Equal("Baby Spinach 175g", Controller.GetLatest().Name);
        }

        [Fact]
        public void Follows_new_inserts()
        {
            var id = InsertRow(new GroceryItem { Name = "Brand New", Category = "Test", PriceDkk = 1m, WeightKg = 0.1 });
            Assert.Equal(id, Controller.GetLatest().Id);
        }

        [Fact]
        public void Throws_when_the_table_is_empty()
        {
            Db.Groceries().Delete();
            Assert.Throws<NotFoundException>(() => Controller.GetLatest());
        }
    }

    #endregion

    /// <summary>
    /// The item bought most recently, judged by <see cref="GroceryItem.LastPurchasedAtUtc"/>.
    /// Rows that were never purchased (the column is null) are not candidates.
    /// Ties on the timestamp are broken by name ascending.
    /// </summary>
    /// <exception cref="NotFoundException">Nothing has ever been purchased.</exception>
    [HttpGet(nameof(GetLastPurchased))]
    public GroceryItem GetLastPurchased()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetLastPurchased

    public class GetLastPurchasedTests : GroceryTest
    {
        [Fact]
        public void Ignores_items_that_were_never_purchased()
        {
            var newest = Controller.GetLastPurchased();
            Assert.NotNull(newest.LastPurchasedAtUtc);
            Assert.Equal("Avocado", newest.Name);
        }

        [Fact]
        public void Throws_when_no_row_has_a_purchase_date()
        {
            Db.Groceries().Set(x => x.LastPurchasedAtUtc, (DateTime?)null).Update();
            Assert.Throws<NotFoundException>(() => Controller.GetLastPurchased());
        }
    }

    #endregion

    /// <summary>
    /// Items carrying one tag. <see cref="GroceryItem.Tags"/> is a semicolon-separated list, so
    /// "dinner" must not match a row tagged "dinners" — the match has to be on a whole element.
    /// </summary>
    /// <returns>5 items for "organic", 7 for "dinner".</returns>
    /// <exception cref="ValidationException">The tag is blank or itself contains a semicolon.</exception>
    [HttpGet(nameof(GetByTag))]
    public List<GroceryItem> GetByTag([FromQuery] string tag)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetByTag

    public class GetByTagTests : GroceryTest
    {
        [Fact]
        public void Finds_items_by_whole_tag()
        {
            Assert.Equal(5, Controller.GetByTag("organic").Count);
            Assert.Equal(7, Controller.GetByTag("dinner").Count);
        }

        [Fact]
        public void Does_not_match_a_partial_tag()
        {
            Assert.Empty(Controller.GetByTag("inner"));
            Assert.Empty(Controller.GetByTag("dinners"));
        }

        [Fact]
        public void Throws_on_a_malformed_tag()
        {
            Assert.Throws<ValidationException>(() => Controller.GetByTag(""));
            Assert.Throws<ValidationException>(() => Controller.GetByTag("a;b"));
        }
    }

    #endregion

    /// <summary>
    /// The price one item actually sells for: <c>PriceDkk * (1 - DiscountPercent / 100)</c>,
    /// computed by the database and returned as a single scalar. An item with no discount sells at
    /// full price.
    /// </summary>
    /// <remarks>
    /// Mind the arithmetic. SQLite stores a discount of 30 as the integer 30 and divides integers
    /// as integers, so <c>DiscountPercent / 100</c> is 0 and every item comes back at full price —
    /// and writing <c>100m</c> in C# does not save you, because the literal still reaches SQLite as
    /// 100. Multiplying by <c>0.01m</c> does work. Print the generated SQL for both and you will
    /// see exactly why.
    /// </remarks>
    /// <exception cref="NotFoundException">No row has that id.</exception>
    [HttpGet(nameof(GetPriceAfterDiscount))]
    public decimal GetPriceAfterDiscount([FromQuery] Guid id)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetPriceAfterDiscount

    public class GetPriceAfterDiscountTests : GroceryTest
    {
        [Fact]
        public void Applies_the_discount()
        {
            var salmon = Row("Salmon Fillet 300g");
            Assert.Equal(30m, salmon.DiscountPercent);
            Assert.Equal(62.65m, Controller.GetPriceAfterDiscount(salmon.Id), 2);
        }

        [Fact]
        public void An_undiscounted_item_sells_at_full_price()
        {
            Assert.Equal(12.50m, Controller.GetPriceAfterDiscount(GrocerySeed.IdOf(1)), 2);
        }

        [Fact]
        public void Throws_for_an_unknown_id()
        {
            Assert.Throws<NotFoundException>(() => Controller.GetPriceAfterDiscount(Guid.NewGuid()));
        }
    }

    #endregion

    /// <summary>
    /// A filtered list. Every parameter that is supplied narrows the result; every parameter left
    /// null is ignored. Build one <see cref="IQueryable{T}"/> and chain <c>Where</c> onto it —
    /// filtering the list after <c>ToList()</c> is the wrong answer, and the generated SQL is how
    /// you prove which one you wrote.
    /// </summary>
    /// <param name="q">Matches name or brand, case-insensitive and partial.</param>
    /// <param name="category">Exact category match.</param>
    /// <param name="storage">Exact storage type match.</param>
    /// <param name="isOrganic">Only organic, or only non-organic.</param>
    /// <param name="inStock">True means stock above zero, false means sold out.</param>
    /// <param name="minPrice">Inclusive lower price bound.</param>
    /// <param name="maxPrice">Inclusive upper price bound.</param>
    /// <returns>Matching items, ordered by name. No parameters at all returns everything.</returns>
    /// <exception cref="ValidationException"><paramref name="minPrice"/> is greater than <paramref name="maxPrice"/>.</exception>
    [HttpGet(nameof(GetFiltered))]
    public List<GroceryItem> GetFiltered(
        [FromQuery] string? q = null,
        [FromQuery] string? category = null,
        [FromQuery] StorageType? storage = null,
        [FromQuery] bool? isOrganic = null,
        [FromQuery] bool? inStock = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        throw new NotImplementedException();
    }

    #region Tests: GetFiltered

    public class GetFilteredTests : GroceryTest
    {
        [Fact]
        public void No_criteria_returns_everything()
        {
            Assert.Equal(32, Controller.GetFiltered().Count);
        }

        [Fact]
        public void Combines_every_supplied_criterion()
        {
            var result = Controller.GetFiltered(category: "Dairy", isOrganic: true);
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.True(x.IsOrganic && x.Category == "Dairy"));
        }

        [Fact]
        public void Filters_on_price_range_and_storage()
        {
            var result = Controller.GetFiltered(storage: StorageType.Frozen, minPrice: 30m);
            Assert.Equal(3, result.Count);
            Assert.All(result, x => Assert.True(x.Storage == StorageType.Frozen && x.PriceDkk >= 30m));
        }

        [Fact]
        public void Finds_the_sold_out_rows()
        {
            Assert.Equal(4, Controller.GetFiltered(inStock: false).Count);
        }

        [Fact]
        public void Throws_when_the_range_is_inverted()
        {
            Assert.Throws<ValidationException>(() => Controller.GetFiltered(minPrice: 50m, maxPrice: 10m));
        }
    }

    #endregion

    /// <summary>
    /// The cheapest item in each category — 8 rows, one per category. Ties on price are broken by
    /// name ascending, so the result is stable.
    /// </summary>
    /// <remarks>
    /// The interesting one. Solve it with <c>GroupBy</c> plus a correlated subquery, then again
    /// with a window function (<c>Sql.Ext.RowNumber().Over().PartitionBy(...)</c>), and compare
    /// the two SQL statements.
    /// </remarks>
    [HttpGet(nameof(GetCheapestPerCategory))]
    public List<GroceryItem> GetCheapestPerCategory()
    {
        throw new NotImplementedException();
    }

    #region Tests: GetCheapestPerCategory

    public class GetCheapestPerCategoryTests : GroceryTest
    {
        [Fact]
        public void Returns_one_row_per_category()
        {
            var result = Controller.GetCheapestPerCategory();
            Assert.Equal(8, result.Count);
            Assert.Equal(8, result.Select(x => x.Category).Distinct().Count());
        }

        [Fact]
        public void Each_row_is_the_cheapest_in_its_category()
        {
            var all = Rows.ToList();
            foreach (var cheapest in Controller.GetCheapestPerCategory())
                Assert.Equal(all.Where(x => x.Category == cheapest.Category).Min(x => x.PriceDkk), cheapest.PriceDkk);
        }
    }

    #endregion
}
