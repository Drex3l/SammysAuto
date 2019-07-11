using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SammysAuto.Data;
using SammysAuto.Models;
using SammysAuto.Utility;
using SammysAuto.ViewModel;

namespace SammysAuto.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Services
        [Authorize]
        public async Task<IActionResult> Index(int carId,int? records = null)
        {
            var car =   await _context.Cars.FirstOrDefaultAsync(c => c.Id == carId);
            var model = new CarAndServicesViewModel
            {
                carId = car.Id,
                Make = car.Make,
                Model = car.Model,
                Style = car.Style,
                VIN = car.VIN,
                Year = car.Year,
                UserId = car.UserId,
                ServiceTypesObj = _context.ServiceTypes.ToList(),
                PastServicesObj = records == null ? _context.Services.Where(s => s.CarId == carId).OrderByDescending(s => s.DateAdded) : _context.Services.Where(s => s.CarId == carId).OrderByDescending(s => s.DateAdded).Take((int)records)
            };
            return View(model);   
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _context.Dispose();
            }
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Services/Create
        [Authorize(Roles = SD.AdminEndUser)]
        public Task<IActionResult> Create(int carId)
        {
            return this.Index(carId,5);
        }

        // POST: Services/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = SD.AdminEndUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarAndServicesViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.NewServiceObj.CarId = viewModel.carId;
                viewModel.NewServiceObj.DateAdded = DateTime.Now;
                _context.Add(viewModel.NewServiceObj);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create), new { carId = viewModel.carId});
            }
            var car = _context.Cars.FirstOrDefault(c => c.Id == viewModel.carId);
            var newModel = new CarAndServicesViewModel
            {
                carId = car.Id,
                Make = car.Make,
                Model = car.Model,
                Style = car.Style,
                VIN = car.VIN,
                Year = car.Year,
                UserId = car.UserId,
                ServiceTypesObj = _context.ServiceTypes.ToList(),
                PastServicesObj = _context.Services.Where(s => s.CarId == viewModel.carId).OrderByDescending(s => s.DateAdded).Take(5)
            };
            return View(newModel);
        }

        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // POST: Services/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Miles,Price,Details,DateAdded,CarId,ServicesTypeId")] Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // GET: Services/Delete/5
        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.Include(s => s.Car).Include(s => s.ServiceType).FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Services/Delete/5
        [Authorize(Roles = SD.AdminEndUser)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Service model)
        {
            var serviceId = model.Id;
            var carId = model.CarId;
            var service = await _context.Services.SingleOrDefaultAsync(m => m.Id == serviceId);
            if (service == null)
            {
                return NotFound();
            }
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Create), new { carId = carId});
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
