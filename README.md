# ArtemisBank
# 🏦 Artemis Banking

Plataforma de banca en línea desarrollada con **ASP.NET Core MVC** y **ASP.NET Core Web API**, siguiendo la **arquitectura Onion**. Permite gestionar cuentas de ahorro, préstamos, tarjetas de crédito y transferencias, con acceso basado en roles para administradores, cajeros y clientes.

> Proyecto académico desarrollado en el Instituto Tecnológico de Las Américas (ITLA).

---

## ✨ Funcionalidades

### 👤 Administrador
- Dashboard con indicadores del sistema: transacciones, pagos, clientes activos/inactivos, productos asignados y deuda promedio.
- Gestión de usuarios (administradores, cajeros y clientes) con activación por correo.
- Asignación de **préstamos** con generación automática de la **tabla de amortización** (sistema francés de cuota fija).
- Evaluación de **riesgo del cliente** antes de aprobar un préstamo, comparando su deuda con el promedio del sistema.
- Recalculo de cuotas futuras al cambiar la tasa de interés de un préstamo.
- Emisión, edición de límite y cancelación de **tarjetas de crédito** (CVC cifrado con SHA-256).
- Gestión de **cuentas de ahorro** principales y secundarias.

### 💳 Cliente
- Consulta de sus productos financieros: cuentas, préstamos y tarjetas.
- Transferencias express, a beneficiarios y entre cuentas propias.
- Pagos a tarjetas de crédito y préstamos.
- Avances de efectivo desde tarjetas de crédito.
- Gestión de beneficiarios.

### 🧾 Cajero
- Depósitos y retiros.
- Pagos a tarjetas y préstamos.
- Transacciones a cuentas de terceros.
- Indicadores de sus operaciones del día.

### ⚙️ Procesos automáticos
- Tarea diaria con **Hangfire** que detecta y marca las cuotas de préstamo atrasadas.
- Notificaciones por correo en cada operación importante.

### 🔌 Web API
- API REST protegida con **JWT** y roles (Administrador y Comercio).
- Endpoints para usuarios, préstamos, tarjetas, cuentas de ahorro y comercios.
- **Hermes Pay:** procesador de pagos que permite a los comercios cobrar con las tarjetas de crédito del sistema.
- Documentación interactiva con **Swagger**.

---

## 🛠️ Tecnologías

| Área | Tecnologías |
|---|---|
| Backend | C#, ASP.NET Core MVC, ASP.NET Core Web API |
| Base de datos | SQL Server, Entity Framework Core (Code First) |
| Seguridad | ASP.NET Identity, JWT |
| Tareas en segundo plano | Hangfire |
| Mapeo | AutoMapper |
| Documentación API | Swagger |
| Interfaz | Bootstrap |

---

## 🏗️ Arquitectura

El proyecto sigue la **arquitectura Onion**, separando responsabilidades en capas:

```
Source
├── Core
│   ├── ArtemisBank.Core.Domain          → Entidades y reglas del dominio
│   └── ArtemisBank.Core.Application     → Lógica de negocio, servicios, DTOs e interfaces
├── Infrastructure
│   ├── ArtemisBank.Infrastructure.Identity     → Autenticación, roles y JWT
│   ├── ArtemisBank.Infrastructure.Persistence  → DbContext, repositorios y migraciones
│   └── ArtemisBank.Infrastructure.Shared       → Servicio de correo
└── Presentation
    ├── ArtemisBank.WebApp   → Aplicación web MVC
    └── ArtemisBank.WebApi   → API REST
```

Además se aplican **repositorios y servicios genéricos**, **DTOs** y **ViewModels con validaciones**.

---

## 🚀 Cómo ejecutarlo

### Requisitos
- [.NET SDK](https://dotnet.microsoft.com/download) (versión del proyecto)
- SQL Server
- Visual Studio 2022 (recomendado)

### Pasos

1. **Clona el repositorio**
   ```bash
   git clone https://github.com/cesarvictorinoeip-source/ArtemisBank.git
   ```

2. **Configura el `appsettings.json`** de `ArtemisBank.WebApp` y `ArtemisBank.WebApi`:
   - `ConnectionStrings:DefaultConnection` → tu servidor de SQL Server.
   - `MailSettings` → tu correo y contraseña de aplicación para el envío de notificaciones.

3. **Aplica las migraciones** desde la Consola del Administrador de paquetes, seleccionando como proyecto predeterminado las capas de **Persistence** e **Identity**:
   ```powershell
   Update-Database
   ```

4. **Ejecuta el proyecto** `ArtemisBank.WebApp` (o `ArtemisBank.WebApi` para la API).

Al iniciar, el sistema crea automáticamente los roles y usuarios por defecto (administrador, cajero y cliente).

### Rutas útiles
- **Swagger:** `/swagger` (en la Web API)
- **Panel de Hangfire:** `/hangfire` (en la Web App, requiere autorización)

---

## 👨‍💻 Autor

**César Tomás Victorino**
Estudiante de Desarrollo de Software — ITLA

- GitHub: [@cesarvictorinoeip-source](https://github.com/cesarvictorinoeip-source)
