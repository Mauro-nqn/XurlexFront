namespace IurixBlazor.Shared.Helpers
{
    public record PermisoDef(string Codigo, string Descripcion, string Modulo);

    public static class PermisosCatalogo
    {
        public static readonly List<PermisoDef> Todos = new()
    {
        // Roles / permisos
        new("Roles.Ver",           "Ver roles",               "Seguridad"),
        new("Roles.Crear",        "Editar roles y permisos", "Seguridad"),
        new("Roles.Editar",        "Editar roles y permisos", "Seguridad"),
        new("Roles.Eliminar",        "Editar roles y permisos", "Seguridad"),


        // Usuarios
        new("Usuarios.Ver",      "Ver usuarios",          "Usuarios"),
        new("Usuarios.Crear",      "Crear usuarios",          "Usuarios"),
        new("Usuarios.Editar",     "Editar usuarios",         "Usuarios"),
        new("Usuarios.Eliminar",   "Eliminar usuarios",       "Usuarios"),

        // PERSONAS
        new("Personas.Ver",       "Ver personas",                   "Personas"),
        new("Personas.Crear",     "Crear personas",                 "Personas"),
        new("Personas.Editar",    "Editar personas",                "Personas"),
        new("Personas.Eliminar",  "Eliminar personas",              "Personas"),

         //CLASIFICACION
        new("ClasificacionPersonas.Ver",       "Ver Clasificacion personas",                   "ClasificacionPersonas"),
        new("ClasificacionPersonas.Crear",     "Crear Clasificacion personas",                 "ClasificacionPersonas"),
        new("ClasificacionPersonas.Editar",    "Editar Clasificacion personas",                "ClasificacionPersonas"),
        new("ClasificacionPersonas.Eliminar",  "Eliminar Clasificacion personas",              "ClasificacionPersonas"),

        //CARACTERES INTERVENCION
        new("CaracteresIntervencion.Ver",       "Ver Caracteres Intervencion",                   "CaracteresIntervencion"),
        new("CaracteresIntervencion.Crear",     "Crear Caracteres Intervencion",                 "CaracteresIntervencion"),
        new("CaracteresIntervencion.Editar",    "Editar Caracteres Intervencion",                "CaracteresIntervencion"),
        new("CaracteresIntervencion.Eliminar",  "Eliminar Caracteres Intervencion",              "CaracteresIntervencion"),


        //DOMICILIOS
        new("Domicilios.Ver",       "Ver Domicilios",                   "Domicilios"),
        new("Domicilios.Crear",     "Crear Domicilios",                 "Domicilios"),
        new("Domicilios.Editar",    "Editar Domicilios",                "Domicilios"),
        new("Domicilios.Eliminar",  "Eliminar Domicilios",              "Domicilios"),

        // GESTIONES
        new("Gestiones.Ver",      "Ver gestiones",                  "Gestiones"),
        new("Gestiones.Crear",    "Crear gestiones",                "Gestiones"),
        new("Gestiones.Editar",   "Editar gestiones",               "Gestiones"),
        new("Gestiones.Eliminar", "Eliminar gestiones",             "Gestiones"),

        
                // CUENTAS CORRIENTES
        new("CuentasCorrientes.Ver",      "Ver Cuentas Corrientes",                  "Cuentas Corrientes"),
        new("CuentasCorrientes.Crear",    "Crear Cuentas Corrientes",                "Cuentas Corrientes"),
        new("CuentasCorrientes.Editar",   "Editar Cuentas Corrientes",               "Cuentas Corrientes"),
        new("CuentasCorrientes.Eliminar", "Eliminar Cuentas Corrientes",             "Cuentas Corrientes"),

                        // RECIBOS  
        new("Recibos.Ver",      "Ver Recibos",                  "Recibos"),
        new("Recibos.Crear",    "Crear Recibos",                "Recibos"),
        new("Recibos.Editar",   "Editar Recibos",               "Recibos"),
        new("Recibos.Eliminar", "Eliminar Recibos",             "Recibos"),
        new("Recibos.Anular", "Anular Recibos",             "Recibos"),
        new("Recibos.Reversa", "Elimina Reversa Recibos",             "Recibos"),


                                // Facturas 
        new("Facturas.Ver",      "Ver Facturas",                  "Facturas"),
        new("Facturas.Crear",    "Crear Facturas",                "Facturas"),
        new("Facturas.Editar",   "Editar Facturas",               "Facturas"),
        new("Facturas.Eliminar", "Eliminar Facturas",             "Facturas"),
        new("Facturas.Anular", "Anular Facturas",             "Facturas"),



        // etc...
    };
    }

}
