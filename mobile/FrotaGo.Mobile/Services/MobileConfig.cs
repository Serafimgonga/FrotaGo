namespace FrotaGo.Mobile.Services;

public static class MobileConfig
{
    // URL do backend FrotaGo.
    //
    // Emulador Android (Windows/Linux):  "http://10.0.2.2:5283/"
    // Dispositivo físico na mesma rede:  "http://192.168.x.x:5283/"
    // Produção / ngrok:                  "https://<dominio>.ngrok-free.app/"
    //
    // Nota: em HTTP (não HTTPS), é necessário ativar ClearText no AndroidManifest.xml
    public static string BaseUrl { get; set; } = "http://10.0.2.2:5283/";
}
