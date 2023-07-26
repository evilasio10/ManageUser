using ManageUser.Model;
using ManageUser.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();


app.MapGet("/usuarios", [Authorize(Roles = "Atendimento,Admin")] (AppDbContext context) =>
{
    var listUsers = context.Usuario.ToList();
    return listUsers.Count > 0 ? Results.Ok(listUsers) : Results.NotFound();
});

app.MapGet("/usuarios/{id}", [Authorize(Roles = "Atendimento,Admin")] (AppDbContext context, int id) =>
{
    var user = context.Usuario.Where(p => p.ide_usuario == id).FirstOrDefault();
    return user is null ? Results.NotFound() : Results.Ok(user);
});

app.MapPost("/usuarios", [Authorize(Roles = "Atendimento,Admin")] (AppDbContext context, [FromBody] Usuario _us) =>
{
    context.Usuario.Add(_us);
    context.SaveChanges();
    return Results.Ok(_us);
});

app.MapPut("/usuarios/{id}", [Authorize(Roles = "Atendimento,Admin")] (AppDbContext context, int id, [FromBody] Usuario _us) =>
{
    if (_us == null || id != _us.ide_usuario) return Results.BadRequest();
    var usuario = context.Usuario.Find(id);
    if (usuario == null) return Results.NotFound();

    usuario.Email = _us.Email;
    usuario.UserName = _us.UserName;
    usuario.Role = _us.Role;

    context.Usuario.Update(usuario);
    context.SaveChanges();
    return Results.Ok();
});

app.MapDelete("/usuarios/{id}", [Authorize(Roles = "Admin")] (AppDbContext context, int id) =>
{
    var user = context.Usuario.Where(p => p.ide_usuario == id).FirstOrDefault();
    if (user != null)
    {
        context.Remove(user);
        context.SaveChanges();
    }
    return user != null ? Results.Ok() : Results.NotFound();
});

app.UseSwaggerUI();
app.Run();

