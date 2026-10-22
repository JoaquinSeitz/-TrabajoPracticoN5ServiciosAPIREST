# 🚀 API RESTful - TP5 (Gestión de Inventario y Ventas)

API para el control de stock, facturación, usuarios y recursos, desarrollada en **.NET 8** y **SQL Server**. Proyecto académico para la Tecnicatura Superior en Desarrollo de Software (ITES). Diseñada con una arquitectura robusta para ser consumida desde aplicaciones web (Blazor) y móviles (.NET MAUI).

**Integrantes:** Joaquín Gaspar Seitz | Alex Falkenstein

---

## 🛠️ Stack Tecnológico y Librerías

- **Backend:** ASP.NET Core Web API (C# 12)
- **Datos:** Entity Framework Core (Code-First) + SQL Server Remoto
- **Seguridad:** JWT (JSON Web Tokens) + Roles + CORS Permisivo
- **Reportes:** QuestPDF (Generación de comprobantes de venta)
- **Comunicaciones:** Servicio SMTP integrado (Envío automático de emails)
- **Monitoreo:** Serilog (Consola y retención de logs en disco)
- **Despliegue:** MonsterASP

---

## 📋 Arquitectura y Funcionalidades Clave

- **Base de Datos 3FN y Roles:** Estructura relacional completamente normalizada y control de acceso seguro basado en roles de usuario.
- **Gestión de Archivos Estáticos:** Endpoint profesional para la subida de imágenes físicas (`multipart/form-data`) almacenadas dinámicamente en `wwwroot/uploads`.
- **Generación y Envío de Comprobantes:** Creación de tickets de venta en PDF directo en memoria y despacho automatizado al correo del cliente tras cada compra.
- **Data Transfer Objects (DTO):** Aislamiento de las entidades de dominio para evitar exposición de datos sensibles.
- **Borrado Lógico (Soft Delete):** Uso del campo `Activo` en todas las entidades principales para preservar la integridad referencial y el historial de ventas.
- **Manejo de Excepciones:** Bloques `try/catch` estandarizados en controladores para garantizar respuestas HTTP 500 estructuradas y evitar caídas del servidor.

---

## ⚙️ Ejecución Local (Evaluadores)

El repositorio no contiene contraseñas por seguridad. Para compilar y ejecutar en un entorno local:
1. Inicialice los secretos de la base de datos y la firma JWT desde la terminal usando `.NET Secret Manager`.
2. Configure las credenciales de correo electrónico en `appsettings.json` (sección `"Smtp"`).
3. Asegúrese de compilar la solución para que el entorno genere automáticamente la carpeta `wwwroot` de archivos estáticos.

---

## 🔗 Endpoints Destacados

| Método | Endpoint | Acción | Acceso |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/Usuarios/login` | Autenticación y emisión de token JWT | 🔓 Público |
| **GET** | `/api/Productos` | Listado paginado del inventario | 🔓 Público |
| **POST** | `/api/Productos/{id}/imagen`| Subida de foto real (Carga en `wwwroot`) | 🔐 Admin / Operador |
| **POST** | `/api/SalidaProductos`| Registro de venta, descuento de stock y envío de Ticket PDF por email | 🔐 Admin / Operador |
| **DELETE**| `/api/Proveedores/{id}`| Baja lógica (desactivación de cuenta) | 🔐 Admin |

---

## 🌐 Producción

**URL de la API:** [https://tp5-joaco-alex.runasp.net](https://tp5-joaco-alex.runasp.net)
