using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddPolicy("AllowClientApp", policy => {
        policy.WithOrigins("http://localhost:5184", "https://localhost:7221", "http://localhost:5177")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<TiendaContext>(options =>
    options.UseSqlite("Data Source=tienda.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowClientApp");

app.MapGet("/", () => "API De mi Tienda Fender - Funciona correctamente.");

app.MapGet("/api/productos", async (TiendaContext db) =>
    await db.Productos.AsNoTracking().ToListAsync()
);

app.MapGet("api/productos/buscar", async (string term, TiendaContext db) =>
{
    return await db.Productos
        .AsNoTracking()
        .Where(p => p.Nombre.Contains(term) || p.Descripcion.Contains(term))
        .ToListAsync();
});

app.MapPost("/api/carritos", async (TiendaContext db) =>
{
    var carritoId = Guid.NewGuid().ToString();
    
    var nuevoCarrito = new Carrito { Id = carritoId };

    db.Carritos.Add(nuevoCarrito);
    await db.SaveChangesAsync(); 

    return Results.Created($"/api/carritos/{carritoId}", nuevoCarrito);
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

    var carrito = await db.Carritos
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c => c.Id == carritoId);

    if (carrito == null)
    {
        carrito = new Carrito { Id = carritoId };
        db.Carritos.Add(carrito);
        await db.SaveChangesAsync();
    }

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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "American Luxe Telecaster", Descripcion = "Guitarra Telecaster de lujo con acabado premium.", Precio = 2500, Stock = 5, ImagenUrl = "/imagenes/american-luxe-telecaster.png" },
            new Producto { Id = 2, Nombre = "American Telecaster Blanca", Descripcion = "Telecaster con cuerpo blanco y sonido potente.", Precio = 2200, Stock = 4, ImagenUrl = "/imagenes/american-telecaster-blanca.png" },
            new Producto { Id = 3, Nombre = "Stratocaster Professional II", Descripcion = "Stratocaster ideal para músicos profesionales.", Precio = 2300, Stock = 6, ImagenUrl = "/imagenes/stratocaster-professional-ii.png" },
            new Producto { Id = 4, Nombre = "Vintage Telecaster", Descripcion = "Modelo Telecaster con estética y tono vintage.", Precio = 2100, Stock = 3, ImagenUrl = "/imagenes/vintage-telecaster.png" }
        );
    }
}

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string ImagenUrl { get; set; }
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
