```
   _____      ____  ______           __     ____  ____  _____ 
  / ___/___  / / / / ____/___ ______/ /_   / __ \/ __ \/ ___/ 
  \__ \/ _ \/ / / / /_  / __ `/ ___/ __/  / /_/ / / / /\__ \  
 ___/ /  __/ / / / __/ / /_/ (__  ) /_   / ____/ /_/ /___/ /  
/____/\___/_/_/ /_/    \__,_/____/\__/  /_/    \____//____/   

        SISTEMA DE PUNTO DE VENTA INTELIGENTE v2.0
```

[![Framework](https://img.shields.io/badge/.NET-8.0--windows-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![UI](https://img.shields.io/badge/WPF-Windows_Desktop-0078D4?style=for-the-badge&logo=windows)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![Database](https://img.shields.io/badge/SQLite-Entity_Framework_Core-003B57?style=for-the-badge&logo=sqlite)](https://www.sqlite.org/)
[![Excel](https://img.shields.io/badge/ClosedXML-Excel_Import%2FExport-217346?style=for-the-badge&logo=microsoft-excel)](https://github.com/ClosedXML/ClosedXML)
[![PDF](https://img.shields.io/badge/QuestPDF-Legal_Receipts-FF4081?style=for-the-badge&logo=adobe-acrobat-reader)](https://www.questpdf.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Download Release](https://img.shields.io/badge/Descargar_Ejecutable-Windows_v2.0-0078D4?style=for-the-badge&logo=windows)](https://github.com/ydmmejia/SellFast-POS/releases)

---

## Descripcion del Proyecto

**SellFast POS v2.0** es una plataforma moderna, agil e inteligente de **Punto de Venta (POS) y Gestion Comercial**, disenada bajo patrones limpios de arquitectura **MVVM (Model-View-ViewModel)** en **.NET 8.0 (WPF)** y **SQLite**. 

Desarrollada bajo la estetica **Prodex SaaS Clean**, ofrece un entorno minimalista con pastillas neon, blanco puro y controles ultra-fluidos sin dependencias pesadas. Es ideal para cafeterias, restaurantes, minimarkets y negocios de comercio al detal.

---

## Caracteristicas Principales

### 1. Asistente de Onboarding Multipaso para Administradores
- **Paso 1: Identificacion Comercial**: Configuracion de Tipo de Persona (*Juridica / Natural*), NIT/RFC, Razon Social, Eslogan, Telefono y Carga de Logo.
- **Paso 2: Localizacion & Moneda**: Seleccion de Pais (*Colombia, Mexico, Peru, Chile, EE. UU., Espana, etc.*), Simbolo de Moneda e Impuestos (IVA / Tax).
- **Paso 3: Modulos Activos**: Interruptores dinamicos para Mesas/Salon, Comandas de Cocina, Fichos de Almuerzo y Propina Sugerida.
- **Paso 4: Administrador Personalizado**: Creacion directa del usuario administrador con nombre y contrasena propia.
- **Paso 5: Importacion de Base de Datos**: Carga masiva desde archivos Excel `.xlsx` de productos y clientes.

### 2. Formatos Fiscales Legales de Comprobante (Colombia & Mexico)
- **Colombia (DIAN — Documento Equivalente POS)**:
  - Header fiscal legal `FACTURA EQUIVALENTE POS`.
  - NIT emisor con digito de verificacion, IVA (19%/5%), INC (8%) y consecutivo `POS-XXXXXX`.
  - Firma digital fiscal y Codigo QR de verificacion.
- **Mexico (SAT — Ticket Simplificado / CFDI Publico en General)**:
  - Header fiscal legal `TICKET SIMPLIFICADO SAT`.
  - RFC emisor, Regimen Fiscal, claves de metodos de pago SAT y Codigo QR.

### 3. Notificaciones & Recordatorios por WhatsApp (`wa.me`)
- Envio directo de comprobantes PDF al cliente a su WhatsApp en 1 clic.
- Recordatorios de saldos pendientes y mensajes personalizados de fidelizacion.

### 4. Sincronizacion Red Local Multi-Terminal (LAN / Wi-Fi)
- **Modo Standalone**: Operacion autonoma en 1 sola computadora.
- **Modo Servidor Principal (Caja Maestra)**: Comparte la base de datos central en tiempo real a la red local.
- **Modo Cliente Secundario**: Conecta laptops, pantallas de cocina y comanderos a la IP del Servidor.
- **Configuracion 100% Visual**: Ajuste de ruta de red compartida (`\\SERVIDOR\Compartido\SellFast.db`) directamente desde la interfaz visual sin tocar codigo.

### 5. Deteccion Automatica de Hardware POS
- Deteccion de impresoras termicas de tickets instaladas en Windows (`POS-80`, `Epson TM-T20`, `Xprinter`).
- Integracion nativa para lectores de codigos de barras USB HID.

### 6. Bitacora & Registro de Auditoria (`AuditLog`)
- Rastreo en tiempo real de cada venta, anulacion, apertura de caja, cambios de configuracion e inicios de sesion con fecha, hora exacta y usuario.

### 7. Copias de Seguridad & Excel
- Respaldos fechados en 1-Clic de la base de datos local SQLite (`.db`).
- Generacion automatica de plantillas Excel e importacion masiva.

---

## Requisitos de Instalacion

- **Sistema Operativo**: Windows 10 / Windows 11 (64-bit).
- **SDK**: [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) o posterior.
- **IDE Recomendado**: Visual Studio 2022 / VS Code / Antigravity IDE.

---

## Guia de Inicio Rapido

### 1. Clonar el Repositorio
```bash
git clone https://github.com/ydmmejia/SellFast-PO.git
cd SellFast-PO
```

### 2. Compilar la Solucion
```bash
dotnet build
```

### 3. Ejecutar la Aplicacion
```bash
dotnet run --project SellFast.App
```

---

## Configuracion Multi-Terminal (Varias Computadoras Conectadas)

SellFast POS permite operar en entornos de una o varias computadoras dentro de la misma red local (LAN/Wi-Fi).

### Opcion 1: Monopuesto / Standalone (1 Computadora)
- La aplicacion opera de forma autonoma almacenando los datos en la base de datos local `SellFast.db`.

### Opcion 2: Red Local LAN con Carpeta Compartida (2 a 4 Computadoras)
Para conectar multiples cajas o terminales directamente desde la interfaz visual (sin modificar codigo):
1. **En la Computadora Principal (Servidor / Caja Maestra)**:
   - Comparte la carpeta que contiene `SellFast.db` en la red local de Windows con permisos de lectura y escritura (ejemplo: `\\192.168.1.100\SellFastData`).
2. **En las Terminales Clientes (Cajas secundarias / Comanderos)**:
   - Abre la aplicacion y entra al menu **Configuracion** -> **Sincronizacion Red Local (Multi-Terminal LAN)**.
   - En el campo **Ruta de Base de Datos**, escribe o examina con el boton la ruta de red compartida:
     `\\192.168.1.100\SellFastData\SellFast.db`
   - Presiona **Guardar Cambios de Configuracion**. La aplicacion guardara el ajuste y se conectara a la base de datos de la caja maestra.
3. **Verificacion de Red**:
   - Utiliza la herramienta integrada de **Probar Conexion** para validar la comunicacion entre la terminal y la IP del servidor.

### Opcion 3: Base de Datos Centralizada PostgreSQL / SQL Server (Produccion Concurrente)
Para negocios con alto flujo simultaneo de ventas y multiples comanderos actuando al mismo tiempo:
- Reemplaza el proveedor de base de datos en `App.xaml.cs` por `UseNpgsql` (PostgreSQL) o `UseSqlServer`.
- Todas las terminales leeran y escribiran en tiempo real contra el servidor de base de datos central.

---

## Contribuciones (Open Source)

Las contribuciones son bienvenidas. Si deseas agregar nuevos formatos fiscales o caracteristicas:
1. Haz un Fork del proyecto.
2. Crea tu rama de caracteristicas (`git checkout -b feature/NuevaFuncion`).
3. Envia tus cambios (`git commit -m 'Anade nueva funcion'`).
4. Haz Push a la rama (`git push origin feature/NuevaFuncion`).
5. Abre un **Pull Request**.

---

## Licencia

Este proyecto esta distribuido bajo la licencia **MIT**. Consulta el archivo `LICENSE` para mas informacion.
