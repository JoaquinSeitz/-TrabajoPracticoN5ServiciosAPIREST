# 🚀 Trabajo Práctico N° 5: Servicios API REST (ASP.NET Core & SQL Server)

API RESTful desarrollada en **ASP.NET Core** y **Entity Framework Core**, conectada a una base de datos relacional en **SQL Server** y desplegada en un servidor cloud (MonsterASP). Este sistema forma parte del proyecto académico de la Tecnicatura Superior en Desarrollo de Software (ITES).

## 👥 Integrantes
* **Seitz, Joaquín Gaspar**
* **Falkenstein, Alex**

---

## 🛠️ Tecnologías y Herramientas Utilizadas
* **Backend:** .NET 8 / ASP.NET Core Web API
* **ORM:** Entity Framework Core
* **Base de Datos:** Microsoft SQL Server (Remota)
* **Seguridad:** Autenticación basada en JSON Web Tokens (JWT)
* **Control de Versiones:** Git y GitHub
* **Despliegue / Hosting:** MonsterASP (`https://tp5-joaco-alex.runasp.net`)
* **Pruebas de Endpoints:** Postman

---

## 📋 Arquitectura y Funcionalidades
* **CRUD Completo:** Gestión de Productos, Categorías, Clientes y Proveedores.
* **Seguridad por Roles / Tokens:** Endpoints protegidos mediante cabeceras de autorización `Bearer Token`.
* **Borrado Lógico (Soft Delete):** Implementación del campo `Activo` en las entidades principales para preservar la integridad histórica de los registros en la base de datos sin eliminarlos físicamente.
* **Control de Movimientos:** Módulos de registro para entradas (Ingreso de productos y control de stock) y salidas de mercadería.

---

## 🔗 Endpoints Principales de la API

| Método | Endpoint | Descripción | Autorización |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/Usuarios/login` | Autenticación de usuario y generación de Token JWT. | Público |
| **GET** | `/api/Productos` | Obtiene el listado completo de productos del inventario. | Requiere Token |
| **POST** | `/api/Categorias` | Alta de una nueva categoría de productos. | Requiere Token |
| **PUT** | `/api/Clientes/{id}` | Modificación de los datos de contacto de un cliente. | Requiere Token |
| **DELETE**| `/api/Proveedores/{id}` | Baja lógica (Soft Delete) de un proveedor específico. | Requiere Token |

---

## 🌐 Enlace de Despliegue (Producción)
La API se encuentra en línea y operativa en el siguiente servidor seguro:
👉 **[https://tp5-joaco-alex.runasp.net]**
