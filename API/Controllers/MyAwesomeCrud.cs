using API;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

public class MyAwesomeCrudController(GroceryDatabase db) : ControllerBase
{
    
    //The goal here: Inserts, Updates and Deletes on the grocery DB

    [HttpPost(nameof(CreateGroceryItem))]
    public int CreateGroceryItem(string name, string category, string branc, DateTime createdAt)
    {
        //Validation (unhappy path)
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name cannot be whitespace");
        
        
        
        //Object instantiaion
        var groceryItem = new GroceryItem()
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = createdAt,
            Name = name,
            Brand = branc,
            Category = category
            
        };
        //Command
        return db.Insert(groceryItem);
    }


    [HttpPut(nameof(UpdateThing))]
    public GroceryItem UpdateThing(Guid id, decimal newDiscount)
    {
      
        //Lookup
        var existing = db.Groceries()
                           .FirstOrDefault(g => g.Id == id)
                       ?? throw new NotFoundException("Did not find thing with ID " + id);

        if (newDiscount < 0)
            throw new ValidationException("dicout cannot be less than 0");
        

        
        existing.DiscountPercent = newDiscount;
        
        //Command
         db.Update(existing);
         
         return existing;

    }

    public void UpdateManyThings()
    {
        
        db.Groceries()
            .Where(g => g.Brand == "Thing")
            .Set(g => g.DiscountPercent, g => 50)
            .Update();
    }

    [HttpDelete(nameof(DeleteThing))]
    public void DeleteThing(Guid id)
    {
        
        
       var existing = db
           .Groceries()
           .FirstOrDefault(g => g.Id == id) ?? throw new NotFoundException("Not found");

       if (existing.StockCount != 0)
           throw new ValidationException("cannot delete if stock exists");

       db.Delete(existing);
    }


    
}