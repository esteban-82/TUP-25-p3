using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios CORS para permitir solicitudes desde el cliente
builder.Services.AddCors(options => {
    options.AddPolicy("AllowClientApp", policy => {
        policy.WithOrigins("http://localhost:5184", "https://localhost:7221")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Agregar controladores si es necesario
builder.Services.AddDbContext<TiendaContext>(options =>
    options.UseSqlite("Data Source=tienda.db"));

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowClientApp");
// para probar la conexión a la base de datos
app.MapGet("/", () => "API De mi Tienda Fender - Funciona correctamente.");
// productos
app.MapGet("/api/productos", async (TiendaContext db) =>
    await db.Productos.AsNoTracking().ToListAsync()
);
 
app.MapGet("api/productos/buscar", async(string term, TiendaContext db) => {
    return await db.Productos
        .AsNoTracking()
        .Where(p => p.Nombre.Contains(term) || p.Descripcion.Contains(term))
        .ToListAsync();
});
// carrito 


app.MapPost("/api/carritos", () =>
{
    var carritoId = Guid.NewGuid().ToString();
    return Results.Created($"/api/carritos/{carritoId}", new { CarritoId = carritoId });
});

app.MapGet("/api/carritos/{carritoId}", async (string carritoId, TiendaContext db) =>
{
    var carrito = await db.Carritos
    .Include(c => c.Items)
    .ThenInclude(i => i.Producto)
    .FirstOrDefaultAsync(c => c.Id == carritoId);
    if (carrito == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(carrito);
});

app.MapPut("/api/carritos/{carritoId}/{productoId}", async (string carritoId, int productoId, int cantidad, TiendaContext db) =>
{
    var producto = await db.Productos.FindAsync(productoId);
    if (cantidad <= 0) return Results.BadRequest("La cantidad debe ser mayor a cero.");
    if (producto == null) return Results.NotFound("Producto no encontrado.");
    if (producto.Stock < cantidad) return Results.BadRequest("Stock insuficiente.");

    // Buscar el carrito en la base de datos
    var carrito = await db.Carritos
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c => c.Id == carritoId);

    // Si el carrito no existe, crearlo
    if (carrito == null)
    {
        carrito = new Carrito { Id = carritoId };
        db.Carritos.Add(carrito);
        await db.SaveChangesAsync();
    }

    // Manejo de los productos en el carrito
    var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
    if (item != null)
    {
        item.Cantidad += cantidad;
    }
    else
    {
        carrito.Items.Add(new ItemCarrito { ProductoId = productoId, Cantidad = cantidad, PrecioUnitario = producto.Precio });
    }

    await db.SaveChangesAsync();
    return Results.Ok();
});
app.Run();

public class TiendaContext : DbContext
{
    public TiendaContext(DbContextOptions<TiendaContext> options) : base(options) { }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Carrito> Carritos { get; set; }
}

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}

public class Carrito
{
    public string Id { get; set; }
    public List<ItemCarrito> Items { get; set; } = new List<ItemCarrito>();
}

public class ItemCarrito
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
