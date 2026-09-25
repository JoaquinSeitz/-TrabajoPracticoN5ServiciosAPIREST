# 🚀 API RESTful - TP5 (Gestión de Inventario)

API para el control de stock, usuarios y recursos, desarrollada en **.NET 8** y **SQL Server**. Proyecto académico para la Tecnicatura Superior en Desarrollo de Software (ITES).

**Integrantes:** Joaquín Gaspar Seitz | Alex Falkenstein

---

## 🛠️ Stack Tecnológico

* **Backend:** ASP.NET Core Web API (C# 12)
* **Datos:** Entity Framework Core (Code-First) + SQL Server Remoto
* **Seguridad:** JWT (JSON Web Tokens) + CORS Permisivo
* **Monitoreo:** Serilog (Consola y retención de logs en disco)
* **Despliegue:** MonsterASP

---

## 📋 Arquitectura y Patrones

* **Data Transfer Objects (DTO):** Aislamiento de las entidades de dominio para evitar exposición de datos sensibles.
* **Borrado Lógico (Soft Delete):** Uso del campo `Activo` en todas las entidades principales para preservar la integridad referencial.
* **Seguridad de Credenciales:** Implementación de `.NET Secret Manager` para excluir del control de versiones las firmas JWT y cadenas de conexión.
* **Manejo de Excepciones:** Bloques `try/catch` estandarizados en controladores para garantizar respuestas HTTP 500 estructuradas ante fallos del servidor.
---

## ⚙️ Ejecución Local (Evaluadores)

El repositorio no contiene contraseñas por seguridad. Para compilar y ejecutar en un entorno local, inicialice los secretos desde la terminal en la raíz del proyecto

---

## 🏗️ Deuda Técnica Aceptada (ADR)

* **Normalización:** Se omitió la 3FN por definición de alcance académico; la redundancia se mitiga con validaciones estrictas en los endpoints.
* **Acceso a Datos:** `ApplicationDbContext` opera temporalmente de forma directa en los controladores. La refactorización al Patrón Repositorio está planificada como próxima iteración arquitectónica.

---

## 🔗 Endpoints Principales

| Método | Endpoint | Acción | Acceso |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/Usuarios/login` | Autenticación y emisión de token JWT | 🔓 Público |
| **GET** | `/api/Productos` | Listado paginado del inventario | 🔐 Token JWT |
| **POST** | `/api/IngresoProductos`| Registro de entrada y recálculo de stock | 🔐 Token JWT |
| **PUT** | `/api/Clientes/{id}` | Actualización de datos de contacto | 🔐 Token JWT |
| **DELETE**| `/api/Proveedores/{id}`| Baja lógica (desactivación de cuenta) | 🔐 Token JWT |

---

## 🌐 Producción

**URL de la API:** [https://tp5-joaco-alex.runasp.net](https://tp5-joaco-alex.runasp.net)
