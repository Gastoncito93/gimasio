# 🏋️‍♂️ Sistema de Gestión Integral para Gimnasios

Sistema web moderno y de alto rendimiento para la administración integral de gimnasios, clubes deportivos y centros de entrenamiento. Diseñado con una arquitectura desacoplada basada en **APIs RESTful** construidas con **ASP.NET Core Web API** y una interfaz dinámica e interactiva desarrollada en **Vue 3**.

---

## 🚀 Tecnologías Utilizadas

### **Backend**
* **Framework:** .NET 8 (ASP.NET Core Web API)
* **ORM:** Entity Framework Core 8
* **Base de Datos:** MySQL / MariaDB
* **Seguridad & Auth:** JWT (JSON Web Tokens) & BCrypt Password Hashing
* **Documentación de APIs:** Swagger UI (OpenAPI)

### **Frontend**
* **Framework:** Vue 3 (Composition API `<script setup>`)
* **Bundler:** Vite
* **Enrutamiento:** Vue Router 4
* **Cliente HTTP:** Axios (con Interceptores automáticos de Token JWT)
* **Estilos:** Vanilla CSS con variables de diseño, badges dinámicos y soporte de tema oscuro.

---

## 🌟 Funcionalidades Principales

1. **📊 Dashboard General de Control:**
   * Métricas en tiempo real: Total de alumnos, coaches activos, planes vigentes y recaudación del mes.
   * Gráficos e indicadores de rendimiento operativo.

2. **👥 Gestión Unificada de Alumnos y Coaches (`/socios`):**
   * **Pestaña Alumnos:** Alta, baja, edición, filtrado por actividad/plan y asignación rápida de instructores.
   * **Pestaña Coaches:** Control de equipo técnico, cupos de alumnos (`X/20`), disciplina asignada y gestión de cuentas.

3. **💳 Módulo de Cuotas y Pagos (`/cuotas`):**
   * **Filtros por Período/Mes:** Consulta e historial de recaudación mensual (`202607`, `202606`, etc.).
   * **Resumen KPI Financiero:** Cálculo automático de monto total recaudado, cuotas cobradas y saldos pendientes.
   * Registro rápido de cobros con prorrateo de fechas y emisión de estados (`Pagada`, `Pendiente`, `Anulada`).

4. **🤸‍♂️ Actividades y Disciplinas (`/actividades`):**
   * Catálogo de disciplinas (Musculación, Crossfit, Funcional, Yoga, etc.) con asignación de instructores.

5. **🏷️ Planes y Membresías (`/planes`):**
   * Configuración de membresías mensuales, precios y descripción de cobertura.

6. **👤 Panel de Alumno y Seguimiento de Progreso:**
   * Perfil individual, historial de asistencias y registro visual con fotos de progreso.

7. **🔐 Seguridad y Roles de Usuario:**
   * Control de acceso basado en roles (`Administrador`, `Coach`, `Alumno`).
   * Validaciones estrictas en registro e inyección de datos (DNI numérico, nombres alfabéticos, contraseñas seguras).
   * Restablecimiento obligatorio de contraseña para usuarios nuevos.

---

## 📂 Estructura del Proyecto

```text
gimnasio/
├── backend/                  # Proyecto ASP.NET Core Web API (C#)
│   ├── Controllers/          # Endpoints de la API REST (Socio, Cuota, Dashboard, etc.)
│   ├── Data/                 # AppDbContext y DataSeeder
│   ├── DTOs/                 # Objetos de Transferencia de Datos
│   ├── Models/               # Entidades de Entity Framework (Socio, Cuota, Usuario, etc.)
│   └── Services/             # Lógica de Negocio e Interfaces
└── frontend/                 # Proyecto Single Page Application (Vue 3)
    ├── src/
    │   ├── components/       # Componentes reutilizables (AppLayout, Modales, etc.)
    │   ├── services/         # Cliente Axios (api.js) y llamados REST
    │   ├── utils/            # Generadores de badges y estilos dinámicos
    │   └── views/            # Vistas principales (SociosView, CuotasView, DashboardView, etc.)
    └── package.json
```

---

## 🛠️ Instalación y Puesta en Marcha

### **Requisitos Previos**
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Node.js (v18+)](https://nodejs.org/) y `npm`
* Servidor MySQL / MariaDB activo

---

### **1. Configuración del Backend**

1. Abrí una terminal e ingresá a la carpeta `backend`:
   ```bash
   cd backend
   ```

2. Ejecutá el servidor Backend en `http://localhost:5055`:
   ```bash
   dotnet run --urls=http://localhost:5055
   ```

3. *(Opcional)* Consultá la documentación interactiva de APIs en Swagger:
   👉 **http://localhost:5055/swagger**

---

### **2. Configuración del Frontend**

1. En una nueva terminal, ingresá a la carpeta `frontend`:
   ```bash
   cd frontend
   ```

2. Instalá las dependencias de Node.js:
   ```bash
   npm install
   ```

3. Iniciá el servidor de desarrollo Vite:
   ```bash
   npm run dev
   ```

4. Abrí tu navegador en:
   👉 **http://localhost:5173**

---

## 🔑 Credenciales de Prueba por Defecto

| Rol | Usuario | Contraseña |
| :--- | :--- | :--- |
| **Administrador** | `admin` | `Admin123!` |
| **Coach Principal** | `coach` | `Coach123!` |

---

## 🌐 Endpoints Principales de la API REST

* `GET /api/dashboard/stats` - Métricas generales del gimnasio.
* `GET /api/socio` - Listado paginado y filtrado de socios.
* `GET /api/cuota?periodo=YYYYMM` - Reporte y cobros de cuotas por período.
* `GET /api/usuario/coaches` - Listado de entrenadores y cupos.
* `POST /api/auth/login` - Autenticación y generación de Token JWT.

---

## 📜 Licencia

Desarrollado para la gestión eficiente y profesional de centros deportivos. Todos los derechos reservados.
