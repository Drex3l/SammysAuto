# SammysAuto
|	PROJECT FIELD		|		CREDENTIAL		|
|-----------------------|-----------------------|
| Name					| **SammysAuto**		|
| Design Pattern		| **MVC**				|
| User Authentication	| **Individual**		|
| Database Server		| **MS SQL**			|
| Schema				| **SammysAuto**		|

## Description
System used by a typical garage to track **services** provided to **cars**.

### Project Notable Entities
* Service Type
* User
	* Admin
	* Customer
* Car
* Service

## Getting Started
Create new Project  

```bash
dotnet new mvc --auth Individual --use-local-db -o SammysAuto
```
Adding user authentication at project creation puts a couple of new files to picture

* **Data/**
	* Migrations/
	 <ul>
	 * 00000000000000_CreateIdentitySchema.cs
	 * 00000000000000_CreateIdentitySchema.Designer.cs
	 * ApplicationDbContextModelSnapshot.cs
	 </ul>
 * ApplicationDbContext.cs
 
### _Notable_ New Files, and Changes to old files
#### Startup.cs
Already, the database context has been registered with the [dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.2) container in the ```Startup.ConfigureServices``` method.  

```cs
public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
```

The ```Startup.Configure``` method has also seen changes, to add the ```app.UseAuthentication()``` call   

#### appsettings.json
A sample **connection string** is added already provided. We just needed to subscribe it to project details.

### Getting Ready
Next, we run the ```dotnet list package``` command, to ensure project references all the relevant packages at this point.  
The following should be the result:

```
Project 'SammysAuto' has the following package references
   [netcoreapp2.2]: 
   Top-level Package                                          Requested   Resolved
   > Microsoft.AspNetCore.All                                 2.2.5       2.2.5   
   > Microsoft.AspNetCore.App                           (A)   [2.2.0, )   2.2.0   
   > Microsoft.EntityFrameworkCore.SqlServer                  2.2.4       2.2.4   
   > Microsoft.EntityFrameworkCore.Tools                      2.2.4       2.2.4   
   > Microsoft.VisualStudio.Web.CodeGeneration.Design         2.2.3       2.2.3   

(A) : Auto-referenced package.
```

If any of these is missing, add with the ```dotnet add package``` command  

Now, the next step is updating database with the ```dotnet ef database update``` command.
## Preparation
First, we build a button template we gonna reuse throughout the project.  
### Icon Button Template
```bash
vim Models/IndividualButtonPartial.cs
```
```cs
using System;
using System.Text;

namespace SammysAuto.Models
{
    public class IndividualButtonPartial
    {
        public string ButtonType { get; set; }
        public string Action { get; set; }
        public string Glyph { get; set; }
        public string Text { get; set; }

        public int? ServiceId { get; set; }
        public string CustomerId { get; set; }
        public int? CarId { get; set; }

        public string ActionParameters
        {
            get
            {
                var param = new StringBuilder(@"/");
                if (ServiceId != 0 && ServiceId != null)
                {
                    param.Append(String.Format("{0}", ServiceId));
                }
                if (CustomerId != null && CustomerId.Length > 0)
                {
                    param.Append(String.Format("{0}", CustomerId));
                }
                if (CarId != 0 && CarId != null)
                {
                    param.Append(String.Format("{0}", CarId));
                }
                return param.ToString().Substring(0, param.Length);
            }
        }
    }
}
```
```bash
vim Views/Shared/_IndividualButtonPartial.cshtml
```
```cs
@model SammysAuto.Models.IndividualButtonPartial

<a type="button" class="btn btn-sm @Model.ButtonType" href="@Url.Action(Model.Action)@Model.ActionParameters">
    <i class="fa glyphicon glyphicon-@Model.Glyph fa-@Model.Glyph"></i>
    <span class="sr-only">@Model.Text</span>
</a>
```
```bash
vim Views/Shared/_TableButtonPartial.cshtml
```
```cs
@model SammysAuto.Models.IndividualButtonPartial

<td style="width:150px;">
    <div class="btn-group" role="group">
        @{await Html.RenderPartialAsync("_IndividualButtonPartial",
        new IndividualButtonPartial {
            Action = "Edit",
            ButtonType = "btn-primary",
            Glyph = "pencil",
            Text = "Edit",
            ServiceId = Model.ServiceId,
            CustomerId=Model.CustomerId,
            CarId=Model.CarId
        });}

        @{await Html.RenderPartialAsync("_IndividualButtonPartial",
        new IndividualButtonPartial {
            Action = "Details",
            ButtonType = "btn-success",
            Glyph = "list",
            Text = "Details Button",
            ServiceId = Model.ServiceId,
            CustomerId = Model.CustomerId,
            CarId = Model.CarId
        });}

        @{await Html.RenderPartialAsync("_IndividualButtonPartial",
        new IndividualButtonPartial {
            Action = "Delete",
            ButtonType = "btn-danger",
            Glyph = "trash",
            Text = "Delete Button",
            ServiceId = Model.ServiceId,
            CustomerId = Model.CustomerId,
            CarId = Model.CarId
        });}
    </div>
</td>
```
### User Accounts
Let's add ability to send/receive emails 

```bash
mkdir Services
vim Services/IEmailSender.cs
```
```cs
using System.Threading.Tasks;

namespace SammysAuto.Services
{
    public interface IEmailSender
    {
         Task SendEmailAsync(string email, string subject, string message);
    }
}
```
```bash
vim Services/EmailSender.cs
```
```cs
using System.Threading.Tasks;

namespace SammysAuto.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Task.CompletedTask;
        }
    }
}
```
Let's add a mechanism to give user **roles** to application.   

```bash
mkdir Utility
vim Utility/SD.cs
```
```cs
namespace SammysAuto.Utility
{
    public class SD
    {
        public const string AdminEndUser = "Admin";
        public const string CustomerEndUser = "Customer";
    }
}
```
Next, using the Entity Framework, we scaffold User Account related templates into existence;  


```bash
dotnet aspnet-codegenerator identity -u ApplicationUser -fi "Account.Register;Account.Login;Account.Logout" -f

```
We then add custom fields to the user account database table provided by the **Identity** framework, by means of the **Application User** class;  

```bash
vim Area/Identity/Data/ApplicationUser.cs
```
```cs
using Microsoft.AspNetCore.Identity;

namespace SammysAuto.Models
{
    public class ApplicationUser : IdentityUser
    {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string PostalCode { get; set; }
    }
}
```
We then inherit class **ApplicationDbContext** from ```IdentityDbContext<ApplicationUser>```, and add the following method;-  

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
   base.OnModelCreating(builder);
  // Customize the ASP.NET Identity model and override the defaults if needed.
  // For example, you can rename the ASP.NET Identity table names and more.
  // Add your customizations after calling base.OnModelCreating(builder);
}
```
 ...and perform a database Migrate   

```bash
dotnet ef migrations add addUserProperties
dotnet ef database update --context ApplicationDbContext
```
The first step is creating the relevant properties, and initializing them in the constructor.  

```cs
private readonly UserManager<ApplicationUser> _userManager;
private readonly SignInManager<ApplicationUser> _signInManager;
private readonly RoleManager<IdentityRole> _roleManager;
private ApplicationDbContext _db;
private readonly IEmailSender _emailSender;
private readonly ILogger _logger;
 
public AccountController
(
   UserManager<ApplicationUser> userManager,
   SignInManager<ApplicationUser> signInManager,
   RoleManager<IdentityRole> roleManager,

   IEmailSender emailSender,
   ILogger<AccountController> logger,
   ApplicationDbContext db
)
{
   _userManager = userManager;
   _signInManager = signInManager;
   _emailSender = emailSender;
   _logger = logger;
   _roleManager = roleManager;
   _db = db;
}

[TempData]
public string ErrorMessage { get; set; }
```

Next, we work on the **Register** action methods.  

```cs
[HttpGet]
[AllowAnonymous]
public IActionResult Register(string returnUrl = null)
{
   ViewData["ReturnUrl"] = returnUrl;
   return View();
}

[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
{
    ViewData["ReturnUrl"] = returnUrl;
    if (ModelState.IsValid)
	{
         var user = new ApplicationUser
         {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Address = model.Address,
            City = model.City,
            PostalCode = model.PostalCode,
			PhoneNumber = model.PhoneNumber
		};
        var result = await _userManager.CreateAsync(user, model.Password);
		if (result.Succeeded)
		{
			if (!await _roleManager.RoleExistsAsync(SD.CustomerEndUser))
			{
				await _roleManager.CreateAsync(new IdentityRole(SD.CustomerEndUser));
			}
			if (!await _roleManager.RoleExistsAsync(SD.AdminEndUser))
			{
				await _roleManager.CreateAsync(new IdentityRole(SD.AdminEndUser));
			}

			if (model.isAdmin)
			{
				await _userManager.AddToRoleAsync(user, SD.AdminEndUser);
			}
			else
			{
				await _userManager.AddToRoleAsync(user, SD.CustomerEndUser);
			}
			_logger.LogInformation("User created a new account with password.");
			//var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			//var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
			//await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl); 

			if (User.IsInRole(SD.AdminEndUser))
            {
				return RedirectToAction("Index", "Users");
            }
            else
			{
                var userDetails = await _userManager.FindByEmailAsync(model.Email);
                await _signInManager.SignInAsync(user, isPersistent: false);
            	return RedirectToAction("Index", "Cars", new { userId = userDetails.Id });
        	}
        }
    	AddErrors(result);
	}
    return View(model);
}
```
Next, we work on the **Login** action methods.  

```cs
[HttpGet]
[AllowAnonymous]
public async Task<IActionResult> Login(string returnUrl = null)
{
	// Clear the existing external cookie to ensure a clean login process
    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

    ViewData["ReturnUrl"] = returnUrl;
    return View();
}

[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
{
	ViewData["ReturnUrl"] = returnUrl;
    if (ModelState.IsValid)
    {
    	// This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if(result.Succeeded){
        	var user = await _userManager.FindByEmailAsync(model.Email);
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault().ToString().Equals(SD.AdminEndUser))
            {
            	return RedirectToAction("Index", "Users");
			}
            else
            {
            	return RedirectToAction("Index", "Cars"  , new { userId = user.Id });
            }
		}
	}
	// If we got this far, something failed, redisplay form
	return View(model);
}
```

We then update the ```Startup.ConfigureServices``` method with the following...  

```cs
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
	services.Configure<CookiePolicyOptions>(options =>
    {
    	// This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
	});

    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

	services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

	// Add application services.
	services.AddTransient<IEmailSender, EmailSender>();

	services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
}
```
We also update file ```Views/Shared/_LoginPartial.cshtml``` to being with the following lines  

```cs
@using Microsoft.AspNetCore.Identity
@using SammysAuto.Models

@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager
```

Give **Admin** ability to add other users, like customers, by adding relevant controller + views...  

```bash
dotnet aspnet-codegenerator controller -m SammysAuto.Models.ApplicationUser -name UsersController -dc ApplicationDbContext --relativeFolderPath Controllers --referenceScriptLibraries --useDefaultLayout

```
Add the following line at class level, on the ```Controllers/UsersController.cs``` file.  

```cs
[Authorize(Roles = SD.AdminEndUser)]
```
Modify the ```DELETE POST``` action to simulate *'cascade on delete'* database event;   

```cs
// POST: Users/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(string id)
{
	var userInDb = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
	var cars = _context.Cars.Where(x => x.UserId == userInDb.Id);
	List<Car> listCar = cars.ToList();

	foreach (var car in listCar)
	{
		var servcies = _context.Services.Where(x => x.CarId == car.Id);
		_context.Services.RemoveRange(servcies);
    }

	_context.Cars.RemoveRange(cars);
	_context.Users.Remove(userInDb);
	await _context.SaveChangesAsync();

	return RedirectToAction(nameof(Index));
}

```
### Authorization and Page Navigation
Tweak the page navigation ```<ul>``` inside the ```<nav>``` element of the ````Views/Shared/_Layout.cshtml``` page, so it resembles the following;  

```cs
<ul class="navbar-nav flex-grow-1">
	@if (User.IsInRole(SD.AdminEndUser))
	{
		<li class="nav-item"><a class="nav-link text-dark" asp-area="" asp-controller="Users" asp-action="Index">Users</a></li>
		<li class="nav-item"><a class="nav-link text-dark" asp-area="" asp-controller="ServiceTypes" asp-action="Index">Service Types</a></li>
	}
		<li class="nav-item"><a class="nav-link text-dark" asp-area="" asp-controller="Cars" asp-action="Index">Cars</a></li>
		<li class="nav-item"><a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Privacy">Privacy</a></li>
</ul>                    
```
## Functionality
### Service Tpye

We first add entity to the **Models**:

```bash
vim Models/ServiceType.cs
```
```cs
namespace SammysAuto.Models
{
    public class ServiceType
    {
        public int Id { get; set;}
        [Required]
        public string Name { get; set;}
    }
}
```

We then add a entity property to the **database context**.  

```bash
vim Data/ApplicationDbContext.cs
```
```cs
public DbSet<ServiceType> ServiceTypes { get; set; }
```
We then migrate entity to database  

```bash
 dotnet ef migrations add AddServiceTypeToDB
 dotnet ef database update
```

Next, we scaffold entity **Controller** and  **Views** into existance.  


```bash
dotnet aspnet-codegenerator controller -m ServiceType -name ServiceTypesController -dc ApplicationDbContext --relativeFolderPath Controllers --referenceScriptLibraries --useDefaultLayout

```

Make access to this controller exclusive to **admin**, by adding the following line to it, at _class level_.  

```cs
[Authorize(Roles = SD.AdminEndUser)]
```


### Cars
We first take a look at the _back-end_ layer.
#### Model
```bash
vim Models/Cars.cs
```
```cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SammysAuto.Models
{
    public class Car
    {
        [Required]
        public int Id { get; set;}
        [Required]
        public string VIN { get; set;}
        [Required]
        public string Make { get; set;}
        [Required]
        public string Model { get; set;}
        public string Style { get; set;}
        [Required]
        public int Year { get; set;}
        public double Miles { get; set;}
        [Required]
        public string Color { get; set;}
        public string UserId { get; set;}
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set;}
    }
}
```
We update the database context with the following property;   

```cs
public DbSet<Car> Cars { get; set; }
```
We migrate **Model** to database.  

```bash
dotnet ef migrations add addCarToDb
dotnet ef database update
```
We need a **ViewModel** to list Cutsomers and Cars.  

```bash
mkdir ViewModel
vim ViewModel/CarAndCustomerViewModel.cs
```
```cs
using System.Collections.Generic;
using SammysAuto.Models;

namespace SammysAuto.ViewModel
{
    public class CarAndCustomerViewModel
    {
        public ApplicationUser UserObj { get; set; }
        public IEnumerable<Car> Cars { get; set;}
    }
}
```
#### Controller
Like most controllers, this one is also gonna implement authorization, by placing the ```[Authorize]``` construct at class level.

```bash
dotnet aspnet-codegenerator controller -m SammysAuto.Models.Car -name CarsController -dc ApplicationDbContext --relativeFolderPath Controllers --referenceScriptLibraries --useDefaultLayout

```
As always, we add a method to dispose database instance, as first step of the new controller.  

```cs
protected override void Dispose(bool disposing)
{
    if(disposing)
    {
         _context.Dispose();
    }
}
```
Next, we change the **Index** action to return the **CarsAndCustomerViewModel**  

```cs
public async Task<IActionResult> Index(string userId = null)
{
       // guest user exclusive
      userId = (userId == null) ? this.User.FindFirstValue(ClaimTypes.NameIdentifier) : userId;
      var model = new CarAndCustomerViewModel 
      {
            Cars = _context.Cars.Where(c => c.UserId == userId),
            UserObj = _context.Users.FirstOrDefault(u => u.Id == userId)
      };
      return View(model);
}
```
We modify the **Action** methods, subscribing them to context;   

```cs
// GET: Cars/Create
public IActionResult Create(string userId)
{
    Car carObj = new Car{Year = DateTime.Now.Year,UserId = userId};
    return View(carObj);
}

// POST: Cars/Create
// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("Id,VIN,Make,Model,Style,Year,Miles,Color,UserId")] Car car)
{
   if (ModelState.IsValid)
   {
      _context.Add(car);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index), new {userId = car.UserId});
   }
   return View(car);
}
```
We change the return values of both, the ```CarsController.Edit``` and the ```CarsController.DeleteConfirmed``` methods, to;  

```cs
return RedirectToAction(nameof(Index), new {userId = car.UserId});
```
We also need to verify user deleting the car on the ```CarsController.DeleteConfirmed``` method, with the following method;   

```cs
var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);

```
#### Views
First step is recreating the index view that it looks as the following;  

```cs
@model SammysAuto.ViewModel.CarAndCustomerViewModel
@using SammysAuto.Utility;

@{
    ViewData["Title"] = "Index";
}

<h2>Car Record for Customer</h2>
<div class="row">
    <div class="col-sm-6">
        <p>
            @if (Model.UserObj != null)
            {
            <a asp-action="Create" asp-route-userId="@Model.UserObj.Id" class="btn btn-primary">Add New</a>
            }
        </p>
    </div>
    <div class="col-sm-6">
        <div class="row">
            <div class="col-sm-4">
                <label asp-for="UserObj.FirstName" class="control-label"></label>
            </div>
            <div class="col-sm-8">
                <input asp-for="UserObj.FirstName" disabled class="form-control" />
            </div>
        </div>
        <div class="row">
            <div class="col-sm-4">
                <label asp-for="UserObj.LastName" class="control-label"></label>
            </div>
            <div class="col-sm-8">
                <input asp-for="UserObj.LastName" disabled class="form-control" />
            </div>
        </div>
        <div class="row">
            <div class="col-sm-4">
                <label asp-for="UserObj.PhoneNumber" class="control-label"></label>
            </div>
            <div class="col-sm-8">
                <input asp-for="UserObj.PhoneNumber" disabled class="form-control" />
            </div>
        </div>
        <div class="row">
            <div class="col-sm-4">
                <label asp-for="UserObj.Email" class="control-label"></label>
            </div>
            <div class="col-sm-8">
                <input asp-for="UserObj.Email" disabled class="form-control" />
            </div>
        </div>
    </div>
</div>
<br/>
@if (Model.Cars.Count() == 0)
{
    <div class="text-primary form-bord"> No Car Found! Please <b>add Car</b>...</div>
}
else
{
<table class="table">
    <thead>
        <tr>
            <th>
                @Html.DisplayNameFor(model => model.Cars.FirstOrDefault().VIN)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Cars.FirstOrDefault().Make)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Cars.FirstOrDefault().Model)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Cars.FirstOrDefault().Style)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Cars.FirstOrDefault().Year)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Cars.FirstOrDefault().Color)
            </th>
            <th></th>
            <th></th>
        </tr>
    </thead>
    <tbody>
@foreach (var item in Model.Cars) {
        <tr>
            <td>
                @Html.DisplayFor(modelItem => item.VIN)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.Make)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.Model)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.Style)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.Year)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.Color)
            </td>
            <td>
                @{ var btn = User.IsInRole(SD.CustomerEndUser) ? new { type = "primary", icon = "wrench", label = "Service History"} : new { type = "success", icon = "plus-sign", label = "New Service" };
                <a class="btn btn-sm btn-@btn.type" asp-controller="Services" asp-action="Index" asp-route-CarId="@item.Id">
                    <span class="glyphicon glyphicon-@btn.icon"></span>@btn.label
                </a>
                }
            </td>
            <td>
                @{await Html.RenderPartialAsync("_TableButtonPartial", new IndividualButtonPartial { CarId=item.Id});}
            </td>
        </tr>
}
    </tbody>
</table>
}
```
We modify the **'Back to List'** link, at the bottom of each page, to make sure it includes the **user ID**.  

```
<a asp-action="Index" asp-route-userId="@Model.UserId">Back to List</a>

```
We also add a hiiden field to store the user ID in the **Edit.cshtml** page.   

### Services
As always, we commence with the back-end layer.  
#### Model
```bash
vim Models/Service.cs
```
```cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SammysAuto.Models
{
    public class Service
    {
        public int Id { get; set; }
        public double Miles { get; set; }
        public double Price { get; set; }
        public string Details { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime DateAdded { get; set;}

        public int CarId { get; set; }
        public int ServiceTypeId { get; set;}

        [ForeignKey("CarId")]
        public virtual Car Car { get; set;}
        [ForeignKey("ServiceTypeId")]
        public virtual ServiceType ServiceType { get; set; }
    }
}
```
We add property to **database context**.  

```cs
public DbSet<Service> Services { get; set; }
```
We the migrate to database.  

```bash
dotnet ef migrations add addServicesToDb
dotnet ef database update
```
We then add a Services **view** model.   

```bash
vim ViewModels/CarAndServicesViewModel.cs
```
```cs
using System.Collections.Generic;
using SammysAuto.Models;

namespace SammysAuto.ViewModel
{
    public class CarAndServicesViewModel
    {
         public int carId { get; set;}
         public string VIN { get; set;}
         public string Make { get; set;}
         public string Model { get; set;}
         public string Style { get; set;}
         public int Year { get; set;}
         public string UserId { get; set; }
         
         public Service NewServiceObj { get; set; }
         public IEnumerable<Service> PastServices { get; set;}
         public List<ServiceType> ServiceTypesObj { get; set; }
    }
}
```
#### Controller
```bash
dotnet aspnet-codegenerator controller -m SammysAuto.Models.Service -name ServicesController -dc ApplicationDbContext --relativeFolderPath Controllers --referenceScriptLibr

```
Step One, as always, we create a method to dispose database context.  
Step Two, we modify the **index**, and **create** actions to the following:  

```cs
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
public async Task<IActionResult> Create(CarAndServicesViewModel model)
{
	if (ModelState.IsValid)
	{
		model.NewServiceObj.CarId = model.carId;
        model.NewServiceObj.DateAdded = DateTime.Now;
        _context.Add(model.NewServiceObj);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Create), new { carId = model.carId});
    }
    var car = _context.Cars.FirstOrDefault(c => c.Id == model.carId);
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
		PastServiceObj = _context.Services.Where(s => s.CarId == model.carId).OrderByDescending(s => s.DateAdded).Take(5)
	};
	return View(newModel);
}
```
We also add ```[Authorize(Roles = SD.AdminEndUser)]```, on the ```DELETE GET``` action, and modify the definition of the ```service``` object, to the following;   

```cs
var service = await _context.Services.Include(s => s.Car).Include(s => s.ServiceType).FirstOrDefaultAsync(m => m.Id == id);

```
We modify the ```DELETE POST``` to be as follows;   

```cs
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
```
#### View
The first step of the **view** layer is creating the following partial pages;  

```bash
vim Views/Shared/_CarDetailsInService.cshtml
```
```cs
@model SammysAuto.ViewModel.CarAndServicesViewModel

<h4>Customer Car record</h4>
<div class="row form-border">
    <div class="col-sm-5">
        @* Make and Model *@
        <div class="row">
            <div class="col-sm-3">
                <label asp-for="Make" class="control-label"></label>
            </div>
            <div class="col-sm-9" style="padding-bottom:5px;">
                <input asp-for="Make" disabled class="form-control" />
            </div>
        </div>
        <div class="row">
            <div class="col-sm-3">
                <label asp-for="Model" class="control-label"></label>
            </div>
            <div class="col-sm-9" style="padding-bottom:5px;">
                <input asp-for="Model" disabled class="form-control" />
            </div>
        </div>
       
    </div>
    <div class="col-sm-2"></div>
    <div class="col-sm-5">
        @* Style and Year and Model *@
        <div class="row">
            <div class="col-sm-3">
                <label asp-for="Style" class="control-label"></label>
            </div>
            <div class="col-sm-9" style="padding-bottom:5px;">
                <input asp-for="Style" disabled class="form-control" />
            </div>
        </div>
        <div class="row">
            <div class="col-sm-3">
                <label asp-for="Year" class="control-label"></label>
            </div>
            <div class="col-sm-9" style="padding-bottom:5px;">
                <input asp-for="Year" disabled class="form-control" />
            </div>
        </div>
    </div>
</div>
```
```bash
vim Views/Shared/_DisplayPastServices.cshtml
```
```
@model SammysAuto.ViewModel.CarAndServicesViewModel
@using SammysAuto.Utility
@using Newtonsoft.Json

    <table class="table table-condensed table-hover">
        <tr>
            <th>
                @Html.DisplayNameFor(m => m.PastServicesObj.FirstOrDefault().Miles)
            </th>
            <th>
                @Html.DisplayNameFor(m => m.PastServicesObj.FirstOrDefault().Price)
            </th>
            <th>
                @Html.DisplayNameFor(m => m.PastServicesObj.FirstOrDefault().Details)
            </th>
            <th>
                @Html.DisplayNameFor(m => m.PastServicesObj.FirstOrDefault().DateAdded)
            </th>
            <th>
                @Html.DisplayNameFor(m => m.PastServicesObj.FirstOrDefault().ServiceType.Name)
            </th>
            <th></th>
        </tr>

        @foreach (var item in Model.PastServicesObj)
        {
            <tr>
                <td>
                    @Html.DisplayFor(m => item.Miles)
                </td>
                <td>
                    @Html.DisplayFor(m => item.Price)
                </td>
                <td>
                    @Html.DisplayFor(m => item.Details)
                </td>
                <td>
                    @Html.DisplayFor(m => item.DateAdded)
                </td>
                <td>
                    @Html.DisplayFor(m => item.ServiceType.Name)
                </td>
                <td>
                    @if (item.DateAdded.Date == DateTime.Now.Date  && User.IsInRole(SD.AdminEndUser))
                    {
                        await Html.RenderPartialAsync("_IndividualButtonPartial", new IndividualButtonPartial
                        {
                            Action = "Delete",
                            ButtonType = "btn-danger",
                            Glyph = "trash",
                            Text = "Delete Button",
                            ServiceId = item.Id
                        });
                    }
                </td>
            </tr>
        }
    </tables>
```
Then we consume these partial views in relevant places;  

```bash
vim Views/Services/Index.cshtml
```
```cs
@model SammysAuto.ViewModel.CarAndServicesViewModel

@{
    ViewData["Title"] = "Index";
}
@{await Html.RenderPartialAsync("_CarDetailsInService");}
<br/>
<h4>Service Record</h4>
<div class="row form-border">
    @{await Html.RenderPartialAsync("_DisplayPastServices", Model);}
    <input type="button" class="btn btn-sm btn-primary" value="Ptint" onclick="window.print();" />
    <a asp-action="Index" asp-controller="Cars" asp-route-userId="@Model.UserId" class="btn btn-sm btn-success">Back to List</a>
</div>
```
```bash
vim Views/Services/Create.cshtml
```
```cs
@model SammysAuto.ViewModel.CarAndServicesViewModel

@{
    ViewData["Title"] = "Index";
}
@{await Html.RenderPartialAsync("_CarDetailsInService");}
<br/>
<h4>Service Record</h4>
<div class="row form-border">
    @{await Html.RenderPartialAsync("_DisplayPastServices", Model);}
    <input type="button" class="btn btn-sm btn-primary" value="Ptint" onclick="window.print();" />
    <a asp-action="Index" asp-controller="Cars" asp-route-userId="@Model.UserId" class="btn btn-sm btn-success">Back to List</a>
</div>
```








