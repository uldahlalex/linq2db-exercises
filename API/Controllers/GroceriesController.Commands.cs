using System.Runtime;
using API.Testing;
using Infa;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

public partial class GroceriesController
{
    /// <summary>
    /// Takes an item out of the range. Idempotent: discontinuing something already discontinued is
    /// a no-op, not an error — the caller asked for a state, and that state already holds.
    /// </summary>
    /// <exception cref="NotFoundException">No row has that id.</exception>
    [HttpPost(nameof(Discontinue))]
    public void Discontinue([FromQuery] Guid id)
    {
        
        //discotinues a specific thing
        var existing = db
            .Groceries()
            .Where(g => g.Id == id)
            .FirstOrDefault();
        if (existing == null)
            throw new NotFoundException("that was not found");

        existing.IsDiscontinued = true;
        db.Update(existing);

    }

    #region Tests: Discontinue

    public class DiscontinueTests : GroceryTest
    {
        [Fact]
        public void Flags_the_row()
        {
            Controller.Discontinue(GrocerySeed.IdOf(1));
            Assert.True(Row(GrocerySeed.IdOf(1)).IsDiscontinued);
        }

        [Fact]
        public void Doing_it_twice_is_fine()
        {
            Controller.Discontinue(GrocerySeed.IdOf(1));
            Controller.Discontinue(GrocerySeed.IdOf(1));
            Assert.True(Row(GrocerySeed.IdOf(1)).IsDiscontinued);
        }

        [Fact]
        public void Removes_it_from_the_category_listing()
        {
            Controller.Discontinue(GrocerySeed.IdOf(1));
            Assert.DoesNotContain(Rows.Where(x => x.Category == "Dairy" && !x.IsDiscontinued).ToList(), x => x.Id == GrocerySeed.IdOf(1));
        }

        [Fact]
        public void Throws_for_an_unknown_id()
        {
            Assert.Throws<NotFoundException>(() => Controller.Discontinue(Guid.NewGuid()));
        }
    }

    #endregion

    /// <summary>
    /// Puts a discontinued item back in the range. Idempotent, exactly like
    /// <see cref="Discontinue"/>.
    /// </summary>
    /// <exception cref="NotFoundException">No row has that id.</exception>
    [HttpPatch(nameof(Reactivate))]
    public void Reactivate([FromQuery] Guid id)
    {
        throw new NotImplementedException();
    }

    #region Tests: Reactivate

    public class ReactivateTests : GroceryTest
    {
        [Fact]
        public void Brings_the_row_back()
        {
            var rice = Row("Basmati Rice 1kg");
            Controller.Reactivate(rice.Id);
            Assert.Contains(Rows.Where(x => x.Category == "Pantry" && !x.IsDiscontinued).ToList(), x => x.Id == rice.Id);
        }

        [Fact]
        public void Undoes_a_discontinue()
        {
            Rows.Where(x => x.Id == GrocerySeed.IdOf(1)).Set(x => x.IsDiscontinued, true).Update();
            Controller.Reactivate(GrocerySeed.IdOf(1));
            Assert.False(Row(GrocerySeed.IdOf(1)).IsDiscontinued);
        }
    }

    #endregion

    /// <summary>
    /// Adds stock to every item in a category, in one statement. Discontinued items are skipped —
    /// you do not reorder something you stopped selling.
    /// </summary>
    /// <returns>How many rows were changed. 5 for "Dairy", 0 for an unknown category.</returns>
    /// <exception cref="ValidationException"><paramref name="amount"/> is below 1, or the category is blank.</exception>
    [HttpPost(nameof(Restock))]
    public int Restock([FromQuery] string category, [FromQuery] int amount)
    {
        //Validation
        if (amount < 1)
            throw new ValidationException("amount cannot be less than 1");
        if (string.IsNullOrWhiteSpace(category))
            throw new ValidationException("category cannot be whitespace");
        
        //Lookup and command
        return db
            .Groceries()
            .Where(g => g.Category == category && g.IsDiscontinued == false)
            .Set(g => g.StockCount, g => g.StockCount + amount)
            .Update();

    }

    #region Tests: Restock

    public class RestockTests : GroceryTest
    {
        [Fact]
        public void Adds_to_every_row_in_the_category()
        {
            var before = Rows.Where(x => x.Category == "Dairy").ToList().ToDictionary(x => x.Id, x => x.StockCount);
            Assert.Equal(5, Controller.Restock("Dairy", 50));
            foreach (var item in Rows.Where(x => x.Category == "Dairy").ToList())
                Assert.Equal(before[item.Id] + 50, item.StockCount);
        }

        [Fact]
        public void Skips_discontinued_rows()
        {
            Assert.Equal(4, Controller.Restock("Pantry", 10));
            Assert.Equal(0, Row("Basmati Rice 1kg").StockCount);
        }

        [Fact]
        public void An_unknown_category_changes_nothing()
        {
            Assert.Equal(0, Controller.Restock("Nonsense", 10));
        }

        [Fact]
        public void Throws_on_a_nonsense_amount()
        {
            Assert.Throws<ValidationException>(() => Controller.Restock("Dairy", 0));
            Assert.Throws<ValidationException>(() => Controller.Restock(" ", 10));
        }
    }

    #endregion

    /// <summary>
    /// Ends every discount campaign in a category by writing null back into
    /// <see cref="GroceryItem.DiscountPercent"/>. Only rows that actually carry a discount are
    /// touched, so a second run reports 0.
    /// </summary>
    /// <returns>How many rows were changed.</returns>
    /// <exception cref="ValidationException">The category is blank.</exception>
    [HttpPost(nameof(ClearDiscounts))]
    public int ClearDiscounts([FromQuery] string category)
    {
        throw new NotImplementedException();
    }

    #region Tests: ClearDiscounts

    public class ClearDiscountsTests : GroceryTest
    {
        [Fact]
        public void Writes_null_back_into_the_column()
        {
            Assert.Equal(2, Controller.ClearDiscounts("Dairy"));
            Assert.All(Rows.Where(x => x.Category == "Dairy").ToList(), x => Assert.Null(x.DiscountPercent));
        }

        [Fact]
        public void Running_it_again_reports_nothing_to_do()
        {
            Controller.ClearDiscounts("Dairy");
            Assert.Equal(0, Controller.ClearDiscounts("Dairy"));
        }
    }

    #endregion

    /// <summary>
    /// Starts a discount campaign on a category. Only rows that are <em>not already discounted</em>
    /// are touched, so running the same campaign twice does not deepen an existing discount and the
    /// second run reports 0 rows. Discontinued rows are skipped.
    /// </summary>
    /// <param name="percent">Above 0 and at most 100.</param>
    /// <returns>How many rows were changed.</returns>
    /// <exception cref="ValidationException">The percent is outside the range, or the category is blank.</exception>
    [HttpPost(nameof(ApplyDiscount))]
    public int ApplyDiscount([FromQuery] string category, [FromQuery] decimal percent)
    {
        if (percent <= 0 || percent >= 100)
            throw new ValidationException("not valid range");
        if (string.IsNullOrWhiteSpace(category))
            throw new ValidationException("non valid cat");
        return db.Groceries()
            .Where(g => !g.IsDiscontinued && (g.DiscountPercent == null || g.DiscountPercent == 0) && g.Category == category)
            .Set(g => g.DiscountPercent, i => percent)
            .Update();
        
    }

    #region Tests: ApplyDiscount

    public class ApplyDiscountTests : GroceryTest
    {
        [Fact]
        public void Discounts_only_the_undiscounted_rows()
        {
            Assert.Equal(2, Controller.ApplyDiscount("Frozen", 25m));
            Assert.All(Rows.Where(x => x.Category == "Frozen").ToList(), x => Assert.NotNull(x.DiscountPercent));
        }

        [Fact]
        public void Running_it_again_changes_nothing()
        {
            Controller.ApplyDiscount("Frozen", 25m);
            Assert.Equal(0, Controller.ApplyDiscount("Frozen", 50m));
            Assert.Equal(15m, Row("Pepperoni Pizza").DiscountPercent);
        }

        [Fact]
        public void Throws_on_a_nonsense_percent()
        {
            Assert.Throws<ValidationException>(() => Controller.ApplyDiscount("Frozen", 0m));
            Assert.Throws<ValidationException>(() => Controller.ApplyDiscount("Frozen", 101m));
        }
    }

    #endregion

    /// <summary>
    /// Housekeeping: drops every row that is past its best-before date <em>and</em> has no stock
    /// left. An expired item still sitting on the shelf is a problem for a human, not for a 
    /// <c>DELETE</c> operation.
    /// </summary>
    /// <returns>How many rows were removed. 3 on fresh seed data.</returns>
    [HttpDelete(nameof(DeleteExpired))]
    public int DeleteExpired()
    {
        //Lookup and command
        return db.Groceries()
            .Where(g => g.StockCount == 0 && g.BestBefore < DateOnly.FromDateTime(DateTime.Now))
            .Delete();
    }

    #region Tests: DeleteExpired

    public class DeleteExpiredTests : GroceryTest
    {
        [Fact]
        public void Removes_expired_rows_that_are_sold_out()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            Assert.Equal(3, Controller.DeleteExpired());
            Assert.Equal(29, RowCount);
            Assert.False(Rows.Any(x => x.BestBefore != null && x.BestBefore < today));
        }

        [Fact]
        public void Running_it_again_finds_nothing()
        {
            Controller.DeleteExpired();
            Assert.Equal(0, Controller.DeleteExpired());
        }

        [Fact]
        public void Keeps_expired_rows_that_still_have_stock()
        {
            Rows.Where(x => x.Category == "Dairy").Set(x => x.StockCount, x => x.StockCount + 5).Update();
            Assert.Equal(2, Controller.DeleteExpired());
            Assert.True(Rows.Any(x => x.Name == "Havarti Cheese 400g"));
        }
    }

    #endregion

    /// <summary>
    /// Removes a row for good — but only when it is safe to: an item still holding stock must not
    /// vanish from the books. Discontinue it and sell the rest first.
    /// </summary>
    /// <exception cref="NotFoundException">No row has that id, including when it was already deleted.</exception>
    /// <exception cref="ConflictException"><see cref="GroceryItem.StockCount"/> is above zero.</exception>
    [HttpDelete(nameof(Delete))]
    public void Delete([FromQuery] Guid id)
    {
        var g = db.Groceries().FirstOrDefault(g => g.Id == id) ?? throw new NotFoundException("");
        if (g.StockCount > 0)
            throw new ConflictException("");
        db.Delete(g);
    }

    #region Tests: Delete

    public class DeleteTests : GroceryTest
    {
        private Guid EmptyShelf() => Rows.First(x => x.StockCount == 0).Id;

        [Fact]
        public void Removes_an_item_with_no_stock()
        {
            var id = EmptyShelf();
            Controller.Delete(id);
            Assert.Equal(31, RowCount);
            Assert.False(RowExists(id));
        }

        [Fact]
        public void Refuses_while_stock_remains()
        {
            Assert.Throws<ConflictException>(() => Controller.Delete(GrocerySeed.IdOf(1)));
            Assert.Equal(32, RowCount);
        }

        [Fact]
        public void Deleting_twice_reports_it_is_gone()
        {
            var id = EmptyShelf();
            Controller.Delete(id);
            Assert.Throws<NotFoundException>(() => Controller.Delete(id));
        }
    }

    #endregion

    /// <summary>
    /// Adds a new item to the catalogue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Four columns are owned by the server and whatever the caller sends in them is ignored:
    /// <see cref="GroceryItem.Id"/> gets a freshly generated <see cref="Guid"/>,
    /// <see cref="GroceryItem.CreatedAtUtc"/> gets <c>DateTime.UtcNow</c>,
    /// <see cref="GroceryItem.TimesPurchased"/> starts at 0 and
    /// <see cref="GroceryItem.LastPurchasedAtUtc"/> starts null. A client cannot backdate a row or
    /// invent a sales history.
    /// </para>
    /// <para>
    /// Validation rules, all of them <see cref="ValidationException"/>:
    /// name is required and at most 120 characters; category is required; price is not negative;
    /// stock count is not negative; weight is above zero; discount percent is null or within
    /// 0 (exclusive) to 100; rating is null or within 0..5; barcode is null or exactly 13 digits;
    /// best-before is null or not in the past.
    /// </para>
    /// </remarks>
    /// <returns>The generated id.</returns>
    /// <exception cref="ValidationException">Any rule above is broken.</exception>
    /// <exception cref="ConflictException">Another row already carries the same barcode.</exception>
    [HttpPost(nameof(Create))]
    public Guid Create([FromBody] GroceryItem item)
    {
        //Validation rules
        if (String.IsNullOrWhiteSpace(item.Name))
            throw new ValidationException("Cannot be whitesapce");

        if (item.Barcode != null && item.Barcode.Length != 13)
            throw new ValidationException("barcode must be 13 chars if not null");
        
        //Object instatiation
        var newObject = new GroceryItem()
        {
            //passed by the client
            Brand = item.Brand,
            Name = item.Name,
            
            //determined by the server
            CreatedAtUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            TimesPurchased = 0
        };
        //Command
        db.Insert(newObject);
        //Returning
        return newObject.Id;
    }

    #region Tests: Create

    public class CreateTests : GroceryTest
    {
        public static GroceryItem Sample() => new()
        {
            Name = "Test Sourdough",
            Brand = "Lab",
            Category = "Bakery",
            Tags = "bread;test",
            PriceDkk = 42.50m,
            DiscountPercent = 12.5m,
            StockCount = 3,
            WeightKg = 0.9,
            RatingAvg = 4.25,
            IsOrganic = true,
            Storage = StorageType.Chilled,
            SuppliedBy = Supplier.LocalFarm,
            BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
            PreparationTime = TimeSpan.FromMinutes(45)
        };

        [Fact]
        public void Inserts_the_item_and_returns_its_id()
        {
            var id = Controller.Create(Sample());
            Assert.NotEqual(Guid.Empty, id);
            Assert.Equal(33, RowCount);
            Assert.Equal("Test Sourdough", Row(id).Name);
        }

        [Fact]
        public void Generates_a_fresh_id_and_ignores_the_one_it_was_given()
        {
            var item = Sample();
            item.Id = GrocerySeed.IdOf(1);
            var id = Controller.Create(item);
            Assert.NotEqual(GrocerySeed.IdOf(1), id);
            Assert.Equal("Whole Milk 1L", Row(GrocerySeed.IdOf(1)).Name);
        }

        [Fact]
        public void Stamps_the_creation_time_and_ignores_a_backdated_one()
        {
            var item = Sample();
            item.CreatedAtUtc = new DateTime(1999, 1, 1);
            var saved = Row(Controller.Create(item));
            Assert.InRange(saved.CreatedAtUtc, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        }

        [Fact]
        public void Resets_the_purchase_history()
        {
            var item = Sample();
            item.TimesPurchased = 9000;
            item.LastPurchasedAtUtc = DateTime.UtcNow;
            var saved = Row(Controller.Create(item));
            Assert.Equal(0, saved.TimesPurchased);
            Assert.Null(saved.LastPurchasedAtUtc);
        }

        [Fact]
        public void Round_trips_every_data_type_through_sqlite()
        {
            var saved = Row(Controller.Create(Sample()));

            Assert.Equal(42.50m, saved.PriceDkk);
            Assert.Equal(12.5m, saved.DiscountPercent);
            Assert.Equal(0.9, saved.WeightKg);
            Assert.Equal(4.25, saved.RatingAvg);
            Assert.True(saved.IsOrganic);
            Assert.False(saved.IsDiscontinued);
            Assert.Equal(StorageType.Chilled, saved.Storage);
            Assert.Equal(Supplier.LocalFarm, saved.SuppliedBy);
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), saved.BestBefore);
            Assert.Equal(TimeSpan.FromMinutes(45), saved.PreparationTime);
            Assert.Null(saved.Barcode);
        }

        [Fact]
        public void Rejects_a_blank_name()
        {
            var item = Sample();
            item.Name = "   ";
            Assert.Throws<ValidationException>(() => Controller.Create(item));
            Assert.Equal(32, RowCount);
        }

        [Fact]
        public void Rejects_negative_numbers()
        {
            var cheap = Sample();
            cheap.PriceDkk = -1m;
            Assert.Throws<ValidationException>(() => Controller.Create(cheap));

            var weightless = Sample();
            weightless.WeightKg = 0;
            Assert.Throws<ValidationException>(() => Controller.Create(weightless));
        }

        [Fact]
        public void Rejects_an_out_of_range_discount_or_rating()
        {
            var discounted = Sample();
            discounted.DiscountPercent = 120m;
            Assert.Throws<ValidationException>(() => Controller.Create(discounted));

            var rated = Sample();
            rated.RatingAvg = 9;
            Assert.Throws<ValidationException>(() => Controller.Create(rated));
        }

        [Fact]
        public void Rejects_a_malformed_barcode()
        {
            var item = Sample();
            item.Barcode = "123";
            Assert.Throws<ValidationException>(() => Controller.Create(item));
        }

        [Fact]
        public void Rejects_a_best_before_in_the_past()
        {
            var item = Sample();
            item.BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            Assert.Throws<ValidationException>(() => Controller.Create(item));
        }

        [Fact]
        public void Rejects_a_barcode_that_is_already_taken()
        {
            var item = Sample();
            item.Barcode = "5701234000011";
            Assert.Throws<ConflictException>(() => Controller.Create(item));
        }
    }

    #endregion

    
    /// <summary>
    /// Registers a sale: stock goes down by the quantity, the purchase counter goes up by it, and
    /// <see cref="GroceryItem.LastPurchasedAtUtc"/> is set to now — all three in a single
    /// <c>UPDATE</c>. Read the row, change it in C# and write it back and you have written a race
    /// condition; <c>.Set(x =&gt; x.StockCount, x =&gt; x.StockCount - quantity)</c> lets the
    /// database do the arithmetic.
    /// </summary>
    /// <param name="id">The item being sold.</param>
    /// <param name="quantity">How many. At least 1.</param>
    /// <exception cref="ValidationException"><paramref name="quantity"/> is below 1.</exception>
    /// <exception cref="NotFoundException">No row has that id.</exception>
    /// <exception cref="ConflictException">There is not enough stock. Nothing is changed.</exception>
    [HttpPost(nameof(Purchase))]
    public void Purchase([FromQuery] Guid id, [FromQuery] int quantity = 1)
    {
        throw new NotImplementedException();
    }

    #region Tests: Purchase

    public class PurchaseTests : GroceryTest
    {
        [Fact]
        public void Moves_stock_counter_and_timestamp_together()
        {
            var before = Row(GrocerySeed.IdOf(1));
            Controller.Purchase(GrocerySeed.IdOf(1), 2);
            var after = Row(GrocerySeed.IdOf(1));

            Assert.Equal(before.StockCount - 2, after.StockCount);
            Assert.Equal(before.TimesPurchased + 2, after.TimesPurchased);
            Assert.InRange(after.LastPurchasedAtUtc!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        }

        [Fact]
        public void Repeating_it_keeps_subtracting()
        {
            var before = Row(GrocerySeed.IdOf(1)).StockCount;
            Controller.Purchase(GrocerySeed.IdOf(1));
            Controller.Purchase(GrocerySeed.IdOf(1));
            Assert.Equal(before - 2, Row(GrocerySeed.IdOf(1)).StockCount);
        }

        [Fact]
        public void Refuses_to_oversell_and_changes_nothing()
        {
            var before = Row(GrocerySeed.IdOf(1));
            Assert.Throws<ConflictException>(() => Controller.Purchase(GrocerySeed.IdOf(1), before.StockCount + 1));
            var after = Row(GrocerySeed.IdOf(1));

            Assert.Equal(before.StockCount, after.StockCount);
            Assert.Equal(before.TimesPurchased, after.TimesPurchased);
        }

        [Fact]
        public void Throws_on_a_nonsense_quantity_or_unknown_id()
        {
            Assert.Throws<ValidationException>(() => Controller.Purchase(GrocerySeed.IdOf(1), 0));
            Assert.Throws<NotFoundException>(() => Controller.Purchase(Guid.NewGuid()));
        }
    }

    #endregion

    /// <summary>
    /// Replaces an existing row wholesale. The parameter is the complete entity: every column the
    /// caller owns is overwritten with what was sent, nulls included. There is no
    /// <c>UpdatePrice</c>, no <c>UpdateName</c> — one command that leaves the row in exactly the
    /// state the body describes, whatever it was before.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The row is found by <see cref="GroceryItem.Id"/>. The four server-owned columns from
    /// <see cref="Create"/> are preserved and never taken from the body: id, creation time,
    /// times purchased and last purchase time.
    /// </para>
    /// <para>
    /// The same validation rules as <see cref="Create"/> apply, except that best-before may be in
    /// the past — an item that has already expired still has to be editable.
    /// </para>
    /// <para>
    /// This must be idempotent: sending the same body twice leaves the same row state and throws
    /// nothing the second time. One <c>UPDATE</c> statement, no read-modify-write.
    /// </para>
    /// </remarks>
    /// <exception cref="ValidationException">A validation rule is broken.</exception>
    /// <exception cref="NotFoundException">No row has that id.</exception>
    /// <exception cref="ConflictException">Another row already carries the same barcode.</exception>
    [HttpPut(nameof(Update))]
    public void Update([FromBody] GroceryItem item)
    {
        var existing = db.Groceries().FirstOrDefault(g => g.Id == item.Id) ??
                       throw new NotFoundException("that did not exist");

        //Id, creation, purchased times and last purchase timestamp never updated
        existing.StockCount = item.StockCount;
        existing.Barcode = item.Barcode ?? existing.Barcode;
        existing.Category = item.Category;
        existing.Name = item.Name;
        existing.RatingAvg = item.RatingAvg ?? existing.RatingAvg;
        existing.BestBefore = item.BestBefore ?? existing.BestBefore;
        existing.PriceDkk = item.PriceDkk;
        existing.PriceDkk = (decimal)item.WeightKg;
        existing.Brand = item.Brand;
        existing.IsOrganic = item.IsOrganic;
        existing.Storage = item.Storage;
        existing.PreparationTime = item.PreparationTime;
        existing.SuppliedBy = item.SuppliedBy;
        existing.Tags = item.Tags;
        existing.CreatedAtUtc = existing.CreatedAtUtc;
        
        db.Update(item);
    }

    #region Tests: Update

    public class UpdateTests : GroceryTest
    {
        private GroceryItem Edited()
        {
            var item = Row(GrocerySeed.IdOf(1));
            item.Name = "Renamed Milk";
            item.PriceDkk = 11.11m;
            item.Storage = StorageType.Frozen;
            item.Brand = null;
            return item;
        }

        [Fact]
        public void Replaces_every_column_it_is_given()
        {
            Controller.Update(Edited());
            var saved = Row(GrocerySeed.IdOf(1));
            Assert.Equal("Renamed Milk", saved.Name);
            Assert.Equal(11.11m, saved.PriceDkk);
            Assert.Equal(StorageType.Frozen, saved.Storage);
            Assert.Null(saved.Brand);
        }

        [Fact]
        public void Is_idempotent()
        {
            Controller.Update(Edited());
            var first = Row(GrocerySeed.IdOf(1));
            Controller.Update(Edited());
            var second = Row(GrocerySeed.IdOf(1));

            Assert.Equal(first.Name, second.Name);
            Assert.Equal(first.PriceDkk, second.PriceDkk);
            Assert.Equal(first.CreatedAtUtc, second.CreatedAtUtc);
            Assert.Equal(32, RowCount);
        }

        [Fact]
        public void Preserves_the_server_owned_columns()
        {
            var before = Row(GrocerySeed.IdOf(1));

            var item = Edited();
            item.CreatedAtUtc = new DateTime(1999, 1, 1);
            item.TimesPurchased = 9000;
            item.LastPurchasedAtUtc = new DateTime(1999, 1, 1);
            Controller.Update(item);

            var after = Row(GrocerySeed.IdOf(1));
            Assert.Equal(before.CreatedAtUtc, after.CreatedAtUtc);
            Assert.Equal(before.TimesPurchased, after.TimesPurchased);
            Assert.Equal(before.LastPurchasedAtUtc, after.LastPurchasedAtUtc);
        }

        [Fact]
        public void Touches_no_other_row()
        {
            Controller.Update(Edited());
            Assert.Equal(16.95m, Row(GrocerySeed.IdOf(2)).PriceDkk);
        }

        [Fact]
        public void Throws_when_the_row_does_not_exist()
        {
            var item = Edited();
            item.Id = Guid.NewGuid();
            Assert.Throws<NotFoundException>(() => Controller.Update(item));
        }

        [Fact]
        public void Throws_when_the_body_is_invalid()
        {
            var item = Edited();
            item.PriceDkk = -1m;
            Assert.Throws<ValidationException>(() => Controller.Update(item));
            Assert.Equal("Whole Milk 1L", Row(GrocerySeed.IdOf(1)).Name);
        }

        [Fact]
        public void Throws_when_the_barcode_belongs_to_another_row()
        {
            var item = Edited();
            item.Barcode = "5701234000028";
            Assert.Throws<ConflictException>(() => Controller.Update(item));
        }

        [Fact]
        public void Accepts_its_own_barcode_unchanged()
        {
            Controller.Update(Edited());
            Assert.Equal("5701234000011", Row(GrocerySeed.IdOf(1)).Barcode);
        }
    }

    #endregion

    /// <summary>
    /// Inserts the item, or replaces the existing row that carries the same barcode. The barcode is
    /// the natural key here, which is what makes the command idempotent: replaying the same body
    /// any number of times leaves exactly one row, with the same id every time.
    /// </summary>
    /// <remarks>
    /// A barcode is mandatory — without a natural key there is nothing to be idempotent about.
    /// Otherwise the rules of <see cref="Create"/> (on insert) and <see cref="Update"/> (on
    /// replace) apply unchanged.
    /// </remarks>
    /// <returns>The id of the inserted or updated row.</returns>
    /// <exception cref="ValidationException">The barcode is missing, or a validation rule is broken.</exception>
    [HttpPut(nameof(Upsert))]
    public Guid Upsert([FromBody] GroceryItem item)
    {
        try
        {
            throw new AmbiguousImplementationException("SQL exception occured somewhere in library code");
        }
        catch
        {
            throw new ValidationException("Some error occured");
        }
        
    }

    #region Tests: Upsert

    public class UpsertTests : GroceryTest
    {
        private static GroceryItem Fresh() => new()
        {
            Name = "Upserted Item",
            Category = "Pantry",
            Barcode = "1234567890123",
            PriceDkk = 30m,
            StockCount = 5,
            WeightKg = 0.5
        };

        [Fact]
        public void Inserts_when_the_barcode_is_new()
        {
            var id = Controller.Upsert(Fresh());
            Assert.Equal(33, RowCount);
            Assert.Equal("Upserted Item", Row(id).Name);
        }

        [Fact]
        public void Replaying_the_same_body_changes_nothing()
        {
            var first = Controller.Upsert(Fresh());
            var second = Controller.Upsert(Fresh());

            Assert.Equal(first, second);
            Assert.Equal(33, RowCount);
        }

        [Fact]
        public void Replaces_the_row_that_owns_the_barcode()
        {
            var item = Fresh();
            item.Barcode = "5701234000011";
            var id = Controller.Upsert(item);

            Assert.Equal(GrocerySeed.IdOf(1), id);
            Assert.Equal(32, RowCount);
            Assert.Equal("Upserted Item", Row(GrocerySeed.IdOf(1)).Name);
        }

        [Fact]
        public void Throws_without_a_barcode()
        {
            var item = Fresh();
            item.Barcode = null;
            Assert.Throws<ValidationException>(() => Controller.Upsert(item));
        }
    }

    #endregion

    /// <summary>
    /// Imports a batch of new items. All or nothing: every item is validated by the rules of
    /// <see cref="Create"/> first, and if a single one fails, nothing at all is inserted. Ids and
    /// timestamps are generated per item exactly as in <see cref="Create"/>.
    /// </summary>
    /// <remarks>
    /// Barcodes must be unique both against the table and <em>within the batch</em> — two rows in
    /// the same payload carrying the same barcode is a conflict, not a silent overwrite.
    /// Insert them with <c>BulkCopy</c> inside one transaction.
    /// </remarks>
    /// <returns>How many rows were inserted.</returns>
    /// <exception cref="ValidationException">The batch is empty, or an item breaks a rule.</exception>
    /// <exception cref="ConflictException">A barcode is duplicated in the batch or already taken.</exception>
    [HttpPost(nameof(Import))]
    public int Import([FromBody] GroceryItem[] items)
    {
        throw new NotImplementedException();
    }

    #region Tests: Import

    public class ImportTests : GroceryTest
    {
        private static GroceryItem Item(string name, string? barcode = null) => new()
        {
            Name = name,
            Category = "Test",
            Barcode = barcode,
            PriceDkk = 10m,
            StockCount = 1,
            WeightKg = 0.2
        };

        [Fact]
        public void Inserts_the_whole_batch()
        {
            Assert.Equal(3, Controller.Import([Item("A"), Item("B"), Item("C")]));
            Assert.Equal(35, RowCount);
        }

        [Fact]
        public void Generates_an_id_for_every_row()
        {
            Controller.Import([Item("A"), Item("B")]);
            var imported = Rows.Where(x => x.Category == "Test").ToList();
            Assert.Equal(2, imported.Select(x => x.Id).Distinct().Count());
            Assert.DoesNotContain(imported, x => x.Id == Guid.Empty);
        }

        [Fact]
        public void One_bad_item_rolls_back_the_whole_batch()
        {
            var bad = Item("Bad");
            bad.PriceDkk = -5m;
            Assert.Throws<ValidationException>(() => Controller.Import([Item("Good"), bad]));
            Assert.Equal(32, RowCount);
        }

        [Fact]
        public void Rejects_duplicate_barcodes_inside_the_batch()
        {
            Assert.Throws<ConflictException>(() => Controller.Import([Item("A", "1112223334445"), Item("B", "1112223334445")]));
            Assert.Equal(32, RowCount);
        }

        [Fact]
        public void Rejects_a_barcode_that_is_already_in_the_table()
        {
            Assert.Throws<ConflictException>(() => Controller.Import([Item("A", "5701234000011")]));
            Assert.Equal(32, RowCount);
        }

        [Fact]
        public void Rejects_an_empty_batch()
        {
            Assert.Throws<ValidationException>(() => Controller.Import([]));
        }
    }

    #endregion

    /// <summary>
    /// Moves stock from one item to another — two <c>UPDATE</c>s that must both happen or neither.
    /// Wrap them in <c>db.BeginTransaction()</c> and commit only at the end.
    /// </summary>
    /// <exception cref="ValidationException">The amount is below 1, or the two ids are the same.</exception>
    /// <exception cref="NotFoundException">Either id is unknown.</exception>
    /// <exception cref="ConflictException">The source has less stock than the amount. Neither row changes.</exception>
    [HttpPost(nameof(TransferStock))]
    public void TransferStock([FromQuery] Guid fromId, [FromQuery] Guid toId, [FromQuery] int amount)
    {
        throw new NotImplementedException();
    }

    #region Tests: TransferStock

    public class TransferStockTests : GroceryTest
    {
        [Fact]
        public void Moves_stock_between_two_rows()
        {
            var from = Row(GrocerySeed.IdOf(1)).StockCount;
            var to = Row(GrocerySeed.IdOf(2)).StockCount;

            Controller.TransferStock(GrocerySeed.IdOf(1), GrocerySeed.IdOf(2), 10);

            Assert.Equal(from - 10, Row(GrocerySeed.IdOf(1)).StockCount);
            Assert.Equal(to + 10, Row(GrocerySeed.IdOf(2)).StockCount);
        }

        [Fact]
        public void Leaves_both_rows_alone_when_the_source_is_short()
        {
            var from = Row(GrocerySeed.IdOf(1)).StockCount;
            var to = Row(GrocerySeed.IdOf(2)).StockCount;

            Assert.Throws<ConflictException>(() => Controller.TransferStock(GrocerySeed.IdOf(1), GrocerySeed.IdOf(2), from + 1));

            Assert.Equal(from, Row(GrocerySeed.IdOf(1)).StockCount);
            Assert.Equal(to, Row(GrocerySeed.IdOf(2)).StockCount);
        }

        [Fact]
        public void Throws_on_nonsense_arguments()
        {
            Assert.Throws<ValidationException>(() => Controller.TransferStock(GrocerySeed.IdOf(1), GrocerySeed.IdOf(2), 0));
            Assert.Throws<ValidationException>(() => Controller.TransferStock(GrocerySeed.IdOf(1), GrocerySeed.IdOf(1), 5));
            Assert.Throws<NotFoundException>(() => Controller.TransferStock(GrocerySeed.IdOf(1), Guid.NewGuid(), 5));
        }
    }

    #endregion
}
