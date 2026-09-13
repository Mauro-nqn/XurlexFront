
using System.Runtime.Serialization;
using IurixBlazor.Shared.Enums;


    //public enum AplicacionDextra
    //{
    //    [EnumMember(Value = "TSJ - SECRETARIA CIVIL")] TSJ_SECRETARIA_CIVIL,
    //    [EnumMember(Value = "TSJ - SECRETARIA DE DEMANDAS ORIGINARIAS")] TSJ_SECRETARIA_DE_DEMANDAS_ORIGINARIAS,
    //    [EnumMember(Value = "CAMARA NEUQUEN")] CAMARA_NEUQUEN,
    //    [EnumMember(Value = "JUZGADO ELECTORAL")] JUZGADO_ELECTORAL,
    //    [EnumMember(Value = "JUZGADOS EJECUTIVOS")] JUZGADOS_EJECUTIVOS,
    //    [EnumMember(Value = "OFICINA JUDICIAL PROCESAL ADMINISTRATIVO")] OFICINA_JUDICIAL_PROCESAL_ADMINISTRATIVO,
    //    [EnumMember(Value = "OFICINA JUDICIAL LABORAL")] OFICINA_JUDICIAL_LABORAL,
    //    [EnumMember(Value = "OFICINA JUDICIAL FAMILIA")] OFICINA_JUDICIAL_FAMILIA,
    //    [EnumMember(Value = "OFICINA JUDICIAL CIVIL")] OFICINA_JUDICIAL_CIVIL

    //}


    public enum AplicacionDextra
    {
    // NEUQUÉN

    [Circunscripcion("NEUQUEN")]
    [EnumMember(Value = "TSJ - SECRETARIA CIVIL")]
    TSJ_SECRETARIA_CIVIL = 0,


    [Circunscripcion("NEUQUEN")]
    [EnumMember(Value = "TSJ - SECRETARIA DE DEMANDAS ORIGINARIAS")]
    TSJ_SECRETARIA_DE_DEMANDAS_ORIGINARIAS = 1,

    [Circunscripcion("NEUQUEN")]
    [EnumMember(Value = "CAMARA NEUQUEN")]
    CAMARA_NEUQUEN = 2,

    [Circunscripcion("NEUQUEN")]
    [EnumMember(Value = "JUZGADO ELECTORAL")]
    JUZGADO_ELECTORAL = 3,

    [Circunscripcion("NEUQUEN")]
    [EnumMember(Value = "JUZGADOS EJECUTIVOS")]
    JUZGADOS_EJECUTIVOS = 4,

    [Circunscripcion("NEUQUEN")]
    [EnumMember(Value = "OFICINA JUDICIAL PROCESAL ADMINISTRATIVO")]
    OFICINA_JUDICIAL_PROCESAL_ADMINISTRATIVO = 5,

    [Circunscripcion("NEUQUEN")]
    [EnumMember(Value = "OFICINA JUDICIAL LABORAL")]
    OFICINA_JUDICIAL_LABORAL = 6,

    [Circunscripcion("NEUQUEN")]
    [EnumMember(Value = "OFICINA JUDICIAL FAMILIA")]
    OFICINA_JUDICIAL_FAMILIA = 7,

    [Circunscripcion("NEUQUEN")]
    [EnumMember(Value = "OFICINA JUDICIAL CIVIL")]
    OFICINA_JUDICIAL_CIVIL = 8,



    // ZAPALA
    [Circunscripcion("ZAPALA")]
    [EnumMember(Value = "CAMARAS SEDE ZAPALA")]
    CAMARAS_SEDE_ZAPALA = 9,

    [Circunscripcion("ZAPALA")]
    [EnumMember(Value = "JUZGADOS ZAPALA")]
    JUZGADOS_ZAPALA = 10,

    [Circunscripcion("ZAPALA")]
    [EnumMember(Value = "OFICINA JUDICIAL PROCESAL ADMINISTRATIVO")]
    OFICINA_JUDICIAL_PROCESAL_ADMIN = 11,


    // VILLA LA ANGOSTURA
    [Circunscripcion("VILLA LA ANGOSTURA")]
    [EnumMember(Value = "JUZGADOS VILLA LA ANGOSTURA")]
    JUZGADOS_VILLA_LA_ANGOSTURA = 12,

    // CUTRAL CO
    [Circunscripcion("CUTRAL CO")]
    [EnumMember(Value = "CAMARA SEDE CUTRAL CO")]
    CAMARA_SEDE_CUTRAL_CO = 13,

    [Circunscripcion("CUTRAL CO")]
    [EnumMember(Value = "JUZGADOS CUTRAL CO")]
    JUZGADOS_CUTRAL_CO = 14,

    // SAN MARTIN DE LOS ANDES
    [Circunscripcion("SAN MARTIN DE LOS ANDES")]
    [EnumMember(Value = "CAMARA SEDE SAN MARTIN DE LOS ANDES")]
    CAMARA_SEDE_SAN_MARTIN_DE_LOS_ANDES = 15,

    [Circunscripcion("SAN MARTIN DE LOS ANDES")]
    [EnumMember(Value = "JUZGADO FAMILIA")]
    JUZGADO_FAMILIA = 16,

    //JUNIN DE LOS ANDES
    [Circunscripcion("JUNIN DE LOS ANDES")]
    [EnumMember(Value = "JUZGADOS JUNIN DE LOS ANDES")]
    JUZGADOS_JUNIN_DE_LOS_ANDES = 17,


    //RINCON DE LOS SAUCES
    [Circunscripcion("RINCON DE LOS SAUCES")]
    [EnumMember(Value = "JUZGADOS RINCON DE LOS SAUCES")]
    JUZGADOS_RINCON_DE_LOS_SAUCES = 18,


    //CHOS MALAL
    [Circunscripcion("CHOS MALAL")]
    [EnumMember(Value = "JUZGADOS CHOS MALAL")]
    JUZGADOS_CHOS_MALAL = 19,
}


