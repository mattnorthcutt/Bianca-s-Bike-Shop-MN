using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using BiancasBikes.Data;
using BiancasBikes.Models;

namespace BiancasBikes.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrderController : ControllerBase
{
  private BiancasBikesDbContext _dbContext;

  public WorkOrderController(BiancasBikesDbContext context)
  {
    _dbContext = context;
  }

  [HttpGet("incomplete")]
  [Authorize]
  public IActionResult GetIncompleteWorkOrders()
  {
    return Ok(_dbContext.WorkOrders
    .Include(wo => wo.Bike)
    .ThenInclude(b => b.Owner)
    .Include(wo => wo.Bike)
    .ThenInclude(b => b.BikeType)
    .Include(wo => wo.UserProfile)
    .Where(wo => wo.DateCompleted == null)
    .OrderBy(wo => wo.DateInitiated)
    .ThenByDescending(wo => wo.UserProfileId == null).ToList());
  }

  [HttpPost]
  [Authorize]
  public IActionResult CreateWorkOrder(WorkOrder workOrder)
  {
    workOrder.DateInitiated = DateTime.Now;
    _dbContext.WorkOrders.Add(workOrder);
    _dbContext.SaveChanges();
    return Created($"/api/workorder/{workOrder.Id}", workOrder);
  }

  [HttpPut("{id}")]
  [Authorize]
  public IActionResult UpdateWorkOrder(WorkOrder workOrder, int id)
  {
    WorkOrder workOrderToUpdate = _dbContext.WorkOrders.SingleOrDefault(wo => wo.Id == id);
    if (workOrderToUpdate == null)
    {
      return NotFound();
    }
    else if (id != workOrder.Id)
    {
      return BadRequest();
    }

    //These are the only properties that we want to make editable
    workOrderToUpdate.Description = workOrder.Description;
    workOrderToUpdate.UserProfileId = workOrder.UserProfileId;
    workOrderToUpdate.BikeId = workOrder.BikeId;

    _dbContext.SaveChanges();

    return NoContent();
  }

  [HttpPut("{id}/complete")]
  [Authorize]
  public IActionResult CompleteWorkOrder(int id)
  {
    // find the work order in the database
    WorkOrder wo = _dbContext.WorkOrders.SingleOrDefault(w => w.Id == id);

    // if it doesnt exist return a 404
    if (wo == null)
    {
      return NotFound();
    }
    // mark it as completed by setting DateCompleted
    wo.DateCompleted = DateTime.Now;
    // save the change to the database
    _dbContext.SaveChanges();
    // return a 204
    return NoContent();
  }

  [HttpDelete("{id}")]
  [Authorize]
  public IActionResult DeleteWorkOrder(int id)
  {
      // find the work order
      WorkOrder woToDelete = _dbContext.WorkOrders.SingleOrDefault(wo => wo.Id == id);
      // if not found give a 404
      if (woToDelete == null)
      {
          return NotFound();
      }
      // remove it from the DbSet
      _dbContext.WorkOrders.Remove(woToDelete);
      // save changes
      _dbContext.SaveChanges();
      // give a 204 if successfully deleted
      return NoContent();
  }

}
