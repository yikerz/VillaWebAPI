## Project Setup

1. Create new project `ASP.NET Core Web API` named `MagicVilla_VillaAPI` and solution named `MagicVilla` (check `Use Controllers`)
2. Delete `WeatherForecast.cs`, `WeatherForecastController.cs`

#### NuGet Installation

1. Install NuGet packages:
   - `Microsoft.EntityFramework.SqlServer`
   - `Microsoft.EntityFramework.Tools`

#### Model Creation (With DTO)

1. Add class `Models/Villa.cs` with props
   - Id: `int` with annotation `[Key]` (optional) and `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]`
   - Name: `string`
   - Details: `string`
   - Rate: `double`
   - Sqft: `int`
   - Occupancy: `int`
   - ImageUrl: `string`
   - Amenity: `string`
   - CreatedDate: `DateTime`
   - UpdatedDate: `DateTime`
2. Create new folder `Models/Dto` and new class `Models/Dto/VillaDTO.cs` with props
   - Id: `int`
   - Name: `string` with annotation `[Required]` and `[MaxLength(30)]`
   - Details: `string`
   - Rate: `double` with annotation `[Required]`
   - Sqft: `int`
   - Occupancy: `int`
   - ImageUrl: `string`
   - Amenity: `string`

## Database Setup

#### Database Connection And DbContext Setup

1. Create new folder `Data` and new class `Data/ApplicationDbContext.cs`

   - Extend base class: `DbContext`
   - Prop: `DbSet<Villa> Villas`
   - Constructor:

   ```cs
   public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
   {
   }
   ```

2. In `appsettings.json`, add `ConnectionStrings`

```json
"ConnectionStrings": {
  "DefaultSQLConnection": "Server=<serverName>;Database=<dbName>;TrustServerCertificate=True;Trusted_Connection=True;MultipleActiveResultSets=True"
}
```

3. In `Program.cs`, inject dependency for `DbContext`

```cs
builder.Services.AddDbContext<ApplicationDbContext>(option => {
  option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});
```

#### Database Migration

1. In PM console, run
   - `add-migration <migrationName>`
   - `update-database`

#### Data Seeding

1. In `ApplicationDbContext.cs`, add method `OnModelCreating(ModelBuilder builder)`

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
  builder.Entity<Villa>().HasData(
    new Villa() {...},
    ...
  )
}
```

2. Add migration and update database

## First End-Point

#### Default End-Point Without Path Variable

1. Add API controller `Controllers/VillaAPIController.cs`
2. For the class `VillaAPIController`
   - Extend base class: `ControllerBase`
   - Route annotation: `[Route("api/[controller]")]`
   - API annotation: `[ApiController]`
3. In the controller, create constructor
   - Input param: `ApplicationDbContext db`
4. In the controller, create GET method `GetVillas`
   - Request-type annotation: `[HttpGet]`
   - Response-type annotation: `[ProducesResponseType(StatusCodes.Status200OK)]`
   - Return type: `ActionResult<IEnumerable<Villa>>`
   - Return: `Ok(_db.Villas.ToList())`

#### Default End-Point Without Path Variable

0. Endpoint `/api/VillaApi` will call `GetVillas()`. Now, we want to make the endpoint with optional param `id`. If the endpoint is `/api/VillaApi/id`, the `Villa` with that `Id` will be returned.
1. Create new GET method in `VillaAPIController` named `GetVilla(int id)`
   - Request-type annotation: `[HttpGet("{id:int}"), Name="GetVilla")]` (expect to input `id`)
   - Response-type annotation: `[ProducesResponseType(...)]` (for `200`, `400`, `404`)
   - Return type: `ActionResult<Villa>`
   - Logic:
     - If `id==0`, return `BadRequest()`
     - Use `FirstOrDefault` to search for `villa`. If `null`, return `NotFound()`
     - Else, return `Ok(villa)`

## CRUD Functionalities

#### Create (POST Request)

1. Create Action method `CreateVilla([FromBody] VillaDTO villaDTO)`
   - Request-type annotation: `[HttpPost]`
   - Response-type annotation: `[ProducesResponseType(...)]`
   - Return type: `ActionResult<Villa>`
   - Logic:
     - If `villaDTO == null`, return `BadRequest(villaDTO)`
     - If `villaDTO.Id > 0`, return `StatusCode(StatusCodes.Status500InternalServerError)` (body in POST request should have default `Id=0`)
     - Map `villaDTO` to `villa` (create new `Villa` using `villaDTO`)
     - Add new DTO: `_db.Villas.Add(villa)`
     - Save Db change: `_db.SaveChanges()`
     - Return `CreateAtRoute("GetVilla", new {id=villa.Id}, villa)`

#### ModelState Validations

1. (Optional) Add model-state validation to action method `CreateVilla`
   - If not `ModelState.IsValid`, return `BadRequest(ModelState)` (see concept below)

_Concept_:

- By default, the validation from annotation in the model class is supported if `[ApiController]` annotation is added in the controller class, which is what we did. If `[ApiController]` is removed, the validation will not be supported unless manual validation is added in the request method.

###### Custom ModelState Validation

1. Add another validation to `CreateVilla`:
   - If `villaDTO` has the same name, run `ModelState.AddModelError("<uniqueKeyName>", "Villa already Exists!")` to set `ModelState` then return `BadRequest(ModelState)`

#### Delete (DELETE Request)

1. Create Action `DeleteVilla(int id)`
   - Request-type annotation: `[HttpDelete("{id:int}", Name="DeleteVilla")]`
   - Response-type annotation: `[ProducesResponseType(...)]`
   - Return type: `IActionResult` (`IActionResult` cannot define return type of object, where we do not need in `delete` request)
   - Logic:
     - If `id==0`, return `BadRequest`
     - If `villaFound == null`, return `NotFound()`
     - Remove `villaFound`: `_db.Villas.Remove(villaFound)`
     - Save Db change: `_db.SaveChanges()`
     - Return `NoContent()`

#### Update

###### PUT Request

1. Create Action `UpdateVilla(int id, [FromBody] VillaDTO villaDTO)`
   - Request-type annotation: `[HttpPut("{id:int}", Name="UpdateVilla")]`
   - Response-type annotation: `[ProducesResponseType(...)]`
   - Return type: `IActionResult`
   - Logic:
     - If `villaDTO == null || id != villaDTO.Id`, return `BadRequest`
     - Find `villa` by `id` with NoTracking, `_db.Villas.AsNoTracking().FirstOrDefault(...)`, if `null`, return `NotFound`
     - Map `villaDTO` to `villa` (create new `Villa` using `villaDTO`)
     - Update database: `_db.Villas.Update(villa)`
     - Save Db change: `_db.SaveChanges()`
     - Return `NoContent()`

###### PATCH Request (Optional)

1. Install NuGet packages
   - `Microsoft.AspNetCore.JsonPatch`
   - `Microsoft.AspNetCore.Mvc.NewtonsoftJson`
2. In `Program.cs`, change the line from `builder.Services.AddControllers()` to `builder.Services.AddControllers().AddNewtonsoftJson()`
3. Create Action `UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchDTO)`
   - Request-type annotation: `[HttpPatch("{id:int}", Name="UpdatePartialVilla")]`
   - Response-type annotation: `[ProducesResponseType(...)]`
   - Return type: `IActionResult`
   - Logic:
     - If `patchDTO == null || id == 0`, return `BadRequest`
     - Find `villa` by `id` with NoTracking, `_db.Villas.AsNoTracking().FirstOrDefault(...)`, if `null`, return `NotFound`
     - Create new `villaDTO` using `villa` field values (Map from `villa` to `villaDTO`)
     - Update `villaDTO` with `patchDTO.ApplyTo(villaDTO, ModelState)`
     - Create new `villa` using `villaDTO` field values (Map from `villaDTO` back to `villa`)
     - Update database: `_db.Villas.Update(villa)`
     - Save Db change: `_db.SaveChanges()`
     - Check `ModelState.IsValid`, if not, return `BadRequest(ModelState)`
     - Return `NoContent()`

## Dependency Injection

#### Logger Injection

1. Add param to constructor for `VillaAPIController`
   - Param: `ILogger<VillaAPIController> logger`
2. Log errors for `GetVilla` using `_logger.LogError("<ErrorMessage>")`

#### Keep Log In File (Optional)

1. Install NuGet packages
   - `Serilog.AspNetCore`
   - `Serilog.Sinks.File`
2. Add below line to `Program.cs` after builder is instantiated (only log when level is higher or equal to `debug`, new file is created per day)

```cs
Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
  .WriteTo.File("log/villaLogs.txt", rollingInterval:RollingInterval.Day).CreateLogger();
builder.Host.UseSerilog();
```

#### Custom Logging (Optional)

1. Create new folder `Logging`, new interface `Logging/ILogging.cs`, new class `Logging/Logging.cs`
2. In `ILogging.cs`, define method:
   - `public void Log(string message, string type)`
3. In `Logging.cs`, extended from `ILogging` and implement method `Log`

```cs
public void Log(string message, string type)
{
  if (type == "error")
  {
    Console.WriteLine("ERROR - " + message);
  }
  else
  {
    Console.WriteLine(message);
  }
}
```

4. In `VillaAPIController` constructor
   - Change the param type from `ILogging<VillaAPIController>` to `ILogging`
   - Change the log method from `_logger.LogError` to `_Logger.Log("<message>", "<type>")`
5. In `Program.cs`, inject dependency add `builder.Services.AddSingleton<ILogging, Logging>()` before `app` is instantiated

## Async Methods

1. Turn all methods in the controller to `async`
   - Add `async` keyword in method definition
   - Wrap return type with `Task<...>`
   - Add `await` and `Async` to the dbContext methods (eg. `ToListAsync`, `AddAsync`)

## DTO Models

#### DTO For POST Request

1. Copy and paste `Models/Dto/VillaDTO.cs` to `Models/Dto/VillaCreateDTO.cs`
2. Change input param of `CreateVilla` to `VillaCreateDTO` (validation for `id>0` is not needed anymore)

#### DTO For PUT Request

1. Copy and paste `Models/Dto/VillaDTO.cs` to `Models/Dto/VillaUpdateDTO.cs`
   - Add annotation `[Required]` to `Id`, `Rate`, `Occupancy`, `Sqft`, `ImageUrl`
2. Change input param of `UpdateVilla` to `VillaUpdateDTO`

## AutoMapper

#### Setup

1. Install NuGet packages
   - `AutoMapper`
   - `AutoMapper.Extensions.Microsoft.DependencyInjection`
2. Create class `Project/MappingConfig.cs`
   - Extend base class: `Profile`
   - Constructor:
   ```cs
   public MappingConfig()
   {
    CreateMap<Villa, VillaDTO>().ReverseMap();
    CreateMap<Villa, VillaCreateDTO>().ReverseMap();
    CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
   }
   ```
3. Inject dependency to `Program.cs`: `builder.Services.AddAutoMapper(typeof(MappingConfig))`

#### AutoMapper Usage

1. In `VillaAPIController` constructor, add param `IMapper mapper`
2. Use `var model = _mapper.Map<Villa>(DTO)` to map DTO object to `Villa` for
   - `CreateVilla`
   - `UpdateVilla`

## Repository

#### Villa Repository And Interface

1. Create two new folders `Repository`, `Repository/IRepository` and one interface `Repository/IRepository/IVillaRepository.cs` with method definitions:

   - `Task<List<Villa>> GetAllAsync(Expression<Func<Villa, bool>>? filter=null)` (replace `ToList`)
   - `Task<Villa> GetAsync(Expression<Func<Villa, bool>> filter=null, bool tracked=true)` (replace `AsNoTracking().FirstOrDefault`)
   - `Task CreateAsync(Villa entity)` (replace `Add`)
   - `Task RemoveAsync(Villa entity)` (replace `Remove`)
   - `Task SaveAsync()` (replace `Save`)
   - `Task UpdateAsync(Villa entity)` (replace `Update`)

2. Create repository `Repository/VillaRepository.cs`
   - Extend base interface: `IVillaRepository` (implement all methods)
   - Constructor param: `ApplicationDbContext`
   - For `GetAsync` as an example:
   ```cs
   public async Task<Villa> GetAsync(Expression<Func<Villa, bool>> filter=null, bool tracked=true)
   {
    IQueryable<Villa> query = _db.Villas; // define object to run query
    if (!tracked)
    {
      query = query.AsNoTracking();
    }
    if (filter != null)
    {
      query = query.Where(filter);
    }
    return await query.FirstOrDefaultAsync();
   }
   ```
3. Inject dependency to `Program.cs`: `builder.Services.AddScoped<IVillaRepository, VillaRepository>()`
4. Change param `ApplicationDbContext` in `VillaAPIController` constructor to `IVillaRepository`
5. Replace DbContext methods to repository methods in all request Actions (eg. `_db.SaveAsync` to `repo.Save`)

#### Generic Repository And Interface (Optional)

1. Create generic interface `Repository/IRepository/IRepository.cs` (use `IRepository<T> where T : class` to declare generic interface)
2. Copy the all method definitions (except `UpdateAsync`) from `IVillaRepository` to `IRepository` and replace `Villa` with `T` (only keep `UpdateAsync` in `IVillaRepository`)
3. Create generic repository `Repository/Repository.cs`
   - Extend base interface: `Repository<T> : IRepository<T> where T : class`
   - Copy and paste everything from `VillaRepository`
   - Setup constructor as below
   ```cs
   public class Repository<T> : IRepository<T> where T : class
   {
    private readonly ApplicationDbContext _db;
    internal DbSet<T> dbSet;
    public Repository(ApplicationDbContext db)
    {
      _db = db;
      this.dbSet = _db.Set<T>(); // for replacing `_db.Villas`
    }
    ...
   }
   ```
   - Replace all `Villa` by `T`

#### Apply Generic Repository Usage

1. Extend interface for `IVillaRepository : IRepository<Villa>`
2. Extend class for `VillaRepository : Repository<Villa>, IVillaRepository` and the constructor should extend `base(db)`
3. Remove all methods but `UpdateAsync` in `VillaRepository` and `UpdateAsync` should look like

```cs
public async Task UpdateAsync(Villa entity)
{
  entity.UpdatedDate = DateTime.Now;
  _db.Villas.Update(entity);
  await _db.SaveChangesAsync();
}
```

## Standard API Response

1. Create new class `Models/APIResponse.cs` with prop
   - StatusCode: `HttpStatusCode`
   - IsSuccess: `bool` with default `true`
   - ErrorMessages: `List<string>`
   - Result: `object` (store return object, eg. `villa`)
2. In `VillaAPIController`, declare variable `protected APIResponse _response` and instantiate in the constructor
3. In `GetVillas()`
   - Replace the return type from `IEnumerable<Villa>` to `APIResponse`
   - Set `_response.Result` to `villas`
   - Set `_response.StatusCode` to `HttpStatusCode.OK`
   - Return `Ok(_response)`
   - Wrap everything with `try` block, in `catch (Exception ex)` block
     - Set `_response.IsSuccess=false`
     - Set `_response.ErrorMessages` to `new List<string>() { ex.ToString() }`
   - Return `_response` outside `try...catch...` blocks
4. Do the similar thing to all request actions in `VillaAPIController`
