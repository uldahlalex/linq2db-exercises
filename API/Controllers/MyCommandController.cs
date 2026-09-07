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
   
    

    
}

