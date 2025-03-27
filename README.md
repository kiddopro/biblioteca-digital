# Biblioteca Digital API con PostgreSQL

API para la gestión de una biblioteca digital desarrollada con C# .NET Core, Entity Framework Core y PostgreSQL como base de datos.

## Requisitos

- .NET 9.0 SDK o superior
- PostgresSQL [https://www.postgresql.org/download/](https://www.postgresql.org/download/)
- Postman o similar.

## Configuración

1. **Clonar el repositorio:**
   - git clone [https://github.com/kiddopro/biblioteca-digital.git](https://github.com/kiddopro/biblioteca-digital.git)
   - `cd biblioteca-digital`
2. **Configurar PostgreSQL local:**
   - Crear una base de datos llamada `BibliotecaDigital`.
3. **Configurar la cadena de conexión:**
   - Abrir el archivo `appsettings.json`
   - Actualizar la cadena de conexión `DefaultConnection` con tus credenciales locales: `"DefaultConnection": "Host=localhost;Port=5432;Database=BibliotecaDigital;Username=postgres;Password=tu_contraseña"`
4. **Ejecutar las migraciones para crear la base de datos:**
   - `dotnet ef migrations add InitialCreate`
   - `dotnet ef database update`
5. **Ejecutar la aplicación:**
   - `dotnet run`




## Mockup de datos
*Esto es en caso de que quiera probar algunos enpoint GET antes de pasar a los POST.*
```sql
-- Insert sample genres
INSERT INTO "Generos" ("Nombre", "Descripcion") VALUES 
('Ciencia Ficción', 'Libros que exploran conceptos futuristas y científicos'),
('Fantasía', 'Libros con elementos mágicos y mundos imaginarios'),
('Misterio', 'Libros que involucran la resolución de un misterio o crimen');

-- Insert sample authors
INSERT INTO "Autores" ("Nombre", "Nacionalidad", "FechaNacimiento") VALUES 
('Isaac Asimov', 'Estadounidense', '1920-01-02'),
('J.K. Rowling', 'Británica', '1965-07-31'),
('Agatha Christie', 'Británica', '1890-09-15');

-- Insert sample books
INSERT INTO "Libros" ("Titulo", "Resumen", "AnioPublicacion", "Imagen", "ISBN", "Stock", "AutorId", "GeneroId") VALUES 
('Fundación', 'Primera parte de la trilogía Fundación', 1951, 'https://example.com/fundacion.jpg', '9788497596695', 10, 1, 1),
('Harry Potter y la piedra filosofal', 'Primer libro de la saga Harry Potter', 1997, 'https://example.com/harrypotter.jpg', '9788478884452', 15, 2, 2),
('Asesinato en el Orient Express', 'Un famoso detective resuelve un misterioso asesinato en un tren', 1934, 'https://example.com/orientexpress.jpg', '9788427298088', 8, 3, 3);

-- Insert a test user
INSERT INTO "Usuarios" ("Nombre", "Email", "Password", "Estado") VALUES 
('Admin', 'admin@example.com', 'password123', 0);
```


## Endpoints de la API

Por defecto la URL donde hacer las llamadas es `http://localhost:5184`

### Autenticación
- `POST /api/auth/register` - Registro de usuarios
- `POST /api/auth/login` - Inicio de sesión (devuelve token JWT)

### Libros
- `GET /api/books` - Listado de libros
- `GET /api/books/{id}` - Detalle de un libro
- `POST /api/books` - Crear libro (requiere autenticación)
- `PUT /api/books/{id}` - Actualizar libro (requiere autenticación)
- `DELETE /api/books/{id}` - Eliminar libro (requiere autenticación)

### Búsqueda
- `GET /api/books?title=nombre` - Búsqueda por título
- `GET /api/books?isbn=1234567890` - Búsqueda por ISBN
- `GET /api/books?author=id` - Filtrado por autor
- `GET /api/books?genre=id` - Filtrado por género

### Autores
- `GET /api/authors` - Listado de autores con cantidad de libros publicados

