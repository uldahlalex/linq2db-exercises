using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


public class MyCommandController(GroceryDatabase db)
    : ControllerBase
{

    [HttpPost]
    public GroceryItem InsertThing(string name)
    {
        //Validation rules
        var any = db.Groceries().Any(g => g.Name == name);
        if (any)
            throw new ValidationException("there was already: " + name);
        
        //Object instantiation
        var groceryItem = new GroceryItem()
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            Name = name
        };
        //Command
         db.Insert(groceryItem);
         //Return
         return groceryItem;
    }

    [HttpPut]
    public void UpdateThing(Guid id, string newName)
    {
        //Validation rules
        var any = db.Groceries().Any(g => g.Name == newName);
        if (any)
            throw new ValidationException("there was already taken this name: " + newName);

        if (string.IsNullOrWhiteSpace(newName))
            throw new ValidationException("The name cannot be whitespace");
        
        //Lookup
        var thingWeAreUpdating = db.Groceries()
            .FirstOrDefault(g => g.Id == id) 
                                 ?? throw new NotFoundException("Not found");
        //Mutation
        thingWeAreUpdating.TimesPurchased++;
        
        //Command
        db.Update(thingWeAreUpdating);
    }

    [HttpPut]
    public void UpdateManyThings()
    {
        db.Groceries()
            .Where(g => g.DiscountPercent < 50)
            .Set(g => g.DiscountPercent,
                item => item.DiscountPercent + 1)
            .Update();
       

    }


    [HttpPut]
    public void UpdateWithOptionals(GroceryItem item)
    {
        //lookup
        var itemToUpdate = db.Groceries().FirstOrDefault(g => g.Id == item.Id) ?? throw new NotFoundException("");
        if (item.Name != null)
            itemToUpdate.Name = item.Name;
        db.Update(itemToUpdate);
        
    }

    [HttpDelete]
    public int DeleteThing(Guid id)
    {
    //Lookup
        var thing = db.Groceries()
            .FirstOrDefault(g => g.Id == id) ?? throw new NotFoundException("Did not exist");
        var now = DateTime.UtcNow;
        var onlyDateNow = DateOnly.FromDateTime(now);
        //validation logic: Is this before today????
        if (thing.BestBefore < onlyDateNow)
            throw new ValidationException("right now as after in time than the 'best before' date");
        
    
        
        //Optional validation logic
        if (thing.StockCount > 0)
            throw new ValidationException("Cannot delete if stock is above 0");
        
        //Command
        return db.Delete(thing);

    }

    
}

