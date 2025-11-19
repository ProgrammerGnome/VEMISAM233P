// REST API with ASP.NET Core Web API
// This code defines a simple REST API for a given controller (on the frontend, this is for a service).
// The given (unknown) controller is referred as "example" with any case.
// A task name (or API purpose) is referred as "task" with any case.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Example>>> GetExampleList()
    {
        // logic to get example list
        return await _context.Examples.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Example>> GetExampleById(int id)
    {
        // logic to get example by id
        var example = await _context.Examples.FindAsync(id);
        if (example == null) return NotFound(); // should display on web
        return example;
    }

    [HttpPost]
    public async Task<ActionResult<Example>> CreateExample(Example example)
    {
        // logic create example
        _context.Examples.Add(example);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetExample), new { id = example.Id }, example);
    }
}