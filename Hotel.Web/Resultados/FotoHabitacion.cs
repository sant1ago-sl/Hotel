namespace Hotel.Web.Resultados;

public static class FotoHabitacion
{
    public static string Ruta(string tipo)
    {
        return tipo.ToLower() switch
        {
            "simple" => "/img/simple.jpg",
            "matrimonial" => "/img/matrimonial.jpg",
            "suite" => "/img/suite.jpg",
            _ => "/img/doble.jpg"
        };
    }
}
