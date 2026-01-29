# ☕ Sistema de Gestión para Cafetería UNAL

Aplicación de escritorio desarrollada en .NET y Windows Forms para la administración completa de un punto de venta de cafetería, incluyendo inventario, usuarios, ventas, control de fichos y un dashboard de reportes.

*(Aquí sería un lugar ideal para que añadas una o dos capturas de pantalla de la nueva interfaz)*

## ✨ Características Principales

*   **Gestión de Productos:** Completo CRUD (Crear, Leer, Actualizar, Eliminar) para el inventario.
*   **Gestión de Usuarios:** Administración de clientes y de usuarios del sistema con diferentes roles.
*   **Punto de Venta (PDV):** Interfaz para realizar nuevas ventas de manera eficiente.
*   **Control de Fichos:** Sistema para generar y gestionar "fichos" de almuerzo diarios.
*   **Historial de Ventas:** Visualización y filtrado del historial de transacciones, con opción de ver detalles y anular ventas.
*   **Dashboard de Reportes:** Un potente módulo con gráficos para analizar:
    *   Ventas por período.
    *   Productos más vendidos.
    *   Estadísticas por tipo de usuario.
    *   Productos con bajo stock.

## 🚀 Tecnologías Utilizadas

*   **Lenguaje:** C# 12
*   **Framework:** .NET 8
*   **UI:** Windows Forms, con la librería `MaterialSkin` para un diseño moderno Material Design.
*   **Base de Datos:** SQLite
*   **ORM:** Entity Framework Core
*   **Gráficos:** `System.Windows.Forms.DataVisualization`
*   **Exportación a Excel:** `ClosedXML`

## 🏁 Cómo Empezar

1.  **Clonar el repositorio:**
    ```bash
    git clone https://github.com/tu-usuario/CafeteriaSistema.git
    ```
2.  **Navegar al directorio:**
    ```bash
    cd CafeteriaSistema
    ```
3.  **Ejecutar la aplicación:**
    ```bash
    dotnet run
    ```
    La aplicación se compilará y se iniciará. La base de datos de SQLite se creará si no existe en la primera ejecución.
