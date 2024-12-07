using Exiled.API.Interfaces;

namespace SCP_SL_DestroyCardReader
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true; // Czy plugin jest włączony
        public bool Debug { get; set; } = false; // Czy tryb debugowania jest aktywny
        public int DestroyDuration { get; set; } = 5; // Czas ukrycia czytnika w sekundach
    }
}


